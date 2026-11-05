# Mục tiêu học của khoá (ma trận)

Decision: [034](decisions/034-welcome-goals-know-can-objectives.md) (#86). Nguồn duy nhất cho dữ liệu là [`analysis/objectives.csv`](../analysis/objectives.csv); trang này chỉ giải thích. Muốn đổi một mục tiêu, sửa CSV đó (và `objective_id` trong `analysis/item_skills.csv`, `analysis/skill_map.csv`), không sửa bảng dưới đây trước.

## Hai loại kiến thức
- **Khai báo (declarative, „Sie kennen:“):** biết một khái niệm hay thuật ngữ: các trường LVS, hai loại thùng, Empf Kz 003 / 000.
- **Quy trình (procedural, „Sie können:“):** làm được một việc: đọc LVS và so nhãn, tính số carton, xếp theo EKZ, lấy nhiều vị trí.

Người học thấy hai cụm này trên bảng „Das lernen Sie“ ở WelcomeScene (`welcome.panel.know`, `welcome.panel.can`) và lại trong tổng kết cuối khoá ở ComplexScene (`complex.end.summary`). Bốn mục „Sie können:“ dùng đúng lời câu đậm trên thẻ mục tiêu „Darum geht es“ của bài 2–5.

## Ma trận
| ID | Mục tiêu | Loại | Dạy | Thực hành | Bằng chứng | Nguồn trong log |
|---|---|---|---|---|---|---|
| D1 | Các trường LVS (Mat.-Nr, Bez., Verp.-Einh., Soll, Ist, Rest, Empf Kz) | khai báo | Concept (tour) | Basic, Mixed, Complex | **dạy, không đánh giá** | chỉ thời gian xem tour (`tour_steps.csv`) |
| D2 | Lagerbehälter và Auftragsbehälter | khai báo | Concept | Basic, Mixed, Complex | quan sát (làm, không chấm) | `container_enter` `container` = `order` ở Concept (layout „032“ trở đi) |
| D3 | Empf Kz 003 khác 000 | khai báo | Concept | Basic, Mixed | chấm | quiz `ekz_003`, `ekz_000` |
| P0 | Luồng xem LVS + thùng kho → lấy → xác nhận | quy trình | Welcome, thẻ bước 1-2-3 | Basic, Mixed, Complex | quan sát | `task_completed` ở cả 3 bài lấy hàng; **thứ tự đọc từng trường không chấm** |
| P1 | Đọc LVS, so nhãn carton với LVS | quy trình | Concept | Basic, Mixed, Complex | chấm | quiz `label_by_matnr` (`material_by_matnr` trước layout „032“), `WRONG_MATERIAL` |
| P2 | Xếp carton theo EKZ | quy trình | Concept | Basic, Mixed vị trí 1 | chấm | `EKZ_PLACEMENT` |
| P3 | Lấy nhiều vị trí nối tiếp | quy trình | Mixed | Mixed, Complex | chấm | lần thử đầu theo vị trí ở Mixed |
| P4a | Tính số carton đầy từ Soll : Verp.-Einh. | quy trình | Concept | Basic, Mixed, Complex | chấm | quiz `cartons_from_soll`, Complex `cartons`, `TOO_FEW`/`TOO_MANY`/`EMPTY` |
| P4b | Tính phần lẻ | quy trình | Complex | Complex | chấm | Complex `loose`, `MISSING_LOOSE`/`EXTRA_LOOSE` |

Kết quả theo mục tiêu: `analyse_set.R` ghi `output/objectives_summary.csv` (và `output/pilot/…`). Mục tiêu „không đánh giá“ luôn cho NA, không bao giờ 0 %.

## Giới hạn (ghi trong luận văn)
- Kiến thức khai báo chỉ đo được ngay sau khi dạy, mỗi mục 1 câu quiz A/B/C (đoán trúng 33 %); báo theo item, không tính điểm theo người. Không có bài kiểm tra trước/sau (#43 đã bỏ).
- Ist, Rest, Bez. và thứ tự đọc các trường (D42) được dạy nhưng không có điểm chấm.
- `TOO_FEW`/`TOO_MANY` ở Basic có thể do tính sai hoặc đếm/đặt nhầm; log không tách được, nên gắn vào P4a là cách hiểu hợp lý nhất, không chắc chắn.
- Không có nhóm đối chứng: không kết luận quan hệ nhân quả giữa kiến thức khai báo và kỹ năng quy trình.
- `P1` hiện chỉ có bằng chứng từ quiz Concept: `WRONG_MATERIAL` chỉ được tính ở scene có skill `check_material` trong `scene_skills.csv`.
