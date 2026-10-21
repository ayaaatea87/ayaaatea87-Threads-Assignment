using System;
using OrderProcessingSystem.Models;
namespace OrderProcessingSystem.Storage;

public class DataSeeder
{
    //  reset/ remove data folder and create new one in each phase
    public static async Task ResetAsync (CancellationToken cancellationToken = default)
    {
        //1- apply Data Folder Path
        string dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
        //3- create new folder at the same path
        Directory.CreateDirectory(dataDirectory);

// Adding Customers Data:
        var customers = new List<Customer>
        {
    new Customer { Id = 1, Name = "Ahmed Ali", IsActive = true, LoyaltyTier = LoyaltyTier.Gold, Email = "ahmed.ali@example.com" },
    new Customer { Id = 2, Name = "Sara Mohamed", IsActive = true, LoyaltyTier = LoyaltyTier.Silver, Email = "sara.mohamed@example.com" },
    new Customer { Id = 3, Name = "Omar Hassan", IsActive = true, LoyaltyTier = LoyaltyTier.Bronze, Email = "omar.hassan@example.com" },
    new Customer { Id = 4, Name = "Mona Ibrahim", IsActive = false, LoyaltyTier = LoyaltyTier.Silver, Email = "mona.ibrahim@example.com" },
    new Customer { Id = 5, Name = "Youssef Adel", IsActive = true, LoyaltyTier = LoyaltyTier.Gold, Email = "youssef.adel@example.com" },
    new Customer { Id = 6, Name = "Nour Khaled", IsActive = true, LoyaltyTier = LoyaltyTier.Bronze, Email = "nour.khaled@example.com" },
    new Customer { Id = 7, Name = "Karim Samir", IsActive = true, LoyaltyTier = LoyaltyTier.Silver, Email = "karim.samir@example.com" },
    new Customer { Id = 8, Name = "Laila Mahmoud", IsActive = true, LoyaltyTier = LoyaltyTier.Gold, Email = "laila.mahmoud@example.com" },
    new Customer { Id = 9, Name = "Hassan Tarek", IsActive = true, LoyaltyTier = LoyaltyTier.Bronze, Email = "hassan.tarek@example.com" },
    new Customer { Id = 10, Name = "Dina Fathy", IsActive = true, LoyaltyTier = LoyaltyTier.Silver, Email = "dina.fathy@example.com" }
        };
 // save data in json file:
        var store = new JsonFileStore();
        await store.WriteAsync(DataFiles.Customers, customers, cancellationToken);

// Adding Products Data:
        var products = new List<Product>
        {
    new Product { Id = 1, Name = "Laptop", Price = 1200.00m, StockQuantity = 10, WeightKg = 2.2m },
    new Product { Id = 2, Name = "Smartphone", Price = 800.00m, StockQuantity = 15, WeightKg = 0.4m },
    new Product { Id = 3, Name = "Wireless Mouse", Price = 25.00m, StockQuantity = 2, WeightKg = 0.1m },
    new Product { Id = 4, Name = "Mechanical Keyboard", Price = 90.00m, StockQuantity = 1, WeightKg = 0.9m },
    new Product { Id = 5, Name = "USB-C Hub", Price = 45.00m, StockQuantity = 30, WeightKg = 0.2m },
    new Product { Id = 6, Name = "Headphones", Price = 150.00m, StockQuantity = 12, WeightKg = 0.3m },
    new Product { Id = 7, Name = "27\" Monitor", Price = 350.00m, StockQuantity = 6, WeightKg = 5.0m },
    new Product { Id = 8, Name = "Webcam", Price = 70.00m, StockQuantity = 20, WeightKg = 0.2m }
        };
// save data in json file:
       await store.WriteAsync(DataFiles.Products , products , cancellationToken );
// Adding Coupons Data:
        var coupons = new List<Coupon>
        {
    new Coupon
    {
        Code = "SAVE10",
        DiscountPercent = 10,
        ExpiryDate = DateTime.Today.AddDays(180)
    },
    new Coupon
    {
        Code = "WELCOME15",
        DiscountPercent = 15,
        ExpiryDate = DateTime.Today.AddDays(90)
    },
    new Coupon
    {
        Code = "FLASH20",
        DiscountPercent = 20,
        ExpiryDate = DateTime.Today.AddDays(7)
    },
    new Coupon
    {
        Code = "OLD20",
        DiscountPercent = 20,
        ExpiryDate = DateTime.Today.AddDays(-30)
    }
        };
// save data in json file:
       await store.WriteAsync(DataFiles.Coupons, coupons , cancellationToken );

// Adding Orders Data:
        var incomingOrders = new List<Order>
        {
            new Order
            {
                Id = 1,
                CustomerId = 1,
                Items = new List<OrderItem>
                {
                   new OrderItem{ProductId = 1, Quantity = 1,UnitPrice = 0m,},
                   new OrderItem{ProductId = 3, Quantity = 1,UnitPrice = 0m,},
                },
                CouponCode = "SAVE10",
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.Now,
                SubTotal = 0m,
                Discount = 0m,
                ShippingCost = 0m,
                TotalAmount = 0m,
                FailureReason = null
            },
            new Order
            {
            Id = 2,
            CustomerId = 2,
            Items = new List<OrderItem>
            {
              new OrderItem { ProductId = 2, Quantity = 1, UnitPrice = 0m }
            },
            CouponCode = null,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.Now,
            SubTotal = 0m,
            Discount = 0m,
            ShippingCost = 0m,
            TotalAmount = 0m,
            FailureReason = null
            },
        new Order
        {
        Id = 3,
        CustomerId = 3,
        Items = new List<OrderItem>
        {
            new OrderItem { ProductId = 5, Quantity = 2, UnitPrice = 0m },
            new OrderItem { ProductId = 8, Quantity = 1, UnitPrice = 0m }
        },
        CouponCode = "WELCOME15",
        Status = OrderStatus.Pending,
        CreatedAt = DateTime.Now,
        SubTotal = 0m,
        Discount = 0m,
        ShippingCost = 0m,
        TotalAmount = 0m,
        FailureReason = null
        },
    new Order
    {
        Id = 4,
        CustomerId = 4,
        Items = new List<OrderItem>
        {
            new OrderItem { ProductId = 6, Quantity = 1, UnitPrice = 0m }
        },
        CouponCode = null,
        Status = OrderStatus.Pending,
        CreatedAt = DateTime.Now,
        SubTotal = 0m,
        Discount = 0m,
        ShippingCost = 0m,
        TotalAmount = 0m,
        FailureReason = null
    },
    new Order
    {
        Id = 5,
        CustomerId = 5,
        Items = new List<OrderItem>
        {
            new OrderItem { ProductId = 7, Quantity = 1, UnitPrice = 0m }
        },
        CouponCode = null,
        Status = OrderStatus.Pending,
        CreatedAt = DateTime.Now,
        SubTotal = 0m,
        Discount = 0m,
        ShippingCost = 0m,
        TotalAmount = 0m,
        FailureReason = null
    },
    new Order
    {
        Id = 6,
        CustomerId = 6,
        Items = new List<OrderItem>
        {
            new OrderItem { ProductId = 5, Quantity = 1, UnitPrice = 0m }
        },
        CouponCode = "FLASH20",
        Status = OrderStatus.Pending,
        CreatedAt = DateTime.Now,
        SubTotal = 0m,
        Discount = 0m,
        ShippingCost = 0m,
        TotalAmount = 0m,
        FailureReason = null
    },
    new Order
    {
        Id = 7,
        CustomerId = 7,
        Items = new List<OrderItem>
        {
            new OrderItem { ProductId = 6, Quantity = 1, UnitPrice = 0m },
            new OrderItem { ProductId = 8, Quantity = 1, UnitPrice = 0m }
        },
        CouponCode = null,
        Status = OrderStatus.Pending,
        CreatedAt = DateTime.Now,
        SubTotal = 0m,
        Discount = 0m,
        ShippingCost = 0m,
        TotalAmount = 0m,
        FailureReason = null
    },
    new Order
    {
        Id = 8,
        CustomerId = 8,
        Items = new List<OrderItem>
        {
            new OrderItem { ProductId = 1, Quantity = 1, UnitPrice = 0m }
        },
        CouponCode = "SAVE10",
        Status = OrderStatus.Pending,
        CreatedAt = DateTime.Now,
        SubTotal = 0m,
        Discount = 0m,
        ShippingCost = 0m,
        TotalAmount = 0m,
        FailureReason = null
    },
    new Order
    {
        Id = 9,
        CustomerId = 9,
        Items = new List<OrderItem>
        {
            new OrderItem { ProductId = 3, Quantity = 5, UnitPrice = 0m }
        },
        CouponCode = null,
        Status = OrderStatus.Pending,
        CreatedAt = DateTime.Now,
        SubTotal = 0m,
        Discount = 0m,
        ShippingCost = 0m,
        TotalAmount = 0m,
        FailureReason = null
    },
    new Order
    {
        Id = 10,
        CustomerId = 10,
        Items = new List<OrderItem>
        {
            new OrderItem { ProductId = 4, Quantity = 1, UnitPrice = 0m }
        },
        CouponCode = null,
        Status = OrderStatus.Pending,
        CreatedAt = DateTime.Now,
        SubTotal = 0m,
        Discount = 0m,
        ShippingCost = 0m,
        TotalAmount = 0m,
        FailureReason = null
    },
    new Order
    {
        Id = 11,
        CustomerId = 1,
        Items = new List<OrderItem>
        {
            new OrderItem { ProductId = 4, Quantity = 1, UnitPrice = 0m }
        },
        CouponCode = null,
        Status = OrderStatus.Pending,
        CreatedAt = DateTime.Now,
        SubTotal = 0m,
        Discount = 0m,
        ShippingCost = 0m,
        TotalAmount = 0m,
        FailureReason = null
    },
    new Order
    {
        Id = 12,
        CustomerId = 2,
        Items = new List<OrderItem>
        {
            new OrderItem { ProductId = 2, Quantity = 1, UnitPrice = 0m }
        },
        CouponCode = "OLD20",
        Status = OrderStatus.Pending,
        CreatedAt = DateTime.Now,
        SubTotal = 0m,
        Discount = 0m,
        ShippingCost = 0m,
        TotalAmount = 0m,
        FailureReason = null
    },
    new Order
    {
        Id = 13,
        CustomerId = 3,
        Items = new List<OrderItem>
        {
            new OrderItem { ProductId = 6, Quantity = 2, UnitPrice = 0m }
        },
        CouponCode = null,
        Status = OrderStatus.Pending,
        CreatedAt = DateTime.Now,
        SubTotal = 0m,
        Discount = 0m,
        ShippingCost = 0m,
        TotalAmount = 0m,
        FailureReason = null
    },
    new Order
{
    Id = 14,
    CustomerId = 5,
    Items = new List<OrderItem>
    {
        new OrderItem { ProductId = 8, Quantity = 2, UnitPrice = 0m },
        new OrderItem { ProductId = 5, Quantity = 1, UnitPrice = 0m }
    },
    CouponCode = "WELCOME15",
    Status = OrderStatus.Pending,
    CreatedAt = DateTime.Now,
    SubTotal = 0m,
    Discount = 0m,
    ShippingCost = 0m,
    TotalAmount = 0m,
    FailureReason = null
},
new Order
{
    Id = 15,
    CustomerId = 99,
    Items = new List<OrderItem>
    {
        new OrderItem { ProductId = 2, Quantity = 1, UnitPrice = 0m }
    },
    CouponCode = null,
    Status = OrderStatus.Pending,
    CreatedAt = DateTime.Now,
    SubTotal = 0m,
    Discount = 0m,
    ShippingCost = 0m,
    TotalAmount = 0m,
    FailureReason = null
},
new Order
{
    Id = 16,
    CustomerId = 6,
    Items = new List<OrderItem>
    {
        new OrderItem { ProductId = 7, Quantity = 1, UnitPrice = 0m },
        new OrderItem { ProductId = 5, Quantity = 1, UnitPrice = 0m }
    },
    CouponCode = null,
    Status = OrderStatus.Pending,
    CreatedAt = DateTime.Now,
    SubTotal = 0m,
    Discount = 0m,
    ShippingCost = 0m,
    TotalAmount = 0m,
    FailureReason = null
},
new Order
{
    Id = 17,
    CustomerId = 7,
    Items = new List<OrderItem>
    {
        new OrderItem { ProductId = 2, Quantity = 1, UnitPrice = 0m }
    },
    CouponCode = "SAVE10",
    Status = OrderStatus.Pending,
    CreatedAt = DateTime.Now,
    SubTotal = 0m,
    Discount = 0m,
    ShippingCost = 0m,
    TotalAmount = 0m,
    FailureReason = null
},
new Order
{
    Id = 18,
    CustomerId = 8,
    Items = new List<OrderItem>
    {
        new OrderItem { ProductId = 6, Quantity = 1, UnitPrice = 0m }
    },
    CouponCode = "FAKE50",
    Status = OrderStatus.Pending,
    CreatedAt = DateTime.Now,
    SubTotal = 0m,
    Discount = 0m,
    ShippingCost = 0m,
    TotalAmount = 0m,
    FailureReason = null
},
new Order
{
    Id = 19,
    CustomerId = 9,
    Items = new List<OrderItem>
    {
        new OrderItem { ProductId = 8, Quantity = 1, UnitPrice = 0m }
    },
    CouponCode = null,
    Status = OrderStatus.Pending,
    CreatedAt = DateTime.Now,
    SubTotal = 0m,
    Discount = 0m,
    ShippingCost = 0m,
    TotalAmount = 0m,
    FailureReason = null
},
new Order
{
    Id = 20,
    CustomerId = 10,
    Items = new List<OrderItem>
    {
        new OrderItem { ProductId = 1, Quantity = 1, UnitPrice = 0m },
        new OrderItem { ProductId = 6, Quantity = 1, UnitPrice = 0m }
    },
    CouponCode = "FLASH20",
    Status = OrderStatus.Pending,
    CreatedAt = DateTime.Now,
    SubTotal = 0m,
    Discount = 0m,
    ShippingCost = 0m,
    TotalAmount = 0m,
    FailureReason = null
}
        };
        // save data in json file:
        await store.WriteAsync(DataFiles.IncomingOrders, incomingOrders, cancellationToken);

        // create the Empty files:
        await store.WriteAsync(DataFiles.Orders, new List<Order>() , cancellationToken);
        await store.WriteAsync(DataFiles.Payments, new List<PaymentTransaction>(), cancellationToken);
        await store.WriteAsync(DataFiles.Emails, new List<EmailMessage>(), cancellationToken);
        await store.WriteAsync(DataFiles.EmailRetry, new List<EmailMessage>(), cancellationToken);



    }
}
