using System;
using OrderProcessingSystem.Models;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Storage;

namespace OrderProcessingSystem.Repositories;

public class IncomingOrderRepository
{
    private readonly JsonFileStore _store;

    public IncomingOrderRepository(JsonFileStore store)
    {
        _store = store;
    }

    public async Task<List<Order>> GetAllAsync(CancellationToken ct)
    {
        await SimulatedLatency.StepAsync(
            nameof(IncomingOrderRepository),
            "Opening connection",
            200,
            200,
            ct);

        await SimulatedLatency.StepAsync(
            nameof(IncomingOrderRepository),
            "Executing query for incoming orders",
            300,
            600,
            ct);

        return await _store.ReadAsync<Order>(
            "incoming_orders.json",
            ct);
    }
}
