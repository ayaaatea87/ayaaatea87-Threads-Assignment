using System;
using OrderProcessingSystem.Logging;
using OrderProcessingSystem.Models;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Storage;

namespace OrderProcessingSystem.Repositories;

public class SyncProductRepository
{
    private readonly JsonFileStore _store;

    public SyncProductRepository(JsonFileStore store)
    {
        _store = store;
    }

    public Product? GetById(int id)
    {
        SyncSimulatedLatency.Step(
            nameof(SyncProductRepository),
            "Opening connection", 200, 200);

        SyncSimulatedLatency.Step(
            nameof(SyncProductRepository),
            $"Executing query for Product #{id}", 300, 600);

        List<Product> products =
            _store.Read<Product>("products.json");

        Product? product =
            products.FirstOrDefault(p => p.Id == id);

        if (product is null)
            Log.Error($"Product #{id} not found.");
        else
            Log.Success($"Product #{id} found ({product.Name}).");

        return product;
    }
}
