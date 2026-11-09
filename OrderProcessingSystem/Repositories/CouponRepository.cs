using System;
using OrderProcessingSystem.Logging;
using OrderProcessingSystem.Models;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Storage;

namespace OrderProcessingSystem.Repositories;

public class CouponRepository
{
    private readonly JsonFileStore _store;

    public CouponRepository(JsonFileStore store)
    {
        _store = store;
    }

    public async Task<Coupon?> GetByCodeAsync(
        string code,
        CancellationToken ct)
    {
        await SimulatedLatency.StepAsync(
            nameof(CouponRepository),
            "Opening connection",
            200,
            200,
            ct);

        await SimulatedLatency.StepAsync(
            nameof(CouponRepository),
            $"Executing query for Coupon {code}",
            300,
            600,
            ct);

        List<Coupon> coupons =
            await _store.ReadAsync<Coupon>("coupons.json", ct);

        Coupon? coupon =
            coupons.FirstOrDefault(c =>
                string.Equals(
                    c.Code,
                    code,
                    StringComparison.OrdinalIgnoreCase));

        if (coupon is null)
        {
            Log.Error(
                $"CouponRepository -> Coupon {code} not found.");

            return null;
        }

        Log.Success(
            $"CouponRepository -> Coupon {code} found.");

        return coupon;
    }
}
