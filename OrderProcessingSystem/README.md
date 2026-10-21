## Phase 1 :
# 1- Why are sync and async times almost the same when orders are awaited one after another?

orders process sequantially one by one 
using async/await ensure that thread never blocked while waiting I/O opertaions 
it doesn't mean that execution time shorter using async/await
as we do validation then saving changes through SaveAsync method 

using synchronous and thread.sleep this blocked current thread
this process apply only validation and never save the result (R2)
time execution was shorter than Async because of the delay in SaveAsync()
I tried to simulate the delay time spent saving 
by using thread.sleep(900) 900 is the total simulated save delay
after that the time became:

[18:55:37.962] [Thread 9] Sync Time: 55.5 seconds
[18:55:37.963] [Thread 9] Async Time: 55.1 seconds
[18:55:37.964] [Thread 9] orders.json now contains 20 records.


##  Is the thread ID before await always the same after await? Why?
No ,When awaited task is incomplete the async method suspends
allowing the current thread to do other work instead of blocking while waiting.
When the task completes the method continues on a different thread

### 3/What does a thread do while `Task.Delay` is waiting? What about `Thread.Sleep`?

Thread.Sleep() blocks the current thread for the specified duration
await Task.Delay() suspends the async method without blocking a thread just to wait for the delay. 
The thread can return to the thread pool and be used for other work. 
When the delay completes, the async method resumes on a different thread.

### 4/ Why does a repository method return Task<T> instead of T?
Returning Task<T> allows the caller to use await to wait for the operation to complete without blocking a thread during I/O operations
such as reading data from a JSON file.


### Phase 2

### 1. What is the difference between starting a task and awaiting it?
Starting a task begins its operation; 
await asynchronously waits for it to finish without blocking the current thread.

### 2. Does every Task create a new thread? Does Task.Delay() use a thread while waiting?
No,Task.Delay() waits without blocking a thread.

### 3. What happens if you call .Result or .Wait() on an unfinished task instead of await? Why is it dangerous?
.Result and .Wait() block the current thread and may cause deadlocks

### 4. What happens to an exception thrown inside a task that is never awaited?
The exception may go unnoticed until observed, potentially causing unhandled task failures.

### 5. When should you use Task.Run(), and when should you avoid it?
use Task.Run() for CPU-bound work
avoid it for  asynchronous I/O operations.

### 6. What is the difference between Task and Task<T>?
Task represents an operation without a return value
while Task<T> returns a result of type T.

### 7. Why can a lock block not contain an await?
lock cannot contain await because the lock must be released synchronously
while await may suspend and resume later

### 8. Why did the unprotected stock reservation oversell, and how did you prevent it?
concurrent orders read the same available stock before updating it causing overselling
I prevented this by SemaphoreSlim


### ## Phase 4 

# For each of Tasks 10.1–10.4: which tool did you use, and why can't "start, then await one by one" meet
  
- 10.1: Task.WhenAny 
- 10.2: Task.WhenAll 
- 10.3: Task.WhenAny 
- 10.4 A: Parallel.For 
- 10.4 B: Parallel.ForEachAsync 

# In 10.2, when several loads fail, how do you get every exception? What does a plain await give you?
I collect exceptions from individual tasks  by `Task.WhenAll.Exception.InnerExceptions`

# In 10.3, what happens to the losing carriers if you never cancel them?
Losing carriers continue running and waste resources unless they receive a cancellation request and observe its token

# Why does 10.4 A get faster with more CPU cores, while 10.4 B does not depend on the number of cores?
Fraud scoring is CPU-bound and can benefit from more CPU cores
Email sending is I/O-bound so its concurrency limit controls rather than CPU usage
