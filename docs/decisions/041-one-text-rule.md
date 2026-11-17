# Quyết định 041: Một luật chữ cho cả khoá: chữ chỉ có khi nói điều cảnh không tự cho thấy; thẻ bước 1 câu; Concept 2 thẻ, tour tự mở; câu hỏi nằm ngay trên A/B/C trong một cụm gần tay

- **Backlog:** #155 (gộp #144, #145, #146, #147; mở khoá phần câu hỏi của #154)
- **Trạng thái:** accepted
- **Ngày:** 2026-11-18
- **Ý kiến đã xét:** instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, german-reviewer, data-analyst (proposal mode, độc lập); decision-synthesizer. Không hỏi requirements-analyst vì #155 không đổi luật nghiệp vụ hay thứ tự bước.
- **Lựa chọn của người dùng (2026-11-18):** chấp nhận phương án B. Ba câu hỏi phụ lấy mặc định:
  1. Nếu sáng 19/11 có một mục chưa `verified-mcp`: với #147 thì dời freeze cho mục đó; với #146 thì ship build không có #146 rồi làm #146 **trước buổi study đầu tiên** (không đợt thu nào trộn layout).
  2. Tiêu đề thẻ mục tiêu `Darum geht es` giữ nguyên.
  3. Thanh tour Concept tự hiện sau 2 s cố định.
- **Thay thế (một phần):**
  - **027:** bỏ câu 2 „So …“ của thẻ mục tiêu; thêm luật độ dài thẻ bước (mục 3 dưới đây).
  - **036:** bỏ luật „bước nào cũng có dòng hành động“. Luật „mỗi `.action` nói việc + mục đích“ giữ, chỉ áp dụng khi bước có dòng hành động.
  - **024 mục 6–7 và 026 (#89):** Concept không còn thẻ 1 làm cổng mở tour; thanh tour tự mở 2 s sau khi vào scene.
  - **040 mục 2–3:** câu hỏi không còn ở mặt phẳng xa; ngoại lệ có lý do cho luật „chữ cần đọc ≥ 1 m“ (mục 5).
- **Giữ nguyên:** 040 mục 1 (chữ tour ở chú thích cạnh màn hình), 040 mục 3 phần `Caption example` (Complex, bên trái), 040 mục 4 (Menu, bàn phím), 040 phần bổ sung (chỗ gần tay ≈ (0,05; 1,01; 0,40), che ≤ 5 %); 017/035 (khung xanh, ✓), 032 (mục tiêu Concept), 038.

## Bài toán

Nhiều chữ có mặt chỉ vì decision bắt buộc, không vì người học cần (góp ý `docs/review-notes-checklist-2026-11-17.md`):
- R2: câu „So …“ trên thẻ mục tiêu ở 5 scene.
- R12: thẻ 1 Basic có 4 ô LVS, mỗi ô hai tên, thêm câu Rest không dùng ở bài này.
- R4: thẻ 1 Concept bắt đọc nhãn trước khi tour giải thích nhãn, và chỉ là cổng mở tour.
- R8: dòng hành động ở bước 8–9 lặp lại thẻ mẫu và chú thích.
- R11: câu hỏi ở cạnh màn hình, đáp án gần tay, cách nhau ≈ 45–50° (suy ra từ toạ độ): người học nhìn qua lại và đổi tiêu cự cho mỗi đáp án, nặng nhất ở `label_by_matnr` (so số 6 chữ số).

## Xung đột giữa các đề xuất

| Vấn đề | Một bên | Bên kia | Chọn |
|---|---|---|---|
| Chỗ đặt câu hỏi | câu hỏi ngay trên A/B/C, một cụm gần tay (ID, UX) | giữ 040, chỉ thêm „phần cần so“ trên nút (ID PA2) | một cụm: PA2 lặp chữ, vẫn nhìn qua lại |
| Độ cao cụm | giữ chỗ hiện nay, cúi ≈ −56° (UX PA1) | „bục đọc“ cao hơn, cúi ≈ −49° (UX PA2) | giữ chỗ; PA2 dễ che dialog > 5 % (040 đo ở −30° che 20–37 %) |
| Thời điểm làm | theo mức rủi ro, có thể hoãn #146/#147 (unity) | một `layout_version` "039" chỉ khi đủ #144–#147 (data) | một build, thứ tự làm theo rủi ro |
| Giới hạn thẻ bước | ≤ 1 câu, ≤ ~90 ký tự DE (ID, german) | ≤ ~15 từ, ≤ 3 dòng (UX) | cả hai: ký tự và số dòng |
| Rút dần theo bài | giới hạn giảm Basic → Mixed → Complex (ID PA3) | một giới hạn chung | chung; rút dần để sau freeze |
| Luật tách R | nhánh `if_else` theo layout (DA PA1) | bảng `analysis/layout_factors.csv` (DA PA2) | PA1; bảng để sau freeze |
| Rà toàn CSV | ngay (german PA2) | chỉ các chuỗi trong *Done when*, luật áp cho chữ mới (german PA3) | PA3; rà toàn bộ sau freeze |

## Quyết định (phương án B)

1. **Khi nào được có chữ** (cho mọi chữ người học thấy, 5 scene). Một dòng chữ chỉ được có khi làm ít nhất một việc:
   - (a) dạy một thuật ngữ hoặc quy tắc mà cảnh không tự cho thấy;
   - (b) đưa một con số hoặc trạng thái không có ở chỗ khác;
   - (c) giải thích *vì sao* sau một lỗi;
   - (d) chỉ chỗ cần nhìn khi chỗ đó nằm ngoài tầm nhìn lúc ấy (vd. bảng EKZ bên phải ở bước 9).

   Mỗi thông tin chỉ nói bằng chữ một lần: thẻ trên thùng, mũi tên, khung hay chú thích đã nói thì bỏ dòng chữ. Không chữ nào bắt đọc một thuật ngữ trước khi thuật ngữ đó được giới thiệu. Ô LVS chỉ gọi bằng nhãn LVS, không thêm tên thứ hai.
2. **Thẻ mục tiêu:** chỉ câu đậm „Sie lernen, … zu …“. Tiêu đề `Darum geht es` giữ.
3. **Thẻ bước:** tiêu đề + **1 câu** mệnh lệnh có Sie; thân ≤ 90 ký tự DE (không tính thẻ `<b>`/`<nobr>`) và ≤ 3 dòng ở bề rộng 200, cả DE và EN (đo bằng `InstructionShelfLayout` / `GetPreferredValues`). Một giới hạn cho Concept, Basic, Mixed, Complex; Welcome kiểm theo cùng giới hạn.
4. **Dòng hành động tour (thay 036).** Chỉ có khi bước yêu cầu một việc mà chú thích và marker chưa nói; khi có thì nói việc + mục đích. Bước không có dòng hành động thì `Action Row` (icon + chữ) tắt hẳn, không ô trống, không `{key}`. Cách đánh dấu „không có dòng“ không dựa vào ô DE trống.
5. **Câu hỏi (thay 040 mục 2–3).** Concept (4 câu) và Complex vị trí 1 (3 câu): „Frage n von …“ + câu hỏi (≤ 2 dòng) ngay trên hàng A/B/C trong một cụm, dòng phản hồi ngay dưới nút. Cụm ở chỗ gần tay hiện nay (≈ (0,05; 1,01; 0,40), nghiêng 50°), cao thêm theo phần câu hỏi.
   - **Ngoại lệ T1/T6 có lý do:** chữ của cụm được đọc cùng lúc với chỗ chạm; đáp án A/B/C ở đó vốn đã là chữ phải đọc.
   - **Ngưỡng của cụm:** chữ ≥ 0,8° ở khoảng cách thật (≈ ≥ 10 mm ở 0,72 m, suy ra); tầm với ngang ≤ 0,46 m; che thùng kho, thùng đơn và dialog ≤ 5 % ở tầm mắt 1,45–1,75 m; không nằm trên đường tay khi nhặt hàng; không chữ quay theo đầu.
   - Ở Complex, ngước lên LVS lấy Soll/Verp.-Einh. và nhìn `Caption example` là tải nội tại, giữ.
   - `Caption question` ở màn hình (Concept `Tour Markers/`, Complex `Screen Captions/`) không còn chữ câu hỏi. `*.quiz.how_to` „Drücken Sie A, B oder C …“ bỏ (luật 1); ô phản hồi trống tới khi trả lời.
6. **Concept (thay 024 mục 6–7, 026/#89).** 2 thẻ bước „1. LVS kennenlernen“, „2. Fragen beantworten“. Thanh tour mờ dần vào **2 s cố định** sau khi vào scene (`autoOpenSeconds` = 2); lần cầm carton không còn mở tour. Bước 1 chỉ log khi thanh thật sự hiện (không cần trường log mới). Khung xanh ở thẻ 1 từ đầu; mở câu hỏi → ✓ thẻ 1, khung sang thẻ 2; đúng cả 4 → ✓ thẻ 2, khung tắt. Mũi tên STECKER C vẫn chỉ carton ở bước `material`.
7. **Log.** Một lần `layout_version` "038" → "039", chỉ trong build có đủ #144–#147, cùng thay đổi: `docs/research-logging-spec.md`, `make_fake_data.R`, `analyse_set.R` (tách Concept; Complex `position_1` mức `quiz_layout` mới; tách `read_s`/`time_to_first_grab_s` của Basic/Mixed/Complex), `selftest.R` có tập trộn "038"/"039" và xanh; `data-analyst` duyệt. Giữ `question_id`, mã đáp án, `step_id`, `step_index`; câu hỏi đổi điều được hỏi hay đổi đáp án thì `question_id` mới. Câu bỏ khỏi thẻ mà là trợ giúp cho kỹ năng được chấm → một dòng `skill_scaffolds.csv` `from_layout` "039" (sơ bộ: không câu nào; data-analyst xác nhận).

## Được
- R11: hết nhảy mắt ≈ 45–50° kèm đổi tiêu cự cho mỗi đáp án.
- R4: thuật ngữ trên nhãn được dạy trước khi phải đọc nhãn; Concept bớt một thẻ và một cổng chờ (tới 60 s).
- R2, R8, R12: bớt chữ ở 5 scene; mỗi dòng còn lại kiểm được theo luật (a)–(d).
- #154 hết bị chặn: bên phải màn hình không còn câu hỏi, chú thích có thêm chỗ.

## Mất / chấp nhận đánh đổi
- Câu hỏi ở ≈ 0,72 m (vùng 0,5–0,8 m của T1), cúi ≈ −56° (vượt mức tốt nhất của T6). Chấp nhận vì hiệu quả học (spatial contiguity khi so số) đứng trên tải UX.
- Cụm cao hơn có thể che nhiều hơn (chưa đo) → xem „Nếu giả định sai“.
- Thẻ 1 Basic mất định nghĩa Rest: Rest vẫn dạy ở bước tour `rest` của Concept; Basic không chấm Rest.
- Thẻ mục tiêu mất lý do (vd. „unbeschädigt“ ở `basic.task.intro`); lý do học vẫn ở `Lernziele` của Welcome.
- Bước 8 không còn câu chỉ „links neben dem Bildschirm“; thẻ mẫu 000 nằm ngay cạnh chú thích Empf Kz.
- Concept bỏ cổng „cầm trước“: có thể bấm „Weiter“ mà chưa cầm carton; bước `material` vẫn bảo cầm và mũi tên vẫn chỉ.
- Công M–L sát freeze; rủi ro ở `ConceptCardTracker`/test, đổi parent cụm quiz, `Surface`/`BoundsClipper`. Tới freeze tối đa `verified-mcp`.
- Dữ liệu "038" (pilot P8xx) không làm mốc cho thời gian đọc thẻ, thời gian quiz và Concept của "039".

## Phần vào build freeze 19/11 / phần làm sau
- **Build freeze (một build "039"):** #144, #145, #146, #147; phần log/R mục 7; đã chạy `LocalizationBinder.ImportCsv`, `InstructionShelfLayout`, `PokeButtonFixer`, `TextComfortAudit` ở các scene liên quan; ảnh `instructions` 5 scene và `quiz` Concept/Complex, DE và EN. Thứ tự làm: CSV (#145, #144 + tắt `Action Row`) → #147 → #146.
- **Sau freeze (không đổi layout):** giới hạn giảm dần theo bài (ID PA3); `analysis/layout_factors.csv`; ghi luật vào `docs/i18n/README.md` và rà toàn bộ CSV; phần còn lại của #154; test kính (bắt buộc).

## Nếu giả định sai
- **Cụm che > 5 % hoặc chữ < 0,8°:** lần lượt nới cụm sang ngang; rút câu hỏi về ≤ 2 dòng (`localize-strings`, german-reviewer); nghiêng thêm hoặc hạ cụm. Vẫn không đạt → người dùng chọn cho riêng câu đó giữa mức che và cách tách của 040. Không dùng phương án „một dòng mấu chốt trên nút“.
- **Test kính: câu hỏi ở −56° khó đọc:** nâng/nghiêng cụm theo hướng „bục đọc“, trong giới hạn che ≤ 5 %. Luật „câu hỏi ngay trên đáp án“ giữ.
- **Log cho thấy nhiều người qua bước `material` mà không cầm carton:** chỉ mở „Weiter“ ở bước đó sau lần cầm đầu tiên, kèm trường log `opened_by` và layout mới. Không quay lại thẻ 1.
- **Người học không tìm thấy ví dụ ở bước 8:** thêm „links“ vào `concept.tour.empf_kz_000.body`, không thêm lại dòng hành động.
- **2 s quá ngắn/dài:** đổi hằng số cùng `layout_version` và hằng số trong R.
- Mọi thay đổi đảo ngược được bằng CSV và field; không xoá script hay `.meta`; bỏ thẻ Concept bằng deactivate/xoá GameObject; field đổi tên có `[FormerlySerializedAs]`.

## Câu hỏi còn mở
- Cho người hướng dẫn (không chặn): câu A-mã trong `docs/requirements/questions-for-instructor.md` về Rest khi lấy hàng ở trạm thật (bỏ câu Rest khỏi thẻ Basic có làm mất điều gì về nghề không).

## Bằng chứng (chính)
- `WmsFieldTour.cs:85` (`autoOpenSeconds` = 60), `:202–221`, `:292–296` (mở tour); `:424–437` (luôn ghi `Loc.Get(…action)`, không tắt row) — đã kiểm.
- `PackageQuizController.cs:76–95, 220–225`, `RemainderQuizController.cs:74–75, 182` (field câu hỏi serialized, `RequiredReference`) — đã kiểm.
- `docs/i18n/strings-todo.csv`: `basic.task.intro` (câu „So …“), `basic.step1.text` (tên kép + Rest), `concept.card.step1–3_*`, `concept.tour.empf_kz_000/003.*` („wie im Beispiel“, thiếu „sich“), `concept.tour.material.action` (cầm STECKER C), Complex thẻ 1 / lời nhắc „rechts neben dem Bildschirm“, `concept.quiz.label_by_matnr.q` — đã kiểm.
- `docs/text-comfort-checklist.md` T1, T6; `docs/decisions/040-read-far-touch-near.md` (chỗ gần tay, che ≤ 5 %, đo ở −30°) — đã kiểm.
- Góc 25°/−14° tới câu hỏi, 7°/−56° tới A/B/C, cỡ ≥ 10 mm ở 0,72 m — suy ra, đo bằng `TextComfortAudit` khi làm.
- Mức che của cụm cao hơn, độ dài DE thật trong cụm — chưa đo.
