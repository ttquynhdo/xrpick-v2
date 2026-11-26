# Quyết định 023: Dải mã pilot P800–P899, chọn được ngôn ngữ, phân tích riêng

- **Backlog:** #75
- **Trạng thái:** accepted (người dùng chọn phương án A, 2026-11-04)
- **Ngày:** 2026-11-04
- **Thay đổi:** quy tắc mã ở spec §2 mục 6 và quy tắc ngôn ngữ ở spec §9 (#11). Mã thật trước đây là `P001`–`P899`, nay là `P001`–`P799`.

## Bài toán
Người dùng chạy một **pilot bằng tiếng Anh**: chính mình đóng vai người học, rồi một người bạn vào thứ 6 2026-11-06. Dữ liệu pilot được phân tích và là cơ sở cải tiến cho round 3. Tiếng Đức dành cho người học cuối thật và cho lượt thử cuối của expert người Đức.

App trước đó không cho làm điều này:
- Tiếng Anh chỉ chạy được với mã thử `P9xx`. Mã thử ghi vào `practice/`, mở hết Menu, và `analyse.R` bỏ đi.
- Mã thật luôn bị ép về tiếng Đức.

## Phương án đã xét
- **A (chọn):** thêm một dải mã pilot riêng.
- **B:** cho mọi mã thật chọn tiếng Anh, rồi R tách theo cột `language`. Bị loại vì nếu người điều phối quên chọn Deutsch, một người học Đức thật sẽ chạy bằng tiếng Anh, và dữ liệu đó lẫn vào đợt thật.

## Quy tắc
| Nhóm | Mã | Ngôn ngữ | Thư mục log | Menu / học tiếp / cảnh báo | Phân tích |
|---|---|---|---|---|---|
| Đợt thật | `P001`–`P799` | luôn `DE` | `research/` | như người học | `output/` |
| Pilot | `P800`–`P899` | theo lựa chọn trên bàn phím | `research/pilot/` | như người học | `output/pilot/`, riêng |
| Thử (dev) | `P000`, `P900`–`P999` | theo lựa chọn | `research/practice/` | Menu mở hết, không cảnh báo | bị loại |

- Dạng log không đổi. R nhận ra pilot qua dải mã, và cảnh báo khi một mã nằm sai thư mục.
- Lượt cuối của expert người Đức cũng dùng mã pilot, với Deutsch.
- Dev test ca „đã có phiên kết thúc“ vẫn dùng `P899`, và xoá file ngay sau đó (giờ nằm trong `research/pilot/`).

## Được / mất
- **Được:** log pilot tiếng Anh dùng được đầy đủ, có biểu đồ, không bao giờ trộn với đợt thật. Mã thật vẫn bắt buộc tiếng Đức.
- **Mất:** dải mã thật ngắn lại còn 799 mã (đủ cho nghiên cứu). Phiếu giấy đã in sẵn mã `P800`–`P899` (nếu có) phải in lại.

## Sửa đổi 2026-11-26 (#184)
Người dùng đổi thiết kế study: mỗi người tham gia tự chọn bản tiếng Đức hoặc tiếng Anh. Mã thật `P001`–`P799` không còn bị ép `DE`, mà giữ ngôn ngữ chọn trên bàn phím như mã pilot và mã thử. Dải mã, thư mục log và cách tách pilot giữ nguyên. Rủi ro đã nêu ở phương án B (người điều phối quên chọn Deutsch) nay được chấp nhận: hướng dẫn người điều phối bảo hỏi người tham gia, chọn nút trước khi gõ mã và ghi ngôn ngữ vào phiếu; R đọc `language` của từng phiên làm biến nghiên cứu, không còn cảnh báo `language_not_de` cho study (việc của mục R kế tiếp).
