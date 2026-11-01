# Quyết định 009: Mọi đèn chỉ báo R2 của dialog WMS đều tắt

- **Backlog:** #10 (và #20)
- **Trạng thái:** accepted (2026-11-01)
- **Ngày:** 2026-11-01
- **Thay thế:** 008 mục 4 (phần chỉ báo). Các mục khác của 008 giữ nguyên.
- **Ý kiến đã xét:** người dùng (không chạy agent chuyên môn)
- **Lựa chọn của người dùng (2026-11-01):** "Tất cả tắt/xám": giữ bố cục R2, không đèn nào bật, kể cả `letzter Pick`; bỏ quy tắc chọn đèn cân.

## Bài toán
Decision 008 mục 4 bật một đèn cân (`Referenzwaage` hoặc `Behälterwaage`, theo Gew. giả và ngưỡng 0,7 kg) và `letzter Pick` màu đỏ. Người dùng xác nhận các đèn R2 không dùng trong kịch bản XR training: không bài tập nào yêu cầu cân hay đổi cách đếm theo đèn. Đèn bật mà không có ý nghĩa trong bài thì kéo sự chú ý của trainee một cách vô ích.

## Quyết định
- Giữ đủ 12 chữ của vùng R2 `Hinweise / Infos` để dialog vẫn giống màn hình thật.
- Mọi đèn đều tắt: không nền, chữ xám. Trạng thái này nằm sẵn trong prefab `WMS_Kommissionierung`, `WmsDialogView` không đổi nó.
- Bỏ quy tắc `CountingMethod` (≤ 7 ST Zählen, ≥ 0,7 kg Behälterwaage) và test của nó khỏi `OrderLine`. `weightKg` vẫn giữ vì ô `Gew.` vẫn hiện.
- #20: không dạy nghĩa từng đèn; chỉ nói ngắn rằng vùng này không dùng trong bài tập.

## Được / mất
- Được: bớt chi tiết gây chú ý không cần thiết; code ít hơn; không phụ thuộc vào ngưỡng 0,7 kg chưa được xác nhận.
- Mất: dialog lệch một chút so với ảnh thật (trên ảnh có đèn bật). Nếu sau này có bài cân, phải thêm lại logic đèn (mở decision mới).
