# Quyết định 042: Complex đảo thứ tự hai dòng đơn; RING D thành túi nhựa trong suốt; Mixed dùng cùng kiểu túi

- **Backlog:** #174 (Complex), #173 (túi Mixed)
- **Trạng thái:** accepted
- **Ngày:** 2026-11-24
- **Ý kiến đã xét:** không chạy `solution-decision`; người dùng chọn trực tiếp trong chat sau khi xem ba phương án (đảo thứ tự + RING D thành túi; giữ thứ tự + viết lại câu hỏi sang „Beutel“; chỉ đổi carton RING D nhỏ và trắng). Nên nhờ `data-analyst` duyệt phần log/R và `german-reviewer` duyệt chữ khi làm #174.
- **Lựa chọn của người dùng (2026-11-24):** phương án đảo thứ tự.
- **Thay thế (một phần):**
  - **016:** vị trí 1 là SCHLAUCHSCHELLE B (carton + kẹp lẻ), vị trí 2 là RING D (túi + vòng lẻ). Câu hỏi phép chia vẫn đứng trước vị trí 1, nay hỏi bằng số của SCHLAUCHSCHELLE B.
  - **032:** Complex không còn carton đã mở chỉ để nhìn; các bài nhặt hàng không còn ngoại lệ nào với luật cầm (Concept giữ hai mẫu EKZ).
  - **029:** RING D không còn carton; túi (Mixed SCHRAUBE B, Complex RING D) không còn là mesh `PlasticBag02` × 0,375 mà là túi nhựa trong suốt dẹt (mục 2).
  - **039:** câu hỏi thứ 3 (cách bỏ vào thùng) giữ nguyên, hỏi cho dòng đơn của vị trí 1.

## Bài toán

Test kính 2026-11-24 (T8): hai dòng đơn của Complex đều là „carton nâu + vòng“, chỉ khác màu hàng lẻ, nên người học bối rối. Người dùng muốn hai dòng đơn có hình dạng hàng khác nhau. Toàn bộ chữ của bài 5 (câu hỏi trước vị trí 1, thẻ nhiệm vụ, tên bài, mục tiêu cuối khoá) nói về „Karton“, và câu hỏi chỉ đứng trước vị trí 1.

## Quyết định

1. **Complex đảo thứ tự:**
   - Vị trí 1 = SCHLAUCHSCHELLE B (443890, VPE 50): carton nâu chuẩn + kẹp ống lẻ (`HoseClamp`, #171) trong khay nông (mục 3). Câu hỏi phép chia (3 câu, #123) đứng trước vị trí 1 như nay, chữ „Kartons“ giữ nguyên.
   - Vị trí 2 = RING D (264857, VPE 25): **túi nhựa trong suốt** có vòng bên trong + vòng lẻ lấy từ khay nông trong thùng kho 2 (mục 3). Không có câu hỏi; người học áp dụng cùng phép chia cho dạng đóng gói khác.
2. **Túi:** túi nhựa trong suốt dẹt, mép hàn mỏng, hàng nhìn thấy bên trong, chỉ để nhìn (cầm cả túi); tem nằm phẳng trên mặt túi, cỡ chuẩn 10 × 6 cm (#88). Mixed SCHRAUBE B: 12 × 10 × 3,5 cm, 20 vít (ảnh `TestEvidence/MixedPickingScene/bag-preview/clear_pouch_*.png`). Complex RING D: túi 14 × 14 × 3,5 cm, bên trong 4 chồng vòng (2 × 2, mỗi chồng 6 vòng ≈ 24 vòng nhìn thấy).
3. **Khay nông chung („Modul“) cho hàng lẻ** (người dùng chọn 2026-11-24, thay „carton đã mở / túi đã mở“): mỗi thùng kho của Complex có một khay nông (thành ~3 cm) nằm trong thùng; kẹp SCHLAUCHSCHELLE B (thùng kho 1) và vòng RING D (thùng kho 2) nằm phẳng trong khay, cầm được. Khay là một phần của thùng kho, không giống kiện hàng: không `PackageData`, không biểu tượng bàn tay gạch chéo, không câu lý do. `VPE_OpenedPackage`, `SB_OpenedPackage` và `Opened Carton Note` bỏ; `complex.storage.opened_carton` bỏ hoặc viết lại. Dựa trên ảnh 5.12 của tài liệu Festo (Anbruchmenge trong một Modul ở thùng kho; câu A13 chưa xác nhận, không chặn).
4. **Số lượng (người dùng duyệt 2026-11-24):** vị trí 1 SCHLAUCHSCHELLE B Soll 152 (3 carton + 2 kẹp), vị trí 2 RING D Soll 53 (2 túi + 3 vòng). Câu hỏi A/B/C, Rest, thẻ kết quả và câu „… Kartons + … Stück“ tự tính từ dòng đơn.
5. **Chữ chung:** câu kết `complex.feedback.correct` „Volle Kartons und loses Material …“ đổi thành cách nói chung cho cả carton và túi (ví dụ „volle Verpackungen“); tên bài 5 và mục tiêu „volle Kartons und lose Teile ausrechnen“ giữ nguyên vì phép tính được dạy và hỏi bằng carton.
6. **Log/R:** bài đổi nên tăng `layout_version`; câu hỏi Complex nay hỏi bằng số của SCHLAUCHSCHELLE B (đáp án mới), `make_fake_data.R`, `analyse_set.R`, `item_skills.csv` sửa theo; không gộp dữ liệu Complex trước/sau.

## Được / mất

**Được:** hai dòng đơn khác hẳn về hình dạng (carton nâu vs túi trong), hết bối rối T8; chữ câu hỏi và mục tiêu bài 5 gần như không đổi; người học chuyển phép tính sang dạng đóng gói thứ hai; túi trong khớp hướng dẫn Festo 5.5/5.6 và Mixed.

**Mất / chấp nhận:** phải dựng khay nông và túi RING D; mất hình ảnh „carton bóc dở“; đổi thứ tự thùng kho và câu thẻ theo vị trí; dữ liệu Complex của layout cũ không gộp được với layout mới; „Lose Teile = Soll – (Verp.-Einh. × volle Kartons)“ ở vị trí 2 áp cho túi mà không nói chữ „Beutel“.

## Câu hỏi còn mở

- Festo (không chặn): A13 – phần dư có để trong một Modul ở thùng kho không; ở Picking 2, vòng/RING-loại hàng có đựng túi nilon trong suốt không.
