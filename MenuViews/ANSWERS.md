# Assignment 04 — Answers

Student name:  
Group / ID:

## Why Expected and Actual disagree

- because of race condition while using atomic operation (views++;)
- one line contain 3 operations ( read , add , write)
- if 2 threads read the value at the same time and update the counter at the same 
- this lead to lost update
- Expected: 400000      Actual:   204761
- expected each thread score 50000 views  >>  all 8 threads will 400000
- but the actual result was 204761 ( update lost ) cause of race conditions


## What you changed

- use interLock.Increment()
- to lock an atomic operations on one variable (views)

## When would you protect the update differently

- interLock for atomic opertaions with one variable
- But for conditions and severals varibles , use Lock Or Monior like( in part02,03)
