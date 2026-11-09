# Threads Assignment

A .NET 10 console application exploring asynchronous programming, multithreading, concurrency, and performance optimization through practical exercises.

## Technologies

- C# 
- .NET 10
- Async / Await
- Task Parallel Library
- SemaphoreSlim
- CancellationToken
- JSON File Storage

## Project Phases

The assignment is divided into four phases:

### Phase 1 – Async Fundamentals
- Working with synchronous and asynchronous operations.
- Data validation and JSON storage.
- Understanding async/await.

**Results:** 15 validated, 5 rejected.

### Phase 2 – Concurrency
- Handling concurrent operations.
- Preventing race conditions.
- Processing orders and generating invoices.

**Results:** 13 completed, 6 rejected, 1 failed.

### Phase 3 – Checkout Workflow
- Checkout and payment processing.
- Refund handling.
- Email notifications, retries, and cancellation.

**Results:** 11 completed, 6 rejected, 2 payment failures, 1 failed.

### Phase 4 – Parallel Processing
- Processing tasks concurrently.
- Reporting progress as tasks complete.
- Parallel fraud detection.
- Bounded email processing.

## Performance Results

| Benchmark | Sequential | Parallel |
|---|---:|---:|
| Invoice Export | ~6 seconds | ~1.4 seconds |
| Fraud Detection | 18,743 ms | 5,310 ms |

Parallel fraud detection achieved approximately **3.53x speedup**.

## How to Run

Clone the repository:

```bash
git clone https://github.com/ayaaatea87/ayaaatea87-Threads-Assignment.git
```

Navigate to the project directory and run a specific phase:

```bash
dotnet run -- phase1
dotnet run -- phase2
dotnet run -- phase3
dotnet run -- phase4
```

Run all phases:

```bash
dotnet run -- all
```

Run with a cancellation timeout:

```bash
dotnet run -- phase4 --cancel-after 2000
```

Running without arguments opens the application menu.

## Key Concepts

- Asynchronous programming
- Concurrency and parallelism
- Race condition prevention
- Thread synchronization
- Cancellation and exception handling
- Performance measurement and optimization

---

**Author:** Aya Atea

[GitHub](https://github.com/ayaaatea87) · [LinkedIn](https://www.linkedin.com/in/aya-atea-6766a831a/)
