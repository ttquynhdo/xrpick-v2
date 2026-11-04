# Quyết định 022: Không ghi pose, perf, tay và tracking trong app; không cài thêm gì lên Quest

- **Backlog:** #30, #74
- **Trạng thái:** accepted (người dùng chọn 2026-11-04)
- **Ngày:** 2026-11-04
- **Thay thế:** 021, chỉ phần „Hướng đầu, tư thế, hiệu năng (#30)“ (`_pose.csv` 10 Hz, `perf_sample`, vùng nhìn gán từ pose). Mọi phần khác của 021 giữ nguyên.
- **Ý kiến đã xét:** unity-reviewer, xr-ux-reviewer, data-analyst, decision-synthesizer (bản nháp „phương án A: stream pose mở sẵn trên main thread, đo ABAB trên Quest“).

## Bài toán
#30 sẽ thêm vào app một bộ lấy mẫu chạy theo frame (pose 10 Hz, perf mỗi 5 s) và một stream ghi file mở suốt phiên. Cả ba chuyên gia đều ghi „chưa có số đo trên Quest“: chi phí ghi file mỗi giây, GC và nhịp lấy mẫu chỉ biết được sau khi đo. Bản nháp khuyên làm A rồi đo; nếu giật thì chuyển sang luồng ghi nền (B), rủi ro mất dòng khi kính ngủ và code đa luồng.

## Lựa chọn của người dùng
Người dùng muốn trải nghiệm mượt nhất và không có rủi ro nào trong đợt thử nghiệm, và chấp nhận không có dữ liệu pose/perf trong log. Vì vậy:

- App **không** có bộ lấy mẫu pose hay perf, không có `_pose.csv`, không có sự kiện `perf_sample`. Không thêm code chạy theo frame nào cho logging.
- Header `event_groups` không có `pose`, `perf`; R coi chúng là NA (đã có quy tắc NA ≠ 0). Cột summary `fps_avg`, `dropped_frames` luôn trống.
- Không cài thêm app nào lên Quest (người dùng chọn 2026-11-04, sau bản đầu của quyết định này đã nêu OVR Metrics Tool). Độ mượt được kiểm bằng mắt trong buổi thử trước đợt thu: người thử cầm/thả kiện, bấm nút đỏ, xem thùng chạy, đổi scene, và báo nếu thấy giật.
- Đường ghi sự kiện #26–#29 không đổi (thưa; `app_pause`, `scene_exit`, `session_end` ghi chắc xuống đĩa). Không đổi gì chỉ để phòng xa: mọi thay đổi code ghi file cũng là một rủi ro mới.

## Được
- Không có chi phí theo frame, không ghi file liên tục, không đa luồng, không rủi ro mất dòng mới.
- Không sửa code hay scene nào; chỉ sửa tài liệu.
- Không có thêm app hay bước cài đặt nào trên kính.

## Mất
- **Q4** (SSQ ↔ rớt frame / cử động đầu): chỉ còn SSQ trên giấy và dấu hiệu gián tiếp trong log (#26: `hmd_removed_count`, `pauses`, `session_end` `facilitator` + ô Abbruchgrund trên phiếu, thời gian học). Không có số rớt frame và cử động đầu cho từng người học.
- **Q3** (thời gian đi đâu): mất phần „nhìn WMS bao lâu“ theo hướng đầu. Vẫn còn `read_s` / `pick_s` / `check_s` từ #29 (`process_by_position.csv`).
- Không có vùng nhìn (spec §4.5) và không có quãng đường đầu đi.

## Nếu giả định sai
- Buổi thử thấy giật đúng lúc cầm/thả hoặc bấm nút đỏ: mở mục mới để ghi sự kiện qua stream mở sẵn hoặc luồng nền, có đo lại; không tự thêm pose/perf.
- Nhóm nghiên cứu cần Q4 đầy đủ về sau: chạy lại `solution-decision` cho #30 (bản nháp A ở trên là điểm bắt đầu).

## Bổ sung: #74 cũng bỏ (người dùng chọn 2026-11-04)
Theo cùng quy tắc „mượt nhất, không rủi ro“: không thêm code bắt va chạm sàn hay đọc trạng thái tracking của Meta SDK trong lúc học. `hand` / `input` trong `grab_*` luôn `null`; không có `object_dropped_floor`, `input_mode_change`, `tracking_lost` / `tracking_regained`; cột `drops_floor` luôn trống. Mất: số lần làm rơi kiện (chỉ đoán qua re-grab và `container_exit` của #29) và cờ „chậm do mất tracking“ cho Q6. Người điều phối ghi tay trên phiếu nếu thấy kính mất tay nhiều. Không câu hỏi nghiên cứu nào lấy dữ liệu chính từ #74.

## Ảnh hưởng
- Backlog #30, #74: `dropped` (theo decision này).
- Spec: §4.5, §4.6 (`perf_sample`), §4.7 ghi là không làm; §5 `fps_avg`, `dropped_frames` luôn trống.
- R: không cần sửa (nhóm `pose`/`perf` không có trong header → NA).
