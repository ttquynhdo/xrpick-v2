# Quyết định 037: Ô xếp carton Empf Kz 003 thấy được và giảm dần; phản hồi sau F6 ở thành trước thùng đơn; bỏ mũi tên làn giữa ở Basic

- **Backlog:** #108, #109
- **Trạng thái:** accepted
- **Ngày:** 2026-11-05
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, data-analyst (proposal mode, độc lập); decision-synthesizer. Chữ mới qua `localize-strings` + `german-reviewer` khi làm.
- **Lựa chọn của người dùng (2026-11-05):** chấp nhận khuyến nghị (phương án B) với mặc định: Mixed vị trí 1 ô hiện sau lần `EKZ_PLACEMENT` đầu tiên (U1); lời gọi Menu chuyển thành highlight trên Menu (U2); dấu ở carton lỗi chỉ là biểu tượng `warning` (U3).
- **Thay thế (một phần):** 003 (ô ẩn, „không nêu số hay vị trí ô“, „Không làm: ô gợi ý mờ sau lần sai; rút dần gợi ý“); 028 („ô vẫn ẩn như 003“ ở Mixed vị trí 1; chính 028 để ngỏ xét lại nếu test kính cho thấy người học không tìm được chỗ sai, và P802 R2 là bằng chứng đó); 033 mục 4 (mũi tên cam trên làn giữa ở Basic). Vị trí thẻ kết quả của #59 đổi, khuôn thẻ giữ nguyên.
- **Giữ nguyên:** 003 D6 (đủ số lượng nhưng xếp sai thì đơn chưa xong), Ist không phụ thuộc trạng thái khớp ô; 006 (chỉ hiện lỗi đầu tiên, không in số đích); 017/035 (khung xanh, ✓); 033 mục 1–3, 5–7.

## Bài toán
P802 (`docs/review-notes-p802.md` R2, R5, R8, R16): ở Basic và Mixed vị trí 1 người học chồng/đặt nghiêng carton lên carton khác → `EKZ_PLACEMENT`, lấy carton ra, bấm khi đang thiếu → `TOO_FEW`. Ô snap vô hình; carton kinematic nằm yên chỗ thả nên nghiêng lơ lửng; Ist = Soll làm thùng trông như đã xong. Thẻ kết quả cách chỗ nhìn (thùng đơn) khoảng 20°, người học bấm lại kiểu thử. Mũi tên cam ở Basic mang nghĩa khác marker „cầm/nhìn cái này“ của Concept và đè lên biển thùng.

## Quyết định
1. **Ô (Basic, Mixed vị trí 1).** Basic: viền ô mờ trên đáy thùng đơn luôn hiện (bật lại `Marker*` với vật liệu viền); ô trống nhận ra được bằng hình, không chỉ bằng màu. Mixed vị trí 1: ô ẩn lúc đầu (kể cả khi cầm carton); từ lần `EKZ_PLACEMENT` đầu tiên ở vị trí đó, 3 ô hiện như Basic tới hết vị trí 1; ô tắt cùng lúc lật trang của 035 (`Settled`). Mixed vị trí 2 và Complex: không có ô.
2. **Thả carton (Basic, Mixed).** Thả gần ô trống → khớp ô, thẳng, có âm thanh (vùng hút rộng hơn một chút). Mỗi ô chỉ nhận một carton (kiểm lại #56). Thả trong thùng mà không vào ô → carton **nằm phẳng**, đáy song song đáy thùng, tựa lên vật bên dưới; **không tự chạy về ô** (nếu không, lỗi EKZ không bao giờ xảy ra). Chỉ chạy sau lần Unselect cuối, không khi đang cầm, không khi `IsMoving`/`IsShipped`, không dời carton ra khỏi trigger thùng. Túi (000): không đổi.
3. **Lỗi xếp.** Mã vẫn `EKZ_PLACEMENT`, lỗi số lượng vẫn hiện trước. Mỗi carton lỏng có một biểu tượng Meta `warning` vàng cạnh carton (như hint tag Concept #100, không phải mũi tên cam); tắt khi kết quả bị xoá (#4). Câu lỗi: câu đầu giữ quy tắc thật (một chiếc một chỗ, có thứ tự, không chồng), câu sau nói việc sửa trong bài tập (đặt carton có dấu vào một ô trống). Không số, không „6 ô“.
4. **Thẻ kết quả (Basic, Mixed, Complex).** Cùng khuôn #59 và `ResultCardStyle`. Object riêng, cố định, không phải con của `PlasticBox_Order`, ở mặt ngoài thành trước thùng đơn (≈ (0.45, 0.80–0.85, 0.48)), nghiêng về phía mắt, không raycast, cùng toạ độ ở 3 scene. „Richtig!“ và câu báo thùng chạy ở cùng thẻ. Basic và Mixed cuối: thẻ ẩn khoảng 2 s sau `OrderBoxConveyor.Departed`. Lời gọi Menu („Wählen Sie jetzt im Menü …“) chuyển thành `Next Lesson Highlight` xanh trên mục Menu kế tiếp sau `Departed` (mẫu Welcome #39 / Concept #93).
5. **Mũi tên.** Basic `Station Ride Cue`/`Middle Lane Arrow` bỏ (`scene-ref-auditor` trước khi xoá). Không thêm chevron.
6. **Log/R.** Định dạng sự kiện không đổi. `layout_version` mới, **một giá trị chung với 036**. `item_skills.csv` thêm Basic `position_1` × `ekz_placement` = có hỗ trợ; `analyse_set.R` lọc theo cặp press × skill (không theo item), nên Basic vẫn đo được `calculate_quantity`. Mixed vị trí 1 không đánh dấu hỗ trợ: P2 = lần cơ hội EKZ đầu tiên ở Mixed vị trí 1 (lần đầu luôn không có ô; `data-analyst` xác nhận R suy ra được mức hỗ trợ thích ứng mà không cần trường mới, nếu không thì thêm trường). Dữ liệu `ekz_placement` của P802 không gộp với layout mới. Spec, `make_fake_data.R`, `selftest.R` sửa cùng lúc.

## Thống nhất với 036
Mũi tên cam chỉ xuống chỉ có một nghĩa: „nhìn/cầm cái này“, chỉ ở tour Concept. Thành trước thùng = chỗ phản hồi về những gì trong thùng (hint tag #100, thẻ kết quả, dấu `warning`). Biểu tượng `warning` vàng = „có gì sai ở đây“ ở mọi scene. `Box Sign` (#111) ở mặt trong thành sau, không va với thẻ ở thành trước. Một `layout_version` cho cả build.

## Được / mất
- **Được:** tiêu chí chấm thấy được ở lần luyện đầu; hỗ trợ giảm Basic → Mixed, Mixed vẫn giữ việc đọc Empf Kz; hết carton nghiêng lơ lửng mà nhiệm vụ xếp không bị bỏ; phản hồi đọc được khi nhìn thùng, nói sửa gì, ở đâu; mỗi ký hiệu một nghĩa; #109 không phải chờ #111.
- **Mất:** Basic không còn là lần đo P2 không hỗ trợ (Mixed vẫn đo); ô là trợ giúp trạm thật không có (đơn giản hoá có ghi rõ); chỉ ra carton sai làm lần bấm thứ 2 trở đi chỉ đo thử–sai (R đã lấy lần đầu); công M, test kính bắt buộc; thẻ có thể gần tay (phải đo).

## Làm gì nếu giả định sai
- Thẻ ở thành trước vướng tay/khó đọc: dời trên cùng thành (thấp hơn, lệch phải), hoặc lùi về thẻ ở nút đỏ + dấu tại carton.
- „Nằm phẳng“ gây lỗi với SDK: tắt script; còn ô thấy được + vùng hút rộng hơn.
- Mixed vẫn sai lần đầu nhiều: Mixed vị trí 1 đổi sang „ô hiện khi cầm“ (một cờ), tăng `layout_version`.
- Người hướng dẫn nói carton 003 được chồng lớp (D43/D4/D35): carton nằm phẳng trên lớp dưới tính là „geordnet“ (đổi `AllPlaced`), không còn dấu lỗi cho trường hợp đó.
- D6 = chỉ nhắc / D4 = carton đặt lỏng: như 003.

## Câu hỏi cho người hướng dẫn (không chặn)
D4, D6, D35 (còn mở); D43 (trong Auftragsbehälter có vạch/chỗ cố định cho carton EKZ 3 không; carton EKZ 3 có được xếp nhiều lớp không?); D44 (sau F6, khi số lượng hoặc cách xếp sai, người lấy hàng thấy gì: thông báo, âm, đèn?).

## Bằng chứng
`Assets/Scripts/EkzPlacementCheck.cs` (chỉ biết carton đã snap; Ist độc lập); `Assets/Scripts/OrderFeedback.cs` + CSV `order.feedback.ekz3_loose`; `docs/decisions/003-ekz-placement.md`, `028-ekz-course-plan.md`, `033-confirm-pause-container-change-cue.md`; `Assets/Scripts/StationRideCue.cs`; carton Basic kinematic, không trọng lực (`BasicPickingScene.unity`); `analysis/analyse_set.R:1911` (`scaffolded` lọc theo item), `analysis/objectives.csv`; `docs/pilot-P802-summary.md`; Kommissionierung slide 6, 8, 9 (qua requirements-analyst: EKZ 3 „einzeln und orientiert“, không có tài liệu về ô, marker hay phản hồi sau F6). Chưa đo: toạ độ thẻ mới so với tay, nút đỏ, `Box Sign`; ô có nhận nhiều carton không.

## Sửa đổi #131 (2026-11-17, người dùng chọn)
Bỏ dấu `warning` vàng trên carton lỏng (mục 3, U3). Từ bản 034 mỗi ô chỉ nhận 1 carton, carton thả gần ô trống thì tự khớp và `CartonSettler` đặt carton lỏng nằm phẳng, nên carton lỏng luôn nằm chồng lên carton khác hoặc ngoài cột ô viền; ô viền trắng còn trống chỉ chỗ đặt. Dấu nổi trên carton và quay theo đầu (checklist T8, nghi vấn C19). Câu 2 của `order.feedback.ekz3_loose`: „Legen Sie jeden Karton auf einen eigenen Platz.“ Người học tự tìm carton sai (lần bấm thứ 2 không còn chỉ là thử–sai theo dấu). Thẻ báo Concept (#100) dán phẳng lên thành trước thùng, không quay theo đầu.


## Sửa đổi #157 (2026-11-17, người dùng chọn)
- **Mục 1 (Mixed vị trí 1, #152):** 3 ô viền hiện **ngay từ đầu** vị trí 1, như Basic (`showPlacesAtStart` bật). Lý do: buổi thử R13 — không thấy ô thì người thử không biết đặt đâu và hiểu là lỗi. Ô vẫn tắt khi thùng kho vị trí 2 dừng (`Settled`).
- **Mục 2 (#140):** carton thả **trên** một ô trống (tới 15 cm trên miệng thùng) thì vào ô đó; carton đặt trong ô cầm lên thì rời ô hẳn. Thả ngoài các ô vẫn nằm lỏng (lỗi EKZ vẫn xảy ra được).
- **Mục 6 (log/R):** người dùng chấp nhận mất phép đo: từ `layout_version` „038“ không lần bấm nào ở Basic hay Mixed `position_1` còn đo `ekz_placement` (ô hiện ở cả hai) → mục tiêu **P2 = `observed`** (không `rated`) cho build „038“; `EKZ_PLACEMENT` vẫn được log (lỗi khi đang thấy ô). Hiểu biết 003/000 vẫn được chấm qua câu hỏi Concept (`ekz_003`/`ekz_000`) và Complex (`ekz`) = D3. `skill_scaffolds.csv`: Mixed `position_1` `always` từ „038“.
- **Thả ngoài ô / ngoài thùng (#153):** mọi kiện thả giữa không trung rơi thẳng (vẫn kinematic) vào thùng bên dưới, lên mặt bàn, hoặc — trên sàn — về chỗ đã lấy (`PackageDropper`). Thứ tự quyết định: ô trống → `CartonSettler` (thùng đơn Basic/Mixed) → thùng bên dưới → mặt bàn → về chỗ lấy.

## Sửa đổi khi làm #181 (decision 043 mục 2, 2026-11-26)
- `order.feedback.ekz3_loose` đổi thành „Bei Empf Kz 003 braucht jeder Karton einen eigenen Platz. Nutzen Sie die freien Plätze.“ (3 dòng trên thẻ). Câu này vẫn nối với luật Empf Kz 003 và chỉ ra ô viền trắng còn trống; phần nhắc lại luật dài đã bỏ.
