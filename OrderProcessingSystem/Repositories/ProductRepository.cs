using System;
using OrderProcessingSystem.Logging;
using OrderProcessingSystem.Models;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Storage;

namespace OrderProcessingSystem.Repositories;

public class ProductRepository
{
    private readonly JsonFileStore _store;

    public ProductRepository(JsonFileStore store)
    {
        _store = store;
    }

    public async Task<Product?> GetByIdAsync(
        int productId,
        CancellationToken ct)
    {
        await SimulatedLatency.StepAsync(
            nameof(ProductRepository),
            "Opening connection",
            200,
            200,
            ct);

        await SimulatedLatency.StepAsync(
            nameof(ProductRepository),
            $"Executing query for Product #{productId}",
            300,
            600,
            ct);

        List<Product> products =
            await _store.ReadAsync<Product>("products.json", ct);

        Product? product =
            products.FirstOrDefault(p => p.Id == productId);

        if (product is null)
        {
            Log.Error(
                $"ProductRepository -> Product #{productId} not found.");

            return null;
        }

        Log.Success(
            $"ProductRepository -> Product #{productId} found ({product.Name}).");

        return product;
    }
// handel reserved& release product using semaphoreslim in updateAsync 
// to avoid race conditions problem 
    public async Task<bool> ReserveStockAsync(int productId , int quantity , CancellationToken ct)
    {
        bool reserved = false;
        await _store.UpdateAsync<Product>("products.json",
        products =>
        {
            Product? product = products.FirstOrDefault(
                p => p.Id == productId);

            if (product is null || product.StockQuantity < quantity)
                return;

            product.StockQuantity -= quantity;
            reserved = true;
        }
        ,ct);
        return reserved;
    }
    public async Task ReleaseStockAsync(int productId, int quantity, CancellationToken ct)
    {
        await _store.UpdateAsync<Product>("products.json",
        products =>
        {
            Product? product = products.FirstOrDefault(
                p => p.Id == productId);

            if (product is null)
            {
                throw new InvalidOperationException(
                    $"Product #{productId} not found.");
            }

            product.StockQuantity += quantity;
        }
        , ct);
       }
    }


