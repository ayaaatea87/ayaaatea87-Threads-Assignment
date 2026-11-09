using System;
using OrderProcessingSystem.Logging;

namespace OrderProcessingSystem.Services;

public static class SimulatedLatency
{
    public static async Task StepAsync(
        string component,
        string step,
        int minMs,
        int maxMs,
        CancellationToken ct)
    {
        Log.Info($"{component} -> {step}...");

        int delayMs = Random.Shared.Next(minMs, maxMs + 1);

        await Task.Delay(delayMs, ct);

        Log.Success(
            $"{component} -> {step} completed ({delayMs} ms).");
    }
}

