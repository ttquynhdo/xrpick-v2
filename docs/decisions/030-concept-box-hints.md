# Quyết định 030: ConceptScene: LVS mẫu đếm theo thùng đơn, kiện sai hoặc thừa trong hai thùng được báo ngay bằng thẻ cạnh kiện

- **Backlog:** #100 (+ phần bổ sung 2026-11-05 của #91)
- **Trạng thái:** accepted
- **Ngày:** 2026-11-05
- **Ý kiến đã xét:** unity-reviewer, requirements-analyst, xr-ux-reviewer, data-analyst, instructional-design-reviewer → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-05):** chấp nhận phương án C, với ba mặc định của khuyến nghị: bước 11 chờ tới khi hai thùng sạch, mỗi lúc chỉ một thẻ lời nhắc, ba carton đếm hiện lại ở bước 11. Trước đó người dùng đã chọn: kiểm thùng kho chỉ ở ConceptScene, các bài khác giữ nguyên.
- **Sửa một phần các quyết định đã chấp nhận:**
  - **010:** bỏ mục „nguồn đơn mẫu cố định, không bắn `ContentsChanged`“ và câu „không có kiện STECKER C trong scene“ (đã cũ từ #84). Lý do: mục tiêu của người dùng là số trên LVS khớp với thùng. Ist vẫn được giảng như khi cân (đơn 120 chiếc ≥ 8 chiếc, slide 7), nên chú thích `concept.tour.ist.body` giữ nghĩa.
  - **025:** bỏ dòng 52 („Concept không có validator, đặt sai không được báo“) và dòng 36 („không event mới“). Lý do: người dùng yêu cầu rõ phải báo kiện sai/thừa ở cả hai thùng, chỉ trong ConceptScene. Dòng 85 (ở bước 11 thùng đơn mang 3 carton đi) được làm đúng lại.

## Bài toán
Ở ConceptScene, LVS mẫu đang hiện Ist 40 và Rest 360. Thực tế thùng đơn rỗng, còn thùng kho có 6 carton STECKER C (240 chiếc), nên theo decision 001 Rest phải là 120. 4 kiện trên mép bàn cầm được và bỏ được vào cả hai thùng, nhưng bỏ gì vào cũng không có phản hồi. Ở bước 11 thùng đơn chạy đi rỗng (carton đếm đã ẩn) và bỏ lại kiện nằm trong đó (`LaneFlowDemo.cs:145`). Người dùng muốn số trên LVS khớp với hai thùng; khi có vật khác Mat.-Nr hoặc thừa số lượng thì scene báo ngay và bảo lấy ra, để dạy luôn quy tắc.

## Xung đột giữa các đề xuất
| Vấn đề | Một bên | Bên kia | Đánh đổi |
|---|---|---|---|
| Đặt lời nhắc ở đâu | unity-reviewer: dòng hành động `TXT_Action` của `Tour Nav Bar` | xr-ux-reviewer: thẻ cạnh kiện sai; instructional-design-reviewer: sát thùng | Dùng lại kênh có sẵn ↔ liền kề không gian. Bar cách hai thùng 26–32° ngang và bị ẩn ở bước 11 (`hideWhileRiding`) |
| Có kiểm thùng kho không | instructional-design-reviewer (PA 1): không, quy tắc dựa trên D12 chưa chốt, không bài nào thực hành lại | người dùng (yêu cầu rõ), xr-ux-reviewer, unity-reviewer: có | requirements-analyst: được phép, không mâu thuẫn tài liệu, nhưng chữ không được nói đây là quy tắc WMS |
| STECKER C vào thùng đơn trước bước `cartons` có vào Ist không | xr-ux-reviewer, instructional-design-reviewer: có | unity-reviewer, data-analyst: không, để LVS đi theo demo | Đúng nghiệp vụ (cân = cái nằm trong thùng) ↔ demo xác định, so được giữa người học |
| STECKER C vào thùng kho có đổi Rest không | xr-ux-reviewer: Rest +40 | instructional-design-reviewer, unity-reviewer, data-analyst: giữ nguyên | Số khớp với mắt nhìn ↔ Rest là số hệ thống tính (slide 7, 001) |
| Bước 11 | data-analyst: mang kiện đi để không bị kẹt; unity-reviewer: không chở kiện đang cầm | instructional-design-reviewer, xr-ux-reviewer: chờ thùng sạch | Không kẹt ↔ không cho xem ví dụ sai. Nếu chờ thì bắt buộc có log lời nhắc |
| Log | data-analyst A: event riêng `concept_hint_shown`/`_cleared` | data-analyst B: mở `container_*` cho Concept | B tốn hơn mà không trả lời thêm câu hỏi nào (việc bỏ kiện vào là tự nguyện, mẫu số vô nghĩa) |
| Chữ lời nhắc | instructional-design-reviewer: dùng lại `order.feedback.*` | unity-reviewer: key `concept.*` mới | Thống nhất với bài thực hành ↔ lời riêng. Thùng kho không có key sẵn |

## Các phương án
| | Đúng nghiệp vụ | Hiệu quả học | Tải nhận thức/UX | Chi phí/rủi ro | Nhất quán | Veto |
|---|---|---|---|---|---|---|
| A. Chỉ sửa số LVS, không kiểm | + Rest đúng 001 | − không dạy quy tắc; ở bước `cartons` thùng có thể có 4 carton khi phép tính nói 3 | 0 | ++ | 0 | không đạt mục tiêu (2) |
| B. Chỉ kiểm thùng đơn, lời nhắc sát thùng đơn, Ist theo thùng đơn, Rest cố định, bước 11 chờ | + không dạy D12 khi chưa có nguồn | + cùng hai lỗi Basic/Mixed chấm | + | + một trigger | + | trái yêu cầu rõ của người dùng về thùng kho |
| **C. Kết hợp: kiểm hai thùng, thẻ cạnh kiện, Ist theo thùng đơn, Rest cố định, log event riêng, bước 11 chờ** | + Ist = cái nằm trong thùng (cân, slide 7); Rest = số hệ thống (001); quy tắc thùng kho là quy ước của bài (D12) | ++ báo ngay, lý do + việc cần làm, cùng chữ với Basic/Mixed; báo ngay ở Concept, chỉ sau nút đỏ ở bài thực hành (fading) | + thẻ tại kiện cần lấy ra, cùng kiểu Label Tag (#90), mỗi lúc một thẻ; − thêm chữ trong scene „ít chữ“ | − M: hai trigger, logic thuần + EditMode test, `SampleWmsOrder` động, LaneFlowDemo chờ, 2 event log + R | + đúng mẫu counter (`PackageData`, HashSet) | — |
| D. Counter Complex cho hai thùng, lời nhắc ở bar, bước 11 mang kiện đi, `container_*` cho Concept | 0 thùng chạy đi mang kiện sai là ví dụ sai | − lời nhắc xa thùng | − bar ẩn ở bước 11 | −− rider cầm được, hai counter cùng gọi `OrderContainerLog.Changed`, đổi `count_after` trong R | − | — |

## Quyết định: phương án C
1. **LVS.** Ist = số chiếc STECKER C trong trigger của thùng đơn (carton đếm đang hiện + carton STECKER C người học bỏ vào), như counter của ba bài thực hành. Rest cố định 120 (6 × 40 − 120, decision 001), không đổi khi người học bỏ STECKER C vào thùng kho. Vào scene: Ist 0, Rest 120. `SampleWmsOrder` bắn `ContentsChanged` để `WmsDialogView` tự làm mới; không vẽ gì lên dialog.
2. **Kiểm thùng đơn.** Kiện không phải `stecker_c` là sai vật liệu, gồm cả carton RING D đã mở (`VPEOpenedDisplay`) và vòng rời (`OrderContents` bỏ qua carton đã mở, nên Concept cần quy tắc riêng). Ist > Soll là thừa. Chữ dùng lại `order.feedback.wrong_material` và `order.feedback.too_many` (`{name}` chỉ có thể là STECKER C, không lộ đáp án quiz).
3. **Kiểm thùng kho.** Kiện có `MaterialKey` khác `stecker_c` → key mới `concept.hint.storage_wrong_material`, ý: trong thùng kho này có vật liệu khác Mat.-Nr, hãy lấy ra. Viết như quy ước của bài, không nói „LVS/WMS báo“. STECKER C bỏ vào thùng kho: không báo.
4. **Thẻ lời nhắc.** Cùng kiểu `Label Tag` (#90), dòng vàng + biểu tượng cảnh báo, ngay cạnh kiện sai. Hiện khi kiện nằm trong trigger, không ai cầm, đã yên khoảng 0,5 s. Mỗi lúc một thẻ: kiện sai được thả vào sớm nhất mà còn trong thùng (như #14); lấy kiện đó ra thì thẻ của kiện sai kế tiếp hiện. Cầm kiện lên: thẻ tắt, Held Label Tag hiện như bây giờ. Không bao giờ in số đích.
5. **Bước 11.** Ba carton đếm hiện lại trong thùng đơn (Ist 120), thùng kho còn 3 carton (khớp 025 dòng 85). Demo chỉ bắt đầu khi đã qua 4 s và không còn lời nhắc nào; trong lúc chờ bar vẫn hiện. Carton STECKER C cầm được nằm trong thùng kho (không ai cầm) chạy theo như rider, collider và interactable tắt như `PositionStockSwitcher`; khi demo reset nó về chỗ cũ trên bàn. Cụm câu hỏi vẫn mở theo `StepShown`, nên việc chờ không chặn bài học.
6. **Log.** Thêm `concept_hint_shown` và `concept_hint_cleared` với `container` (`order`/`storage`), `reason` (`WRONG_MATERIAL`/`TOO_MANY`), `object_id`, `material_key`, `tour_step_id`. Concept không ghi `container_*`, `quiz_answer` hay lượt thử từ lời nhắc. Đổi chung trong build "029": spec, `make_fake_data.R`, `analyse_set.R` cùng lúc.
7. Carton STECKER C trên bàn vẫn là mẫu đọc nhãn ngoài hai thùng (cả năm agent cùng ý; marker, `heldMaterialKey` và thẻ 1 cần nó).

**Được:** đạt cả hai mục tiêu; người học gặp trước đúng hai lỗi mà Basic/Mixed chấm, cùng câu chữ; lời nhắc nằm tại vật cần lấy ra, kể cả ở bước 11 khi bar ẩn; thùng đơn không chạy đi mang kiện sai hay chạy đi rỗng; data-analyst có biến tiếp xúc cho Q1 và thấy chỗ kẹt ở bước 11.

**Mất / chấp nhận:**
- Quy tắc „một thùng kho, một Mat.-Nr“ dựa trên giả định D12; bài thực hành không chấm lại (lỗ hổng đã biết). Chấp nhận vì không sai theo tài liệu, người dùng yêu cầu rõ, chữ không gán nó cho WMS.
- Ist không còn tất định: nếu STECKER C vào thùng đơn trước bước `cartons`, thẻ demo ghi 40/80/120 còn Ist ghi 80/120/160; khi đó `too_many` chỉ vào carton thừa, chính là lúc dạy.
- Trường hợp hiếm: STECKER C trong thùng kho → thùng có 7 carton, Rest vẫn 120 (Rest là số hệ thống).
- Công M và thêm chữ trong scene „ít chữ“; chữ chỉ hiện khi làm sai.

**Đề xuất bị bác:** lời nhắc ở `TXT_Action`/bar (xa thùng, ẩn ở bước 11, dễ để chữ cũ); chỉ sửa số (không đạt mục tiêu 2); counter đầy đủ + mang kiện đi (rủi ro cao, ví dụ sai); không tính STECKER C trước bước `cartons` (lệch nghĩa cân; muốn đổi thì thêm một cờ); Rest +40 (Rest là số hệ thống); `container_*` cho Concept (tốn hơn, không trả lời thêm câu nào); không log (bước 11 chờ thì bắt buộc có log); mang kiện đi ở bước 11 (ví dụ sai; quiz không chờ demo nên rủi ro kẹt thấp, log sẽ cho thấy); không kiểm thùng kho (trái lựa chọn của người dùng); kiện tự trượt về (không giải thích, giữ làm dự phòng); bỏ „Beim Wiegen“ (không cần, đơn 120 chiếc là trường hợp cân; chỉ rà qua `german-reviewer`).

## Done when
#91: thay câu „Ở bước 11: thùng kho rời đi với 3 carton“ bằng các câu bước 11 dưới đây. #100 thay bằng:
- Vào ConceptScene: thùng đơn rỗng, LVS hiện Soll 120, Ist 0, Rest 120.
- Bỏ carton STECKER C cầm được vào thùng đơn trước bước `cartons`: LVS hiện Ist 40, Rest 120, không có lời nhắc.
- Ở bước `cartons`, khi 3 carton đếm đã ở trong thùng đơn, bỏ carton STECKER C vào thùng đơn: sau khoảng 0,5 s LVS hiện Ist 160 và cạnh carton đó hiện thẻ vàng chữ `order.feedback.too_many`. Lấy carton ra: thẻ tắt, Ist 120.
- Bỏ KABELVERBINDER A, SCHRAUBE B, carton RING D đã mở hoặc một vòng RING D vào thùng đơn: cạnh kiện đó hiện thẻ vàng chữ `order.feedback.wrong_material`, Ist không đổi. Lấy ra: thẻ tắt.
- Bỏ KABELVERBINDER A, SCHRAUBE B, carton RING D đã mở hoặc một vòng RING D vào thùng kho: cạnh kiện đó hiện thẻ vàng `concept.hint.storage_wrong_material`. Lấy ra: thẻ tắt.
- Bỏ carton STECKER C vào thùng kho: không có thẻ, Rest vẫn 120.
- Bỏ một vòng RING D vào thùng đơn rồi một túi SCHRAUBE B vào thùng kho: chỉ có thẻ ở kiện được thả trước. Lấy kiện đó ra: thẻ hiện ở kiện còn lại.
- Cầm một kiện đưa qua thùng mà không thả: không có thẻ.
- Cầm lại kiện đang có thẻ: thẻ vàng tắt, thẻ nhãn của #90 hiện.
- Không thẻ nào ghi một con số đích.
- Ở bước 11, khi hai thùng không có kiện sai: thùng đơn có 3 carton đếm, LVS hiện Ist 120, thùng kho có 3 carton; sau 4 s thùng kho chạy đi, rồi thùng đơn mang 3 carton.
- Ở bước 11, khi một vòng RING D nằm trong thùng đơn: thẻ vàng hiện cạnh vòng, không thùng nào chạy, thanh vẫn hiện. Lấy vòng ra: demo bắt đầu.
- Ở bước 11, khi một carton STECKER C cầm được nằm trong thùng kho: carton chạy theo thùng kho. Chọn „Von vorn“: carton nằm lại chỗ cũ trên bàn và cầm được.
- Không có gì vẽ lên dialog LVS.
- (code) Logic sai/thừa là hàm thuần trong `XRPick.Orders` có EditMode test; kiện nhận diện qua `GetComponentInParent<PackageData>()`, mỗi kiện đếm một lần.
- (code) Lời nhắc không ghi `quiz_answer` hay `tour_step`, không đổi `ConceptCardTracker` hay số lượt thử của Concept; Concept không ghi `container_*`.
- (code) `concept_hint_shown`/`concept_hint_cleared` ghi `container`, `reason`, `object_id`, `material_key`, `tour_step_id`; spec, `make_fake_data.R`, `analyse_set.R` sửa cùng lúc, `selftest.R` xanh, `data-analyst` duyệt; build có `layout_version` "029".
- (code) Key mới và chú thích `concept.tour.ist.body`/`.rest.body` qua `localize-strings` + `german-reviewer`; decision 010 và 025 ghi là đã bị 030 sửa một phần.

## Nếu giả định sai
- Người hướng dẫn bác D12: tắt phần kiểm thùng kho (cờ riêng, key riêng) → còn phương án B.
- Có người bị kẹt ở bước 11 (log hoặc buổi thử): demo tự đưa kiện sai về chỗ cũ trên bàn rồi mới chạy.
- Thẻ demo và Ist lệch nhau gây rối: thêm cờ không tính kiện thả vào trước bước `cartons`.
- Thẻ khó đọc ở ~1,15 m: tăng tỉ lệ thẻ, không đổi chỗ.

## Câu hỏi còn mở
- Người hướng dẫn: D12 (thùng kho chỉ một Mat.-Nr? gặp vật lạ thì làm gì), A8 (thừa so với Soll thì LVS xử lý thế nào). Không câu nào chặn việc làm: phương án an toàn với cả hai câu trả lời.

## Bằng chứng
- `SampleWmsOrder` cố định, event rỗng → `Assets/Scripts/Concept/SampleWmsOrder.cs:16, 25-30` → đã kiểm
- `WmsDialogView` làm mới khi có `ContentsChanged` → `Assets/Scripts/WmsDialogView.cs:82` → đã kiểm
- Thùng đơn chạy đi không mang kiện → `Assets/Scripts/Concept/LaneFlowDemo.cs:145` → đã kiểm
- 010: nguồn tĩnh, „không có kiện STECKER C“, Ist chỉ khi cân → `docs/decisions/010-wms-fields-teaching.md:37, 40, 48` → đã kiểm
- 025: không event mới, bác tự đặt carton, bước 11 thùng đơn mang 3 carton → `docs/decisions/025-concept-show-and-do.md:36, 52, 85` → đã kiểm
- Chữ `wrong_material` nói về Auftragsbehälter, không có số; `too_many` có `{name}` → `docs/i18n/strings-todo.csv:42, 44` → đã kiểm
- Ist tăng khi cân từ 8 chiếc; Rest = tồn thùng kho; quy tắc thùng kho không có trong tài liệu → Kommissionierung slide 7, 18–19; câu hỏi B7/B7a, A8, D12 (báo cáo requirements-analyst) → chưa kiểm
- Góc nhìn 26–32° tới thùng, 45° cúi, ~1,15 m → tính toán của xr-ux-reviewer → chưa kiểm
- Lời nhắc trùng kỹ năng `check_material`; R cảnh báo khi `container` ≠ "order" → `analysis/item_skills.csv:3`, `analyse_set.R:139, 1027-1028` (báo cáo data-analyst) → chưa kiểm
- Collider vật lý thật trên `PlasticBox_Storage`/`PlasticBox_Order`; rider cầm được đi cùng `LaneRide` → cần kiểm trong Editor → chưa kiểm
