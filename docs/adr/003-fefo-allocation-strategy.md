# ADR 003: FEFO (First Expired, First Out) Allocation Strategy

**Status:** Accepted
**Date:** 2026-01-29
**Deciders:** Ramon Reyes
**Technical Story:** N/A

## Context and Problem Statement
The inventory consists of perishable goods (Pork). Standard FIFO (First In, First Out) or LIFO (Last In, First Out) strategies do not account for expiration dates. Using FIFO could result in newer stock being sold while older stock sits in the back and eventually spoils, causing revenue loss.

## Decision Drivers
* Need to minimize product waste/spoilage.
* Need to automate stock rotation logic.

## Decision Outcome
The `MeatLocker` will implement logic to sort valid batches by `ExpirationDate` (ascending) before allocating stock.

* **Logic:** `_inventory.Where(valid).OrderBy(ExpirationDate)`

## Consequences
* **Positive:** Minimizes waste and spoilage costs by ensuring the oldest viable product is always sold first.
* **Negative:** Slight performance cost. Sorting a list is $O(N \log N)$, whereas popping from a FIFO queue is $O(1)$.
    * *Mitigation:* Given the expected batch count is not in the millions, this CPU cost is negligible compared to the cost of wasted product.