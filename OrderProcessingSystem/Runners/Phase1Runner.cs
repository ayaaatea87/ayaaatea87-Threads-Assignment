using System.Diagnostics;
using OrderProcessingSystem.Logging;
using OrderProcessingSystem.Models;
using OrderProcessingSystem.Repositories;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Storage;

namespace OrderProcessingSystem.Runners;

public class Phase1Runner
{
    private readonly JsonFileStore _store;
    private readonly SyncOrderValidator _syncValidator;
    private readonly OrderValidator _asyncValidator;
    private readonly IncomingOrderRepository _incomingOrderRepository;
    private readonly OrderRepository _orderRepository;

    public Phase1Runner(
        JsonFileStore store,
        SyncOrderValidator syncValidator,
        OrderValidator asyncValidator,
        IncomingOrderRepository incomingOrderRepository,
        OrderRepository orderRepository)
    {
        _store = store;
        _syncValidator = syncValidator;
        _asyncValidator = asyncValidator;
        _incomingOrderRepository = incomingOrderRepository;
        _orderRepository = orderRepository;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        // 1- Prepare clean data
        await DataSeeder.ResetAsync();

        List<Order> syncOrders = _store.Read<Order>("incoming_orders.json");

        Log.Info("===== Synchronous Validation =====");

        Stopwatch syncStopwatch = Stopwatch.StartNew();

        foreach (Order order in syncOrders)
        {
            ct.ThrowIfCancellationRequested();
            _syncValidator.ValidateOrder(order);
        }

        syncStopwatch.Stop();

        Log.Success($"Sync Time: {syncStopwatch.Elapsed.TotalSeconds:F1} seconds");

        // 2- Reset data so the async run starts from the same input.
        await DataSeeder.ResetAsync();

        List<Order> asyncOrders =
            await _incomingOrderRepository.GetAllAsync(ct);

        Log.Info("===== Asynchronous Validation =====");

        Stopwatch asyncStopwatch = Stopwatch.StartNew();

        foreach (Order order in asyncOrders)
        {
            ct.ThrowIfCancellationRequested();
            await _asyncValidator.ValidateOrderAsync(order, ct);
        }

        asyncStopwatch.Stop();

        // 3- Read the saved results from orders.json
        List<Order> allOrders =
            await _orderRepository.GetAllAsync(ct);

        List<Order> validatedOrders =
            await _orderRepository.GetByStatusAsync(
                OrderStatus.Validated, ct);

        List<Order> rejectedOrders =
            await _orderRepository.GetByStatusAsync(
                OrderStatus.Rejected, ct);

        // 4- Print the final summary
        Log.Info("===== Validation Summary =====");
        Log.Info($"Total Orders: {allOrders.Count}");
        Log.Success($"Validated: {validatedOrders.Count}");
        Log.Error($"Rejected: {rejectedOrders.Count}");

        foreach (Order order in rejectedOrders)
        {
            Log.Error($"- Order #{order.Id}: {order.FailureReason}");
        }

        Log.Info($"Sync Time: {syncStopwatch.Elapsed.TotalSeconds:F1} seconds");
        Log.Info($"Async Time: {asyncStopwatch.Elapsed.TotalSeconds:F1} seconds");
        Log.Info($"orders.json now contains {allOrders.Count} records.");
    }
}