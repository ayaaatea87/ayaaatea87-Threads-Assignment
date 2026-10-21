using System;


using OrderProcessingSystem.Models;
using OrderProcessingSystem.Repositories;
using OrderProcessingSystem.Exceptions;

namespace OrderProcessingSystem.Services;

public class PricingService
{
    private readonly ProductRepository _productRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly CouponRepository _couponRepository;
    private readonly HashSet<int> _failShippingForOrderIds;

    public PricingService(
        ProductRepository productRepository,
        CustomerRepository customerRepository,
        CouponRepository couponRepository,
        IEnumerable<int> failShippingForOrderIds)
    {
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _couponRepository = couponRepository;
        _failShippingForOrderIds = new HashSet<int> (failShippingForOrderIds);
    }

    public async Task<decimal> CalculateSubTotalAsync(Order order,CancellationToken ct)
    {
        decimal subTotal = 0m;

        foreach (OrderItem item in order.Items)
        {
            Product? product =
                await _productRepository.GetByIdAsync(item.ProductId, ct);

            if (product is null)
            {
                throw new InvalidOperationException(
                    $"Product #{item.ProductId} not found.");
            }

            item.UnitPrice = product.Price;
            subTotal += item.UnitPrice * item.Quantity;
        }

        order.SubTotal = RoundMoney(subTotal);
        return order.SubTotal;
    }

    public async Task<decimal> GetDiscountAsync(
        Order order,
        CancellationToken ct)
    {
        Customer? customer =
            await _customerRepository.GetByIdAsync(order.CustomerId, ct);

        if (customer is null)
        {
            throw new InvalidOperationException(
                $"Customer #{order.CustomerId} not found.");
        }

        decimal loyaltyPercent = customer.LoyaltyTier switch
        {
            LoyaltyTier.Bronze => 0m,
            LoyaltyTier.Silver => 5m,
            LoyaltyTier.Gold => 10m,
            _ => 0m
        };

        decimal couponPercent = 0m;

        if (!string.IsNullOrWhiteSpace(order.CouponCode))
        {
            Coupon? coupon =
                await _couponRepository.GetByCodeAsync(order.CouponCode, ct);

            if (coupon is not null &&
                coupon.ExpiryDate >= DateTime.UtcNow)
            {
                couponPercent = coupon.DiscountPercent;
            }
        }

        decimal totalDiscountPercent =
            Math.Min(loyaltyPercent + couponPercent, 30m);

        order.Discount = RoundMoney(
            order.SubTotal * totalDiscountPercent / 100m);

        return order.Discount;
    }

    public async Task<decimal> CalculateShippingCostAsync( Order order, CancellationToken ct)
    {
        if (_failShippingForOrderIds.Contains(order.Id))
        {
            throw new ShippingProviderUnavailableException("Shipping provider unavailable");
        }
        if (order.SubTotal >= 1000m)
        {
            order.ShippingCost = 0m;
            return order.ShippingCost;
        }

        int totalQuantity = 0;
        decimal totalWeight = 0m;

        foreach (OrderItem item in order.Items)
        {
            Product? product =
                await _productRepository.GetByIdAsync(item.ProductId, ct);

            if (product is null)
            {
                throw new InvalidOperationException(
                    $"Product #{item.ProductId} not found.");
            }

            totalQuantity += item.Quantity;
            totalWeight += product.WeightKg * item.Quantity;
        }

        decimal shippingCost =
            5m + 1.50m * totalQuantity + 2m * totalWeight;

        order.ShippingCost = RoundMoney(shippingCost);
        return order.ShippingCost;
    }

    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}

