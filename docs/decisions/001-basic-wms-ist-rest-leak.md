# Quyết định 001: Ist/Rest trên WMS làm lộ đáp án ở BasicPickingScene

- **Backlog:** #34
- **Trạng thái:** accepted (phương án C, 2026-11-01)
- **Ngày:** 2026-11-01
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-01):** chưa làm phương án nào, chờ người hướng dẫn. Khi có câu trả lời thì chạy lại `solution-decision`, bắt đầu từ file này.
- **Cập nhật 2026-11-01:** nhóm trả lời B6, B7, B7a là **theo tài liệu** (Rest = tồn kho thùng kho; đếm tay thì Ist = Soll, cân thì Ist tăng dần), gần phương án C. Chưa chạy lại `solution-decision`; cần làm sau khi #1 chốt đơn Basic, vì cách lấy (đếm hay cân) phụ thuộc Soll.
- **Lựa chọn của người dùng (2026-11-01, sau decision 003):** đơn Basic là 300 chiếc (giả định A1), tức là cân (B7a). Người dùng chọn **phương án C, theo tài liệu**: Ist tăng dần trong lúc lấy, Rest là tồn kho tĩnh của thùng kho. Chấp nhận rằng Ist cho biết lúc nào đủ; phép tính 300 ÷ 50 không còn được kiểm ở bài này. Không chạy lại `solution-decision`: người dùng chọn thẳng giữa hai phương án còn lại (C hoặc A).
- ***Done when* của phương án C** (thay cho mục *Done when* của A bên dưới):
  - Lúc vào scene: Soll hiện `300`, Ist hiện `0`, Rest hiện tồn kho còn lại của thùng kho sau khi lấy đủ đơn (tồn kho thùng kho − 300; số tồn kho theo #24).
  - Đặt 1 carton vào thùng đơn → Ist tăng đúng 50; lấy ra → Ist giảm đúng 50. Rest không đổi.
  - Đặt đủ 6 carton → Ist `300`; bấm xác nhận → `Correct`.
  - Console không có lỗi mới.

## Bài toán
Trong BasicPickingScene, trainee phải tự tính số kiện cần lấy (Soll 300 ÷ VPE 50). Họ chỉ được biết đúng hay sai sau khi xác nhận. `CalcForWMS` cập nhật số trên WMS theo từng kiện đặt vào thùng: Rest giảm 50, Ist tăng 50. Vì vậy `Rest: 0` hoặc `Ist = Soll` báo luôn là đã đủ. Ngoài ra, Rest đang mang nghĩa "còn thiếu", trong khi tài liệu yêu cầu định nghĩa Rest là tồn kho còn lại của thùng kho (Kommissionierung, slide 7).

**Phát hiện mới:** không chỉ Rest mà **Ist cũng làm lộ đáp án**. Chỉ sửa Rest thì chưa đạt *Done when*.

## Xung đột giữa các đề xuất
| Vấn đề | Một bên | Bên kia | Đánh đổi |
|---|---|---|---|
| Có che Ist không | requirements: Ist cập nhật theo cân là tài liệu cho phép | ID, UX, Unity: Ist = Soll cũng lộ đáp án | Giữ một trường gần thực tế nhưng không đạt *Done when* |
| Rest: ẩn ngay hay hiện số tồn kho | ID, UX, Unity: ẩn trước, làm đúng nghĩa sau B6 | requirements: chỉ nghĩa "tồn kho" mới khớp hoàn toàn với tài liệu | Đúng nghĩa, nhưng phải chờ B6 và chọn số tồn kho |
| Hiện Ist sau khi xác nhận để làm feedback | ID, UX: có, để giải thích sai ở đâu | UX must, #14: không được in số còn thiếu. Ist chỉ đúng khi #2 đã sửa | Học tốt hơn nhưng phụ thuộc #2 và #14 |
| Câu hỏi "Wie viele VPE?" | UX: tăng tải tăng cường | Quy trình thật không có bước này | Học sâu hơn, nhưng lệch quy trình |

## Các phương án
| | Đúng nghiệp vụ | Hiệu quả học | Tải nhận thức/UX | Chi phí/rủi ro | Nhất quán | Veto |
|---|---|---|---|---|---|---|
| 0. Không làm gì | −− Rest = "còn thiếu" | −− không phải tính | − | ++ | 0 | V1 |
| **A. Ist và Rest hiện `–` trong lúc nhặt**; Soll và VPE vẫn hiện; kết quả qua thẻ `Correct`/`Incorrect` | 0/+ đơn giản hoá có chủ ý, không dạy sai | + phải tính | + bớt nhiễu | ++ một script, không có chuỗi mới | 0 | — |
| B. A + hiện Ist cạnh Soll sau khi xác nhận | 0 | ++ (nếu không in số còn thiếu) | + | − cần #2, sửa `OrderConfirm`, có chuỗi mới | − chồng với #14/#33 | — |
| C. Theo WMS thật: Rest = tồn kho tĩnh, Ist theo chế độ thật | ++ nếu B6/B7 xác nhận | − nếu chưa dạy (#20) | 0 | − blocked, phải chọn số tồn kho | + khớp #10/#20 | — (blocked) |
| D. Thêm câu hỏi "Wie viele VPE?" | − | + | − | − | − | — |

## Khuyến nghị ban đầu của synthesizer (không được chọn): phương án A
- **Được:** đạt *Done when* mà không cần #1/#2. Bỏ nghĩa sai của Rest. Không có chuỗi mới cần dịch. Dễ đảo lại.
- **Mất:** panel lệch so với dialog thật, và Ist/Rest chưa được dạy (việc của #10/#20). Mất phần feedback giải thích; phần này đưa vào #14, làm sau #2.
- **Ràng buộc khi làm:** `Update()` ghi đè text mỗi frame, nên phải che trong script, không thể làm trong Inspector. Không đổi tên field public. `currentSize` vẫn phải đếm, vì `OrderConfirm` dùng nó.
- ***Done when* bổ sung nếu chọn A:**
  - Lúc vào scene, Ist và Rest hiện `–`, Soll hiện `300`, VPE hiện `VPE: 50`.
  - Đặt rồi lấy ra từng kiện, Ist và Rest vẫn là `–`.
  - 6 kiện rồi bấm → `Correct`; 5 kiện rồi bấm → `Incorrect` (giống trước khi sửa).
  - Console không có lỗi mới.
- **Nếu câu trả lời khác giả định:**
  - B6 = "tồn kho" → Rest hiện một số tĩnh.
  - B7/B7a = Ist cập nhật live theo cân → phép tính phải được kiểm bằng cách khác (xem D, hỏi lại người hướng dẫn).

## Câu hỏi còn mở
- Người hướng dẫn: **B6, B7, B7a** (mới), A4, A1 (#1). Nội dung trong file câu hỏi bảo mật.
- Người dùng, khi bỏ chặn:
  - Dùng `–` hay để trống?
  - Có chuyển phần "hiện Ist sau khi xác nhận" sang #14 không?
  - Có sửa #15 (ComplexScene có `Ist: 30 ST` hiện ngay trong lúc nhặt, có thể cùng kiểu lộ đáp án) theo cùng quy tắc không?
- **Lưu ý:** kể cả sau A, bỏ hết các kiện có sẵn vào thùng vẫn là đáp án đúng (#24).
