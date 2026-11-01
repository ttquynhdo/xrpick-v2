# Quyết định 003: Biến EKZ thành việc trainee phải làm khi đặt hàng vào thùng đơn

- **Backlog:** #41
- **Trạng thái:** accepted
- **Ngày:** 2026-11-01
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-01):** phương án B, chỉnh theo câu trả lời của nhóm cho D4–D7 và A1 (xem mục *Câu trả lời của nhóm*). Các câu trả lời này chưa phải của người hướng dẫn; khi người hướng dẫn trả lời khác thì làm theo mục *Nếu giả định sai*.

## Bài toán
EKZ hiện chỉ là chữ trên WMS (`EKZ: 003` ở Basic; Mixed/Complex không có trường này). Cách trainee đặt hàng không phụ thuộc EKZ: `CalcForWMS` đếm theo trigger, không xét kiện có khớp vào ô snap hay không. Ở Basic, 4 ô xám của `BoxSnapper` hiện sẵn trong thùng đơn, nên trainee chỉ cần nhắm theo ô. Nhóm phát triển muốn dùng snap để thể hiện EKZ 3 ở Basic và đặt lỏng để thể hiện EKZ 0 ở các scene khác, để trainee so sánh được hai cấp.

## Xung đột giữa các đề xuất
| Vấn đề | Một bên | Bên kia | Đánh đổi |
|---|---|---|---|
| Có bắt carton snap theo EKZ 3 không | unity: có | requirements, ID: bảng EKZ nói về từng chiếc; slide 6/9 nói carton đặt "lose" | Làm được ngay, nhưng có thể dạy sai (đã giải bằng D4) |
| Snap có ảnh hưởng Ist không | unity A: chỉ tính Ist cho kiện đã snap | UX: kiện trong thùng mà Ist không đổi thì trainee tưởng app hỏng; gộp lỗi số lượng với lỗi cách đặt | Ít code hơn, nhưng feedback không nói được sai ở đâu và chồng với #34 |
| Ô xám trong thùng | hiện trạng: hiện sẵn | ID: rút dần; UX: ẩn ngay | Dễ làm, nhưng không cần hiểu EKZ |
| Làm thật hay chỉ nhận biết | ID 3: câu hỏi chọn cách xử lý | ID 1, UX 1, unity A: đặt thật | Rẻ, nhưng không đạt mục tiêu "phải làm" |
| EKZ 0 cho ring | ID: không tăng tải | requirements: ring có thể là Magnetring/xi lanh (EKZ 4–6); UX: chỉ đổi tên thì không học được gì | Nhất quán, nhưng có thể gắn sai cấp (đã giải bằng D5) |

## Các phương án
| | Đúng nghiệp vụ | Hiệu quả học | Tải nhận thức/UX | Chi phí/rủi ro | Nhất quán | Veto |
|---|---|---|---|---|---|---|
| 0. Chỉ hiện `Empf Kz` (#10) và dạy ở #20 | 0, ô xám vẫn hiện mà không ai chấm | −− không có luyện/kiểm | 0 | ++ | + | không đạt mục tiêu |
| A. Snap cứng: Ist chỉ tính kiện đã snap, giữ ô hiện | − | + nhưng không tách được loại lỗi | − Ist đứng yên khi kiện đã trong thùng | − | − đổi nghĩa Ist | — |
| **B. Kết hợp** | + (theo D4, D5) | ++ | + | − cần component mới, phải test trên Quest | + | — |
| C. Câu hỏi nhận biết trước khi xác nhận | 0/− thêm bước ngoài quy trình | 0 | − | + | 0 | — |

## Phương án được chọn: B
- **Basic (EKZ 3):**
  - Ẩn ô xám (MeshRenderer của `BoxSnapper`). Snap chỉ giúp nắn carton vào ô khi trainee thả gần ô, đúng hướng; có tín hiệu khớp không chỉ bằng màu (âm thanh hoặc rung); gỡ ra được.
  - Số lượng đếm như cũ (theo `PackageData`, sau #2). Ist/Rest không phụ thuộc trạng thái khớp, nên #34 và decision 001 vẫn làm độc lập.
  - Khi bấm xác nhận mới kiểm thêm "mỗi carton đã khớp một ô". Carton nằm lỏng trong thùng = đơn chưa hoàn thành, báo sai cách đặt theo EKZ 3, không nêu số hay vị trí ô.
  - `Empf Kz` hiện `003`.
  - Đọc trạng thái khớp qua event của Interaction SDK. Nếu snap trên Quest gây khó cầm nắm thì chuyển sang kiểm vị trí bằng code (unity B), *Done when* giữ nguyên trừ ca "tự khớp".
- **Mixed, Complex (EKZ 0):** `Empf Kz` hiện `000` cho mọi vật tư (Ring D, Screw B, Cable Connector A). Đặt lỏng là đúng, logic đúng/sai không đổi. Phần hiển thị làm cùng #10.
- **Không làm:** nhãn EKZ trên mép thùng (UX 2: lặp thông tin, làm thay bước đọc WMS); ô gợi ý mờ sau lần sai; rút dần gợi ý (để sau, khi chốt tên bài #18).
- **Thứ tự:** làm #24 (kiện thừa) trước phần Basic của #41, để không dồn hai yếu tố mới vào một bài.
- **Đề xuất bị bác:** unity A (Ist chỉ tính kiện đã snap; cơ chế đọc event vẫn được dùng); ID 3 / C chuyển sang #20 làm phần kiểm tra hiểu; UX 3 (câu hỏi "vì sao ring được để lỏng") chuyển sang #20/#14.

## Câu trả lời của nhóm (2026-11-01, chờ người hướng dẫn xác nhận)
- **A1:** đơn Basic đúng khi **tổng số chiếc** bằng Soll: 300 chiếc = 6 carton × 50 (quy tắc của `CalcForWMS`/`OrderConfirm`). Quy tắc "2 GG" của `PackageCounter` bị bỏ (#1).
- **D4:** ở Basic, người lấy hàng đặt **cả carton**. Cách hiểu của nhóm: "lose" ở slide 6 nghĩa là không cho vào túi PU, còn "EKZ gerecht" nghĩa là carton vẫn xếp theo EKZ. Áp EKZ 3 (bảng nói về từng chiếc) cho carton là đơn giản hoá có chủ ý.
- **D5:** Ring D (vòng) và Screw B (dạng trụ) là **EKZ 0**; Cable Connector A cũng EKZ 0. Quy tắc xi lanh ở slide 12–15 là cho xi lanh khí nén, không áp cho ốc vít; Ring D không được mô tả là vòng nam châm (slide 16). Vật tư là dữ liệu giả (B10).
- **D6:** đủ số lượng nhưng có carton không khớp ô → đơn **chưa hoàn thành**, báo sai cách đặt.
- **D7:** theo spec dialog thật. Spec không có ô "EKZ Hinweis", nên `Leerräume auffüllen` bị bỏ; câu `Bitte Material umpacken!` không có trong tài liệu, cũng bỏ (#10, #18).

## Done when
- Lúc vào BasicPickingScene, đáy thùng đơn không còn ô xám nào.
- Ô `Empf Kz` trên dialog của Basic hiện `003`.
- Thả một carton gần một ô, đúng hướng → carton tự khớp vào ô, có âm thanh hoặc rung (ca thủ công trên Quest/Simulator).
- Cầm một carton đã khớp, kéo ra khỏi ô → lấy ra được.
- Đủ 300 chiếc, carton nào cũng khớp một ô, bấm xác nhận → `Correct`.
- Đủ 300 chiếc nhưng có ít nhất một carton nằm lỏng, bấm xác nhận → `Incorrect`; feedback tiếng Đức (*Sie*) nêu quy tắc EKZ 3, không nêu số kiện hay vị trí ô.
- Thiếu số lượng, mọi carton đều khớp, bấm xác nhận → `Incorrect`; lý do là số lượng, không phải EKZ.
- Đặt carton vào rồi lấy ra, khớp hay không khớp, Ist/Rest vẫn hiện theo quy tắc của decision 001 khi chạy lại.
- Một carton đã khớp bị tắt hoặc huỷ thì không còn được tính ở lần xác nhận tiếp theo.
- (code) Logic đếm số lượng không đọc trạng thái khớp; trạng thái khớp chỉ được kiểm khi bấm xác nhận.
- (code) Trạng thái khớp đọc qua event của Interaction SDK, không poll trong `Update`, hủy đăng ký trong `OnDisable`. Không file nào dưới `Assets/Oculus/` hay `Library/PackageCache/` bị sửa.
- (code) Mọi ô snap còn dùng đều nằm trong bounds trigger của `PlasticBox_Order`.
- Ở MixedPickingScene và ComplexScene, dialog hiện `Empf Kz` `000`; đặt lỏng đúng đơn, bấm xác nhận → kết quả như trước khi sửa.
- Mọi chuỗi mới có key + EN + DE trong bảng Localization và đã qua `german-reviewer`.
- Console không có lỗi mới ở cả 4 scene picking.

## Nếu giả định sai
- **D4 = carton đặt lỏng, không xếp:** tắt `BoxSnapper`/`SnapInteractable` ở Basic (bật lại được, không xoá, không đụng `.meta`). Chạy lại `solution-decision` cho câu "luyện EKZ 3 ở bài nào, với chiếc lẻ nào", với `Thay thế: 003`.
- **D6 = chỉ nhắc:** vẫn hiện feedback EKZ khi xác nhận nhưng không đổi `Correct`; chỉ sửa một nhánh trong validator.
- **D5 = vật tư có EKZ khác 0:** đổi số trên `Empf Kz`; nếu là EKZ 4–6 thì ghi rõ là không chấm cách đặt (snap không mô phỏng được lớp ngăn) hoặc đổi vật tư giả.
- **A1 = đơn khác:** đổi Soll và số carton; *Done when* giữ nguyên cấu trúc.

## Hệ quả cho mục khác
- **#1:** bỏ chặn theo giả định A1.
- **#34 / decision 001:** Soll 300 chiếc thuộc nhóm "từ 8 chiếc thì cân" (B7a), nên theo tài liệu Ist tăng dần trong lúc lấy, tức vẫn cho biết lúc nào đủ. Cần chạy lại `solution-decision` từ 001 với đơn này.
- **#10:** thêm `Empf Kz` `003`/`000`, bỏ `EKZ Hinweis` và `Bitte Material umpacken!`.
- **#20:** dạy EKZ 0 và EKZ 3 (dùng ví dụ trên slide 8) trước Basic; câu hỏi nhận biết (ID 3) đặt ở đây.

## Bằng chứng
- `CalcForWMS` đếm theo tag trong `OnTriggerEnter/Exit`, không xét snap → `Assets/Scripts/WS Scripts/CalcForWMS.cs` → đã kiểm
- Bảng EKZ-Vorgabe (cấp 0: hàng rời; cấp 3: từng chiếc, đúng hướng, có thứ tự) → Kommissionierung slide 8 → đã kiểm
- Carton đặt "lose", "EKZ gerecht" → Kommissionierung slide 6; "immer lose" ở slide 9 dưới mục Bühne/M2/KONFEKT → đã kiểm
- Quy tắc xi lanh (Rund-, Kompakt-, Normzylinder, EKZ 4–6) → slide 12–15 → đã kiểm
- 6 `BoxSnapper` có `SnapInteractable`, `SnapArea` tắt → `BasicPickingScene.unity` → đã kiểm
- Dialog mới có `Empf Kz` (3 chữ số), ô `Anweisung`, không có ô EKZ Hinweis → spec dialog WMS (bảo mật) → đã kiểm
- Ô snap ở x 0.57 có nằm trong thùng đơn không; `SnapInteractor` có thuộc carton không; cảm giác snap trên Quest → chưa kiểm
