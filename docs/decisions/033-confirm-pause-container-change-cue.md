# Quyết định 033: Chờ ngắn cố định để đọc thẻ kết quả, gợi ý nhìn ngay tại thùng khi đổi thùng

- **Backlog:** #99
- **Trạng thái:** accepted
- **Ngày:** 2026-11-05
- **Thay thế (một phần):** 019, câu „Từ lúc bấm tới lúc thùng mới dừng ≈≤ 4 s“ (nay khoảng 7 s: 4 s chờ + khoảng 3 s chạy). Mọi phần khác của 019 (ba làn, tự động, D27) giữ nguyên.
- **Ý kiến đã xét:** unity-reviewer, requirements-analyst, data-analyst, xr-ux-reviewer, instructional-design-reviewer, rồi decision-synthesizer. Chữ mới đi qua `localize-strings` + `german-reviewer` khi làm.
- **Lựa chọn của người dùng (2026-11-05):**
  - Chấp nhận phương án B.
  - U1: ở Complex cuối, `Congratulations` hiện khoảng 2 s sau khi thùng đơn bắt đầu chạy, không hiện ngay.
  - U2: chờ 4 s (theo đề xuất).
  - U3: không thêm âm thanh băng tải.

## Bài toán
Ở Basic, Mixed và Complex, sau khi bấm nút đỏ (F6) đúng thì chỉ 0,6 s sau thùng kho đã chạy, người học chưa kịp đọc thẻ kết quả. Backlog giả định chờ cố định 12 s. Người dùng lo rằng chờ lâu thì người học quay đi chỗ khác (ví dụ sang Menu) và bỏ lỡ cảnh đổi thùng trên ba làn, mà không biết là mình cần xem.

Hình học (xr-ux-reviewer, suy ra từ toạ độ, mắt (0, 1.6, 0)): thẻ kết quả lệch phải khoảng 53°, thùng kho lệch trái khoảng 26°, tức phải quay đầu khoảng 79°. Menu chỉ cách thẻ khoảng 10°. Người học trôi sang Menu là do bố trí, không chỉ do phải chờ lâu.

## Tài liệu nói gì (requirements-analyst)
- Tài liệu không quy định thời gian giữa lần xác nhận và lúc thùng chạy, cũng không có thao tác tay để „cho thùng chạy“ (Kommissionierung slide 20; Detail Description).
- Detail Description có cụm „ggf. Abschieben“ cho thùng đơn, chưa rõ là đẩy tay hay tự động (D11, D26c, câu mới D41).
- Mọi cách tự động (chờ ngắn, chờ + gợi ý, chờ theo hướng nhìn) đều là giản lược huấn luyện được phép. Thêm một thao tác „cho thùng chạy“ (nút trên thẻ, F6 lần hai) thì mâu thuẫn với cách hiểu OIS2 hiện tại.

## Các phương án
| | Đúng nghiệp vụ | Hiệu quả học | Tải nhận thức/UX | Chi phí/rủi ro |
|---|---|---|---|---|
| A. Chỉ chờ cố định 4 s + một câu trên thẻ | + | 0: vẫn phải quay đầu 79° | 0 | ++ chỉ Inspector |
| **B. A + gợi ý tại thùng (chọn)** | + | +: mắt được dẫn tới làn, nối tên thùng đã học ở Concept | +: gợi ý ở chỗ sự kiện xảy ra | +: S→M, dùng lại `Box Sign`, `concept.lane.*`, marker cam |
| C. B + chỉ chạy khi đầu hướng về trạm, có mức tối đa | + | ++ | 0: quy tắc vô hình, ngưỡng góc phải chỉnh trên kính | −: M, thêm `released_by` vào log, sửa spec + R, khó test bằng MCP |
| D. Nút trên thẻ / F6 lần hai | −−: thêm thao tác trạm thật không có; F6 hai lần dạy sai bàn đạp | −: bấm cho qua mà không nhìn | −: mắt vẫn ở thẻ | −: làm méo `confirm_pressed`/`confirm_ignored` |

Chờ 12 s là A với số lớn hơn, bị loại: chờ lâu mà không có gợi ý thì người học vẫn không biết phải nhìn.

## Quyết định: phương án B
1. `PositionStockSwitcher.startDelay` = **4 s**, cùng giá trị ở Basic, Mixed và Complex (bằng `LaneFlowDemo` ở Concept). Thẻ kết quả hiện ngay. Trong lúc chờ, mọi thứ khoá như lúc thùng đang chạy: `IsMoving` đã là true từ trước khi chờ (`PositionStockSwitcher.StartRide`: `blockingRides++` và `Freeze()` chạy trước `RunRide` đợi `startDelay`), nên nút đỏ chỉ ghi `confirm_ignored`, kiện trong thùng cũ không cầm được, dialog WMS và highlight thẻ đợi.
2. Thẻ kết quả có **một** câu báo thùng sắp đổi và chỉ hướng nhìn, cùng mẫu câu ở 3 scene, gần chữ đã dạy ở Concept (`concept.tour.material_flow.action`, `concept.flow.step9_auto`). Complex vị trí 1 cũng có câu này trên thẻ kết quả (thẻ kệ 3 giữ câu của nó).
3. Nhãn „Lagerbehälter“ / „Auftragsbehälter“ (`Box Sign` như Concept #92, key `concept.lane.*`) gắn ở thành sau thùng kho và thùng đơn ở Basic, Mixed, Complex, chạy theo thùng.
4. Mũi tên cam (cùng kiểu marker ở Concept) trên làn giữa chỉ ở Basic, lần đổi thùng thật đầu tiên: hiện trong lúc chờ và lúc thùng chạy, tắt khi thùng đơn đã đi. Các bài sau chỉ có câu trên thẻ và nhãn thùng (gợi ý giảm dần).
5. Complex cuối: `Congratulations` hiện khoảng 2 s sau khi thùng đơn bắt đầu chạy, để panel không che làn giữa lúc thùng đi (người dùng U1). `EndSession("completed")` giữ thời điểm như cũ.
6. Không có âm thanh mới (U3).
7. Log: định dạng sự kiện không đổi; `station_move_start` vẫn ghi lúc thùng bắt đầu chạy (sau khi chờ). `layout_version` lên "033" vì `wait_s` của dữ liệu cũ (0,6 s) và mới (4 s) không được gộp. Spec ghi rõ khoảng chờ; `analyse_set.R` lấy khoảng chờ từ dữ liệu (`station_move_start.t − position_completed.t`), không giả định 0,6 s; `selftest.R` xanh; `data-analyst` duyệt.

## Được / mất
- **Được:** đọc kịp thẻ trước khi có chuyển động; mắt được dẫn tới chỗ đổi thùng; không thêm thao tác mà trạm thật không có; cả khoá thêm khoảng 17 s; quay lại dễ (một giá trị Inspector).
- **Mất:** người đang nhìn Menu vẫn có thể lỡ một lần đổi thùng. Chấp nhận vì ba làn là mục tiêu „chỉ xem“ (decision 032), không chấm điểm, và có 5 lần đổi thùng.

## Nếu giả định sai
- 4 s quá ngắn/dài trên kính: đổi `startDelay` trong Inspector.
- Người học vẫn bỏ lỡ cảnh đổi thùng dù có gợi ý: làm C trên nền B (thêm `released_by` vào `station_move_start`, sửa spec + `make_fake_data.R` + `analyse_set.R` + `selftest.R`, tăng `layout_version`).
- Người hướng dẫn nói thùng đơn phải đẩy tay (D11, D41): B vẫn an toàn, vì Concept đã nói trong bài tập thùng tự chạy, ở trạm thật thì bằng tay.

## Câu hỏi mở
- D41 (người hướng dẫn, không chặn): có khoảng chờ thật giữa F6 cuối và lúc thùng kho đi không; thùng đơn có phải đẩy tay không.
