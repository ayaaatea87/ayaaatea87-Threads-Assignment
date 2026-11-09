using System;

using OrderProcessingSystem.Logging;
using OrderProcessingSystem.Models;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Storage;

namespace OrderProcessingSystem.Repositories;

public class SyncCouponRepository
{
    private readonly JsonFileStore _store;

    public SyncCouponRepository(JsonFileStore store)
    {
        _store = store;
    }

    public Coupon? GetByCode(string code)
    {
        SyncSimulatedLatency.Step(
            nameof(SyncCouponRepository),
            "Opening connection", 200, 200);

        SyncSimulatedLatency.Step(
            nameof(SyncCouponRepository),
            $"Executing query for Coupon {code}", 300, 600);

        List<Coupon> coupons =
            _store.Read<Coupon>("coupons.json");

        Coupon? coupon = coupons.FirstOrDefault(c =>
            string.Equals(
                c.Code,
                code,
                StringComparison.OrdinalIgnoreCase));

        if (coupon is null)
            Log.Error($"Coupon '{code}' does not exist.");
        else
            Log.Success($"Coupon '{code}' found.");

        return coupon;
    }
}
