using System;

using OrderProcessingSystem.Models;

namespace OrderProcessingSystem.Services;

public class OrderPricer
{
    private readonly PricingService _pricingService;

    public OrderPricer(PricingService pricingService)
    {
        _pricingService = pricingService;
    }

    public async Task PriceOrderAsync(Order order,CancellationToken ct)
    {
    // should await here because both (GetDiscountAsync , CalculateShippingCostAsync) depend of calculating subtotal
        await _pricingService.CalculateSubTotalAsync(order, ct);

        Task<decimal> discountTask =
            _pricingService.GetDiscountAsync(order, ct);

        Task<decimal> shippingTask =
            _pricingService.CalculateShippingCostAsync(order, ct);

        await Task.WhenAll(discountTask, shippingTask);

        order.Discount = await discountTask;
        order.ShippingCost = await shippingTask;

        order.TotalAmount = Math.Round(
            order.SubTotal - order.Discount + order.ShippingCost,
            2,
            MidpointRounding.AwayFromZero);
    }

 // sequential task one a time
    public async Task PriceOrderSequentialAsync(Order order,CancellationToken ct)
    {
        await _pricingService.CalculateSubTotalAsync(order, ct);

        order.Discount =await _pricingService.GetDiscountAsync(order, ct);

        order.ShippingCost = await _pricingService.CalculateShippingCostAsync(order, ct);

        order.TotalAmount = Math.Round(
            order.SubTotal - order.Discount + order.ShippingCost,
            2,
            MidpointRounding.AwayFromZero);
    }
}

