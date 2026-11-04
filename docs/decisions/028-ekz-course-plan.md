# Quyết định 028: Lộ trình EKZ 0/3 qua các bài: Mixed vị trí 1 dùng EKZ 003, mẫu ở Concept là carton và túi

- **Backlog:** #94
- **Trạng thái:** accepted
- **Ngày:** 2026-11-04
- **Thay thế (một phần):** 003 (câu „Mixed, Complex (EKZ 0)“ và case Mixed tương ứng: Mixed vị trí 1 nay là 003). Phần còn lại của 003 giữ nguyên.
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, data-analyst → decision-synthesizer.
- **Lựa chọn của người dùng (2026-11-04):** chấp nhận phương án A.

## Bài toán
Hiện EKZ 3 được dạy ở Concept, luyện và chấm một lần ở Basic, rồi biến mất (Mixed, Complex đều `000`). EKZ 0 chưa bao giờ được nói rõ là „đúng“. Thùng đơn lộn xộn vẫn được báo „Richtig!“ (test kính P801, R12, R16), nên người học dễ hiểu rằng xếp hay không xếp đều được. Ở Concept, hai thùng mẫu EKZ nằm trên làn giữa (đường đi của thùng), dùng vòng RING D không giống cách làm ở bài sau, và có vòng xuyên thành thùng (R10). Người dùng đề xuất vật mới (R19: ốc, cáp CEE, phụ kiện ống).

## Tài liệu nói gì (requirements-analyst)
- Slide 6/9 (Kommissionierung): carton được đặt nguyên cái, lỏng, „EKZ gerecht“. Đây là quy tắc bắt buộc duy nhất. Xếp vào ô chặt là một đơn giản hoá để đào tạo, đã được chấp nhận trong 003.
- Nghĩa của EKZ 0, 2, 3 không có trong tài liệu, chỉ có tên tài liệu CA0050. „EKZ 0 = được đổ“ vẫn là giả định (D5, D21).
- Tài liệu không cấm hai vật liệu nằm chung một thùng đơn (chỉ tách khi „100% Positionstrennung“).
- Slide 6: hàng không vừa tay phải cho vào PU-Beutel, nên „đổ ốc vít rời“ không phải là ví dụ an toàn.

## Các phương án
| | Hiệu quả học | Tải nhận thức/UX | Chi phí/rủi ro |
|---|---|---|---|
| 0. Chỉ sửa mẫu Concept | EKZ 3 chỉ luyện một lần, không đạt „độ khó tăng dần“ | tốt | rẻ, không đổi log |
| **A. Mixed vị trí 1 = 003, vị trí 2 = 000; Complex 000/000; mẫu Concept = carton/túi** | người học phân biệt EKZ 0 và 3 trong cùng một bài | carton dễ cầm | thêm 3 ô + `EkzPlacementCheck` ở Mixed, đổi log/R |
| B. A + Complex một vị trí 003 với chi tiết rời | 3 điểm đo, nhưng lỗi có thể đo độ khéo tay hoặc lỗi engine | element interactivity tăng vọt | phụ thuộc #96, có thể vượt 25 phút |
| C. Vật mới cho EKZ 3 | đổi vật và EKZ cùng lúc, không tách được lỗi theo kỹ năng | ốc nhỏ không cầm được | model, giấy phép, hiệu năng Quest |

## Quyết định: phương án A
- **Concept (bài 2):** hai thùng mẫu tĩnh (không `Grabbable`, `PackageData`, collider, log) đặt trên bàn học của decision 026, không đặt trên làn nào, gần chú thích Empf Kz. Mẫu 003 là carton KABELVERBINDER A xếp thẳng trong ô; mẫu 000 là túi SCHRAUBE B đổ vào. Bỏ vòng RING D khỏi hai mẫu. Quiz `ekz_003`/`ekz_000` và chữ tour `empf_kz_003`/`000` nói về carton/túi.
- **Basic (bài 3):** giữ nguyên `003`, 6 ô (decision 003).
- **Mixed (bài 4):** vị trí 1 KABELVERBINDER A là `003`: 3 carton vào 3 ô ở một vùng riêng của thùng đơn. Vị trí 2 SCHRAUBE B là `000`: túi đổ vào vùng còn lại. Yếu tố mới duy nhất của bài là đọc Empf Kz rồi đổi cách đặt.
- **Complex (bài 5):** giữ `000`/`000`. Độ khó tăng nhờ phần dư và chi tiết rời; người học tự đọc Empf Kz, không có gợi ý.
- **Kiểm cách xếp:** dùng cùng `EkzPlacementCheck` và cùng câu lỗi như Basic. Lỗi xếp chỉ hiện sau lỗi số lượng. Ô vẫn ẩn như 003. Chỉ package của vị trí hiện tại được kiểm; carton vị trí 1 đã khớp ô không gây lỗi ở vị trí 2.
- **Hai vật liệu chung một thùng:** được phép.
- **Log:** `EKZ_PLACEMENT` ghi ở Basic và Mixed vị trí 1. Mỗi lần xác nhận vị trí ghi Empf Kz của vị trí đó, để R tính đúng mẫu số „cơ hội EKZ“ (chỉ những lần bấm có số lượng đúng ở vị trí 003). `item` của `EKZ_PLACEMENT` vẫn là `null` (mỗi vị trí 003 chỉ có một vật liệu), không tăng `schema_version`. Tăng `layout_version`; spec, `skill_map.csv`, `make_fake_data.R`, `analyse_set.R` sửa cùng lúc, `selftest.R` xanh. Dữ liệu Mixed trước và sau không gộp.

## Được và mất
- **Được:** người học phân biệt có chủ đích EKZ 0 và EKZ 3; vật mẫu ở Concept giống vật ở bài sau; không cần model mới; không phụ thuộc #96; R đo `ekz_placement` ở 2 bài.
- **Mất:** Complex không chấm cách xếp (2 điểm đo thay vì 3); dữ liệu Mixed sau thay đổi không gộp được với P801/pilot cũ; không có phản hồi tại chính carton đặt sai.

## Đề xuất bị bác
- Complex có vị trí 003 (data-analyst, unity B, ID 2): hoãn đến khi #96 xong và đo được thời lượng khoá; có thể mở lại thành mục riêng.
- Vật mới (UX B, unity C, data C): EKZ thật của vật chưa xác nhận, thêm vật mới giữa khoá, chi phí model cao. Nếu dùng thì chỉ ở Concept (view-only) sau khi người hướng dẫn xác nhận.
- Viền tại carton đặt sai, bật `Marker*` rồi rút dần (UX): trái 003 (ô ẩn) và làm thay bước đọc WMS. Ứng viên cho mục sau nếu test kính cho thấy người học không tìm được carton đặt sai.
- `item` + `schema_version` 3 cho `EKZ_PLACEMENT` (data B.2): chưa cần.

## Giả định
- D4 (carton đặt nguyên cái, xếp theo EKZ) như 003.
- **D34 (mới):** EKZ thuộc về dòng đơn và là dữ liệu giả, nên cùng một vật liệu có thể mang EKZ khác nhau giữa các bài (KABELVERBINDER A: `003` ở Basic và Mixed vị trí 1, dù D5 coi nó là EKZ 0).
- D21–D24 (nghĩa EKZ 2, 4–9) không đổi.

## Nếu giả định sai
- D34 sai (KABELVERBINDER A không được mang EKZ 3): đổi `empfKz` vị trí 1 của Mixed về `000`, tắt (không xoá) `EkzPlacementCheck` và các ô; Mixed về phương án 0.
- D4 sai: làm theo mục „Nếu giả định sai“ của 003, áp cho cả Mixed.
- Người hướng dẫn chỉ ra một vật thật là EKZ 3 (ví dụ phụ kiện có van): mở mục mới cho vật mẫu ở Concept trước; đưa vào bài thực hành chỉ sau #96.

## Done when (thay câu điều kiện của #94)
- Ở ConceptScene, mẫu EKZ 003 là carton KABELVERBINDER A xếp thẳng trong ô, mẫu EKZ 000 là túi SCHRAUBE B đổ vào; cả hai nằm trên bàn học, không nằm trên làn nào.
- Ở ConceptScene, đứng nhìn chú thích Empf Kz thì thấy được hai thùng mẫu mà không phải quay người.
- Ở ConceptScene, không carton, túi hay vòng nào xuyên qua thành thùng mẫu.
- Quiz `ekz_003`/`ekz_000` và chữ tour `empf_kz_003`/`000` nói về carton/túi, không nói về vòng; chữ sửa qua CSV, có EN + DE (Sie), đã qua `german-reviewer`.
- Ở MixedPickingScene, vị trí 1 dialog hiện `Empf Kz` `003`, vị trí 2 hiện `000`.
- Mixed vị trí 1: đủ 150 chiếc (3 carton), cả 3 carton khớp ô, bấm xác nhận → đúng, sang vị trí 2.
- Mixed vị trí 1: đủ số lượng nhưng một carton nằm lỏng, bấm xác nhận → `Nicht richtig` với cùng câu lỗi EKZ 3 như Basic, vẫn ở vị trí 1.
- Mixed vị trí 1: thiếu số lượng và có carton lỏng, bấm xác nhận → lý do hiện ra là số lượng.
- Mixed vị trí 2: 3 túi đổ lộn xộn vào vùng còn trống, bấm xác nhận → đúng; carton vị trí 1 vẫn trong thùng và không gây lỗi.
- Ô snap của Mixed nằm trong trigger của `PlasticBox_Order` và không chiếm vùng đổ túi của vị trí 2.
- Ở ComplexScene, cả hai vị trí hiện `000`; đặt lộn xộn đúng số lượng → kết quả như trước.
- Basic: mọi case của 003 vẫn chạy như trước.
- (code) `EKZ_PLACEMENT` được log ở Basic và Mixed vị trí 1, không ở nơi khác; mỗi lần xác nhận vị trí log Empf Kz của vị trí đó; `layout_version` đã tăng.
- (code) Spec, `skill_map.csv`, `make_fake_data.R`, `analyse_set.R` sửa cùng lúc; `selftest.R` xanh; `XRPick.Orders.Tests` xanh.

## Câu hỏi cho người hướng dẫn
D35–D38 trong `docs/requirements/questions-for-instructor.md` (nghĩa EKZ 0/2/3, D34, ốc vít đổ hay túi PU, hai EKZ chung một thùng); D4 nhắc lại.

## Bằng chứng
- `Assets/Scripts/EkzPlacementCheck.cs` dòng 11–84: chỉ theo dõi `PackageData` đã snap, không gắn với vật liệu hay `OrderRule` (đã kiểm).
- `docs/research-logging-spec.md` dòng 93, 106, 192: `EKZ_PLACEMENT` chỉ ở Basic, sau các mã `OrderRule`, `item` null (đã kiểm).
- `docs/decisions/003-ekz-placement.md` dòng 31–46 (đã kiểm).
- Ảnh `TestEvidence/review-P801/10-concept-ekz-samples.png`, `16-complex-jumbled-box.webp` (xr-ux-reviewer: thấy).
- Chưa kiểm: thùng đơn Mixed đủ chỗ cho 3 ô + vùng đổ túi (đo trong Editor khi làm); `OrderConfirm` là nơi duy nhất gọi `placementCheck` (unity-reviewer).
