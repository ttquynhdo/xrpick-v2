# Quyết định 012: MixedPickingScene làm từng position như dialog Kommissionierung thật

- **Backlog:** #21
- **Trạng thái:** accepted (sửa 2026-11-03 bởi 019/#71: đổi thùng kho bằng chạy trên làn trái/giữa, không còn thu nhỏ/phóng to)
- **Ngày:** 2026-11-01
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, data-analyst → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-01):** phương án A. Nếu đến buổi demo 2026-11-02 phần đổi thùng chưa ổn định thì demo bản C (dự phòng), #21 chưa `done` ở mức đó. Bỏ hẳn hộp RING D (`VPE_OpenedPackage`) khỏi MixedPickingScene.

## Bài toán
MixedPickingScene vẫn dùng `Info Panel` cũ: hiện cả hai dòng đơn cùng lúc bằng tiếng Anh, có TV và `PACKAGE PLACED`; thùng kho `PlasticBox_Storage` chứa lẫn carton KABELVERBINDER A và túi SCHRAUBE B, thêm hộp RING D không thuộc đơn. Mục tiêu: cùng `WMS_Monitor` + `WMS_Kommissionierung` như Basic/Complex, trainee làm position 1, xác nhận, rồi position 2; mỗi position có Ist riêng; kết quả trên thẻ cạnh nút đỏ. Số đơn giữ theo A5 (100/50, 40/20).

## Xung đột giữa các đề xuất
| Vấn đề | Một bên | Bên kia | Đánh đổi |
|---|---|---|---|
| Đèn `Mehrfachpick` / `letzter Pick` | Done when cũ của #21: bật Mehrfachpick ở cả hai position, `letzter Pick` ở position 2 | requirements: tài liệu đánh dấu Mehrfachpick bằng số vàng "1/2", không nói đèn R2 sáng; decision 009: mọi đèn tắt | Giống màn hình thật ↔ giữ quyết định đã chấp nhận; bật đèn không có quy tắc thì không đúng tài liệu hơn |
| Trainee biết "còn position sau" từ đâu | xr-ux PA2: chỉ từ dialog ("1/2", chờ B14) | ID, xr-ux PA1: thẻ hướng dẫn nói trước, thẻ kết quả nói bước tiếp | Tín hiệu WMS thật ↔ tín hiệu chắc chắn có ngay |
| Đổi thùng kho | unity A: bật/tắt hai thùng; xr-ux PA1: fade 0,3–0,5 s tại chỗ | xr-ux PA3, ID: trượt vào như băng lăn | Kịp demo, dễ đảo ngược ↔ gần D11 hơn (đụng #47) |
| Cấu trúc code | unity A: một container có chỉ số position | unity B: hai container, hai thùng đơn | B nhân đôi thùng đơn, trái quy tắc bố cục trạm |
| Thùng kho trong demo | unity C: để đổi thùng lại sau | Done when, ID, xr-ux: thùng chỉ chứa vật tư position đang làm | Rủi ro demo ↔ trainee lấy nhầm vật tư |
| Ghi log | data B: #21 tự log `line_confirmed` | data A: #21 chỉ lộ dữ liệu, #28 mới log | Có dữ liệu sớm ↔ đổi contract C#↔R trước demo |

## Các phương án
| | Đúng nghiệp vụ | Hiệu quả học | Tải nhận thức/UX | Chi phí/rủi ro | Nhất quán | Veto |
|---|---|---|---|---|---|---|
| **A. Một container có chỉ số position, xác nhận từng position, hai thùng kho một vật tư đổi bằng fade, đèn R2 vẫn tắt, báo position trên thẻ** | + một xác nhận chấm một position, một pick một mã | ++ phản hồi sau từng bước con, chỉ thêm một yếu tố mới so với Basic | + mỗi lúc một dòng, thùng không lẫn vật tư, có nhịp chuyển | 0 ~1 ngày, rủi ro trung bình | + cùng prefab, order chỉ trong `orderLines` | — |
| B. Hai container, hai thùng đơn | + | + | − thùng đơn thừa | − wiring phức tạp | − trái quy tắc bố cục trạm | — |
| C. Monitor + tuần tự, giữ một thùng kho lẫn vật tư | 0 | 0 lỗi lấy nhầm kiểm "chọn vật tư" | − phải tìm trong thùng lẫn | + nửa ngày | 0 | — (dự phòng demo) |
| D. Chuyển position nhưng chỉ xác nhận một lần cuối | − | − không biết sai ở position nào | 0 | + | 0 | không đạt Done when; mất số lần thử theo position |

## Phương án được chọn: A
- Container biết position hiện tại: `CurrentLine` = dòng đó, `CheckOrder` chỉ chấm dòng đó (một dòng vào `OrderRule.Check`).
- Chỉ chuyển position khi `Correct`. Sai thì ở lại position hiện tại, thấy lỗi đầu tiên (006).
- Khi chuyển: gói của position 1 không còn được tính vào Ist hay "hàng lạ" của position 2; Ist position 2 bắt đầu từ 0; dialog vẽ lại qua `ContentsChanged`. Xử lý cả trường hợp thùng kho bị tắt khi gói đang nằm trong trigger (`OnTriggerExit` có thể không bắn).
- Hai thùng kho, mỗi thùng một vật tư, cùng một chỗ; thùng 2 hiện bằng fade ngắn.
- Thẻ hướng dẫn nói trước rằng đơn có hai position; thẻ kết quả khi position 1 đúng nói bước tiếp (tiếng Đức, *Sie*, qua `localize-strings`).
- Container lộ ra số thứ tự position (từ 1), tổng số position và một sự kiện khi đổi position, cho #28. #21 không ghi log.
- Bỏ TV, `Info Panel`, `PACKAGE PLACED`, `orderLineTexts` (chạy `scene-ref-auditor` trước khi xoá) và hộp RING D.

**Decision 009 giữ nguyên:** mọi đèn R2, kể cả `Mehrfachpick` và `letzter Pick`, tắt trong Mixed. Tài liệu không nói đèn Mehrfachpick sáng (dấu hiệu thật là số vàng "1/2"), `letzter Pick` chưa có quy tắc (B13). Nếu B14 = có, thêm chữ vàng "1/2"/"2/2" góc trên phải (decision 008 đã dự trù cho #21); đó là chữ hiển thị, không phải đèn R2, và không điều khiển bộ đếm position.

- **Được:** đúng luồng một position, một mã, một lần xác nhận, phản hồi ngay; Mixed nhất quán với Basic/Complex; logic chọn dòng test được bằng EditMode; #28 có dữ liệu theo từng position.
- **Mất / chấp nhận:** chưa có "1/2" thật (bù bằng câu trên thẻ, chờ B14); thùng đổi bằng fade, chưa trượt (để #47); chi phí cao hơn C.
- **Bị bác:** bật đèn theo Done when cũ (trái 009, không có căn cứ); hai container (B); chỉ dựa vào dialog (phụ thuộc B14); thùng trượt vào (hoãn sang #47); hỗ trợ giảm dần giữa hai position (ý cho #25/#33 sau); một xác nhận cuối (D); log ngay trong #21 (để #28).

## Done when (thay các câu cũ của #21)
- Vào MixedPickingScene: dialog `WMS_Kommissionierung` trong `WMS_Monitor` ở (0.06, 1.26, 1.066), cùng cỡ như Basic/Complex, chỉ hiện KABELVERBINDER A, Soll 100, Ist 0. Không còn TV, `Info Panel` hay `PACKAGE PLACED`.
- Vào scene: thùng kho chỉ chứa carton KABELVERBINDER A; không có túi SCHRAUBE B hay hộp RING D trong scene.
- Đặt 2 carton KABELVERBINDER A, bấm nút đỏ → thẻ cạnh nút báo position 1 đúng và nói bước tiếp bằng chữ; dialog chuyển sang SCHRAUBE B, Soll 40, Ist 0; thùng kho cũ mờ đi, thùng chỉ chứa túi SCHRAUBE B hiện ra đúng chỗ đó.
- Ở position 1, bấm nút đỏ với 1 carton → thẻ báo lỗi đầu tiên kèm lý do; dialog vẫn ở position 1, thùng kho không đổi.
- Ở position 2, các carton KABELVERBINDER A đã nằm trong thùng đơn không làm đổi Ist và không bị báo là vật tư lạ.
- Ở position 2, bấm nút đỏ với số túi sai → thẻ báo lỗi, dialog vẫn ở position 2, không quay về position 1.
- Ở position 2, đặt 2 túi SCHRAUBE B, bấm nút đỏ → thẻ báo cả đơn đúng.
- Ở mọi position, tất cả đèn R2 (gồm `Mehrfachpick`, `letzter Pick`) tắt.
- Thẻ hướng dẫn trên bàn nói trước khi bắt đầu rằng đơn có hai position, làm lần lượt.
- (code) Container có số thứ tự position, tổng số position và một sự kiện khi đổi position; `OrderRule` không đổi; order chỉ khai báo trong `orderLines`. Console không có lỗi mới.

## Nếu giả định sai
- B14 = có "1/2": thêm chữ vàng "1/2"/"2/2" vào dialog, có thể bớt câu trên thẻ.
- B13 có quy tắc cho `letzter Pick`: decision mới thay một phần 009.
- D11 = thùng đi bằng băng lăn: thay fade bằng chuyển động ở #47, logic không đổi.
- D12 = một thùng chứa nhiều mã: gộp lại một thùng.
- Mỗi position dùng Auftragsbehälter riêng (D13): đổi cách xử lý gói của position 1 trong container.

## Câu hỏi còn mở
- Người hướng dẫn: B13, B14, D11, D12 (đã có); D13 (gói của position 1 ở lại cùng thùng đơn hay thùng đơn được đổi sau mỗi position), D14 (thứ tự position có cố định không).

## Bằng chứng
- 009 tắt mọi đèn, kể cả `letzter Pick` → `docs/decisions/009-wms-indicators-off.md:8` → đã kiểm
- Done when cũ yêu cầu bật Mehrfachpick/`letzter Pick` → `docs/backlog.md` #21 → đã kiểm
- 008 dự trù "1/2" ở #21 nếu B14 = có; B13 còn mở → `docs/decisions/008-wms-dialog-panel.md:39,42` → đã kiểm
- `WmsDialogView` giữ đèn tắt và đọc `source?.CurrentLine` → `Assets/Scripts/WmsDialogView.cs:31,129` → đã kiểm
- `MixedPickingContainer.CheckOrder` chấm cả đơn một lần → `Assets/Scripts/MixPickingContainer.cs:66-69` → đã kiểm
- Mehrfachpick đánh dấu bằng số vàng "1/2"; nhiều mã thì đánh số → Kommissionierung slide 4, 9 (requirements-analyst) → chưa kiểm
- Mixed có 6 carton A, 3 túi B, `VPE_OpenedPackage` RING D → scene inventory, ảnh overview → chưa kiểm

## Cập nhật 2026-11-03 (B14, D26)
- `Picks 2/4` của dialog cũ đếm lượt pick từ **cùng một thùng kho** (`docs/requirements/old/WMS explanation.txt`, mục 13), không đếm position của đơn. Dòng *Nếu giả định sai* „B14 = có "1/2": thêm chữ vàng "1/2"/"2/2"“ vì vậy **bỏ**: không hiện „1/2“ để chỉ position; câu trên thẻ hướng dẫn tiếp tục báo position sau.
- Bảng bấm giờ „OIS2 Kommi2“ (bước 1, 3, 9, 10) và ảnh trạm `old/Orderpicking.png` (các số trên ảnh là các bước đó) mô tả: thùng kho tới bằng làn trái và được kéo vào chỗ làm việc ở giữa; thùng rỗng kéo vào làn phải thành thùng đơn; sau mỗi lần xác nhận thùng kho tự đi (suy ra: về kho theo làn giữa, vì làn trái chỉ đi vào theo người dùng nhớ), thùng kho tiếp theo vào từ trái; thùng đơn đứng yên, chỉ được kéo sang làn giữa và tự đi khi đơn xong. Cách Mixed làm (thùng đơn đứng yên, thùng kho đổi theo position) khớp; việc đổi thùng kho bằng thu nhỏ/phóng to thay cho chạy trên băng được ghi thành #71. Chỗ chưa rõ (thùng kho nhiều lượt pick, làn giữa, ba ô trên dialog cũ) hỏi ở D26.
