# Quyết định 026: ConceptScene có một „bàn học“ dưới màn hình (thanh tour ở giữa chứa chữ của bước, đổi qua lại tour ↔ câu hỏi), thẻ nhãn cạnh kiện đang cầm, vật chỉ để xem trông khác kiện thật; cụm câu hỏi ở Concept và Complex thu lại khi xong

- **Backlog:** #89, #90, #91, #92, #93 (một quyết định chung)
- **Trạng thái:** accepted
- **Ngày:** 2026-11-04
- **Thay thế (một phần):** 024 (mục 1, 3, 4, 7, 8, 9), 025 (thẻ nhãn khi cầm, „không khoá tour“), 020 (mục 3), 016/#85 (lúc ẩn cụm Complex). Phần còn lại của các quyết định đó giữ nguyên.
- **Ý kiến đã xét:** unity-reviewer, data-analyst, xr-ux-reviewer, instructional-design-reviewer → decision-synthesizer. Không hỏi requirements-analyst (không quy tắc nghiệp vụ nào đổi, chỉ đổi chỗ, lúc hiện và hình dạng). Mọi chuỗi mới qua `localize-strings` + `german-reviewer` khi làm.
- **Lựa chọn của người dùng (2026-11-04):** chấp nhận phương án B, Complex theo C1; thanh tour hiện khi cầm đủ 4 loại kiện; lối dự phòng tự hiện sau 60 s; câu hỏi ở bước 11 mở bằng nút „Zu den Fragen“. Mặc định cho phần chưa hỏi: `Stock STECKER C` giảm còn 1–2 carton + „Nur zum Ansehen“ khi chạm (chọn cuối cùng sau khi thử kính); R tách Complex `position_1` theo `layout_version` (data-analyst khuyên).

## Bài toán
Test kính P801 (`docs/review-notes-p801.md` R4–R9, R11, R14): ở ConceptScene người học phải ghép thông tin từ 2–3 chỗ: màn hình LVS, thanh nút lệch 18 cm và bảng chữ bên phải (R5, R6); kiện trên tay và `Info Panel` cách 0,5–0,7 m (R4); ô Soll/Verp.-Einh., bảng phép tính và thùng đơn (R8). Người học không nhận ra phải cầm carton STECKER C (R7), thử cầm các vật chỉ để xem (R9). Cụm câu hỏi không bao giờ thu lại: ở Concept chắn khi lùi tour (R11), ở Complex chắn thùng đơn suốt vị trí 1 (R14).

Mục tiêu: mỗi bước một chỗ đọc và bấm; biết lúc nào phải cầm kiện; phân biệt vật cầm được với vật chỉ để xem; cụm câu hỏi xong thì không chắn đường.

## Xung đột giữa các đề xuất
| Vấn đề | Bên A | Bên B | Đánh đổi |
|---|---|---|---|
| Chữ của 4 bước dùng bảng | unity A: giữ bảng, thêm marker; unity C: chuyển nhưng phải mở 024 | UX A, ID A/B: chữ lên thanh giữa, bỏ bảng; UX B (không khuyên): dời bảng sát viền | Rẻ ↔ một chỗ đọc |
| Câu hỏi ở bước 11 | ID B, UX A: tự hiện ở bước 11, ẩn khi lùi, thu khi xong | ID A: „Zu den Fragen“ → chính chỗ thanh chuyển sang câu hỏi | Không thêm lần bấm ↔ không tranh chỗ, demo xong mới đánh giá |
| Dòng „làm gì“ | ID: trong chú thích ô, có biểu tượng tay | UX: trên thanh (−42°, gần carton −68° hơn) | Sát ô ↔ sát nút và vật |
| `Info Panel` | unity B: giữ làm nhãn phụ | ID, UX: bỏ | Lưới an toàn ↔ bớt một tấm |
| Lúc thanh hiện | R5: xong thẻ 1 (4 loại) | ID: ≥1 kiện hoặc 60 s; UX: thẻ 1 hoặc N giây | Đúng lời thẻ ↔ ít chờ |
| Ghi cách mở tour | data B: trường `opened_by` | data A: suy ra từ `grab_start` | Tách được nguyên nhân ↔ log giữ nguyên |
| Vật chỉ để xem | UX C (khác): cho cầm, thả thì về chỗ | ID, UX: trông khác + „Nur zum Ansehen“; data: không `PackageData`/`Grabbable` | Tự nhiên ↔ `grab_*` sạch, demo không vỡ |
| Complex sau câu 2 | data C: giữ `Worked Example` thu gọn | UX C, ID: ẩn hẳn, giữ một dòng trên thẻ 2 | So sánh dữ liệu giữa build ↔ fading, không che thùng |

## Các phương án
| | Đúng nghiệp vụ | Hiệu quả học | Tải nhận thức/UX | Chi phí/rủi ro | Nhất quán |
|---|---|---|---|---|---|
| 0. Tối thiểu (căn giữa thanh, ẩn ở thẻ 1, thu cụm, nâng mũi tên, viền vật minh hoạ; giữ bảng và `Info Panel`) | 0 | 0, R4/R6/R8 còn | − 2–3 chỗ ở 5 bước; thanh giữa đè cụm ở bước 11 | ++ S–M | 0 |
| A. Thanh giữa có chữ + câu hỏi tự hiện cạnh thanh | 0 | + | −/0 bước 11 có chữ, demo và câu hỏi cùng lúc; tranh chỗ với Menu | − M–L | + |
| **B. Bàn học: thanh đổi chế độ tour ↔ câu hỏi, thẻ nhãn cạnh kiện, phép tính sát ô, vật minh hoạ khác kiện thật; Complex thu cụm + dòng kết quả** | 0 | ++ demo trước, đánh giá sau; dòng hành động tách riêng | ++ một chỗ đọc + bấm; nhãn cạnh tay | − M–L | + một quy tắc „xong thì thu lại“, một quy tắc „vật nào cầm được“ |
| C. Giữ bảng tour, kéo sát viền / lên trên màn hình | 0 | 0 | − vẫn hai chỗ, ngẩng đầu | + | 0 |
| Complex C1: ẩn cụm + `Worked Example`, dòng „2 Kartons + 5 Stück“ trên thẻ 2 ↔ C2: giữ bảng thu gọn | 0 | C1 + fading | C1 ++ / C2 − | S / S | C2 + cho R |

Không phương án nào bị veto: không quy tắc nghiệp vụ nào đổi; không sửa SDK (thẻ nhãn chỉ đọc vị trí kiện); độ dài chữ DE và độ đọc được ở −42° / 30–40 cm là điều kiện của Done when.

## Quyết định: phương án B, Complex theo C1
1. **Thanh tour (#89).** Tâm x bằng tâm màn hình (0,06), sát dưới viền, mép trên < y 1,22 (#84). Trên thanh: dòng chữ của bước, „Schritt n von 11“, [Zurück] [Weiter]. Bước intro, not_used, material_flow không còn chữ trên `Sample Description`; bảng tắt hẳn. Mọi lời dặn „làm gì“ nằm trên thanh, có biểu tượng tay; chú thích ô chỉ còn „là gì“, tối đa 1 câu. Thanh ẩn khi vào bài; hiện khi xong thẻ 1 (đủ 4 `MaterialKey`, như #79) hoặc sau 60 s kể từ khi vào bài (chỉnh trong Inspector). Lúc thanh hiện: khung xanh sang thẻ 2, tour mới hiện bước 1, nên `tour_step` `intro` ghi lúc thanh hiện, không ghi lúc `Start`.
2. **Dặn cầm kiện và thẻ nhãn (#90).** Bước `material`, `verp_einh`: dòng hành động trên thanh „cầm carton STECKER C“, mũi tên cam trên carton nảy nhẹ; đang cầm STECKER C thì dòng đổi sang lời so sánh nhãn ↔ LVS. Cầm bất kỳ kiện nào: thẻ nhỏ lệch sang bên kiện, không che nhãn, quay về phía mắt, ghi Mat.-Nr và VPE; hai kiện thì hai thẻ; thả thì tắt. Thẻ chỉ đọc transform của kiện, không ghi transform hay rigidbody. `Info Panel` tắt (giữ object). `concept.card.step1_body` bỏ câu „links neben dem Bildschirm“.
3. **Bước `cartons` (#91).** Phép tính „120 : 40 = 3 Kartons“ thành chú thích sát hai ô Soll và Verp.-Einh. (kiểu 024 mục 1), có dấu nối từ hai số. Thẻ 40/80/120 trên carton đếm cùng màu/kiểu với ô LVS. Carton đếm vẫn ở thùng đơn (chỗ có nghĩa); dòng hành động trên thanh chỉ xuống thùng.
4. **Câu hỏi Concept (#93).** Lần đầu tới bước 11: `quiz_unlocked` như hiện nay, khung xanh sang thẻ 3, nút phải của thanh ghi „Zu den Fragen“ (thay „Von vorn“). Bấm nút: chính chỗ thanh chuyển sang cụm A/B/C, kèm nút quay lại tour; thanh và cụm không bao giờ cùng hiện. Đúng câu 4: giữ „Richtig!“ 1,5 s rồi về chế độ tour ở bước 11 với „Von vorn“ (không gọi `ShowStep`, không ghi `tour_step`); thẻ 3 có ✓; mục bài 3 trên Menu sáng xanh như Welcome (#39), lời nhắc chọn bài 3 không còn trên cụm. Lùi tour khỏi bước 11: không cụm nào hiện.
5. **Vật chỉ để xem (#92).** Quy tắc cho mọi scene: trông như kiện thật thì cầm được; vật minh hoạ trông khác ngay khi nhìn. Carton đếm và thùng mẫu EKZ: bán trong suốt + viền. Tay lại gần vật minh hoạ: hiện „Nur zum Ansehen“. Vật minh hoạ không có `PackageData`/`Grabbable`. `Stock STECKER C`: mặc định giảm còn 1–2 carton + chữ khi chạm; chọn cuối cùng sau khi thử kính.
6. **Complex (#93, C1).** Đúng câu 2, hết 1,5 s, khi thùng kho đã vào chỗ: `Remainder Quiz` và `Worked Example` ẩn. Thẻ kệ 2 ở vị trí 1 ghi kết quả của người học „2 Kartons + 5 Stück“ (`PositionTexts`). #58 vẫn dựa vào `IsAnswered`.
7. **Log.** `layout_version` "026" (chuỗi số, R kiểm `as.integer(...) >= 25`), một build cho #89–#93. 11 `step_id`, 4 `question_id` giữ nguyên, không event hay trường mới (không `opened_by`: đi đường dự phòng suy ra khi có < 4 `material_key` khác nhau trước `tour_step` đầu). Spec ghi: `intro` bắt đầu lúc thanh hiện; Complex `position_1` khác mức scaffold giữa "025" và "026". R tách Complex `position_1` theo `layout_version`.

**Được:** mỗi bước một chỗ đọc và bấm (R5, R6); nhãn cạnh tay (R4); lời dặn cầm kiện tách riêng, có tín hiệu trên vật (R7); phép tính sát ô (R8); biết vật nào cầm được (R9); không cụm nào chắn đường sau khi xong (R11, R14); hợp đồng log giữ nguyên, chỉ đổi mã layout.

**Mất / chấp nhận:** thêm một lần bấm „Zu den Fragen“; carton đếm vẫn cách phép tính một lần đưa mắt (thùng đơn +32°/−49°); tắt `Info Panel` mất lưới an toàn (object còn, bật lại được); thanh ẩn ở thẻ 1 nên 025 không còn „không khoá“ tuyệt đối (dự phòng 60 s); dữ liệu Concept "025" và "026" không gộp được; viết lại chữ cho 11 bước (M–L).

**Bị bác:** phương án 0 làm riêng (R4, R6, R8 còn; thanh đè cụm); giữ `Info Panel` làm nhãn phụ (bảng trống khi không cầm gì); dời bảng tour (vẫn hai chỗ đọc); câu hỏi tự hiện cạnh thanh (tranh chỗ, bước 11 quá tải; dự phòng); dòng hành động trong chú thích ô (thanh gần carton hơn); cho vật minh hoạ cầm được rồi về chỗ (phức tạp `Lane Flow Demo`, bẩn `grab_*`); trường `opened_by` (suy ra được); giữ `Worked Example` thu gọn (vẫn che thùng); dấu „✓ 4/4“ riêng trên cụm (thẻ 3 đã có ✓).

## Done when
Xem `docs/backlog.md` #89, #90, #91, #92, #93.

## Nếu giả định sai
- Người học không hiểu „Zu den Fragen“: câu hỏi tự thay thanh khi demo ba làn dừng (không đổi log).
- Chữ DE không vừa thanh: chữ của bước thành một dải ngay dưới thanh; vẫn không vừa thì giữ chú thích bên phải cho đúng bước đó.
- Thẻ nhãn khó đọc khi xoay kiện: cố định thẻ cạnh cổ tay; muộn nhất bật lại `Info Panel`.
- Người học hay đi đường dự phòng (log có < 4 khoá trước `intro`): mở thanh ở lần cầm đầu tiên, đổi chữ thẻ 1 thành „eine Verpackung“.
- Bóng mờ trông sai nghĩa: viền + „Nur zum Ansehen“, giữ vật đặc.
- Người học quên kết quả ở Complex: bật lại `Worked Example` thu gọn.
- Mọi bước lùi chỉ là marker, transform, thông số Inspector hoặc CSV; hợp đồng log chỉ đổi `layout_version`.

## Câu hỏi cho người hướng dẫn
Không có câu mới. D30/D31 của 024 vẫn giữ. Quan sát ở buổi test: người mới có tự cầm STECKER C khi dòng hành động nằm trên thanh không.

## Bằng chứng
- `WmsFieldTour.Start` gọi `ShowStep(0)`; `ShowStep` ghi `LogTourStep` rồi `StepShown` → `Assets/Scripts/Concept/WmsFieldTour.cs:94–113, 153–192` → đã kiểm
- Cụm Concept mở một lần (`!IsUnlocked`, `clusterRoot.SetActive`), không bao giờ ẩn lại; có `LessonCompleted` → `Assets/Scripts/PackageQuizController.cs:226, 251–263, 348` → đã kiểm
- Complex chỉ ẩn cụm ở `OnLineAdvanced` → `Assets/Scripts/Complex/RemainderQuizController.cs:273–285` → đã kiểm
- #58 dựa vào `remainderQuiz.IsAnswered` → `Assets/Scripts/PiecesOrderValidator.cs:111` → đã kiểm
- `LayoutVersion = "025"`, một hằng số → `Assets/Scripts/Research/ResearchEventData.cs:12` → đã kiểm
- R cần mã layout số ≥ 25 → `analysis/analyse_set.R:1205–1207` → đã kiểm (data-analyst)
- Thẻ 1 tính theo `MaterialKey` khi `Grabbable` Select → `Assets/Scripts/Concept/ConceptCardProgress.cs` → đã kiểm (synthesizer)
- `concept.card.step1_body` „links neben dem Bildschirm“; `concept.tour.material.body` lời dặn cuối câu 2 → `docs/i18n/strings-todo.csv:55, 71` → đã kiểm
- P801: `intro` ≈ 150 s, 17 `grab_start` trước `material`, lùi 10→4 sau `task_completed` → log `05_data/raw/P801_*` (data-analyst) → chưa kiểm lại
- Thanh giữa (≈ 0,40 m) đè cụm x 0,10–0,42; góc nhìn thanh −42°, carton −68°, thùng đơn +32°/−49° → suy ra từ toạ độ (xr-ux-reviewer) → chưa kiểm bằng ảnh
- Chữ DE vừa thanh; thẻ nhãn đọc được ở 30–40 cm; phát hiện tay chạm vật không có `Grabbable` → chưa kiểm (đo khi làm, thử trên kính)
