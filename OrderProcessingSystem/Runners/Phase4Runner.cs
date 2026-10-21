
using System.Diagnostics;
using OrderProcessingSystem.Models;
using OrderProcessingSystem.Repositories;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Storage;

namespace OrderProcessingSystem.Runners;

public class Phase4Runner
{
    private readonly JsonFileStore _store;
    private readonly OrderRepository _orders;
    private readonly CustomerRepository _customers;
    private readonly ProductRepository _products;
    private readonly CouponRepository _coupons;

    public Phase4Runner(
        JsonFileStore store,
        OrderRepository orders,
        ProductRepository products,
        CustomerRepository customers,
        CouponRepository coupons)
    {
        _store = store;
        _orders = orders;
        _customers = customers;
        _products = products;
        _coupons = coupons;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        Console.WriteLine("""
        
        ===== PHASE 4 =====
        1. Live Checkout
        2. Customer Dashboard
        3. Shipping Race
        4. Fraud Scoring
        5. Concurrent Emails
        0. Exit
        """);

        Console.Write("Choice: ");

        switch (Console.ReadLine())
        {

            case "1":
                await DataSeeder.ResetAsync(ct);

                var phase1Runner = new Phase1Runner(
                    _store,
                    new SyncOrderValidator(
                        new SyncCustomerRepository(_store),
                        new SyncProductRepository(_store),
                        new SyncCouponRepository(_store)),
                    new OrderValidator(
                        _customers,
                        _products,
                        _coupons,
                        _orders),
                    new IncomingOrderRepository(_store),
                    _orders);

                await phase1Runner.RunAsync(ct);

                await new Phase3Runner(
                    _store,
                    _orders,
                    _products,
                    _customers,
                    _coupons).RunAsync(ct);

                break;


            case "2":
                await DashboardAsync(3, ct);
                await DashboardAsync(99, ct);
                break;


            case "3":
                await DataSeeder.ResetAsync(ct);

                var shippingPhase1Runner = new Phase1Runner(
                    _store,
                    new SyncOrderValidator(
                        new SyncCustomerRepository(_store),
                        new SyncProductRepository(_store),
                        new SyncCouponRepository(_store)),
                    new OrderValidator(
                        _customers,
                        _products,
                        _coupons,
                        _orders),
                    new IncomingOrderRepository(_store),
                    _orders);

                await shippingPhase1Runner.RunAsync(ct);

                await ShippingRaceAsync(ct);
                break;


            case "4":
                RunFraudScoring(ct);
                break;

            case "5":
                await ConcurrentEmailsAsync(ct);
                break;

            case "0":
                return;

            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
    }


    private async Task ShippingRaceAsync(CancellationToken ct)
    {
        Console.Write("Order ID: ");

        if (!int.TryParse(Console.ReadLine(), out int orderId))
        {
            Console.WriteLine("Invalid Order ID.");
            return;
        }

        var allOrders = await _orders.GetAllAsync(ct);
        var order = allOrders.FirstOrDefault(o => o.Id == orderId);

        if (order is null)
        {
            Console.WriteLine($"Order #{orderId} not found.");
            return;
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        var carriers = new[]
        {
        GetQuoteAsync("Carrier A", 700, 1200, cts.Token),
        GetQuoteAsync("Carrier B", 400, 1800, cts.Token),
        GetQuoteAsync("Carrier C", 900, 800, cts.Token)
    };

        try
        {
            while (carriers.Length > 0)
            {
                var finished = await Task.WhenAny(carriers);
                var remaining = carriers.Where(t => t != finished).ToArray();

                if (finished.IsCompletedSuccessfully)
                {
                    var quote = await finished;

                    cts.Cancel();

                    Console.WriteLine(
                        $"Winner: {quote.Carrier}, Cost: {quote.Cost}, " +
                        $"ETA: {quote.EstimatedMinutes} minutes");

                    return;
                }

                await finished;
                carriers = remaining;
            }

            Console.WriteLine("All carriers failed.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Shipping race timed out or was cancelled.");
        }
    }

    private static async Task<ShippingQuote> GetQuoteAsync(
        string carrier,
        int cost,
        int estimatedMinutes,
        CancellationToken ct)
    {
        int delay = carrier switch
        {
            "Carrier A" => 700,
            "Carrier B" => 400,
            _ => 900
        };

        await Task.Delay(delay, ct);

        return new ShippingQuote
        {
            Carrier = carrier,
            Cost = cost,
            EstimatedMinutes = estimatedMinutes
        };
    }

    private sealed class ShippingQuote
    {
        public string Carrier { get; init; } = "";
        public int Cost { get; init; }
        public int EstimatedMinutes { get; init; }
    }


    private async Task DashboardAsync(int customerId, CancellationToken ct)
    {
        Console.WriteLine($"\nDashboard for Customer #{customerId}");

        var timer = Stopwatch.StartNew();

        Task<Customer?> customerTask = LoadCustomerAsync(customerId, ct);
        Task<List<Order>> ordersTask = LoadOrdersAsync(customerId, ct);
        Task<List<PaymentRecord>> paymentsTask = LoadPaymentsAsync(customerId, ct);
        Task<List<EmailRecord>> emailsTask = LoadEmailsAsync(customerId, ct);

        var tasks = new Task[]
        {
            customerTask, ordersTask, paymentsTask, emailsTask
        };

        try
        {
            await Task.WhenAll(tasks);
        }
        catch
        {
            foreach (var task in tasks)
            {
                if (task.Exception != null)
                {
                    foreach (var error in task.Exception.Flatten().InnerExceptions)
                        Console.WriteLine($"Load error: {error.Message}");
                }
            }
        }

        if (customerTask.IsCompletedSuccessfully)
            Console.WriteLine($"Customer: {customerTask.Result?.Name ?? "Not found"}");

        if (ordersTask.IsCompletedSuccessfully)
            Console.WriteLine($"Orders: {ordersTask.Result.Count}");

        if (paymentsTask.IsCompletedSuccessfully)
            Console.WriteLine($"Payments: {paymentsTask.Result.Count}");

        if (emailsTask.IsCompletedSuccessfully)
            Console.WriteLine($"Emails: {emailsTask.Result.Count}");

        timer.Stop();
        Console.WriteLine($"Dashboard load: {timer.ElapsedMilliseconds} ms");
    }

    private async Task<Customer?> LoadCustomerAsync(int id, CancellationToken ct)
    {
        await Task.Delay(500, ct);
        return await _customers.GetByIdAsync(id, ct);
    }

    private async Task<List<Order>> LoadOrdersAsync(int id, CancellationToken ct)
    {
        await Task.Delay(500, ct);
        return (await _orders.GetAllAsync(ct))
            .Where(o => o.CustomerId == id).ToList();
    }

    private async Task<List<PaymentRecord>> LoadPaymentsAsync(int id, CancellationToken ct)
    {
        await Task.Delay(500, ct);

        if (id == 99)
            throw new TimeoutException("Payments store timeout");

        var payments = await _store.ReadAsync<PaymentRecord>("payments.json", ct);
        var orderIds = (await _orders.GetAllAsync(ct))
            .Where(o => o.CustomerId == id)
            .Select(o => o.Id)
            .ToHashSet();

        return payments.Where(p => orderIds.Contains(p.OrderId)).ToList();
    }

    private async Task<List<EmailRecord>> LoadEmailsAsync(int id, CancellationToken ct)
    {
        await Task.Delay(500, ct);

        var emails = await _store.ReadAsync<EmailRecord>("emails.json", ct);
        var orderIds = (await _orders.GetAllAsync(ct))
            .Where(o => o.CustomerId == id)
            .Select(o => o.Id)
            .ToHashSet();

        return emails.Where(e => orderIds.Contains(e.OrderId)).ToList();
    }
    private static void RunFraudScoring(CancellationToken ct)
    {
        var sequential = FraudScoringService.RunSequential(ct);
        var parallel = FraudScoringService.RunParallel(ct);

        Console.WriteLine($"Sequential: {sequential.ElapsedMs} ms");
        Console.WriteLine($"Parallel: {parallel.ElapsedMs} ms");

        Console.WriteLine(
            $"Speedup: {(parallel.ElapsedMs == 0 ? 0 : (double)sequential.ElapsedMs / parallel.ElapsedMs):F2}x");

        Console.WriteLine(
            $"Same results: {sequential.FlaggedOrderIds.SequenceEqual(parallel.FlaggedOrderIds)}");

        Console.WriteLine($"Flagged orders: {parallel.FlaggedOrderIds.Count}");
        Console.WriteLine($"Threads: {parallel.DistinctThreads}");
        Console.WriteLine($"CPU cores: {Environment.ProcessorCount}");
    }

    private async Task ConcurrentEmailsAsync(CancellationToken ct)
    {
        var emailService = new EmailService(_store, failuresEnabled: false);
        var customer = new Customer
        {
            Id = -1,
            Name = "Test Customer",
            Email = "test@example.com",
            IsActive = true
        };

        var timer = Stopwatch.StartNew();
        int active = 0;
        int maxActive = 0;

        await Parallel.ForEachAsync(
            Enumerable.Range(1001, 30),
            new ParallelOptions
            {
                CancellationToken = ct,
                MaxDegreeOfParallelism = 4
            },
            async (id, token) =>
            {
                int now = Interlocked.Increment(ref active);
                int observed;

                do
                {
                    observed = maxActive;
                    if (observed >= now) break;
                }
                while (Interlocked.CompareExchange(ref maxActive, now, observed) != observed);

                try
                {
                    await Task.Delay(1000, token);

                    await emailService.SendConfirmationAsync(
                        new Order { Id = id }, customer, token);
                }
                finally
                {
                    Interlocked.Decrement(ref active);
                }
            });

        timer.Stop();
        Console.WriteLine($"Emails sent: 30");
        Console.WriteLine($"Maximum concurrent sends: {maxActive}");
        Console.WriteLine($"Elapsed: {timer.Elapsed.TotalSeconds:F2} sec");
    }
}
