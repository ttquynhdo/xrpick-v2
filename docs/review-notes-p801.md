# Góp ý sau buổi test kính P801 (2026-11-04)

Người dùng ghi góp ý từng điểm ở đây. Khi đủ, phân tích chung một lần (`review-to-backlog`, rồi `solution-decision` cho mục có đánh đổi). Ảnh chụp lưu ở `TestEvidence/review-P801/` (không commit).

Mỗi mục: **góp ý** (lời người dùng), **hiện trạng** (đọc từ code/CSV, chưa sửa), **ghi chú phân tích**.

**Đã chuyển vào backlog (2026-11-04):** R1 → #86 · R2 → #87 · R3, R13, R15 → #88 · R5, R6 → #89 · R4, R7 → #90 · R8 → #91 · R9 → #92 · R11, R14 → #93 · R10, R12, R16, R19 → #94 · R17, R18 → #95 · vật lý vòng (R16, video) → #96.

---

## R1. WelcomeScene, bảng „What you will learn“ (`Info Panel`)

- **Ảnh:** `TestEvidence/review-P801/01-welcome-objectives.webp`
- **Góp ý:**
  1. Lề trái quá sát, cảm giác bí bách.
  2. Mỗi mục tiêu phải nằm trên đúng một dòng, không xuống dòng.
  3. Khoảng cách giữa các dòng rộng hơn cho đỡ dày.
  4. Cân nhắc chia 5 gạch đầu dòng thành 2 cụm: học kiến thức lý thuyết gì, rồi thực hành gì.
- **Hiện trạng:** chuỗi `welcome.panel.goals` (`docs/i18n/strings-todo.csv`, font 20, hộp 530 px, tối đa 6 dòng). Ghi chú trong CSV đã yêu cầu „each bullet one line ≤ ~520 px“, nhưng bản EN mục 1 („read an order in the warehouse management system (WMS)“) bị xuống dòng, nên bảng thành 7 dòng. Bản DE chưa kiểm trên kính. Khối chữ sát mép trái khung, dòng sát nhau (line spacing mặc định).
- **Ghi chú phân tích:** chia 2 cụm phải khớp thứ tự bài (2. Grundlagen = lý thuyết/đọc LVS và nhãn; 3–5 = thực hành). Thêm tiêu đề cụm và giãn dòng làm bảng cao hơn, nên phải đo lại chỗ trên `Welcome` canvas. Mỗi chữ mới cần EN + DE (`localize-strings`, `german-reviewer`). Liên quan #23 (5 mục tiêu khoá học).

## R2. Bảng thẻ nhiệm vụ (`InstructionShelf`) ở mọi bài

- **Ảnh:** `TestEvidence/review-P801/02-cards-welcome.png`, `02-cards-concept.webp`, `02-cards-mixed.png`, `02-cards-complex.webp`
- **Góp ý:**
  1. Thẻ „Task“ (tổng quan của bài) màu đen giống hệt các thẻ bước, nên rối và không nắm nhanh được mục tiêu bài. Cần thiết kế để nhìn qua là biết mục tiêu bài là gì, rồi từng bước là gì.
  2. Padding/margin không đều trong cùng một bài, và giữa các bài với nhau.
  3. Welcome chỉ có 3 thông tin mà bảng gỗ rất to, chiếm nhiều chỗ, nhìn khó chịu. Các bài khác thì chữ dày đặc.
  4. Khoảng cách không đều làm đọc rất khó chịu.
- **Hiện trạng (từ ảnh và CLAUDE.md):** mọi thẻ cùng một mẫu (nền đen, tiêu đề đậm, thân chữ nhỏ), rộng 200, mỗi thẻ là một canvas world-space riêng, **không có layout group chung**. Vị trí thẻ đặt tay theo „slot“ cố định, cách nhau 1,2 cm, cao theo chữ dài nhất DE/EN của cả hai vị trí (#62/#63). Vì vậy ở Complex, giữa thẻ 1 và 2 có khoảng trống lớn (slot đủ chỗ cho chữ dài hơn ở vị trí 1). Bảng gỗ có cùng kích thước ở mọi bài (Complex cao hơn, scale y 0,97, #71). Ô „Aufgabe“ không bị dim hay đóng khung bởi `InstructionStepHighlighter`/`ConceptCardProgress`, nhưng trông vẫn như một bước.
- **Ghi chú phân tích:** cần một mẫu thẻ chung cho cả 5 bài (khác biệt rõ giữa thẻ mục tiêu và thẻ bước, ví dụ màu/khung/vị trí riêng). Khoảng cách nên do layout tự tính thay vì slot đặt tay, và bảng nên co theo nội dung. Phải giữ được các thứ đang chạy trên thẻ: khung xanh bước hiện tại (#40, #79), dấu ✓ `Done Check` (#39, #79), chữ đổi theo vị trí (`PositionTexts`, #53/#62). Chữ dày ở Mixed/Complex có thể rút gọn (liên quan R1 và memory „show, don't tell“). Cùng chủ đề bố cục/chữ với R1, nên quyết chung.

## R3. Cỡ kiện hàng và cỡ chữ trên nhãn khác nhau giữa các bài

- **Ảnh:** `TestEvidence/review-P801/03-packages-concept.webp` (Concept: KABELVERBINDER A và STECKER C cầm trên tay), `03-packages-schlauchschelle.webp` (carton SCHLAUCHSCHELLE B, Basic/Complex)
- **Góp ý:**
  1. Ở Concept các hộp và túi nhỏ, rất khó cầm. Ở Mixed và Complex hộp to hơn.
  2. Chữ trên nhãn to so với diện tích mặt hộp, nhìn khó chịu.
- **Hiện trạng (CLAUDE.md, chưa đo lại):** nhãn là TMP 3 dòng, auto-size, không xuống dòng, sticker trước phủ ~90 × 80 % mặt hộp + bản sao `Label Top` trên nắp (#36). Vì auto-size, cỡ chữ tự to theo sticker, nên carton lớn có chữ rất to sát mép (ảnh 2). Trên ảnh 1, cùng một bài mà cỡ chữ đã khác hẳn: KABELVERBINDER A chữ nhỏ (dòng tên dài, auto-size co lại), STECKER C chữ to. Concept (#84): KABELVERBINDER A root scale 0,25 „cỡ Mixed“, SCHRAUBE B 0,3/0,3/0,25, STECKER C copy carton KABELVERBINDER A; cần đo lại xem kiện Concept thực sự nhỏ hơn bài khác bao nhiêu.
- **Ghi chú phân tích:** cần một cỡ chuẩn cho mỗi loại kiện (carton, túi), giống nhau ở mọi bài và đủ to để cầm bằng tay thật. Nhãn nên có cỡ chữ cố định (hoặc giới hạn max) và lề quanh chữ, thay vì auto-size phủ kín sticker. Đổi cỡ carton kéo theo: số carton xếp vừa thùng lưu/thùng đơn, ô `BoxSnapper` 2 × 3 ở Basic (#56), thùng mẫu EKZ và carton đếm ở Concept (#83). Liên quan #36 (nhãn), #84 (cỡ kiện Concept).

## R4. ConceptScene: giải thích nhãn nằm trên bảng xa, không ở cạnh kiện đang cầm

- **Ảnh:** `TestEvidence/review-P801/04-concept-labelboard.webp`
- **Góp ý:** khi cầm một kiện, người học tự nhiên nhìn vào kiện đó. Giải thích lại hiện trên một bảng khác (`Info Panel`, bên trái màn hình), nên phải chuyển mắt sang đọc, bị split attention, rất khó chịu. Muốn giải thích hiện ngay cạnh kiện hoặc cạnh tay đang cầm.
- **Hiện trạng:** `Assets/Scripts/PackageInfoOnGrab.cs` (#45, decision 007 mục 5): khi cầm, ghi tên + `concept.label.matnr` + `concept.label.vpe` (lấy từ `PackageData`) lên `PackageInfoDisplay` của bảng `Info Panel` ở (−0,66, 1,30, 0,98); thả ra thì bảng về chữ mặc định („1. Read the label / Pick up a package…“). Bảng cách kiện trên tay khoảng 0,5–0,7 m và lệch hướng nhìn. Khi chưa cầm gì, bảng là một khối đen lớn gần như trống.
- **Ghi chú phân tích:** làm được: một chú thích nhỏ gắn theo kiện đang cầm (canvas world-space đi theo kiện hoặc cổ tay, luôn quay về phía mắt, đặt lệch sang bên để không che nhãn), có thể có đường nối/mũi tên chỉ vào đúng dòng Mat.-Nr và VPE trên nhãn. Cần cân nhắc: cầm hai kiện cùng lúc (hai chú thích?), đọc được khi tay rung/xoay, không che chữ trên nhãn, cỡ chữ đủ đọc ở 30–40 cm, và có còn cần bảng `Info Panel` hay bỏ hẳn (giải phóng chỗ bên trái màn hình). Cùng nguyên lý spatial contiguity đã dùng cho chú thích từng ô ở #78; hợp với memory „show, don't tell“. Liên quan R3 (cỡ kiện, cỡ nhãn).

## R5. ConceptScene: thanh „Back / Step n of 11 / Next“ lệch tâm và hiện quá sớm

- **Ảnh:** `TestEvidence/review-P801/05-concept-navbar.webp`
- **Góp ý:**
  1. Thanh điều hướng tour LVS không nằm giữa so với màn hình LVS. Người học đứng chính giữa để tương tác nên phải nhìn lệch, khó chịu.
  2. Thanh chỉ nên hiện khi sang bước 2 của bài (thẻ „2. Get to know the WMS“). Lúc mới vào bài không cần hiện, để người học khỏi phải đoán đây là cái gì.
- **Hiện trạng (CLAUDE.md):** `Interactables/Tour Nav Bar` (#80, decision 024) tâm ≈ (−0,12, 1,17, 0,48), ba nút cách nhau 16 cm; `NextButton` ở (0, 1,17, 0,48). Màn hình LVS tâm x ≈ 0,06, nên thanh lệch trái khoảng 18 cm. Thanh luôn hiện từ đầu, tour đứng ở „Step 1 of 11“ ngay khi vào bài. Bên phải thanh là cụm câu hỏi `AnswerOption` (x 0,10–0,42, hiện từ bước 11), bên trái là `Info Panel`.
- **Ghi chú phân tích:** dời thanh về giữa thì phải xếp lại với cụm câu hỏi (hai thứ cùng độ cao, cùng độ sâu z ≈ 0,48–0,49) và giữ tầm với tay (#84 đã đo chỗ này). Ẩn thanh đến khi xong thẻ 1 thì phải đổi cách `ConceptCardProgress` (#79) chuyển khung xanh sang thẻ 2 (hiện chuyển khi bấm „Weiter“ lần đầu hoặc khi xong thẻ 1), và quyết định: nếu người học không cầm đủ 4 loại kiện thì có cách nào khác mở tour không (tránh kẹt). Tour step 1 (`intro`) và log `tour_step` sẽ bắt đầu muộn hơn; `layout_version` phải đổi và R cần biết (decision 024: không gộp dữ liệu Concept trước/sau). Liên quan R2 (thẻ bước), R4 (bảng `Info Panel`).

## R6. ConceptScene: bảng tour bên phải màn hình bắt người học ghép thông tin từ 3 chỗ

- **Ảnh:** `TestEvidence/review-P801/06-concept-tour-intro.webp` (bước 1 `intro`: „The WMS – This is an order in the warehouse management system (WMS). Press "Next".“)
- **Góp ý:** để hiểu bước này, người học phải nhìn 3 nơi: màn hình LVS, chỗ nút Next (dưới màn hình, lệch trái) và bảng chữ bên phải màn hình, rồi tự ghép lại. Vi phạm split attention.
- **Hiện trạng:** bảng tour `Sample Description` ở (0,70, 1,30, 0,98), bên phải màn hình. Từ #78 (decision 024), các bước về một ô (material, verp_einh, soll, ist, rest, empf_kz_003, empf_kz_000) đã dùng chú thích sát ô; còn 4 bước `intro`, `cartons`, `not_used`, `material_flow` vẫn dùng bảng bên phải (`concept.tour.<id>.body`). Bảng nhỏ chữ, phần dưới trống. Chữ „Press "Next"“ chỉ tới nút nằm ở chỗ khác (thanh điều hướng, R5).
- **Ghi chú phân tích:** gom chữ hướng dẫn, số bước và nút Next/Back vào một chỗ, sát màn hình (ví dụ một thanh ngay dưới viền màn hình, giữa, có chữ của bước + Back/Next), và bỏ bảng bên phải cho 4 bước còn lại. Mỗi bước nên chỉ thẳng vào thứ nó nói tới (bước `intro`: khung/mũi tên quanh cả dialog; `material_flow`: chữ gần băng chuyền). Giữ quy tắc màn hình (decision 010: không vẽ gì lên dialog). Gộp với R5 (vị trí + lúc hiện thanh điều hướng) và R4 (bỏ/giữ `Info Panel`): ba điểm này quyết chung một bố cục cho ConceptScene.

## R7. ConceptScene: chú thích ô dài, không có dấu hiệu „cầm kiện lên“

- **Ảnh:** `TestEvidence/review-P801/07-concept-caption-material.png` (bước `material`: „Mat.-Nr and Bez. – Material number and description. Pick up the STECKER C carton and compare both with its label.“)
- **Góp ý:** chú thích dài, một khối chữ liền không ngắt dòng, không có signaling. Người học loay hoay mãi mới hiểu là phải cầm kiện trên bàn lên để xem nhãn và so với LVS.
- **Hiện trạng:** chú thích `Tour Markers/Caption material` (#78, khung 300 × 240, chữ auto-size 20–26, tối đa 2 câu) bên trái màn hình; mũi tên cam chỉ vào ô Mat.-Nr và Bez. Lời dặn hành động („Pick up the STECKER C carton…“) nằm cuối câu thứ 2, cùng kiểu chữ với phần giải thích. #83 đã đặt mũi tên cam `STECKER C Marker` trên carton ở mép bàn ở bước này và bước `verp_einh`, nhưng người học không nhận ra (carton nằm thấp, ngoài hướng nhìn khi đọc chú thích ở độ cao màn hình).
- **Ghi chú phân tích:** tách phần „làm gì“ khỏi phần „là gì“: một dòng hành động ngắn, nổi bật (biểu tượng tay/cầm, màu khác, dòng riêng), phần giải thích ngắn lại. Tín hiệu trên kiện phải lọt vào tầm nhìn lúc đang đọc chú thích (mũi tên lớn hơn, nhấp nháy/nảy nhẹ, hoặc đường nối từ chú thích xuống carton), và có phản hồi khi đã cầm (ví dụ chú thích đổi sang „So sánh …“). Hợp với R4 (giải thích hiện cạnh kiện khi cầm): khi cầm STECKER C, chú thích ở kiện có thể chỉ thẳng vào Mat.-Nr trên nhãn và trên LVS. Cùng vấn đề chữ dày với R1, R2.

## R8. ConceptScene bước `cartons`: phép tính, số trên LVS và carton trong thùng ở 3 chỗ khác nhau

- **Ảnh:** `TestEvidence/review-P801/08-concept-cartons-board.png` (bảng tour: „Number of cartons – 120 : 40 = 3 cartons“, phần còn lại của bảng trống)
- **Góp ý:** lại split attention: người học phải tìm Soll và Verp.-Einh. trên LVS, nhìn sang bảng này đọc phép tính, rồi nhìn xuống thùng xem carton, rồi tự ghép ba thứ lại. Rất tốn sức.
- **Hiện trạng:** bước `cartons` dùng bảng tour bên phải màn hình (#78, một trong 4 bước còn bảng, xem R6). #83 thêm `Carton Count Demo`: 3 carton STECKER C xuất hiện lần lượt (0,6 s) ở phía sau `PlasticBox_Order`, mỗi carton có thẻ „40“, „80“, „120 = Soll“; thùng đơn ở bên phải dưới bàn (x ≈ 0,45, y 0,77), còn Soll (120) và Verp.-Einh. (40) ở nửa trái màn hình. Ba chỗ cách nhau hàng chục cm và khác độ cao.
- **Ghi chú phân tích:** đặt phép tính ngay cạnh hai ô nó dùng (chú thích sát Soll/Verp.-Einh. như các bước một ô của #78, có mũi tên từ „120“ và „40“ vào phép tính) và/hoặc ngay trên thùng đơn nơi carton xuất hiện, để mắt chỉ đi một đường. Thẻ số trên carton nên dùng cùng màu/kiểu với ô LVS mà nó ứng với. Thuộc nhóm bố cục ConceptScene cùng R4–R7; quyết chung.

## R9. ConceptScene: carton trông giống hệt kiện thật nhưng không cầm được

- **Ảnh:** `TestEvidence/review-P801/09-concept-static-cartons.png` (3 carton của `Carton Count Demo` trong thùng đơn, thẻ „40 / 80 / 120 = Soll“)
- **Góp ý:** theo thói quen, người học chủ động đưa tay cầm các carton này nhưng không cầm được, cũng không biết vì sao. Người học bắt đầu nghi ngờ: trong môi trường này không phải vật nào cũng cầm được à? Làm mất niềm tin vào quy tắc tương tác.
- **Hiện trạng:** decision 025 / #83 chủ ý làm các vật minh hoạ ở Concept „chỉ để xem“: `Carton Count Demo` (3 carton), `Stock STECKER C` (6 carton trong thùng lưu), thùng mẫu `EKZ Sample 003/000` với các vòng RING D, đều không có `Grabbable`, `PackageData` hay collider. Chúng dùng đúng mô hình và nhãn của kiện cầm được (STECKER C), nên nhìn không phân biệt được. Tay chạm vào thì xuyên qua, không có phản hồi gì.
- **Ghi chú phân tích:** hai hướng: (a) mọi thứ trông như kiện thì cầm được (thêm `Grabbable`, nhưng phải giữ demo/đếm không bị phá, không log `grab_*` lẫn với kiện học, và khi thả thì trả về chỗ cũ); (b) vật chỉ để xem phải trông khác rõ (mờ/bán trong suốt, viền, kiểu „ảnh minh hoạ“) và có phản hồi khi tay chạm (rung nhẹ, chữ nhỏ „chỉ để xem“). Quy tắc chung cần thống nhất cho mọi scene: điều gì báo cho người học biết vật nào tương tác được (affordance). Liên quan R3 (cỡ kiện), R8 (vị trí carton đếm), memory „show, don't tell“.

## R10. ConceptScene: hai thùng mẫu EKZ trên làn giữa gây hiểu lầm, RING D xếp thẳng hàng không thực tế

- **Ảnh:** `TestEvidence/review-P801/10-concept-ekz-samples.png` („Empf Kz 003“: 4 vòng xếp 2 × 2; „Empf Kz 000“: 6 vòng đổ lộn xộn, một vòng thò ra ngoài thành thùng)
- **Góp ý:**
  1. Vì sao hai thùng này lại đặt ở làn giữa? Gây hiểu lầm (làn giữa là đường đi của thùng, decision 019).
  2. Dùng vòng RING D để minh hoạ EKZ 3 không thuyết phục: ngoài thực tế không ai xếp từng vòng gọn gàng như vậy.
- **Hiện trạng / lý do cũ:** decision 025 (bước `empf_kz_003`/`empf_kz_000`) chọn RING D chiếc lẻ vì bảng slide 8 nói EKZ về từng chiếc, và để cho xem EKZ 000 (P811: được hỏi trong quiz nhưng chưa từng thấy). Chỗ đặt `Layout Slots/EKZ Sample 003/000` lấy từ #84: khoảng trống duy nhất giữa hai thùng, trên làn giữa. Decision 025 tự ghi: Basic dùng carton cho EKZ 3, mẫu dùng chiếc lẻ; EKZ 3 cho carton là đơn giản hoá D4. Vòng ở thùng 000 cắt xuyên thành thùng (lỗi đặt vật).
- **Ghi chú phân tích:** chọn vật mẫu khớp với cách người học sẽ làm ở bài 3 (Basic: carton xếp 2 × 3 cho EKZ 003), ví dụ thùng 003 có carton xếp ngay ngắn, thùng 000 có carton/túi đổ lộn xộn; hoặc hỏi `requirements-analyst` vật nào thực tế được xếp theo EKZ 3. Đặt mẫu ở chỗ không mang nghĩa luồng thùng (không trên làn), gần chú thích EKZ trên màn hình (R6/R8: chữ, ô LVS và vật cùng một chỗ). Câu quiz `ekz_003`/`ekz_000` và chữ `concept.tour.empf_kz_*` có thể phải đổi theo vật mới. Sửa vòng thò ra ngoài thành thùng. Liên quan R9 (vật chỉ để xem), decision 018/025, D4/D5.

## R11. ConceptScene: cụm câu hỏi luôn hiện sau khi làm xong, không tắt được

- **Ảnh:** `TestEvidence/review-P801/11-concept-quiz-stays.png` (câu 4/4 đã đúng: „You answered all questions correctly. Now choose "3. Order picking by EKZ 3" in the menu.“)
- **Góp ý:** làm xong quiz rồi mà cụm câu hỏi vẫn luôn hiện ở đó, không có cách nào tắt. Khi người học muốn học lại tour (bấm Back/Next), cụm này đứng chắn giữa, rất khó chịu.
- **Hiện trạng:** `PackageQuizController` mở cụm `AnswerOption` „for good“ khi tour lần đầu tới bước `material_flow` (#73/#78, decision 020/024) và không bao giờ ẩn lại, kể cả khi tour đi lùi hay „Von vorn“. Sau câu 4 đúng, cụm giữ dòng „You answered all questions correctly…“. Video P801 (khoảng 10:20–11:00): sau khi xong quiz người học lùi tour về bước 4 và 7, cụm vẫn hiện ngay dưới màn hình, cạnh thanh điều hướng.
- **Ghi chú phân tích:** ví dụ: xong quiz thì cụm thu lại thành một dấu „✓ 4/4“ nhỏ (hoặc ẩn hẳn, vì thẻ 3 đã có ✓ của #79); khi tour lùi khỏi bước 11 thì ẩn cụm, quay lại bước 11 thì hiện kết quả. Lời nhắc „chọn bài 3 trong Menu“ nên nằm ở Menu (mục bài 3 sáng xanh như Welcome #39) hơn là trên cụm câu hỏi. Không được làm mất trạng thái đã đúng (khoá Menu #42 dựa vào `ILessonCompletion`) và không ghi thêm `quiz_answer`. Liên quan R5 (thanh điều hướng cùng chỗ), R6.

## R12. MixedPickingScene: carton đổ lộn xộn, nên dùng EKZ 3 cho carton

- **Ảnh:** `TestEvidence/review-P801/12-mixed-cartons-jumbled.webp` (vị trí 1, carton KABELVERBINDER A nằm chồng chéo trong thùng đơn)
- **Góp ý:** ở Mixed, với hàng là carton thì dùng EKZ 3 hợp lý hơn về mặt nhìn: carton xếp ngay ngắn, không lộn xộn chồng lên nhau.
- **Hiện trạng:** cả hai vị trí của Mixed đều có `Empf Kz 000` (ô xám trên LVS, `OrderLine.empfKz` khai trên `MixedPickingContainer`), nên thả thế nào cũng được và không có ô xếp. Chỉ Basic có EKZ 003 với 6 ô `BoxSnapper` 2 × 3 và `EkzPlacementCheck` (#41, #56). Mixed giữ lại carton của vị trí 1 trong thùng khi làm vị trí 2 (decision 012).
- **Ghi chú phân tích:** đây là quyết định thiết kế khoá học, không chỉ hình thức: bài 4 hiện dạy „nhiều vị trí nối tiếp“, và bài 3 đã luyện EKZ 3. Lựa chọn: (a) vị trí 1 (carton) EKZ 003 có ô xếp, vị trí 2 (túi SCHRAUBE B) EKZ 000, vừa nhắc lại EKZ 3 vừa cho thấy hai EKZ trong một đơn (người học phải đọc Empf Kz mỗi vị trí, đúng thẻ 1 hiện nay); (b) giữ 000 và chỉ làm thùng gọn hơn. (a) cần ô xếp trong thùng đơn chừa chỗ cho hàng của vị trí 2, `EkzPlacementCheck`/`MixedPickingValidator` báo lỗi xếp, chữ thẻ/feedback mới, và log `EKZ_PLACEMENT` cho Mixed (R phải biết). Hỏi `requirements-analyst` EKZ thực tế cho carton vs túi. Liên quan R10 (mẫu EKZ ở Concept nên khớp với cách làm ở bài 3–4), decision 012, D4.

## R13. Nhãn trên túi SCHRAUBE B quá to so với túi

- **Ảnh:** `TestEvidence/review-P801/13-bag-label-too-big.png` (Mixed, túi SCHRAUBE B trong thùng đơn)
- **Góp ý:** nhãn quá to so với túi nilon.
- **Hiện trạng:** #36: túi có một sticker nằm ngang; trên ảnh sticker rộng gần bằng cả túi, chìa ra ngoài mép túi và thành tấm phẳng cứng đứng tách khỏi bề mặt túi tròn. Chữ auto-size phủ kín sticker (như R3). Cùng túi này có ở Concept và Mixed.
- **Ghi chú phân tích:** gộp với R3 (một chuẩn kích thước nhãn theo loại kiện): nhãn túi nhỏ hơn, nằm trong mép túi, ôm theo mặt túi (hoặc treo bằng thẻ như nhãn thật), cỡ chữ cố định đọc được ở 30–40 cm khi cầm. Kiểm lại người học vẫn đọc được Mat.-Nr và VPE khi túi nằm trong thùng (vị trí 2 Mixed, cảnh `labels`).

## R14. ComplexScene: cụm câu hỏi + bảng „Example“ vẫn hiện khi đã sang bước nhặt hàng

- **Ảnh:** `TestEvidence/review-P801/14-complex-quiz-stays.webp` (`Remainder Quiz` câu 2/2 và bảng `Worked Example` dưới màn hình)
- **Góp ý:** cụm quiz luôn hiện kể cả khi đã qua bước 1 của bài, chắn tầm nhìn cho các bước thực hành sau, rất phiền.
- **Hiện trạng:** `RemainderQuizController` (#19, #85): cụm `Remainder Quiz` và bảng `Worked Example` chỉ ẩn khi sang vị trí 2 (`PositionTexts`/`LineAdvanced`). Suốt lúc nhặt hàng vị trí 1 (sau khi trả lời đúng cả 2 câu, `PositionStockSwitcher.RevealFirstSet` đã đưa thùng lưu vào) cả hai vẫn đứng dưới màn hình, giữa người học và thùng đơn. Video P801 (khoảng 17:10–17:35): khi đặt vòng RING D vào thùng, cụm quiz và bảng Example che phần trên thùng.
- **Ghi chú phân tích:** sau câu 2 đúng (khi thùng lưu chạy vào) thu cụm lại hoặc ẩn hẳn; giữ lại phần người học còn cần khi nhặt (kết quả „2 carton + 5 chiếc“, một dòng nhỏ, đặt sát thùng đơn hoặc trên thẻ 2) thay vì cả bảng Example. Không được phá luật #58 (bấm nút đỏ trước khi đúng 2 câu chỉ hiện lời nhắc vàng). Cùng vấn đề với R11 (Concept quiz không tắt): thống nhất một quy tắc „xong thì thu lại“ cho mọi cụm câu hỏi/bảng tạm.

## R15. Carton có hai nhãn (mặt trước + nắp), nhãn quá to; chỉ để một nhãn trên nắp

- **Ảnh:** `TestEvidence/review-P801/15-ringd-two-labels.png` (2 carton RING D trong thùng đơn Complex, nhãn ở cả mặt trước và nắp)
- **Góp ý:** nhãn quá to; chỉ cần nhãn ở mặt trên của hộp, không cần nhãn mặt bên, vì hai nhãn làm rối mắt. Áp dụng cho mọi carton: chỉ một nhãn trên mặt trên.
- **Hiện trạng:** #36 thêm cho mỗi carton sticker trước `Cube` (~90 × 80 % mặt hướng về người học) và bản sao `Label Top` trên nắp, ở mọi scene; lý do khi đó: đọc được nhãn từ chỗ đứng khi carton nằm trong thùng lưu (cảnh `labels`). Chữ auto-size phủ kín sticker (R3).
- **Ghi chú phân tích:** bỏ sticker mặt trước, giữ một `Label Top` nhỏ hơn có lề. Cần kiểm: từ chỗ đứng (mắt ≈ 1,6 m, thùng lưu ở y ≈ 0,77) nắp carton có đọc được không; carton xếp nghiêng/đứng trong thùng (Mixed, R12) có còn thấy nhãn không; khi cầm lên ở Concept (R4, R7) người học phải xoay để thấy nắp. Đây là đảo ngược một phần #36, cần ghi lý do mới. Gộp với R3 và R13 thành một mục chuẩn nhãn cho mọi kiện.

## R16. ComplexScene: vòng và carton đổ lung tung trong thùng đơn; có nên một vị trí EKZ 3, một vị trí EKZ 0?

- **Ảnh:** `TestEvidence/review-P801/16-complex-jumbled-box.webp` (cuối vị trí 2: carton RING D và SCHLAUCHSCHELLE B chồng lên nhau, vòng RING D và vòng kẹp nằm lẫn trên carton, có vòng xuyên qua carton)
- **Góp ý / câu hỏi:**
  1. Thả vòng lung tung vào thùng như vậy có đúng với tài liệu training không?
  2. Ở bài này có nên kết hợp một vị trí EKZ 3 và một vị trí EKZ 0 không? (người dùng viết „1 ekz 3 và 1 ekz 3“, hiểu là một EKZ 3 + một EKZ 0; xác nhận lại khi phân tích)
- **Hiện trạng:** cả hai vị trí Complex đều `Empf Kz 000` (RING D 55/25, SCHLAUCHSCHELLE B 156/50, decision 016), không có ô xếp; hàng của vị trí 1 nằm lại trong thùng khi làm vị trí 2, nên cuối bài hai vật liệu lẫn vào nhau. Vật lý: vòng và carton xuyên qua nhau (cùng hiện tượng Ist nhảy 54/55/56 ở vị trí 1, video ~17:10).
- **Ghi chú phân tích:** câu 1 cần `requirements-analyst` đọc tài liệu: EKZ 0 cho chi tiết rời có thật là „đổ vào“ không, và hai vật liệu khác nhau có được nằm lẫn trong cùng thùng đơn không (hay phải tách/ngăn). Câu 2 gắn với R12 (Mixed) và R10 (mẫu EKZ ở Concept): cần một kế hoạch EKZ cho cả khoá (bài 3 chỉ 003, bài 4 và 5 kết hợp thế nào), tránh lặp và tăng độ khó dần (`instructional-design-reviewer`). Nếu một vị trí là EKZ 003 với chi tiết rời thì cần cách „xếp từng chiếc“ làm được bằng tay thật trong VR. Vật lý xuyên vật: kiểm collider của vòng (mesh/convex) và carton.

## R17. Băng chuyền ba làn trông lộn xộn, như thừa thanh/cạnh

- **Ảnh:** `TestEvidence/review-P801/17-lanes-extra-rails.webp` (Complex, cuối bài, nhìn xuống chỗ thùng đơn vừa chạy đi; một vòng RING D bị ném ra nằm trên băng)
- **Góp ý:** sao làn lại lung tung như vậy, có phải bị dư cạnh không?
- **Hiện trạng (từ ảnh, chưa kiểm trong Editor):** giữa làn giữa và làn phải có nhiều thanh ray xám đứng sát nhau/chồng lên nhau, các thanh không thẳng hàng và có thanh cắt ngang con lăn; con lăn của các làn lệch nhau. `RollerConveyorThreeLanes` là bản sao của `RollerConveyor` hai làn có thêm `MiddleLane` (#71/#72), cộng `CentreStrip` con lăn ở khe giữa; dưới đó còn mesh `WarehouseConveyorNetworkThreeLanes` (làn giữa nối vào làn đơn ở z 3). Khả năng: ray cạnh của làn cũ không bị bỏ khi chèn làn giữa, hoặc ray của prefab và của mesh mạng băng chồng lên nhau ở đoạn dưới/sau bàn.
- **Ghi chú phân tích:** đo trong Editor (ảnh từ trên xuống, cảnh `container` và một góc nhìn thấp) để biết các thanh thuộc object nào; bỏ ray thừa, căn con lăn ba làn cùng nhịp. Đổi prefab → cả Basic, Mixed, Complex, Concept (#84) cùng đổi; không đổi `path` của `LaneRide`/`OrderBoxConveyor`. Vòng rơi trên băng: vật bị ném ra khỏi thùng vẫn nằm lại trên băng sau khi thùng chạy đi (ổn, nhưng xem có cần dọn khi `OrderCompleted`).

## R18. Hai trạm trống minh hoạ: có tấm gỗ nằm vắt trên đường ray

- **Ảnh:** `TestEvidence/review-P801/18-empty-station-board.png` (một trạm trống ở xa, ~5 m, có thùng xanh, bàn và màn hình; một thanh gỗ nằm ngang trên băng)
- **Góp ý:** vì sao ở khu vực 2 trạm minh hoạ không người lại có tấm gỗ nằm trên đường ray?
- **Hiện trạng (chưa kiểm trong Editor):** hai trạm trống ở x ±5 thuộc `WarehouseConveyorNetworkThreeLanes` (#49 decision 013, #71/#72), dựng bằng code thành mesh gộp `Assets/Meshes/WarehouseConveyor*_<material>.asset` + trạm `RearBench`/thùng. Khả năng: thanh gỗ là bản sao `FrontBoard` (tấm gỗ phía trước bàn ở trạm học viên, nơi người học đứng) được dựng cho trạm trống nhưng đặt sai chỗ (lệch lên băng), hoặc thuộc mesh gỗ của mạng băng cũ hai làn còn sót.
- **Ghi chú phân tích:** tìm object/mesh chứa thanh gỗ đó; nếu là `FrontBoard` thì đặt lại cho khớp với trạm học viên (trạm trống nên trông giống hệt trạm thật) hoặc bỏ. CLAUDE.md dặn: đổi tuyến/mesh mạng băng thì dựng lại mesh bằng code, không thêm primitive. Có ở cả Basic, Mixed, Complex. Cùng nhóm „hình học băng chuyền“ với R17.

## R19. Ý tưởng: dùng mô hình ốc vít cho EKZ 0

- **Ảnh tham khảo:** `TestEvidence/review-P801/19-screws-model-idea.webp` (bộ „Screws Free 3D model“, nhiều loại ốc/vít kim loại)
- **Góp ý:** người dùng đang cân nhắc dùng ốc vít làm vật cho EKZ 0 (đổ vào, không cần xếp).
- **Hiện trạng:** chi tiết rời hiện là vòng RING D (Concept, Complex vị trí 1) và vòng kẹp SCHLAUCHSCHELLE B (`clamp`, mesh vòng, Complex vị trí 2); SCHRAUBE B đã có nhưng là túi 20 ST (Concept, Mixed), không phải chiếc rời.
- **Ghi chú phân tích:** hợp lý về thực tế (vít rời là hàng điển hình được đổ vào thùng), và giải quyết luôn R10/R16 (vòng xếp thẳng hàng không thuyết phục). Cần cân nhắc: (1) vít thật dài 2–5 cm, cầm từng chiếc bằng tay thật trong VR rất khó, và đếm „5 chiếc rời“ sẽ mỏi; có thể làm to hơn thực tế hoặc dùng vít cho vật chỉ đổ (EKZ 0) và vật to hơn cho phần phải đếm; (2) hiệu năng Quest: mỗi vít là mesh nhiều đa giác, cần bản ít đa giác + collider capsule đơn giản; (3) giấy phép của bộ mô hình (thường CC-BY: phải ghi tên tác giả), và người dùng tự tải về, kiểm file trước khi nhập vào `Assets/`; (4) đổi vật liệu thì đổi `PackageData` (`MaterialKey`, Mat.-Nr, VPE), nhãn, chữ thẻ/quiz và `XRPick.Orders.Tests`. Gộp vào kế hoạch EKZ chung (R10, R12, R16).
- **Phương án 2 (người dùng gửi thêm):** cáp nối dài CEE (dây đen cuộn, phích đỏ hai đầu), ảnh `TestEvidence/review-P801/19-cable-model-idea.png`. So với vít: to, dễ cầm bằng tay thật, cuộn cáp cứng làm một mesh được. Nhưng cáp thường bán theo chiếc (VPE 1) hoặc thùng ít chiếc, nên phép tính „carton + chiếc rời“ (Soll : VPE) kém tự nhiên; đổ cáp vào thùng cũng ít điển hình hơn đổ vít. Mô hình trông như quét ảnh (photogrammetry): texture nặng, cần giảm đa giác/texture cho Quest. Gợi ý: vì Concept có vật liệu STECKER C (phích cắm), cáp/phích có thể hợp làm hình cho STECKER C hơn là làm vật EKZ 0. Quyết cùng phương án vít.
- **Phương án 3 (người dùng hỏi: dùng cho EKZ nào?):** bộ phụ kiện ống có mặt bích (cút 45°/90°, tê, ống nối, tê có đồng hồ áp, tê có van tay đỏ), ảnh `TestEvidence/review-P801/19-pipe-fittings-idea.png`. Cỡ ~10–20 cm, cầm dễ bằng tay thật. Theo bảng EKZ trong app (slide 8, `concept.ekz.*`; nghĩa EKZ 2, 4–9 chưa được xác nhận, D21–D24): phụ kiện trơn (cút, tê, ống nối) chắc chắn, không hỏng khi chạm nhau → hợp EKZ 0 (đổ vào) hoặc EKZ 2 (bỏ bằng tay, nhiều chiếc một lần). Chiếc có đồng hồ hoặc van (mặt kính, tay van dễ gãy) → hợp EKZ 3 (từng chiếc, có thứ tự), hoặc EKZ 4–6 (có tấm ngăn) nếu sau này dạy thêm. Điểm mạnh: cùng một bộ cho thấy luôn vì sao EKZ khác nhau (hàng bền đổ được, hàng dễ hỏng phải xếp), dễ hiểu hơn vòng RING D xếp thẳng hàng (R10). Hình khối không xếp chồng gọn như carton, nên ô xếp EKZ 3 phải là khay/ô định sẵn. Cần `requirements-analyst` xác nhận ví dụ thực tế và người hướng dẫn xác nhận nghĩa EKZ (D21–D24) trước khi dạy EKZ khác 0 và 3.
