# ADR 004: In-Memory State Management (Volatile Persistence)

**Status:** Accepted
**Date:** 2026-01-29
**Deciders:** Ramon Reyes
**Technical Story:** N/A

## Context and Problem Statement
We are building a high-performance prototype to demonstrate architectural patterns (Channels, Locking, Background Services). We need to validate the logic without the overhead of setting up external dependencies like SQL Server or Redis.

## Decision Outcome
State (`PorkInventory` and `PorkBatch`) will be held in memory via a Singleton service (`MeatLocker`).

## Consequences
* **Positive:** Extremely high performance (zero network latency for data retrieval).
* **Positive:** Zero deployment friction (runs immediately via `dotnet run`).
* **Negative:** **Data Loss.** All inventory and order history is lost if the application restarts or crashes.
* **Mitigation:** This is explicitly acceptable for the "FreshStream" prototype phase. It must be replaced with a persistent backing store (PostgreSQL or Redis) before production release.