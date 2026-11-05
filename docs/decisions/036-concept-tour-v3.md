# Quyết định 036: ConceptScene tour v3: câu hành động có mục đích, khung trên LVS, hai mẫu EKZ cùng carton, khoá 2 s sau câu sai

- **Backlog:** #104, #105, #107, #110 (#106 bỏ, xem dưới)
- **Trạng thái:** accepted
- **Ngày:** 2026-11-05
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, german-reviewer, data-analyst (proposal mode, độc lập); decision-synthesizer.
- **Lựa chọn của người dùng (2026-11-05):** chấp nhận khuyến nghị, **trừ phần thanh tour**: người dùng thấy thanh tour hiện tại không có vấn đề, nên thanh giữ chỗ và hình dạng của #89 (decision 026 mục 1), chữ của bước vẫn ở trên thanh, và #106 bỏ. Mặc định được chọn: mẫu 000 đổi sang carton (bỏ lựa chọn S1 của 031); khoá 2 s; không thêm log cho chạm trong lúc khoá.
- **Sửa / thay quyết định đã chấp nhận:**
  - **010:** câu „không khung/đánh dấu trên dialog“. Từ nay được vẽ khung trên dialog, **chỉ trong tour ConceptScene** (người dùng 2026-11-05, P802 R13). Basic/Mixed/Complex vẫn không vẽ gì lên LVS.
  - **024 mục 2:** bỏ câu „Ist, Verp.-Einh. dựa vào tiêu đề + mũi tên hàng … không vẽ gì lên dialog“.
  - **026 mục 2:** thay toàn bộ (thẻ nhãn cạnh kiện đang cầm, `action_held`).
  - **028 và 031 (S1):** mẫu 000 không còn là túi SCHRAUBE B.
  - **020 / 024 mục 5:** thêm khoá sau câu sai; chấm ngay khi chạm giữ nguyên.
  - Giữ nguyên: 024 mục 3 và 026 mục 1 (thanh tour), 025, 030, 032.

## Bài toán
Pilot P802 (EN, layout "033", `docs/review-notes-p802.md`): thẻ Mat.-Nr/VPE cạnh kiện đang cầm nháy và chỉ lặp lại nhãn (R1); 4 dòng hành động chỉ là „Press Next“, „Pick up the STECKER C carton“ không nói để làm gì (R9, R11); bước 11 dài, thanh ẩn sau 4 s nên đọc không kịp, người học bấm Back rồi Next để đọc lại (R15); mũi tên đặt từ cạnh khung nên chú thích Ist bị đọc thành của Soll, thẻ chú thích cao cố định và lệch theo tầm mắt, marker cam che nhãn carton (R4, R10, R13); hai mẫu EKZ khác nhau cùng lúc ở loại hàng, vật tư và cách xếp (R14); người học đoán liền A→C→B (R6).

## Quyết định
1. **Thẻ cầm tay (#104).** Bỏ `Held Label Tags`, `.action_held`, `heldMaterialKey`, `heldPackages`; xoá key `concept.label.tag.*` (`ImportCsv` dọn khỏi bảng); xoá `HeldPackageTracker` + test nếu không còn ai dùng. Giữ `TagPlacement` (`ConceptBoxHints` dùng). Chạy `scene-ref-auditor` trước khi xoá.
2. **Dòng hành động trên thanh (#104).** Mỗi dòng nói **việc + mục đích**. `material`: cầm carton STECKER C và so Etikett với Mat.-Nr/Bez. `verp_einh`: so VPE trên Etikett với Verp.-Einh. `intro`, `ist`, `rest`, `not_used`: một việc nhìn trên LVS. Không dòng nào chỉ nói bấm „Weiter“ (kể cả „Drücken Sie dann …“ ở `soll`). Câu `rest` không được nói hay ngụ ý Rest đổi khi người học di chuyển carton (Rest cố định 120, decision 001/030). Câu `ist` nối với việc vừa đặt carton ở bước 5. Cột context trong CSV đổi thành „instruction + purpose“.
3. **Bước 11 (#104).** Trước và trong demo: tiêu đề + một câu ngắn + lời „xem“. Thanh vẫn ẩn khi thùng chạy như #89. Khi thùng đơn đã đi và thanh quay lại: hiện câu „In der Praxis bewegen Sie die Behälter von Hand.“ (giữ `bewegen`, không nêu đích, giả định D32/D17) và nút „Zu den Fragen“. `startDelay` giữ 4 s.
4. **Khung trên LVS (#105).** Khung cam bao nhãn + ô số của bước, nằm trên màn hình, nối với thẻ chú thích bằng một đường dẫn trong khe giữa hai hàng; mũi tên không còn chạm ô đứng trước trong cùng hàng. Lớp vẽ là con của `WMS_Monitor/Head/Screen`, **ngoài** prefab `WMS_Kommissionierung`, cách mặt dialog 1–2 mm, không raycast, không che chữ hay số. Vị trí neo theo rect thật của ô (nếu đọc rect lúc chạy không ổn thì đặt tay). Thẻ chú thích `Caption <id>` cao vừa chữ, cùng mặt phẳng với màn hình, mép trên ngang hàng ô. `not_used` thành khung trên màn hình.
5. **Marker STECKER C (#105).** Dời sang cạnh nắp, không che `Label Top`; tắt khi carton được cầm lần đầu trong bước.
6. **Mẫu EKZ (#107).** Cả hai mẫu là carton KABELVERBINDER A cùng cỡ. 003: 3 carton một hàng, cùng hướng (như 031). 000: ≥ 5 carton nghiêng theo nhiều hướng, chồng lên nhau, không xuyên thành thùng (nới thùng 000 nếu cần). Thẻ mẫu không thêm động từ (sẽ lộ đáp án quiz). Không dùng chi tiết nhỏ đổ đống (Detail Description, „Kleinstteile“) và không nói carton được „einfüllen/schütten“ (slide 9: carton luôn đặt lỏng). Chữ đổi „Tüten“ → „Kartons“ ở `concept.tour.empf_kz_000.body`, `concept.quiz.ekz_000.q`, `.ordered.wrong`; chữ đáp án `ekz_000.poured`, `ekz_003.poured` đổi nghĩa thành „ungeordnet hineinlegen“, key và đáp án đúng giữ nguyên (`german-reviewer` chọn chữ). Hàng EKZ 0 trên `EKZ Overview` giữ nguyên. Hoạt cảnh đặt ↔ thả: hoãn, mở lại nếu `ekz_003` vẫn sai lần đầu ở buổi test sau.
7. **Khoá sau câu sai (#110).** Dòng giải thích hiện dưới A/B/C, ba nút mờ đi và không bấm được 2 s, không đếm ngược; dòng giải thích ở lại tới lần chạm sau. Một hằng số chung cho `PackageQuizController` và `RemainderQuizController`. Chạm trong lúc khoá: không có `quiz_answer`.
8. **Log.** Không thêm sự kiện hay trường. `layout_version` đổi **một lần** chung với decision 037; không build nào có thay đổi của đợt rời máy dev mà vẫn ghi "033". Spec ghi khoá 2 s. R: hằng số khoá theo layout (0 cho ≤ "033", 2 cho layout mới); `lane_demo_delay_s` thành hằng số theo layout (vẫn 4); `complex_remainder` tách theo layout. `make_fake_data.R` thêm khoảng nghỉ ≥ 2 s sau câu sai; `selftest.R` xanh; `data-analyst` duyệt.

## Được / mất
- **Được:** mỗi bước nói rõ nhìn/làm gì và vì sao; khung chạm đúng ô, hết lỗi Ist/Soll; người học tự đọc nhãn thật; mẫu EKZ chỉ khác một yếu tố và nối với bài 3–5; lời giải thích đọc được trước khi chọn lại; hợp đồng log không đổi hình dạng.
- **Mất:** lựa chọn S1 của 031 (túi vẫn được dạy ở Mixed vị trí 2); chữ đáp án EKZ đổi nên `ekz_000`/`ekz_003` không gộp được giữa layout (Concept vốn đã tách); không đo trực tiếp được khoá có giảm đoán không.
- **Không làm (người dùng 2026-11-05):** chuyển chữ của bước sang cạnh ô, thanh chỉ còn nút ở mép bàn, dải chữ dưới màn hình (#106). Nếu buổi test sau thấy thanh che thùng ở bước 2/5, mở lại #106 với phương án đó (synthesizer: phương án C).

## Làm gì nếu giả định sai
- Đọc rect ô lúc chạy không ổn: đặt khung bằng toạ độ cố định.
- 5 carton không vừa thùng 000: nới thùng; cuối cùng 4 carton nhưng chồng rõ ràng.
- D32/D17 = thùng tự chạy: bỏ câu „von Hand“.
- Người hướng dẫn nói carton EKZ 0 không được nằm chồng/nghiêng (D45): mẫu 000 còn carton lỏng, không thẳng hàng nhưng không chồng.
- Demo bước 11 chờ vì có hint tag (`IsClear` false): nút „Zu den Fragen“ vẫn mở như hiện nay khi có hint; kiểm khi làm.

## Câu hỏi cho người hướng dẫn (không chặn)
D32, D35 (còn mở); D45 (carton EKZ 0 nằm lộn xộn/chồng?), D46 (`einfüllen`/`schütten`/`hineinlegen`), D40 (đơn 120 chiếc trong carton: cân hay đếm Ist?).

## Bằng chứng
`docs/review-notes-p802.md` R1, R4, R6, R9–R11, R13–R15; `docs/pilot-P802-summary.md`; `Assets/Scripts/Concept/WmsFieldTour.cs` (`heldPackages`, `heldMaterialKey`, `.action_held`); `Assets/Scripts/PackageQuizController.cs` (`CorrectHoldSeconds` 1,5, câu sai không khoá); `Assets/Scripts/Concept/LaneFlowDemo.cs` (`startDelay` 4, `hideWhileRiding`); `analysis/analyse_set.R` (`lane_demo_delay_s <- 4`); `docs/i18n/strings-todo.csv` (`concept.tour.*`, `concept.quiz.ekz_*`); Kommissionierung slide 6, 8, 9 và Detail Description „Kleinstteile“ (qua requirements-analyst).
