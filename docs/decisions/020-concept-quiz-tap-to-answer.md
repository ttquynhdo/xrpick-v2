# Quyết định 020: Cụm câu hỏi ConceptScene chấm ngay khi chạm như Complex, tự nói cách trả lời, chỉ mở sau tour

- **Backlog:** #61 (làm trong mục Concept gộp #73)
- **Trạng thái:** accepted (sửa một phần bởi 026, 2026-11-04: mục 3, cụm câu hỏi Concept)
- **Ngày:** 2026-11-03
- **Ý kiến đã xét:** unity-reviewer, data-analyst, xr-ux-reviewer, instructional-design-reviewer → decision-synthesizer. Không hỏi requirements-analyst (không có quy tắc nghiệp vụ mới) và german-reviewer (chữ mới qua `localize-strings` + `german-reviewer` khi làm).
- **Lựa chọn của người dùng (2026-11-03):** phương án B (chạm là chấm). Nút đỏ ở Concept: không còn việc gì cần tương tác nên **ẩn** theo nguyên lý redundancy (không giữ lại chỉ để có tiếng bấm). Áp dụng mọi thay đổi backlog liên quan vì mục tiêu nhất quán.
- **Sửa quyết định đã chấp nhận:** 015, chỉ dòng „Nút đỏ ở quiz: giữ“ và mục bị bác „bỏ nút đỏ ở quiz“. Vị trí cụm, đáp án in trên nút và „Weiter“ giữ nguyên. Lý do: góp ý video 2026-11-03 (người mới không biết cách trả lời); #58 (thói quen „chọn rồi bấm nút đỏ“ chuyển sang Complex); 019 chốt nút đỏ là F6. 016 không đổi.

## Bài toán
Cụm A/B/C ở ConceptScene hiện ngay từ bước tour 1, giữa tầm nhìn, trong khi câu EKZ cần kiến thức của bước 7–8. Tại cụm không có chữ nào nói cách trả lời, chọn đáp án không có phản hồi, kết quả hiện ở thẻ `Instruction Table/Feedback` cách cụm ≈33°. Complex chấm ngay khi chạm (016), nên trong khoá có hai cách chấm.

## Xung đột giữa các đề xuất
| Vấn đề | Bên A | Bên B | Đánh đổi |
|---|---|---|---|
| Concept chấm bằng gì | unity, data: giữ nút đỏ, thêm chữ tại cụm | ID, UX: chạm là chấm như Complex | Sửa ít, giữ 015 ↔ một cách chấm, không quay đầu, nút đỏ chỉ nghĩa F6 |
| Chạm nhầm | data: thành `WRONG_ANSWER` ở lần thử đầu | ID, UX: chấp nhận, log ghi lần thử | Thước đo „đúng lần đầu“ sạch ↔ nhất quán, ít tải |
| Nút „Prüfen“ cho cả hai quiz | data | — | Dữ liệu sạch nhất ↔ sửa 015 + 016, thêm UI |
| Mở quiz khi nào | ID, UX, unity, data: sau tour, đã mở thì luôn mở | ID PA 3: từng câu sau bước dạy nó | Rẻ, đủ ↔ gắn chặt nhất nhưng công L |

## Các phương án
- **A. Giữ nút đỏ chấm, dòng 3 pha tại cụm, mở sau tour:** vẫn quay đầu 4 lần, nút đỏ hai nghĩa, hai cách chấm. Dự phòng.
- **B. Chạm là chấm như Complex, phản hồi trong cụm, mở sau tour:** chọn.
- **C. B + nút „Prüfen“ tại cụm cho cả hai quiz:** tốn nhất, sửa cả 016. Chỉ khi buổi test thấy chạm nhầm thật.
- **D. Từng câu mở ngay sau bước tour dạy nó:** công L, đụng #60/#71. Để sau.

## Quyết định: phương án B
1. **Chấm khi chạm** theo mẫu `RemainderQuizController`: bật một toggle là chấm câu đó. Sai → trong cụm hiện „Nicht richtig“ + lý do `.wrong`, câu giữ nguyên, chọn lại được. Đúng → dòng trong cụm hiện „Richtig!“ + câu nhắc câu tiếp theo, câu sau hiện, dòng vẫn còn để người học thấy kết quả. Câu cuối → dòng hiện câu `all_done` (nhắc mở bài 3 trong Menu), toggle thôi chấm. Đổi câu và reset toggle không chấm thêm lần nào (mẫu `switching` của Complex).
2. **Dòng chỉ dẫn/phản hồi trong cụm Concept**, cùng chỗ và cỡ với `Remainder Quiz/Unity Canvas/Feedback` (font 16). Trước khi chọn: nói chạm A/B/C để trả lời. TMP do script ghi, không gắn `LocalizeStringEvent`.
3. **Mở sau tour:** trước khi mở, cụm chỉ hiện một dòng báo câu hỏi mở sau phần giới thiệu, không nút nào chạm được. Cụm mở lần đầu khi tour tới bước `material_flow` (nhận bằng `id`, không bằng số). Đã mở thì luôn mở, kể cả sau „Von vorn“; không dùng `steps[].markers` cho việc mở. Controller nằm trên object luôn bật, chỉ phần hiển thị ẩn/hiện.
4. **Nút đỏ ở Concept: ẩn** (người dùng, redundancy: không còn việc gì cần tương tác). Thẻ `Instruction Table/Feedback` không dùng nữa: tắt hẳn, nối lại `feedbackText` sang dòng mới, không để tham chiếu None. `concept.card.step3_body` bỏ câu nút đỏ. `concept.quiz.select_first` không còn dùng, xoá khỏi CSV.
5. **Complex không đổi** (016). Hai quiz chấm cùng một cách.

**Được:** cả 5 ý của *Done when* ngay tại chỗ người học nhìn; nút đỏ chỉ một nghĩa (F6, 008/019) trong cả khoá; câu EKZ chỉ hỏi sau bước 7–8; hai quiz ghi log cùng kiểu.

**Mất / chấp nhận:** sửa 015; Concept không còn chỗ luyện „chọn → bấm nút đỏ“ (Welcome đã cho tập bấm, nghiệp vụ thật không có thao tác này); chạm nhầm tính là một lần sai (Complex đã chịu rủi ro này, khoá trong lúc tour bỏ phần lớn chạm thừa, #28 ghi lần thử); cỡ M; người muốn làm quiz ngay phải đi hết tour.

**Bị bác:** giữ nút đỏ chấm (A, dự phòng); „Prüfen“ cho cả hai (C); từng câu sau từng bước (D); ẩn hẳn cụm khi khoá (cần báo trước); mở quiz bằng `markers` („Von vorn“ ẩn lại); giữ nút đỏ ở Concept chỉ để có tiếng bấm (người dùng: redundancy).

## Done when
Xem `docs/backlog.md` #73 (phần #61).

## Nếu giả định sai
- Buổi test thấy chạm nhầm hay đoán lần lượt A, B, C: chuyển cả hai quiz sang C („Prüfen“ tại cụm); dòng phản hồi và khoá đến hết tour vẫn dùng được.
- Người học không thấy cụm vừa mở: thêm một câu chỉ xuống cụm vào bảng tour ở bước cuối, nếu vừa giới hạn chữ.
- Người hướng dẫn muốn câu EKZ trả lời từ trí nhớ: bỏ bảng EKZ khỏi `markers` của bước cuối (sửa nhỏ 018).
- Muốn quay lại nút đỏ chấm: logic gọn trong `PackageQuizController`, hiện lại nút, nối lại `CheckAnswer`.

## Ảnh hưởng tới backlog
#58 vẫn cần; #60 làm trước #61 trong cùng mục Concept (bước Soll : VPE phải nằm trước bước cuối); #59 chỉ còn Basic/Mixed/Complex; #63 làm sau mục Concept; phần Concept của #71 dùng chung bước `material_flow` (kiểm ảnh `overview`: bảng 10 bước, bảng EKZ và cụm vừa mở không chồng nhau); #28 nhận ràng buộc mới (`quiz_answer` của Concept ghi lúc chạm, bỏ `NO_ANSWER`, thêm `quiz`, `question_id`, `selected`, `attempt_in_question`, event `quiz_unlocked`, nên có `tour_step`); #13 có key mới, `concept.card.step3_body` đổi, `concept.quiz.select_first` bị xoá.

## Câu hỏi cho người hướng dẫn (giả định, kiểm ở buổi test)
Người làm có được tra bảng khi trả lời câu về Empf Kz không (giả định: được). Quan sát: người mới có chạm nhầm A/B/C không.
