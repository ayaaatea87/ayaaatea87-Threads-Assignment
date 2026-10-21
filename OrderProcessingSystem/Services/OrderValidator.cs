using OrderProcessingSystem.Models;
using OrderProcessingSystem.Repositories;

namespace OrderProcessingSystem.Services;

public class OrderValidator
{
    private readonly CustomerRepository _customerRepository;
    private readonly ProductRepository _productRepository;
    private readonly CouponRepository _couponRepository;
    private readonly OrderRepository _orderRepository;

    public OrderValidator(
        CustomerRepository customerRepository,
        ProductRepository productRepository,
        CouponRepository couponRepository,
        OrderRepository orderRepository)
    {
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _couponRepository = couponRepository;
        _orderRepository = orderRepository;
    }

    public async Task<Order> ValidateOrderAsync(
        Order order,
        CancellationToken ct)
    {
        // 1- Check customer exists
        Customer? customer =
            await _customerRepository.GetByIdAsync(order.CustomerId, ct);

        if (customer is null)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason =
                $"Customer #{order.CustomerId} not found";

            await _orderRepository.SaveAsync(order, ct);
            return order;
        }

        // 2- Check customer is active
        if (!customer.IsActive)
        {
            order.Status = OrderStatus.Rejected;
            order.FailureReason = "Customer is inactive";

            await _orderRepository.SaveAsync(order, ct);
            return order;
        }

        // 3- Check products exist and stock 
        foreach (OrderItem item in order.Items)
        {
            Product? product =
                await _productRepository.GetByIdAsync(item.ProductId, ct);

            if (product is null)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason =
                    $"Product #{item.ProductId} not found";

                await _orderRepository.SaveAsync(order, ct);
                return order;
            }

            if (item.Quantity <= 0)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason =
                    $"Insufficient stock for '{product.Name}'";

                await _orderRepository.SaveAsync(order, ct);
                return order;
            }

            if (product.StockQuantity < item.Quantity)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason =
                    $"Insufficient stock for '{product.Name}'";

                await _orderRepository.SaveAsync(order, ct);
                return order;
            }

            item.UnitPrice = product.Price;
        }

        // 4- Check coupon exists
        if (!string.IsNullOrWhiteSpace(order.CouponCode))
        {
            Coupon? coupon =
                await _couponRepository.GetByCodeAsync(order.CouponCode, ct);

            if (coupon is null)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason =
                    $"Coupon '{order.CouponCode}' does not exist";

                await _orderRepository.SaveAsync(order, ct);
                return order;
            }

            // 5- Check coupon expiry
            if (coupon.ExpiryDate.Date < DateTime.Today)
            {
                order.Status = OrderStatus.Rejected;
                order.FailureReason =
                    $"Coupon '{order.CouponCode}' has expired";

                await _orderRepository.SaveAsync(order, ct);
                return order;
            }
        }

        // All validations passed
        order.Status = OrderStatus.Validated;
        order.FailureReason = null;

        await _orderRepository.SaveAsync(order, ct);

        return order;
    }
}