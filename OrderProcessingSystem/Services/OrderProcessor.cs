using System.Diagnostics;
using OrderProcessingSystem.Exceptions;
using OrderProcessingSystem.Models;
using OrderProcessingSystem.Repositories;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Storage;

namespace OrderProcessingSystem.Services;

public class OrderProcessor
{
    private readonly ProductRepository _productRepository;
    private readonly OrderPricer _orderPricer;
    private readonly JsonFileStore _store;
    private readonly OrderRepository _orderRepository;
    private readonly InvoiceService _invoiceService;
    private readonly CustomerService _customerService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IEmailService _emailService;

    public OrderProcessor(
        ProductRepository productRepository,
        OrderPricer orderPricer,
        JsonFileStore store,
        InvoiceService invoiceService,
        CustomerService customerService,
        IPaymentGateway paymentGateway,
        IEmailService emailService,
        OrderRepository orderRepository)
    {
        _productRepository = productRepository;
        _orderPricer = orderPricer;
        _store = store;
        _invoiceService = invoiceService;
        _customerService = customerService;
        _paymentGateway = paymentGateway;
        _emailService = emailService;
       _orderRepository = orderRepository;
    }

    public async Task<bool> ProcessOrderAsync(
        Order order,
        CancellationToken ct = default)
    {
        var timer = Stopwatch.StartNew();
        var reservedItems = new List<OrderItem>();

        Customer? customer = null;
        string? transactionId = null;

        try
        {
            Log(order, "Step 1/6 Customer", "START", timer);

            customer = await _customerService.GetCustomerAsync(order.CustomerId);

            if (!customer.IsActive)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason = "Customer is inactive";

                await SaveOrderAsync(order);
                return false;
            }

            ct.ThrowIfCancellationRequested();

            Log(order, "Step 2/6 Reserve stock", "START", timer);

            foreach (OrderItem item in order.Items)
            {
                bool reserved = await _productRepository.ReserveStockAsync(
                    item.ProductId,
                    item.Quantity,
                    ct);

                if (!reserved)
                {
                    order.Status = OrderStatus.Rejected;
                    order.FailureReason =
                        $"Insufficient stock for Product #{item.ProductId}.";

                    await ReleaseReservedStockAsync(reservedItems);
                    await SaveOrderAsync(order);

                    return false;
                }

                reservedItems.Add(item);
            }

            ct.ThrowIfCancellationRequested();

            Log(order, "Step 3/6 Pricing", "START", timer);

            await _orderPricer.PriceOrderAsync(order, ct);

            ct.ThrowIfCancellationRequested();

            Log(order, "Step 4/6 Payment", "START", timer);

            using var paymentCts =
                CancellationTokenSource.CreateLinkedTokenSource(ct);

            paymentCts.CancelAfter(TimeSpan.FromSeconds(3));

            PaymentResult payment;

            try
            {
                payment = await _paymentGateway.ProcessPaymentAsync(
                    order.Id,
                    order.TotalAmount,
                    paymentCts.Token);
            }
            catch (OperationCanceledException)
                when (!ct.IsCancellationRequested)
            {
                await ReleaseReservedStockAsync(reservedItems);

                order.Status = OrderStatus.PaymentFailed;
                order.FailureReason = "Payment gateway timeout (3s)";

                await SaveOrderAsync(order);
                return false;
            }

            if (!payment.Success)
            {
                await ReleaseReservedStockAsync(reservedItems);

                order.Status = OrderStatus.PaymentFailed;
                order.FailureReason =
                    payment.Message ?? "Card declined by issuer";

                await SaveOrderAsync(order);
                return false;
            }

            transactionId = payment.TransactionId;
            order.TransactionId = transactionId;

            Log(order, "Step 5/6 Save order", "START", timer);

            order.Status = OrderStatus.Completed;
            order.FailureReason = null;

            try
            {
                await SaveOrderAsync(order);
            }
            catch (DatabaseWriteException ex)
            {
                await _paymentGateway.RefundAsync(transactionId);
                await ReleaseReservedStockAsync(reservedItems);

                order.Status = OrderStatus.Failed;
                order.FailureReason =
                    $"Database save failed after payment; payment {transactionId} refunded";

                await SaveOrderAsync(order);

                Console.WriteLine($"Order #{order.Id} failed: {ex.Message}");
                return false;
            }

            await _invoiceService.GenerateInvoiceAsync(order, ct);

            Log(order, "Step 6/6 Email", "START", timer);

            await SendConfirmationWithRetryAsync(order, customer);

            Log(order, "Step 6/6 Email", "FINISHED", timer);

            return order.Status == OrderStatus.Completed;
        }
        catch (CustomerNotFoundException ex)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason = ex.Message;

            await SaveOrderAsync(order);
            return false;
        }
        catch (InvalidCouponException ex)
        {
            await ReleaseReservedStockAsync(reservedItems);

            order.Status = OrderStatus.Rejected;
            order.FailureReason = ex.Message;

            await SaveOrderAsync(order);
            return false;
        }
        catch (OperationCanceledException)
        {
            await ReleaseReservedStockAsync(reservedItems);

            order.FailureReason = "Order processing cancelled";
            order.Status = OrderStatus.Cancelled;

            await SaveOrderAsync(order);

            throw;
        }
        catch (Exception ex)
        {
            await ReleaseReservedStockAsync(reservedItems);

            order.Status = OrderStatus.Failed;
            order.FailureReason = ex.Message;

            await SaveOrderAsync(order);
            return false;
        }
    }

    private async Task SendConfirmationWithRetryAsync(
        Order order,
        Customer customer)
    {
        int[] delays = [500, 1000, 2000];
        string lastError = "";

        for (int attempt = 1; attempt <= 4; attempt++)
        {
            try
            {
                await _emailService.SendConfirmationAsync(order, customer);
                return;
            }
            catch (SmtpUnavailableException ex)
            {
                lastError = ex.Message;

                Console.WriteLine(
                    $"Email for Order #{order.Id}: attempt {attempt}/4 failed.");

                if (attempt < 4)
                {
                    await Task.Delay(delays[attempt - 1]);
                }
            }
        }

        // Queue failed email for later retry
        if (_emailService is EmailService concreteEmailService)
        {
            await concreteEmailService.QueueForRetryAsync(
                order.Id,
                4,
                lastError);
        }

        Console.WriteLine(
            $"Email for Order #{order.Id} queued for retry later");
    }

    private async Task ReleaseReservedStockAsync(
        List<OrderItem> reservedItems)
    {
        foreach (OrderItem item in reservedItems)
        {
            await _productRepository.ReleaseStockAsync(
                item.ProductId,
                item.Quantity,
                CancellationToken.None);
        }

        reservedItems.Clear();
    }

    private Task SaveOrderAsync(Order order)
    {
        return _orderRepository.SaveAsync(order);
    }

    private static void Log(
        Order order,
        string step,
        string result,
        Stopwatch timer)
    {
        Console.WriteLine(
            $"[{DateTime.Now:HH:mm:ss.fff}] " +
            $"[Thread {Environment.CurrentManagedThreadId}] " +
            $"Order #{order.Id} -> {step}: {result} " +
            $"in {timer.ElapsedMilliseconds} ms");
    }
}
