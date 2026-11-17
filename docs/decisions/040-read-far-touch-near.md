# Quyết định 040: Bảng vừa đọc vừa bấm: chữ cần đọc ra xa, cạnh LVS (≥ 1 m); gần tay (≤ 0,46 m) chỉ còn nút; Menu và bàn phím giữ chỗ, có ghi lý do

- **Backlog:** #129, #130, #134 (từ #125, góp ý C19)
- **Trạng thái:** accepted
- **Ngày:** 2026-11-17
- **Ý kiến đã xét:** unity-reviewer, instructional-design-reviewer, data-analyst, xr-ux-reviewer (proposal mode, độc lập); decision-synthesizer. Không hỏi requirements-analyst và german-reviewer vì không đổi luật nghiệp vụ, không thêm chữ mới.
- **Lựa chọn của người dùng (2026-11-17):**
  - Chấp nhận phương án C, kể cả mở lại phần thanh tour của 036.
  - Menu và bàn phím giữ chỗ.
  - Làm ngay, trong **một** build "037" trước design freeze 19/11.
- **Sửa / mở lại quyết định đã chấp nhận:**
  - **036**, phần „Lựa chọn của người dùng (2026-11-05)“ và mục „Không làm“: chữ của bước rời thanh tour, thanh chỉ còn nút. 036 chỉ cho mở lại khi „thanh che thùng“. Lần này mở lại vì lý do khác: góp ý C19 và số đo mới của #135 (checklist T2/T3). Người dùng đồng ý riêng phần này.
  - **026 mục 1:** dòng hành động không còn nằm trên thanh gần carton.
  - **026 mục 4 / #93** („cụm thế chỗ thanh, không hiện cùng lúc“): giữ nguyên ý. Dải nút gần đổi tour ↔ A/B/C, chữ xa đổi câu của bước ↔ câu hỏi.
  - **015 mục 3** (nút trong tầm tay): giữ nguyên.

## Bài toán

Người học vừa đọc vừa bấm các bảng gần: thanh tour và cụm câu hỏi ở Concept, cụm câu hỏi và bảng ví dụ ở Complex, Menu ở 5 scene, bàn phím nhập mã ở Welcome.
- **Khoảng cách:** các bảng này cách mắt 0,55–0,74 m, đúng vùng 0,5–0,8 m mà Meta khuyên tránh.
- **Đổi tiêu cự:** ở mỗi bước, mắt phải đổi tiêu cự giữa chữ gần và chú thích/ô LVS ở xa: Concept khoảng 0,6 D × 11 bước, Complex khoảng 1 D.
- **Cúi đầu:** bảng ví dụ ở Complex bắt cúi tới −54°.

Góp ý C19 („chữ lúc gần lúc xa, chóng mặt“, bài 2) là lý do của bài toán. Số đo và nguồn: `docs/text-comfort-checklist.md`, `TestEvidence/text-comfort/` (#135).

## Xung đột giữa các đề xuất

| Vấn đề | Một bên | Bên kia | Bản chất đánh đổi |
|---|---|---|---|
| Kéo nguyên bảng vào ≤ 0,46 m | ít việc nhất, giữ 036 | chênh tiêu cự ~1,3–1,4 D, gấp đôi hiện nay; cúi sâu hơn −35° | Rẻ ↔ làm nặng thêm đúng cái gây C19 |
| Tách chữ (xa) và nút (gần) | chữ đọc chênh ≈ 0 D, đạt cả T2 lẫn T3 | phải tách canvas, rủi ro hồi quy, mở lại 036 | Mắt thoải mái, chữ sát ô nó nói tới ↔ công M–L sát freeze |
| Dòng phản hồi quiz | đặt lên mặt phẳng xa (một độ sâu đọc) | ngay dưới hàng nút vừa bấm | Một độ sâu ↔ phản hồi ở đúng chỗ đang nhìn |
| Menu / bàn phím | kéo vào ≤ 0,46 m | giữ chỗ: kéo vào thì quay đầu > 45° hoặc che thùng đơn/thẻ kết quả; dữ liệu Menu phải tách ở 5 scene | Đạt con số T2 ↔ không lợi gì cho việc học |
| Tia (ray) | chữ và nút cùng ở xa | thêm kỹ năng ngoài nghề kho; trỏ trượt bị chấm là sai; phải đổi hợp đồng log | Không dùng |

## Quyết định (phương án C)

Vị trí chính xác đo trong lúc làm, bằng `TextComfortAudit`.

1. **Concept, tour.**
   - `TXT_Step` và `TXT_Action` lên mặt phẳng của chú thích, cạnh viền màn hình (1,2–1,3 m, cùng độ sâu với ô LVS).
   - Bước có chú thích: dòng hành động nằm trong/dưới `Caption <id>` của bước đó, nối với ô bằng khung/đường dẫn của #105.
   - Bước không có chú thích (`intro`, `not_used`, `material_flow`): ở chỗ chú thích cạnh viền.
   - Gần tay (≤ 0,46 m từ mắt) chỉ còn dải nút `Zurück` / „Schritt n von 11“ / `Weiter` / „Zu den Fragen“. Dải nút vẫn thuộc `hideWhileRiding`, và mờ dần thay vì tắt phụt.
2. **Concept, quiz.**
   - Câu hỏi + „Frage n von 4“ lên mặt phẳng xa, thay chỗ chữ của bước.
   - Hàng A/B/C (nhãn đáp án ngắn) cùng dòng phản hồi ngay dưới nút thay chỗ dải nút gần, mờ dần vào chỗ đó.
   - Thanh và cụm câu hỏi vẫn không hiện cùng lúc (#93).
3. **Complex, vị trí 1.**
   - `Worked Example` thành bảng chỉ để đọc, không nằm trên canvas có poke. Bảng ở ≥ 1 m, cạnh viền trái, ngang hàng Soll/Verp.-Einh., cúi không quá −35°.
   - Câu hỏi lên mặt phẳng xa. A/B/C và dòng phản hồi ở gần.
   - Ẩn sau 1,5 s (#93, `hideWhenPositionDone`) giữ nguyên.
4. **Menu và bàn phím (#134): giữ nguyên chỗ trong cả 5 scene.** Đây là ngoại lệ có lý do cho T2:
   - **Menu:** chỉ dùng một lần cuối bài, có `Next Lesson Highlight` dẫn tới, không có cảnh đọc xen gần–xa. Tính từ vai phải (giả định ở (0,2, 1,4, 0)), Menu cách khoảng 0,49 m.
   - **Bàn phím:** do người điều phối bấm, người học không đọc và không học gì từ nó.
5. **Log.**
   - Không đổi event hay trường. Logic chấm điểm, vòng đời `quiz_unlocked`/`tour_step`, `hideWhileRiding` và luật khoá Menu (#42) giữ nguyên.
   - `layout_version` đổi từ "036" sang "037" **một lần**, trong build đầu tiên có thay đổi này lên kính, chung cho #129 và #130.
   - Spec ghi rõ: Concept tour/quiz và Complex `position_1` không gộp "036" với "037".
   - Menu không đổi nên `menu_after_done.csv` không phải tách.
   - `make_fake_data.R` và `analyse_set.R` cập nhật trong cùng thay đổi; `selftest.R` phải xanh; `data-analyst` duyệt.

**Được:**
- Chữ đọc trong một bước cùng độ sâu với ô LVS, tức là nghi vấn chính cho C19 được xử lý.
- Không chữ đọc nào nằm ở vùng 0,5–0,8 m.
- Hết cúi −54° và hết „bảng chỉ để đọc nằm trên canvas poke“.
- Câu chỉ dẫn và ví dụ nằm sát ô/số chúng nói tới (spatial contiguity).
- Concept và Complex dùng chung một mẫu.
- Menu ổn định ở 5 scene.

**Mất / chấp nhận:**
- Mở lại lựa chọn ở 036.
- Dòng hành động không còn sát carton. Ở bước `material`/`verp_einh`, nhìn từ ô LVS xuống carton là việc của nhiệm vụ.
- Phản hồi A/B/C vẫn đọc ở gần rồi người học nhìn lên LVS: một lần nhảy có chủ đích sau mỗi lần bấm.
- Công M–L, rủi ro hồi quy ở `WmsFieldTour`, `PackageQuizController`, `RemainderQuizController`. Phải tách canvas: `questionText`, `actionText` là field serialized, và `clusterRoot.SetActive` không còn ẩn được phần chữ ở xa.
- Dữ liệu "036"/"037" của Concept và Complex không gộp được. Chưa có log "036" thật nào nên mất ít.

**Đề xuất bị bác:**
- Kéo nguyên bảng vào gần: chênh tiêu cự gấp đôi.
- Chỉ dời `Worked Example`: Concept tệ hơn hiện nay.
- Đẩy hết ra xa mà không có tia: không bấm được khi đứng yên.
- Dùng tia: lý do ở bảng trên.
- Phản hồi lên mặt phẳng xa: phản hồi phải ở chỗ vừa bấm.
- Kéo Menu/bàn phím vào gần: rủi ro che thùng đơn/thẻ kết quả, chạm nhầm; bàn phím sát mặt người học.
- Bỏ autoSize: ý đúng nhưng thuộc #132, làm sau #129/#130.

## Ảnh hưởng tới *Done when*

Ghi vào `docs/backlog.md`, các mục #129, #130, #134. Thêm ràng buộc của data-analyst mà bản tổng hợp chưa ghi: dải nút và hàng A/B/C không nằm trên đường tay đi khi nhặt hàng, vì mỗi lần chạm A/B/C là một lần trả lời được chấm.

## Làm gì nếu giả định sai

- **Phần Concept không kịp trước freeze:** ship trong **một** build "037" phương án thay thế: thanh Concept giữ chỗ, chỉ thêm mờ dần, ghi lý do giữ 0,6 D; Complex làm như mục 3. Không đổi layout hai lần, không làm kiểu „kéo thanh vào gần“.
- **Test kính cho thấy C19 do thẻ quay theo đầu (#131) chứ không do thanh:** các thay đổi ở đây vẫn có ích (T2/T6), không lùi lại.
- **Chú thích + dòng hành động không vừa ở 1,2 m (#132, ≥ 0,8°):** tách dòng hành động thành một dải ngay dưới chú thích, cùng độ sâu.
- **Dải nút ≤ 0,46 m che dialog, hoặc người thấp khó với:** hạ hoặc nghiêng dải nút. Vì chỉ là nút nên cúi quá −35° chấp nhận được.
- **Menu khó đọc/khó bấm trên kính:** nghiêng mặt Menu về phía mắt, cùng một chỗ ở 5 scene.

## Câu hỏi còn mở

- **Cho nhóm review (không chặn):** „Ở bài 2, lúc thấy chữ lúc gần lúc xa, bạn đang nhìn chữ nào: thanh dưới màn hình, chú thích cạnh màn hình, thẻ vàng trên thùng hay nhãn số trên carton?“
- **Cho người hướng dẫn:** không có.

## Bằng chứng

- **Số đo khoảng cách, góc cúi, autoSize, T12:** `TestEvidence/text-comfort/report.md`. **Đã kiểm.**
- **Ngưỡng 0,5 D:** do dự án tự đặt, không có nguồn (`docs/text-comfort-checklist.md`). **Đã kiểm.**
- **Chênh ~1,3–1,4 D khi kéo vào 0,46 m:** phép tính 1/0,46 − 1/1,25. **Suy ra**; cảm nhận chưa kiểm.
- **036 và 026:** `docs/decisions/036-concept-tour-v3.md` (lựa chọn người dùng, mục Không làm), `docs/decisions/026-concept-study-desk.md`. **Đã kiểm.**
- **Field serialized và các mảng ẩn/hiện:** `LaneFlowDemo.cs:76, 309–312`, `RemainderQuizController.cs:71–76, 366`, `PackageQuizController.cs:73–89, 266`, `WmsFieldTour.cs:65–67, 225`. **Đã kiểm.**
- **Menu cách vai ≈ 0,49 m:** vị trí vai là giả định. **Suy ra.**
- **Kéo Menu vào gần sẽ che thùng/thẻ kết quả:** **chưa kiểm.**
- **Dải nút ≤ 0,46 m không che dialog; chữ cạnh viền đủ cỡ và không che làn:** đo bằng `TextComfortAudit` và ảnh Play mới khi làm. **Chưa kiểm.**
