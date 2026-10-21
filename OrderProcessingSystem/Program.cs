using OrderProcessingSystem.Storage;
using OrderProcessingSystem.Services;
using OrderProcessingSystem.Logging;
using OrderProcessingSystem.Repositories;
using OrderProcessingSystem.Runners;

try
{

    JsonFileStore store = new JsonFileStore();

    // Synchronous repositories
    SyncCustomerRepository syncCustomerRepository =new SyncCustomerRepository(store);
    SyncProductRepository syncProductRepository =new SyncProductRepository(store);
    SyncCouponRepository syncCouponRepository =new SyncCouponRepository(store);
    SyncOrderValidator syncValidator = new SyncOrderValidator(
        syncCustomerRepository,
        syncProductRepository,
        syncCouponRepository);

    // Asynchronous repositories
    CustomerRepository customerRepository =new CustomerRepository(store);

    ProductRepository productRepository =new ProductRepository(store);
    CouponRepository couponRepository =new CouponRepository(store);
    OrderRepository orderRepository =new OrderRepository(store,failSaveForOrderIds: new[] { 13 });
    IncomingOrderRepository incomingOrderRepository = new IncomingOrderRepository(store);
    OrderValidator asyncValidator = new OrderValidator(
        customerRepository,
        productRepository,
        couponRepository,
        orderRepository);

    // Run Phase 1
Phase1Runner runner = new Phase1Runner(
        store,
        syncValidator,
        asyncValidator,
        incomingOrderRepository,
        orderRepository);

    await runner.RunAsync(CancellationToken.None);

Phase2Runner phase2Runner = new Phase2Runner(
    store,
    orderRepository,
    productRepository,
    customerRepository,
    couponRepository);

    await phase2Runner.RunAsync(CancellationToken.None);

    Phase3Runner phase3Runner = new Phase3Runner(
    store,
    orderRepository,
    productRepository,
    customerRepository,
    couponRepository);

    await phase3Runner.RunAsync(CancellationToken.None);

    Phase4Runner phase4Runner = new Phase4Runner(
    store,
    orderRepository,
    productRepository,
    customerRepository,
    couponRepository);

    await phase4Runner.RunAsync(CancellationToken.None);
}
catch (Exception ex)
{
    Console.WriteLine("\n========== ERROR ==========");
    Console.WriteLine(ex.ToString());
}
finally
{
    Console.WriteLine("\nPress Enter to exit...");
    Console.ReadLine();
}

Console.ReadLine();
