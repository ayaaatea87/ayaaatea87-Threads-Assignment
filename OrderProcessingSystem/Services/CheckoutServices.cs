using OrderProcessingSystem.Models;
using OrderProcessingSystem.Repositories;
using OrderProcessingSystem.Storage;
using OrderProcessingSystem.Exceptions;

namespace OrderProcessingSystem.Services;

// 1- Customer Service : get customer by Id
public sealed class CustomerService
{
    private readonly CustomerRepository _repository;

    public CustomerService(CustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Customer> GetCustomerAsync(
        int customerId,
        CancellationToken ct = default)
    {
        Customer? customer =
            await _repository.GetByIdAsync(customerId, ct);

        if (customer is null)
        {
            throw new CustomerNotFoundException(
                $"Customer #{customerId} not found");
        }

        return customer;
    }
}

// 2- Payment Models
public sealed class PaymentResult
{
    public bool Success { get; init; }
    public string Status { get; init; } = "";
    public string? TransactionId { get; init; }
    public string? Message { get; init; }
}

public sealed class PaymentRecord
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
    public string Status { get; set; } = "";
    public string? Message { get; set; }
}

// 3- Payment Interface
public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(
        int orderId,
        decimal amount,
        CancellationToken ct = default);

    Task RefundAsync(string transactionId);
}

// 4- Payment Gateway
public sealed class PaymentGateway : IPaymentGateway
{
    private readonly JsonFileStore _store;
    private readonly HashSet<int> _timeoutOrderIds;
    private readonly HashSet<int> _declineOrderIds;

    public PaymentGateway(
        JsonFileStore store,
        IEnumerable<int>? timeoutOrderIds = null,
        IEnumerable<int>? declineOrderIds = null,
        bool failuresEnabled = true)
    {
        _store = store;

        _timeoutOrderIds = failuresEnabled
            ? new HashSet<int>(timeoutOrderIds ?? [])
            : [];

        _declineOrderIds = failuresEnabled
            ? new HashSet<int>(declineOrderIds ?? [])
            : [];
    }
// handel case of timeOut , desclined , success :
    public async Task<PaymentResult> ProcessPaymentAsync(
        int orderId,
        decimal amount,
        CancellationToken ct = default)
    {
        // Simulate slow gateway for selected orders in order #5 delay 5000
        // others delay 700
        int delay = _timeoutOrderIds.Contains(orderId)
            ? 5000
            : 700;

        await Task.Delay(delay, ct);

 // Simulate a declined card. order #8 :
        if (_declineOrderIds.Contains(orderId))
        {
            await SavePaymentAsync(new PaymentRecord
            {
                OrderId = orderId,
                Amount = amount,
                Status = "Declined",
                Message = "Card declined by issuer"
            });

            return new PaymentResult
            {
                Success = false,
                Status = "Declined",
                Message = "Card declined by issuer"
            };
        }

// Successful payment
        string transactionId =
            "TXN-" + Guid.NewGuid().ToString("N")[..12]
                .ToUpperInvariant();

        await SavePaymentAsync(new PaymentRecord
        {
            OrderId = orderId,
            Amount = amount,
            TransactionId = transactionId,
            Status = "Captured"
        });

        return new PaymentResult
        {
            Success = true,
            Status = "Captured",
            TransactionId = transactionId
        };
    }
// RefundAsync used when payment done but saving order failed 
// order #13
    public async Task RefundAsync(string transactionId)
    {
        await _store.UpdateAsync<PaymentRecord>(
            "payments.json",
            payments =>
            {
                PaymentRecord? payment = payments
                    .LastOrDefault(p =>
                        p.TransactionId == transactionId);

                if (payment is null)
                {
                    throw new InvalidOperationException(
                        $"Transaction {transactionId} not found.");
                }

                payment.Status = "Refunded";
            },
            CancellationToken.None);
    }
// savePayment in json file
    private Task SavePaymentAsync(PaymentRecord payment)
    {
        return _store.UpdateAsync<PaymentRecord>(
            "payments.json",
            payments => payments.Add(payment),
            CancellationToken.None);
    }
}

// 5- Email Models
public sealed class EmailRecord
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

public sealed class EmailRetryRecord
{
    public int OrderId { get; set; }
    public int Attempts { get; set; }
    public string LastError { get; set; } = "";
}

// 6- Email Interface
public interface IEmailService
{
    Task SendConfirmationAsync(Order order, Customer customer, CancellationToken ct = default);

    Task QueueForRetryAsync(
        int orderId,
        int attempts,
        string lastError);
}

// 7- Email Service
public sealed class EmailService : IEmailService
{
    private readonly JsonFileStore _store;
    private readonly Dictionary<int, int> _failAttempts;
    private readonly Dictionary<int, int> _attempts = [];

    public EmailService(
        JsonFileStore store,
        Dictionary<int, int>? failAttempts = null,
        bool failuresEnabled = true)
    {
        _store = store;

        _failAttempts = failuresEnabled
            ? new Dictionary<int, int>(failAttempts ?? [])
            : [];
    }

    public async Task SendConfirmationAsync(
        Order order,
        Customer customer, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        int orderId = order.Id;

        _attempts.TryGetValue(orderId, out int previousAttempts);

        int currentAttempt = previousAttempts + 1;
        _attempts[orderId] = currentAttempt;

        // Simulate SMTP failures
        if (_failAttempts.TryGetValue(orderId, out int failures)
            && currentAttempt <= failures)
        {
            throw new SmtpUnavailableException(
                $"SMTP unavailable for Order #{orderId}, " +
                $"attempt {currentAttempt}.");
        }

        // Save the sent email.
        await _store.UpdateAsync<EmailRecord>(
            "emails.json",
            emails => emails.Add(new EmailRecord
            {
                OrderId = orderId,
                CustomerName = customer.Name,
                CustomerEmail = customer.Email,
                SentAt = DateTime.UtcNow
            }),
            CancellationToken.None);
    }

    public Task QueueForRetryAsync(
        int orderId,
        int attempts,
        string lastError)
    {
        return _store.UpdateAsync<EmailRetryRecord>(
            "email_retry.json",
            retries =>
            {
             // Keep only one retry record per order
                retries.RemoveAll(r => r.OrderId == orderId);

                retries.Add(new EmailRetryRecord
                {
                    OrderId = orderId,
                    Attempts = attempts,
                    LastError = lastError
                });
            },
            CancellationToken.None);
    }
}
