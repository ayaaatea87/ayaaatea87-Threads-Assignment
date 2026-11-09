using System;
using System.Diagnostics;
using OrderProcessingSystem.Models;
using OrderProcessingSystem.Repositories;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Storage;

namespace OrderProcessingSystem.Runners;

public class Phase2Runner
{
    private readonly JsonFileStore _store;
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly CouponRepository _couponRepository;

    public Phase2Runner(
        JsonFileStore store,
        OrderRepository orderRepository,
        ProductRepository productRepository,
        CustomerRepository customerRepository,
        CouponRepository couponRepository)
    {
        _store = store;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _couponRepository = couponRepository;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        Console.WriteLine("\n========== PHASE 2 ==========");

        var pricingService = new PricingService(
            _productRepository,
            _customerRepository,
            _couponRepository,
            new[] { 7 });

        var orderPricer = new OrderPricer(pricingService);
        var invoiceService = new InvoiceService();

        var customerService = new CustomerService(_customerRepository);

        var paymentGateway = new PaymentGateway(_store);

        var emailService = new EmailService(_store);
        var orderProcessor = new OrderProcessor(
            _productRepository,
            orderPricer,
            _store,
            invoiceService,
            customerService,
            paymentGateway,
            emailService,
            _orderRepository);

        var reportService = new ReportService(_store);

        // 1- Compare parallel and sequential pricing.
        List<Order> validatedOrders =
            await _orderRepository.GetByStatusAsync(
                OrderStatus.Validated, ct);

        Order? firstOrder = validatedOrders
            .FirstOrDefault(o => o.Id == 1);

        if (firstOrder is not null)
        {
            // cloneOrder to temporary object to keep original data safe
            Order parallelOrder = CloneOrder(firstOrder);
            Order sequentialOrder = CloneOrder(firstOrder);

            var stopwatch = Stopwatch.StartNew();

            await orderPricer.PriceOrderAsync(parallelOrder, ct);

            stopwatch.Stop();
            Console.WriteLine(
                $"Parallel pricing: {stopwatch.ElapsedMilliseconds} ms");

            stopwatch.Restart();

            await orderPricer.PriceOrderSequentialAsync(
                sequentialOrder, ct);

            stopwatch.Stop();
            Console.WriteLine(
                $"Sequential pricing: {stopwatch.ElapsedMilliseconds} ms");

            Console.WriteLine(
                $"Order #1 total: {parallelOrder.TotalAmount:F2}");
        }

        // 2- Process validated orders concurrently
        validatedOrders = await _orderRepository.GetByStatusAsync(
            OrderStatus.Validated, ct);

        Console.WriteLine(
            $"\nProcessing {validatedOrders.Count} validated orders...");

        await Task.WhenAll(
            validatedOrders.Select(order =>
                orderProcessor.ProcessOrderAsync(order, ct)));

        // 3- Generate the sales report after processing finish
        await reportService.GenerateSalesReportAsync(ct);

        // 4- Print final status count
        List<Order> allOrders =
            await _orderRepository.GetAllAsync(ct);

        Console.WriteLine("\n========== FINAL RESULTS ==========");

        Console.WriteLine(
            $"Completed: {allOrders.Count(o => o.Status == OrderStatus.Completed)}");

        Console.WriteLine(
            $"Rejected: {allOrders.Count(o => o.Status == OrderStatus.Rejected)}");

        Console.WriteLine(
            $"Failed: {allOrders.Count(o => o.Status == OrderStatus.Failed)}");

        foreach (Order order in allOrders
            .Where(o => o.Status == OrderStatus.Failed))
        {
            Console.WriteLine(
                $"Order #{order.Id}: {order.FailureReason}");
        }

        Console.WriteLine("\nSales report generated.");
    }

    private static Order CloneOrder(Order source)
    {
        return new Order
        {
            Id = source.Id,
            CustomerId = source.CustomerId,
            Items = source.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList(),
            CouponCode = source.CouponCode,
            SubTotal = source.SubTotal,
            Discount = source.Discount,
            ShippingCost = source.ShippingCost,
            TotalAmount = source.TotalAmount,
            Status = source.Status,
            CreatedAt = source.CreatedAt,
            FailureReason = source.FailureReason,
            TransactionId = source.TransactionId
        };
    }
}

