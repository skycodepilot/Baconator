# ADR 001: Asynchronous Order Ingestion via System.Threading.Channels

**Status:** Accepted
**Date:** 2026-01-29
**Deciders:** Ramon Reyes
**Technical Story:** N/A

## Context and Problem Statement
The system must handle high-velocity "bursts" of orders (e.g., during flash sales). Processing these orders synchronously—waiting for database writes, inventory allocation algorithms, and third-party integrations—blocks the HTTP request thread. Under heavy load, this leads to thread pool starvation, increased latency, and eventual HTTP 503 errors for the client.

## Decision Drivers
* Need to support high throughput (requests per second).
* Need to prevent the API from crashing during traffic spikes.
* Need to decouple the "Order Received" event from the expensive "Order Processing" logic.

## Considered Options
* **Synchronous Processing:** Process everything in the Controller action.
* **External Message Broker:** Use RabbitMQ or Azure Service Bus.
* **In-Memory Channels:** Use `System.Threading.Channels`.

## Decision Outcome
We chose to implement a **Producer-Consumer pattern** using .NET's `System.Threading.Channels`.

1.  **Ingestion:** The API endpoint adds the order to a `BoundedChannel` and immediately returns `202 Accepted`.
2.  **Processing:** A background `HostedService` (`BaconWorker`) reads from the channel and executes the business logic.
3.  **Backpressure:** The channel is configured with `BoundedChannelOptions(100)` and `FullMode = BoundedChannelFullMode.Wait`.

## Pros and Cons of the Options

### In-Memory Channels (Selected)
* **Good:** The API remains highly responsive during traffic spikes.
* **Good:** "Wait" mode prevents OutOfMemory exceptions by naturally slowing down the producer (the API) if the consumer (the Worker) falls too far behind.
* **Good:** No extra infrastructure (Docker containers, cloud services) required for development.
* **Bad:** **Data Durability Risk.** If the application crashes or restarts, orders sitting in the in-memory Channel are lost.
* **Bad:** Error handling complexity; the client has already received a success message before the actual processing fails.

### Synchronous Processing
* **Good:** Simple to reason about; if it fails, the user knows immediately.
* **Bad:** Poor performance under load; highly susceptible to cascading failures.

### External Message Broker
* **Good:** High durability and scalability.
* **Bad:** Significant infrastructure overhead/complexity for the current prototype phase.