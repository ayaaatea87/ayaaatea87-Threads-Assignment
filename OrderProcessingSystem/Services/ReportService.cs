using System;
using System.Text;
using OrderProcessingSystem.Models;
using OrderProcessingSystem.Repositories;
using OrderProcessingSystem.Storage;

namespace OrderProcessingSystem.Services;

public class ReportService
{
    private readonly JsonFileStore _store;

    public ReportService(JsonFileStore store)
    {
        _store = store;
    }

    public async Task GenerateSalesReportAsync(CancellationToken ct)
    {
        List<Order> orders =
            await _store.ReadAsync<Order>("orders.json", ct);

        List<Product> products =
            await _store.ReadAsync<Product>("products.json", ct);
//LINQ !! 
        var sales = orders
            .Where(o => o.Status == OrderStatus.Completed)
            .SelectMany(o => o.Items)
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                QuantitySold = group.Sum(item => item.Quantity)
            })
            .Join(
                products,
                sale => sale.ProductId,
                product => product.Id,
                (sale, product) => new
                {
                    product.Name,
                    sale.QuantitySold
                })
            .OrderByDescending(sale => sale.QuantitySold)
            .ToList();

        var report = new StringBuilder();
        report.AppendLine("SALES REPORT");
        report.AppendLine("============");

        foreach (var sale in sales)
        {
            report.AppendLine(
                $"{sale.Name}: {sale.QuantitySold} units sold");
        }

        string reportDirectory = Path.Combine(
            AppContext.BaseDirectory, "Data", "reports");

        Directory.CreateDirectory(reportDirectory);

        string reportPath = Path.Combine(
            reportDirectory, "sales-report.txt");

        await File.WriteAllTextAsync(
            reportPath, report.ToString(), ct);
    }
}

