# Assignment 01 — Answers

Student name: aya atea  
Group / ID: 2

## Problems with the current design

- blocking operations by using ( thread.sleep() ) which block the current thread 
- Synchronous I/O :read customer profile file and  write invoice file synchronously caused blocking the current thread awaitng for completed this task 
- using foreach loop makes order process sequantially one order completed move to the next
- by increasing number of orders it will increase the total time export
-  First I try solving synchronous operations problem by converting to async await and keep using foreach 
- But the export time still the same ....
- Asynchronous operations solve the problem of blocking threads while waiting 
- But using the Foreach making the process running Sequantially 
- slove it by using Task.WhenALL with applying Concurrency 
- this improve  total time export from 6.0 to 1.4 s

## What you changed

- synchronous >>> asynchronous 
- thread.sleep() >> Task.Delay()
- using async...await
- convert method to Task 
- using WhenALL to solve problem of using Foreach
- time before : 6.0 s
- time after : 1.4 s
- improve total time export