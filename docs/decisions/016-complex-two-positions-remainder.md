# Quyết định 016: ComplexScene thành một đơn hai position, có ví dụ phép chia có dư, câu hỏi phép chia và chiếc lẻ lấy từ carton đã mở

- **Backlog:** #19
- **Trạng thái:** accepted
- **Ngày:** 2026-11-02
- **Ý kiến đã xét:** unity-reviewer, requirements-analyst, data-analyst, instructional-design-reviewer, xr-ux-reviewer → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-02):** phương án A, với các chi tiết: position 2 = SCHLAUCHSCHELLE B (`training_b`, carton VPE 50), Soll 156 = 3 carton + 6 chiếc lẻ; ví dụ mẫu 64 / VPE 20 = 3 carton + 4 chiếc; thùng kho position 1 khoá đến khi trả lời đúng câu hỏi chia; carton RING D đã mở chứa 12 vòng. Câu nhắc Anbruchmenge nói rõ phần dư để trong Modul ở thùng kho (xem *Tài liệu*).

## Bài toán
Complex đưa vào cùng lúc phép chia có dư, chiếc lẻ và hai cách đóng gói của một vật tư. Trước đó không scene nào làm mẫu phép chia có dư (quiz Concept chỉ hỏi 100/25, chia hết). 13 chiếc lẻ nằm sẵn trên bàn nên trainee không tự lấy. Từ #10 (decision 008) Ist trên dialog tăng khi đặt hàng, nên đặt đúng gói không chứng minh trainee biết chia. Mục tiêu: làm từng bước, có ví dụ mẫu trước, phép chia được chấm trực tiếp, trainee tự lấy chiếc lẻ từ carton đã mở.

## Tài liệu (đã kiểm 2026-11-02)
- **Anbruchmenge (Detail Description 5.12 + ảnh `sec020_img1`; tài liệu cũ 4.6.6 + `sec025_*`):** phần dư khi lấy từ một VPE phải để trong một Modul. Ảnh ✅ cho thấy phần dư nằm trong một khay carton nhỏ đặt *trong thùng kho*, trên mặt hàng; ❌ là để lẫn vào hàng. Tài liệu cũ nêu lý do: bao bì đã mở bỏ thẳng vào thùng thì không thấy được phần dư, có Modul thì thấy ngay. "Thùng trong ảnh là thùng kho" là suy ra (thùng chứa toàn một loại hàng), cần người hướng dẫn xác nhận (A13).
- **Chiếc lẻ và túi PU (Kommissionierung slide 6):** đến 7 chiếc thì đếm tay, chỉ thứ không nắm vừa tay mới đóng túi PU có nhãn F5; từ 8 chiếc (Referenzwaage) hàng lẻ luôn đóng túi PU + F5. Câu "đến 7 chiếc để rời trong thùng đơn" nằm trong mục đặc biệt Bühne/M2/KONFEKT. → Với ≤ 7 vòng nhỏ, bỏ rời vào thùng đơn là hợp tài liệu; chờ xác nhận (A12). Số chiếc lẻ của mọi position giữ ≤ 7 (cũng tránh Picktrennung từ 8 chiếc, DD 5.10).
- **Tự mở carton:** không có câu cho phép trực tiếp, nhưng 5.12 định nghĩa Anbruchmenge là phần dư phát sinh khi lấy từ một VPE, tài liệu cũ nói tới bao bì đã mở, và chỉ có một ngoại lệ bị cấm mở (túi 10 cái hơn 2 thành phần, DD "Informationen Kommissionierung"). → Lấy chiếc lẻ từ carton đã mở là có căn cứ; không cần hỏi thêm.

## Xung đột giữa các đề xuất
| Vấn đề | Một bên | Bên kia | Đánh đổi |
|---|---|---|---|
| Một scene hay hai | unity, ID, UX: một scene, hai position theo 012 | data-analyst: position 1 thành scene riêng (một dòng `summary.csv` mỗi bước) | Đơn vị phân tích sẵn có ↔ đổi tên ở build order, 5 `SceneMenuController`, `LessonNames`, Menu 6 bài, `scenes.csv`, lệch mẫu 012 |
| Tự lấy chiếc lẻ trong #19 hay tách ra | ID, UX, Done when: trong #19 | unity: rủi ro cao (cha–con, `GrabFreeTransformer`, TOO_MANY), tách item riêng | Mục tiêu chính ↔ rủi ro kỹ thuật, thử trên Quest |
| Bước cất Anbruchmenge | UX: đặt carton đã mở vào Modul | requirements: nhắc bằng chữ là mức tối thiểu chấp nhận được | Gần quy trình ↔ thêm thao tác |
| Ví dụ mẫu ở đâu, số nào | unity: thẻ trên giá, dùng 55 = 2×25 + 5 | data H1, ID, UX: bảng cạnh màn hình, số khác bài làm | Ít vật mới ↔ không lộ đáp án, giữ mẫu thẻ `Aufgabe/1/2/3` |

## Các phương án
| | Đúng nghiệp vụ | Hiệu quả học | Tải nhận thức/UX | Chi phí/rủi ro | Nhất quán | Veto |
|---|---|---|---|---|---|---|
| **A. Một scene, hai position (012), bảng ví dụ số khác, câu hỏi chia A/B/C trước khi nhặt, chiếc lẻ lấy từ carton đã mở, Anbruchmenge nhắc bằng chữ** | + | ++ ví dụ → làm có hỗ trợ → làm không hỗ trợ | + | − cỡ L, thử trên Quest | + cùng mẫu Mixed, Menu 5 bài | — |
| B. Hai scene | + | + | 0 thêm một lần chuyển scene | −− | − | — |
| C. A nhưng chiếc lẻ vẫn nằm sẵn | 0 | + | + | + | + | không đạt "tự lấy hàng lẻ" (dùng làm đường lùi) |
| D. Chỉ thêm ví dụ và câu hỏi | 0 | − | + | ++ | 0 | không đạt Done when |
| E. A + thao tác đặt carton đã mở vào Modul | + (theo ảnh 5.12) | + | 0 thêm thao tác | − | 0 | hoãn: làm sau nếu A13 xác nhận |

## Phương án được chọn: A
1. **Một đơn hai position** trên `PiecesContainerCounter.orderLines` (005), chạy bằng `OrderSequence` như 012. Position 1: RING D, Soll 55, VPE 25. Position 2: SCHLAUCHSCHELLE B, Soll 156, VPE 50. Chỉ chuyển position khi xác nhận đúng; gói của position 1 nằm lại trong thùng đơn nhưng không còn được tính. `OrderCompleted` (014) chỉ bắn khi position 2 đúng. KABELVERBINDER A vẫn là vật gây nhiễu ở cả hai position.
2. **Bảng ví dụ mẫu** (64 / VPE 20 → 3 carton nguyên + 4 chiếc lẻ) chỉ hiện ở position 1, sát bên phải màn hình theo lối `Sample Description` của Concept. Giải từng bước: Soll ÷ VPE → số carton nguyên, phần dư → số chiếc lẻ. Không chỗ nào in sẵn đáp án của đơn thật.
3. **Câu hỏi phép chia** trước khi nhặt ở position 1: cụm nút A/B/C dưới màn hình theo mẫu 015, hỏi số carton nguyên và số chiếc lẻ. Đáp án nhiễu là lỗi hay gặp (làm tròn lên, quên phần lẻ, nhầm phần dư với phần còn trong carton). Hỏi theo kiểu số học để vẫn đúng dù A7 trả lời thế nào. Sai thì nói cách tính, không nói đáp án. Chấm ngay khi chọn, không dùng nút đỏ (nút đỏ là F6 xác nhận đơn, 008). Thùng kho position 1 hiện sau khi trả lời đúng (cơ chế `PositionStockSwitcher`).
4. **Chiếc lẻ:** carton RING D đã mở (12 vòng) đứng cố định ở kho, không cầm được và không bao giờ được tính là carton nguyên. Từng vòng cầm được riêng; mỗi vòng đặt vào thùng đơn làm Ist tăng 1.
5. **Anbruchmenge:** một câu trên thẻ: phần còn lại trong carton đã mở để trong Modul ở thùng kho. Không chấm.
6. **Position 2** không có bảng ví dụ, không có câu hỏi (fading). Thẻ chỉ hiện nội dung của position đang làm.
7. Nút đỏ (F6) và `Instruction Table/Result` giữ chỗ ở mọi pha; đèn R2 tắt (009).

- **Được:** đạt cả ba câu Done when cũ; phép chia được chấm trực tiếp, tách khỏi `TOO_FEW`/`MISSING_LOOSE`; cùng cơ chế Mixed nên Menu, build order, `LessonNames` không đổi; logic position và nhánh carton mở test được bằng EditMode.
- **Mất / chấp nhận:** phân tích theo bước phải dựng từ events (bù bằng ràng buộc cho #28); resume sau crash làm lại từ position 1 (như Mixed); cỡ L, bắt buộc thử trên Quest. Ba giản lược ghi rõ: không đóng túi PU/F5 (chờ A12), Anbruchmenge chỉ nhắc bằng chữ, carton đã mở sẵn.
- **Thứ tự làm trong #19:** position, ví dụ, câu hỏi trước và kiểm bằng MCP; carton mở làm sau. Nếu carton mở không ổn trên Quest: lùi về C (chiếc lẻ nằm sẵn cạnh carton mở), mở item mới, #19 chưa `done`.
- **Bị bác:** hai scene (B, lợi ích phân tích có được bằng trường `step` ở #28); ví dụ dùng 55/25 hoặc đặt trên giá thẻ (lộ đáp án, phá mẫu thẻ); thao tác Modul thật (E, hoãn tới A13); "dạng gói thứ hai" = Karton ↔ PU-Beutel (vướng A10).

## Done when (thay các câu cũ của #19)
Xem `docs/backlog.md` #19.

## Ràng buộc cho #28
- Thêm `step`/`line_no` vào các event kết quả và một event khi đổi position.
- `task_completed` của Complex và `EndSession("completed")` chỉ ứng với lúc cả đơn đúng (thiết kế logger hiện dùng lần xác nhận đúng đầu tiên ở Complex, phải sửa).
- Ghi câu hỏi chia thành `quiz_answer` với skill mới (`skill_map.csv`, `scene_skills.csv`); đáp án là khoá cố định, không phải chữ tiếng Đức.
- Spec, `make_fake_data.R`, `analyse.R`, `selftest.R` đổi trong cùng một commit.

## Nếu giả định sai
- Carton mở không ổn trên Quest: lùi về C, mở item mới.
- A13 xác nhận Modul trong thùng kho: có thể làm E (đặt carton đã mở vào khay Modul), có log cất / không cất.
- A12 = phải đóng túi PU/F5: thêm bước đóng túi (item mới); câu hỏi chia giữ nguyên.
- A7 chấp nhận cách lấy khác: chỉ sửa rule phần lẻ (005/006).
- Nghiên cứu cần một dòng `summary.csv` mỗi bước: tách position 1 thành scene riêng (B), đặt tên scene trước khi #28 chạy với người thật.
