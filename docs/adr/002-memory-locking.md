# ADR 002: In-Memory Locking for Inventory Transactions

**Status:** Accepted
**Date:** 2026-01-29
**Deciders:** Ramon Reyes
**Technical Story:** N/A

## Context and Problem Statement
Allocating inventory requires a multi-step transactional check:
1.  Filter batches for expiration.
2.  Sum available weight across multiple batches.
3.  If sufficient, deduct weight from specific batches.

Standard concurrent collections (`ConcurrentDictionary`, `ConcurrentBag`) ensure atomic operations for *single* actions, but not for multi-step workflows. Without a transaction boundary, race conditions will occur (e.g., two users ordering the last 10lbs simultaneously will both succeed, resulting in -10lbs inventory).

## Decision Drivers
* Must prevent "overselling" inventory at all costs.
* Must maintain accurate stock levels during concurrent requests.

## Decision Outcome
We will use a standard C# `lock` statement (Monitor) on a private `_lock` object within the `MeatLocker` service. This enforces strict mutual exclusion during the "Check" and "Deduct" phases.

## Consequences
* **Positive:** Guarantees data consistency. It is impossible to oversell inventory.
* **Positive:** Simple to implement compared to distributed locking or database optimistic concurrency control.
* **Negative:** Reduces throughput. Only one order can be processed at a time.
    * *Mitigation:* Given the in-memory nature of the operation, the lock duration is extremely short (nanoseconds/microseconds), making this acceptable for current loads.