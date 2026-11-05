# Góp ý sau buổi test kính P802 (2026-11-05)

Người dùng ghi góp ý từng điểm ở đây. Khi đủ, phân tích chung một lần (`review-to-backlog`, rồi `solution-decision` cho mục có đánh đổi). Ảnh lưu ở `TestEvidence/review-P802/` (không commit). Kết quả log và video: `docs/pilot-P802-summary.md`.

**Đã chuyển vào backlog (2026-11-05):** R18 → #103 · R1, R9, R11, R15 → #104 (phần ẩn thanh của R15 → #106) · R4, R10, R13 → #105 · R3 → #106 · R14 → #107 · R2, R8 → #108 · R5, R16 → #109 · R6 → #110 · R12, R17 → #111 · R7 → #112 · R19 → #113 · log cầm giả do ô snap (R) → #114. Hai điểm nhỏ: xem lại ở buổi test sau.

Mỗi mục: **góp ý** (lời người dùng), **hiện trạng** (đọc từ code/CSV, chưa sửa), **ghi chú phân tích**.

---

## R1. ConceptScene: thẻ chữ bên cạnh kiện đang cầm cứ nháy; bỏ luôn thẻ đó

- **Ảnh:** chưa có ảnh riêng (video `07_presentations/com.xrise.xpick-20261105-194250-0.mp4`, khoảng 1:20–2:40 và 3:40–4:20, lúc cầm carton STECKER C)
- **Góp ý:** khi cầm kiện ở Concept, chữ ở bên cạnh cứ nháy đi nháy lại. Tốt nhất là bỏ hẳn thông tin đó, vì nhãn trên kiện đã có cùng thông tin như trên LVS, để người học tự đọc và tự hiểu.
- **Hiện trạng:** thẻ là `Held Label Tags` (`Assets/Scripts/Concept/HeldLabelTags.cs`, #90, decision 026 mục 2). Khi cầm một kiện có `PackageData`, script nhân bản `Label Tag` (thẻ tối 260 × 64) và **mỗi khung hình** (`LateUpdate`) đặt nó cạnh kiện, quay về phía mắt, chữ `concept.label.tag.matnr` / `.vpe` („Mat.-Nr: 625417 – material number / VPE: 40 ST – pieces per carton or bag“). Thẻ đặt bên trái hay bên phải tuỳ kiện đang ở bên nào của đầu (`HeldPackageTracker.TagSides`, theo dấu của toạ độ x so với mắt). Từ #92 chỉ còn STECKER C nên thẻ luôn ghi đúng các số đã in trên nhãn.
- **Ghi chú phân tích:**
  - Nguyên nhân nháy (giả thuyết, chưa đo): khi tay cầm kiện ở gần giữa tầm nhìn, x đổi dấu liên tục vì tay rung, nên thẻ nhảy qua lại giữa hai bên. Ngoài ra thẻ bám theo kiện từng khung hình, nên rung theo tay.
  - Bỏ thẻ là hợp với memory „show, don't tell“: thẻ chỉ lặp lại chữ đã có trên nhãn (redundancy), và người học phải tự so nhãn với LVS (quiz `label_by_matnr` kiểm đúng kỹ năng này).
  - Bỏ thẻ kéo theo: `WmsFieldTour.heldPackages` (đang là `HeldLabelTags`) dùng để đổi dòng hành động sang `concept.tour.<id>.action_held` („so nhãn với LVS“) khi đang cầm STECKER C. Nếu bỏ hẳn script thì phải giữ phần theo dõi kiện đang cầm (tách `HeldPackageTracker` ra, hoặc chỉ tắt việc hiện thẻ), hoặc bỏ luôn `action_held`. `TagPlacement` vẫn được `ConceptBoxHints` dùng. Hai key `concept.label.tag.*` thành thừa (xoá ở CSV, `ImportCsv` tự bỏ khỏi bảng). Decision 026 mục 2 phải được ghi là bị thay.
  - Không đổi log: thẻ không ghi sự kiện nào. Cùng nhóm với điểm „mũi tên cam che nhãn“ (bản tổng hợp video, điểm 3): cả hai đều về việc người học đọc nhãn thật.

---

Các mục R2–R8 do Claude ghi từ video và log P802 (người dùng đồng ý thêm vào, 2026-11-05). Giờ video ≈ t trong log + 19 s. Chỉ xem khung hình mỗi 6 s (1 s ở các đoạn lỗi), không có tiếng, một người test.

## R2. Basic, Mixed: không thấy ô snap khi xếp theo Empf Kz 003

- **Ảnh:** `TestEvidence/review-P802/01-basic-carton-not-snapped.jpg` (Basic, video 8:06: một carton nằm nghiêng đè lên carton khác, Ist đã 300)
- **Quan sát:** ở cả Basic (t 465) và Mixed vị trí 1 (t 514), người học chồng hoặc đặt nghiêng carton lên carton khác, bị `EKZ_PLACEMENT`. Sau đó người học lấy một carton ra, bấm nút khi đang thiếu, bị thêm `TOO_FEW`, rồi mới xếp đúng. Cùng một chuỗi lỗi ở cả hai bài.
- **Hiện trạng:** ô snap (`BoxSnapper*`) không nhìn thấy được: renderer `Marker*` màu xám đã tắt (#41/#56). Ở Mixed, 3 ô nằm ở cột trái thùng (#94), nửa phải để trống cho túi. Thông báo `EKZ_PLACEMENT` („Nicht alle Kartons liegen geordnet. Bei Empf Kz 003: …“) không nói phải đặt ở đâu. Carton chưa vào ô vẫn được tính vào Ist, nên LVS báo Ist = Soll dù xếp sai.
- **Ghi chú phân tích:** cân nhắc cho ô thấy được (viền mờ trên đáy thùng, hoặc chỉ hiện khi đang cầm carton) và/hoặc thông báo chỉ ra carton nào chưa vào ô. Ở công việc thật không có ô vẽ sẵn, nên cần quyết định mức hỗ trợ (scaffold) và có giảm dần qua các bài không. Liên quan R8 (carton nằm nghiêng).

## R3. Concept: thanh tour che thùng đơn đúng lúc phải làm bằng tay

- **Ảnh:** `TestEvidence/review-P802/02-tourbar-over-box.jpg` (bước 5 `cartons`)
- **Quan sát:** ở bước 2 („Pick up the STECKER C carton“) và bước 5 („Put the cartons … into the order container“), nền tối nửa trong suốt của thanh tour nằm ngay trước thùng đơn và phần đầu làn giữa. Bước 5 là bước lâu nhất của tour (63 s, `tour_steps.csv`).
- **Hiện trạng:** `Tour Nav Bar` ở (0.06, 1.08, 0.48), `Step Panel` mép trên y 1.214 (#89). Thùng đơn ở (0.45, 0.77, 0.71). Thanh tour đã được hạ thấp để không che màn LVS ở tầm mắt 1,45–1,75 m, nên giờ nằm trên đường nhìn xuống thùng.
- **Ghi chú phân tích:** đo trong Editor (cảnh `container` và `overview`). Hướng xử lý có thể: thu nhỏ hoặc làm trong suốt nền ở các bước cần thao tác, hoặc dời thanh sang trái hoặc lên cao hơn. Phải giữ ràng buộc không che LVS và tầm với tay (#84). Cùng chỗ với cụm quiz (#93).

## R4. Concept: mũi tên cam che nhãn carton

- **Ảnh:** `TestEvidence/review-P802/03-marker-on-label.jpg` (bước 2 `material`, video 2:12)
- **Quan sát:** mũi tên cam chỉ xuống nằm đè lên tờ nhãn của carton cần cầm, che một phần „STECKER C / Mat.-Nr …“, trong khi bước này yêu cầu đọc nhãn đó.
- **Hiện trạng:** `STECKER C Marker` (mũi tên cam phía trên `STECKER C Karton 1`, nhấp nhô 1,5 cm, `MarkerBob`, #83/#90/#92) là marker của bước `material` và `verp_einh`.
- **Ghi chú phân tích:** đặt mũi tên cao hơn hoặc lệch sang một bên nắp, hoặc tắt khi carton đã được cầm (`HeldPackageTracker`). Cùng nhóm với R1: người học phải tự đọc được nhãn thật, không bị che.

## R5. Basic, Mixed, Complex: thẻ kết quả nằm ở mép dưới bên phải tầm nhìn

- **Ảnh:** `TestEvidence/review-P802/04-result-card-edge.jpg` (Complex, video 11:16: „Not correct … RING D …“ chỉ lộ một góc)
- **Quan sát:** khi người học nhìn vào thùng đơn, thẻ kết quả chỉ lộ một góc ở mép dưới bên phải. Log cho thấy nhiều lần bấm nút đỏ cách nhau vài giây (Basic t 465 → 469 → 475, Mixed 514 → 519), nên có thể người học sửa theo kiểu thử mà không đọc lý do lỗi.
- **Hiện trạng:** thẻ kết quả ở (0.50, 0.84, 0.38), nghiêng 40°, cạnh nút đỏ (#59). Menu ở (0.52, 1.15, 0.27), ngay phía trên.
- **Ghi chú phân tích:** split attention: chỗ người học nhìn (thùng) khác chỗ có phản hồi. Có thể đưa phản hồi lại gần thùng (trên thành sau thùng, như `Box Sign`), hoặc đánh dấu ngay tại carton bị lỗi (như hint tag ở Concept, #100). Không được che LVS. Cần `xr-ux-reviewer` xem.

## R6. Concept, Complex: quiz chấm ngay khi chạm nên khuyến khích đoán

- **Ảnh:** `TestEvidence/review-P802/05-concept-quiz-1fps.jpg` (16 khung hình, 1 s mỗi khung, video 6:40–6:55)
- **Quan sát:** Concept câu 1 sai, sai, đúng trong 6 s (A „2“ → C „25“ → B „4“); câu 2 và 3 sai rồi đúng cách nhau ~1,2 s. Complex câu „cái lẻ“ B → C → A trong ~2 s. Không dừng lại đọc dòng giải thích màu đỏ. Chỉ 1/4 câu Concept đúng lần đầu.
- **Hiện trạng:** câu được chấm ngay khi chạm (#73, decision 020; Complex #85). Sau câu sai, các nút vẫn bấm được ngay. R đã chỉ tính lần chạm đầu (`items.csv`), nên dữ liệu nghiên cứu không sai, chỉ là việc học kém đi.
- **Ghi chú phân tích:** có thể khoá các nút 1,5–2 s sau câu sai (giống `CorrectHoldSeconds` sau câu đúng) để người học đọc lời giải thích, hoặc chỉ cho chọn lại sau khi đã nhìn lời giải thích. Đổi cách chấm thì đổi `layout_version`, và R phải tách dữ liệu Concept/Complex quiz theo layout.

## R7. Mọi bài: đĩa đen của facilitator nằm trong tầm nhìn, không có nhãn

- **Ảnh:** `TestEvidence/review-P802/06-facilitator-disc.jpg` (Concept, đĩa đen bên trái)
- **Quan sát:** người học thấy đĩa đen lơ lửng bên trái ở mọi bài. Cuối buổi, sau bảng Congratulations, người học chạm vào đĩa (video 12:36–12:42); không có gì xảy ra vì phiên đã kết thúc.
- **Hiện trạng:** `FacilitatorEndSession` ở (−0.80, 1.40, 0.10), phải giữ 3 s mới mở hộp hỏi kết thúc (#44); chạm nhanh thì không có gì.
- **Ghi chú phân tích:** đó là một vật trông có thể bấm được nhưng không có ý nghĩa với người học (gây nhiễu). Có thể dời ra sau lưng hoặc thấp hơn tầm nhìn (người hướng dẫn biết chỗ), làm mờ hơn, hoặc ẩn khi chưa có ai nhìn vào. Giữ cử chỉ giữ 3 s.

## R8. Basic, Complex: carton nằm nghiêng hoặc lơ lửng trên carton khác

- **Ảnh:** `TestEvidence/review-P802/01-basic-carton-not-snapped.jpg`, `04-result-card-edge.jpg`
- **Quan sát:** carton thả ra nằm nghiêng trên carton khác, không rơi xuống chỗ trống. Có lúc che chữ „Order container“. Trông như đã xếp xong dù chưa vào ô (liên quan R2).
- **Hiện trạng:** ở Concept, kiện là kinematic, không có trọng lực, thả ở đâu nằm yên ở đó (CLAUDE.md, #100). Ở Basic và Complex chưa kiểm cài đặt Rigidbody; trên video, carton thả ra không rơi xuống chỗ trống. Ở Basic, carton chỉ thẳng hàng khi được ô snap hút vào.
- **Ghi chú phân tích:** cân nhắc cho kiện rơi nhẹ hoặc tự xoay thẳng khi thả vào trong thùng. Phải đo tác động lên bộ đếm trigger và việc chở kiện theo băng chuyền (`LaneRide`, `OrderBoxConveyor` tắt collider và interactable của kiện đang được chở). Liên quan R16 của P801 (vòng và carton đổ lung tung, #96).

## R9. Concept: dòng hành động „Press "Next".“ không nói làm gì

- **Ảnh:** `TestEvidence/review-P802/09-press-next-line.webp` (thanh tour bước 1 `intro`: „**The WMS:** Here you see an order in the warehouse management system (WMS).“ / tay + „Press "Next".“ / [Back] Step 1 of 11 [Next])
- **Góp ý:** dòng „Press "Next"“ không giải thích làm để làm gì, nên không khác gì bản thân nút Next, vốn ai cũng hiểu là để bấm.
- **Hiện trạng:** dòng hành động là `concept.tour.<id>.action` (#89, decision 026: mỗi bước một câu „làm gì“, cạnh biểu tượng bàn tay). Bốn bước chỉ có „Press "Next".“ / „Drücken Sie „Weiter“.“: `intro`, `ist`, `rest`, `not_used`. Bước `soll` có „Read the number in the Soll field. Then press "Next".“; các bước còn lại có việc cụ thể (cầm carton, so nhãn, xếp carton, xem ví dụ EKZ, xem băng chuyền).
- **Ghi chú phân tích:** dòng hành động chỉ có giá trị khi nó nói một việc người học phải nhìn hoặc làm. Ở 4 bước kia có thể: (1) thay bằng việc cụ thể, ví dụ `intro` „Find Soll, Ist and Rest on the screen.“, `ist` „Look at Ist: it is the number of pieces in the order container.“, `rest` „Look at Rest: it is the stock left in the storage container.“, `not_used` „Look at the marked areas: you do not need them.“; hoặc (2) ẩn dòng hành động (cả biểu tượng tay) khi bước không có việc gì ngoài đọc. Cách (2) gọn hơn nhưng làm thanh tour không đồng đều giữa các bước. Mọi chữ mới cần EN + DE qua `localize-strings` và `german-reviewer`. Không đổi log. Liên quan R3 (thanh tour): ẩn dòng thừa cũng làm thanh thấp hơn, đỡ che thùng.

## R10. Concept: chú thích bên trái màn hình không thẳng hàng với ô trên LVS

- **Ảnh:** `TestEvidence/review-P802/10-caption-arrows-offset.png` (bước 2 `material`: thẻ xanh „Mat.-Nr and Bez. / Material number and description.“, hai mũi tên cam chỉ sang ô Mat.-Nr và Bez.); cũng thấy ở video 2:12–3:24 (bước `verp_einh`, `soll`, `cartons`)
- **Góp ý:** vị trí của ô trên LVS bị lệch so với phần giải thích ở ngoài, làm rất khó hiểu.
- **Hiện trạng (từ ảnh, chưa đo trong Editor):** chú thích là `Tour Markers/Caption <id>` (#78, decision 024): một thẻ 300 × 330 đơn vị (16,5 cm rộng) bên trái khung màn hình, đặt „ngang hàng ô của bước“; mũi tên hàng (`Tour Markers`, chỉ còn mũi tên ngang từ #89) là object riêng. Trên ảnh:
  - Tiêu đề thẻ („Mat.-Nr and Bez.“) nằm cao hơn ô Mat.-Nr; phần chữ nằm ở nửa trên, nửa dưới thẻ trống, nên thẻ không „thuộc về“ hàng nào.
  - Mũi tên không chạm thẻ cũng không chạm ô: giữa mũi tên và nhãn „Mat.-Nr:“ có khoảng trống và cạnh khung màn hình, mũi tên dưới nằm hơi thấp hơn chữ „Bez.:“.
  - Thẻ và mũi tên nằm ngoài mặt màn hình (khác độ sâu), nên mức lệch đổi theo chiều cao mắt và chỗ đứng (thị sai). Ở bước `cartons` (#91) chú thích được xếp „Soll [120]“ ngang hàng Soll, „Verp.-Einh. [40]“ ngang hàng Verp.-Einh., ở đó dễ đọc hơn (video 3:18).
- **Ghi chú phân tích:** quy tắc màn hình (#45/#78) là không vẽ gì lên dialog LVS, nên chú thích phải ở ngoài khung. Hướng xử lý có thể: (1) thẻ thấp, vừa chữ (không cao cố định 330), mép thẻ ngang hàng ô, mũi tên nối liền từ mép thẻ tới sát cạnh khung ngay tại hàng đó; (2) đặt thẻ và mũi tên cùng mặt phẳng với màn hình (cùng z, cùng góc nghiêng) để không lệch khi đổi tầm mắt; (3) với bước một ô, dùng kiểu của bước `cartons`: nhãn ô lặp lại ngay đầu thẻ, ngang hàng ô. Đo trong Editor với cảnh `wms` ở 3 tầm mắt (1,45 / 1,60 / 1,75 m). Liên quan R3 và R9 (thanh tour), cùng nhóm „tour Concept“.

## R11. Concept: „Pick up the STECKER C carton.“ không nói cầm để làm gì, không nối với chú thích trên LVS

- **Ảnh:** `TestEvidence/review-P802/11-pickup-no-purpose.webp` (bước 2 `material`: chú thích „Mat.-Nr and Bez.“ bên trái màn hình, dòng hành động „Pick up the STECKER C carton.“ trên thanh tour)
- **Góp ý:** câu „Pick up the STECKER C carton“ chưa có liên kết với phần chú ý trên LVS. Ví dụ: cầm carton STECKER C lên để làm gì?
- **Hiện trạng:** bước `material` và `verp_einh` có hai câu hành động (#89/#90, decision 026 mục 2): trước khi cầm là `concept.tour.<id>.action` „Pick up the STECKER C carton.“ / „Nehmen Sie den Karton STECKER C in die Hand.“ (giống hệt nhau ở cả hai bước); chỉ khi đang cầm STECKER C (`WmsFieldTour.steps[].heldMaterialKey`) mới đổi sang `.action_held`: `material` „Compare Mat.-Nr and Bez. in the WMS with the label.“, `verp_einh` „Read the VPE on the label and compare it with Verp.-Einh. in the WMS.“. Tức là mục đích chỉ hiện sau khi đã cầm; thả carton ra thì câu lại về „Pick up …“. Trên video, ở bước `verp_einh` người học đọc được câu so sánh (video 2:36–3:00), nhưng lúc mới vào bước thì chỉ thấy câu cầm.
- **Ghi chú phân tích:** một câu nên nói cả việc và mục đích, nối với ô đang được chú thích, ví dụ `material` „Pick up the STECKER C carton and compare its label with Mat.-Nr and Bez.“, `verp_einh` „Pick up the carton and compare the VPE on its label with Verp.-Einh.“. Như vậy có thể bỏ cơ chế đổi câu theo kiện đang cầm (`action_held`, `heldMaterialKey`), và cùng với R1 (bỏ thẻ chữ) thì `HeldLabelTags` không còn việc gì ở thanh tour. Hai bước không còn câu giống hệt nhau. Chữ mới cần EN + DE (`localize-strings`, `german-reviewer`), câu dài hơn thì phải đo lại chiều rộng thanh (`Step Panel`). Cùng nhóm với R9 (dòng hành động nói rõ việc), R10 (chú thích không thẳng hàng) và R1.

## R12. Biển „Storage container“ / „Order container“ lơ lửng ngoài thùng; kiểm mọi scene

- **Ảnh:** `TestEvidence/review-P802/12-box-sign-floating.png` (Concept: biển „Storage container“ nằm bên trái, phía trên băng con lăn, tách khỏi thùng lưu)
- **Góp ý:** ở Concept, chữ „Storage container“ đang lơ lửng ở ngoài. Cần kiểm ở tất cả các scene, cả biển „Order container“.
- **Hiện trạng (đọc YAML scene, chưa đo trong Editor; toạ độ là toạ độ local trong thùng, thùng có scale riêng nên chưa đổi ra mét):**

  | Scene | Biển | local z | anchored x / y | nghiêng |
  |---|---|---|---|---|
  | Concept | biển 1 (có lẽ thùng lưu) | 0.426 | **−0.8** / 0.364 | 15° |
  | Concept | biển 2 (có lẽ thùng đơn) | 0.426 | 0 / 0.464 | 15° |
  | Basic (2 biển), Mixed (3), Complex (1) | | 0.376 | 0 / 0.332 | 0° |
  | Complex (2 biển, gồm `PlasticBox_Storage_2`) | | 0.376 | −0.025 / **0.550** | 0° |

  - Concept dùng bản gốc của #92 („ở mặt sau thùng“, z 0.426, nghiêng 15°); Basic/Mixed/Complex dùng bản sao của #99 („mặt trong thành sau“, z 0.376, mép trên y 1.0 để lọt dưới bàn sau khi thùng chạy). Hai bản không giống nhau.
  - Biển 1 ở Concept lệch ngang −0.8 so với tâm thùng; với thùng rộng 0,40 m thì đây có thể chính là biển nằm ra ngoài trên ảnh (cần đo).
  - Hai biển ở Complex cao hơn hẳn các biển khác (y 0.55 so với 0.33).
  - Biển là canvas world-space không có nền gắn vào thành thùng, nên nhìn từ trên xuống dễ thấy như treo lơ lửng, kể cả khi đúng vị trí (ảnh `02`, `04`: „Order container“ trông dán vào thành sau, nhưng bị carton nghiêng che).
- **Ghi chú phân tích:** đo trong Editor ở 4 scene, cả thùng lưu, thùng đơn và thùng lưu vị trí 2 (Mixed `PlasticBox_Storage_2`, Complex `PlasticBox_Storage_2`), với cảnh `container` và một góc nhìn từ chỗ đứng. Thống nhất một mẫu: cùng vị trí local (giữa, sát mặt trong thành sau, mép trên thấp hơn mép thùng một chút hoặc ngang mép), cùng góc nghiêng, cùng cỡ chữ; có thể thêm nền dán sát thành để biển trông „in trên thùng“. Phải giữ: không chạm mặt dưới bàn sau (y 1.01) khi thùng chạy (#99), không che hint tag ở thành trước (Concept #100), không che `Opened Carton Note` (Complex). Không đổi log.

## R13. Concept: mũi tên của ô ở giữa màn hình (Ist) bị hiểu là chỉ vào Soll; cho phép vẽ mũi tên lên LVS

- **Ảnh:** `TestEvidence/review-P802/13-ist-arrow-points-soll.webp` (bước 6 `ist`: chú thích „Ist / When weighing, Ist shows how many pieces are already in the order container.“ bên trái, mũi tên cam ở cạnh trái khung, ngay trước ô Soll)
- **Góp ý:** mũi tên chỉ như vậy thì bị hiểu là mô tả cho Soll. **Người dùng cho phép vẽ mũi tên lên LVS**, chỉ thẳng vào các ô nằm sâu bên trong, không chỉ ở ngay viền màn hình.
- **Hiện trạng:** quy tắc màn hình từ #45/#78 (decision 024): không vẽ gì lên dialog LVS, mọi marker ở ngoài khung (`WMS_Monitor/Head/Screen/Tour Markers`). Từ #89 chỉ còn mũi tên ngang theo hàng (mũi tên cột `Top`/`TopTip` tắt), nên ô nào cũng được chỉ từ cạnh trái khung (Rest từ cạnh phải). Ô nằm đầu hàng (Soll, Empf Kz, Mat.-Nr, Bez.) thì rõ; ô ở giữa hàng (Ist, Verp.-Einh.) thì mũi tên chạm ô đầu hàng trước, nên bị hiểu nhầm. Ist và Verp.-Einh. là hai ô quan trọng của bài (Ist = Soll khi xong, Soll : Verp.-Einh. = số carton).
- **Ghi chú phân tích:**
  - Đây là **thay đổi quy tắc đã quyết**: decision 010 (dòng „Khung trên dialog … Người dùng chọn: không cho phép“; mục rủi ro dòng 72 đã tính trước: „Người dùng sau này cho phép đánh dấu trên dialog: thêm khung vào child R3/R4 (Concept trước)“) và decision 024 mục 2 („Ist, Verp.-Einh. dựa vào tiêu đề + mũi tên hàng … Không vẽ gì lên dialog“; chính quyết định này gây ra lỗi trên ảnh). Decision 008 mục 9 đã chia dialog thành child theo vùng để gắn đánh dấu. Khi làm phải ghi một decision mới thay phần đó (`solution-decision`), nói rõ phạm vi: chỉ trong tour Concept, chỉ mũi tên/khung, không đổi chữ hay số của dialog, không vẽ ở Basic/Mixed/Complex.
  - Cách vẽ: overlay riêng nằm trước mặt `Screen` một chút (tránh z-fighting), không nằm trong prefab `WMS_Kommissionierung`, không raycast; mũi tên ngắn chỉ vào cạnh trái ô (hoặc khung viền quanh ô), cùng màu cam với marker hiện có; mỗi bước chỉ bật nhóm của bước đó như bây giờ.
  - Khi mũi tên nằm trên màn hình thì chú thích bên ngoài có thể nối bằng đường dẫn từ thẻ tới mũi tên, hoặc chú thích vẫn ở ngoài và mũi tên trên màn hình chỉ đánh dấu ô. Gộp với R10 (chú thích không thẳng hàng): cả hai cùng là cách tour chỉ vào một ô.
  - Kiểm lại mọi bước: `material` (Mat.-Nr + Bez.), `verp_einh`, `soll`, `cartons` (Soll + Verp.-Einh.), `ist`, `rest`, `empf_kz_003/000`, `not_used` (khung các vùng không dùng: đã là khung ngoài; nên chuyển thành khung trên màn hình luôn cho đồng bộ).

## R14. Concept: hai mẫu Empf Kz 000 và 003 nhìn không thấy khác nhau

- **Ảnh:** `TestEvidence/review-P802/14-ekz-samples-alike.png` (bước 9 `empf_kz_000`: bên trái thùng `EKZ Sample 000` với 2 túi SCHRAUBE B màu cam, bên phải thùng `EKZ Sample 003` với 3 carton KABELVERBINDER A nắp lên trên một hàng; mỗi thùng có thẻ „Empf Kz 000 = EKZ 0“ / „Empf Kz 003 = EKZ 3 / Example – not for picking up.“)
- **Góp ý:** nhìn hai loại EKZ này vẫn không phân biệt được khác biệt.
- **Hiện trạng:** `Layout Slots/EKZ Sample 003` (thùng 40 × 21 cm, 3 carton tĩnh nằm thẳng một hàng) và `EKZ Sample 000` (thùng 21 × 32 cm, 2 túi tĩnh hơi nghiêng), trên bàn sau bên trái màn hình, chỉ hiện ở bước 8–9 (#83/#94/#101/#92, decision 028/031/032). Ý định: 003 = „từng chiếc, có thứ tự“, 000 = „đổ vào, không thứ tự“ (`concept.ekz.*`).
- **Ghi chú phân tích (vì sao không thấy khác):**
  - Hai mẫu khác nhau ở **ba thứ cùng lúc**: loại hàng (carton ↔ túi), vật tư (KABELVERBINDER A ↔ SCHRAUBE B) và cách xếp. Người học dễ hiểu EKZ là „carton hay túi“ chứ không phải „xếp hay đổ“.
  - 2 túi nằm cạnh nhau, hơi nghiêng, vẫn trông **gọn**; không có gì giống „đổ lộn xộn“. 3 carton một hàng cũng không cho thấy „thứ tự“ khác gì cách đặt bình thường.
  - Hai thẻ giống nhau về khuôn; khác biệt chỉ nằm ở hình nhỏ (pictogram) và một chữ số.
- **Hướng có thể (để quyết chung):**
  1. Cùng một loại hàng ở cả hai mẫu, chỉ khác cách xếp: 003 = carton xếp thẳng hàng trong ô; 000 = nhiều chiếc (5–8) cùng loại nằm lộn xộn, nghiêng, chồng lên nhau, hoặc chi tiết rời đổ đống (vít, phụ kiện ống: P801 R19).
  2. Cho mẫu „động“: ở bước 9 chạy một hoạt cảnh ngắn: carton được đặt từng chiếc vào ô (003) ↔ chi tiết được đổ vào (000), giống cách `LaneFlowDemo` chạy ở bước 11 (show, don't tell).
  3. Thẻ ghi luôn hành động: „003: từng chiếc, có thứ tự“ / „000: đổ vào được“ (hiện chỉ có trên bảng `EKZ Overview`, ở xa bên phải).
  - Gộp với kế hoạch EKZ chung của P801 (R10, R12, R16, R19 → #94, #96) và R2, R8 của P802 (xếp carton theo ô). Quiz `ekz_003` / `ekz_000` kiểm đúng khác biệt này, nên mẫu phải làm rõ trước khi hỏi.

## R15. Concept bước 11: chữ dài, chưa đọc kịp thì thanh tour đã biến mất

- **Ảnh:** `TestEvidence/review-P802/15-material-flow-text-long.webp` (bước 11 `material_flow`: tiêu đề + 3 câu thân + 2 câu hành động trên thanh tour)
- **Góp ý:** nội dung dài, đọc không kịp thì đã biến mất. Đề xuất rút gọn chữ để nắm được ý nhanh.
- **Hiện trạng:**
  - Chữ trên thanh: `concept.tour.material_flow.title/body` „**The way of the containers:** The storage container comes from the left and leaves over the middle lane; the order container leaves only when the order is done. In practice you move the containers by hand.“ (DE dài hơn: „… Der Lagerbehälter kommt von links und fährt über die mittlere Bahn weg; der Auftragsbehälter fährt erst weg, wenn der Auftrag erledigt ist. In der Praxis bewegen Sie die Behälter von Hand.“) + `.action` „Watch the containers on the roller conveyor. Then press "To the questions".“ Tổng ~45 từ EN.
  - `LaneFlowDemo.startDelay` = 4 s (#89: „thời gian để đọc thanh“), rồi `hideWhileRiding` ẩn `Tour Nav Bar` + `AnswerOption` đến khi thùng đơn đã đi, vì thanh che đầu làn giữa. 4 s chỉ đủ đọc khoảng 10–12 từ.
  - **Log P802 khớp với góp ý:** bước 11 hiện ở t 337.6; ở t 360.7 người học bấm „Back“ về bước 10 rồi „Weiter“ lại ngay (t 362.1), tức là quay lại để xem lại chữ (`tour_steps.csv`: `material_flow` 2 lần hiện, 1 lần restart). Mỗi lần như vậy toàn bộ trạm bị đặt lại và chạy lại demo.
- **Ghi chú phân tích:**
  - Rút gọn: một ý mỗi câu, bỏ phần người học sẽ tự thấy trên băng chuyền (hướng đi của thùng). Ví dụ EN: tiêu đề „The way of the containers“ + body „Watch: storage container leaves, order container leaves when done.“ hoặc chỉ giữ câu mà demo không cho thấy được: „At the real station you move the containers by hand.“ Câu hành động có thể còn „Watch the roller conveyor.“, còn „To the questions“ hiện lại sau khi demo xong.
  - Có thể kết hợp: chữ hiện lại sau khi thùng đơn đi (thanh đã quay lại lúc đó) để đọc kỹ, hoặc đặt `startDelay` theo độ dài chữ. Gỡ việc ẩn thanh thì phải giải R3 (thanh che làn giữa) trước.
  - Chữ mới cần EN + DE (`localize-strings`, `german-reviewer`); câu DE thường dài hơn EN ~20 %. Cùng nhóm với R9 và R11 (chữ trên thanh tour), R3 (vị trí thanh). Đổi `startDelay` thì R phải biết (`lane_demo_delay_s` trong `analyse_set.R` đang giả định 4 s).

## R16. Basic: mũi tên cam trên làn giữa sau khi xác nhận đúng làm người học bối rối

- **Ảnh:** `TestEvidence/review-P802/16-basic-middle-lane-arrow.webp` (Basic sau lần xác nhận đúng, video 8:12–8:18: mũi tên cam chỉ xuống, nhìn từ chỗ đứng thì nằm đè lên chữ „Order container“ của thùng đơn)
- **Góp ý:** mũi tên (vàng/cam) này làm người học hơi bối rối: chuyện gì đang xảy ra, và họ có nên làm gì không?
- **Hiện trạng:** `Station Ride Cue/Middle Lane Arrow` (`StationRideCue`, #99, decision 033): bản sao mũi tên cam của Concept (`MarkerBob`, nhấp nhô) đặt phía trên làn giữa, hiện từ `OrderCompleted` đến 2 s sau khi thùng đơn thật sự chạy (`OrderBoxConveyor.Departed`); chỉ có ở Basic. Thẻ kết quả cùng lúc ghi „… The order container moves onto the middle lane and leaves. Watch the containers on the roller conveyor.“, nhưng thẻ đó nằm ở mép phải dưới (R5). Thùng chỉ chạy sau 4 s chờ (decision 033), nên trong 4 s đầu chỉ thấy mũi tên đứng yên mà chưa có gì chuyển động.
- **Ghi chú phân tích (vì sao bối rối):**
  - **Cùng một ký hiệu, hai nghĩa:** ở Concept, mũi tên cam này nghĩa là „cầm carton này lên“ / „nhìn vào đây“ (marker của bước tour). Ở Basic, nó lại nghĩa là „thùng sẽ đi đường này“, không yêu cầu làm gì. Người học đã học nghĩa thứ nhất ở bài trước.
  - **Nhìn từ chỗ đứng, mũi tên chỉ vào thùng đơn**, không chỉ vào làn giữa: làn giữa nằm ngay sau thùng, và mũi tên đè lên biển „Order container“ (thêm vào R12). Trông như „hãy làm gì đó với thùng đơn“.
  - Mũi tên chỉ xuống, không chỉ hướng đi; không có chữ đi kèm ở gần.
- **Hướng có thể:** (1) bỏ mũi tên, để thùng tự chạy là đủ, chữ ở thẻ kết quả nói „hãy nhìn“; (2) đổi thành ký hiệu chỉ hướng đi (mũi tên nằm trên mặt làn giữa, chỉ dọc theo làn ra phía sau, khác màu hoặc khác hình với marker „cầm lên“ của Concept), hiện khi thùng bắt đầu chạy chứ không trong 4 s chờ; (3) đặt cao hơn hoặc lệch khỏi thùng đơn để không đè biển. Quy tắc chung nên có: một ký hiệu = một nghĩa trong cả khoá học (cam chỉ xuống = „nhìn/cầm cái này“). Không đổi log. Liên quan R5 (thẻ kết quả xa), R12 (biển thùng), decision 033.

## R17. Complex: chú thích carton đã mở rối, lơ lửng

- **Ảnh:** `TestEvidence/review-P802/17-complex-opened-carton-note.webp` (Complex vị trí 1, nhìn xuống thùng lưu: thẻ đen „The opened carton stays in the module in the storage container. Take out only the loose pieces.“ + biểu tượng bàn tay gạch chéo, phía sau là chữ lớn „Storage c…“ của biển thùng lưu; trên nắp carton mở có nhãn đen xoay nghiêng, trên thành trước carton có thêm một biểu tượng bàn tay gạch chéo)
- **Góp ý:** chú thích rất rối, hiển thị lơ lửng.
- **Hiện trạng:** #92 (decision 032): mỗi thùng lưu ở Complex có hai thứ cho carton đã mở: `Opened Carton Mark` (biểu tượng `No Grab Symbol` trên thành trước carton, trên mép thùng) và `Opened Carton Note` (biểu tượng + chữ `complex.storage.opened_carton`) đặt phía trên `Storage Label` (bảng tên thùng). Thêm vào đó là `Box Sign` „Storage container“ ở thành sau (#99, R12) và nhãn RING D trên nắp carton.
- **Ghi chú phân tích (vì sao rối):**
  - **Bốn lớp chữ/ký hiệu chồng nhau trong một góc nhìn:** thẻ chú thích, biển „Storage container“ (to, bị thẻ che một phần), nhãn đen trên nắp xoay nghiêng, và biểu tượng thứ hai trên thành carton. Biểu tượng bàn tay gạch chéo xuất hiện hai lần.
  - **Thẻ treo trên không**, không dính vào carton hay thùng nào; từ chỗ đứng nó nằm đè lên biển thùng phía sau, nên không rõ nó nói về vật nào.
  - **Chữ dùng từ khó:** „module“ (Modul) chưa được dạy ở bài nào; hai câu dài cho một ý („không lấy carton này, chỉ lấy chiếc lẻ“).
- **Hướng có thể:** (1) chỉ giữ một ký hiệu: biểu tượng trên thành carton, và chữ ngắn ngay dưới nó, gắn sát carton (ví dụ „Only take the loose pieces.“ / „Nur lose Teile entnehmen.“); (2) bỏ thẻ nổi trên `Storage Label`; câu về Anbruchmenge ở lại Modul (A13, #92) có thể chuyển sang thẻ bước hoặc bỏ nếu không cần để làm bài; (3) kiểm nhãn trên nắp carton mở cho thẳng, đọc được từ chỗ đứng. Phải giữ quy tắc decision 032: vật chỉ xem có ký hiệu + một câu lý do, nói một lần. Liên quan R12 (biển thùng lơ lửng), R4 (ký hiệu đè lên nhãn). Đổi chữ cần EN + DE (`localize-strings`).

## R18. Cầm ở khá xa mà carton vẫn bị nhấc lên, không cần chạm sát hộp

- **Ảnh:** `TestEvidence/review-P802/18-grab-from-distance.webp` (Complex, carton RING D đang được nhấc; các ngón tay ở ngoài góc carton, không chạm mặt hộp)
- **Góp ý:** tại sao cầm ở khoảng cách khá xa mà hộp vẫn bị nhấc, chứ không cần cầm sát hộp?
- **Hiện trạng (chưa đo trong Editor):** kiện dùng `Grabbable` + interactable cầm bằng tay của Meta ISDK; vùng nhận cầm là collider của kiện (dưới Rigidbody) cộng bán kính phát hiện của tay. #88 (decision 029) đổi mọi carton sang cỡ chuẩn 17,8 × 9,2 × 12,3 cm bằng `PackageStandardizer`; tool đó chỉ chỉnh lại **BoxCollider đặc trên object mesh** (`SwapToCartonMesh`). Collider của sticker trước `Cube` thì **được giữ nguyên** (renderer và chữ tắt, CLAUDE.md: „never delete it“, vì counter dùng nó) và không được chỉnh theo cỡ mới.
- **Giả thuyết cần kiểm (theo thứ tự khả năng):**
  1. Collider vô hình của `Cube` (hoặc collider cũ khác) nhô ra ngoài hộp sau khi đổi cỡ, nên tay chạm vào khoảng trống đó là đã cầm được hộp.
  2. Vùng/bán kính phát hiện cầm của interactable (palm/pinch grab) đặt rộng, cộng với việc không có hand-grab pose: tay giữ nguyên vị trí, hộp đi theo với khoảng lệch, nên trông như cầm từ xa.
  3. Carton ở Complex (`SM_box_1` từ #88) có collider khác với carton Basic.
- **Ghi chú phân tích:** đo trong Editor: bật Gizmos cho collider của 1 carton ở mỗi scene (Concept STECKER C, Basic, Mixed, Complex RING D + SCHLAUCHSCHELLE B) và so với mesh; xem interactable cầm bằng tay đang dùng loại nào và vùng phát hiện bao nhiêu. Sửa collider thì phải chạy lại test đếm (counter dùng collider sticker ở Concept/Basic/Mixed) và B3/M3/X-case. Cầm từ xa làm người học không tin cảm giác tay, và làm khó đặt carton chính xác vào ô (liên quan R2, R8). Nếu đúng là giả thuyết 1 thì `PackageStandardizer` cũng phải chỉnh collider sticker khi đổi cỡ.

## R19. Complex: bảng Congratulations hiện ngay trước màn LVS, chữ xung quanh làm mất tập trung

- **Ảnh:** `TestEvidence/review-P802/19-congratulations-over-wms.webp` (cuối Complex: bảng tối „What you learned … Please take off the headset and tell the course leader. / Code: P802“ đè lên giữa màn LVS; nhãn và ô LVS (Soll, Empf Kz, Mat.-Nr, F-phím, Gew. 1,56) lộ ra ở cả bốn phía)
- **Góp ý:** màn hình Congratulations hiển thị trước màn LVS gây rất rối, vì khoảng xung quanh có chữ làm mất tập trung.
- **Hiện trạng:** `Congratulations` ở (0, 1.16, 0.95), ngay trước `WMS_Monitor` (#23); nhỏ hơn màn hình nên LVS lộ ra quanh bảng. Từ #99 bảng hiện 2 s sau khi thùng đơn chạy đi (`PiecesOrderValidator.congratulationsDelay`, trước đó `CanvasGroup` alpha 0). Nội dung: tóm tắt mục tiêu (`complex.end.summary`, lặp lại hai cụm „You now know / You can now“ của Welcome, #86) + câu đạp chân + câu tháo kính + mã người tham gia (`ParticipantCodeLabel`, phải đọc mã trước `EndSession`).
- **Ghi chú phân tích:**
  - Nền phía sau bảng là một màn hình đầy chữ nhỏ và số (không còn liên quan, bài đã xong), nên mắt bị kéo ra các mép; đây là extraneous load ngay ở khoảnh khắc kết thúc.
  - Hướng có thể: (1) tắt hoặc làm mờ màn LVS khi bảng hiện (dialog về trống, như màn hình đã đăng xuất); (2) đặt bảng ở chỗ trống (ví dụ phía trên hoặc bên phải, nền là kho) hoặc làm bảng to hơn và có viền/nền rộng che hết màn hình; (3) rút gọn nội dung bảng: chỉ „Geschafft!“, một dòng tháo kính và mã; phần tóm tắt mục tiêu đã có ở Welcome và có thể để người hướng dẫn nói.
  - Phải giữ: bảng không che làn giữa trong lúc thùng chạy (#99), mã vẫn được đọc trước `EndSession`. Chữ mới cần EN + DE. Không đổi log (layout không đổi sự kiện).

## Điểm nhỏ (chưa thành mục)

- Basic: nút đỏ bị bấm khi thùng còn trống, 13 s sau khi vào bài (t 435.7), lúc người học đang nhìn về phía Menu. Có thể là chạm nhầm vì nút nằm sát thùng và Menu. Video chưa đủ rõ để khẳng định.
- Concept: bấm „Back“ từ bước 11 về bước 10 làm Ist về 0 và thùng đặt lại chỗ cũ (video 6:18). Đúng thiết kế (#92), nhưng trông như bị mất kết quả.
