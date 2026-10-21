# Assignment 03 — Answers

Student name:  Aya Atea
Group / ID: #2

## 1.What is `System.Threading.Monitor` used for?
System.Threading.Monitor is a class used to synchronize access to shared resources between threads.
which means only one thread at a time can execute a critical section protected by the same lock object

## 2.What does the C# `lock` statement compile to?
the C# compiler uses (Monitor.Enter) and (Monitor.Exit) inside a try/finally to ensure that lock release if any exception thrown

## 3.List at least two differences between using `lock` and calling `Monitor` APIs directly.?
## lOCK :
1- easy to read syntax that protect the critical section in code
2- always release automatically the lock when exits the block

## MONITOR :
1- thread that acquire the lock must release the lock by calling Monitor.Exit()
2- additional controle with additional method like ( TryEnter, Wait, Pulse, and PulseAll)

## 4.When might a developer use `Monitor.TryEnter`, `Monitor.Pulse`, or `Monitor.Wait` instead of a plain `lock`?
# Monitor.Wait:
When a thread needs to wait for a condition to change
it releases the monitor while waiting and reacquires it before returning.

# Monitor.TryEnter:
when thread try to acquire the lock for limited time instead of waiting

# Monitor.Pulse:
Sends a signal to one or more waiting threads. The signal notifies a waiting thread that the state of the locked object has changed,
and the owner of the lock is ready to release the lock.

## 5.Write the same critical section twice: once with `lock`, once with `Monitor.Enter` / `Monitor.Exit`.?
 i used code in part02 converting from using lock to monitor
 in (FridayNightBooking.cs)

Links:

- https://learn.microsoft.com/
