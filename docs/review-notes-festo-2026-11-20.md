# Đánh giá chuyên gia Festo (2026-11-20)

Nguồn: `study design/completed_XPick_Training_Review_2026-11-19.docx` (thư mục chỉ có trên máy, không commit). Đây là bản Festo điền vào file tổng quan 10 câu hỏi `XPick_Training_Review_2026-11-19.docx` (ảnh từng bài + câu hỏi, tiếng Anh, dựng từ 41 ảnh chụp trên Quest ngày 2026-11-19, mã thử P101). Người điền: kỹ sư Supplier & Logistics Quality Engineering của Festo, cùng trưởng nhóm picking (Picking 2-2), ký ngày 20.11.2026 tại Saarbrücken.

**Đã chuyển vào backlog (2026-11-20):** F4 → #164 · F5 → #165 · F6 → #166 (cả ba để sau study). F1 (phím xác nhận) **chưa** thêm: chờ Festo trả lời file hỏi lại `study design/XPick_Confirm_Key_Questions_2026-11-20.docx`.

## Kết quả chung

- Q1–Q10 đều chọn **Yes**. C1 (độ khó tăng đều): Yes, „gamified approach: you need to master the easier levels before you can complete the more difficult ones“.
- Bài 1: „It describes the basics procedure very well“. Bài 3: „I really like the result cards“. Q8 (đổi dòng đơn): „Completely clear and realistic. It's the same process of the real procedure.“ Q9, Q10: „completely“.
- C2: bản hiện tại „already represents the actual process very accurately and in great detail“, là nền tốt để phát triển tiếp ở học kỳ sau.

## Các điểm

### F1. Phím xác nhận là F12 hoặc Enter trên bàn phím, không phải F6 (phải sửa trước study)
- **Góp ý:** comment ở dòng „F6 (red button)“ của bảng thuật ngữ, C2 („we use a keyboard rather than a physical button“), C3 („The only change needed is to replace the F6 key with F12/Enter … before the practical study“). Kèm ảnh chụp màn hình LVS thật: `F12 Bestätigen` (khoanh đỏ), `F6 Aufkl. korr.`
- **Hiện trạng:** app dựng theo dialog mới (`Kommissionierung.pptx` slide 2, „neuer Dialog – TMA, CSC4“, „Bestätigen mit Fuß“): nhãn `F6 Bestätigen` / `F12 Aufkl. korr.` trong `WMS_Kommissionierung`, nút đỏ ghi „F6“, 8 chuỗi CSV có „(F6)“, `complex.end.foot` „… bestätigen Sie mit dem Fuß“. Ảnh của Festo là dialog cũ (slide 3, „alter Dialog – KOM 1/2“, „Bestätigen mit F12“). Nguồn gốc F6: backlog dòng 8 (B8, nhóm tự trả lời 2026-11-01), decision 008 (nút đỏ = bàn đạp, B11 chưa được xác nhận), spec `docs/requirements/wms-specs/kommissionierung-dialog.md` dòng 179 („suy ra“).
- **Ghi chú:** đã gửi lại Festo 4 câu hỏi (dialog nào đang dùng, có bàn đạp không, có sắp chuyển sang dialog mới không, study nên hiện dialog nào + phím nào). Thêm mục backlog sau khi có trả lời; nếu chỉ đổi phím thì sửa nhãn F6/F12 trong prefab, `WmsDialogView.FlashConfirmKey`, nhãn nút đỏ, chuỗi „(F6)“, `complex.end.foot`, CLAUDE.md và decision 008; nếu phải hiện dialog cũ thì là việc lớn (màn LVS 4 scene + khung tour 11 bước).

### F2. Hàng có bao bì thường là EKZ 6 (sau study)
Gộp vào F6.

### F3. Kết quả, phản hồi (không cần làm)
Thẻ kết quả, cách đổi dòng đơn, mức hỗ trợ giảm dần ở Bài 5, màn hình kết thúc đều được khen. Không có mục nào.

### F4. Mã vật tư đa dạng hơn (sau study)
- **Góp ý:** Bài 2 „wider variances in part numbers“, C2 „there is much wider variances of part numbers in the picking tasks“, C3 „include more examples of EKZs with different part numbers“.
- **Hiện trạng:** mỗi dòng đơn một vật tư, cả khoá 5 vật tư giả: Concept `stecker_c`; Basic `training_b`; Mixed `connector_a`, `screw_b`; Complex `ring_d`, `training_b` (đếm `materialKey` trong scene 2026-11-20). Mỗi thùng kho một Mat.-Nr (giả định D12).
- **Ghi chú:** Festo ghi „task for further development“. → #164.

### F5. Ảnh thật làm ví dụ EKZ trên bảng (sau study)
- **Góp ý:** Bài 2 „Real photo examples of EKZ“; C3 „add real-life photos of EKZ examples to the board“. Kèm ảnh bảng EKZ gốc của Festo (EKZ 0, 2, 3, 4, 5, 6, 8, 9; hàng „Beispiel“ có ảnh thật, khoanh đỏ).
- **Hiện trạng:** `EKZ Overview` ở ConceptScene chỉ có pictogram tự dựng bằng UI Image (#57, decision 018), không có ảnh.
- **Ghi chú:** ảnh là tài liệu nội bộ Festo: cần Festo đồng ý trước khi đưa vào app/repo. → #165.

### F6. EKZ 6 thay vì EKZ 3 cho hàng có bao bì (sau study)
- **Góp ý:** Bài 3 „part numbers with packaging are normally assigned to EKZ 6, because the parts are stored separately on each layer. However, the current setup is perfectly fine for learning purposes“; C2 tương tự.
- **Hiện trạng:** Bài 3 (Basic) và Mixed dòng 1 dạy EKZ 003 (decision 003, 028); EKZ 6 chỉ có một dòng trên bảng EKZ ở Concept.
- **Ghi chú:** Festo nói không cần sửa bây giờ. → #166 (cần `solution-decision` trước).

## Kế hoạch study (từ email của Festo, 2026-11-20, không nằm trong file review)
- Cả nhóm picking 50 người (80 % có kinh nghiệm, 20 % người mới), tự nguyện, mục tiêu 30 người. Dự phòng: ít nhất 10 kỹ sư và học viên logistics (ít nhất 3 người mới).
- Tuần lịch 2 và 4 (năm 2027), thứ Ba và thứ Tư cho nhóm picking; nhóm kỹ sư linh hoạt hơn.
- Còn phải trình thiết kế study cho hội đồng công nhân (IG Metall); quản lý nhà máy cho là không có vấn đề. Lịch và nội dung có thể còn thay đổi.
