using System;

using OrderProcessingSystem.Logging;
using OrderProcessingSystem.Models;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Storage;

namespace OrderProcessingSystem.Repositories;

public class SyncCustomerRepository
{
    private readonly JsonFileStore _store;

    public SyncCustomerRepository(JsonFileStore store)
    {
        _store = store;
    }

    public Customer? GetById(int id)
    {
        SyncSimulatedLatency.Step(
            nameof(SyncCustomerRepository),
            "Opening connection", 200, 200);

        SyncSimulatedLatency.Step(
            nameof(SyncCustomerRepository),
            $"Executing query for Customer #{id}", 300, 600);

        List<Customer> customers =
            _store.Read<Customer>("customers.json");

        Customer? customer =
            customers.FirstOrDefault(c => c.Id == id);

        if (customer is null)
            Log.Error($"Customer #{id} not found.");
        else
            Log.Success($"Customer #{id} found ({customer.Name}).");

        return customer;
    }
}
