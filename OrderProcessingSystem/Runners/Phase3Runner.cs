using System;
using OrderProcessingSystem.Models;
using OrderProcessingSystem.Repositories;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Storage;

namespace OrderProcessingSystem.Runners;

public class Phase3Runner
{
    private readonly JsonFileStore _store;
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly CouponRepository _couponRepository;

    public Phase3Runner(
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
        Console.WriteLine("\n========== PHASE 3 ==========");

        var pricingService = new PricingService(
            _productRepository,
            _customerRepository,
            _couponRepository,
            Array.Empty<int>());

        var orderPricer = new OrderPricer(pricingService);
        var invoiceService = new InvoiceService();
        var customerService = new CustomerService(_customerRepository);

        var paymentGateway = new PaymentGateway(
            _store,
            timeoutOrderIds: new HashSet<int> { 5 },
            declineOrderIds: new HashSet<int> { 8 });

        var emailService = new EmailService(
            _store,
            failAttempts: new Dictionary<int, int>
            {
                [3] = 2, // order #3 fails twice
                [17] = int.MaxValue  // order #17 always fails 
            });

        var orderProcessor = new OrderProcessor(
            _productRepository,
            orderPricer,
            _store,
            invoiceService,
            customerService,
            paymentGateway,
            emailService,
            _orderRepository);

        List<Order> orders = await _orderRepository.GetByStatusAsync(
            OrderStatus.Validated,
            ct);

        Console.WriteLine(
            $"\nProcessing {orders.Count} validated orders...");

        List<Task<bool>> processingTasks = orders
            .Select(order => orderProcessor.ProcessOrderAsync(order, ct))
            .ToList();

        bool[] results = await Task.WhenAll(processingTasks);

        Console.WriteLine("\n========== PHASE 3 RESULTS ==========");
        Console.WriteLine($"Processed: {results.Length}");
        Console.WriteLine($"Completed: {results.Count(result => result)}");
        Console.WriteLine($"Not completed: {results.Count(result => !result)}");

        List<Order> allOrders = await _orderRepository.GetAllAsync(ct);

        foreach (Order order in allOrders.OrderBy(order => order.Id))
        {
            Console.WriteLine(
                $"Order #{order.Id}: {order.Status} | {order.FailureReason}");
        }
    }
}

