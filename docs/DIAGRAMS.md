# Diagrams

These are planned designs. GitHub renders the Mermaid blocks. The repository tree in ARCHITECTURE is the folder diagram.

## 1. Business context

```mermaid
flowchart LR
    Customer["Business customer"] -->|Pays invoice| Provider["Payment provider simulator"]
    Provider -->|Net payout| Bank["Tenant bank - statement fixtures"]
    Finance["Tenant finance team"] -->|Manages receivables| App["Reclevia"]
    Provider -->|Events and settlement reports| App
    Bank -->|Imported statement| App
    Tenant["Tenant organization"] -->|Software subscription| Platform["Reclevia SaaS billing"]
    Platform -->|Controls tenant entitlements| App
```

## 2. Application and dependencies

```mermaid
flowchart TB
    Browser["Finance browser / Scalar"] --> Host["ASP.NET Core host: cookies, policies, endpoints, pages"]
    subgraph Process["One deployable application"]
        Host --> Cases["Feature handlers"]
        Cases --> Core["Core business rules"]
        Cases --> EF["EF Core persistence"]
        Worker["BackgroundService"] --> Cases
        Worker --> Adapter["Provider adapter"]
    end
    EF --> PG[("Windows PostgreSQL: business, ledger, audit, inbox, outbox")]
    Worker --> PG
    Adapter --> Sim["Local provider simulator - separate logical boundary"]
    Sim -->|Signed webhook| Host
```

## 3. Core data relationships

Conceptual model: routine columns and support tables are omitted. Every tenant-owned relationship includes TenantId. Allocations retain release history for credits; journal sources link to their originating records.

```mermaid
erDiagram
    USER ||--o{ MEMBERSHIP : joins
    TENANT ||--o{ MEMBERSHIP : grants
    TENANT ||--o{ CUSTOMER : serves
    CUSTOMER ||--o{ INVOICE : receives
    INVOICE ||--|{ INVOICE_LINE : contains
    INVOICE ||--o{ CREDIT_NOTE : reduced_by
    INVOICE ||--o{ PAYMENT_ATTEMPT : targeted_by
    PAYMENT_ATTEMPT ||--o{ ALLOCATION : " "
    INVOICE ||--o{ ALLOCATION : paid_by
    PAYMENT_ATTEMPT ||--o{ REFUND : " "
    CREDIT_NOTE o|--o{ REFUND : authorizes
    TENANT ||--o{ JOURNAL_ENTRY : owns
    JOURNAL_ENTRY ||--|{ POSTING : contains
    ACCOUNT ||--o{ POSTING : receives
    TENANT ||--o{ ACCOUNT : owns
    TENANT ||--o{ PROVIDER_ACCOUNT : maps
    PROVIDER_ACCOUNT ||--o{ SETTLEMENT_BATCH : reports
    SETTLEMENT_BATCH ||--o{ SETTLEMENT_LINE : groups
    TENANT ||--o{ BANK_STATEMENT : imports
    BANK_STATEMENT ||--o{ BANK_LINE : contains
    SETTLEMENT_BATCH ||--o{ RECONCILIATION_CASE : investigated_by
    BANK_LINE o|--o{ RECONCILIATION_CASE : evidence_for
    TENANT ||--o| SUBSCRIPTION : subscribes
    PLAN ||--o{ SUBSCRIPTION : selected_by
    SUBSCRIPTION ||--o{ BILLING_CYCLE : renews
```

## 4. Invoice state and calculated views

```mermaid
stateDiagram-v2
    [*] --> Draft
    Draft --> Issued: Validate, number, freeze, post receivable
    Draft --> Deleted: Remove unissued draft
    Issued --> Issued: Record allocations or credit notes
    Deleted --> [*]
```

Issued is the document state. **Open, partially paid, paid, credited, and overdue are calculated views** of its amounts and dates, not a second mutable state machine. A fully paid invoice stays issued after a refund; the credit note and refund explain the correction.

## 5. Payment attempt state

```mermaid
stateDiagram-v2
    [*] --> Created
    Created --> Pending: Commit and dispatch operation
    Pending --> Succeeded: Verified final success
    Pending --> Failed: Verified final failure
    Succeeded --> [*]
    Failed --> [*]
```

Timeouts and unresolved provider status leave an attempt Pending. A new attempt after confirmed failure is a new operation. Refunds have their own Pending/Succeeded/Failed records; they never rewrite a successful payment. Contradictory events require provider lookup or an exception, not blind state changes.

## 6. Payment execution and crash recovery

```mermaid
sequenceDiagram
    actor Finance
    participant API
    participant DB as PostgreSQL
    participant Worker
    participant Provider as Simulator
    Finance->>API: Request payment + idempotency key
    API->>DB: Commit attempt, request result, audit, outbox
    API-->>Finance: 202 Accepted + attempt ID
    Worker->>DB: Lease outbox job
    Worker->>Provider: Submit with stable operation ID
    Provider-->>Worker: Accepted or uncertain outcome
    Provider->>API: Signed confirmation event
    API->>DB: Commit unique inbox event
    API-->>Provider: Acknowledge durable receipt
    Worker->>DB: Lease event, lock invoice, apply confirmation
    Note over Worker,DB: Atomic commit: payment, allocation, journal<br/>audit, outbox, inbox completion
    Finance->>API: Read payment and invoice
    API-->>Finance: Confirmed amount and remaining debt
    Note over Worker,Provider: On crash, retry the same operation ID<br/>or query its status
```

## 7. Reconciliation decisions

```mermaid
flowchart TD
    Import["Import provider batch and bank statement"] --> Unique{"Already imported?"}
    Unique -->|Yes| Existing["Return existing import"]
    Unique -->|No| Transactions["Match provider lines to payments, refunds, and fees"]
    Transactions --> Match{"References, currency, amounts agree?"}
    Match -->|No| Exception["Open exception with evidence"]
    Match -->|Yes| Net["Calculate expected net payout"]
    Net --> Bank{"Matching bank receipt or debit?"}
    Bank -->|Missing or ambiguous| Exception
    Bank -->|Yes| Commit["Post bank journal once and mark reconciled"]
    Exception --> Review["Finance investigates and records resolution"]
    Review --> Transactions
```

## 8. SaaS lifecycle

```mermaid
stateDiagram-v2
    [*] --> Trialing
    Trialing --> Active: Initial charge confirmed
    Trialing --> Suspended: Trial expires unpaid
    Active --> PastDue: Renewal fails
    PastDue --> Active: Recovery confirmed
    PastDue --> Suspended: Grace period ends
    Suspended --> Active: Recovery confirmed
    Active --> Canceled: Scheduled cancellation reaches period end
    PastDue --> Canceled: Scheduled cancellation reaches period end
    Suspended --> Canceled: Owner cancels
    Trialing --> Canceled: Owner cancels
```

Successful renewal keeps the subscription Active. Before a scheduled cancellation takes effect, current access remains. Canceled tenants may start a new billing period through an explicit reactivation flow. Collection and reconciliation of existing financial obligations continue in every subscription state.
