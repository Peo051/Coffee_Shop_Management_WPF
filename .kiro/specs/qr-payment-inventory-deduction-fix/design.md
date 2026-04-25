# QR Payment Inventory Deduction Bugfix Design

## Overview

This bugfix implements a deferred inventory deduction pattern for QR Payment transactions. The fix separates invoice creation from inventory/points deduction, ensuring data accuracy when customers do not complete payment. The solution introduces a two-phase approach: (1) WPF creates pending invoices without deducting inventory, and (2) Backend finalizes deductions atomically when webhook confirms PAID status.

## Glossary

- **Bug_Condition (C)**: The condition that triggers the bug - when a QR Payment pending invoice is created and inventory/points are immediately deducted before payment confirmation
- **Property (P)**: The desired behavior - inventory/points should only be deducted when webhook confirms PAID status
- **Preservation**: Existing payment method behaviors (Tiền mặt, Chuyển khoản, Thẻ, Ví) that must continue to deduct inventory immediately
- **HoaDonBanRepository.CreateAsync**: The method in `CoffeeShop.Wpf/Repositories/HoaDonBanRepository.cs` that creates invoices and manages inventory deduction
- **PaymentRepository.FinalizeQrPaymentAsync**: The method in `CoffeeShop.PaymentApi/Repositories/PaymentRepository.cs` that performs deferred inventory deduction when payment is confirmed
- **skipInventoryDeduction**: Boolean parameter that controls whether inventory/points deduction is skipped during invoice creation
- **isQrPendingPayment**: Boolean flag indicating whether the invoice is a pending QR payment (used to determine skipInventoryDeduction value)
- **TrangThaiThanhToan**: Invoice payment status field ("Chờ thanh toán" for pending, "Đã thanh toán" for paid)
- **PaymentStatus**: Provider payment status field ("PENDING", "PAID", "CANCELLED", "EXPIRED")
- **Mon.TonKho**: Product inventory (finished goods) in units/servings
- **NguyenLieu.TonKho**: Raw ingredient inventory in kg/liters
- **DiemTichLuy**: Customer loyalty points balance
- **LichSuTonKho**: Product inventory history records
- **LichSuNguyenLieu**: Ingredient inventory history records
- **Two-Tier Inventory Deduction**: System manages both finished product inventory (Mon.TonKho) and raw ingredient inventory (NguyenLieu.TonKho) simultaneously

## Bug Details

### Bug Condition

The bug manifests when a QR Payment pending invoice is created in the WPF application. The `HoaDonBanRepository.CreateAsync` method immediately deducts Mon.TonKho (product inventory), NguyenLieu.TonKho (ingredient inventory based on recipe formulas), and customer loyalty points (DiemTichLuy), even though the payment has not been confirmed. This causes data inconsistencies when customers do not complete payment, as inventory and points are already deducted but no actual transaction occurred.

**Formal Specification:**
```
FUNCTION isBugCondition(input)
  INPUT: input of type InvoiceCreationRequest
  OUTPUT: boolean
  
  RETURN input.HinhThucThanhToan == "QR Payment"
         AND input.TrangThaiThanhToan == "Chờ thanh toán"
         AND inventoryDeductedImmediately == true
         AND pointsDeductedImmediately == true
END FUNCTION
```

### Examples

- **Example 1**: Create QR pending invoice for 2 coffees (MonId=1, SoLuong=2) with customer using 10 points → Mon.TonKho immediately reduced by 2, NguyenLieu.TonKho reduced by (DinhLuong × 2), DiemTichLuy reduced by 10 → Customer does not pay → Inventory and points incorrectly deducted
- **Example 2**: Create QR pending invoice for 5 teas (MonId=3, SoLuong=5) → Mon.TonKho immediately reduced by 5, ingredient inventory reduced → QR expires after 10 minutes → Inventory remains incorrectly deducted, requires manual rollback
- **Example 3**: Create QR pending invoice for 3 smoothies with customer earning 15 points → Inventory deducted, points added immediately → Customer cancels payment → Data inconsistency requires manual correction
- **Edge case**: Create QR pending invoice, inventory deducted → Another order depletes remaining inventory → Webhook PAID arrives → Finalization should fail gracefully with insufficient inventory error

## Expected Behavior

### Preservation Requirements

**Unchanged Behaviors:**
- Regular payment methods (Tiền mặt, Chuyển khoản, Thẻ, Ví) must continue to deduct Mon.TonKho immediately upon invoice creation
- Regular payment methods must continue to deduct NguyenLieu.TonKho immediately upon invoice creation
- Regular payment methods must continue to update customer loyalty points immediately upon invoice creation
- Regular payment methods must continue to create LichSuTonKho and LichSuNguyenLieu records immediately
- Regular payment methods must continue to set TrangThaiThanhToan to "Đã thanh toán" immediately
- Payment status query functionality must continue to return accurate status information from the database
- QR payment cancellation must continue to update PaymentStatus to "CANCELLED" without requiring inventory rollback
- QR payment expiration must continue to be treated as EXPIRED status without requiring inventory rollback

**Scope:**
All inputs that do NOT involve QR Payment pending invoices (isQrPendingPayment = false) should be completely unaffected by this fix. This includes:
- Cash payments (Tiền mặt)
- Bank transfer payments (Chuyển khoản)
- Card payments (Thẻ)
- E-wallet payments (Ví)
- Any other payment methods that require immediate inventory deduction

## Hypothesized Root Cause

Based on the bug description and code analysis, the root cause is:

1. **Unconditional Inventory Deduction**: The `HoaDonBanRepository.CreateAsync` method always executes inventory deduction logic regardless of payment method or payment status. There is no conditional logic to skip deduction for pending QR payments.

2. **Missing Deferred Deduction Mechanism**: The system lacks a backend mechanism to perform inventory/points deduction when payment is confirmed. The webhook handler only updates payment status without finalizing inventory operations.

3. **No skipInventoryDeduction Parameter**: The repository interface and implementation do not provide a way to control whether inventory deduction should be skipped, making it impossible to defer deduction for QR payments.

4. **Immediate Points Update**: Customer loyalty points (DiemTichLuy) are updated immediately during invoice creation without considering payment confirmation status.

## Correctness Properties

Property 1: Bug Condition - Deferred Inventory Deduction for QR Pending

_For any_ invoice creation request where the payment method is "QR Payment" and the status is "Chờ thanh toán" (isQrPendingPayment = true), the fixed CreateAsync method SHALL skip all inventory deduction operations (Mon.TonKho, NguyenLieu.TonKho) and loyalty points updates (DiemTichLuy), creating the invoice without modifying inventory or points data.

**Validates: Requirements 2.1, 2.2, 2.3, 2.4, 2.5, 2.10**

Property 2: Preservation - Immediate Deduction for Regular Payments

_For any_ invoice creation request where the payment method is NOT "QR Payment" or the status is NOT "Chờ thanh toán" (isQrPendingPayment = false), the fixed CreateAsync method SHALL produce exactly the same behavior as the original method, immediately deducting Mon.TonKho, NguyenLieu.TonKho, and updating DiemTichLuy, preserving all existing functionality for regular payment methods.

**Validates: Requirements 3.1, 3.2, 3.3, 3.4, 3.5**

Property 3: Finalization - Atomic Deduction on PAID Webhook

_For any_ webhook event where the payment status is "PAID" and the invoice is in "Chờ thanh toán" status, the fixed FinalizeQrPaymentAsync method SHALL perform atomic inventory deduction (Mon.TonKho, NguyenLieu.TonKho), loyalty points update (DiemTichLuy), and status update (TrangThaiThanhToan = "Đã thanh toán") within a single transaction, ensuring data consistency.

**Validates: Requirements 2.6, 2.7**

Property 4: Idempotency - No Duplicate Deductions

_For any_ webhook event that is received multiple times for the same payment, the fixed FinalizeQrPaymentAsync method SHALL detect that the payment is already finalized (PaymentStatus = "PAID") and return success without performing duplicate inventory/points deductions, ensuring idempotency.

**Validates: Requirements 2.8**

Property 5: Rollback - Transaction Integrity on Failure

_For any_ finalization attempt where inventory availability check fails (insufficient Mon.TonKho or NguyenLieu.TonKho) or points check fails (insufficient DiemTichLuy), the fixed FinalizeQrPaymentAsync method SHALL rollback the entire transaction and keep the invoice in "Chờ thanh toán" status, maintaining data integrity.

**Validates: Requirements 2.9**

## Fix Implementation

### Changes Required

The fix has been implemented across both WPF and Backend layers:

**File**: `CoffeeShop.Wpf/Repositories/IHoaDonBanRepository.cs`

**Interface**: `IHoaDonBanRepository`

**Specific Changes**:
1. **Add skipInventoryDeduction Parameter**: Added `bool skipInventoryDeduction = false` parameter to `CreateAsync` method signature
   - Default value is `false` to maintain backward compatibility
   - When `true`, the method skips all inventory/points deduction logic

**File**: `CoffeeShop.Wpf/Repositories/HoaDonBanRepository.cs`

**Method**: `CreateAsync`

**Specific Changes**:
1. **Add skipInventoryDeduction Parameter**: Added `bool skipInventoryDeduction = false` parameter to method signature
2. **Conditional Inventory Deduction**: Wrapped all inventory deduction logic in `if (!skipInventoryDeduction)` block:
   - Recipe formula retrieval (`_congThucMonRepository.GetByMonAsync`)
   - Ingredient inventory check (`_nguyenLieuRepository.GetTonKhoAsync`)
   - Product inventory retrieval and deduction (`UPDATE Mon SET TonKho = TonKho - @SoLuongBan`)
   - Product inventory history recording (`_lichSuTonKhoRepository.ThemLichSuAsync`)
   - Ingredient inventory deduction (`_nguyenLieuRepository.TruTonKhoAsync`)
   - Ingredient inventory history recording (`_lichSuNguyenLieuRepository.ThemLichSuAsync`)
3. **Conditional Points Update**: Wrapped loyalty points update in `if (!skipInventoryDeduction && hoaDonBan.KhachHangId.HasValue && ...)` block:
   - Points deduction/addition (`UPDATE KhachHang SET DiemTichLuy = DiemTichLuy - @DiemSuDung + @DiemCong`)
4. **Unconditional Invoice Creation**: Invoice and detail records are always created regardless of `skipInventoryDeduction` value:
   - `INSERT INTO HoaDonBan` always executes
   - `INSERT INTO ChiTietHoaDonBan` always executes
   - `SoThuTuGoiMon` calculation always executes

**File**: `CoffeeShop.Wpf/Services/HoaDonBanService.cs`

**Method**: Service layer method that calls repository

**Specific Changes**:
1. **Pass skipInventoryDeduction Flag**: When calling `CreateAsync`, pass `isQrPendingPayment` as the `skipInventoryDeduction` parameter
   - `isQrPendingPayment = true` when payment method is "QR Payment" and status is "Chờ thanh toán"
   - This triggers the conditional logic in the repository to skip inventory deduction

**File**: `CoffeeShop.PaymentApi/Repositories/IPaymentRepository.cs`

**Interface**: `IPaymentRepository`

**Specific Changes**:
1. **Add FinalizeQrPaymentAsync Method**: Added new method signature:
   ```csharp
   Task<(bool Success, string? ErrorMessage)> FinalizeQrPaymentAsync(
       int hoaDonBanId,
       string maGiaoDich,
       DateTime paymentConfirmedAt,
       CancellationToken cancellationToken = default);
   ```
   - Returns tuple with success flag and optional error message
   - Performs deferred inventory/points deduction when payment is confirmed

**File**: `CoffeeShop.PaymentApi/Repositories/PaymentRepository.cs`

**Method**: `FinalizeQrPaymentAsync` (new method)

**Specific Changes**:
1. **Transaction Management**: Wrap all operations in a SQL transaction with proper error handling
2. **Invoice Validation**: Query invoice with `UPDLOCK, HOLDLOCK` to prevent race conditions, retrieve `TrangThaiThanhToan`, `PaymentStatus`, `CreatedByUserId`, `KhachHangId`, `DiemSuDung`, `DiemCong`
3. **Idempotency Check**: If `PaymentStatus = "PAID"` or `TrangThaiThanhToan = "Đã thanh toán"`, return success immediately without deduction
4. **Status Validation**: Verify invoice is in "Chờ thanh toán" status, return error if not
5. **Retrieve Invoice Details**: Query `ChiTietHoaDonBan` to get list of products and quantities
6. **Product Inventory Check**: For each product, verify `Mon.TonKho >= SoLuong`, return error if insufficient
7. **Ingredient Inventory Check**: For each product, retrieve recipe from `CongThucMon`, calculate required ingredient quantity (`DinhLuong × SoLuong`), verify `NguyenLieu.TonKho >= soLuongCanDung`, return error if insufficient
8. **Product Inventory Deduction**: For each product, get `TonTruoc`, execute `UPDATE Mon SET TonKho = TonKho - @SoLuong WHERE MonId = @MonId AND TonKho >= @SoLuong`, calculate `TonSau`, insert `LichSuTonKho` record with `LoaiPhatSinh = 'BanHang'` and `GhiChu = 'QR Payment finalized - Hóa đơn #...'`
9. **Ingredient Inventory Deduction**: For each ingredient in recipe, get `TonTruoc`, execute `UPDATE NguyenLieu SET TonKho = TonKho - @SoLuong WHERE NguyenLieuId = @NguyenLieuId AND TonKho >= @SoLuong`, calculate `TonSau`, insert `LichSuNguyenLieu` record with `LoaiPhatSinh = 'XuatKho'` and `GhiChu = 'QR Payment finalized cho món ... - HĐ #...'`
10. **Loyalty Points Update**: If `KhachHangId` exists and (`DiemSuDung > 0` or `DiemCong > 0`), execute `UPDATE KhachHang SET DiemTichLuy = DiemTichLuy - @DiemSuDung + @DiemCong WHERE KhachHangId = @KhachHangId AND IsActive = 1 AND DiemTichLuy >= @DiemSuDung`, return error if insufficient points
11. **Invoice Status Update**: Execute `UPDATE HoaDonBan SET PaymentStatus = 'PAID', TrangThaiThanhToan = N'Đã thanh toán', MaGiaoDich = @MaGiaoDich, PaymentConfirmedAt = @PaymentConfirmedAt`
12. **Commit Transaction**: If all steps succeed, commit transaction; if any step fails, rollback and return error message

**File**: `CoffeeShop.PaymentApi/Services/PayOsPaymentGatewayService.cs`

**Method**: `HandleWebhookAsync`

**Specific Changes**:
1. **Replace MarkPaymentPaidAsync Call**: Changed from calling `_paymentRepository.MarkPaymentPaidAsync` to `_paymentRepository.FinalizeQrPaymentAsync`
2. **Handle Finalization Result**: Check `(success, errorMessage)` tuple returned from finalize method
3. **Log Success**: If `success = true`, log "Payment finalized successfully for HoaDonBan {HoaDonBanId}"
4. **Log Failure**: If `success = false`, log error with detailed error message from finalization

## Testing Strategy

### Validation Approach

The testing strategy follows a three-phase approach: first, surface counterexamples that demonstrate the bug on unfixed code (exploratory testing), then verify the fix works correctly (fix checking), and finally ensure existing behavior is preserved (preservation checking).

### Exploratory Bug Condition Checking

**Goal**: Surface counterexamples that demonstrate the bug BEFORE implementing the fix. Confirm the root cause analysis by observing that QR pending invoices immediately deduct inventory/points.

**Test Plan**: Create QR pending invoices in the WPF application and observe database state immediately after creation. Run these tests on the UNFIXED code to observe that inventory/points are incorrectly deducted before payment confirmation.

**Test Cases**:
1. **QR Pending Immediate Deduction Test**: Create QR pending invoice for 2 coffees with customer using 10 points → Observe Mon.TonKho reduced by 2, NguyenLieu.TonKho reduced, DiemTichLuy reduced by 10 (will demonstrate bug on unfixed code)
2. **QR Pending History Creation Test**: Create QR pending invoice → Observe LichSuTonKho and LichSuNguyenLieu records created immediately (will demonstrate bug on unfixed code)
3. **QR Pending No Payment Test**: Create QR pending invoice → Do not send webhook → Observe inventory/points remain incorrectly deducted (will demonstrate bug on unfixed code)
4. **Multiple QR Pending Test**: Create 3 QR pending invoices → Observe inventory deducted 3 times even though no payments confirmed (will demonstrate bug on unfixed code)

**Expected Counterexamples**:
- Mon.TonKho is reduced immediately when QR pending invoice is created
- NguyenLieu.TonKho is reduced immediately based on recipe formulas
- DiemTichLuy is updated immediately (points deducted/added)
- LichSuTonKho and LichSuNguyenLieu records are created immediately
- When customer does not pay, inventory/points remain incorrectly deducted
- Possible root cause: No conditional logic to skip deduction for pending QR payments

### Fix Checking

**Goal**: Verify that for all inputs where the bug condition holds (QR pending invoices), the fixed function produces the expected behavior (no immediate deduction, deferred deduction on webhook).

**Pseudocode:**
```
FOR ALL input WHERE isBugCondition(input) DO
  // Phase 1: Create QR pending invoice
  hoaDonBanId := CreateAsync_fixed(input, skipInventoryDeduction = true)
  
  // Verify no immediate deduction
  ASSERT Mon.TonKho = TonKho_before
  ASSERT NguyenLieu.TonKho = TonKho_before
  ASSERT DiemTichLuy = DiemTichLuy_before
  ASSERT COUNT(LichSuTonKho WHERE HoaDonBanId = hoaDonBanId) = 0
  ASSERT COUNT(LichSuNguyenLieu WHERE HoaDonBanId = hoaDonBanId) = 0
  
  // Phase 2: Send PAID webhook
  (success, errorMessage) := FinalizeQrPaymentAsync(hoaDonBanId, maGiaoDich, now)
  
  // Verify deferred deduction
  ASSERT success = true
  ASSERT Mon.TonKho = TonKho_before - SoLuong
  ASSERT NguyenLieu.TonKho = TonKho_before - (DinhLuong × SoLuong)
  ASSERT DiemTichLuy = DiemTichLuy_before - DiemSuDung + DiemCong
  ASSERT COUNT(LichSuTonKho WHERE HoaDonBanId = hoaDonBanId) > 0
  ASSERT COUNT(LichSuNguyenLieu WHERE HoaDonBanId = hoaDonBanId) > 0
  ASSERT TrangThaiThanhToan = "Đã thanh toán"
  ASSERT PaymentStatus = "PAID"
END FOR
```

**Test Cases**:
1. **QR Pending No Immediate Deduction**: Create QR pending invoice → Verify Mon.TonKho unchanged, NguyenLieu.TonKho unchanged, DiemTichLuy unchanged, no history records
2. **Webhook PAID Deferred Deduction**: After creating QR pending invoice, send PAID webhook → Verify Mon.TonKho deducted, NguyenLieu.TonKho deducted, DiemTichLuy updated, history records created, status updated to "Đã thanh toán"
3. **Webhook Idempotency**: Send PAID webhook multiple times → Verify inventory/points only deducted once, subsequent webhooks return success without duplicate deduction
4. **Webhook Insufficient Inventory**: Create QR pending invoice, manually reduce inventory, send PAID webhook → Verify finalization fails, transaction rolled back, invoice remains "Chờ thanh toán"

### Preservation Checking

**Goal**: Verify that for all inputs where the bug condition does NOT hold (regular payment methods), the fixed function produces the same result as the original function.

**Pseudocode:**
```
FOR ALL input WHERE NOT isBugCondition(input) DO
  ASSERT CreateAsync_original(input) = CreateAsync_fixed(input, skipInventoryDeduction = false)
  
  // Verify immediate deduction still occurs
  ASSERT Mon.TonKho = TonKho_before - SoLuong
  ASSERT NguyenLieu.TonKho = TonKho_before - (DinhLuong × SoLuong)
  ASSERT DiemTichLuy = DiemTichLuy_before - DiemSuDung + DiemCong
  ASSERT COUNT(LichSuTonKho WHERE HoaDonBanId = hoaDonBanId) > 0
  ASSERT COUNT(LichSuNguyenLieu WHERE HoaDonBanId = hoaDonBanId) > 0
  ASSERT TrangThaiThanhToan = "Đã thanh toán"
END FOR
```

**Testing Approach**: Property-based testing is recommended for preservation checking because:
- It generates many test cases automatically across the input domain (different payment methods, products, quantities, customers)
- It catches edge cases that manual unit tests might miss (e.g., zero quantity, null customer, multiple products)
- It provides strong guarantees that behavior is unchanged for all non-buggy inputs

**Test Plan**: Observe behavior on UNFIXED code first for regular payment methods (Tiền mặt, Chuyển khoản, Thẻ, Ví), then write property-based tests capturing that behavior. Run tests on FIXED code to verify preservation.

**Test Cases**:
1. **Cash Payment Immediate Deduction**: Create invoice with payment method "Tiền mặt" → Verify Mon.TonKho deducted immediately, NguyenLieu.TonKho deducted immediately, DiemTichLuy updated immediately, history records created immediately, status = "Đã thanh toán"
2. **Bank Transfer Immediate Deduction**: Create invoice with payment method "Chuyển khoản" → Verify immediate deduction behavior identical to unfixed code
3. **Card Payment Immediate Deduction**: Create invoice with payment method "Thẻ" → Verify immediate deduction behavior identical to unfixed code
4. **E-Wallet Immediate Deduction**: Create invoice with payment method "Ví" → Verify immediate deduction behavior identical to unfixed code
5. **Multiple Products Immediate Deduction**: Create invoice with 3 different products using cash payment → Verify all products' inventory deducted immediately
6. **Customer Points Immediate Update**: Create invoice with customer using points and earning points, payment method "Tiền mặt" → Verify points updated immediately

### Unit Tests

- Test `CreateAsync` with `skipInventoryDeduction = true` → Verify no inventory/points deduction
- Test `CreateAsync` with `skipInventoryDeduction = false` → Verify immediate inventory/points deduction
- Test `FinalizeQrPaymentAsync` with valid invoice → Verify successful deduction and status update
- Test `FinalizeQrPaymentAsync` with already paid invoice → Verify idempotency (no duplicate deduction)
- Test `FinalizeQrPaymentAsync` with insufficient inventory → Verify rollback and error message
- Test `FinalizeQrPaymentAsync` with insufficient points → Verify rollback and error message
- Test `FinalizeQrPaymentAsync` with invalid status → Verify error message
- Test webhook handler calling finalize method → Verify correct parameter passing and result handling

### Property-Based Tests

- Generate random QR pending invoices (varying products, quantities, customers, points) → Verify no immediate deduction for all cases
- Generate random PAID webhooks for pending invoices → Verify successful finalization for all valid cases
- Generate random regular payment invoices (varying payment methods, products, quantities) → Verify immediate deduction preserved for all cases
- Generate random duplicate webhook scenarios → Verify idempotency holds for all cases
- Generate random insufficient inventory scenarios → Verify rollback behavior for all cases

### Integration Tests

- Test full QR payment flow: Create pending invoice in WPF → Verify no deduction → Send PAID webhook to backend → Verify deduction and status update → Query status from WPF → Verify "Đã thanh toán"
- Test QR payment cancellation flow: Create pending invoice → Cancel payment → Verify status = "CANCELLED", inventory unchanged
- Test QR payment expiration flow: Create pending invoice → Wait for expiration → Verify status = "EXPIRED", inventory unchanged
- Test concurrent webhook handling: Create pending invoice → Send multiple PAID webhooks simultaneously → Verify only one finalization occurs
- Test mixed payment methods: Create 5 invoices (2 cash, 2 QR pending, 1 card) → Verify cash and card deduct immediately, QR pending do not → Send PAID webhooks for QR → Verify all invoices finalized correctly
