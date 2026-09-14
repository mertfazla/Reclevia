# Domain Guide

## 1. Who pays whom?

**Reclevia sells software to businesses (B2B).** Each subscribing business is a tenant. A tenant sells services to its own business customers and uses Reclevia to follow the resulting receivables.

Example: **Northstar Consulting** pays Reclevia for software. **Harbor Manufacturing** owes Northstar EUR 1,000 for completed consulting work. A payment provider processes Harbor's payment; Northstar's bank receives the eventual payout.

These are two separate commercial relationships: **tenant -> Reclevia subscription** and **customer -> tenant invoice**. Subscription charges never enter a tenant's customer-payment ledger.

## 2. Vocabulary through the workflow

| Term | Plain meaning |
| --- | --- |
| Tenant / membership | A business using our app / a user's permission to work in that business. |
| Receivable / invoice | Money owed to the tenant / the document requesting payment. |
| Payment terms | When payment is due; “Net 30” means 30 days after issue. |
| Payment attempt | A request to collect money; it can fail or remain uncertain. |
| Collection / allocation | Confirmed incoming payment / the portion assigned to an invoice. |
| Provider / webhook | External payment processor / its HTTP notification about an event. |
| Clearing account | Records money due from the provider before it reaches the bank. |
| Fee / payout | Provider charge / transfer from the provider toward the tenant's bank. |
| Settlement batch | A provider report grouping payments, refunds, fees, and a payout. |
| Reconciliation | Match our records, the provider report, and the bank statement. |
| Credit note / refund | Reduction of an invoice's value / actual return of collected money. |
| Subledger / journal | Detailed financial records for one area / one balanced business entry. |
| Idempotency | Retrying one operation produces one business effect. |
| Entitlement / dunning | Feature access from a plan / recovering failed subscription renewals. |

## 3. Follow EUR 1,000

Our subledger uses the **tenant's perspective**. Services are already delivered; this example excludes tax and deferred revenue. “Debit” and “credit” are accounting sides, not synonyms for bad/good or outgoing/incoming. Assets increase by debit; income and liabilities increase by credit.

| Event | Debit | Credit | Business meaning |
| --- | --- | --- | --- |
| Issue invoice | Receivables 1,000 | Sales 1,000 | Customer owes us 1,000. |
| Confirm payment | Provider clearing 1,000 | Receivables 1,000 | Customer debt is paid; provider owes us. |
| Recognize provider fee | Processing expense 20 | Provider clearing 20 | Provider retains 20. |
| Confirm bank receipt | Bank 980 | Provider clearing 980 | Bank evidence confirms receipt. |

Every journal has equal total debits and credits. Final receivable and clearing balances are zero; bank increased by 980. The invoice is paid before the bank receipt exists. Provider payout status alone is not bank evidence. This distinction follows real [payout reconciliation](https://docs.stripe.com/payouts/reconciliation) workflows.

## 4. Version 1 business rules

1. **Tenant isolation:** every business record belongs to a tenant; membership authorizes access. Related records must share TenantId. Provider account mapping, not an untrusted webhook field, identifies the tenant.
2. **Money:** EUR only; store integer cents in C# `long` / PostgreSQL `bigint`. EUR 10.50 = 1050. Use checked arithmetic; quantities are positive integers. No floating-point money or cross-currency totals.
3. **Invoices:** drafts are editable; issuing freezes customer/line snapshots, amount, issue date, and tenant-unique number. A draft can be deleted; an issued invoice is corrected with a credit note. Cumulative credits cannot exceed the original invoice total. Never recycle an issued number.
4. **Outstanding:** `invoice total - credit notes - allocated confirmed payments`. Credits first reduce outstanding; any credit against already-paid value releases that part of the allocation into a refund liability. Therefore outstanding never becomes negative. Overdue is a derived condition: outstanding > 0 and due date has passed in the tenant's time zone.
5. **Payments:** a payment attempt targets one invoice; an invoice may have many attempts and partial payments. Requests cannot intentionally exceed current outstanding. Unexpected confirmed excess is still real money: post it to Unapplied customer funds, open an exception, and return it through a controlled refund. Never silently discard it.
6. **External truth:** browser redirects and request timeouts do not prove payment success or failure. Use verified provider evidence/status lookup; keep an unresolved attempt pending and block blind recharging.
7. **Refunds:** a credit note authorizes an invoice-related refund; an excess-payment refund uses its unapplied liability. Lock the payment and include pending reservations when checking `successful + reserved refunds <= captured amount`. A failed refund releases its reservation but leaves the liability owed.
8. **Ledger:** journal entries are immutable, balanced per tenant/currency, and unique per source event. Corrections append linked reversing entries. Audit logs explain actor/action; they cannot replace the financial journal.
9. **Reconciliation:** match provider transactions first, then the batch's expected net against the bank statement. Compare stable references, currency, and cents. Preserve discrepancies as exceptions; a manual adjustment needs a reason, evidence, authorization, and a journal when it changes money.

## 5. Credits and refunds without rewriting history

For an unpaid EUR 100 credit: debit Sales returns, credit Receivables. For a paid EUR 100 credit: debit Sales returns, credit Customer refunds payable; release EUR 100 of the payment allocation. Split a partially paid credit between those two cases.

On confirmed refund: debit the relevant customer liability, credit Provider clearing. A refund after payout can make clearing negative: the tenant now owes the provider. Keep that debt visible; a later batch offsets it, or a bank debit settles it (debit Clearing, credit Bank). Refund failure creates no cash journal. Do not assume the original provider fee is returned; recognize any returned fee from separate provider evidence.

## 6. SaaS access and responsibility

Owner has Finance permissions and manages membership/subscription; Finance operates invoices/payments/refunds/reconciliation; Viewer reads and exports. A user may belong to multiple tenants with different roles.

Subscription policy: one monthly plan subscription per tenant; trial -> active -> past due -> suspended, with cancellation effective at period end. Use a 14-day trial and a 7-day renewal grace period as **project choices**. No proration or usage billing in v1. Enforce plan limits atomically on the server. Reconcile delayed subscription events against the latest provider state; never downgrade access from an older notification. See [subscription lifecycle](https://docs.stripe.com/billing/subscriptions/overview).

Suspension prevents new commercial writes. Existing collections, refunds, reconciliation, security changes, subscription recovery, and read/export remain available to authorized users.

## 7. Deferred domain areas

**KYB/KYC** verifies businesses/people; **AML** concerns detecting and managing money-laundering risk; **PCI DSS** concerns payment-card data security; **chargebacks** are disputed payments that can reverse earlier funds. We avoid card data and identity documents entirely. Real-money expansion requires a specific jurisdiction and provider, operational controls, and a fresh assessment of these areas. These are boundaries, not claims of compliance.
