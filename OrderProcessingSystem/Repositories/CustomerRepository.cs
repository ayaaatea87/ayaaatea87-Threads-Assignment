using System;
using OrderProcessingSystem.Logging;
using OrderProcessingSystem.Models;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Storage;

namespace OrderProcessingSystem.Repositories;

public class CustomerRepository
{
    private readonly JsonFileStore _store;

    public CustomerRepository(JsonFileStore store)
    {
        _store = store;
    }

    public async Task<Customer?> GetByIdAsync(
        int customerId,
        CancellationToken ct)
    {
        await SimulatedLatency.StepAsync(
            nameof(CustomerRepository),
            "Opening connection",
            200,
            200,
            ct);

        await SimulatedLatency.StepAsync(
            nameof(CustomerRepository),
            $"Executing query for Customer #{customerId}",
            300,
            600,
            ct);

        List<Customer> customers =
            await _store.ReadAsync<Customer>("customers.json", ct);

        Customer? customer =
            customers.FirstOrDefault(c => c.Id == customerId);

        if (customer is null)
        {
            Log.Error(
                $"CustomerRepository -> Customer #{customerId} not found.");

            return null;
        }

        Log.Success(
            $"CustomerRepository -> Customer #{customerId} found ({customer.Name}).");

        return customer;
    }
}

