# Quyết định 035: Dấu ✓ trên thẻ bước ở Basic, Mixed, Complex và dòng position trên giá

- **Backlog:** #97
- **Trạng thái:** accepted
- **Ngày:** 2026-11-05
- **Bổ sung cho:** 017 (khung xanh bước hiện tại giữ nguyên), 027 (✓ không bao giờ ở thẻ mục tiêu „Darum geht es“), 033 (4 s chờ + thùng chạy).
- **Ý kiến đã xét:** xr-ux-reviewer (proposal mode). Không chạy đủ `solution-decision`: người dùng chọn qua mockup trong chat.
- **Lựa chọn của người dùng (2026-11-05):** phương án C cho thẻ 2; thêm dòng position (cách 2) theo dạng gọn mà xr-ux-reviewer đề xuất.

## Bài toán
Welcome (#39) và Concept (#79) có ✓ trên thẻ việc đã xong, Basic/Mixed/Complex chỉ có khung xanh bước hiện tại (#40). ✓ là scaffolding: người học nhìn giá là biết việc nào xong, việc nào còn lại. Ở Mixed/Complex thẻ đổi chữ theo position, nên ✓ phải tắt khi sang position 2 (người dùng chốt 2026-11-04); khi đó giá trông như lúc mới vào bài, người học không biết position 1 đã xong.

## Khi nào thẻ 2 có ✓
| | Ưu | Nhược |
|---|---|---|
| A. Khi Ist = Soll | ✓ không báo sai | lộ đáp án: bỏ thêm tới khi ✓ hiện; tỉ lệ đúng lần đầu (Q1) cao giả |
| B. Khi trong thùng có kiện | ✓ hiện sớm | báo „xong“ khi còn thiếu; trái với thẻ kết quả „Nicht richtig“; bật/tắt liên tục |
| **C. Cùng thẻ 3, khi nút đỏ xác nhận đúng (chọn)** | không báo sai, không lộ đáp án; „xong“ = đã xác nhận | ✓ 2 và 3 hiện cùng lúc; khung xanh (#40) đã cho biết đang ở bước nào |

## Quy tắc
1. Thẻ 1 có ✓ khi người học xong bước đọc: cầm kiện đầu tiên của position (như khung xanh chuyển sang thẻ 2). Complex position 1: thẻ 1 là „trả lời câu hỏi“, nên ✓ (và khung sang thẻ 2) khi cả hai câu đã đúng.
2. Thẻ 2 và thẻ 3 có ✓ cùng lúc khi nút đỏ xác nhận position đúng. Bấm sai: không ✓ nào đổi.
3. Sau xác nhận đúng, cả 3 thẻ có ✓, không khung, không làm mờ, trong suốt 4 s chờ và lúc thùng chạy: đó là tín hiệu „xong position“.
4. Position không phải cuối: khi thùng kho mới đã dừng (`IStationStock.Settled`), mọi thứ đổi một lần như lật trang: ✓ tắt, chữ thẻ (`PositionTexts`) đổi, khung về thẻ 1, LVS đổi, dòng position đổi. Trước đây `PositionTexts` đổi chữ ngay lúc bấm (sửa trong #97).
5. Position cuối / Basic: cả 3 ✓ và giữ nguyên.
6. Dòng position (chỉ Mixed và Complex, nhiều position): một dòng giữa thẻ mục tiêu và thẻ 1, cỡ chữ tiêu đề thẻ (14, đậm): „Position 1 ✓ · Position 2“. Position xong mang `Done Check`; position hiện tại màu xanh #0391DD và gạch chân; không tô nền xanh lá (thẻ không dùng chữ đỏ/xanh lá, người mù màu). Chữ „Position“ giữ nguyên ở DE/EN (từ của LVS).
7. ✓ là badge `Done Check` của Welcome/Concept, góc trên phải thẻ, không che chữ ở DE và EN. Không có log mới.

## Không làm
- Không có ✓ trên thẻ „Darum geht es“ (027).
- Bỏ qua trường hợp hiếm: ở position 2 cầm lại một kiện của position 1 nằm ngoài thùng → thẻ 1 có ✓ sớm (xr-ux-reviewer: ưu tiên thấp).

## Cần xem khi làm
- Cỡ chữ 14 ở khoảng 1,4 m có đủ đọc không (ảnh `instructions`, rồi trên kính).
- Complex: dòng position thêm chiều cao, thẻ 3 vẫn phải nằm trên thùng kho position 2 (`InstructionShelfLayout`).
