using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace OrderProcessingSystem.Services;

public sealed class FraudScoringService
{
    public sealed record Result(
        List<int> FlaggedOrderIds,
        long ElapsedMs,
        int DistinctThreads);

    private static int Score(int orderId)
    {
        long h = orderId;

        for (int i = 0; i < 100_000; i++)
            h = (h * 31 + i) % 1_000_000_007;

        return (int)(h % 100);
    }

    public static Result RunSequential(CancellationToken ct)
    {
        var flagged = new List<int>();
        var timer = Stopwatch.StartNew();

        for (int id = 1; id <= 10_000; id++)
        {
            ct.ThrowIfCancellationRequested();

            if (Score(id) >= 90)
                flagged.Add(id);
        }

        timer.Stop();

        return new Result(
            flagged,
            timer.ElapsedMilliseconds,
            1);
    }

    public static Result RunParallel(CancellationToken ct)
    {
        var flagged = new List<int>();
        var threads = new HashSet<int>();
        object lockObj = new();

        var timer = Stopwatch.StartNew();

        Parallel.For(
            1,
            10_001,
            new ParallelOptions
            {
                CancellationToken = ct,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            },
            id =>
            {
                int score = Score(id);
                int threadId = Environment.CurrentManagedThreadId;

                lock (lockObj)
                {
                    threads.Add(threadId);

                    if (score >= 90)
                        flagged.Add(id);
                }
            });

        timer.Stop();

        flagged.Sort();

        return new Result(
            flagged,
            timer.ElapsedMilliseconds,
            threads.Count);
    }
}

