# Sửa log grab_start giả khi carton hút vào ô snap

- **Nhánh / commit:** `main` (chưa commit), dựa trên `727eee1`
- **Ngày:** 2026-11-05
- **Phát hiện từ:** buổi pilot P802 (`docs/pilot-P802-summary.md`, mục 4): Basic có 15 `grab_start` nhưng chỉ 9 `grab_end`, 6 lần start xuất hiện < 0,1 s sau grab_end của chính carton đó; Mixed có 3 lần tương tự.
- **Nguyên nhân:** `SnapInteractor` (ISDK) gửi `Select` của nó vào `Grabbable` của carton suốt thời gian carton nằm trong ô snap. `PackageGrabLogger` coi đó là một lần cầm.
- **Sửa:** `Assets/Scripts/PackageGrabLogger.cs`: bỏ qua sự kiện có `evt.Data is SnapInteractor` (`_data` của mọi SnapInteractor ở Basic (9) và Mixed (6) để trống, nên `Data` = chính interactor).

- **Review `data-analyst`:** không có lỗi chặn. Spec (`grab_start`, `grabs`) đã sửa trong cùng thay đổi. Không tăng `layout_version` (nếu tăng thì dữ liệu Concept của "033" và "034" sẽ bị tách riêng dù Concept không đổi). Việc cho R bỏ các `grab_start` giả trong log cũ và đếm chúng vào `data_quality.snap_grab_artifacts` (với P802 là 9) để làm sau, cùng phần học R (#102).

## Kết quả test
| Case | Ai chạy | Kết quả | Bằng chứng |
|---|---|---|---|
| G1 Basic: gửi Select như SnapInteractor vào `Grabbable` của carton → không có `grab_start` | MCP | trước: ⏳ · sau: ⏳ chưa chạy (Editor không mở) | – |
| G2 Basic: Select/Unselect kiểu tay → 1 `grab_start` + 1 `grab_end` | MCP | ⏳ chưa chạy | – |
| G3 Mixed: như G1 với carton KABELVERBINDER A | MCP | ⏳ chưa chạy | – |
| G4 Tay thật: thả carton vào ô snap ở Basic rồi lấy ra → mỗi lần cầm đúng 1 start + 1 end | Bạn | ⏳ | – |
| B3 / M3 6 (3) carton vào ô → xác nhận đúng, thùng chạy đi | Bạn | ⏳ | – |
| A1 Console không lỗi đỏ | MCP | ⏳ chưa chạy | – |
| G5 Tay thật: cầm lại một carton đang nằm trong ô snap rồi thả ra ngoài → đúng 1 `grab_start` + 1 `grab_end`, `duration_s` ≈ thời gian cầm trên tay (data-analyst đề nghị: đường này chưa từng thấy trong log thật) | Bạn | ⏳ | – |
| (code) chỉ bỏ qua sự kiện của SnapInteractor, sự kiện tay không đổi | code | ✅ | `Assets/Scripts/PackageGrabLogger.cs` (`HandlePointerEvent`, dòng đầu) |
