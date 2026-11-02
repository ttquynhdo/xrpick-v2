# Quyết định 017: Làm nổi bước hiện tại trên giá thẻ (Basic, Mixed, Complex)

- **Backlog:** #40
- **Trạng thái:** accepted
- **Ngày:** 2026-11-02
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-02):** phương án B. Sửa chiều rộng thẻ `Confirm` ngay trong #40. Câu nhắc quiz trên thẻ 1 ở Complex tách thành mục backlog riêng (#53).

## Bài toán
Bốn thẻ trên giá (~55 từ) nổi bật như nhau, nên trainee không biết mình đang ở bước nào. Cần một tín hiệu cho bước hiện tại ở cả 3 scene picking. Bước chỉ được chuyển theo thao tác (cầm kiện, kiện vào thùng), không bao giờ theo đúng/sai. Mixed và Complex có 2 position nối tiếp, kiện của position đã xong vẫn nằm lại trong thùng. Riêng Complex có quiz trước position 1.

## Tài liệu
Kommissionierung slide 2: Sichtung (dialog + thùng kho) → Bearbeitung des Pickvorganges → Bestätigen (chân hoặc Restmengenzählung) → ggf. Abschieben. Thứ tự thẻ 1–3 (từ #25) đã khớp. Tài liệu không nói gì về điều kiện chuyển bước, lùi bước hay vị trí quiz, nên các chi tiết đó là giản lược huấn luyện được phép. Vòng lặp lặp lại cho mỗi position, nên reset theo position là hợp quy trình.

## Xung đột giữa các đề xuất
| Vấn đề | Một bên | Bên kia | Đánh đổi |
|---|---|---|---|
| Thẻ 3 sáng ngay khi carton đầu tiên vào thùng | requirements, unity: một thẻ sáng, đúng *Done when* cũ | ID, UX: thẻ 2 vẫn là bước chính, thẻ 3 chỉ báo "sẵn sàng" | Đơn giản ↔ không đẩy trainee xác nhận khi Basic còn thiếu 5/6 carton |
| Lùi bước | unity: chỉ tiến | requirements, ID: lùi khi thùng rỗng / sau xác nhận sai | Phản ánh đúng trạng thái ↔ nhấp nháy, nguy cơ lộ đáp án |
| ✓ cho bước đã xong | requirements, ID: kiểu #39 | UX, unity: ✓ = "đúng", thẻ không đỏ/xanh | Thấy tiến độ ↔ lộ đáp án |
| Quiz ở Complex | requirements, UX: thẻ 1 sáng suốt quiz | ID: thêm chữ trỏ tới quiz | Giữ #40 thuần highlight ↔ nối bước 1 với chỗ thao tác (cần chữ mới) |
| Thu gọn thẻ / pulse | unity, ID: chỉ restyle, tĩnh | UX: thu gọn thẻ khác, pulse 0,5 s | Chi phí thấp ↔ segmenting mạnh hơn, dễ thấy ở rìa mắt (chưa có dẫn chứng) |

## Các phương án
- **0. Không làm gì:** thứ tự thẻ đã đúng, nhưng không có signalling.
- **A. Một thẻ sáng, chỉ tiến, reset theo position:** đơn giản; ở Basic thẻ 3 thành thẻ chính sau carton đầu tiên, dễ bị hiểu là "xác nhận ngay".
- **B. A + thẻ 2 vẫn chính, thẻ 3 có mức phụ "sẵn sàng":** chọn.
- **C. B + dòng chữ trỏ tới quiz ở Complex:** đổi nội dung, tách thành #53.
- **D. Thu gọn thẻ không phải bước hiện tại:** hoãn; ở Complex ẩn gợi ý bài toán, có nguy cơ đổi kích thước thẻ, trùng hướng "nội dung mờ dần" mà 011 đã để sau.

## Quyết định: phương án B
- „Aufgabe“ không phải là bước: luôn trung tính, không sáng, không bị làm mờ.
- **Đọc:** thẻ `1. Angaben im LVS ablesen` là thẻ chính lúc bắt đầu và sau mỗi `LineAdvanced`. Ở Complex position 1 thẻ này sáng suốt lúc làm quiz; không cần logic riêng vì thùng kho chỉ hiện sau quiz.
- **Lấy hàng:** lần Select `Grabbable` đầu tiên trên một kiện có `PackageData` trong position hiện tại → thẻ 2 thành thẻ chính, giữ tới hết position, kể cả sau xác nhận sai.
- **Mức phụ "sẵn sàng" trên thẻ 3:** bật khi thùng đơn có ≥ 1 kiện chưa thuộc position đã xong (kiện nào cũng tính, kể cả vật liệu lạ), tắt khi số đó về 0. Đây là trạng thái duy nhất được lùi; nó chỉ phản ánh có/không có kiện.
- **Xong:** `OrderCompleted` → tắt mọi highlight. Không ✓, không xanh lá/đỏ.
- **Hình thức:** một đối tượng đánh dấu riêng, không raycast/collider, màu xanh dương như `Next Lesson Highlight` (#39), cộng một tín hiệu không phụ thuộc màu (viền dày hơn, hoặc giảm tương phản các thẻ bước khác nhưng vẫn đọc được). Không sửa chữ thẻ, không chèn „» “ (tiêu đề thẻ 1 đã xuống 2 dòng). Thẻ không đổi kích thước/vị trí.
- **Điều kiện trước:** thẻ `Confirm` cùng chiều rộng và lề với 3 thẻ `Info Canvas` (Basic: thẻ 3 đang rộng hơn, lệch trái, chữ bị cắt).

**Được:** signalling theo thứ tự quy trình; highlight không bao giờ đẩy trainee bấm sớm và không đổi theo kết quả; reset theo `LineAdvanced` không lộ gì thêm (thẻ Result và WMS đã đổi); không chữ mới; dùng đúng sự kiện duy nhất `OrderCompleted` (decision 011).
**Mất / chấp nhận:** thêm một mức hiển thị so với A. Lúc đặt kiện trainee nhìn đi chỗ khác (~57° từ thẻ tới thùng, suy ra), nên highlight phục vụ "liếc lại là biết mình ở đâu". Kiện cầm tay lướt qua trigger vẫn bật mức phụ; chấp nhận vì mức phụ yếu và tự tắt.

**Bị bác:** một mức (A) vì tín hiệu xác nhận sớm; lùi bước đầy đủ (chỉ giữ cho mức phụ); ✓ (lộ đáp án, quy tắc không đỏ/xanh); event bước riêng trong từng validator (lặp logic 3 lần); polling `Update` (lệch trạng thái, tốn chu kỳ trên Quest). Hoãn: pulse (chờ thử trên Quest), thu gọn thẻ (D).

## Done when
Xem `docs/backlog.md` #40.

## Ghi chú kỹ thuật (từ unity-reviewer, không bắt buộc cách làm)
- Logic bước nên là lớp C# thuần test được bằng EditMode; MonoBehaviour mỏng chỉ nối sự kiện và bật/tắt hình.
- Mixed/Complex phát `ContentsChanged` ngay sau `LineAdvanced` (`MixPickingContainer.cs:100–104`, `PiecesContainerCounter.cs:106–110`), nên không coi mọi `ContentsChanged` là "có kiện vào"; cần đọc số kiện của position hiện tại (hiện chỉ có `CurrentPackages()` private).
- Kiện của position 2 inactive lúc bắt đầu: phải theo dõi cả đối tượng inactive.
- Thẻ 2 ở Complex đổi chữ theo `PositionTexts`: highlight bám theo thẻ, không cache chữ.

## Nếu giả định sai
- Dấu phụ gây hiểu nhầm trên Quest: quay về A, hoặc A với điều kiện "thùng có kiện và tay không cầm gì" cho thẻ 3. Chỉ đổi trạng thái hiển thị.
- Highlight tĩnh không được nhận ra: thêm pulse một lần.
- Người hướng dẫn muốn quiz là bước riêng: làm qua #53, cơ chế #40 không đổi.

## Câu hỏi cho người hướng dẫn
D19, D20.
