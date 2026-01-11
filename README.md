# 🥓 FreshStream (The Baconator)
**High-Concurrency Perishable Inventory Engine**

> **Concept:** A .NET 9 Web API designed to handle high-velocity order ingestion for perishable goods (Pork) using a **FEFO (First Expired, First Out)** allocation strategy.

## 🎯 The Problem
In food processing, standard inventory algorithms (FIFO/LIFO) could arguably result in spoilage (and therefore revenue loss). Additionally, during high-volume sales events, synchronous API processing can cause race conditions where inventory is oversold (two orders coming in at exactly the same time for limited inventory could create an oversell, e.g. two orders for 50lbs of inventory, when **only** 50lbs remain).

## 💡 The Solution
**Baconator** is an asynchronous backend service that decouples **Order Ingestion** from **Inventory Processing**.
* **Zero-Blocking:** The API accepts orders instantly (`202 Accepted`) and offloads processing to a background worker.
* **Spoilage Reduction:** Implements a **FEFO algorithm** to automatically allocate the oldest valid inventory first.
* **Thread Safety:** Uses rigorous locking mechanisms to prevent race conditions during concurrent high-volume access.

---

## 🏗 Architecture

The system utilizes a **Producer/Consumer** pattern using `System.Threading.Channels` to handle backpressure and ensure API stability.

```mermaid
graph LR
    A[Client] -- POST Order --> B(API Endpoint)
    B -- Write Async --> C{Bounded Channel}
    C -- Read Async --> D[Background Worker]
    D -- Lock & Allocate --> E[(In-Memory MeatLocker)]
    E -- Update State --> F[Inventory Batch]