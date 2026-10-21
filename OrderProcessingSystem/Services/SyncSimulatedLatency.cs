using System;

using OrderProcessingSystem.Logging;

namespace OrderProcessingSystem.Services;

public static class SyncSimulatedLatency
{
    public static void Step(
        string component,
        string step,
        int minMs,
        int maxMs)
    {
        Log.Info($"{component} -> {step}...");

        int delayMs = Random.Shared.Next(minMs, maxMs + 1);
        Thread.Sleep(delayMs);

        Log.Success(
            $"{component} -> {step} completed ({delayMs} ms).");
    }
}
