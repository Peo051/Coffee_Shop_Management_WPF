# Bugfix Requirements Document

## Introduction

The QR Payment feature incorrectly deducts inventory (Mon.TonKho, NguyenLieu.TonKho) and customer loyalty points immediately when creating a pending QR payment invoice, before payment is confirmed. This causes data inconsistencies when customers do not complete payment, as inventory and points are already deducted but no actual transaction occurred.

The fix implements deferred inventory deduction: pending QR invoices skip inventory/points deduction at creation time, and only finalize these deductions when the webhook confirms PAID status. This ensures data accuracy and eliminates the need for manual rollback operations.

## Bug Analysis

### Current Behavior (Defect)

1.1 WHEN a QR Payment pending invoice is created THEN the system immediately deducts Mon.TonKho (product inventory)

1.2 WHEN a QR Payment pending invoice is created THEN the system immediately deducts NguyenLieu.TonKho (ingredient inventory) based on recipe formulas

1.3 WHEN a QR Payment pending invoice is created THEN the system immediately deducts customer loyalty points (DiemTichLuy)

1.4 WHEN a QR Payment pending invoice is created THEN the system immediately creates LichSuTonKho (inventory history) records

1.5 WHEN a QR Payment pending invoice is created THEN the system immediately creates LichSuNguyenLieu (ingredient history) records

1.6 WHEN a customer does not complete QR payment THEN inventory and points remain incorrectly deducted, causing data inconsistency

### Expected Behavior (Correct)

2.1 WHEN a QR Payment pending invoice is created THEN the system SHALL NOT deduct Mon.TonKho until payment is confirmed

2.2 WHEN a QR Payment pending invoice is created THEN the system SHALL NOT deduct NguyenLieu.TonKho until payment is confirmed

2.3 WHEN a QR Payment pending invoice is created THEN the system SHALL NOT deduct customer loyalty points until payment is confirmed

2.4 WHEN a QR Payment pending invoice is created THEN the system SHALL NOT create LichSuTonKho records until payment is confirmed

2.5 WHEN a QR Payment pending invoice is created THEN the system SHALL NOT create LichSuNguyenLieu records until payment is confirmed

2.6 WHEN webhook confirms PAID status for a QR payment THEN the system SHALL finalize the transaction by deducting Mon.TonKho, NguyenLieu.TonKho, and customer loyalty points in a single atomic transaction

2.7 WHEN webhook confirms PAID status for a QR payment THEN the system SHALL create LichSuTonKho and LichSuNguyenLieu records with "QR Payment finalized" notation

2.8 WHEN webhook is received multiple times for the same payment THEN the system SHALL apply idempotency to prevent duplicate inventory/points deductions

2.9 WHEN finalization fails due to insufficient inventory or points THEN the system SHALL rollback the entire transaction and keep the invoice in "Chờ thanh toán" status

2.10 WHEN a customer does not complete QR payment THEN inventory and points SHALL remain unchanged, maintaining data accuracy

### Unchanged Behavior (Regression Prevention)

3.1 WHEN creating an invoice with regular payment methods (Tiền mặt, Chuyển khoản, Thẻ, Ví) THEN the system SHALL CONTINUE TO deduct Mon.TonKho immediately

3.2 WHEN creating an invoice with regular payment methods THEN the system SHALL CONTINUE TO deduct NguyenLieu.TonKho immediately

3.3 WHEN creating an invoice with regular payment methods THEN the system SHALL CONTINUE TO deduct customer loyalty points immediately

3.4 WHEN creating an invoice with regular payment methods THEN the system SHALL CONTINUE TO create LichSuTonKho and LichSuNguyenLieu records immediately

3.5 WHEN creating an invoice with regular payment methods THEN the system SHALL CONTINUE TO set TrangThaiThanhToan to "Đã thanh toán" immediately

3.6 WHEN querying payment status THEN the system SHALL CONTINUE TO return accurate status information from the database

3.7 WHEN cancelling a QR payment THEN the system SHALL CONTINUE TO update PaymentStatus to "CANCELLED" without requiring inventory rollback

3.8 WHEN a QR payment expires THEN the system SHALL CONTINUE TO treat it as EXPIRED status without requiring inventory rollback
