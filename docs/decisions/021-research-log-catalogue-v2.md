# Quyết định 021: Danh mục sự kiện log nghiên cứu v2 khớp với bài tập hiện tại, cách chia #28–#30

- **Backlog:** #28, #29, #30 (+ mục mới #74 tách từ #29)
- **Trạng thái:** accepted
- **Ngày:** 2026-11-04
- **Ý kiến đã xét:** data-analyst, unity-reviewer, instructional-design-reviewer, xr-ux-reviewer → decision-synthesizer. Không hỏi requirements-analyst (không đổi quy tắc nghiệp vụ) và german-reviewer (log không thêm chữ cho người học).
- **Lựa chọn của người dùng (2026-11-04):** phương án A.
- **Sửa quyết định đã chấp nhận:** 011, chỉ phần đo hướng nhìn: collider vùng nhìn trên giá/monitor vẫn giữ, nhưng #30 không ray vào chúng nữa (lý do ở „Mất“). 019 không đổi (mốc start/arrived, `layout_version`, một build mỗi đợt thu đều nằm trong danh mục dưới đây).

## Bài toán
Danh mục sự kiện (`docs/research-logging-spec.md` §4–§6), `analysis/skill_map.csv`, `analysis/scene_skills.csv` và `analysis/make_fake_data.R` viết ngày 2026-11-01, trước khi: bỏ „loại gói“ GG/VPE (006/007), Mixed và Complex làm từng position (012, 016), hai quiz chấm ngay khi chạm (016, 020), kiểm tra EKZ 3 ở Basic (003, #41), thùng chạy tự động trên ba làn (019), Menu khoá theo bài (#42). Các dòng „Bổ sung“ của #28–#30 chưa thống nhất với nhau. Cần chốt thu gì, bỏ gì, thêm gì, chia việc thế nào, trước người tham gia thật đầu tiên.

Phát hiện chính: `scene_skills.csv` vẫn gán `check_material` cho Mixed và Complex, nơi thùng kho chỉ chứa vật tư của position hiện tại (D12, #71), nên tỷ lệ lỗi luôn 0 % và biểu đồ Q1 sẽ vẽ ra „tiến bộ“ giả.

## Xung đột giữa các đề xuất
| Vấn đề | Bên A | Bên B | Đánh đổi |
|---|---|---|---|
| Lần bấm nút đỏ không được chấm | data: `confirm_pressed` + `rated:false` | xr-ux / ID / unity: sự kiện riêng | Một sự kiện có cờ (R lọc ở mọi mẫu số) ↔ `confirm_count` giữ nghĩa |
| Đo hướng nhìn | spec §4.5, 011, data B: ray vào collider, `gaze_target_*` | xr-ux A: chỉ `_pose.csv`, R gán vùng theo góc | Dwell có sẵn trong app ↔ tính lại được sau, không Inspector |
| #29 hay #30 trước | data: #30a trước | unity, ID, xr-ux: #29 rút gọn trước | Q3 (hạng 3) ↔ Q4 (hạng 4) |
| Mốc bước 1/2/3 | xr-ux `step_changed`, unity `first_grab` | data: R dựng lại | Dễ đọc ↔ một nguồn |
| Gán kỹ năng | ID PA 3: `skill` trong sự kiện | data, ID PA 1: CSV trong `analysis/` | Phải build lại ↔ trưởng nhóm tự sửa |

## Các phương án
- **A. Hợp đồng v2, bump schema một lần; #28 → #29 rút gọn → #30 pose/perf; vùng nhìn tính bằng R:** chọn.
- **B. Như A nhưng #30 sinh `gaze_target_*` bằng collider:** vùng sát nhau 5–22° không tách được; thùng di chuyển; Inspector 4 scene. Dự phòng (xem „Nếu giả định sai“).
- **C. Tối thiểu: #28 + mốc máy + `tour_step`, hoãn #29/#30:** thiếu Q3 chi tiết và Q4. Dự phòng nếu hết thời gian.
- **D. Song song theo bộ phận:** trường chung dễ lệch, bump nhiều lần, một dev. Bác.

## Quyết định: phương án A

### Header
Thêm `layout_version` (011, 019) và một trường liệt kê nhóm sự kiện build này phát (để R tách NA với 0). `schema_version` = 2.

### Phiên và điều hướng
- `scene_enter.via`: `start` / `menu` / `resume` / `facilitator_end`. Ghi từ `Start` trở đi.
  - **Ghi chú khi làm #29 (2026-11-04, data-analyst):** bỏ `facilitator_end`. `Beenden` kết thúc phiên (`session_end.reason` `facilitator`) trước khi nạp WelcomeScene, nên không có `scene_enter` nào sau đó; phiên kế bắt đầu bằng `start`. Giá trị này chưa từng được ghi, nên không cần bump schema.
- `menu_select`: giữ `target_scene`, bỏ `item_label`, thêm `blocked` (chỉ khi relay pointer lúc chạy làm được không cần Inspector/sửa SDK; không thì bỏ trường, chỉ cộng thêm nên không chặn).
- Thêm `end_control_opened`, `end_control_cancelled` (#44).

### Kết quả (#28)
- `confirm_pressed` chỉ cho lần bấm **được chấm**, ghi **trước** `AdvanceToNextLine`: dạng dòng đơn `{item (material_key), soll, vpe, required_full, required_loose, placed_full, placed_loose}` + vật tư lạ `{item, count}` (bỏ hẳn khoá `gg/vpe/gga/ggb/loose/other`); `position`, `position_count`, `attempt_in_position`; giữ `attempt_no` (theo lượt scene), `correct`, `error_codes`, `time_since_scene_enter_s`.
- Thêm `confirm_ignored {reason: station_moving | completed | quiz_first, position}` cho các nhánh trả về sớm (`PiecesOrderValidator.ConfirmOrder`, `MixedPickingValidator.ConfirmOrder`, nhắc #58). Không là lần thử, không vào `confirm_count`.
- Thêm `position_completed {position}` (từ `LineAdvanced`, và một lần cho position cuối ở `OrderCompleted`).
- `task_completed`: Basic/Mixed/Complex ở `OrderCompleted`; Concept ở `LessonCompleted`. `attempts_to_correct` = tổng lần thử được chấm. Complex gọi `EndSession("completed")` ở `OrderCompleted` (`Congratulations` đã bật trước).
- `quiz_answer {quiz: concept | complex_remainder, question_id, selected (khoá đáp án), correct, attempt_in_question, error_codes}` ở mọi lần chạm được chấm, từ `AnswerChosen` của `PackageQuizController` và `RemainderQuizController`. `question_id`: Concept `cartons_from_soll`, `material_by_matnr`, `ekz_003`, `ekz_000`; Complex `cartons`, `loose`.
- Thêm `quiz_unlocked` (`PackageQuizController.Unlocked`) và `tour_step {step_id, step_index}` (`WmsFieldTour.StepShown`; „Von vorn“ = `step_index` về 1).
- Thêm `station_move_start` / `station_move_arrived {what: storage_out | storage_in | order_box, position}`, thứ tự `confirm_pressed` → `position_completed` → start → arrived (gồm thùng 1 của Complex tới sau quiz). `PositionStockSwitcher` và `OrderBoxConveyor` cần event mới chỉ để log, không đổi hành vi.
- Thêm `welcome_practice_done {action: grab | button}`.

### Mã lỗi
- Bỏ `WRONG_TYPE` (enum giữ là reserved, không bao giờ ghi) và `NO_ANSWER` khỏi spec, `skill_map.csv`, dữ liệu giả.
- Thêm `EKZ_PLACEMENT` (Basic), đứng sau các mã của `OrderRule`. `correct == (error_codes rỗng)` vẫn giữ.
- `EMPTY` ở position > 1 = không có gói nào của position hiện tại.

### Thao tác (#29 rút gọn)
- `grab_start` / `grab_end {object_id, material_key, duration_s}`, đăng ký lúc chạy cho mọi `Grabbable` có `PackageData` theo mẫu `InstructionStepHighlighter` (không gắn `GrabLogger` qua Inspector). `hand` / `input` nếu SDK đọc được, không thì `null`.
- `container_enter` / `container_exit` chỉ khi HashSet đổi; vật trên băng chạy không sinh `exit` giả.
- Bỏ `button_touch`, `info_shown`, `end_pos`. Không thêm `step_changed` / `first_grab` (R dựng từ `grab_start` + `position_completed`).
- Tách sang #74 (chỉ test được trên Quest, không chặn thu dữ liệu): `object_dropped_floor`, `input_mode_change`, `tracking_lost` / `tracking_regained`, `hand` nếu cần thêm công.

### Hướng đầu, tư thế, hiệu năng (#30)
- Bỏ `gaze_target_enter/exit` khỏi app. R gán vùng thô từ `_pose.csv` theo bảng góc của từng `layout_version`: picking `screen_column` (WMS + quiz + Beispiel), `shelf`, `desk_left`, `desk_right`, `confirm_area` (nút + thẻ kết quả), `menu`; Concept `tour`, `ekz`, `flow`, `labelboard`; ngưỡng 200 ms là tham số R. Không còn vùng riêng `order_box` / `storage_box` (nhìn thùng là việc của chính nhiệm vụ).
- `perf_sample` mỗi 5 s; `_pose.csv` 10 Hz, chỉ đầu, qua stream mở sẵn, flush/đóng khi pause, đổi scene, kết thúc phiên.
- Bộ lấy mẫu pose/perf nằm ngoài `Research/` (Assembly-CSharp, timer/coroutine) và gọi logger một chiều; `Research/` không có hàm chạy theo frame.

### Summary CSV
Thêm `confirm_ignored` (số lần); bỏ `dwell_wms_s`, `dwell_instructions_s` (R tính trong file rộng); `confirm_count` chỉ đếm lần bấm được chấm.

### Spec §7
Thay ghi chú „feedback lộ đáp án“ (lỗi thời sau #14) bằng: quiz Complex cố ý đưa sẵn con số ở position 1 (scaffold).

### Map kỹ năng (`analysis/`)
- `skill_map.csv`: xoá `WRONG_TYPE`, `NO_ANSWER`; sửa mô tả `WRONG_MATERIAL` theo khoá vật tư; thêm `EKZ_PLACEMENT → ekz_placement`; `WRONG_ANSWER` lấy kỹ năng theo câu hỏi; bỏ kỹ năng `package_concept`.
- `scene_skills.csv`: Concept `calculate_quantity`, `check_material`, `ekz_placement`; Basic `calculate_quantity`, `ekz_placement`; Mixed `calculate_quantity`; Complex `calculate_quantity`, `calculate_loose`. Bỏ `check_material` ở Mixed/Complex.
- File mới `item_skills.csv` (`scene, item, skill, scaffolded`), trưởng nhóm nghiên cứu sửa được không build lại. Đề xuất ban đầu (chờ D28, D29):

| scene | item | skill | scaffolded |
|---|---|---|---|
| ConceptScene | `cartons_from_soll` | calculate_quantity | |
| ConceptScene | `material_by_matnr` | check_material | |
| ConceptScene | `ekz_003`, `ekz_000` | ekz_placement | |
| ComplexScene | `cartons` | calculate_quantity | |
| ComplexScene | `loose` | calculate_loose | |
| ComplexScene | position 1 | | true |
| ComplexScene | position 2 | | false |

- Chỉ số chính của Q1: **tỷ lệ đúng ngay lần đầu theo mục** (câu hỏi / position). Tỷ lệ lỗi theo lần bấm là chỉ số phụ (quiz A/B/C đoán đúng 33 %).

### Thứ tự làm
1. #28 bước 1 (chỉ giấy tờ + R): hợp đồng v2 cho toàn bộ danh mục trên (cả trường của #29/#30), CSV kỹ năng, dữ liệu giả mô phỏng Mixed/Complex 2 position + 2 quiz + EKZ + `confirm_ignored` + mốc máy; `selftest.R` xanh; data-analyst duyệt.
2. #28 bước 2: cắm sự kiện vào validator, quiz, tour, `PositionStockSwitcher`, `OrderBoxConveyor`, Welcome, `EkzPlacementCheck`; điền summary.
3. #29 rút gọn (test bằng MCP).
4. #30 pose/perf (đo trên Quest) + bảng vùng trong R + một lần ghi kiểm chứng trên kính.
5. #74 (không chặn).

Không thu dữ liệu thật trước khi xong bước 1–4.

**Được:** một lần bump schema cho cả #28–#30; Q1/Q2/Q5 đo theo mục, có lần thử và kết quả lần đầu; ở Complex phần tính (quiz) và phần nhặt đo riêng; Q3 trừ được thời gian máy chạy; Q4 có pose và perf; cách gán vùng nhìn sửa được sau khi đã thu.

**Mất / chấp nhận:** không có dwell trong `summary.csv`; collider vùng nhìn của 011 giữ nhưng không dùng (vùng quiz/Beispiel/WMS sát nhau 5–22°, thùng di chuyển từ 019, gán trong R tính lại được); hướng đầu chỉ được coi là „đang đọc“ sau lần thử trên kính; tay/tracking/rơi sàn đến muộn hơn (#74).

**Bị bác:** song song theo bộ phận (D); tối thiểu (C, dự phòng); ghi khoá menu ở `scene_enter` (`menu_select.blocked` trực tiếp hơn); `first_grab` / `step_changed` (trùng nguồn); `rated:false` trong `confirm_pressed` (thay bằng `confirm_ignored`, cùng nội dung); #30a trước #29 (Q3 xếp trên Q4; đổi thứ tự vẫn chấp nhận được); `gaze_target_*` bằng collider (B, dự phòng); `skill` trong sự kiện; vùng `order_box`/`storage_box`; `button_touch`, `info_shown`, `item_label`, `end_pos`.

## Done when
Xem `docs/backlog.md` #28, #29, #30, #74. Các „Bổ sung“ cũ của #28–#30 đã được hợp nhất vào quyết định này.

## Nếu giả định sai
- Hướng đầu không đủ để biết người học đọc gì (thử trên kính thất bại): thêm `gaze_target_*` bằng collider của 011 như trường cộng thêm, không bump schema.
- Trưởng nhóm đổi map kỹ năng: chỉ sửa `item_skills.csv` / `scene_skills.csv`.
- Không làm được relay cho `menu_select.blocked`: bỏ trường.
- Bộ lấy mẫu 10 Hz làm tụt FPS: giảm 5 Hz, ghi vào spec.

## Ngoài phạm vi
Câu „lý do kết thúc nào được tính là đã hoàn tất“ (Bổ sung #27/#44 trong #28) không thuộc quyết định này; chốt riêng trước người thật.

## Câu hỏi cho trưởng nhóm nghiên cứu
D28 (map câu hỏi/position → kỹ năng, position 1 của Complex có tính cho `calculate_quantity` không), D29 (`ekz_placement` có là kỹ năng riêng trong Q1 khi chỉ có 2 điểm đo không).
