# Assignment 02 — Answers

Student name: AYA ATEA 
Group / ID: #2

## What is wrong
- race conditions the result depends on which thread run first
- threads shared data in heap ( TablesLeft) 
- but each one has its own copy on stack (TablesLeft--; Booked++;)
- two thread read TableLeft Value = 1 at the same time ,each one increase Booked and decrease TableLeft
- the expected left 0 But The result was left -1 

## How you fixed it

- using lock on object 
- any thread ask for the same _sync wait for the locked thread completed
