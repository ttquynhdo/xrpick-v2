# Góp ý trong buổi test tay theo checklist (2026-11-17)

Nguồn: góp ý người dùng gửi trong lúc điền `docs/manual-test-checklist-2026-11-17.md`. Ảnh (nếu có) chép vào `TestEvidence/review-checklist-2026-11-17/` (không commit).

Cách làm như các file review trước: chỉ ghi lại, chưa sửa, chưa mở mục backlog. Khi bạn báo đủ, chạy `review-to-backlog` cho cả file.

**Đã chuyển vào backlog (2026-11-17):** R1 → #141, R2 + R12 → #145, R3 → #150, R4 → #146, R5 → #151, R6 + R9 (+ dòng phản hồi bị cắt của R11) → #142, R7 → #149, R8 → #144, R10 → #148, R11 → #147, R13 → #140 (chỗ đặt, carton tự xê dịch) + #152 (viền từ đầu), R14 → mở lại #136, R15 → #143, R16 (câu hỏi) → #153 (cùng F2 phần (b) của `docs/review-notes-2026-11-17.md`).
**Gộp theo gốc lỗi (cùng ngày):** #142 + #148 → #154 (A, bố cục chữ cố định); #144 + #145 + #146 + #147 → #155 (B, luật chữ, decision chung); #136 + #149 + #150 → #156 (C, hiện/ẩn theo bước); #140 + #152 + #153 → #157 (D, đặt kiện khi thả); lỗ hổng test → #158. Đứng riêng: #141, #143, #151.

Mỗi mục R<n> có: **góp ý** (lời của bạn), **item checklist liên quan**, **hiện trạng** (đọc từ code/scene trên `main` bdf77d0), **ghi chú**.

---

## R1. Bảng nhập mã và bảng Menu hiện hai lớp (bị „duplicate“)

- **Góp ý:** „bảng nhập mã participant tại sao lại bị duplicate, tương tự bảng menu cũng bị duplicate“
- **Ảnh:** `TestEvidence/review-checklist-2026-11-17/R1_keypad_double.png`. Đây là bàn phím WelcomeScene (`Keypad Panel`, phiên chọn English). Bảng cong ở trước, phía sau lệch sang phải/xuống có một bảng thứ hai cùng màu, mép lòi ra ở cạnh phải và cạnh dưới.
- **Item checklist liên quan:** #11 (L10 nút ngôn ngữ), #64 (F6 Menu), #124, #126, #134 (Menu và bàn phím).
- **Hiện trạng (đọc nhanh scene, chưa chạy Play):**
  - Cả `EntryCanvas` lẫn `Menu` dùng mẫu canvas cong của Meta: một `Unity Canvas` phẳng được vẽ vào render texture, rồi `Canvas Mesh` / `CurvedUnityCanvas` hiện texture đó trên mặt cong.
  - Trong WelcomeScene có 3 cặp như vậy. Hai `Unity Canvas` nằm ở layer 4 (`Water`), một ở layer 5 (`UI`). Hai `Canvas Mesh` có `_renderingMode: 1`.
  - Giả thuyết cần kiểm: camera mắt vẫn thấy `Unity Canvas` phẳng gốc (layer của nó chưa bị loại khỏi culling mask của camera, hoặc rendering mode khiến canvas gốc không bị ẩn), nên người học thấy bảng phẳng phía sau bảng cong. Khác layer giữa các canvas (4 và 5) cũng cần xem lại.
  - Chưa xác nhận: có thể chỉ thấy trên kính (chế độ render của Quest), ảnh MCP trong Editor trước đây chưa ghi nhận lỗi này.
- **Ghi chú:**
  - Lỗi trình bày, không ảnh hưởng logic, nhưng làm bảng trông như vỡ và có thể gây nhầm chỗ chạm (hai lớp ở hai độ sâu).
  - Khi phân tích: kiểm cả 5 scene (Menu có ở cả 5), so với cách Meta sample `CurvedUnityCanvas` đặt layer và camera.

## R2. Thẻ mục tiêu: bỏ câu „So …“ / „That way …“ ở cả 5 scene, rồi cân lại lề cả khu thẻ

- **Góp ý:** „ở các bảng instructions của mọi scene thì câu mô tả thêm ví dụ như "That way you can focus fully on the order later" mang giá trị không nhiều, nên bỏ đi để giảm thiểu số chữ bắt người học đọc vì trong môi trường VR rất dễ gây quá tải. Trong lúc remove dòng mô tả ngắn ở card đầu tiên của tất cả các scene, nhớ review về consistent margin/padding để không bị rộng quá, hay hẹp quá, gây mất cân đối cho cả khu vực instructions“
- **Ảnh:** `TestEvidence/review-checklist-2026-11-17/R2_welcome_goal_card.webp` (WelcomeScene, EN: thẻ kem „What this is about“ + 3 thẻ bước, thẻ 1 có khung xanh).
- **Item checklist liên quan:** #87 (mẫu thẻ nhiệm vụ chung, H1 đọc thẻ 5 bài), #116 (thẻ Welcome).
- **Hiện trạng:** thân thẻ mục tiêu (`common.card.task_title` „Darum geht es“ / „What this is about“) = câu đậm „Sie lernen, …“ + câu „So …“ ở 5 key:
  - `welcome.card.task_body`: „So können Sie sich später ganz auf den Auftrag konzentrieren.“ / „That way you can focus fully on the order later.“
  - `concept.card.task_body`: „So nehmen Sie bei jedem Auftrag das richtige Material in der richtigen Menge.“
  - `basic.task.intro`: „So kommt die Ware unbeschädigt an.“
  - `mixed.card.task_body`: „So stimmen bei jeder Position Material und Menge, auch wenn Verpackung und EKZ wechseln.“ (câu dài nhất; còn chữ „Position“, xem memory „tránh chữ positions“)
  - `complex.card.task_body`: „So stimmt die Menge auch dann, wenn sie nicht in volle Kartons passt.“
  - Chiều cao mỗi thẻ do tool `InstructionShelfLayout` đặt theo chữ dài nhất (DE/EN), các thẻ cách nhau 1,2 cm, `Board` kết thúc 1,5 cm dưới thẻ cuối, `Post` kéo dài theo. Bỏ câu thì chạy lại tool ở 5 scene; Complex giữ thẻ đầu cao hơn 6 cm (để thẻ 3 ở trên thùng kho vị trí 2), nên ở Complex bảng có thể còn khoảng trống trên.
  - Trong ảnh: lề trong thẻ kem (chữ sát mép trái hơn thẻ bước?) và khoảng trống dưới thẻ 3 tới mép bảng cần đo lại khi phân tích.
- **Ghi chú:**
  - Câu „So …“ là quy định của decision 027 (bổ sung 2026-11-05: thẻ mục tiêu = „Sie lernen …“ + câu „So …“ nói lý do). Bỏ câu cần sửa/ghi bổ sung decision 027 (có thể qua `solution-decision`, instructional-design-reviewer sẽ cân lợi ích „biết vì sao“ so với tải đọc).
  - Hợp với memory „Show, do not tell in VR“ và #125/#132 (bớt chữ phải đọc).
  - Sửa chữ qua `localize-strings` (CSV → `ImportCsv`), rồi `InstructionShelfLayout` cả 5 scene, rồi `TextComfortAudit`.

## R3. WelcomeScene bước „Aufgaben“: băng „Welcome to XPick!“ đứng một mình, trông chơi vơi

- **Góp ý:** „bắt đầu vào grab box ở màn welcome thì hiển thị mỗi dòng welcome này cứ có cảm giác chơi vơi, mất cân đối“
- **Ảnh:** `TestEvidence/review-checklist-2026-11-17/R3_welcome_banner_alone.webp` (EN, sau „Zu den Aufgaben“: băng tiêu đề tối ở giữa trên cao, bên dưới chỉ có tường trống và pallet nền; giá thẻ bên trái đã có ✓ ở thẻ 1).
- **Item checklist liên quan:** #121 (W12, Welcome hai bước), #116, #86.
- **Hiện trạng:**
  - #121 (decision 038): phiên chạy hiện `courseRoots` = `Welcome`, `Info Panel`, `Tasks Button`. Bấm „Zu den Aufgaben“ → `OpenTasks` ẩn `objectiveRoots` (`Info Panel`, `Tasks Button`), hiện `taskRoots` (`Menu`, `BigRedButton`, `Interactables Obj`, `InstructionShelf`). Canvas `Welcome` (y 1.63) không nằm trong `objectiveRoots` nên vẫn còn.
  - Ở bước 1 băng này là phần đầu của `Info Panel` (mép trên bảng y 1.53, ngay dưới băng). Sang bước 2 bảng biến mất, băng còn lại một mình giữa khoảng trống, không gắn với vật gì ở bước làm việc (việc ở dưới thấp: hộp, nút đỏ, giá thẻ bên trái, Menu bên phải).
- **Ghi chú:**
  - Hướng có thể: (a) ẩn cả `Welcome` khi mở bước việc (thêm vào `objectiveRoots`); (b) thu băng thành tiêu đề nhỏ / đưa xuống gần khu làm việc; (c) giữ băng nhưng hạ xuống khớp chỗ trống. Cân với decision 038 (mục tiêu trước, việc sau) và R2 (bớt chữ).
  - Các scene khác không có băng tiêu đề như vậy; ẩn nó cũng làm Welcome bước 2 giống bố cục các bài sau.

## R4. ConceptScene thẻ 1 „Etikett lesen“ thừa: việc đọc nhãn đã nằm trong tour (thẻ 2), và lúc đó người học chưa biết thuật ngữ trên nhãn

- **Góp ý:** „instructions ở phần concept có bước 1 là read the label nhưng thực ra bước này là 1 step của bước 2 rồi nên tách riêng này làm dư thừa. Hơn nữa, nếu giữ cứ để vậy thì khi cầm box lên nhìn các nhãn nhưng cũng không hiểu gì vì chưa được học về thuật ngữ“
- **Ảnh:** `TestEvidence/review-checklist-2026-11-17/R4_concept_cards.webp` (EN: thẻ mục tiêu + 3 thẻ bước, khung xanh ở thẻ 1).
- **Item checklist liên quan:** #79/#89/#92 (thẻ Concept, mở thanh tour), #104, #136/#138 (mũi tên STECKER C), R2 (câu „So …“ của cùng thẻ mục tiêu).
- **Hiện trạng:**
  - Thẻ 1 `concept.card.step1_*` „1. Etikett lesen“ / „Nehmen Sie einen Karton aus dem Lagerbehälter und lesen Sie das Etikett.“ Thẻ 2 „2. LVS kennenlernen“ (đọc chú thích, „Weiter“), thẻ 3 „3. Fragen beantworten“.
  - `ConceptCardProgress`: lần cầm carton STECKER C đầu tiên → ✓ thẻ 1 → `WmsFieldTour.Open()` (thanh tour hiện, khung sang thẻ 2); không cầm thì thanh tự mở sau `autoOpenSeconds` 60 s. Nghĩa là thẻ 1 hiện là cổng mở tour bằng một lần cầm.
  - Trong tour, bước 2 `material` đã bảo cầm carton và so nhãn với Mat.-Nr / Bez. (`concept.tour.material.action`, có mũi tên cam), bước 3 `verp_einh` so VPE trên nhãn với Verp.-Einh. Vậy việc đọc nhãn được làm lại, lần này có giải thích từng ô.
  - Ở thẻ 1, nhãn ghi `NAME` / `Mat.-Nr: …` / `VPE: n ST` mà chưa ai nói các chữ đó là gì → đọc mà không hiểu (đúng như góp ý).
- **Ghi chú:**
  - Hướng có thể: bỏ thẻ 1 (còn 2 thẻ bước: tour, câu hỏi), thanh tour hiện ngay từ đầu (hoặc sau một khoảng ngắn); hoặc đổi thẻ 1 thành việc thuần thao tác (vd. nhìn LVS) mà không bảo đọc nhãn. Cân với: lý do ban đầu của thẻ 1 (#89: tour bar hiện từ thẻ 2 để người học nhìn quanh/cầm thử trước), log `tour_step` bắt đầu khi thanh mở (R/`tour_steps.csv`), thẻ có ✓ (#79, `ConceptCardTracker` + test EditMode), `InstructionShelfLayout`, Welcome đã dạy cầm.
  - Đổi số thẻ = đổi bố cục Concept → cần `layout_version` mới và báo data-analyst (Concept không gộp dữ liệu trước/sau, decision 024).
  - Liên quan decision 024 (thẻ Concept, mục 6–7), 026 (#89), 032 (một chuỗi thao tác ở thùng kho).

## R5. Mặt bàn và băng con lăn lệch nhau ở hai mép, trông mất cân đối

- **Góp ý:** „đây là hình ảnh mép phải và mép trái bàn. Diện tích bàn và khu vực rãnh đi khác nhau nhìn rất mất cân đối khó chịu“
- **Ảnh:**
  - `TestEvidence/review-checklist-2026-11-17/R5_desk_right_edge.png` (mép phải: mặt bàn màu be thò ra ngoài thùng đơn xanh và ray ngoài bên phải một dải rộng, góc bàn trơ trọi).
  - `TestEvidence/review-checklist-2026-11-17/R5_desk_left_edge.webp` (mép trái: làn trái với con lăn chạy đè lên mặt bàn be, mặt bàn hết giữa chừng dưới con lăn, nửa ngoài làn không có bàn; phía sau là rèm của mái che `LeftLaneHood`).
- **Item checklist liên quan:** #51 (bàn sau đứng trên bàn), #71/#72 (ba làn), #95 (ray băng chuyền).
- **Hiện trạng:**
  - Bàn là `Desk` của SDK (mặt y 0.77), băng là prefab `RollerConveyorThreeLanes` (#71/#72) đặt lên trên: `LeftLane` x −0.75, `WorkSpot` thùng kho x −0.346, `MiddleLane` x 0.054, `RightLane` thùng đơn x 0.453; các làn rộng 0.40, ray ngoài cao, ray chung thấp giữa các làn (#95).
  - Bàn có từ trước khi có ba làn; khi thêm làn trái (#71) băng chìa ra ngoài mép trái bàn, còn bên phải bàn rộng hơn băng. Chưa đo số chính xác mép bàn (cần đọc `Desk` trong scene).
  - Chưa rõ ảnh chụp ở scene nào; bố cục giống nhau ở Basic, Mixed, Complex và Concept (#84).
- **Ghi chú:**
  - Hướng có thể: co/dời mặt bàn cho khớp đúng chiều ngang băng (hai mép trùng ray ngoài, hoặc thò ra đều nhau ở hai bên); hoặc bỏ mặt bàn dưới băng và để băng có khung chân riêng như trạm thật (ảnh trạm `docs/requirements/wms-specs/kommissionierplatz-photo.md`).
  - Phải giữ: 4 chân `RearBench` đứng trên mặt bàn (#51), vị trí giá thẻ (`InstructionShelf` x −1.15), nút đỏ, tầm với thùng; sửa một chỗ cho cả 4 scene (prefab hoặc cùng số).

## R6. ConceptScene bước `cartons`: dòng hành động tràn ra ngoài nền chú thích

- **Góp ý:** „giải thích bị ra ngoài“
- **Ảnh:** `TestEvidence/review-checklist-2026-11-17/R6_caption_cartons_overflow.png` (EN, bước „Number of cartons“: khung xanh Soll 120 : Verp.-Einh. 40 = 3 cartons; bên dưới, dòng có biểu tượng bàn tay „Put cartons from the storage container into the order container until Ist equals Soll.“ chạy 6 dòng hẹp, nằm ngoài/dưới nền xanh, trên nền bàn).
- **Item checklist liên quan:** #91 (bước `cartons`), #104 (dòng hành động), #132 (cỡ chữ cố định, K1 đọc chú thích).
- **Hiện trạng:**
  - `Action Row` (icon bàn tay + `TXT_Action` = `concept.tour.<id>.action`) được `WmsFieldTour.PlaceActionRow` gắn lúc chạy vào chú thích của bước. Các caption khác có `VerticalLayoutGroup` + `ContentSizeFitter` nên nền giãn theo chữ; riêng `Caption cartons` (360 wide) **không có layout group, đặt tay** → nền không giãn, dòng hành động nằm dưới nền.
  - #132 đặt chữ cố định (body/action 34), nên dòng `concept.tour.cartons.action` (EN 15 từ, DE „Legen Sie Kartons aus dem Lagerbehälter in den Auftragsbehälter, bis Ist und Soll gleich sind.“ còn dài hơn) xuống 6+ dòng trong bề rộng hẹp. Ảnh MCP của #132 có thể chưa chụp bước này với dòng hành động.
  - Chú thích này nằm bên phải màn hình (Rest phía phải), mép trái hẹp.
- **Ghi chú:**
  - Hướng có thể: cho `Caption cartons` cùng cấu trúc layout như các caption khác (nền bao cả dòng hành động), nới rộng caption, hoặc rút ngắn câu (cùng tinh thần R2: bớt chữ).
  - Kiểm lại mọi bước có dòng hành động ở cả DE/EN (`TextComfortAudit` + ảnh), không chỉ bước `cartons`.

## R7. ConceptScene: biển „Lagerbehälter“ / „Auftragsbehälter“ lúc hiện lúc mất

- **Góp ý:** „tại sao ở màn concepts label cho 2 thùng kho và thùng đơn cứ lúc hiển thị, lúc biến mất“
- **Ảnh:** `TestEvidence/review-checklist-2026-11-17/R7_concept_box_signs_missing.webp` (EN/DE?, thùng kho trái và thùng đơn phải đều đầy carton STECKER C, thành sau của cả hai thùng không có biển).
- **Item checklist liên quan:** #111 (H2: đọc biển ở bước 5, 6, 11), #92, #81.
- **Hiện trạng:**
  - Đây là thiết kế, không phải lỗi: ở Concept hai `Box Sign` (con của `PlasticBox_Storage` / `PlasticBox_Order`) là **marker của tour**, chỉ hiện ở bước 5, 6 và 11 (`WmsFieldTour.steps[].markers`); bước khác tắt, „Von vorn“ tắt. Ở Basic, Mixed, Complex biển luôn hiện.
  - Lý do ban đầu (#92, decision 032 / #84): mỗi bước chỉ hiện vật của bước đó (bớt rối), biển xuất hiện đúng lúc chữ của bước nói tới thùng.
  - Người thử không biết quy tắc này nên thấy biển „tự biến mất“ khi bấm „Weiter“; tên hai thùng còn được dùng ở thẻ 1 („aus dem Lagerbehälter“), bước `cartons` (R6) và nhiều dòng hành động ngoài bước 5/6/11 → lúc chữ nhắc tên thùng thì biển lại không có.
- **Ghi chú:**
  - Hướng có thể: cho biển luôn hiện ở Concept như 3 bài sau (một mẫu một nghĩa, #111; tên thùng là thuật ngữ cần thấy suốt bài), còn bước 5/6/11 chỉ thêm nhấn mạnh (khung/mũi tên); hoặc giữ marker nhưng hiện ở mọi bước có chữ nhắc tên thùng.
  - Kiểm biển không bị thanh tour/carton che (H2 của #111), và lúc thùng chạy ở bước 11 biển đi theo thùng.

## R8. ConceptScene bước 8–9: bỏ câu „xem ví dụ bên trái màn hình“, giữ câu dẫn sang bảng EKZ bên phải

- **Góp ý:** „ở những bước mô tả EKZ mà có dòng look at the example to the left of the screen thì có vẻ không cần thiết lắm vì 2 example hiển thị ngay lập tức và có chú thích to nên người học tự kết nối được là đó chính là example. bỏ chú thích đó đi để giảm chữ. Nhưng phần giới thiệu bảng EKZ đầy đủ bên phải thì vẫn phải giữ vì lúc đó người học vẫn đang nhìn phía trái đó nên cần text guide họ hướng navigate“
- **Ảnh:** `TestEvidence/review-checklist-2026-11-17/R8_step8_ekz000.png` (bước 8 „Empf Kz“: thân + dòng hành động „Look at the example „Empf Kz 000“ to the left of the screen.“), `R8_step9_ekz003.webp` (bước 9 „Other value: Empf Kz 003“: thân + „Compare the two examples to the left of the screen. Then look at the EKZ overview on the right.“; dưới là hai thùng mẫu với thẻ „Empf Kz 000 = EKZ 0“ + „Example – not for picking up.“ và „Empf Kz 003 = EKZ 3“).
- **Item checklist liên quan:** #137 (H1 đọc bước 8–9), #94, #101, #107, #104 (dòng hành động).
- **Hiện trạng:**
  - `concept.tour.empf_kz_000.action`: „Sehen Sie sich links neben dem Bildschirm das Beispiel „Empf Kz 000“ an.“ / „Look at the example „Empf Kz 000“ to the left of the screen.“
  - `concept.tour.empf_kz_003.action`: „Vergleichen Sie die beiden Beispiele links neben dem Bildschirm. Sehen Sie danach rechts die EKZ-Übersicht an.“ / „Compare the two examples … Then look at the EKZ overview on the right.“
  - Thân hai bước cũng đã nói „wie im Beispiel“ / „like in the example“, và thẻ mẫu ghi to „Empf Kz 000 = EKZ 0“ / „Empf Kz 003 = EKZ 3“, nên câu chỉ chỗ ví dụ lặp lại.
  - `Action Row` hiện ở mọi bước (#89/#104); quy tắc decision 036 (#104): mỗi `.action` nói việc + mục đích, không bao giờ chỉ „Weiter“. Bước 8 bỏ câu thì dòng hành động trống → cần cách xử lý (ẩn `Action Row` khi key rỗng, hoặc câu khác).
- **Ghi chú:**
  - Đề xuất: bước 8 bỏ dòng hành động (hoặc ẩn); bước 9 chỉ giữ câu dẫn sang phải, vd. „Sehen Sie rechts die EKZ-Übersicht an.“ / „Look at the EKZ overview on the right.“ (cần thêm mục đích theo decision 036, hoặc sửa decision đó).
  - Cùng hướng R2 (bớt chữ) và memory „Show, do not tell in VR“. Đổi chữ qua `localize-strings` + `german-reviewer`.

## R9. Bảng EKZ: nhãn „In the exercise“ bị nền trắng cắt mất chữ „I“

- **Góp ý:** „label in the exercise bị highlight cắt mất chữ i“
- **Ảnh:** `TestEvidence/review-checklist-2026-11-17/R9_ekz_overview_badge_clipped.webp` (EN, `EKZ Overview` ở bước 9: hàng EKZ 0 và EKZ 3 có khung trắng; dưới dòng mô tả là ô nền trắng chữ đen „n the exercise“, chữ „I“ đầu và một phần chữ cuối bị cắt ở cả hai hàng).
- **Item checklist liên quan:** #57 (E8b đọc bảng EKZ), #137 (bước 9), #132 (cỡ chữ cố định).
- **Hiện trạng:**
  - Key `concept.ekz.in_training` „In der Übung“ / „In the exercise“ trong ô nền trắng ở hàng EKZ 0 và EKZ 3 (#57, decision 018).
  - Chữ rộng hơn ô nền (hoặc lề trong = 0 / căn giữa với overflow), nên hai đầu bị ô cắt (mask). Có thể do #132 đặt cỡ chữ cố định (bỏ auto-size) mà không nới ô; EN „In the exercise“ dài hơn DE „In der Übung“, nên DE có thể không lỗi.
- **Ghi chú:**
  - Sửa: nới ô theo chữ dài nhất DE/EN (+ lề trong đều hai bên) hoặc cho ô co theo chữ (`ContentSizeFitter`); không thu nhỏ chữ (quy tắc #132). Đo bằng `TMP_Text.GetPreferredValues`, rồi `TextComfortAudit`.
  - Kiểm luôn các ô nền khác có chữ cố định ở Concept/Complex (cùng kiểu lỗi).

## R10. Chú thích tour bên phải màn hình quá hẹp: mỗi dòng chỉ 2–3 chữ, mắt phải nhảy dòng liên tục

- **Góp ý:** „những step mà có nội dung hiển thị dài thì cần cố định xem kích thước chiều rộng là bao nhiêu để tránh 1 dòng hiển thị được có 3 chữ, làm mắt bị điều chỉnh liên tục khó chịu vô cùng“
- **Ảnh:** `TestEvidence/review-checklist-2026-11-17/R10_caption_step_narrow.png` (EN, bước `not_used` „Not in the exercise“ trong `Caption step` bên phải màn hình: tiêu đề 2 dòng, thân 4 dòng, dòng hành động 5 dòng, mỗi dòng 2–4 từ).
- **Item checklist liên quan:** #132 (K1), #129 (N9 đọc chữ ở 1,2–1,3 m), #105, #89.
- **Hiện trạng:**
  - #132: chú thích tour cỡ cố định (tiêu đề 38, thân/hành động 34), chú thích **bên trái** màn hình rộng 640, **bên phải** rộng 360 (giữ mép phải để đường dẫn vẫn chạm). `Caption step` (bước intro, not_used, material_flow) là bản sao `Caption rest`, nằm bên phải → 360 wide; `Caption cartons` cũng 360 (R6).
  - Với chữ 34 trong 360 đơn vị, một dòng chỉ chứa ~15–20 ký tự (2–3 từ tiếng Anh; tiếng Đức từ dài hơn, còn tệ hơn).
  - Ở ảnh này dòng hành động còn lặp lại thân („You don't need the marked areas here“ ↔ „Look at the marked areas: you don't need them“), xem thêm R2/R8 (bớt chữ).
- **Ghi chú:**
  - Đề xuất: đặt một bề rộng tối thiểu chung cho mọi chú thích (theo số ký tự/dòng, vd. ~35–50 ký tự ở cỡ 34, cần đo), ghi vào `docs/text-comfort-checklist.md` thành một ngưỡng mới, và để `TextComfortAudit` báo chữ nào dưới ngưỡng ký tự/dòng.
  - Bên phải màn hình chỗ có hạn (bảng `EKZ Overview`, `Sample Description`, Menu) → có thể phải dời chú thích, đổi cạnh (bên trái có 640), hoặc rút chữ. Đo lại mọi bước DE/EN.
  - Cùng nhóm với R6 (chú thích `cartons` tràn) và R9 (ô chữ cố định bị cắt): cả ba là hệ quả của #132 (cỡ cố định) chưa đi kèm luật bề rộng.

## R11. Câu hỏi và đáp án ở hai nơi xa nhau (split attention): gộp vào một thẻ, câu hỏi trên, đáp án dưới

- **Góp ý:** „câu hỏi 1 nơi, đáp án 1 nơi xong tần suất so thì nhiều nhìn rất khó chịu, vì cần phải so liên tục các chữ trong câu hỏi với các lựa chọn trong đáp án nên để xa làm split attention. Tốt nhất để luôn vào thẻ đáp án, bố cục câu hỏi trên, đáp án dưới như cách các bài kiểm tra trên sách vẫn làm“
- **Ảnh:**
  - `TestEvidence/review-checklist-2026-11-17/R11_question_caption.png` (Concept, `Caption question` bên phải màn hình: „Question 2 of 4 / The WMS shows Mat.-Nr 738190. Which label matches?“, cũng hẹp như R10).
  - `R11_answer_row.png` (cụm A/B/C thấp gần tay: „A: Put in one by one and in order“, „» B: Put in unordered“ được chọn, dòng phản hồi „Not correct / … the cartons must lie one by one and in order.“ — dòng thứ hai bị mép dưới cụm cắt mất một phần).
- **Item checklist liên quan:** #129 (N9: „câu hỏi (trên, phải) và A/B/C (dưới) cách khoảng 50° có thấy phiền không“ → góp ý này trả lời là **có**), #130 (Complex), #132 (K1), #93.
- **Hiện trạng:**
  - Decision 040 „read far, touch near“ (#129 Concept, #130 Complex vị trí 1): câu hỏi + „Frage n von 4/3“ nằm ở `Tour Markers/Caption question` / `Screen Captions/Caption question` cạnh màn hình (~1,2–1,3 m), còn A/B/C + dòng phản hồi ở cụm thấp gần tay (0.05, 1.01, 0.40) nghiêng 50°. Lý do: tránh đọc chữ ở vùng 0,5–0,8 m (T-ngưỡng Meta trong `docs/text-comfort-checklist.md`) và giữ nút trong tầm chạm 42–46 cm.
  - Hệ quả: người học phải nhìn qua lại ~50° và đổi tiêu cự giữa câu hỏi xa và đáp án gần cho mỗi lựa chọn; câu hỏi `label_by_matnr` lại bắt so số (Mat.-Nr) giữa câu hỏi, đáp án và nhãn carton.
  - Phụ: dòng phản hồi ở ảnh 2 bị cắt ở mép dưới cụm (cùng nhóm R6/R9/R10: chữ cố định, ô không nới).
- **Ghi chú:**
  - Mâu thuẫn trực tiếp với decision 040 → cần `solution-decision` lại (xr-ux-reviewer + instructional-design-reviewer): split attention (Mayer/Sweller) vs. vùng chữ 0,5–0,8 m. Phương án lai: câu hỏi ngắn trên chính cụm đáp án (ở tầm chạm), còn dữ liệu cần so (LVS) ở màn hình; hoặc cả cụm ở ~0,8–1 m với nút to hơn / ray chạm xa.
  - Ảnh hưởng log: đổi bố cục quiz → `layout_version` mới (R tách Concept/Complex `position_1` theo layout, như "037").

## R12. BasicPickingScene: thẻ bước dài, rối, đọc lâu mới hiểu — cần ngắn lại

- **Góp ý:** „nội dung thẻ hướng dẫn ở bài này dài, rối, đọc mãi mới hiểu, cần ngắn gọn lại“
- **Ảnh:** `TestEvidence/review-checklist-2026-11-17/R12_basic_cards.png` (EN, Basic: thẻ mục tiêu + 3 thẻ bước, thẻ 1 có khung xanh, thân 6 dòng).
- **Item checklist liên quan:** #87 (H1 đọc thẻ 5 bài), #62 (T10), R2 (câu „So …“ của thẻ mục tiêu này: „That way the goods arrive undamaged.“).
- **Hiện trạng (chữ trên thẻ):**
  - Thẻ 1 `basic.step1.text`: „Lesen Sie im LVS ab: Materialnummer (Mat.-Nr), Sollmenge (Soll), VPE (Verp.-Einh.), EKZ (Empf Kz). Rest: So viele Stück bleiben nach dem Auftrag im Lagerbehälter.“ — liệt kê 4 ô, mỗi ô hai tên (tên đầy đủ + nhãn LVS trong ngoặc), thêm một câu định nghĩa Rest (ô không cần cho việc ở bài này). Đây là phần rối nhất.
  - Thẻ 2 `basic.step2.text`: „Rechnen Sie aus, wie viele Kartons Sie brauchen. Legen Sie sie so in den Auftragsbehälter, wie es das Empf Kz verlangt.“
  - Thẻ 3 `basic.step3.text`: „Prüfen Sie im LVS, ob Ist und Soll gleich sind. Drücken Sie dann den roten Knopf (F6).“
  - Ghi chú phụ: CSV còn key `basic.card.step1_body` gần trùng (bản „LVS“ trong EN) — kiểm xem key nào đang dùng, bỏ key thừa.
  - Decision 027: thân thẻ bước tối đa 2 câu — thẻ 1 đúng luật nhưng câu đầu là một danh sách dài.
- **Ghi chú:**
  - Concept đã dạy các ô (tour), nên Basic có thể chỉ gọi đúng nhãn LVS, không cần tên đầy đủ trong ngoặc; bỏ câu Rest. Vd. (chỉ hướng): „Lesen Sie Mat.-Nr, Soll, Verp.-Einh. und Empf Kz ab.“
  - Rà cùng lúc thẻ bước của Mixed và Complex (cùng mẫu `common.card.step*`), theo một luật độ dài chung (vd. ≤ N từ/thẻ), đi cùng R2 và R10; `localize-strings` + `german-reviewer`, rồi `InstructionShelfLayout`.

## R13. MixedPickingScene vị trí 1: carton tự di chuyển, chỗ đặt không hiện từ đầu, chỗ giữa (2) không giữ được carton → **không hoàn thành được bài** (chặn)

- **Góp ý:** „với màn mixed picking, có 1 box cầm xong cứ bị lộn xộn, không cần cầm chạm vào box mà vẫn di chuyển được box. Lúc mới vào thì thùng đơn cũng không hiển thị 3 snap interactor, phải 1 lúc sau khi đặt được 1 vật vào mới hiển thị. Vị trí snap ở giữa số 2 không thể đặt vào nổi, cứ bị trượt ra ngoài làm màn này bị stuck, không thể hoàn thành được“
- **Ảnh:**
  - `TestEvidence/review-checklist-2026-11-17/R13_mixed_carton_tilted.webp` (một carton KABELVERBINDER A nằm nghiêng, chống vào thành trước bên trái thùng đơn; bóng tay (outline) ở giữa thùng; chưa có viền chỗ đặt).
  - `R13_mixed_snap_places.webp` (carton 1 nằm đúng chỗ trên cùng bên trái; viền trắng của chỗ giữa trống; carton thứ hai nằm lệch ở nửa phải, tay outline đang chạm nhãn; carton thứ ba ở mép dưới).
- **Item checklist liên quan:** #94 (H1 xếp 3 carton vào ô bằng tay thật), #108 (H1 thả vào ô), #103 (T8 Mixed 3 carton vào 3 ô), #88 (H1), grab-snap-fix (B3/M3), #139 (vừa sửa `ContentsChanged` ở Mixed).
- **Hiện trạng:**
  - Vị trí 1 = Empf Kz 003: 3 chỗ đặt `BoxSnapper`, `(1)`, `(2)` ở cột trái thùng đơn, `EkzPlacementCheck` trên `PlasticBox_Order`; mỗi chỗ nhận 1 carton (`_maxSelectingInteractors` 1, vùng hover 16 × 11 × 10 cm); carton thả gần sẽ lấy chỗ gần nhất đang chạm.
  - Viền chỗ đặt: ở Mixed `showPlacesAtStart` **tắt có chủ ý** (#108, decision 037): viền chỉ hiện sau `EKZ_PLACEMENT` đầu tiên của vị trí 1 → đúng như người thử thấy, nhưng người thử hiểu là lỗi (không biết chỗ đặt ở đâu trước khi tình cờ đặt trúng).
  - `CartonSettler` (Basic, Mixed) đặt carton thả trong thùng mà không snap nằm phẳng lên đáy / lên kiện bên dưới (thân carton là trigger, chỉ sticker trước là solid). Carton tự xê dịch khi không cầm có thể do: Settler đặt lại, snap kéo carton sang chỗ gần nhất, hoặc vùng cầm (#103: sticker `Cube` sai chỗ làm tay „cầm từ xa“; Mixed chưa được kiểm như Complex) — chưa xác định.
  - Chỗ giữa `(1)`/„2“ trượt ra: chưa tái hiện. Nghi vấn: vùng hover của chỗ giữa chồng lên hai chỗ bên (carton chọn chỗ gần nhất đã đầy → bị đẩy ra), hoặc `_maxSelectingInteractors` 1 + carton khác đang hover chỗ đó, hoặc Settler đè lên snap.
  - #139 (commit bdf77d0) mới đổi `MixedPickingContainer`/`MixedPickingValidator` (không phát `ContentsChanged` cho kiện vị trí đã xong; chỗ đặt EKZ chỉ xoá thẻ khi dòng cần chỗ đặt) — cần loại trừ hồi quy, dù phần snap không đổi.
- **Ghi chú:**
  - Mức ưu tiên cao nhất trong đợt (P0: người học kẹt, không qua được bài 4, Complex bị khoá theo thứ tự bài ở mã nghiên cứu).
  - Khi phân tích: dùng `superpowers:systematic-debugging`, tái hiện bằng Meta XR Simulator hoặc kính (MCP không cầm tay thật được), log `grab_*`/`EKZ_PLACEMENT`/`container_*` của phiên này nếu có.
  - Về viền: cân lại decision 037 cho Mixed (hiện viền ngay từ đầu vị trí 1 như Basic), vì người học không có gợi ý nào khác về chỗ đặt.

## R14. ConceptScene: sau khi đã cầm carton, bấm „Weiter“ mũi tên cam vẫn nháy lên một khung hình rồi mới tắt (hồi quy của #136)

- **Góp ý:** „sau bước cầm box lên thì mũi tên vàng đã biến mất, nhưng khi bấm Next thì mũi tên vàng nháy phát rồi biến mất như bị lỗi“
- **Ảnh:** `TestEvidence/review-checklist-2026-11-17/R14_marker_flash.png` (thùng kho Concept với carton STECKER C; mũi tên không còn trong ảnh — ảnh chụp sau cú nháy).
- **Item checklist liên quan:** #136 (H1: cầm carton ở bước 2, thả, „Weiter“ → bước 3 không có mũi tên → **❌ một phần**: có nháy), #138.
- **Hiện trạng (đã đọc code):**
  - `Assets/Scripts/Concept/MarkerHideOnGrab.cs`: khi tour bật marker ở bước mới, `OnEnable` chỉ đặt `hideNow = grabbedOnce`; việc tắt thật (`gameObject.SetActive(false)`) chờ tới `Update`. Ghi chú trong code: không tắt được GameObject ngay trong lúc nó đang được bật.
  - Tour bật marker trong lúc xử lý sự kiện bấm nút; component vừa bật giữa vòng Update thường chỉ nhận `Update` ở khung sau → mũi tên được vẽ ít nhất 1 khung (trên Quest 72–90 Hz là ~11–14 ms, đủ thấy một cú nháy; có thể thêm `PanelFade`/`MarkerBob`).
  - Test MCP của #136 chỉ kiểm trạng thái `activeSelf` sau khi bấm, không bắt được khung hình nháy.
- **Ghi chú:**
  - Hướng sửa: ẩn ngay trong `OnEnable` mà không tắt GameObject (vd. tắt `Canvas`/renderer hoặc `CanvasGroup.alpha = 0` khi `grabbedOnce`), hoặc để `WmsFieldTour` hỏi marker trước khi bật (không bật marker đã „xong“). Cách đầu ít đụng tour nhất.
  - Thêm case test: đọc trạng thái hiển thị ngay trong khung bấm (không chỉ khung sau).

## R15. Mixed/Complex: tiếng Anh dùng „Position 1/2“, không thống nhất với „order line“

- **Góp ý:** „ở màn Mixed đang để là Position thì không consistent với tên gọi tiếng Anh thì phải“
- **Ảnh:** `TestEvidence/review-checklist-2026-11-17/R15_position_strip.png` (EN, `Position Strip` trên giá thẻ: „Position 1“ (gạch chân xanh) · „Position 2“; thẻ 1 có ✓, thân bắt đầu „… current position: …“).
- **Item checklist liên quan:** #128 (tên bài 4 „Several order lines“), #116, #97 (H2 đọc dòng position), memory „tránh chữ positions“.
- **Hiện trạng:**
  - Glossary: `Position` (DE) = EN **order line**, „Short form on screen only as Position 1, Position 2“; `Auftragsposition` = order line. Tức là EN cố ý giữ „Position 1/2“ như nhãn ngắn — nhưng #128 đã đổi tên bài thành „Several order lines“ và nhóm thấy „positions“ khó hiểu.
  - 6 key EN còn chữ „position“: `common.position.label` „Position {position}“ (dải Position), `mixed.card.step1_body` „Read for the current position: …“, `mixed.feedback.position_done` „Position {position} is done. …“, `complex.feedback.position_done` „Position {done} is done. …“, `complex.quiz.q_cartons` „… for this position?“, và `mixed.card.task_body` (câu „So …“: „… right for every position …“, xem R2).
  - DE „Position 1/2“ là chữ đúng của người kho (LVS cũng dùng), nên chỉ EN cần đổi.
- **Ghi chú:**
  - Hướng: EN đổi hết sang „Order line 1/2“, „the current order line“, „Order line 1 is done.“; sửa ghi chú glossary cho dòng `Position`. Kiểm bề rộng `Position Strip` (EN „Order line 1 · Order line 2“ dài hơn) và các thẻ kết quả (cỡ chữ cố định, xem R6/R9/R10).
  - Pilot EN (P8xx) là cơ sở vòng 3, nên chữ EN cũng quan trọng. Đi qua `localize-strings`.

## R16. (Câu hỏi) Có nên bật trọng lực cho kiện để giống thật? Rủi ro cho tương tác và log

- **Câu hỏi:** „bình thường các prototype, khi nào nên dùng trọng lực để cho giống thật. Ví dụ đặt hộp thì nó tự rơi xuống bàn, nhưng liệu làm vậy có tăng khả năng gây lỗi vì dùng physical rồi gravity thì ảnh hưởng đến các interaction khác không? đặc biệt cho việc thu thập log thì có hay bị log rác không“
- **Liên quan:** R13 (carton tự di chuyển, snap trượt), F2 phần (b) của `docs/review-notes-2026-11-17.md` (kiện thả ngoài thùng lơ lửng), #108 `CartonSettler`, #100 `ConceptBoxHints`, #29 log `container_*`.
- **Hiện trạng:**
  - Mọi kiện là rigidbody **kinematic, không trọng lực**: thả ở đâu nằm yên ở đó. Ngoại lệ có chủ ý: `CartonSettler` (Basic, Mixed) đặt carton thả trong thùng đơn (không snap) nằm phẳng xuống đáy / lên kiện bên dưới — tức „trọng lực giả“, tính một lần, có kết quả xác định. Thùng chạy (`LaneRide`, `PositionStockSwitcher`) giữ kiện kinematic khi chở.
  - Log `container_*` lấy từ trigger của counter; Concept chỉ tính kiện nằm yên 0,5 s trong vùng (`ConceptBoxHints`).
- **Trả lời tóm tắt (ghi lại để bàn khi phân tích):**
  - **Khi nào nên dùng trọng lực thật:** khi chính việc rơi/xếp/đổ là nội dung cần học (vd. đổ hàng EKZ 0, xếp chồng ổn định, kiện rơi vỡ), hoặc khi kiện lơ lửng phá cảm giác thật đến mức gây hiểu sai. Với bài này (đọc LVS, đếm, đặt đúng chỗ), trọng lực không phải nội dung học.
  - **Rủi ro khi bật trọng lực + va chạm:**
    - Kiện rơi khỏi bàn, xuống sàn, lọt xuyên mesh mỏng → người học phải cúi nhặt, cần cơ chế trả kiện về.
    - Kiện xếp chồng rung, trượt, nảy; tay/thân bằng collider có thể hất kiện khác; dễ đè lên vùng snap (đúng loại lỗi R13).
    - Khi thùng chạy trên băng, kiện vật lý dễ văng/rơi khỏi thùng → phải ép kinematic như hiện nay.
    - Khác nhau giữa Editor và Quest (tần số physics, khung hình) → khó tái hiện lỗi, test MCP kém tin cậy hơn.
  - **Log rác:** có, đây là rủi ro chính. Kiện nảy/lăn qua mép trigger sinh chuỗi `container_enter`/`container_exit` liên tiếp; kiện bị hất mà không ai cầm làm thay đổi Ist (và có thể đổi kết quả `confirm_pressed`) dù người học không làm gì. R phải lọc theo thời gian, và số `container_enters/exits` mất ý nghĩa „hành động của người học“. Nếu dùng thì cần: chỉ log khi kiện đã nằm yên (debounce như 0,5 s của Concept), log nguồn gây đổi (tay / vật lý), và đổi `layout_version` vì dữ liệu trước/sau không so được.
  - **Thực hành phổ biến ở prototype huấn luyện:** lai — khi thả: rơi thẳng xuống bề mặt gần nhất bên dưới (hoặc trọng lực thật trong chốc lát), rồi „ngủ“ / chuyển kinematic khi đã nằm yên; trong thùng/snap thì khoá; thêm vùng trả kiện về nếu rơi xuống sàn. Ở dự án này hướng ít rủi ro nhất là mở rộng `CartonSettler` (rơi thẳng có kiểm soát) cho mọi chỗ thả (bàn, ngoài thùng), giữ kinematic, không bật trọng lực thật.
- **Ghi chú:** quyết định thuộc `solution-decision` (unity-reviewer + xr-ux-reviewer + data-analyst cho phần log). Trước design freeze 19/11 nên giữ kinematic.
