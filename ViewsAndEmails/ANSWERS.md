# Assignment 05 — Answers

Student name:  Aya Atea
Group / ID: #2

## Failures you observed

- traditional collections are not Thread Saftey
- race conditions problem cause exceptions, lost updates

## What you changed

- Dictionary to ConcurrentDictionary
- Queue to ConcurrentQueue
- AddOrUpdate() to safely update product view counts
- TryDequeue() to safely retrieve email jobs

## Collection safety vs business-rule safety

- Collection safety ensures that individual collection operations are thread-safe, 
- but it doesn't guarantee business-rule safety, 
- especially when multiple operations or conditions must be handled together
