using System;

using OrderProcessingSystem.Models;
using OrderProcessingSystem.Repositories;

namespace OrderProcessingSystem.Services;

public class SyncOrderValidator
{
    private readonly SyncCustomerRepository _customers;
    private readonly SyncProductRepository _products;
    private readonly SyncCouponRepository _coupons;

    public SyncOrderValidator(
        SyncCustomerRepository customers,
        SyncProductRepository products,
        SyncCouponRepository coupons)
    {
        _customers = customers;
        _products = products;
        _coupons = coupons;
    }

    public Order ValidateOrder(Order order)
    {
        Customer? customer =
            _customers.GetById(order.CustomerId);

        if (customer is null)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason =
                $"Customer #{order.CustomerId} not found";
            //simulate save 
            Thread.Sleep(900);
            return order;
        }

        if (!customer.IsActive)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason = "Customer is inactive";
            //simulate save 
            Thread.Sleep(900);
            return order;
        }

        foreach (OrderItem item in order.Items)
        {
            Product? product =
                _products.GetById(item.ProductId);

            if (product is null)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason =
                    $"Product #{item.ProductId} not found";
                //simulate save 
                Thread.Sleep(900);
                return order;
            }

            if (item.Quantity <= 0 ||
                product.StockQuantity < item.Quantity)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason =
                    $"Insufficient stock for '{product.Name}'";
                //simulate save 
                Thread.Sleep(900);
                return order;
            }

            item.UnitPrice = product.Price;
        }

        if (!string.IsNullOrWhiteSpace(order.CouponCode))
        {
            Coupon? coupon =
                _coupons.GetByCode(order.CouponCode);

            if (coupon is null)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason =
                    $"Coupon '{order.CouponCode}' does not exist";
                //simulate save 
                Thread.Sleep(900);
                return order;
            }

            if (coupon.ExpiryDate.Date < DateTime.Today)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason =
                    $"Coupon '{order.CouponCode}' has expired";
                //simulate save 
                Thread.Sleep(900);
                return order;
            }
        }

        order.Status = OrderStatus.Validated;
        order.FailureReason = null;
        //simulate save 
        Thread.Sleep(900);
        return order;
    }
}
