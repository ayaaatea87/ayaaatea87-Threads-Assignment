
using OrderProcessingSystem.Exceptions;
using OrderProcessingSystem.Models;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Storage;

namespace OrderProcessingSystem.Repositories;

public class OrderRepository
{
    private readonly JsonFileStore _store;
    private readonly HashSet<int> _failSaveForOrderIds;

    public OrderRepository(
        JsonFileStore store,
        IEnumerable<int>? failSaveForOrderIds = null)
    {
        _store = store;
        _failSaveForOrderIds =
            failSaveForOrderIds?.ToHashSet() ?? new HashSet<int>();
    }

    public async Task SaveAsync(
        Order order,
        CancellationToken ct = default)
    {
        await SimulatedLatency.StepAsync(
            nameof(OrderRepository),
            "Opening connection",
            200, 200, ct);

        await SimulatedLatency.StepAsync(
            nameof(OrderRepository),
            "Beginning transaction",
            100, 100, ct);

        await SimulatedLatency.StepAsync(
            nameof(OrderRepository),
            $"Saving Order #{order.Id}",
            400, 400, ct);

        // Simulate a database failure after payment
        // but only when trying to save a Completed order
        if (order.Status == OrderStatus.Completed &&
            _failSaveForOrderIds.Contains(order.Id))
        {
            throw new DatabaseWriteException(
                "Connection lost during commit");
        }

        await _store.UpdateAsync<Order>(
            "orders.json",
            orders =>
            {
                int index = orders.FindIndex(
                    o => o.Id == order.Id);

                if (index >= 0)
                    orders[index] = order;
                else
                    orders.Add(order);
            },
            ct);

        await SimulatedLatency.StepAsync(
            nameof(OrderRepository),
            "Committing transaction",
            200, 200, ct);
    }

    public async Task<List<Order>> GetAllAsync(
        CancellationToken ct)
    {
        await SimulatedLatency.StepAsync(
            nameof(OrderRepository),
            "Opening connection",
            200, 200, ct);

        await SimulatedLatency.StepAsync(
            nameof(OrderRepository),
            "Executing query for all orders",
            300, 600, ct);

        return await _store.ReadAsync<Order>(
            "orders.json", ct);
    }

    public async Task<List<Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken ct)
    {
        await SimulatedLatency.StepAsync(
            nameof(OrderRepository),
            "Opening connection",
            200, 200, ct);

        await SimulatedLatency.StepAsync(
            nameof(OrderRepository),
            $"Executing query for status {status}",
            300, 600, ct);

        List<Order> orders =
            await _store.ReadAsync<Order>(
                "orders.json", ct);

        return orders
            .Where(o => o.Status == status)
            .ToList();
    }
}
