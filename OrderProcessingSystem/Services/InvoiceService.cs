using System;


using System.Text;
using OrderProcessingSystem.Models;

namespace OrderProcessingSystem.Services;

public class InvoiceService
{
    public async Task GenerateInvoiceAsync(Order order,CancellationToken ct)
    {
        // create invoices folder
        string invoiceDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "invoices");

        Directory.CreateDirectory(invoiceDirectory);
        // create file for easch order
        string filePath = Path.Combine(
            invoiceDirectory,
            $"invoice_{order.Id}.txt");

        string invoiceContent = await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();

            // Simulate CPU-intensive invoice processing.
            long checksum = 0;

            for (int i = 0; i < 20_000_000; i++)
            {
                if (i % 100_000 == 0)
                    ct.ThrowIfCancellationRequested();

                checksum += i % 97;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Invoice #{order.Id}");
            sb.AppendLine($"Customer ID: {order.CustomerId}");
            sb.AppendLine($"Subtotal: {order.SubTotal:F2}");
            sb.AppendLine($"Discount: {order.Discount:F2}");
            sb.AppendLine($"Shipping: {order.ShippingCost:F2}");
            sb.AppendLine($"Total: {order.TotalAmount:F2}");
            sb.AppendLine($"Checksum: {checksum}");

            return sb.ToString();
        }, ct);

        await File.WriteAllTextAsync(filePath, invoiceContent, ct);
    }
}

