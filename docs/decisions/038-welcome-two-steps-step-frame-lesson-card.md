# Quyết định 038: Welcome hai bước (mục tiêu ngắn có biểu tượng, rồi tới việc), khung xanh theo bước, thẻ 3 „Lektion wählen“; Menu được tập ở Welcome

- **Backlog:** #121, #124
- **Trạng thái:** accepted
- **Ngày:** 2026-11-16
- **Ý kiến đã xét:** instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, data-analyst, german-reviewer (proposal mode, độc lập); decision-synthesizer. Không hỏi requirements-analyst: Welcome không dạy quy tắc WMS hay quy trình.
- **Lựa chọn của người dùng (2026-11-16):** chấp nhận khuyến nghị (phương án B) với mặc định: biểu tượng nằm trong #121, nếu không kịp freeze 19/11 thì giao bảng chỉ có dòng ngắn trước (U1); đổi tên bài 4 thành „4. Mehrere Auftragspositionen“ là item riêng (U2); phần R cho Menu ở Welcome/Concept (`menu_after_done`, mức `menu_cue` mới) để cho #102 (U3); chữ nút „Zu den Aufgaben“ (U4).
- **Thay thế (một phần):** 034 (`welcome.panel.can` không còn phải giống từng chữ với câu đậm của thẻ mục tiêu bài 2–5: chỉ cần cùng ý, cùng thứ tự, và cùng lời với các dòng `complex.end.*` của #120); 027 mục 7 (khối hai dòng vàng của Welcome bỏ đi).
- **Giữ nguyên:** 011 (Menu bên phải bàn), 017 (khung xanh = bước hiện tại), 027 mẫu thẻ (thẻ mục tiêu bài 2–5 giữ câu đầy đủ), 034 U4 (dòng thời lượng), 035 (✓), 036/037 (một ký hiệu một nghĩa; mũi tên cam chỉ ở tour Concept), #42 (luật khoá Menu), #109 (highlight Menu ở Basic/Mixed).

## Bài toán
Review nhóm 2026-11-09 (`docs/review-notes-team-2026-11-09.md`): nhập mã xong người học thấy cùng lúc mục tiêu, thời lượng, hai dòng vàng, giá thẻ, hộp, nút đỏ và Menu (C3); bảy dòng mục tiêu dài, đọc mệt (C2); nhấc hộp xong không có gì chỉ sang thẻ 2 (C10); người học không nhận ra Menu bên phải bàn (C20). Menu nằm lệch phải khoảng 63°, cúi 37° tính từ mắt (xr-ux-reviewer, suy ra từ toạ độ), nên C20 có nguyên nhân hình học.

## Quyết định
1. **Bước 1, mục tiêu.** Sau khi nhập mã chỉ hiện tiêu đề `Welcome` và `Info Panel`: tiêu đề „Lernziele“, hai câu dẫn „Nach dem Kurs kennen Sie:“ / „Nach dem Kurs können Sie:“, 7 dòng ngắn cùng lời với `complex.end.know.1–3` / `can.1–4` (dòng 1 trên Welcome viết đầy đủ „die Felder im Lagerverwaltungssystem (LVS)“, glossary), dòng thời lượng. Mỗi dòng có một biểu tượng tự dựng bằng UI Image từ những gì đã có (pictogram EKZ, hình thùng/carton, icon Meta SDK), không trùng ✓, `warning` vàng hay mũi tên cam. Hai dòng vàng `common.panel.see_cards` và `welcome.panel.next` bỏ khỏi bảng (key giữ lại). Dưới bảng một nút poke tấm xám kiểu `Tour Nav Bar`: „Zu den Aufgaben“ / „To the tasks“, khác hẳn nút đỏ. Không hẹn giờ tự chuyển.
2. **Bước 2, việc.** Bấm nút: `Info Panel` và nút ẩn hẳn (cả poke surface); giá thẻ, hộp, nút đỏ, Menu hiện. `courseRoots` của `ParticipantEntryController` tách thành hai pha.
3. **Khung xanh (017) ở Welcome** trên thẻ đầu tiên chưa xong: thẻ 1 lúc đầu, thẻ 2 sau lần nhấc hộp đầu tiên, thẻ 3 khi cả hai thao tác xong. ✓ như #39.
4. **Thẻ 3 „3. Lektion wählen“** theo mẫu 027: câu mệnh lệnh có Sie, động từ „wählen“, „Menü“, „markierte Lektion“. Khung lên thẻ 3 cùng lúc với `Next Lesson Highlight`. Luật khoá Menu (#42) không đổi.
5. **Concept, Basic, Mixed** không thêm tín hiệu: hỗ trợ giảm dần từ thẻ có khung (Welcome) sang câu (Concept) rồi chỉ còn highlight (Basic, Mixed).
6. **Log.** Một event mới lúc bấm nút, mang `time_since_scene_enter_s`, chỉ mã enum, không chữ mục tiêu (tên do data-analyst chọn). `layout_version` "036" (chung với decision 039 nếu cùng build). Spec, `make_fake_data.R`, `analyse_set.R` (ít nhất khai báo event) sửa cùng thay đổi, `selftest.R` xanh. `welcome_practice_done` `grab`/`button` vẫn mỗi loại một lần. Mọi ID (study, pilot, test) cùng một luồng. Logging không điều khiển luồng.

## Đã bác
- Tự chuyển bước theo thời gian/nhìn: cắt ngang người đọc chậm, thời gian đọc không đo được.
- Chỉ làm nhẹ (rút gọn + khung, không chia bước): không giải quyết C3.
- Mũi tên/viền xanh „Menü“ ở thẻ kết quả/thanh tour cả 4 bài: không giảm hỗ trợ dần, mở rộng nghĩa màu xanh, quay lại một phần 037 U2; thẻ kết quả ẩn 2 s sau `Departed` nên mũi tên dễ hụt. Giữ làm đường lùi.
- Dời Menu vào vùng nhìn: trái 011 (lựa chọn của người dùng).
- Menu làm sơ đồ khoá học: bố cục cả 5 scene trước freeze, nhóm „kennen“ không ứng với từng bài.
- Hiện Menu ngay ở bước 1: thêm một thứ khi người học còn đang đọc.
- Rút gọn cả 4 thẻ mục tiêu: không cần, thẻ mục tiêu giữ câu đầy đủ.
- Bỏ dòng thời lượng: 034 U4.
- Nút „Los geht's“ (lệch tông) / „Weiter“ (không nói đi đâu).
- Data-analyst muốn thêm mức `menu_cue` và mở rộng `menu_after_done` ngay: tín hiệu Basic/Mixed không đổi, tách theo `layout_version` là đủ; dữ liệu thô không mất, làm ở #102 (người dùng chọn U3).

## Được / mất
- **Được:** C3 (đọc trước, làm sau), C10 (khung lên thẻ 2), C2 (bớt khoảng nửa số chữ, có biểu tượng), C20 bằng thực hành (chọn Menu là một bước có khung ở Welcome). Mục tiêu đầu và cuối khoá cùng lời (chỉ khác „jetzt“). Không ký hiệu mới. Research đo được thời gian đọc mục tiêu.
- **Mất:** Basic/Mixed vẫn chỉ có highlight ở góc ~63°. Welcome dài thêm thời gian đọc và một nút. Mục tiêu không còn giống từng chữ thẻ mục tiêu. Công M trước freeze; biểu tượng dễ trễ nhất. `menu_after_done` cho Welcome/Concept chưa có số ngay.

## Nếu giả định sai
- Người học vẫn khó tìm Menu ở Basic/Mixed: viền xanh quanh cả Menu bật cùng `Next Lesson Highlight`, mức `menu_cue` mới, tăng `layout_version`.
- Sau khi bấm nút người học không nhìn sang giá thẻ: giữ riêng dòng `common.panel.see_cards` ở chỗ của bảng.
- Biểu tượng không kịp hoặc dễ hiểu nhầm: giao bảng chỉ có dòng ngắn, biểu tượng thành item riêng.
- Người học muốn đọc lại mục tiêu: thẻ mục tiêu Welcome trên giá vẫn còn; nếu cần thì cho bảng thu nhỏ thay vì ẩn.

## Câu hỏi mở
- Người hướng dẫn: không có câu chặn.
- Chưa đo: dòng có „Lagerverwaltungssystem (LVS)“ + biểu tượng có vừa một dòng; chỗ dưới `Info Panel` cho nút; ảnh Play mode Welcome/Menu sau #116/#117 (chụp khi làm).

## Bằng chứng
- `Assets/Scripts/Welcome/WelcomePracticeFeedback.cs:69–87` (chỉ ✓ và highlight), `Assets/Scripts/ParticipantEntry/ParticipantEntryController.cs:45, 324–330` (`courseRoots` bật cùng lúc).
- `docs/i18n/strings-todo.csv` (`welcome.panel.*`, `complex.end.*`).
- Decisions 011, 017, 027 mục 7, 034, 037 mục 4–5/U2.
- `analysis/analyse_set.R` (`menu_cue` chỉ Basic/Mixed).
- `docs/review-notes-team-2026-11-09.md` C2, C3, C10, C20; ảnh của buổi test (thư mục thesis `07_presentations`).

## Sửa đổi #156 (2026-11-17, R3)
Bấm „Zu den Aufgaben“ ẩn cả băng tiêu đề `Welcome` cùng bảng mục tiêu và nút (`objectiveRoots` = `Welcome`, `Info Panel`, `Tasks Button`), để bước 2 không còn một băng tiêu đề đứng một mình trên cao. Bước 1 và log `welcome_tasks_opened` không đổi.
