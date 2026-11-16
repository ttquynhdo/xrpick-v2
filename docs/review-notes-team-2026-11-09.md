# Góp ý của nhóm sau buổi test Prototype v2 (2026-11-09)

Nguồn: trang Notion „Internal team review · Prototype v2 · Phân tích và xử lý comment“ (Thesis / Research, sửa lần cuối 2026-11-09). Các bạn test bản tiếng Anh, nên comment trích chữ EN. Cột „Loại“ là phân loại của nhóm trên Notion: **Sửa ngay**, **Lý thuyết 01** (bớt thông tin thừa, chia nhỏ), **02** (đặt gần nhau, tránh chia chú ý), **04** (học qua ví dụ), **Bug**, **Audio**.

Kế hoạch trên Notion: các mục „Sửa ngay“ trước 15/11, audio để vòng sau, Festo review 18/11, design freeze 19/11. Ngày 2026-11-16 chưa mục nào được làm (repo không có commit sau 7/11).

**Đã chuyển vào backlog (2026-11-16):** C1, C4, C9 → #116 · C5, C6 → #117 · C7, C8, C11, C12, C13 → #118 · C15 → #119 · C21 → #120 · C2, C3, C10 → #121 · C16, C17 → #122 · C18 → #123 · C20 → #124 · C19 → #125 (chờ các bạn chỉ chữ nào) · C14 → #126 (chờ bước tái hiện) · C22, C23 → #127 (vòng sau).

**Ảnh của buổi test** (người dùng thêm 2026-11-16): `5. Thesis/07_presentations/Image.jpg`, `Image (1).jpg` … `Image (11).jpg`, 12 khung hình chụp từ video quay trên kính (đồng hồ quay 00:05 → 21:01). Mã phiên **P805** (bảng chúc mừng, ảnh 11), là mã pilot nên log nằm ở `research/pilot/` trên Quest; máy này chưa có file log P805 (`05_data/raw` chỉ có P802). Mọi chữ trên ảnh khớp với CSV hiện tại (layout 034), nên các bạn test đúng bản đang có trên `main`.

| Ảnh | Giờ video | Scene, nội dung | Comment liên quan |
|---|---|---|---|
| `Image.jpg`, `Image (1).jpg` | 01:16, 01:21 | Welcome: bảng „What you will learn“ (You know / You can, 2 dòng vàng), thẻ 1 „Reach for the box with one hand and lift it up.“ | C1–C5, C9 |
| `Image (2).jpg` | 00:05 | Concept lúc vào: giá thẻ, thẻ 1 khung xanh „Read the label“, màn LVS bên phải chưa có câu nào giới thiệu | C7 |
| `Image (3).jpg` | 02:20 | Concept bước 2: khung cam quanh Mat.-Nr/Bez., dòng hành động „Pick up the STECKER C carton and check that its label matches Mat.-Nr and Bez.“; ảnh nhòe do quay | — |
| `Image (4).jpg`, `Image (5).jpg` | 09:38, 09:43 | Concept cuối: thanh tour „… you move the containers … menu.“, bảng EKZ bên phải, Menu bên phải dưới bảng EKZ; người học đưa tay tới „3. Order picking by EKZ 3“ (nền xanh) | C5, C6, C20 |
| `Image (6).jpg` | 11:41 | Basic: thẻ 1–3 | — |
| `Image (7).jpg` | 15:45 | Mixed position 2 (SCHRAUBE B, Soll 60, Empf Kz 000 nền xám) | C18 (000 xám, không nổi) |
| `Image (8).jpg` | 17:01 | Complex position 1: thẻ 1 nhắc câu hỏi và bảng „Example“ | C16 |
| `Image (9).jpg` | 17:19 | Complex: câu 1 „How many full cartons …“ (Soll 55, Verp.-Einh. 25) và bảng ví dụ 64/20 ngay dưới | C16, C17 |
| `Image (10).jpg` | 18:23 | Complex: người học chọn **C: 3 cartons** (đúng là 2), phản hồi „Not correct. Divide Soll by VPE. Only full cartons count.“ — đúng số của ví dụ (64 : 20 → 3) | C15, C16 |
| `Image (11).jpg` | 21:01 | Complex `Congratulations`: „What you learned“, 7 gạch, câu bàn đạp chân, câu tháo kính, `Code: P805` | C21 |

Cách đánh số bước: „Step n“ là bước của tour ConceptScene („Step n of 11“): 1 `intro`, 2 `material`, 3 `verp_einh`, 4 `soll`, 5 `cartons`, 6 `ist`, 7 `rest`, 8 `empf_kz_003`, 9 `empf_kz_000`, 10 `not_used`, 11 `material_flow` (thứ tự đọc từ `ConceptScene.unity`). „Basics“ là bài 2 (`lesson.concept` „2. Basics“ = ConceptScene).

Mỗi mục: **góp ý** (theo Notion), **hiện trạng** (đọc từ CSV/code ngày 2026-11-16), **ghi chú**.

---

## WelcomeScene

### C1. Tiêu đề „What you will learn“ dài (Sửa ngay)
- **Góp ý:** đổi thành „Learning Objective“, „You know“ → „You will know“, „You can“ → „You will be able to“.
- **Hiện trạng:** `welcome.panel.title` „What you will learn“ / DE „Das lernen Sie“; `welcome.panel.know` mở đầu „**You know:**“, `welcome.panel.can` „**You can:**“ (#86, decision 034).
- **Ghi chú:** đúng. DE cần câu tương ứng tự nhiên (vd. „Lernziele“, „Danach kennen Sie:“ / „Danach können Sie:“; `localize-strings` + `german-reviewer`). `complex.end.summary` lặp lại hai cụm với „now“ (decision 034), giữ nguyên dạng „You now know“.

### C2. Mục tiêu dài, đọc mệt (Lý thuyết 01)
- **Góp ý:** rút gọn, thêm biểu tượng.
- **Hiện trạng:** `Info Panel` có 3 gạch đầu dòng „know“ + 4 „can“, cỡ chữ 20, mỗi dòng gần chạm mép bảng (dòng DE dài nhất 577/578 đơn vị). 4 gạch „can“ trùng chữ đậm của thẻ mục tiêu bài 2–5 (decision 027/034).
- **Ghi chú:** rút gọn kéo theo thẻ mục tiêu bài 2–5 và `complex.end.summary` (cùng chữ). Cần `solution-decision` cùng C3.

### C3. Welcome quá nhiều thông tin (Lý thuyết 01)
- **Góp ý:** chia thành chuỗi: mục tiêu trước, nhiệm vụ sau.
- **Hiện trạng:** cùng lúc thấy: tiêu đề + phụ đề, `Info Panel` (mục tiêu, thời lượng, „Your tasks are on the cards on the left“, „Then choose the next lesson in the menu“), giá thẻ (thẻ mục tiêu + 2 thẻ bước), Menu, nút đỏ, hộp. Bàn phím mã người học biến mất sau khi nhập mã.
- **Ghi chú:** cần quyết định cách chia (vd. bảng mục tiêu trước, thẻ việc + hộp hiện sau một nút „Weiter“). Cùng item với C2, C10.

### C4. Gạch „You can“ thứ 3 khó hiểu (Sửa ngay)
- **Hiện trạng:** „pick several positions one after another“ / DE „mehrere Positionen nacheinander kommissionieren“. Người mới chưa biết „position“ là một dòng của đơn.
- **Ghi chú:** đúng. Câu này là chữ đậm của thẻ mục tiêu Mixed (`mixed.card.task_body`) và nằm trong `complex.end.summary`, nên sửa cả ba (decision 027/034: cùng câu). Câu mới phải vừa một dòng ở cả EN và DE.

### C9. „Grab the box“: nhấc lên rồi không biết làm gì (Sửa ngay)
- **Hiện trạng:** `welcome.card.step1_body` „Reach for the box with one hand and lift it up.“
- **Ghi chú:** đúng: thẻ không nói thả xuống. Sửa thành „… lift it up and put it down again.“ (EN + DE).

### C10. Bước tiếp theo sau khi nhấc hộp chưa rõ (Lý thuyết 04)
- **Hiện trạng:** nhấc hộp → ✓ trên thẻ 1 (`WelcomePracticeFeedback`), thẻ 2 „Press the button“ không được làm nổi (Welcome không có `InstructionStepHighlighter`).
- **Ghi chú:** C9 xử lý phần chữ; phần „chỉ bước tiếp theo“ (khung xanh trên thẻ 2 như Basic/Mixed/Complex) đưa vào cùng quyết định với C2/C3.

## Chuyển bài (Welcome + Concept)

### C5. „Then choose the next lesson“ (Sửa ngay)
- **Góp ý:** „To move to the next section, choose the highlighted lesson on the menu“.
- **Hiện trạng:** `welcome.panel.next` „Then choose the next lesson in the menu.“; Concept sau câu hỏi cuối: `concept.tour.material_flow.action_done` „Now choose the next lesson in the menu.“ Cả hai bài đều đã có nền xanh `Next Lesson Highlight` sau bài kế tiếp (#39, #93), nhưng câu không nhắc tới nó.
- **Ghi chú:** đúng. Basic/Mixed không có câu này nữa (#109); Concept và Welcome thì còn.

### C6. Sau Basics dễ bấm „Start over“ nhầm (Sửa ngay)
- **Hiện trạng:** khi trả lời đúng câu cuối, nút phải của thanh tour đổi lại thành „Start over“ (`FinishQuestions`), đứng ngay dưới màn hình; Menu ở bên phải.
- **Ghi chú:** đúng. Câu hành động nói rõ hai lựa chọn: chọn bài được tô sáng trong Menu để học tiếp, hoặc „Start over“ để xem lại phần giải thích. Gộp với C5 (#117).

## ConceptScene (tour)

### C7. Basics thiếu câu giới thiệu WMS (Sửa ngay)
- **Hiện trạng:** bước 1 `concept.tour.intro.body` „Here you see an order in the warehouse management system (WMS).“ Thanh tour chỉ hiện sau thẻ 1 (cầm carton) hoặc 60 s, nên lúc đầu người học thấy màn hình mà chưa có câu nào nói đó là WMS.
- **Ghi chú:** câu đã có nhưng mơ hồ („Here“). Sửa thành „The screen in front of you is the WMS …“. Gộp với C8.

### C8. Bước 1: „order“ dễ hiểu nhầm (Sửa ngay)
- **Ghi chú:** đổi thành „customer order“ ở bước 1. DE „Auftrag“ → kiểm `glossary.csv` (vd. „Kundenauftrag“), `german-reviewer`.

### C11. Bước 6–7: không biết phải bỏ thêm carton cho tới khi Ist = Soll (Lý thuyết 04)
- **Hiện trạng:** bước 5 `cartons.action` „Put the cartons from the storage container into the order container.“; bước 6 `ist.action` „Look at Ist: it shows how many pieces you have put into the order container.“; Ist chỉ tăng khi carton nằm trong thùng đơn (#100).
- **Ghi chú:** đúng: không câu nào nói „bỏ tiếp tới khi Ist = Soll“. Thêm vào dòng hành động bước 5/6 (vd. „… until Ist equals Soll“). Không cần decision: chỉ chữ.

### C12. Bước 9: chữ ghi „left“ nhưng nằm bên phải (Sửa ngay)
- **Hiện trạng:** bước 9 `empf_kz_000.action` „Compare the two examples to the left of the screen.“ Hai thùng mẫu đúng là ở bên trái màn hình (trên bàn sau, #94/#107). Nhưng ở bước 9 bảng `EKZ Overview` và mũi tên chỉ hàng EKZ 0 bật lên ở **bên phải** (marker của bước 9–11).
- **Ghi chú:** không thể chỉ đổi „left“ → „right“: mẫu thật ở bên trái. Hai vật cùng hiện ở hai bên làm người học nhìn sang bảng bên phải. Sửa: câu nói rõ „the two example containers to the left of the screen“, và bảng EKZ không hiện ở bước 9 (chỉ 10–11), hoặc câu nhắc cả hai. Kiểm lại trên ảnh `ekz` khi làm.

### C13. Bước 11: thùng chạy quá nhanh (Lý thuyết 01, 02)
- **Hiện trạng:** `LaneFlowDemo.startDelay` 4 s sau khi bước 11 hiện (#89), có thể còn bỏ carton vào trước (`FillOrder`, 0,6 s mỗi carton); thùng chạy 0,6 m/s; thanh tour ẩn khi thùng chạy.
- **Ghi chú:** chờ lâu hơn và/hoặc chạy chậm hơn ở Concept (lần đầu người học thấy băng chạy). Các bài thực hành giữ 4 s (decision 033) trừ khi nhóm muốn đổi.

### C15. „Divide by VPE“ chưa rõ (Sửa ngay)
- **Góp ý:** „Soll divided by Verp.-Einh.“ (Notion ghi „Lesson 4“).
- **Hiện trạng:** chữ có ở `concept.quiz.cartons_from_soll.25.wrong` „Divide Soll by VPE.“ (bài 2) và `complex.quiz.wrong_cartons` „Divide Soll by VPE.“ (bài 5), `complex.quiz.wrong_loose` „Soll minus (full cartons × VPE)“. Trên LVS ô tên là `Verp.-Einh.`, VPE là chữ trên nhãn carton.
- **Ghi chú:** đúng: dùng tên ô LVS „Verp.-Einh.“ trong mọi phép tính. Chỉ đổi CSV.

## ComplexScene

### C16. Ví dụ Soll 64 dễ bị chép nhầm số (Lý thuyết 04)
- **Hiện trạng:** bảng `Worked Example` „Soll 64 ST, VPE 20 ST … 3 full cartons … 4 loose pieces“ ngay dưới cụm câu hỏi; đơn thật 55/25 (số khác có chủ ý, decision 016).
- **Ghi chú:** ảnh 9–10 xác nhận: với Soll 55, Verp.-Einh. 25 người học chọn „3 cartons“, đúng kết quả của ví dụ 64 : 20. Thêm câu „Use the numbers in the WMS for your order.“ Gộp với C17 (#122).

### C17. Dấu trừ và logic chưa được giải thích (Lý thuyết 04)
- **Góp ý:** dùng „−“, giải thích trước „Loose pieces = Soll − (Verp.-Einh. × cartons)“.
- **Hiện trạng:** ví dụ viết „64 - 60 = 4 loose pieces“ (dấu gạch nối), công thức chỉ hiện khi trả lời sai câu 2 (`complex.quiz.wrong_loose`).
- **Ghi chú:** đúng. Đưa công thức lên bảng ví dụ trước các số.

### C18. Bài cuối: Empf Kz 000 dễ bị bỏ qua (Lý thuyết 02)
- **Hiện trạng:** cả hai position Complex là 000; thẻ 1 có liệt kê `Empf Kz`; từ #105 Basic/Mixed/Complex không vẽ gì lên LVS.
- **Ghi chú:** làm nổi chỗ nào và bằng gì (thẻ, LVS, thùng) mâu thuẫn với quy tắc „không vẽ lên LVS ở bài thực hành“ (#105) → cần `solution-decision`.

### C21. Màn chúc mừng quá dài (Sửa ngay)
- **Hiện trạng (ảnh 11):** `Congratulations`: tiêu đề + „What you learned“ + `complex.end.summary` (3 + 4 gạch đầu dòng lặp lại mục tiêu Welcome, decision 034, cuối có câu „At the real picking station you confirm with your foot.“) + câu tháo kính + mã người học. Bảng đầy gần hết chiều cao.
- **Ghi chú:** thay phần tóm tắt bằng biểu tượng và dấu ✓. Thay một phần decision 034 (tóm tắt lặp lại mục tiêu). Mã và câu tháo kính phải giữ (người điều phối cần).

## Cả khoá

### C14. Nút bấm không hoạt động tốt (Bug)
- **Ghi chú:** chưa biết nút nào, scene nào, bấm thế nào. Cần các bạn tả lại (hoặc giờ trong video). Đã sửa trước đây: vùng chạm A/B/C (#76).

### C19. Chữ nổi phía trước lúc gần lúc xa, gây chóng mặt (Lý thuyết 02, VR)
- **Ghi chú:** chưa rõ chữ nào. Các chữ bám theo người hoặc vật hiện có: dấu cảnh báo vàng trên carton nằm sai ô (#108, quay theo camera), thẻ báo của `ConceptBoxHints`. Thẻ, thanh tour và Menu đứng yên. Cần các bạn chỉ chỗ.

### C20. Menu khó nhận ra (Lý thuyết 02)
- **Góp ý:** thêm mũi tên chỉ tới Menu.
- **Hiện trạng:** Menu bên phải bàn; sau khi xong bài có nền xanh sau bài kế tiếp. Mũi tên cam hiện chỉ có nghĩa „nhìn/cầm cái này“ trong tour Concept (decision 037 mục 5: một ký hiệu một nghĩa).
- **Ghi chú:** cần `solution-decision` (ký hiệu nào, lúc nào hiện).

### C22, C23. Âm thanh đúng/sai, thêm audio thay chữ (Audio)
- **Ghi chú:** theo kế hoạch Notion: vòng sau, ngoài phạm vi trước design freeze.
