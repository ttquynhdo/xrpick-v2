# Quyết định 011: Thẻ hướng dẫn lên giá ngang tầm mắt bên trái màn hình, Menu ở bên phải gần tay; thùng đơn tự trượt đi khi xác nhận đúng

- **Backlog:** #35 (gồm #37) + #47
- **Trạng thái:** accepted
- **Ngày:** 2026-11-01
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, data-analyst → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-01):** phương án A + băng lăn K1, **trừ vị trí Menu**: Menu ở bên phải, gần tay, không cần bước xác nhận khi đổi bài (người dùng: Menu và nút đỏ khác nhau rõ, khó bấm nhầm; chưa ai thấy chạm nhầm thật). Không cho thùng rỗng mới trượt vào sau khi thùng đơn đi. Làm #35/#47 trước #30.

## Bài toán
Thẻ hướng dẫn và Menu nằm phẳng ở mép bàn trong cả 5 scene: thẻ cách mắt ~0,9 m, cúi 59–60°, chữ phụ gần như không đọc được; Menu lệch phải 56°, cúi 43°. Ở trạm thật (`old/Orderpicking.png`, chú giải là suy ra, D10) mép bàn trước hai thùng để trống, hai thùng nằm trên băng lăn, và sau khi xác nhận thì thùng đơn được đẩy đi ("ggf. Abschieben", Kommissionierung slide 2). Mục tiêu: bố cục giống trạm thật đủ để chuyển giao, thẻ và Menu đọc và chạm được không mỏi, đơn đúng có khoảnh khắc "xong việc", không thêm tải ngoại lai. Ràng buộc: màn hình chỉ hiện WMS; không thêm cân, bàn đóng gói; nút đỏ và thẻ kết quả giữ chỗ (008, #33); `OrderRule` không đổi.

## Xung đột giữa các đề xuất
| Vấn đề | Một bên | Bên kia | Đánh đổi |
|---|---|---|---|
| Chỗ đặt giá thẻ | ID, UX, data: cạnh màn hình, ngang tầm mắt | unity: cạnh bàn, gần tay | Bớt chú ý phân tán ở bước đọc WMS ↔ dễ với, chữ gần |
| Vị trí Menu | UX, ID: tấm riêng, xa nút đỏ (trái) | Người dùng: bên phải tiện tay, khác nút đỏ rõ ràng | Tránh chạm nhầm (poke) ↔ tầm với tay phải; người dùng chọn bên phải, kiểm bằng ca test trên Quest |
| Giá gần màn hình | unity: dễ bị hiểu là "màn hình hiện hướng dẫn" | ID: được nếu giá là vật tách rời rõ | Giải bằng vị trí ngoài ±15° và khung khác |
| Ai đẩy thùng | requirements: đẩy tay sát tài liệu hơn | ID, UX: tự trượt, giản lược tới khi có D11 | Giống thật ↔ không thêm thao tác phải học |
| Cơ chế trượt | unity: thùng kinematic, trigger là con của thùng | data: tắt trigger, hoặc chỉ mesh trượt | Chỉ mesh trượt thì kiện bị bỏ lại; tắt/bật trigger bắn `ContentsChanged` xoá `Correct` |
| Bấm xác nhận sau khi đã đúng | data: phải có quy tắc | — | `confirm_count` bị phồng |
| Phiên bản bố cục trong log | data: `layout_version` ngay | — | Chưa có code log container/gaze; ghi làm ràng buộc cho #28–#30 |

## Các phương án
| | Đúng nghiệp vụ | Hiệu quả học | Tải nhận thức/UX | Chi phí/rủi ro | Nhất quán | Veto |
|---|---|---|---|---|---|---|
| 0. Chỉ dời Menu | 0 | 0 | − vẫn cúi 59° | ++ | 0 | không đạt Done when |
| **A. Giá ngang tầm mắt ~27° trái màn hình** | + | + (thẻ cạnh WMS) | ++ cúi ~12° (suy ra); − chữ phải to hơn | − prefab cho 5 scene | + | — |
| B. Giá chung cạnh bàn trái | + / − chiếm chỗ cân | 0 (sang WMS ~54°) | + cúi ~29° | + | − | — (phương án lùi) |
| C. A + nội dung thẻ mờ dần | + | ++ | + | − cần #25, người hướng dẫn | + | — (để sau) |
| **K1. Thùng tự trượt** (kinematic, trigger con của thùng) | + giản lược tới D11 | + | + rig đứng yên | − kiểm Rigidbody/Grabbable, chung thiết kế với #41 | + | — |
| K2. Trainee tự đẩy | ++/− tuỳ D11 | 0 | − | −− | 0 | chặn bởi D11 |
| K3. Chỉ mesh trượt | 0 | − | − kiện bị bỏ lại | 0 | − | V2 |

## Phương án được chọn: A (Menu bên phải) + K1
**Bố cục (#35)**
- Một prefab giá thẻ đứng gần thẳng (nghiêng ~15° về phía mắt), tâm khoảng (−0,50, 1,35, 1,0): ~27° bên trái màn hình, ngoài vùng ±15° của 010. Khung và màu khác hẳn `WMS_Monitor`.
- Menu là tấm riêng ở bên phải, trong tầm tay phải, không nằm trên mép bàn trước thùng và không trên đường tay giữa hai thùng. Không thêm bước xác nhận. Có thể cùng prefab nhưng là vật riêng.
- `sceneMenu` nối ở từng instance (như `FacilitatorEndSession`); tên các hàm `Open…` của `SceneMenuController` giữ nguyên.
- Thẻ kết quả (`Result`/`Correct`/`Incorrect`) không thuộc giá. Ở Complex, `Result` hiện là con của `Instruction Table`: tách ra và giữ chỗ cũ cạnh nút đỏ (#33).
- Mỗi vùng nhìn (giá thẻ, `WMS_Monitor`, Menu, hai thùng) có collider riêng, không giao nhau, để #30 đo được (data).
- Làm prefab bằng "Create Prefab" từ object có sẵn; chạy `scene-ref-auditor` trước và sau (giữ tham chiếu `OrderConfirm.incorrectText`, `PiecesOrderValidator.resultCard`).
- Câu cũ "nút đỏ nằm sát thẻ 3" bị bỏ.

**Băng lăn (#47)**
- Bệ con lăn bằng primitive dưới hai thùng, cùng vị trí và độ cao như hiện nay.
- Validator phát một sự kiện đúng duy nhất (dùng chung cho #40 và #28); băng lăn đăng ký trong `OnEnable`, huỷ trong `OnDisable`; chỉ coroutine, không `Update`.
- Thùng đơn kinematic, trigger của counter là con của thùng, kiện đi cùng thùng; tắt `Grabbable` của thùng (nếu có) trong lúc trượt. Thùng trượt theo +z ~2 s, có ease, thấp hơn mép dưới màn hình.
- Sai thì thùng đứng yên. Sau khi đúng, nút đỏ không tính thêm lần xác nhận.
- Không có thùng rỗng mới trượt vào.

- **Được:** thẻ cạnh WMS ở bước đọc WMS; cúi ~12° thay vì 59°; mép bàn trống như trạm thật; Menu tiện tay phải; có bước Abschieben mà không thêm thao tác phải học; counter không đếm lại.
- **Mất / chấp nhận:** chữ trên giá phải to hơn, vị trí phải thử trên Quest; từ thẻ 3 xuống nút đỏ ~60° (lúc xác nhận nhìn thẻ kết quả cạnh nút); tự trượt là giản lược tới D11; Menu gần nút đỏ hơn phương án khuyến nghị (rủi ro chạm nhầm bằng poke, kiểm trên Quest).
- **Bị bác:** giá chung cạnh bàn (phương án lùi nếu A mỏi cổ); Menu bên trái và bước xác nhận đổi bài (người dùng chọn bên phải, chưa thấy chạm nhầm); đẩy tay (chờ D11); chỉ mesh trượt (V2); tắt trigger khi trượt; `layout_version` ngay bây giờ (ràng buộc cho #28–#30); thùng rỗng mới trượt vào (quy tắc bố cục trạm).

## Done when
**#35**
- Ở cả 5 scene, không thẻ hướng dẫn hay Menu nào nằm trên mép bàn giữa trainee và hai thùng.
- Trong ảnh `overview`, thẻ hướng dẫn nằm trên một giá đứng bên trái màn hình, ngoài khoảng ±15° quanh tâm màn hình; khung của giá khác khung màn hình.
- Từ vị trí đứng, ảnh `overview` đọc được chữ phụ của mọi thẻ; góc cúi tới thẻ nhỏ hơn hiện tại (mục tiêu theo `xr-lit-scout`).
- Menu là tấm riêng ở bên phải; đứng tại chỗ (không bước) chạm được mọi mục bằng tay phải, thử trên Quest. Mục của bài hiện tại được làm nổi bằng chữ đậm và ký hiệu, không chỉ bằng màu.
- Trên Quest, làm trọn một bài (lấy hàng, bấm nút đỏ 5 lần) không lần nào vô tình mở một mục Menu.
- Bấm nút đỏ ở Basic và Complex → thẻ kết quả vẫn hiện trong khoảng 0,15 m quanh nút; thẻ kết quả không thuộc prefab giá.
- Đặt hàng từ thùng kho sang thùng đơn, tay không chạm giá hay Menu.
- Mở ConceptScene: giá không che màn hình, bảng chú thích hay bảng giảng của 010.
- (code) Giá thẻ, `WMS_Monitor`, Menu và hai thùng có collider vùng nhìn riêng, không giao nhau. `scene-ref-auditor` trước và sau không mất tham chiếu nào.

**#47**
- Lúc vào scene, hai thùng nằm trên bệ con lăn, cùng vị trí và độ cao như trước.
- Đơn đúng, bấm nút đỏ → thẻ `Correct` hiện trước hoặc cùng lúc thùng bắt đầu trượt, và vẫn còn sau khi thùng đi; thùng trượt ra sau, rời vùng làm việc trong ~2 s.
- Trong lúc thùng trượt, thùng không che dialog trong ảnh `wms`; kiện trong thùng không rơi ra.
- Đơn sai, bấm nút đỏ → thùng đứng yên.
- Sau khi đơn đúng, bấm nút đỏ lần nữa không đổi kết quả và không tính thêm lần xác nhận.
- Bấm Menu hoặc vào lại scene → thùng đơn ở vị trí ban đầu.
- (code) Trigger của counter là con của thùng; trong lúc trượt không có `ContentsChanged` nào; chuyển động bắt đầu từ sự kiện đúng của validator, không poll trong `Update`; `OrderRule` không đổi. Console không có lỗi mới.

## Nếu giả định sai
- **Chạm nhầm Menu trên Quest:** thêm bước xác nhận khi đổi bài, hoặc dời Menu sang trái (chỉ đổi transform). Không đổi phần còn lại.
- **A mỏi cổ hoặc chữ không đọc được:** dời giá sang vị trí B (cạnh bàn trái), chỉ đổi transform của prefab.
- **D11 = đẩy tay:** thay chuyển động tự động bằng thùng đẩy được (K2), sửa Done when của #47; sự kiện đúng giữ nguyên.
- **Concept không đủ chỗ cạnh bảng của 010:** dời giá xa thêm sang trái; không đặt vào ±15°.
- **#41 giữ kiện bằng cách đổi cha:** gộp hai cơ chế thành một thiết kế thùng trước khi code.

## Câu hỏi còn mở
- Người hướng dẫn: D10, D11 (cả "có khi nào thùng ở lại không"), D12 (không chặn).
- Cho #28: nút đỏ ở Basic và Mixed gọi hàm xác nhận hai lần trong `_whenSelect` (scene inventory); chưa rõ lỗi hay chủ ý, ảnh hưởng `confirm_count`.
- Cho #28–#30: khi có log container/gaze, thêm phiên bản bố cục vào header (data); thùng trượt không được sinh `container_exit` giả.

## Bằng chứng
- `Instruction Table` ở (0, 0.85, 0.45) trong cả 5 scene → `.claude/cache/scene-inventory/*.md` → đã kiểm
- `Result` của Complex là con của `Instruction Table` → `ComplexScene.md:41–42` → đã kiểm
- Vùng ±15° và bảng của Concept → `010-wms-fields-teaching.md` → đã kiểm
- #41 cũng đụng thùng đơn → `docs/backlog.md` (#41) → đã kiểm
- Nút đỏ Basic/Mixed gọi xác nhận hai lần → `BasicPickingScene.md:82,84`, `MixedPickingScene.md:73–74` → đã kiểm (synthesizer)
- Mép bàn trống, băng lăn, hướng đẩy → `old/Orderpicking.png` → đã kiểm (requirements, ID, UX; chú giải suy ra)
- "ggf. Abschieben"; abschieben là hành động của người khi thùng đầy → Kommissionierung slide 2–3 và đoạn "Behälter voll" → đã kiểm (requirements)
- `ContentsChanged` xoá kết quả; trigger dùng `OnTriggerEnter/Exit` → `PiecesContainerCounter.cs:56–80`, `CalcForWMS.cs:103–137` → đã kiểm (unity)
- Trigger có là con của `PlasticBox_Order`, thùng có Rigidbody/Grabbable không → chưa kiểm (cần Editor)
- Góc 12° / 27° / 54° → tính toán của xr-ux-reviewer → chưa kiểm (vị trí nút đỏ suy ra)
