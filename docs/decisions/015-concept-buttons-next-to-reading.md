# Quyết định 015: Nút của tour và quiz ở ConceptScene nằm cạnh chỗ đọc, trong tầm tay duỗi; đáp án in trên nút A/B/C, câu hỏi rời giá ở −60°

- **Backlog:** #52
- **Trạng thái:** accepted
- **Ngày:** 2026-11-02
- **Thay thế (một phần):** 011, chỉ phần ConceptScene: thẻ câu hỏi quiz (`InstructionShelf/Panel/Cards/Info Canvas/Question`) không còn trên giá ở −60°. Các phần khác của 011 giữ nguyên (giá, thẻ Aufgabe/1/2/3, Menu bên phải, nút đỏ và thẻ kết quả). Sửa chi tiết „Cách làm“ của 010: „đáp án in trên thẻ câu hỏi vì ô nút nhỏ“ không còn đúng.
- **Ý kiến đã xét:** unity-reviewer, instructional-design-reviewer, xr-ux-reviewer → decision-synthesizer. Không hỏi requirements-analyst (không đổi quy tắc nghiệp vụ); chữ mới vẫn qua `german-reviewer`.
- **Lựa chọn của người dùng (2026-11-02):** phương án C. Cụm quiz ở bên phải gần nút đỏ. „Zurück“ chưa làm, chỉ làm nếu test kính thấy hay bấm nhầm „Weiter“.

## Bài toán
Test kính (2026-11-02): bấm „Weiter“ và A/B/C rất khó. Mỗi nút 3,6 × 2,5 cm ở mép bàn (y 0,79, z 0,34), cúi ~65°, chỉ ~0,34 m trước thân nên phải co khuỷu tay. Câu hỏi và danh sách đáp án ở trên giá (yaw −60°), nút chỉ ghi chữ cái (yaw 8°): từ đề xuống đáp án phải quay ~68° ngang và nhớ „B = …“. „Weiter“ (yaw −22°) cách bảng tour (+36°) 58° ngang, 51° dọc. Mục tiêu: duỗi tay thoải mái, bấm trúng lần đầu, thấy thay đổi mà không cúi/quay nhiều.

## Xung đột giữa các đề xuất
| Vấn đề | Một bên | Bên kia | Kết luận |
|---|---|---|---|
| Vấn đề chính | unity: tầm với, cỡ nút | ID, UX: chú ý bị tách rời (đề ↔ đáp án 68°) | Sửa cả hai |
| Một chỗ bấm hay hai | UX PA 1, ID PA 2: bảng chung giữa bàn | ID PA 1, UX PA 2, unity PA B: mỗi hoạt động một cụm cạnh chỗ đọc | Hai cụm; bảng chung nằm trên đường tay lấy kiện |
| Đáp án trên nút | ID, UX: có | unity: không đổi code | Đổi nhỏ phần hiển thị của `PackageQuizController` |
| Dời thẻ câu hỏi | ID: dời | unity: thêm giá là thêm vật thừa | Dời thẻ (thay chỗ cũ, không thêm tấm mới) |
| „Zurück“ | UX: nên có | — | Để sau |
| Nút đỏ ở quiz | ID: giữ | UX: giữ | Giữ |

## Phương án được chọn: C (hai cụm, mỗi cụm cạnh chỗ đọc)
1. **Cụm quiz:** thẻ câu hỏi rời `InstructionShelf`, nằm ngay trên hàng nút A/B/C, cả cụm nghiêng về phía mắt, trong tầm tay phải duỗi, giữa chính diện và nút đỏ. Mỗi nút là một hàng rộng ghi „A: …“ bằng nhãn đáp án sẵn có; thẻ chỉ còn đề và „Frage n von 4“. Đáp án đang chọn có „» “ và thêm một dấu hiệu không dựa vào màu đúng/sai. Nút đỏ vẫn là bước kiểm tra; thẻ phản hồi giữ chỗ (#33).
2. **„Weiter“:** nút to, trong tầm tay, ở phía bảng tour `Sample Description`: lệch phải, thấp hơn đường nhìn tới bảng, không che bảng hay dialog, cách Menu đủ xa.
3. **Một mẫu nút cho cả hai cụm:** mỗi nút ≥ 6–8 cm rộng, 4–5 cm cao, khe ≥ 1,5 cm, nghiêng 30–40°, ~0,5–0,55 m trước vai; hàng đáp án rộng hơn để vừa chữ. Phản hồi hover/nhấn thấy ngay trên nút; âm thanh đã có (`AudioTrigger.PlayAudio`). Giữ canvas poke ISDK đang có, chỉ dời và đổi cỡ; không đổi tên, không thay loại component.
4. **Code:** `WmsFieldTour` không đổi. `PackageQuizController` chỉ đổi cách hiện (nhãn nút ghi đáp án, thẻ không liệt kê đáp án); logic chấm, `wrongReason`, thứ tự câu, key, ID giữ nguyên.

**Được:** bỏ chỗ tách rời nặng nhất (đề ↔ đáp án 68°), không phải nhớ chữ cái, „Weiter“ cạnh nội dung nó đổi, mặt bàn giữa trống cho tay lấy kiện.

**Mất / chấp nhận:** hai chỗ bấm thay vì một (hiệu quả học xếp trên tính gọn); phía phải bàn đông hơn (Menu, nút đỏ, thẻ phản hồi, cụm quiz, „Weiter“), phải kiểm trên Quest; từ bảng tour sang marker quanh màn hình vẫn ~35° (giữ như 010); câu Mat.-Nr vẫn phải nhìn bảng nhãn hoặc kiện bên trái (tải của bài tập, 007); sửa 011 cho riêng Concept.

**Bị bác:** chỉ phóng to/nâng lên (không sửa phần đọc đề); vùng chạm lớn tại chỗ cũ (không sửa co tay); bảng chung giữa bàn (trên đường tay và đường nhìn tới kiện; phương án dự phòng); ghép quiz vào bảng tour (câu Mat.-Nr phải quay +36° ↔ −34°); bỏ nút đỏ ở quiz.

## Done when
- Trên Quest, đứng tại chỗ, tay phải duỗi thoải mái (khuỷu không gập rõ) chạm được „Weiter“ và từng nút đáp án, bằng hand tracking lẫn controller.
- Trên Quest, bấm „Weiter“ 10 lần liền: lần nào cũng trúng ngay lần đầu.
- Ảnh Play mode từ mắt (0; 1,6; 0) nhìn về bảng tour: thấy „Weiter“ và bảng tour trong cùng khung hình.
- Ảnh Play mode nhìn về cụm quiz: thấy đề, cả ba đáp án và „Frage n von 4“ trong cùng khung hình; mỗi nút ghi đủ đáp án (ví dụ „A: 2 Kartons“), không chữ nào bị cắt.
- Đáp án dài nhất („Nur einzeln und geordnet einlegen“) hiện đủ trên nút, đọc được từ chỗ đứng.
- Thẻ câu hỏi không còn liệt kê „A: … B: … C: …“; không còn thẻ câu hỏi nào trên `InstructionShelf`.
- Trong ảnh `overview` và `wms`, các nút không che dialog, bảng tour, `Info Panel` hay ba kiện trên bàn.
- Trên Quest, cầm lần lượt ba kiện trên bàn rồi đặt lại: không lần nào vô tình bấm „Weiter“ hay đổi đáp án.
- Trên Quest, trả lời trọn 4 câu, bấm nút đỏ mỗi câu: không lần nào vô tình mở Menu. Các nút cách nút đỏ và mép Menu ≥ 15 cm (đo trong Editor).
- Hover và nhấn một nút: nút đổi trạng thái thấy được ngay và có âm thanh. Đáp án đang chọn có „» “ và một dấu hiệu thứ hai không dựa vào màu đúng/sai.
- Tour vẫn đi 10 bước theo thứ tự cũ, bước cuối nút ghi „Von vorn“. Quiz chấm như trước: sai → „Nicht richtig“ + lý do; đúng → câu sau; câu cuối nhắc bài „3. Kommissionieren nach EKZ 3“.
- Mọi chuỗi mới hoặc đổi có key + EN + DE qua `localize-strings` và đã qua `german-reviewer`.
- (code) `WmsFieldTour` không đổi; `PackageQuizController` không đổi logic chấm, ID câu, key đáp án, `wrongReason`; không mất tham chiếu Toggle/ToggleGroup; Console ConceptScene không lỗi mới.

## Nếu giả định sai
- Hai cụm gây nhầm, hoặc phía phải quá đông, hay chạm nhầm Menu/nút đỏ: chuyển sang bảng chung (B), đặt cao hơn đường tay, dùng lại cụm và mẫu nút.
- Hay bấm nhầm „Weiter“: dời ra xa đường tay; vẫn chưa đủ thì thêm „Zurück“ (chuỗi mới qua `localize-strings`).
- Đáp án dài không vừa nút: cho xuống hai dòng hoặc nới hàng nút; không rút gọn đáp án EKZ khi chưa qua `german-reviewer`.
- Mọi vị trí là transform, đổi lại được mà không động tới code.

## Ràng buộc cho việc sau
- #30 (gaze): cụm quiz mới cần collider vùng nhìn riêng, không giao với giá, màn hình và Menu (theo 011).

## Bằng chứng
- Nút chỉ hiện chữ cái, đáp án liệt kê trên thẻ → `Assets/Scripts/PackageQuizController.cs` (`Letters`, `ShowQuestion`, `RefreshLabels`) → đã kiểm
- „Weiter“ tự nhả, tour quay vòng theo modulo → `Assets/Scripts/Concept/WmsFieldTour.cs` (`NextStep`, `OnNextToggled`) → đã kiểm
- Số đo nút, góc nhìn, vị trí bảng/Menu/nút đỏ → đo trong Editor 2026-11-02 (brief #52) → đã kiểm
- Giá ở −60° vì `Info Panel` chiếm −27..−44° → `docs/decisions/011-workstation-layout-conveyor.md`, CLAUDE.md → đã kiểm
- Góc quay 58°/51° và 68°/40° → tính của xr-ux-reviewer từ số đo → chưa kiểm lại
- Cỡ nút tối thiểu theo khuyến nghị Meta → không có nguồn → chưa kiểm
