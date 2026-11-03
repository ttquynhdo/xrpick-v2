# Quyết định 008: Dialog `Kommissionierung` chung cho Basic và Complex; Mixed làm cùng #21; nút đỏ đóng vai bàn đạp F6

- **Backlog:** #10 (tách: #10 = prefab + Basic + Complex; dọn dẹp sau #21 = #46; Mixed = #21; phần Complex của #33 gộp vào #10)
- **Trạng thái:** accepted (phương án C, 2026-11-01); mục 4 phần chỉ báo thay bởi 009
- **Ngày:** 2026-11-01
- **Ý kiến đã xét:** requirements-analyst, unity-reviewer, instructional-design-reviewer, xr-ux-reviewer → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-01):** chấp nhận C; nút đỏ = bàn đạp (F6 trên màn hình không bấm được, chỉ nháy); Mixed làm cùng #21 (giữ panel đen tới lúc đó); kết quả Complex chuyển ra thẻ cạnh nút đỏ (phần Complex của #33) ngay trong #10.

## Bài toán
Basic, Mixed và Complex dùng ba kiểu panel WMS khác nhau. Basic là dialog xám nhưng lẫn trường của dialog cũ (`Auftrags-Nr`, `EKZ:`, `Pick: 1/1`); Mixed/Complex là panel đen tiếng Anh, không có Mat.-Nr, trong khi feedback sai vật tư (#14) bảo trainee kiểm Mat.-Nr trên WMS. Cần một mẫu panel tái hiện dialog mới (TMA/CSC4, `F6 Bestätigen`) theo spec bảo mật `docs/requirements/wms-specs/kommissionierung-dialog.md`, dữ liệu giả cùng định dạng thật (B10), đọc từ nguồn đơn có sẵn (005/006), giữ Rest/Ist theo 001.

## Xung đột chính
- Mixed trước #21: hai dialog cạnh nhau (một mẫu ngay) hay làm cùng #21 (đúng thực tế, Mixed lệch một thời gian).
- Xác nhận: bấm F6 trên màn hình (chữ của *Done when*, B9) hay nút đỏ đóng vai bàn đạp ("Bestätigen mit Fuß", pptx slide 2; panel ở ~1,1 m ngoài tầm tay).
- Đủ trường ngay từ Basic hay mở dần từng vùng.
- Nơi lưu Mat.-Nr / Empf Kz / Gew.: cấu hình của view hay dòng đơn.
- Feedback trước #33: dải tạm dưới dialog hay chuyển thẳng ra thẻ cạnh nút.

## Phương án đã chọn: C
1. **Một prefab phẳng** (canvas mới, tỉ lệ 1,59:1, child đặt tên theo vùng R1–R6) + một view script đọc nguồn đơn qua một interface nhỏ (dòng đơn, Ist, Rest, `ContentsChanged`), cập nhật theo event, không ghi text trong `Update`. Nhãn đúng từng ký tự theo spec (`Empf Kz:`, `Verp.-Einh.:`, `Mat.-Nr:`, `Bez.:`, `Karton:`, `Gew.:`, `Anweisung`), 12 chỉ báo, 14 phím F đúng thứ tự R6. Không có chrome MdiShell (B5).
2. **Xác nhận:** nút đỏ vật lý vẫn gọi `OrderConfirm.OrderConfirmation` / `ConfirmOrder` (validator, UnityEvent không đổi). 14 phím F là hình tĩnh, không hover, không nhận poke/ray; khi bấm nút đỏ, `F6 Bestätigen` hiện trạng thái *pressed* ~0,3 s.
3. **Dữ liệu hiển thị nằm cùng dòng đơn:** `OrderLine` có thêm Mat.-Nr, Empf Kz, Gew. và kiểu cân (Referenzwaage / Behälterwaage). Basic giữ các trường này trên `CalcForWMS` (cạnh `necessarySize`, `vpe`). Lúc Start, Mat.-Nr của dòng đơn phải bằng `PackageData.MaterialNumber` của carton cùng khoá, lệch thì `Debug.LogError`. `OrderRule` và định dạng kết quả 006 không đổi.
4. **Chỉ báo** (phần đèn thay bởi decision 009: mọi đèn R2 tắt): `Modus` = `Kommissionierung`; đúng một chỉ báo cân bật (theo Gew. giả và ngưỡng 0,7 kg), `Zählen` tắt; `letzter Pick` bật đỏ ở Basic và Complex (một pick); `Mehrfachpick`, `Vollbehälterausl. möglich` tắt; Empf Kz Basic `003` vàng, Complex `000` xám (003, D5); Karton và Anweisung trống, không tô (B4).
5. **Ist / Rest:** Ist tăng dần khi cân (cả hai scene), Complex dùng `TotalPieces`. Rest là tồn kho thùng kho, không đổi khi nhặt; Complex tính từ tồn kho Ring D trong scene trừ Soll, như `CalcForWMS` ở Basic.
6. **Gỡ:** Basic: `Auftrags-Nr`, `Pick: 1/1`, `EKZ Hinweis`, 3 thanh xanh `Anweisungen`, nhãn `EKZ:`. Complex: `Cartons placed` / `Ist: n ST` riêng (đã có Ist trong dialog), câu "Look below…"; hướng dẫn chỉ còn trên `Instruction Table`.
7. **Kết quả Complex** chuyển ra thẻ `Correct`/`Incorrect` cạnh nút đỏ, cùng kiểu Basic (phần Complex của #33), luôn có chữ, không chỉ màu.
8. **Cỡ và vị trí:** ~0,87 × 0,55 m, cùng chỗ panel hiện tại, nghiêng về phía mắt ~17°. Nếu đo trên Quest không đọc được: 1,18 m (chỉ đổi scale).
9. **Không thêm chú thích** trên dialog trong #10; child theo vùng để #20 gắn signalling. Nhãn WMS mới qua `localize-strings`, `do_not_translate`, giống nhau ở `en` và `de` (002).
10. **Các bước:** #10 = prefab + Basic, rồi Complex (+ #33 phần Complex). Mixed trong #21. Dọn field text cũ (`orderLineTexts`, `packageCountText`, `piecesPlacedText`…) ở #46 sau #21; chạy `scene-ref-auditor` trước khi thay hoặc gỡ.

- **Được:** một bố cục đúng spec ở Basic và Complex; Mat.-Nr có trên panel nên feedback sai vật tư có thứ để kiểm; Ist/Rest giữ 001; không ai học một dialog kép hay bị cắt bớt; kết quả Complex hết split attention; validator, `OrderRule`, hợp đồng log không đổi.
- **Mất / chấp nhận:**
  - Mixed giữ panel đen tiếng Anh tới #21.
  - *Done when* "bấm F6 thì kiểm đơn" đổi thành: nút đỏ đóng vai bàn đạp, F6 nháy. Nếu muốn bấm F6 được: thêm `Button` gọi đúng method cũ, chạy lại case xác nhận hai lần, #28 ghi nguồn bấm.
  - `Zählen` và Ist điền sẵn không xuất hiện ở bài nào; phép chia có dư ở Complex chỉ còn là việc trainee tự lên kế hoạch (Ist báo khi đủ). Đúng nghiệp vụ (B7 + 001) xếp trên hiệu quả học; phép chia được kiểm lại ở #19.
  - Basic hiện đủ dialog trước khi #20 dạy các trường.
  - Chữ R2/R6 khoảng 0,6–0,7° (suy ra, phải đo trên Quest); ô của nhiệm vụ (Soll, Ist, Mat.-Nr) ~2°.
- **Bị bác:** hai dialog cạnh nhau cho Mixed (màn hình không có thật, ±37°, làm lại ở #21); Empf Kz trong cấu hình view (thêm chỗ chép dữ liệu); component order riêng và prefab lồng; dải feedback tạm (dời hai lần); mở dần từng vùng và chỉ phóng to R3/R4 (trái *Done when*, thứ bậc khác màn hình thật); chú thích ngay trong #10 (thuộc #20).
- **Nếu giả định sai:** người hướng dẫn muốn bấm F6 → thêm `Button`; Gew. nghĩa khác → đổi dữ liệu dòng đơn; `letzter Pick` quy tắc khác → đổi cờ; Mehrfachpick có "1/2" góc phải trên → thêm ở #21; chữ không đọc được → scale 1,18 m; Verp.-Einh. thật luôn 1 (A9) → đổi số hiển thị.

## Câu hỏi còn mở
- Người hướng dẫn: B11 (bàn đạp hay F6 trên màn hình), B12 (nghĩa của `Gew.`), B13 (khi nào `letzter Pick` bật), B14 (Mehrfachpick "1/2" góc phải trên), B15 (`F4 Übersicht`), A9.
- Ghi chú cho mục khác: #28 ghi nguồn bấm nếu F6 bấm được; #30 vùng dwell WMS phải bao panel mới; #19 phép chia có dư không còn được chấm ở Complex.

## Bằng chứng (đã kiểm)
Spec `kommissionierung-dialog.md` (E13, E14, E50, dòng 10, 179), `_extracted/Kommissionierung/text.md` (slide 2 "Bestätigen mit Fuß", slide 9 Mehrfachpick), quyết định 001, 002, 003, 005, 006, 007; `OrderLine.cs`, `PackageData.cs`, `CalcForWMS.cs`, `PiecesContainerCounter.cs`, `MixPickingContainer.cs`, `OrderFeedback.cs:21`; scene inventory Basic/Mixed/Complex. Theo xr-ux-reviewer (tính, chưa kiểm): góc nhìn, cỡ chữ theo độ, panel ngoài tầm tay.

## Cập nhật 2026-11-03 (B14)
Đọc lại dialog cũ: ghi chú kèm ảnh (`docs/requirements/old/WMS explanation.txt`, mục 13) giải thích `Picks 2/4` là số lượt lấy từ **cùng một thùng kho** trước khi thùng về kho, không phải position của đơn. Vì vậy dự trù ở dòng *Nếu giả định sai* („Mehrfachpick có "1/2" góc phải trên → thêm ở #21“) **không áp dụng** để đánh số position: app không hiện „1/2“ cho position. Nếu sau này mô phỏng một thùng kho phục vụ nhiều lượt pick thì „1/2“ mới có chỗ. Câu hỏi tiếp: D26.
