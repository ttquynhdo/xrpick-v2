# Quyết định 034: Bảng „Das lernen Sie“ chia „Sie kennen:“ / „Sie können:“, ma trận mục tiêu cho research trong `analysis/`, chưa thêm đo lường

- **Backlog:** #86 (mở rộng)
- **Trạng thái:** accepted
- **Ngày:** 2026-11-05
- **Thay thế (một phần):** 027 mục 5 (câu dẫn cụm 1 „Sie verstehen …:“ = bài 2, cụm 2 „Sie können:“ = bài 3–5) và cách hiểu D33 (tên đầy đủ LVS ở tiêu đề cụm). Mọi phần khác của 027 (mục 6–8: lề, giãn dòng, khối vàng riêng, không che, mỗi cụm một key, chữ qua `localize-strings` + `german-reviewer`; mẫu thẻ mục tiêu #87) giữ nguyên.
- **Ý kiến đã xét:** unity-reviewer, requirements-analyst, data-analyst, xr-ux-reviewer, instructional-design-reviewer, german-reviewer, rồi decision-synthesizer.
- **Lựa chọn của người dùng (2026-11-05):**
  - Chấp nhận phương án A (bên dưới). Bỏ „Ablauf/Reihenfolge“ khỏi bảng người học.
  - U1: ở trạm thật người lấy hàng có so nhãn carton với Mat.-Nr trong LVS (người dùng quyết; người hướng dẫn chỉ xác nhận, D39).
  - U2: luồng làm việc lớn (xem dialog + thùng kho → lấy → xác nhận) là mục tiêu quy trình trong ma trận research, bằng chứng = thẻ bước 1-2-3; không thêm dòng trên bảng.
  - U3: `cartons_from_soll`, Complex `cartons`, `TOO_FEW`/`TOO_MANY`/`EMPTY` → P4a (tính số carton đầy); Complex `loose`, `MISSING_LOOSE`/`EXTRA_LOOSE` → P4b (tính phần lẻ). Cả hai là quy trình.
  - U4: dòng thời lượng nói cả tổng số bài: „Dauer: 5 Lektionen, je etwa 5 Minuten“ (nháp), không ghi tổng phút.
  - Chưa thêm quiz đo kiến thức khai báo; mở backlog riêng nếu luận văn cần.

## Bài toán
Người học (người Đức) và người làm research phải thấy khoá dạy cả kiến thức khai báo (biết các trường LVS, các thùng, Empf Kz) và kiến thức quy trình (lấy hàng đúng theo LVS). Bảng Welcome hiện là một khối 5 mục, không theo thứ tự bài. Decision 027 chốt câu dẫn „Sie verstehen …:“ cho cụm 1, nhưng ghép với mục Concept („ablesen und … vergleichen“) thì sai ngữ pháp: „verstehen“ chỉ nhận danh từ. Cả 4 câu đậm trên thẻ mục tiêu bài 2–5 đều là việc phải làm, nên chia hai cụm theo bài là sai khái niệm; phải chia theo loại kiến thức.

## Tài liệu nói gì (requirements-analyst)
- Kommissionierung slide 2–3: luồng xem dialog và thùng kho (Soll, Ist, Rest, EKZ, VPE) → lấy hàng → xác nhận bằng chân → xác nhận thùng đơn. Các trường chỉ được liệt kê, không ghi là thứ tự bắt buộc (D42).
- Slide 7–8 và Detail Description (mục Begriffe): định nghĩa Soll, Ist, Rest, VPE, EKZ, Lagerbehälter, Auftragsbehälter. Mọi mục khai báo trên bảng đều có cơ sở ở đây.
- So nhãn carton với Mat.-Nr: tài liệu không nói rõ (chỉ „Sichtung vom Lagerbehälter“). Người dùng quyết là có (U1).
- Dạy sai cần tránh: nói thứ tự đọc là bắt buộc; coi Ist là số đếm tay; coi Rest là lượng đã lấy.

## Các phương án
| | Đúng nghiệp vụ | Hiệu quả học | Tải nhận thức/UX | Chi phí/rủi ro |
|---|---|---|---|---|
| **A. „Sie kennen:“ ≤3 mục danh từ + „Sie können:“ 4 câu đậm; ma trận = `analysis/objectives.csv`; không đổi log (chọn)** | + | ++ mỗi mục được dạy và thực hành, cùng lời ở Welcome, thẻ mục tiêu, tổng kết | + khoảng 13 dòng | + chỉ CSV, 2 TMP, R join |
| B. Phác thảo người dùng 4 mục khai báo (thêm „den Ablauf am Kommissionierplatz“) | 0 | − hứa một mục không bài nào dạy dưới dạng khai báo (`Ablauf Overview` tắt từ #80) | − khoảng 14 dòng | + |
| C. Decision 027 nguyên văn | 0 | − chia theo bài | 0 | veto: câu dẫn + mục sai ngữ pháp |
| D. A + quiz Concept mới (Ist/Rest, thùng) | 0 | − chấm thứ khoá không thực hành; đoán trúng 33 %; chỉ nhớ tức thì | − Concept dài thêm | −− tăng `layout_version`, sửa spec, `make_fake_data.R`, selftest; dữ liệu pilot cũ NA |
| E. 5 mục đổi lời, phân loại chỉ ở research | + | − người học không thấy phân chia | ++ | ++ |

Bị bác thêm: hai cột (mục Complex tràn dòng, lấn Menu/giá thẻ); một TMP cho cả bảng (khó có khoảng hở giữa cụm); VerticalLayoutGroup (bảng tĩnh, chỉ 2 TMP); ma trận chỉ trong `docs/` (R không đọc được).

## Quyết định: phương án A
1. **Bảng Welcome** (chữ nháp, bản cuối qua `german-reviewer`):
   ```
   Sie kennen:
   • die Felder im Lagerverwaltungssystem (LVS)
   • Lagerbehälter und Auftragsbehälter
   • den Unterschied zwischen Empf Kz 003 und 000
   Sie können:
   • die Angaben im LVS ablesen und mit dem Etikett vergleichen
   • Kartons nach EKZ einlegen
   • mehrere Positionen nacheinander kommissionieren
   • ausrechnen, wie viele volle Kartons und lose Teile Sie brauchen
   ```
   EN „You know:“ / „You can:“. Cụm 1 tối đa 3 mục. Cụm 2 = câu đậm thẻ mục tiêu Concept, Basic, Mixed, Complex, theo thứ tự bài. Tên đầy đủ „Lagerverwaltungssystem (LVS)“ ở mục đầu tiên, sau đó chỉ „LVS“ (glossary: đầy đủ ở lần dùng đầu). Không mục nào nói tới thứ tự đọc.
2. Mỗi cụm một key và một TMP (thay `welcome.panel.goals`); khoảng hở giữa hai cụm lớn hơn khoảng giữa các dòng; câu dẫn in đậm như câu đậm thẻ mục tiêu. Không layout group, không hai cột.
3. Dòng thời lượng: „Dauer: 5 Lektionen, je etwa 5 Minuten“ (EN „Duration: 5 lessons, about 5 minutes each“), một dòng, xám, trên khối vàng (U4).
4. `complex.end.summary`: cùng các mục, cùng thứ tự, câu dẫn „Sie kennen jetzt:“ / „Sie können jetzt:“; dòng nút đỏ đứng riêng.
5. **Research:** `analysis/objectives.csv` (`objective_id` ASCII cố định, `knowledge_type` declarative|procedural, `taught_scene`, `practised_scene`, `evidence` rated|observed|none) + cột `objective_id` trong `item_skills.csv` và `skill_map.csv`. Một trang ma trận trong `docs/` chỉ trỏ tới CSV này. Mục tiêu `evidence = none` cho NA, không bao giờ 0 %. Không đổi `schema_version`/`layout_version`; `analyse_set.R` ra bảng theo `objective_id`/`knowledge_type`; `selftest.R` xanh; `data-analyst` duyệt. Nháp (người làm research chốt khi làm):
   | ID | Mục tiêu | Loại | Bằng chứng |
   |---|---|---|---|
   | D1 | Các trường LVS (Mat.-Nr, Bez., Verp.-Einh., Soll, Ist, Rest, Empf Kz) | declarative | none (tour chỉ cho thời gian tiếp xúc) |
   | D2 | Lagerbehälter và Auftragsbehälter | declarative | observed: `container_enter` `container` = `order` ở Concept |
   | D3 | Empf Kz 003 khác 000 | declarative | rated: quiz `ekz_003`, `ekz_000` |
   | P0 | Luồng xem LVS + thùng kho → lấy → xác nhận (U2) | procedural | observed: thẻ bước 1-2-3 ở Basic, Mixed, Complex (đi hết vị trí); không chấm thứ tự đọc từng trường |
   | P1 | Đọc LVS, so nhãn với LVS | procedural | rated: quiz `label_by_matnr`, `WRONG_MATERIAL` |
   | P2 | Xếp carton theo EKZ | procedural | rated: `EKZ_PLACEMENT` |
   | P3 | Nhiều vị trí nối tiếp | procedural | rated: lần thử đầu theo vị trí ở Mixed |
   | P4a | Tính số carton đầy từ Soll : Verp.-Einh. (U3) | procedural | rated: quiz `cartons_from_soll`, Complex `cartons`, `TOO_FEW`/`TOO_MANY`/`EMPTY` |
   | P4b | Tính phần lẻ (U3) | procedural | rated: Complex `loose`, `MISSING_LOOSE`/`EXTRA_LOOSE` |

   Ghi trong luận văn: `TOO_FEW`/`TOO_MANY` ở Basic có thể do tính sai hoặc đếm/đặt nhầm; log không tách được. Dữ liệu khai báo chỉ là nhận biết ngay sau khi dạy, 1 item mỗi mục tiêu, báo theo item.

## Được / mất
- **Được:** người học thấy ranh giới biết / làm ngay ở câu dẫn; câu dẫn + mục đúng ngữ pháp; mỗi mục được dạy và thực hành, gặp lại đúng lời trên thẻ mục tiêu và trong tổng kết; người học biết khoá có 5 bài; R phân tích theo loại kiến thức mà không đổi hợp đồng log.
- **Mất:** „thứ tự đọc LVS“ không xuất hiện với người học (chỉ trong ma trận); Ist, Rest, Bez. không có điểm chấm (giới hạn ghi trong luận văn); bảng cao thêm khoảng 4 dòng, chưa đo chỗ trống.

## Nếu giả định sai
- Bảng che Menu hoặc giá thẻ: cụm 1 còn 2 mục (bỏ „Lagerbehälter und Auftragsbehälter“, đã có trên `Box Sign` của thùng). Không giảm font.
- Mục Complex không vừa một dòng dù đã nới bảng: rút lời ở cả bảng và thẻ mục tiêu Complex cùng lúc, qua `german-reviewer`.
- Người hướng dẫn nói thứ tự đọc các trường là cố định (D42): thêm mục „den Ablauf am Kommissionierplatz“ (một key) khi đã có bài dạy nó.
- Luận văn cần điểm chấm cho khai báo: backlog riêng (quiz Concept + tăng `layout_version`, hoặc kiểm tra ngắn ngoài VR); `objectives.csv` chỉ đổi `evidence`.

## Câu hỏi mở
- D33 (sửa): tên đầy đủ LVS chỉ ở mục đầu tiên có đủ không. Giả định: đủ.
- D42: thứ tự đọc các trường trong dialog có cố định không.
- D39: xác nhận người lấy hàng so nhãn carton với Mat.-Nr (người dùng đã quyết: có).
