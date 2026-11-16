# Quyết định 039: Complex hỏi Empf Kz bằng câu hỏi thứ 3 của Remainder Quiz (position 1), không vẽ lên LVS

- **Backlog:** #123
- **Trạng thái:** accepted
- **Ngày:** 2026-11-16
- **Ý kiến đã xét:** requirements-analyst, unity-reviewer, data-analyst, instructional-design-reviewer, xr-ux-reviewer (proposal mode, độc lập); decision-synthesizer.
- **Lựa chọn của người dùng (2026-11-16):** chấp nhận khuyến nghị (phương án C) với mặc định: câu EKZ đặt sau hai câu tính (U1); dòng kết quả thẻ 2 ghi thêm câu trả lời EKZ (U2); tăng `layout_version`, dữ liệu Complex position 1 không gộp với layout cũ (U3).
- **Thay thế (một phần):** 028 dòng „Complex: người học tự đọc Empf Kz, không có gợi ý“: position 1 có câu hỏi; position 2 vẫn tự đọc. Phần còn lại của 028 giữ: Complex 000/000, không chấm cách xếp, không `EKZ_PLACEMENT`.
- **Giữ nguyên:** 008 (dialog như thật, ô 000 nền xám), 036/#105 (bài thực hành không vẽ lên LVS), 037 mục 5 (cam chỉ ở tour Concept; `warning` = có gì sai), 016 (position 2 không có câu hỏi), #58 (nhắc vàng khi bấm F6 trước khi trả lời xong), #85 (giữ „Richtig!“ 1,5 s).

## Bài toán
Ở ComplexScene cả hai position là Empf Kz 000; trong review nhóm 2026-11-09 (C18) người học không để ý trường này. Thẻ 1 position 1 đã liệt kê „Empf Kz“ mà vẫn bị bỏ qua. Ở Complex đặt kiểu nào cũng được chấp nhận, nên Empf Kz không đòi một quyết định nào. Tài liệu không bắt làm nổi Empf Kz (slide 7 chỉ nói đọc Soll/Ist/Rest; slide 8: EKZ 0 có thể là hàng rời); nhận ra Empf Kz là mục tiêu học của khoá (D3), không phải yêu cầu quy trình. Log hiện không đo được việc để ý: Complex chỉ có `calculate_quantity`/`calculate_loose`.

## Quyết định
1. Ở position 1, sau hai câu tính, thêm câu 3 („Frage 3 von 3“): các phần (carton và chiếc lẻ) được đặt vào thùng đơn thế nào. Thân câu không ghi „000“, để người học phải đọc LVS. Ba nút A/B/C, mỗi nút một cách đặt; đáp án đúng cùng nghĩa với đáp án đúng của `concept.quiz.ekz_000`; đáp án nhiễu không được là quy tắc có thật cho 000.
2. Sai: phản hồi chỉ chỗ đọc (Empf Kz trong LVS) và nói vì sao, không nói đáp án (kiểu `complex.quiz.wrong_*`).
3. Thùng kho chỉ vào sau khi đúng **cả 3** câu; F6 trước đó chỉ hiện nhắc vàng #58.
4. Sau khi đúng, dòng đậm thẻ 2 (`complex.card.step2_body_answered`) ghi thêm câu trả lời EKZ cạnh „2 Kartons + 5 Stück“.
5. Position 2 không có câu hỏi. LVS không đổi.
6. Log: mỗi lần chạm câu 3 một `quiz_answer` `complex_remainder` với `question_id` cố định mới, `selected` là khoá enum. `item_skills.csv` thêm dòng Complex cho câu này → `ekz_placement`, D3. `scene_skills.csv` Complex **không** thêm `ekz_placement`. `layout_version` "036" (chung với decision 038 nếu cùng build). Spec, `make_fake_data.R`, `analyse_set.R` cùng thay đổi, `selftest.R` xanh, data-analyst duyệt.

## Đã bác
- Chỉ sửa chữ thẻ 1: chữ đó đã có và đã bị bỏ qua.
- Biển tĩnh „Empf Kz 000 = EKZ 0“ ở thùng/bàn sau: không có quyết định nào, lặp lại LVS và đọc hộ bước đọc; chép nguyên tag Concept kéo theo `No Grab Symbol` sai nghĩa. Có thể xét lại nếu sau C người học vẫn sai nhiều.
- Câu giải thích 000 trên thẻ kết quả sau F6: chỉ nói cho biết sau khi làm, thẻ đã 3 dòng.
- Chọn 000/003 trước khi F6 nhận ở position 2: bước nhân tạo, đổi `schema_version`.
- Pictogram EKZ 0 cạnh `Worked Example`: bảng ví dụ đã gây nhầm (C16).
- Khung/mũi tên cam trên LVS, `warning` vàng, phạt cách đặt ở 000, đổi Complex sang 003.
- Đặt câu EKZ trước hai câu tính (data-analyst): phải sắp lại luồng và dữ liệu `cartons`/`loose` không gộp được; câu hỏi đã là gợi ý dù đặt ở đâu.

## Được / mất
- **Được:** Empf Kz thành một quyết định có chấm, ngay dưới LVS. Không ký hiệu mới, không đụng LVS. Điểm đo thứ hai cho D3 sau Concept. Câu 1, câu 2 và hold #85 giữ nguyên (chỉ thêm nhánh code).
- **Mất:** phần hỏi dài thêm khoảng 10–20 s (cụm này đã gây nhầm, C15/C16, xử lý ở #119/#122). Chi phí code và log cao hơn. Position 2 không đo việc để ý 000.

## Nếu giả định sai
- Nghĩa EKZ 0 (D5/D21/D35/D45) sai: sửa đáp án đúng ở Concept và Complex cùng lúc qua CSV.
- Pilot thấy phần hỏi quá dài hoặc câu 3 gần như luôn đúng ngay: tắt câu 3, giữ key và log; dự phòng rẻ là câu giải thích sau F6.
- Muốn đo „để ý khi chưa có gợi ý“: chuyển câu EKZ lên đầu (sắp lại luồng).

## Câu hỏi mở
- Người hướng dẫn: D45 (EKZ 0 cho carton nguyên), D47 (có phải kiểm Empf Kz ở mọi dòng, 000 khác 003 thế nào khi làm).

## Bằng chứng
- `Assets/Scripts/Complex/RemainderQuizController.cs:40–41, 168–173, 208–240` (cứng 2 câu, `RevealFirstSet` sau câu 2, `quiz_answer` dòng 213).
- `docs/i18n/strings-todo.csv` (`complex.card.step1_body_position1`, `complex.card.step2_body_answered`, `complex.feedback.position_done`, `concept.quiz.ekz_000.*`).
- Decisions 008, 016, 028, 036, 037.
- `analysis/scene_skills.csv`, `item_skills.csv`, `objectives.csv`.
- Kommissionierung.pptx slide 7–9 (requirements-analyst; tóm lược, không trích dài).
- `docs/review-notes-team-2026-11-09.md` C18; ảnh `Image (8).jpg`, `Image (9).jpg` (thư mục thesis `07_presentations`).
