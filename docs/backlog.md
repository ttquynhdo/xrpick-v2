# Backlog

Nguồn: `/code-review` toàn bộ `Assets/Scripts` (2026-11-01), đã đối chiếu với code và scene (commit `db4160f`).
Mục #14–#23: review thiết kế khóa học (`instructional-design-reviewer`, cả 5 scene, 2026-11-01, chưa có ảnh Play mode). Các mã A2, B8… là câu hỏi trong `docs/requirements/questions-for-instructor.md`.
Mục #24–#25 và các dòng **Bổ sung (ảnh Play mode)**: review lần 2 có ảnh Play mode của 5 góc nhìn chuẩn ở cả 5 scene (`TestEvidence/<Scene>/views/`, chỉ có trên máy, không commit), 2026-11-01. Đã đối chiếu lại với code/scene.
Mục #26–#31: log dữ liệu nghiên cứu theo từng người học, từ `docs/research-logging-spec.md` mục 8 (2026-11-01). Mục #42: từ quyết định ở spec mục 9 (khoá Menu khi nghiên cứu), thêm khi rà backlog 2026-11-01. Mục #43: bài kiểm tra trong VR, ghi lại để sau khi bàn thiết kế #26 (không có bài kiểm tra trên giấy). Mục #44: nút kết thúc phiên, tách từ #27 theo decision 004.
Mục #32–#40 và các dòng **Bổ sung (review UX)**: review UI/UX theo tải nhận thức (`xr-ux-reviewer`, cả 5 scene, ảnh `TestEvidence/<Scene>/views/`, 2026-11-01). Góc/khoảng cách tính từ mắt (0, 1.6, 0). Vị trí nút đỏ là suy ra từ cấu trúc cha–con, chưa đo trong Play mode.
**Cập nhật 2026-11-01 theo câu trả lời B1–B10, C1–C4, D1–D3** trong file câu hỏi (trả lời của nhóm, chưa phải của người hướng dẫn): mọi scene dùng **dialog mới** (`F6 Bestätigen`); Rest = tồn kho thùng kho, Ist theo cách lấy thật (B6, B7, B7a); dữ liệu giả cùng định dạng thật (B10); #22 ngoài phạm vi (D2); giữ thông tin kiện sau khi thả làm quiz quá dễ (D3). Câu A1–A6 vẫn mở.
**Cập nhật 2026-11-01 (decision 003):** nhóm trả lời A1 (đơn Basic = tổng 300 chiếc) và D4–D7 (EKZ khi đặt hàng); thêm #41, bỏ chặn #1, #24. Sau đó nhóm trả lời nốt A2, A3, A5, A6, B3, B5, D1, chọn phương án C cho #34 (decision 001) và chốt mục 9 của spec log: không còn mục nào `blocked`.
**Quy tắc màn hình (người dùng 2026-11-01, #10):** màn hình máy tính `WMS_Monitor` chỉ hiện WMS (dialog `WMS_Kommissionierung`). Hướng dẫn nằm trên thẻ ở bàn, kết quả xác nhận hiện trên thẻ cạnh nút đỏ (#33), không mục nào ghi hướng dẫn hay kết quả lên màn hình. Basic, Complex đã làm (#10); Mixed làm ở #21; Concept quyết định ở #45; Welcome không cần WMS (tuỳ chọn: đặt màn hình tắt để trạm làm việc giống các bài sau).
**Quy tắc bố cục trạm (người dùng 2026-11-01, so với ảnh trạm thật `old/Orderpicking.png`):** chỉ thêm vào cảnh thứ mà bài tập dùng tới (#35 dời thẻ và Menu khỏi mép bàn, gồm #37; #47 băng lăn, #21 mỗi vị trí một thùng kho; #48 bàn phím đã bỏ vì bị thùng che và chắn đường trượt). **Không thêm** cân, bàn đóng gói, tacker, băng dính, máy in nhãn, kệ carton: không bài nào dùng chúng, thêm vào chỉ tăng tải nhận thức (cùng lý do decision 009). Nếu muốn trạm trông đầy đủ thì chỉ đặt ở xa, ngoài vùng nhìn chính, không cầm được.

Mỗi mục = 1 phiên làm việc = 1 nhánh. Không đánh số lại; mục xong thì ghi `done (commit …)`.

**Trạng thái:** `todo` → `in progress` → `verified-mcp (x/y ✅, chờ test tay)` → `done (commit …, test x/x ✅)`; hoặc `blocked (lý do)`; mục bị bỏ ghi `dropped (lý do)`. Chỉ ghi `done` khi mọi case trong `docs/test-reports/bl<nn>.md` đều ✅, kể cả phần bạn test bằng tay. Các dòng **Bổ sung** được gộp vào **Done when** / **Scene(s)** khi bắt đầu làm mục đó.

Mức ưu tiên: **P0** lỗi khi chạy / đếm sai · **P1** logic huấn luyện sai · **P2** bảo trì, mở đường cho việc khác · **P3** dọn dẹp.

**Công** (ước lượng thô theo số script/scene phải đụng, chưa đo): **S** vài giờ · **M** khoảng 1 ngày · **L** nhiều ngày.
**Test trên kính** (phần bạn phải tự đeo Quest test, MCP không làm được): **không** (MCP/Editor đủ) · **nhanh** (5–10 phút xem thấy/đọc được) · **bắt buộc** (tay thật, tầm với, độ thoải mái, FPS). Gom các mục "nhanh"/"bắt buộc" đã `verified-mcp` lại để test chung một buổi.

**Thứ tự đề xuất (rà lại 2026-11-01, sau khi merge #18 → #17 → #6 → #14 → #8 vào `main`, commit `b64b40f`).** Hai nhánh không được cùng sửa một file `.unity` một lúc. Không còn mục nào bị chặn bởi câu hỏi (nhóm trả lời hết; người hướng dẫn xác nhận sau, các mục làm theo giả định ghi rõ ở Status). #22 và #48 đã bỏ, #5 gộp vào #18, #32 gộp vào #45. **Người dùng chọn 2026-11-01:** các mục log nghiên cứu còn mở (#28–#31, #42, #43) để đợt cuối (Đợt 6).
- **Đã `done`:** Đợt 1 (#1, #2, #3, #4, #7, #9, #15, #16, #24, #26, #34, #38), đợt #27/#44, chuỗi validator #18 → #6 → #14 → #8, và #10, #20, #45 (người dùng báo test tay ✅ cho cả buổi test kính, 2026-11-01).
- **Chỉ còn chờ người hướng dẫn:** #17 (test tay ✅; thành `done` khi người hướng dẫn chốt A2).
- **Buổi test kính tiếp theo (rà lại 2026-11-02, sau khi bạn báo #25, #33, #35, #41, #47 đạt):** #21 H1 làm lại (túi SCHRAUBE B không còn biến mất khi cầm) + H2 (đọc thẻ kết quả, xem đổi thùng), N6 ✅ 2026-11-02, #49 done. Mới: #50 thùng đơn chạy tiếp tới khu Versand ở cửa kho (D11, D15 nhóm đã trả lời), decision 014 (phương án B, 2026-11-02), verified-mcp: test kính H1 (xem cả chuyến, FPS), H2 (kiện đang cầm ở lại).
- **Sau #41 (2026-11-02):** thứ tự cho trạm làm việc là #41 → #35 → #47. #47 cần cả #35 (dời thẻ và Menu khỏi mép bàn) lẫn #41 (cùng sửa thùng đơn). #35 và #47 đã có decision 011, không cần `solution-decision` lại. Người dùng chọn làm #35 ngay sau #41 (2026-11-02).
- **Để sau buổi demo (người dùng 2026-11-01, demo ngày 2026-11-02):** #35, #13, #29, #30, #42, #39, #36, #40, #46, #23 không làm trước demo; xếp lại thứ tự sau demo. Các mục còn mở có thể làm trước demo: #25, #33, #21, #41, #19, #47, #11, #12, #28, #31.
1. **Đợt 3 – nội dung, không kính hoặc kính nhanh** (làm tuần tự vì đụng chung `Instruction Table` / ConceptScene):
   - #45 ConceptScene: dạy Mat.-Nr/VPE, quiz vận dụng (decision 007, gồm #32; test nhanh).
   - #25 mẫu thẻ hướng dẫn chung, tiếng Đức ở cả 5 scene: verified-mcp, chờ đọc trên kính.
   - #33 kết quả cạnh nút đỏ: verified-mcp (đã có sẵn từ #10/#21/#45), chờ test kính H1.
   - #23 tổng kết cuối khoá + mục tiêu Welcome (không kính).
   - #21 Mixed từng position (decision 012): verified-mcp, chờ test tay H1/H2 → #46 dọn dẹp.
   - #39 phản hồi ở WelcomeScene.
   - #11 nền tảng DE/EN: phần Editor làm được trong đợt này; thử ConceptScene trên Quest cùng buổi test.
2. **Đợt 4 – dịch** (không kính, chờ nội dung ổn định): #12 (sau #11, #25, #45) → #13.
3. **Đợt 5 – bắt buộc test kính** (tay thật, tầm với, FPS), gom thành các buổi test riêng:
   - **Chuỗi trạm làm việc** (cùng sửa `Instruction Table`/`Menu`/bàn ở cả 5 scene, nên tuần tự; decision 011 đã chốt bố cục cho #35 và #47; cả chuỗi làm trước #30): #25 nội dung thẻ (đợt 3) → **#35** giá cho thẻ + Menu (gồm #37) → **#40** làm nổi bước hiện tại trên giá mới → **#47** bệ băng lăn, thùng đơn chạy đi (D10, D11). Nút đỏ và thẻ kết quả cạnh nút (#33) giữ chỗ suốt chuỗi.
   - #36 nhãn đọc được từ chỗ đứng (sau #17; độc lập với chuỗi trên).
   - #41 EKZ khi đặt hàng (phần `Empf Kz` sau #10; làm trước #47 vì cùng đụng thùng đơn).
   - #19 Complex hai bước (sau #3, #16, #17).
4. **Đợt 6 – log nghiên cứu (cuối cùng, người dùng chọn 2026-11-01).**
   - #28 (đã gỡ chặn: Basic chấm bằng `OrderRule` từ #14; object kết quả trong `XRPick.Orders`) → #31.
   - #29, #30 (sau #26; bắt buộc test kính: FPS khi bật logger).
   - #42 khoá Menu khi nghiên cứu (sau #26, #27, #18, #35).
   - Chờ quyết định: #43 bài kiểm tra ngắn trong VR (để sau).

---

## P0 – Lỗi khi chạy / đếm sai

### #1 [P0] Thống nhất cách kiểm tra đơn hàng ở BasicPickingScene
- **Type:** logic
- **Scene(s):** BasicPickingScene
- **Files:** Assets/Scripts/PackageCounter.cs:112, Assets/Scripts/WS Scripts/OrderConfirm.cs:24, Assets/Scripts/WS Scripts/CalcForWMS.cs
- **Problem:** Nút xác nhận gọi **cả hai** `PackageCounter.CheckOrder()` (đúng khi có 2 package GG) và `OrderConfirm.OrderConfirmation()` (đúng khi `currentSize == necessarySize`; trong scene `necessarySize = 300`, mỗi `BoxInfo.packageSize = 50`, tức là 6 package). Hai quy tắc này không khớp nhau, nên trainee có thể thấy chữ "Correct!" cùng lúc với object "incorrect" hiện lên. Có thể nó chỉ "trùng khớp" nhờ lỗi đếm trùng ở #2.
- **Done when:** (giả định A1: đơn đúng khi tổng số chiếc = Soll 300, tức 6 carton × 50) 6 carton GG trong thùng, bấm xác nhận → chỉ hiện `Correct`, không có thông báo sai nào. 2 carton trong thùng, bấm xác nhận → chỉ hiện `Incorrect`. (code) Nút xác nhận chỉ còn gọi một quy tắc kiểm tra (`OrderConfirm`/`CalcForWMS`); quy tắc "2 GG" của `PackageCounter` không còn được gọi.
- **Inspector work expected:** yes. Có thể phải bỏ một call khỏi `_whenSelect` của nút, hoặc sửa `necessarySize`/`requiredGGCount`.
- **Công:** M
- **Test trên kính:** không
- **Depends on:** —
- **Ghi chú (2026-11-01):** Tài liệu requirements không nêu con số cụ thể. Về nguyên lý, tài liệu tính Soll và Ist theo **số chiếc**, còn Rest là tồn kho của thùng. Trong scene có 6 GG đang active × 50 = 300 = `necessarySize`, nên có vẻ ý đồ ban đầu là 6 package, còn `requiredGGCount = 2` là phần sót lại. Cần người hướng dẫn chốt con số.
- **Bổ sung (ảnh Play mode):** Đã kiểm tra trong Editor: cả 3 reference của `PackageCounter` (`packagePlacedText`, `orderFeedbackText`, `instructionText`) trỏ vào `Info Panel/Unity Canvas/*`, mà panel này đang **tắt** (Basic dùng `Info Panel (1)`). Nghĩa là trainee không bao giờ thấy feedback của `PackageCounter`. Chỉ có kết quả của `OrderConfirm` (thẻ `Correct`/`Incorrect` trên bàn) là nhìn thấy được.
- **Bổ sung (2026-11-01, đối chiếu cả tài liệu cũ):** Không tài liệu nào có kịch bản đào tạo kèm số liệu, và không có "300 = 6 × 50". Chỉ có vài ảnh dialog thật (Soll nhỏ, một chữ số hoặc hai chữ số; VPE đều là 1) và chu trình 10 bước chuẩn. Đề xuất cho A1: dựng đơn Basic theo một ảnh dialog thật, đổi sang dữ liệu giả cùng định dạng (B10). Nếu Soll ≥ 8 chiếc thì theo tài liệu là lấy bằng cân (B7a), ảnh hưởng #34.
- **Bổ sung (2026-11-01, nhóm trả lời A1):** lấy **tổng số chiếc** làm quy tắc đúng (300 = 6 × 50), bỏ quy tắc "2 GG". Chờ người hướng dẫn xác nhận; nếu khác thì chỉ đổi con số. Xem `docs/decisions/003-ekz-placement.md`.
- **Ghi chú (2026-11-01, khi làm):** Đã bỏ `CheckOrder` khỏi nút (sửa trong Editor, không sửa script). `PackageCounter` vẫn nằm trên `PlasticBox_Order`, vẫn đếm theo tag và ghi vào panel đang tắt; xoá hẳn thuộc #8/#18.
- **Test report:** docs/test-reports/bl01.md
- **Status:** done (commit a866e31, test 9/9 ✅, theo giả định A1)

### #2 [P0] CalcForWMS: đếm theo package, không theo collider
- **Type:** bug
- **Scene(s):** BasicPickingScene
- **Files:** Assets/Scripts/WS Scripts/CalcForWMS.cs:23, :32–46
- **Problem:** Mỗi lần một collider vào hoặc ra, script cộng hoặc trừ `packageSize` một lần mà không có HashSet. Vì vậy package có nhiều collider bị cộng nhiều lần. `other.GetComponent<BoxInfo>()` chỉ tìm trên chính collider và không kiểm tra null, nên collider con sẽ gây NullReferenceException. Ngoài ra `Update` ghi lại 3 text mỗi frame dù giá trị không đổi, gây lãng phí trên Quest.
- **Done when:** Thả 1 package GG vào thùng thì IST tăng đúng 50, lấy ra thì giảm đúng 50. Thả và lấy nhiều lần, giá trị vẫn khớp. Console không có lỗi. (code) Text chỉ được cập nhật khi có thay đổi.
- **Inspector work expected:** no (chỉ cần test)
- **Công:** S
- **Test trên kính:** không
- **Depends on:** #1 chỉ cho con số đích; sửa đếm trùng và NullReferenceException làm được ngay
- **Ghi chú (2026-11-01, khi làm):** Trong scene hiện tại mỗi kiện chỉ có một collider mang tag, nên thả/lấy bình thường không bị đếm trùng. Lỗi chỉ xảy ra khi collider con mang tag (tái hiện bằng tag tạm trong Play mode).
- **Test report:** docs/test-reports/bl02.md
- **Status:** done (commit f496719, test 10/10 ✅)

## P1 – Logic huấn luyện sai

### #3 [P1] ComplexScene: package sai loại phải làm đơn sai
- **Type:** logic
- **Scene(s):** ComplexScene
- **Files:** Assets/Scripts/PiecesContainerCounter.cs:37, :59, :71–85, :104
- **Problem:** Bộ lọc loại đang bị comment out. Mọi `PackageData` được thêm vào set, nhưng chỉ GGA, GGB và LooseItemDisplay được đếm. Scene có một package `VPEFull`: bỏ nó vào thùng cùng 2 GGB và 5 chiếc lẻ thì vẫn được báo "Correct!". Hàm `IsAcceptedType` còn sót lại cũng liệt kê sai loại.
- **Done when:** Có 2 GGB + 5 chiếc lẻ + 1 VPEFull trong thùng (MCP: tạm gắn collider cho `VPE_OpenedPackage`, xem ghi chú) → bấm xác nhận → bị báo sai, feedback có dòng báo vật tư không thuộc đơn. Bỏ VPEFull ra → bấm xác nhận → báo đúng. (code) Mọi kiện có loại khác GGA/GGB/LooseItemDisplay trong thùng đều làm `IsCorrect()` trả về false; không còn bộ lọc bị comment và `IsAcceptedType`.
- **Ghi chú (2026-11-01, khi làm):** `VPE_OpenedPackage` (VPEFull duy nhất) không có collider và `Grabbable`; 13 chiếc lẻ là con của nó. Trainee hiện không bỏ được nó vào thùng, nên lỗi chỉ nằm ở code. Cho cầm VPE thuộc #19.
- **Inspector work expected:** no
- **Công:** S
- **Test trên kính:** không
- **Depends on:** —
- **Test report:** docs/test-reports/bl03.md
- **Status:** done (commit 785e04f, test 9/9 ✅)

### #4 [P1] Xoá trạng thái "thành công" khi kiểm tra lại mà đơn sai
- **Type:** logic
- **Scene(s):** BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** Assets/Scripts/WS Scripts/OrderConfirm.cs, Assets/Scripts/MixPickingValidator.cs, Assets/Scripts/PiecesOrderValidator.cs (+ event "nội dung thùng đổi" trong CalcForWMS, MixPickingContainer, PiecesContainerCounter)
- **Problem:** Trainee làm đúng một lần, sau đó lấy bớt package ra rồi bấm kiểm tra lại. Lúc này feedback chuyển sang đỏ, nhưng `instructionText` vẫn ghi "successfully completed" (BasicPicking), hoặc `congratulationsPanel` vẫn hiện (Complex).
- **Done when:** Ở BasicPickingScene, MixedPickingScene và ComplexScene: sau chuỗi đúng → lấy bớt → bấm nút, màn hình chỉ còn thông báo sai: không còn chữ thành công, không còn thẻ `Correct` (Basic), không còn panel chúc mừng (Complex). Sau một lần bấm nút (đúng hoặc sai), đặt thêm hoặc lấy ra 1 kiện → kết quả cũ biến mất (Basic: cả hai thẻ `Correct`/`Incorrect` tắt; Mixed/Complex: `TXT_Feedback` trống, chúc mừng tắt, hướng dẫn trở về như lúc vào scene), và chỉ hiện lại khi bấm nút. (Đã gộp Bổ sung 2026-11-01; `PackageCounter` không còn nối với nút sau #1.)
- **Inspector work expected:** no
- **Công:** S
- **Test trên kính:** không
- **Depends on:** —
- **Bổ sung (review UX):**
  - Ở Basic, trainee chỉ **thấy** thẻ `Correct`/`Incorrect` của `OrderConfirm` (feedback của `PackageCounter` nằm trên panel đang tắt, xem #1). Phần Basic của mục này phải xét cả hai thẻ đó.
  - Kết quả cũ còn hiện khi nội dung thùng đã đổi, tới lúc bấm lại mới cập nhật. Điều này đúng ở Basic (thẻ `Correct` vẫn bật khi Ist đã khác, Assets/Scripts/WS Scripts/OrderConfirm.cs:22–33), Mixed và Complex (`TXT_Feedback`, `Congratulations`). Thêm câu Done when: *Sau một lần xác nhận (đúng hoặc sai), đặt thêm hoặc lấy ra 1 kiện → kết quả cũ biến mất, chỉ hiện lại khi bấm nút.* Scene: thêm MixedPickingScene.
- **Test report:** docs/test-reports/bl04.md
- **Status:** done (commit 837d4b7, test 15/15 ✅)

### #5 [P1] Menu ở BasicPickingScene thiếu nút sang ConceptScene
- **Type:** bug
- **Scene(s):** BasicPickingScene
- **Files:** Assets/Scenes/BasicPickingScene.unity (menu toggles), Assets/Scripts/SceneMenuController.cs:14
- **Problem:** Các scene khác đều có toggle gọi `OpenConcept`. Trong BasicPickingScene chỉ có `OpenWelcome`, `OpenBasicPicking`, `OpenMixedPicking`, `OpenComplex`. Nếu menu có nút Concept thì nút đó không làm gì.
- **Done when:** Bấm Concept trong menu của BasicPickingScene thì ConceptScene được mở.
- **Inspector work expected:** yes. Thêm call `SceneMenuController.OpenConcept` vào `onValueChanged` của toggle Concept (đây là việc trong Editor, không phải sửa code).
- **Công:** S
- **Test trên kính:** không
- **Depends on:** —
- **Ghi chú (2026-11-01):** Kiểm tra lại scene: menu Basic có 4 mục riêng (`1. Einleitung` → Welcome, `2. EKZ 2` → Basic, `3. EKZ 3` → Mixed, `4. EKZ 8` → Complex) và **không có** toggle Concept nào. Muốn sửa phải thêm hẳn một mục, mà #18 làm lại toàn bộ menu này. Người dùng chọn gộp vào #18.
- **Status:** dropped (gộp vào #18, 2026-11-01)

### #14 [P1] Một mẫu feedback chung: khi sai thì nói lý do và giữ hướng dẫn
- **Type:** logic
- **Scene(s):** BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** Assets/Scripts/MixPickingValidator.cs:38–39, :44–64, Assets/Scripts/PiecesOrderValidator.cs:39–56, BasicPickingScene `Instruction Table/Correct|Incorrect/Secondary Text`
- **Problem:** Khi làm sai, Mixed và Complex xoá `instructionText`, nên trainee mất luôn đề bài. Feedback chỉ liệt kê số đếm, không nói *vì sao* sai (sai loại, thừa, cần hàng lẻ). Basic chỉ ghi "Deine Auswahl war inkorrekt!", không có số nào. Khi đúng, mỗi scene chỉ sang bài tiếp theo một kiểu: Basic ghi "EKZ 8" (không có trong menu), Complex không nói gì.
- **Bổ sung (ảnh Play mode):** Feedback sai hiện tại **lộ đáp án**: Mixed in `GG Connector A: x / 2`, `GG Screw B: x / 2`, `VPE: x / 0` (Assets/Scripts/MixPickingValidator.cs:47–51, :58–62). Complex in `Packages: x / 2`, `Loose pieces: x / 5` (Assets/Scripts/PiecesOrderValidator.cs:50–53). Trainee sai một lần là biết số đúng, không cần tự tính. Ngoài ra chỗ hiện feedback mỗi scene một kiểu: Mixed/Complex/Concept ghi lên panel, Basic dùng thẻ riêng trên bàn.
- **Decision:** docs/decisions/006-validator-result-format.md
- **Done when:** Ở cả 3 scene: làm sai thì hướng dẫn vẫn còn, feedback nêu **loại lỗi** (sai vật tư, thừa, thiếu, thiếu hoặc thừa hàng lẻ; theo A2/A3 không còn "sai loại kiện") và lý do, nhưng **không in số đích**. Làm đúng thì feedback nói đúng ở đâu và tên mục tiếp theo trong menu (Basic → Mixed → Complex → kết thúc). Feedback hiện ở cùng một chỗ trong mọi scene. Thêm (decision 006):
  - Có nhiều lỗi cùng lúc thì màn hình chỉ nêu lỗi đầu tiên theo thứ tự `EMPTY > WRONG_MATERIAL > TOO_FEW/TOO_MANY > hàng lẻ` (lý do + việc cần làm), kèm một dòng chung "còn lỗi khác".
  - ComplexScene: đặt 2 carton GGA thay cho 2 carton Ring D thì màn hình chỉ nêu lý do sai vật tư và việc cần làm, kèm dòng "còn lỗi khác"; không có số đích.
  - ~~BasicPickingScene: thùng có 6 carton TRAINING-MATERIAL B và carton mở Ring D thì bị báo sai vật tư.~~ Không còn làm được: #18 đã xoá Ring D khỏi Basic (người dùng chọn). Thay bằng: BasicPickingScene, 5 carton SCHLAUCHSCHELLE B → thẻ „Nicht richtig“ nêu thiếu và cách tính; 7 carton → nêu thừa; không có số đích.
  - Feedback dùng tên vật tư trên WMS/nhãn, không dùng khoá log hay tên enum.
  - "Feedback hiện ở cùng một chỗ": vị trí thuộc #33 (xem Bổ sung review UX); #14 chỉ lo nội dung.
- **Inspector work expected:** yes, với Basic (text nằm trong scene, không trong code)
- **Công:** M
- **Test trên kính:** không
- **Depends on:** #4 (sửa cùng các dòng), #1 cho phần Basic, #6 (cùng sửa hai validator, làm sau #6), #17 (thuật ngữ cuối cùng cho câu feedback). Định dạng kết quả đã chốt ở decision 006.
- **Bổ sung (review UX):** "Cùng một chỗ" nên là **cạnh nút đỏ**, vì đó là chỗ trainee đang nhìn lúc xác nhận. Basic đã làm vậy (thẻ cách nút khoảng 0.11 m). Concept, Mixed và Complex thì ghi lên panel WMS, lệch 42–51° so với nút. Vị trí do #33 xử lý, mục này chỉ lo nội dung.
- **Ghi chú (2026-11-01, khi làm):** câu feedback (EN cho Mixed/Complex tới #12, DE cho Basic) đã qua german-reviewer, nằm trong `Assets/Scripts/OrderFeedback.cs`, key `order.feedback.*`. Basic giờ cũng chấm bằng `OrderRule` (dòng đơn dựng từ `CalcForWMS`: `materialKey`, `materialName`, `vpe`, Soll = `necessarySize`); điểm chặn #28 ở Basic đã gỡ. Mixed khi đúng chỉ sang „5. Volle Kartons und loses Material“ (lấy từ `LessonNames`); Complex khi đúng để panel Congratulations nói bước tiếp.
- **Test report:** docs/test-reports/bl14.md
- **Status:** done (commit f61133a, test 17/17 ✅)

### #15 [P1] ComplexScene: bộ đếm không được lộ đáp án
- **Type:** logic
- **Scene(s):** ComplexScene
- **Files:** Assets/Scripts/PiecesContainerCounter.cs:95, :100
- **Problem:** Bộ đếm hiện `PACKAGES PLACED: x / 2` và `PIECES PLACED: x / 55`. Trainee không cần tự tính 55 = 2×25 + 5, vì màn hình đã nói cần 2 package. Đây là scene khó nhất nhưng lại được hỗ trợ nhiều hơn Mixed.
- **Done when:** Trong Play mode bộ đếm chỉ hiện số đã đặt (vd. `Ist: 30 ST`), không hiện số package cần. Kết quả đúng/sai không đổi.
- **Inspector work expected:** no
- **Công:** S
- **Test trên kính:** không
- **Depends on:** — (nếu làm #6 trước thì sửa luôn ở đó)
- **Test report:** docs/test-reports/bl15.md
- **Status:** done (commit 785e04f, test 4/4 ✅)

### #16 [P1] ComplexScene: sửa thông tin Ring D bị mâu thuẫn
- **Type:** logic
- **Scene(s):** ComplexScene
- **Files:** ComplexScene `[Info Panel]/TXT_Description`, `Cube (1)/Text (TMP)`, `GGB/[VPE_1..3]/Cube/Text (TMP)`
- **Problem:** Panel WMS ghi `Package type VPE & GG`, `Package size 25 pcs / package`, nhưng VPE trên kệ ghi 20 pcs, GG ghi 25 pcs. Một dòng đơn mà có hai cỡ gói, trainee không biết dùng số nào để tính.
- **Done when:** (giả định A5) Mọi nhãn kiện Ring D và panel WMS đều ghi 25 chiếc/gói; không còn chữ `20 pcs` nào của Ring D trong ComplexScene. 2 carton + 5 chiếc lẻ trong thùng, bấm xác nhận → `Correct`.
- **Inspector work expected:** yes (sửa text trong scene)
- **Công:** S
- **Test trên kính:** không
- **Depends on:** câu trả lời A5, A6 (nhóm đã trả lời 2026-11-01: một cỡ gói 25 chiếc cho Ring D; VPEFull trong thùng = đơn sai)
- **Test report:** docs/test-reports/bl16.md
- **Status:** done (commit 5601da9, test 5/5 ✅, theo giả định A5)

### #17 [P1] Bỏ GG/VPE như "loại kiện": khoá vật tư và một mẫu nhãn trên mọi kiện
- **Type:** logic
- **Scene(s):** BasicPickingScene, MixedPickingScene, ComplexScene, ConceptScene (chỉ 3 kiện), WelcomeScene (câu mục tiêu)
- **Files:** Assets/Scripts/PackageData.cs, Assets/Scripts/PackageType.cs (không xoá giá trị), nhãn TMP trên mọi kiện, BasicPickingScene các package nhãn `VPE … 50 Stück` nhưng đếm theo tag `GGPackage` (Assets/Scripts/WS Scripts/CalcForWMS.cs:41, :92), WelcomeScene "Identify GG and VPE package types"
- **Problem:** ConceptScene dạy GG và VPE là hai *loại kiện*, chỉ VPE được mở. Tài liệu yêu cầu định nghĩa VPE là *số cái trong một thùng/túi* và không nhắc "GG". Ngoài ra tên object, nhãn và cách code đếm không khớp nhau, nên trainee học một quy tắc rồi gặp ngược lại ở bài sau.
- **Decision:** docs/decisions/007-material-labels-and-keys.md (phần quiz/panel ConceptScene tách sang #45; mục Menu sang #18)
- **Done when** (decision 007):
  - Nghĩa GG/VPE đã được người hướng dẫn chốt và ghi vào `docs/i18n/glossary.csv` (dòng `GG` đánh dấu bỏ, thêm dòng `Mat.-Nr` với `do_not_translate=yes`). Trước đó mục này tối đa là `verified-mcp` theo giả định A2.
  - Ở cả 4 scene, mọi carton, túi và chiếc lẻ có `PackageData` với khoá vật tư không rỗng; Console không có lỗi thiếu khoá.
  - Mọi nhãn kiện có ba dòng theo cùng mẫu (tên tiếng Đức, `Mat.-Nr: …`, `VPE: …`); số VPE trên nhãn bằng VPE trong `PackageData` (so bằng scene inventory).
  - Cùng một vật tư có cùng khoá, Mat.-Nr, tên và VPE ở mọi scene; Ring D ghi VPE 25 ở mọi nơi.
  - Không TMP nào trainee nhìn thấy (nhãn, panel, WelcomeScene) còn chữ `GG`, hoặc `VPE` dùng như tên một loại kiện.
  - Cách đếm không đổi: Mixed đơn đúng hiện tại → `Correct`, có carton Ring D trong thùng → sai (như trước); Complex 2 carton Ring D + 5 chiếc lẻ → `Correct`; Basic 6 carton → Ist `300`, `Correct`, Rest không đổi.
- **Inspector work expected:** yes (nhãn, `PackageData` trên mọi kiện)
- **Công:** L
- **Test trên kính:** nhanh (cầm kiện đọc nhãn)
- **Depends on:** #1, #18 (quyết object sót lại ở Basic; thứ tự người dùng chọn 2026-11-01: #18 → #17 → #6). A2, A3 (nhóm đã trả lời 2026-11-01): bỏ GG/VPE như "loại kiện", kiện là carton của một mã vật tư ghi Mat.-Nr và VPE; bỏ nhầm vật tư khác = đơn sai. Nên chạy `solution-decision` trước khi làm (mục L, đụng 4 scene).
- **Bổ sung (2026-11-01, decision 006):** làm **trước #6**. Mỗi carton mang một **khoá vật tư** ngắn do dự án đặt (ví dụ `connector_a`, `screw_b`, `ring_d`, `training_b`), ổn định, không phụ thuộc ngôn ngữ, không hiện cho trainee, tách khỏi Mat.-Nr giả hiển thị trên nhãn. #6 dùng khoá này cho dòng đơn và vật tư lạ, nên không cần bảng tạm `PackageType` → khoá. Không thiết kế tình huống đặt nguyên carton đã mở vào thùng. Nếu người hướng dẫn xác nhận A2 thì xoá mã giữ chỗ `WRONG_TYPE` khỏi enum và `skill_map.csv`. Có thể làm theo giả định A2 (như #1 theo A1); chỉ thành `done` khi người hướng dẫn chốt.
- **Bổ sung (ảnh Play mode):**
  - **Quiz ở Concept ít giá trị kiểm tra:** Khi cầm kiện GG, panel hiện ngay `Rule: GG packages must NOT be opened.` (`[Info Panel]/TXT_Rule`, giá trị `packageRule` của `PackageInfoOnGrab`). Hộp VPE thì đã được mô hình hoá ở trạng thái mở sẵn. Vì vậy câu hỏi "Which package can be opened?" chỉ kiểm tra trainee có nhớ dòng vừa đọc vài giây trước hay không.
  - **Thông tin cho A2:** Tài liệu Detail Description (mục Informationen Kommissionierung) có quy tắc: túi 10 cái gồm hơn 2 thành phần thì không được mở; phải lấy nguyên túi và chỉnh lại Restmenge. Có thể quy tắc "GG không được mở" bắt nguồn từ đây.
- **Bổ sung (2026-11-01, đối chiếu cả tài liệu cũ):** "GG" không có trong tài liệu nào, cũ hay mới. VPE là một **con số** (ô `Verp.-Einh.` trên dialog). Đề xuất cho A2/A3: bỏ cách chia "loại kiện GG/VPE"; kiện được phân biệt bằng mã vật tư (Mat.-Nr) và ghi VPE; lỗi là bỏ nhầm vật tư khác hoặc sai số lượng. Vẫn cần chốt A2/A3.
- **Ghi chú (2026-11-01, khi làm):** tên vật tư giả (german-reviewer): KABELVERBINDER A (`connector_a`, 512304, VPE 50), SCHRAUBE B (`screw_b`, 738190, 20), RING D (`ring_d`, 264857, 25), SCHLAUCHSCHELLE B (`training_b`, 443890, 50), in hoa như trên WMS. Nhãn: không tự xuống dòng, tự thu nhỏ. Feedback Mixed đổi tên vật tư (`RING D: x / 0` thay `VPE: x / 0`). Quiz/panel Concept còn chữ GG/VPE: thuộc #45. Thẻ hướng dẫn Mixed/Complex vẫn ghi "package type" (EN): thuộc #25.
- **Test report:** docs/test-reports/bl17.md
- **Status:** verified-mcp (12/12 ✅ kể cả test tay H1–H2 báo 2026-11-01; chỉ còn chờ người hướng dẫn chốt A2, commit 82a60fb)

### #18 [P1] Đưa BasicPickingScene về cùng chuỗi bài
- **Type:** logic
- **Scene(s):** BasicPickingScene
- **Files:** BasicPickingScene: menu (prefab override `2. EKZ 2`, `3. EKZ 3`, `4. EKZ 8`), `Instruction Table/*` (`1. Lesen des LVS` … `3. Versenden des Auftrags`), các package cũ dưới `Interactables` (`[VPE_OpenedPackage]`, Cable Connector A), panel cũ `[Info Panel]` (CABLE CONNECTOR A 100 pcs)
- **Problem:** Basic được làm lại riêng thành bài "EKZ 3": menu đặt tên theo EKZ, mẫu hướng dẫn khác các scene còn lại, bước 3 ghi "versenden" (gửi đi) thay vì xác nhận, còn sót package và panel của kịch bản cũ. Trainee đi từ Concept sang Basic gặp một giao diện hoàn toàn khác.
- **Done when:** Menu giống hệt các scene khác (tên bài, thứ tự, có Concept). Bấm mục Concept trong menu của BasicPickingScene → ConceptScene được mở (từ #5). Hướng dẫn theo cùng mẫu tiêu đề + 3 bước. Không còn package/panel nào không thuộc đơn hiện tại, trừ khi được đặt có chủ ý làm vật gây nhiễu.
- **Inspector work expected:** yes (chủ yếu sửa scene)
- **Công:** M
- **Test trên kính:** nhanh (bấm menu, xem tổng thể scene)
- **Depends on:** #1 (#5 đã gộp vào đây). D1 (nhóm đã trả lời 2026-11-01): tên bài theo kỹ năng: `Einführung / Grundlagen / Einzelpick nach EKZ 3 / Mehrere Positionen / Volle Kartons + Einzelteile` (tiếng Đức chờ `german-reviewer`). Thêm câu Done when: *Menu ở cả 5 scene hiện đúng 5 tên này theo thứ tự.*
- **Ghi chú:** Cần Play mode kiểm tra package cũ có nhìn thấy và cầm được không. Trong ảnh `overview`/`container` không thấy các package này.
- **Bổ sung (ảnh Play mode):**
  - **Chỉ dẫn bắt làm việc không làm được:** Panel WMS ra lệnh `Bitte Material umpacken!` (`[Info Panel (1)]/Anweisung Visuals/Text (TMP) (1)`) và `EKZ Hinweis: Leerräume auffüllen`, nhưng scene không cho làm hai việc đó. Thẻ hướng dẫn lại bảo "Achte immer auf die Anweisungen", nên trainee học cách bỏ qua chỉ dẫn. Hoặc bỏ hai dòng này, hoặc dạy chúng (#20).
  - **Menu lệch số EKZ:** Mục menu `2. EKZ 2` trong khi WMS hiện `EKZ: 003`.
  - **Gợi ý bài tiếp theo bỏ qua Mixed:** Thẻ `Correct` ghi "nächste Aufgabe (EKZ 8)", tức là Complex, bỏ qua Mixed.
  - **Nhãn nút F12:** Nút đỏ ghi `F12` và thẻ ghi "(F12)". Theo spec dialog, F12 là `Aufkl. korr.` (sửa nhãn), còn xác nhận là `F6 Bestätigen` (suy ra). Cần câu trả lời B8/B9 rồi thống nhất với #10.
- **Bổ sung (2026-11-01, trả lời B8, D1):**
  - B8: dùng dialog mới → nút xác nhận và thẻ hướng dẫn ghi `F6`, không còn `F12`. Thêm câu Done when: *Nút đỏ và thẻ hướng dẫn ghi `F6`; không còn chữ `F12` nào trong scene.*
  - D1: mọi scene sẽ có cùng dialog WMS, nên đơn nào cũng có EKZ; đặt tên bài theo EKZ không phân biệt được các bài. Hướng đề xuất: đặt tên theo **kỹ năng** mỗi bài luyện (vd. đọc dialog → một pick → nhiều vị trí lần lượt → carton nguyên + chiếc lẻ). Cần rà soát lại cả 5 scene (`instructional-design-reviewer`), chốt tên, rồi mới sửa menu ở đây và ở #37. Tên tiếng Đức qua `localize-strings`.
- **Bổ sung (2026-11-01, decision 007):** làm **trước #17**. Mục Menu "GG vs VPE" / "2. Package Types" đổi thành `2. Grundlagen` ở cả 5 scene (D1), không còn chữ "GG" trên Menu. Quyết rõ object sót lại ở Basic (7 carton Connector A, 2 Ring D): xoá, hay giữ có chủ ý làm vật gây nhiễu. Nếu giữ, ghi vào backlog #17 (gắn khoá) và #28 (thêm `BasicPickingScene,check_material`); nếu xoá, #28 không thêm dòng đó.
- **Ghi chú (2026-11-01, khi làm):** người dùng chọn **xoá** các kiện cũ (đang tắt) ở Basic, cùng `PacketTests`, panel WMS cũ `Info Panel` và component `PackageCounter` (code chết trỏ vào panel đó). Tên Menu dùng đúng chuỗi của `LessonNames` (#27: `3. Kommissionieren nach EKZ 3`, `5. Volle Kartons und loses Material`), cỡ chữ 12 ở cả 5 scene để mục dài nhất không bị cắt. D7: ẩn `EKZ Hinweis`, xoá chữ "Bitte Material umpacken!". Chữ hướng dẫn và kết quả của Basic viết lại bằng *Sie* (german-reviewer, key `basic.*`). Mục #25 vẫn lo mẫu thẻ chung cho cả 5 scene.
- **Test report:** docs/test-reports/bl18.md
- **Status:** done (commit 4dad03e, test 24/24 ✅)

### #24 [P1] BasicPickingScene: thêm kiện thừa để phép tính có ý nghĩa
- **Type:** logic
- **Scene(s):** BasicPickingScene
- **Files:** BasicPickingScene `[GG_Package (18)]`…`[GG_Package (23)]` (`BoxInfo.packageSize` 50 ×6), `CalcForWMS.necessarySize` 300
- **Problem:** Thùng kho chỉ có đúng 6 kiện TRAINING-MATERIAL B, và đơn cần đủ cả 6 (300 ÷ 50). Trainee bỏ hết vào thùng là đúng, không phải chọn hay tính gì (ảnh `TestEvidence/BasicPickingScene/views/container.png`). Tải đọc WMS cao nhất khóa học nhưng việc thực hành lại dễ nhất.
- **Done when:** (giả định: thùng kho 9 carton = 450 chiếc) Thùng kho có nhiều kiện hơn số đơn cần. Bỏ hết vào thì bị báo sai, bỏ đúng số tính được thì được báo đúng.
- **Inspector work expected:** yes (nhân bản kiện, sửa `necessarySize` nếu cần)
- **Công:** S
- **Test trên kính:** không
- **Depends on:** #1 (chốt đơn đúng là bao nhiêu; giả định A1 = 300 chiếc, 2026-11-01). Làm trước phần Basic của #41 (decision 003).
- **Ghi chú (2026-11-01, khi làm):** chọn 9 carton (450 chiếc). Sau khi lấy đủ đơn, tồn kho còn 150: đây là giá trị Rest cho #34. Bỏ hết vào thì Rest hiện −150 cho tới khi #34 xong.
- **Test report:** docs/test-reports/bl24.md
- **Status:** done (commit f8410b4, test 7/7 ✅, theo giả định A1, 9 carton)

### #25 [P1] Một mẫu thẻ hướng dẫn chung cho cả 5 scene
- **Type:** logic
- **Scene(s):** WelcomeScene, ConceptScene, BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** các text `Instruction Table/Info Canvas/*` và `Instruction Table/Confirm/*` trong từng scene
- **Problem:** Vị trí và kiểu thẻ đã giống nhau, nhưng nội dung thì không.
  - **Tiêu đề thẻ đầu:** `Welcome!` / `Observe` / `Aufgabe` / `Task`.
  - **Bước tính toán:** nằm ở bước 2 trong Mixed ("Calculate how many…"), nhưng ở bước 1 trong Complex ("1. Read WMS – Determine how many full packages and loose items…").
  - **Động từ cho nút đỏ:** Welcome dạy "Touch … fingertip", Concept "click", Mixed/Complex "press", Basic "drücke".
  - Trainee phải đọc lại cấu trúc mỗi bài.
- **Done when** (người dùng chọn 2026-11-01: tiếng Đức, *Sie*, ở cả 5 scene; bản EN vào `docs/i18n/strings-todo.csv`):
  - Vào Basic, Mixed, Complex: 4 thẻ trên bàn theo cùng mẫu: `Aufgabe`, `1. Angaben im LVS ablesen`, `2. … kommissionieren` (tính và lấy hàng), `3. Auftrag bestätigen`; bước tính toán nằm ở thẻ 2 trong cả ba scene.
  - Vào Welcome và Concept: thẻ đầu cũng là `Aufgabe`.
  - Đọc mọi thẻ của 5 scene: thao tác nút đỏ luôn viết „Drücken Sie … den roten Knopf“, không còn „Touch“, „click“, „press“, „Bestätigen Sie mit …“.
  - Đọc thẻ Mixed và Complex: không còn chữ tiếng Anh, không còn „package type“.
  - Đọc thẻ và panel hướng dẫn của 5 scene: không chữ nào tô đỏ hay xanh lá (hai màu này chỉ dành cho kết quả sai/đúng); Welcome `TXT_NextAction` không xanh lá và không nhỏ hơn các chữ khác của panel.
  - Mixed: 2 GGA + 1 GGB → bấm nút đỏ → feedback tiếng Đức „Nicht richtig“ + „Zu wenig Material SCHRAUBE B. …“; 2 GGA + 2 GGB → bấm → „Richtig! …“ tiếng Đức kèm tên bài tiếp theo.
  - Complex: thùng rỗng → bấm → thẻ `Result` tiếng Đức „Nicht richtig“ + „Der Auftragsbehälter ist leer. …“; 2 GGB + 5 lẻ → bấm → „Richtig! …“ tiếng Đức, panel chúc mừng có tiêu đề tiếng Đức.
  - So ảnh `instructions` của 5 scene: cùng cấu trúc, không chữ nào bị cắt hay tràn khỏi thẻ.
  - (code) Không `Secondary Text` nào của thẻ kết thúc bằng dấu xuống dòng thừa. Mọi chuỗi mới hoặc đổi có key + EN + DE trong `strings-todo.csv` và đã qua `german-reviewer`.
  - Ngoài phạm vi: panel WMS cũ của Mixed (`WMS / ORDER ID`, các dòng đơn, `PACKAGE PLACED`) vẫn tiếng Anh tới #21; mục tiêu trên panel Welcome có thể được viết lại ở #23.
- **Inspector work expected:** yes (text trong scene)
- **Công:** M
- **Test trên kính:** nhanh (đọc thẻ trên kính)
- **Depends on:** #18 cho phần Basic (verified-mcp). Nên làm **trước** #12 để chỉ phải dịch một lần.
- **Bổ sung (2026-11-01, sau #14/#17/#18):** Basic đã dùng *Sie* và mẫu `Aufgabe` + 3 bước (#18, key `basic.*`). Mixed/Complex: thẻ bước 1 vẫn ghi "Identify the package type …" (sai theo A2/#17). Từ #14, khi xác nhận sai thì `TXT_Instruction` giữ nguyên, nên câu hướng dẫn trên panel phải đúng cả lúc có feedback.
- **Bổ sung (review UX):**
  - **Mỗi màu một nghĩa:** vàng = việc cần làm, xanh lá = đúng, đỏ = sai. Hiện chữ "red" trên thẻ được tô đỏ, trùng màu với feedback lỗi. Welcome `[Info Panel]/TXT_NextAction` là chữ xanh lá cỡ 16, trùng màu "đúng", và là chữ nhỏ nhất panel.
  - **Xoá ký tự xuống dòng thừa** cuối `Secondary Text` (Welcome có 7, Concept có 6), để layout không bị đẩy khi dịch.
  - Concept có hai thứ cùng gọi là "instructions": panel `Instruction Panel` và các thẻ trên bàn. Nên đặt tên khác cho panel (xem #32).
- **Test report:** docs/test-reports/bl25.md
- **Status:** done (commit e42b105, test 16/16 ✅, test tay báo 2026-11-02)

## P2 – Bảo trì

### #6 [P2] Một nguồn duy nhất cho số lượng yêu cầu của đơn
- **Type:** refactor
- **Scene(s):** BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** Assets/Scripts/MixPickingValidator.cs:31–64, Assets/Scripts/PiecesContainerCounter.cs:100, :106, Assets/Scripts/PiecesOrderValidator.cs:37, :52–53, Assets/Scripts/PackageCounter.cs:115
- **Problem:** Validator viết cứng các số `2 / 2 / 0`, `GGB==2 && GGA==0 && Loose==5`, "selected 2 GG", và không gọi `IsOrderCorrect()` / `IsCorrect()`. Ai đổi số trong Inspector thì text PACKAGE và validator sẽ lệch nhau.
- **Decision:** docs/decisions/005-order-single-source.md
- **Done when** (theo decision 005):
  - MixedPickingScene: đổi Soll của Cable Connector A từ 100 thành 150 trong Inspector rồi vào Play. Panel WMS hiện `150 pcs`. 3 carton Connector + 2 Screw B thì đúng; 2 + 2 thì sai.
  - MixedPickingScene với đơn mặc định: 2 + 2 thì đúng; 2 + 2 + 1 VPEFull thì sai.
  - ComplexScene: đổi Soll Ring D từ 55 thành 80. Panel WMS hiện `80 pcs`. 3 carton + 5 chiếc lẻ thì đúng; 2 carton + 5 chiếc lẻ thì sai.
  - ComplexScene với đơn mặc định: 2 carton + 5 chiếc lẻ thì đúng; thêm 1 carton GGA thì sai.
  - BasicPickingScene: đổi `necessarySize` từ 300 thành 250 thì Soll hiện `250`, Rest hiện `200`; 5 carton thì đúng, 6 carton thì sai.
  - Grep 4 script Mixed/Complex: không còn literal của đơn (`== 2`, `== 5`, `"/ 2"`, `"/ 5"`, `"2 GG"`).
  - Bộ đếm trong thùng vẫn chỉ hiện số đã đặt (#15); feedback không thêm số đích mới, các số `x / N` đang có lấy N từ nguồn.
  - Mỗi chuỗi mới hoặc đã đổi cho trainee có dòng key + EN + DE trong `docs/i18n/strings-todo.csv`.
  - Console không có lỗi mới.
- **Inspector work expected:** yes. Field mới (Soll chiếc + VPE), không dùng `[FormerlySerializedAs]` cho field kiện cũ vì đổi nghĩa; nhập lại Mixed 100/50, 40/20 và Complex 55/25 rồi lưu scene.
- **Công:** M
- **Test trên kính:** không
- **Depends on:** #1, #3 (cả hai verified-mcp), #17 (khoá vật tư trên carton, decision 006 §6; người dùng chọn làm #17 trước, 2026-11-01). Làm trước #14 (cùng sửa hai validator). Rule đơn và object kết quả đặt trong asmdef `XRPick.Orders` có test EditMode (decision 006). A7, A5, A3/A6 (người hướng dẫn) không chặn: nếu trả lời khác thì chỉ đổi số trong Inspector hoặc rule ở một chỗ.
- **Ghi chú (2026-11-01, rà backlog):** Basic không còn số viết cứng trong logic: `OrderConfirm` so `CalcForWMS.currentSize` với `necessarySize`. `PackageCounter.cs:115` ("selected 2 GG") là code chết sau #1, xoá ở #8/#18.
- **Ghi chú (2026-11-01, khi làm):** dòng đơn có thêm `materialName` (tên vật tư trên nhãn/WMS) để sinh chữ WMS. Carton Ring D đã mở được tính là carton Ring D (TOO_MANY ở Complex, vật tư lạ ở Mixed). Basic chưa đi qua `OrderRule`: việc này chuyển sang đầu #28 (review data-analyst).
- **Test report:** docs/test-reports/bl06.md
- **Status:** done (commit 20ce9e1, test 14/14 ✅)

## P3 – Dọn dẹp

### #7 [P3] Kiểm tra null cho reference Inspector trong validator
- **Type:** cleanup
- **Scene(s):** BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** Assets/Scripts/PackageCounter.cs:119, Assets/Scripts/MixPickingValidator.cs:33–63, Assets/Scripts/PiecesOrderValidator.cs:39–56
- **Problem:** `feedbackText`, `instructionText` và `congratulationsPanel` được dùng mà không kiểm tra null. Hiện tại trong scene chúng **đều đã được gán**, nên chưa gây lỗi. Nhưng nếu sau này quên gán thì lỗi chỉ hiện ra đúng lúc trainee làm đúng.
- **Done when:** Xoá tạm một reference trong Inspector rồi bấm xác nhận: Console báo `LogError` có tên GameObject, không có NullReferenceException. Sau đó gán lại.
- **Inspector work expected:** no
- **Công:** S
- **Test trên kính:** không
- **Depends on:** #4 (sửa cùng các dòng đó)
- **Ghi chú (2026-11-01, khi làm):** Basic giờ dùng `OrderConfirm` (sau #1), nên sửa ở đó thay cho `PackageCounter`. MCP gán null qua code trong Play mode, tương đương xoá ô trong Inspector.
- **Test report:** docs/test-reports/bl07.md
- **Status:** done (commit c0c41bd, test 9/9 ✅)

### #8 [P3] Xoá code chết trong scripts
- **Type:** cleanup
- **Scene(s):** —
- **Files:** Assets/Scripts/SceneMenuController.cs:48–102, Assets/Scripts/PackageInfoDisplay.cs (khối comment), Assets/Scripts/MixPickingContainer.cs:3, Assets/Scripts/MixPickingValidator.cs:87–107, Assets/Scripts/WS Scripts/BoxInfo.cs:6–16, Assets/Scripts/PackageCounter.cs (`CheckOrder` không còn được gọi sau #1) (số dòng rà lại 2026-11-01, sau #4, #7)
- **Problem:** Có các khối code bị comment out, `using System.Xml.Schema` không dùng, hai nhánh if/else giống hệt nhau, và các `Start`/`Update` rỗng. Chúng làm code khó đọc. Còn `MenuSelectionController.cs` đã bị comment toàn bộ và `ScaleItemCounter.cs` không được scene nào dùng: phải quyết định giữ hay xoá cả `.cs` lẫn `.meta`. Trước khi xoá, chạy `scene-ref-auditor`.
- **Done when:** Code compile không lỗi, cả 5 scene chạy như trước, và không có khối code bị comment dài nào. (code) `PackageCounter.CheckOrder` (quy tắc "2 GG") đã bị xoá, hoặc cả component bị gỡ khỏi `PlasticBox_Order` nếu #18 chưa làm việc đó.
- **Ghi chú (2026-11-01, rà backlog):** `OrderConfirm.cs` không còn code chết (đoạn :11–20 cũ giờ là `OnEnable`/`OnDisable` của #4). Xoá hẳn `PackageCounter` (vẫn đếm tag, ghi vào `Info Panel` đang tắt) làm ở đây hoặc ở #18, mục nào làm trước; chạy `scene-ref-auditor` trước khi xoá.
- **Inspector work expected:** no
- **Công:** S
- **Test trên kính:** không
- **Depends on:** #2, #3, #6 (làm sau để tránh xung đột)
- **Ghi chú (2026-11-01, #18):** component `PackageCounter` đã được gỡ khỏi BasicPickingScene, nên `PackageCounter.cs` không còn scene nào dùng: xoá cả file (`.cs` + `.meta`) ở đây. Phần `MixPickingValidator.cs:87–107` và `using System.Xml.Schema` ở `MixPickingContainer.cs:3` đã sửa trong #6 (2026-11-01).
- **Ghi chú (2026-11-01, khi làm):** xoá `PackageCounter.cs` và `MenuSelectionController.cs` (kèm `.meta`); giữ `ScaleItemCounter.cs` (code chạy được, để cho kịch bản cân sau này).
- **Test report:** docs/test-reports/bl08.md
- **Status:** done (commit 201cde6, test 7/7 ✅, không có case test tay)

### #9 [P3] Xoá các UnityEvent call trỏ vào "None"
- **Type:** cleanup
- **Scene(s):** WelcomeScene, ConceptScene, BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** WelcomeScene.unity:556–575 (`MixedPickingValidator.ConfirmOrder`, target trống), các call `PackageInfoDisplay.ShowDefaultInfo` có `m_Target: {fileID: 0}` trong Basic/Mixed/Complex
- **Bổ sung (2026-11-01, từ scene inventory):** Sự kiện cầm package (`ISDK_HandGrabInteraction`) trong MixedPickingScene còn gọi `PackageInfoDisplay.ShowGGInfo` / `ShowVPEInfo`. Hai method này đã bị comment out trong code, và target cũng đang trống. Cần kiểm tra BasicPickingScene và ComplexScene xem có tình trạng tương tự không.
- **Problem:** Các call còn sót lại khi copy prefab. Chúng không làm gì nhưng gây nhầm lẫn khi đọc Inspector.
- **Bổ sung (review UX):** Ở WelcomeScene, nút đỏ chỉ có call trống này. Thẻ 2 bảo "Touch the button", nhưng chạm vào thì không có gì xảy ra. Xoá call thôi chưa đủ, phần phản hồi thuộc #39.
- **Done when:** Trong Inspector không còn dòng event nào có target "None (Object)". Các nút vẫn chạy như trước.
- **Inspector work expected:** yes. Xoá từng dòng event trong Editor.
- **Công:** S
- **Test trên kính:** không
- **Depends on:** —
- **Ghi chú (2026-11-01, khi làm):** quét bằng script trong Editor, xoá 68 call (target None hoặc method rỗng) ở cả 5 scene, kể cả ConceptScene.
- **Test report:** docs/test-reports/bl09.md
- **Status:** done (commit 68e2e15, test 8/8 ✅)

---

## UX – Giảm tải ngoại lai (review `xr-ux-reviewer`)

Mục tiêu: bỏ những gì làm trainee phải tìm, quay đầu, đọc lại hoặc đoán (tải ngoại lai). **Giữ nguyên** phần việc chính (tải nội tại), và hỗ trợ trainee hiểu vì sao (tải germane).

**Không nên đổi (tải nội tại / giống hệ thống thật):**
- Kiện gây nhiễu có chủ ý: VPE trong Mixed, 6 GGA trong Complex. GGA và GGB trông giống nhau, chỉ khác ở nhãn; đó chính là bước kiểm tra vật tư. **Không** tô màu theo vật tư.
- Trainee tự tính số kiện và hàng lẻ (55 = 2×25 + 5). Không hiện sẵn kết quả.
- Bố cục dialog xám của Basic (Auftrags-Nr, Material-Nr, `Pick: 1/1`) và nhãn `50 Stück` trên kiện: giữ vì giống hệ thống thật.
- Câu phản hồi có lý do ở Concept ("GG packages must remain closed…"): giữ nội dung, chỉ đổi chỗ hiển thị.

**Cần chụp thêm** (thêm view vào CLAUDE.md khi chụp lần đầu):
- `confirm`: nhìn từ trên xuống vùng nút đỏ và thẻ 3, chụp sau khi bấm đúng và sau khi bấm sai.
- `shelf`: nhìn thùng kho để đọc nhãn kiện, gợi ý (−0.2, 1.4, 0.3) → (−0.2, 0.8, 0.75).
- `welcome`: canvas `Welcome` ở y 1.63.
- Ngoài ra cần đo world scale của các canvas để tính chiều cao chữ theo độ. Hỏi `xr-lit-scout` về khuyến nghị cỡ chữ và khoảng cách UI của Meta.

### #32 [P1] ConceptScene: phản hồi quiz bị xoá khi cầm/thả kiện
- **Type:** bug
- **Scene(s):** ConceptScene
- **Files:** ConceptScene.unity:1043 (`PackageQuizController.feedbackText`) và :17204 (`PackageInfoDisplay.descriptionText`) cùng trỏ vào một TMP (`fileID 1358751443`, `[Info Panel]/TXT_Description`); `_whenUnselect → ShowDefaultInfo` ở :5031, :8681, :9679; Assets/Scripts/PackageInfoDisplay.cs:68–72
- **Problem:** Phản hồi của quiz và thông tin kiện dùng chung một ô chữ. Bấm nút xong mà cầm rồi thả một kiện thì phản hồi bị ghi đè bằng "Look below to read the instructions…". Thông tin kiện cũng biến mất ngay khi thả, nên trainee phải vừa giữ kiện vừa đọc hai panel ở xa (`Info Panel` và `Sample Description`, lệch nhau 29°). Ngoài ra panel `Sample Formula` để trống khi bắt đầu, tiêu đề panel ("Instruction Panel") trùng tên với thẻ trên bàn, `TXT_Feedback` trống nằm chồng lên vùng mô tả, và các ô đáp án không cho thấy ô nào đang được chọn.
- **Done when:** Chọn VPE và bấm nút → hiện phản hồi đúng. Sau đó cầm rồi thả một kiện → phản hồi vẫn còn. Thả kiện → thông tin của kiện vừa cầm biến mất (giữ như hiện tại, để quiz không quá dễ), còn phản hồi quiz thì không bị ghi đè. Lúc bắt đầu scene, không có panel trống nào. Chọn một đáp án → ô đó được đánh dấu rõ ràng.
- **Inspector work expected:** yes. Tạo TMP riêng cho phản hồi (đặt cạnh nút, xem #33), gán lại `feedbackText`, giữ `ShowDefaultInfo` trong `_whenUnselect` (nó chỉ còn ghi vào ô mô tả), ẩn hoặc gộp `Sample Description`, xoá `TXT_Feedback` thừa.
- **Công:** M
- **Test trên kính:** nhanh (cầm/thả kiện bằng tay)
- **Depends on:** —. Câu D3 (2026-11-01): giữ thông tin kiện sau khi thả làm quiz quá dễ, nên **không** giữ.
- **Bổ sung (2026-11-01, decision 007):** gộp vào **#45** (quiz cũ "Chọn VPE" bị bỏ, nên *Done when* ở trên mất nghĩa). Các ý về ô phản hồi riêng, panel trống, đánh dấu đáp án được chuyển sang #45.
- **Status:** dropped (gộp vào #45, 2026-11-01)

### #45 [P1] ConceptScene: dạy Mat.-Nr và VPE, quiz vận dụng (gồm #32)
- **Type:** feature
- **Scene(s):** ConceptScene
- **Files:** Assets/Scripts/PackageQuizController.cs, Assets/Scripts/PackageInfoOnGrab.cs, Assets/Scripts/PackageInfoDisplay.cs, Assets/Scripts/SampleFormulaDisplay.cs; ConceptScene `[Info Panel]` (`TXT_Title`, `TXT_Description`, `TXT_Rule`), `Sample Description`, `TXT_Feedback`, `Interactables/AnswerOption`; các tham chiếu trùng TMP của #32 (ConceptScene.unity:1043, :17204)
- **Problem:** Concept dạy GG/VPE như hai loại kiện và hỏi "Which package can be opened?" với đáp án VPE. Tài liệu nói việc mở carton phụ thuộc số lượng (Anbruch, Detail Description 5.12), quy tắc cấm mở chỉ cho túi 10 cái hơn 2 thành phần (4.1). Panel hiện sẵn đáp án (`TXT_Rule`) và kết quả phép chia (`SampleFormulaDisplay`), nên quiz chỉ kiểm tra trí nhớ vài giây. Thêm các lỗi của #32: phản hồi quiz và thông tin kiện dùng chung một TMP, panel trống lúc đầu, không thấy đáp án nào đang chọn.
- **Decision:** docs/decisions/007-material-labels-and-keys.md (mục 5)
- **Done when:**
  - Cầm kiện → panel giải thích Mat.-Nr/VPE của kiện đó, không in kết quả phép chia. Thả → thông tin kiện biến mất (D3), phản hồi quiz vẫn còn.
  - Lúc bắt đầu scene, không có panel trống nào; không còn `TXT_Rule` hiện sẵn đáp án.
  - Quiz có ít nhất 2 câu vận dụng: một câu "Soll X, VPE Y: wie viele Kartons?" (chia hết), một câu "welcher Karton gehört zu Mat.-Nr …"; số trong câu hỏi khác số trong ví dụ. Không câu nào hỏi kiện nào được mở.
  - Mỗi câu có ID ổn định, mỗi đáp án có khoá ngắn không phụ thuộc ngôn ngữ (cho #28).
  - Trả lời sai → phản hồi nêu lý do. Chọn một đáp án → ô đó được đánh dấu bằng chữ/viền, không chỉ bằng màu.
  - Mọi chuỗi mới qua `localize-strings` (key + EN + DE, *Sie*); không chữ `GG` nào.
  - Không ghi log mới (để #28).
- **Inspector work expected:** yes (TMP phản hồi riêng, UI câu hỏi mới, gán reference)
- **Công:** M–L
- **Test trên kính:** nhanh (cầm/thả kiện, làm quiz)
- **Depends on:** #17 (dữ liệu và nhãn 3 kiện Concept). Chạy song song được với #6 (khác scene). Câu hỏi mới cho người hướng dẫn: A9, A10, A11 (không chặn).
- **Bổ sung (2026-11-01, #10):** Basic/Complex đã dùng màn hình máy tính `WMS_Monitor` thay TV; Concept chỉ có panel trơn 0,57 × 0,43 m không có màn hình. **Đã quyết ở decision 010 (#20, 2026-11-01): có đặt `WMS_Monitor` chỉ xem trong Concept; #45 và #20 làm chung một nhánh; không đánh dấu gì trên vùng dialog.** Đề xuất ban đầu: đặt `WMS_Monitor` với dialog ở chế độ chỉ xem (một đơn mẫu), nhãn chú thích nằm ngoài màn hình chỉ vào `Mat.-Nr`, `Verp.-Einh.`, `Soll`; lời giảng và quiz trên một bảng riêng bên cạnh, không đè lên màn hình (theo quy tắc màn hình ở đầu file). Đánh đổi cần cân: làm quen màn hình WMS sớm, bố cục giống các bài sau ↔ thêm thứ phải nhìn trong bài lý thuyết.
- **Test report:** docs/test-reports/bl20.md (chung với #20)
- **Status:** done (commit b950df5, test 17/17 ✅, cùng #20)

### #33 [P1] Hiện kết quả xác nhận ngay cạnh nút đỏ
- **Type:** logic
- **Scene(s):** ConceptScene, MixedPickingScene, ComplexScene (Basic đã đúng mẫu: thẻ kết quả cách nút khoảng 0.11 m)
- **Files:** `Instruction Table/Feedback` (Concept, #45), `Instruction Table/Result` (Mixed #21, Complex #10); không cần sửa script
- **Problem:** Lúc bấm nút, mắt trainee đang nhìn xuống bàn (nút ở khoảng 40° phải, 60° xuống). Kết quả lại hiện trên panel WMS (16° xuống), lệch 42° (Mixed), 43° dọc + 34° ngang (Concept) hoặc 51° (Complex). Trainee phải ngẩng đầu để biết mình đúng hay sai, và có thể không thấy kết quả. Đây là split attention.
- **Done when:** Bấm nút đỏ với đơn sai (Concept: đáp án sai) → thẻ kết quả cạnh nút (tâm thẻ cách tâm nút ≤ 0.15 m) hiện „Nicht richtig“ màu đỏ + lý do của lỗi đầu tiên, nhìn thấy trong ảnh `confirm`. Bấm với đơn đúng (Concept: đáp án đúng) → cùng thẻ đó hiện „Richtig!“ màu xanh. Sau mỗi lần bấm, màn hình `WMS_Monitor` không hiện chữ kết quả nào (quy tắc màn hình, #10). Thẻ kết quả ở cả ba scene có cùng nền thẻ với thẻ `Correct`/`Incorrect` của Basic.
- **Ghi chú (2026-11-01, khi làm):** Done when cũ có câu „Chi tiết (nếu cần) vẫn nằm trên WMS“, trái quy tắc màn hình nên đã bỏ. Không phải sửa code hay scene: Complex (#10), Mixed (#21), Concept (#45) đã có thẻ kết quả cạnh nút. Kiểu chữ hơi khác Basic (không có tiêu đề cỡ 14), ghi trong báo cáo, không chặn mục này.
- **Inspector work expected:** no
- **Công:** S (chỉ kiểm tra)
- **Test trên kính:** nhanh (thấy dòng kết quả khi bấm nút)
- **Depends on:** #14 (nội dung feedback), #45 cho Concept (đã gộp #32). Có thể tách mỗi scene một nhánh.
- **Bổ sung (2026-11-01, decision 008):** phần ComplexScene làm trong #10, phần MixedPickingScene làm trong #21. #33 chỉ còn ConceptScene (sau #45).
- **Test report:** docs/test-reports/bl33.md
- **Status:** done (commit 2d69e85, test 9/9 ✅, không đổi code; test tay báo 2026-11-02)

### #34 [P1] BasicPickingScene: Rest đếm ngược làm lộ đáp án
- **Type:** logic
- **Scene(s):** BasicPickingScene
- **Files:** Assets/Scripts/WS Scripts/CalcForWMS.cs:35, :45; BasicPickingScene.unity:11092–11093 (`necessarySize: 300`, `restSize: 300`)
- **Problem:** Rest bắt đầu bằng Soll (300) và giảm 50 mỗi khi đặt một kiện, nên `Rest: 0` báo luôn "đủ rồi". Trainee không phải tự tính 300 ÷ 50, chỉ cần bỏ kiện vào tới khi Rest về 0. Như vậy tải nội tại bị mất, và con số 300 hiện hai lần. Theo tài liệu yêu cầu, Rest là tồn kho còn lại trong thùng (#20), không phải "còn thiếu".
- **Done when:** (decision 001, phương án C; giả định A1 = 300 chiếc, tức là cân) Lúc vào scene: Soll hiện `300`, Ist hiện `0`, Rest hiện tồn kho còn lại của thùng kho sau khi lấy đủ đơn (tồn kho thùng kho − 300, số tồn kho theo #24). Đặt 1 carton vào thùng đơn → Ist tăng đúng 50; lấy ra → Ist giảm đúng 50; Rest không đổi. Đặt đủ 6 carton → Ist `300`; bấm xác nhận → `Correct`. Console không có lỗi mới.
- **Ghi chú (2026-11-01):** người dùng chọn theo tài liệu và chấp nhận rằng Ist cho biết lúc nào đủ; phép tính 300 ÷ 50 không còn được kiểm ở bài Basic. Câu Done when cũ ("không có số nào cho biết còn thiếu") bị bỏ.
- **Inspector work expected:** có thể (giá trị `restSize`, hoặc ẩn text Rest)
- **Công:** S
- **Test trên kính:** không
- **Depends on:** #2 (Ist phải đếm đúng), #1, #24 (số tồn kho thùng kho). B6, B7, B7a đã có câu trả lời của nhóm. Ghi chú (2026-11-01): **Ist cũng làm lộ đáp án** (`Ist = Soll`), không chỉ Rest.
- **Bổ sung (2026-11-01, trả lời B6, B7, B7a):** Nhóm chọn **theo tài liệu**: Rest = tồn kho còn lại trong thùng kho; đếm tay (≤ 7 chiếc) thì Ist điền sẵn bằng Soll; cân (≥ 8 chiếc) thì Ist tăng dần trong lúc lấy. Hướng này gần phương án C của decision 001, nhưng Ist tăng dần vẫn cho biết lúc nào đủ. (Đã xử lý: người dùng chọn thẳng phương án C, không chạy lại `solution-decision`; Done when đã viết lại theo C, xem dòng dưới.)
- **Decision:** docs/decisions/001-basic-wms-ist-rest-leak.md (accepted, phương án C)
- **Bổ sung (2026-11-01, giả định A1 = 300 chiếc):** 300 chiếc thuộc nhóm "từ 8 chiếc thì cân" (B7a), nên theo tài liệu Ist tăng dần trong lúc lấy. Người dùng chọn phương án C (theo tài liệu), decision 001 đã ghi lại.
- **Test report:** docs/test-reports/bl34.md
- **Status:** done (commit b7ee6d0, test 8/8 ✅, theo giả định A1, tồn kho 9 carton)

### #35 [P2] Bố cục trạm: thẻ hướng dẫn và Menu lên một giá riêng, mép bàn chỉ còn thùng và nút đỏ (gồm #37)
- **Type:** logic
- **Scene(s):** cả 5 scene (`Instruction Table` ở (0, 0.85, 0.45) và `Menu` ở khoảng (0.66, 0.86, 0.45) giống nhau trong mọi scene, nhưng **không** phải prefab)
- **Files:** `Instruction Table`, `Menu` trong từng scene; nút đỏ và thẻ kết quả cạnh nút (#33) giữ chỗ
- **Problem:** Gộp ba vấn đề cùng một chỗ trên bàn:
  - **Đọc thẻ (UX):** thẻ nằm phẳng trên bàn, cách mắt khoảng 0.9 m, cúi 59–60°, chữ phụ cỡ 11 (~6.7 mm, suy ra). Ảnh `overview` gần như không đọc được. Mỗi bước phải cúi đọc thẻ rồi ngẩng lên WMS (~43°).
  - **Menu (UX, từ #37):** lệch phải 56°, cúi 43°, cách 1.09 m; ở Welcome bị cắt mép ảnh; không đánh dấu bài đang làm; ở Complex chỉ cách nút đỏ ~0.33 m, chạm nhầm là đổi scene.
  - **Giống trạm thật:** ở trạm thật (`old/Orderpicking.png`) mép trước hai thùng là mép bệ băng lăn, không có giấy tờ; vùng trước mặt chỉ có thùng, màn hình và bàn đạp.
- **Done when:**
  - Không thẻ hướng dẫn hay Menu nào nằm trên mép bàn giữa trainee và hai thùng; cả hai đặt trên một giá riêng cạnh vùng làm việc. Năm scene giống nhau (một prefab).
  - Từ vị trí đứng, ảnh `overview` đọc được chữ phụ của mọi thẻ. Góc cúi tới thẻ nhỏ hơn hiện tại; con số mục tiêu theo khuyến nghị mà `xr-lit-scout` tìm được.
  - Đứng tại chỗ (không bước) chạm được mọi mục Menu, thử trên Quest. Mục của bài hiện tại được làm nổi.
  - Khoảng cách từ nút đỏ tới Menu lớn hơn hiện tại, hoặc chuyển scene cần xác nhận.
  - Nút đỏ (đóng vai bàn đạp, decision 008) và thẻ kết quả cạnh nút (#33) giữ chỗ cũ; bấm nút → kết quả vẫn hiện trong khoảng 0.15 m quanh nút.
  - Đặt hàng từ thùng kho sang thùng đơn không chạm vào giá hay Menu.
- **Inspector work expected:** yes. Biến `Instruction Table` + `Menu` thành một prefab trước, rồi sửa một lần.
- **Công:** L
- **Test trên kính:** bắt buộc (đọc chữ, độ cúi đầu, chạm Menu không bước, tay không vướng)
- **Depends on:** #25 (nội dung thẻ); #18 (tên mục Menu, verified-mcp); đo world scale của canvas. Cần `solution-decision` trước khi làm: chỗ đặt giá (cạnh màn hình ngang tầm mắt, gần WMS nhưng xa nút đỏ ↔ bên cạnh bàn, gần tay) và câu cũ "nút đỏ nằm sát thẻ 3" (bỏ, vì thẻ rời khỏi mép bàn; thẻ 3 chỉ nói nút ở đâu).
- **Decision:** docs/decisions/011-workstation-layout-conveyor.md (giá thẻ ngang tầm mắt ~27° trái màn hình; Menu tấm riêng bên phải, không xác nhận; Done when đầy đủ ở mục *Done when / #35* của 011, thay các câu trên)
- **Ghi chú:** từ #18, Menu dùng tên bài của `LessonNames`, cỡ chữ 12 để mục dài nhất („5. Volle Kartons und loses Material“) không bị cắt; khi làm Menu to hơn thì trả cỡ chữ lên lại.
- **Test report:** docs/test-reports/bl35.md
- **Status:** done (commit 1d4b294, test 18/18 ✅, test tay báo 2026-11-02)

### #36 [P2] Nhãn kiện đọc được từ chỗ đứng
- **Type:** logic
- **Scene(s):** ConceptScene, MixedPickingScene, ComplexScene
- **Files:** `…/Cube/Text (TMP)` của các kiện GG/GGA/GGB (font 0.2), nhãn túi `[PlasticBag_VPE_3]` (rect 0.07×0.13), `GGB/[VPE_n]/Cube/Text (TMP)`
- **Problem:** Trong ảnh `container`, nhãn chỉ là những ô hoặc chấm trắng. GGA và GGB cùng một mô hình, nên nhãn là cách duy nhất để phân biệt vật tư. Ở Concept, nhãn GG là chữ xám mảnh, trong khi nhiệm vụ yêu cầu "Look for … labels". Trainee phải nhấc từng kiện lên mới đọc được.
- **Done when:** Ảnh `shelf` đọc được tên vật tư và số lượng trên mọi kiện trong thùng kho. Nhãn quay về phía trainee, chữ đen đậm. Màu kiện **không** đổi theo vật tư.
- **Inspector work expected:** yes (prefab kiện, TMP nhãn)
- **Công:** M
- **Test trên kính:** nhanh (đọc nhãn từ chỗ đứng)
- **Depends on:** #17 (nội dung nhãn đúng, verified-mcp)
- **Bổ sung (2026-11-01, #17):** nhãn giờ có 3 dòng `TÊN` / `Mat.-Nr: …` / `VPE: n ST`, không tự xuống dòng, tự thu nhỏ chữ (min 30 %). Nhãn túi SCHRAUBE B (Mixed, Concept) và nhãn carton Basic (một mặt bên) rất nhỏ khi nhìn từ chỗ đứng (`TestEvidence/ConceptScene/bl17/L_after_labels_1.png`, `TestEvidence/BasicPickingScene/bl17/L_after_labels.png`). Nhãn vẫn là chữ tĩnh, phải khớp `PackageData` (`testing.md` K2).
- **Status:** todo

### #37 [P2] Menu: trong tầm với, đánh dấu bài đang làm, khó chạm nhầm
- **Type:** logic
- **Scene(s):** cả 5 scene
- **Files:** `Menu` (local (0.34, −0.06, 0.20), world khoảng (0.66, 0.86, 0.45)) trong từng scene
- **Problem:** Menu nằm lệch phải 56°, cúi 43°, cách 1.09 m. Ở Welcome, Menu bị cắt ở mép ảnh `overview`. Không có mục nào được đánh dấu là bài đang làm. Feedback bảo "Select the next scenario option in the MENU" mà không nói mục nào. Ở Complex, Menu chỉ cách nút đỏ khoảng 0.33 m, chạm nhầm là đổi scene ngay.
- **Done when:** Đứng tại chỗ (không bước) chạm được mọi mục, thử trên Quest. Mục của bài hiện tại được làm nổi. Khoảng cách từ nút đỏ tới Menu lớn hơn hiện tại, hoặc chuyển scene cần xác nhận.
- **Inspector work expected:** yes
- **Công:** M
- **Test trên kính:** bắt buộc (chạm mọi mục mà không bước)
- **Depends on:** #18 (tên mục ở Basic, verified-mcp)
- **Bổ sung (2026-11-01, #18):** Menu ở cả 5 scene giờ dùng tên bài của `LessonNames`, cỡ chữ 12 để mục dài nhất („5. Volle Kartons und loses Material“) không bị cắt; mục 2 ở Basic là bản sao của mục Basic (`OpenConcept`). Khi làm Menu to hơn thì trả cỡ chữ lên lại.
- **Status:** dropped (gộp vào #35, 2026-11-01: Menu và thẻ cùng lên một giá; Done when đã chuyển sang #35)

### #38 [P2] ComplexScene: panel chúc mừng chồng lên WMS, ô feedback quá thấp
- **Type:** bug
- **Scene(s):** ComplexScene
- **Files:** `Congratulations` (khoảng (0.00, 1.36, 1.10), scale 0.35, cùng z với `Info Panel`); `[Info Panel]/TXT_Feedback` (rect cao 47.2, font 20); Assets/Scripts/PiecesOrderValidator.cs:50–53 (3 dòng)
- **Problem (suy ra, cần ảnh):** Panel chúc mừng nằm cùng mặt phẳng với WMS, nên có thể chồng lên hoặc nhấp nháy (z-fighting). Feedback sai có 3 dòng trong ô chỉ đủ cao cho khoảng 2 dòng, nên có thể đè lên `TXT_PiPlaced`.
- **Done when:** Ảnh `wms` sau một lần bấm sai: các dòng feedback không đè lên bộ đếm. Feedback dài nhất (có dòng vật tư lạ của #3) hiện đủ mọi dòng bên trong panel (gộp từ Bổ sung). Ảnh sau một lần bấm đúng: panel chúc mừng không chồng và không nhấp nháy với WMS.
- **Inspector work expected:** yes
- **Công:** S
- **Test trên kính:** không
- **Bổ sung (2026-11-01, ảnh từ #3):** Đã thấy trong ảnh `wms`: `Congratulations` che mép trên panel WMS (`TestEvidence/ComplexScene/bl03/T2_after_wms.png`). Feedback sai có 4 dòng thì dòng cuối bị cắt (`T1_after_wms.png`).
- **Ghi chú (2026-11-01, khi làm):** đo tọa độ thì `Congratulations` không chồng WMS (chỉ chạm viền TV; ảnh `wms` chụp từ dưới nên trông như chồng). Lỗi thật là feedback 5 dòng tràn xuống sau viền TV. Đã nới ô feedback + auto-size, nâng panel chúc mừng 2 cm.
- **Test report:** docs/test-reports/bl38.md
- **Depends on:** chụp ảnh trước để xác nhận. Nếu #33 chuyển feedback xuống cạnh nút thì nửa sau của mục này có thể không còn cần.
- **Status:** done (commit 7cd7369, test 6/6 ✅)

### #39 [P2] WelcomeScene: có phản hồi khi cầm thùng và chạm nút
- **Type:** feature
- **Scene(s):** WelcomeScene
- **Files:** WelcomeScene `BigRedButton` (`_whenSelect`, đang trống, xem #9), thùng trong `Interactables Obj`, `Instruction Table` thẻ 1–2, `Menu`
- **Problem:** Welcome dạy hai thao tác (cầm, chạm nút) nhưng cả hai đều không có phản hồi. Trainee không biết mình đã làm đúng chưa, cũng không biết lúc nào "xong" để sang Menu. Mọi chữ hiện ra cùng lúc ngay từ đầu (4 khối).
- **Done when:** Cầm thùng → thẻ 1 có dấu ✓. Chạm nút đỏ → thẻ 2 có dấu ✓ kèm âm thanh. Xong cả hai → Menu (mục tiếp theo) được làm nổi. Nếu prefab nút đã có âm thanh hoặc animation thì giữ nguyên, chỉ thêm dấu ✓.
- **Inspector work expected:** yes
- **Công:** M
- **Test trên kính:** nhanh (cầm thùng, chạm nút, nghe âm thanh)
- **Depends on:** #9; nên làm cùng hoặc sau #27 (bàn phím nhập mã cũng nằm ở Welcome)
- **Status:** todo

### #40 [Feature] Làm nổi bước hiện tại trên thẻ hướng dẫn
- **Type:** feature
- **Scene(s):** BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** `Instruction Table` (thẻ 1–3); script mới theo dõi **thao tác** (đã cầm kiện, thùng có kiện)
- **Problem:** Cả 3–4 thẻ (khoảng 55 từ) hiện cùng lúc với cùng độ nổi bật, nên trainee không biết mình đang ở bước nào (segmenting/signalling).
- **Done when:** Lúc bắt đầu, thẻ 1 nổi bật. Cầm kiện đầu tiên → thẻ 2 nổi bật. Thùng đơn có ít nhất 1 kiện → thẻ 3 nổi bật. Việc chuyển bước **chỉ** dựa vào thao tác, không bao giờ dựa vào đúng hay sai, để không lộ đáp án.
- **Inspector work expected:** yes
- **Công:** L
- **Test trên kính:** bắt buộc (chuyển bước theo thao tác cầm thật)
- **Depends on:** #25, #35 (thẻ đã lên giá mới; làm nổi bước trên giá đó, không làm hai lần)
- **Status:** todo

---

## Feature

### #11 [Feature] Nền tảng đa ngôn ngữ DE/EN và nút chọn ngôn ngữ
- **Type:** feature
- **Scene(s):** tất cả (nút chọn ngôn ngữ đặt ở WelcomeScene)
- **Files:** Packages/manifest.json (`com.unity.localization`), String Table Collection `UI` (locale `de` mặc định, `en`), script lưu lựa chọn ngôn ngữ
- **Problem:** End user là người Đức bản ngữ và không đọc được tiếng Anh, trong khi toàn bộ app hiện là tiếng Anh. Đội phát triển lại cần tiếng Anh để test.
- **Done when:** WelcomeScene có nút **Deutsch / English** cạnh canvas `Welcome`; Menu của các scene không có nút này. Lần chạy đầu (chưa có lựa chọn đã lưu), text mẫu ở mỗi scene hiện tiếng Đức. Bấm `English` ở WelcomeScene thì scene tải lại và text mẫu hiện tiếng Anh; chọn ConceptScene trong Menu thì text mẫu ở đó cũng là tiếng Anh. Thoát rồi mở lại app vẫn là tiếng Anh. ConceptScene (scene thử) chạy được trên Quest với package Localization; ghi lại thời gian build và có nhấp nháy ở frame đầu hay không. Dùng một key không tồn tại thì Console có `LogError` ghi tên key và TMP hiện tên key, không để trống. Một chuỗi runtime có placeholder (bộ đếm ở Concept) hiện đúng số ở cả `de` và `en`.
- **Inspector work expected:** yes (cài package, tạo Localization Settings và locale, gắn `LocalizeStringEvent`)
- **Công:** M
- **Test trên kính:** nhanh (ConceptScene thử package Localization trên Quest: chữ Đức hiện đúng, thời gian build, nhấp nháy frame đầu)
- **Depends on:** —
- **Decision:** docs/decisions/002-localization-mechanism.md
- **Bổ sung (2026-11-01, từ #26):** header của log nghiên cứu có trường `language` (hiện `null`). Trước khi thu dữ liệu thật, `language` phải khác `null` (ghi `DE`/`EN` lúc bắt đầu phiên) và phải có sự kiện `language_change` cho chế độ thử. Ở chế độ nghiên cứu (mã ngoài dải thử) ngôn ngữ khoá ở DE.
- **Status:** todo

### #12 [Feature] Chuyển toàn bộ text hiển thị sang bảng dịch (EN + DE)
- **Type:** refactor
- **Scene(s):** tất cả
- **Files:** khoảng 128 TMP text trong 5 scene; các chuỗi feedback trong `OrderFeedback` (#14), `MixPickingValidator`, `PiecesOrderValidator`, `MixPickingContainer`/`PiecesContainerCounter` (text WMS sinh từ dòng đơn, #6), `PackageQuizController`, `PackageInfoDisplay`, `SampleFormulaDisplay`, `LessonNames`
- **Problem:** Text đang viết cứng bằng tiếng Anh trong code và scene, nên không dịch được.
- **Done when:** Ở chế độ Deutsch không còn chữ tiếng Anh nào, trừ chú thích WMS. Mọi chuỗi tiếng Đức đã qua `german-reviewer`. Có file `docs/i18n/review-status.csv`. Ở `de`, ảnh `wms`, `instructions` và `menu` của cả 5 scene không có chữ tràn khung và không có ô `□`. Nhãn WMS (`do_not_translate=yes`) giống từng ký tự ở cả hai locale. (code) Không còn câu nào ghép từ mảnh (`SampleFormulaDisplay.cs:31–36`, `PiecesOrderValidator.cs:40–42`); số lượng là placeholder.
- **Inspector work expected:** yes (gắn key cho từng TMP text)
- **Công:** L
- **Test trên kính:** không (cần người Đức duyệt bản dịch)
- **Depends on:** #11. #4, #6, #14 đã verified-mcp; nên làm sau #25 để không phải dịch các chuỗi sắp bị sửa. Nút xác nhận là `F6 Bestätigen` (B8, dialog mới, 2026-11-01); theo A2 (2026-11-01) không còn chữ "GG" làm tên loại kiện.
- **Decision:** docs/decisions/002-localization-mechanism.md
- **Bổ sung (2026-11-01, review thiết kế khóa học):** Khi dịch, thống nhất luôn: một thuật ngữ cho LVS/WMS (tài liệu dùng LVS); một tên cho mỗi vật tư (Mixed đang có "Cable Connector A", "GG Connector A", "GG Screw packages"…); đơn vị `ST` trên panel WMS và `Stück` trong câu; Basic đang xưng *du* và có lỗi chính tả "Vervolsständige" → chuyển sang *Sie*. Chuỗi ghép từng mảnh (`SampleFormulaDisplay.cs:31–36`, `PiecesOrderValidator.cs:40–42`) phải thành câu trọn vẹn có placeholder.
- **Status:** todo

### #13 [Feature] File export EN–DE cho người bản ngữ duyệt
- **Type:** feature
- **Scene(s):** —
- **Files:** `docs/i18n/review/` (CSV UTF-8 BOM + hướng dẫn bằng tiếng Đức), theo skill `localize-strings` mục 4–5
- **Problem:** Người hướng dẫn hoặc một người Đức bản ngữ cần duyệt toàn bộ bản dịch trong Excel, rồi gửi lại để cập nhật.
- **Done when:** Tạo được file review mở trong Excel, umlaut hiển thị đúng. Import lại một file mẫu đã điền thì trạng thái chuyển sang `confirmed`. File do Excel tiếng Đức lưu lại (dấu `;`) import được. File có ký tự `�` bị từ chối và có báo lỗi. Một dòng có placeholder khác EN bị báo lỗi và trạng thái giữ nguyên.
- **Inspector work expected:** no
- **Công:** M
- **Test trên kính:** không (người duyệt mở file trong Excel)
- **Depends on:** #12
- **Decision:** docs/decisions/002-localization-mechanism.md
- **Status:** todo

### #10 [Feature] Dựng panel WMS giống dialog Kommissionierung thật
- **Type:** feature
- **Scene(s):** BasicPickingScene trước, sau đó MixedPickingScene và ComplexScene (B8: cả ba dùng dialog mới)
- **Files:** panel WMS hiện có trong scene, `Assets/Scripts/WS Scripts/CalcForWMS.cs`. Spec nằm ở chỗ bảo mật (`docs/requirements/wms-specs/kommissionierung-dialog.md`, không commit)
- **Problem:** Panel WMS hiện tại không giống màn hình thật mà trainee sẽ dùng ở kho, nên bài tập chưa sát thực tế.
- **Bổ sung (ảnh Play mode):** Hiện có 3 kiểu panel trong 3 scene thực hành: Basic là dialog xám tiếng Đức (`Soll/Ist/Rest`, `EKZ`, `VPE: 50`), còn Mixed/Complex là panel đen tiếng Anh (`Order quantity`, `Package type`, `Package size`). Cùng một khái niệm có tên khác nhau giữa các bài (`Soll` ↔ `Order quantity`, `VPE: 50` ↔ `50 pcs / package`). Hơn nữa, màn hình giống thật lại xuất hiện *trước* màn hình đơn giản. Khi làm mục này thì dùng **một mẫu** cho cả 3 scene, với tên trường thật.
- **Bổ sung (review UX):** Ở Mixed, cột giá trị trong `TXT_Description` lệch nhau vì dùng lẫn tab và dấu cách. Khi dựng panel mới, nên dùng TMP riêng cho nhãn và cho giá trị.
- **Bổ sung (2026-11-01, trả lời B1–B10):** Dựng theo dialog mới (B8); màu tương đối, dùng cột "Đề xuất cho UI" của spec (B1); không có số khoanh tròn đỏ (B2); không highlight ô Anweisung (B4); chỉ `F6 Bestätigen` có chức năng, các nút F khác chỉ hiển thị (B9); dữ liệu giả theo bảng định dạng trong spec (B10). B3 (2026-11-01): ô `Empf Kz` có nền màu (006 đỏ, 000 xám, 003 vàng). B5: chỉ vùng dialog.
- **Decision:** docs/decisions/008-wms-dialog-panel.md, docs/decisions/009-wms-indicators-off.md (đèn R2 tắt hết) (phương án C: prefab dialog đầy đủ cho Basic và Complex; Mixed làm cùng #21; dọn field cũ ở #46; nút đỏ = bàn đạp; phần Complex của #33 gộp vào đây)
- **Done when** (decision 008):
  - BasicPickingScene, Play, view `wms_wide` đặt cạnh `slide07_img1.jpg`: panel có đủ R1–R6 theo spec (12 chỉ báo đúng chữ, 14 phím F đúng thứ tự R6, nhãn `Empf Kz:`, `Verp.-Einh.:`, `Mat.-Nr:`, `Bez.:`, `Karton:`, `Gew.:`, `Anweisung` đúng từng ký tự); không có thanh tiêu đề/menu/thanh trạng thái MdiShell; không còn `Auftrags-Nr`, `Pick: 1/1`, `EKZ Hinweis`, `EKZ:`, các thanh xanh trong ô Anweisung.
  - BasicPickingScene lúc vào scene: Soll `300`, Ist `0`, Rest = tồn kho thùng kho − 300, đơn vị `ST`; Mat.-Nr `443890`, Bez. `SCHLAUCHSCHELLE B`; Empf Kz `003` nền vàng; Gew. có dấu phẩy thập phân; Karton và Anweisung trống, không tô; `Modus` `Kommissionierung`; cả 12 chỉ báo R2 tắt (không nền, chữ xám), decision 009.
  - BasicPickingScene: đặt 1 carton → Ist +50; lấy ra → Ist −50; Rest không đổi.
  - BasicPickingScene: 6 carton, bấm nút đỏ → thẻ `Correct`, phím `F6 Bestätigen` hiện trạng thái *pressed* ~0,3 s; 5 carton, bấm → thẻ `Incorrect`, F6 cũng nháy.
  - Poke hoặc ray vào bất kỳ phím F nào → không hover, không kết quả, Console không log mới.
  - Đổi Empf Kz của đơn sang `006` → nền đỏ; `000` → nền xám. Đổi Soll hoặc Mat.-Nr của đơn trong Inspector → dialog hiện số mới. Mat.-Nr của đơn khác `PackageData.MaterialNumber` của carton cùng khoá → Console có một lỗi nêu tên object.
  - Panel ~0,87 × 0,55 m ở chỗ cũ, nghiêng về phía mắt; trong view `overview` mép dưới R5 không bị thùng hay bàn che.
  - (người dùng 2026-11-01) Basic và Complex không còn TV: dialog nằm trong prefab màn hình máy tính `WMS_Monitor` (viền mỏng vừa dialog, cột + đế sau bàn), cùng vị trí (0.06, 1.26, 1.066) và cùng cỡ ở cả hai scene; panel `Congratulations` không chồng lên viền màn hình.
  - ComplexScene lúc vào scene: Soll `55`, Ist `0`, Rest = tồn kho Ring D trong scene − 55; Mat.-Nr `264857`, Bez. `RING D`, Empf Kz `000` nền xám, mọi chỉ báo R2 tắt. Đặt 1 carton → Ist `25`; thêm 1 chiếc lẻ → Ist `26`; Rest không đổi. Panel không còn `Cartons placed`, `Ist: n ST` riêng, câu "Look below…".
  - ComplexScene: 2 carton + 5 chiếc lẻ, bấm nút đỏ → kết quả đúng hiện trên thẻ trong khoảng 0,15 m quanh nút, không hiện trên dialog; 1 carton, bấm → kết quả sai ở cùng chỗ (phần Complex của #33). `Congratulations` không che dialog.
  - Trên Quest, từ chỗ đứng: đọc được Soll, Ist, Rest, Mat.-Nr, Empf Kz và chữ `F6 Bestätigen` ở cả hai scene (test tay).
  - Mọi nhãn mới có key trong `localize-strings`, giống từng ký tự ở `en` và `de`. (code) Không có ảnh hay tài liệu gốc trong `Assets/`; không ghi text trong `Update`. Console không có lỗi mới.
- **Done when (bản cũ, thay bởi trên):** Trong Play mode, đặt panel cạnh ảnh gốc: bố cục, chữ và trạng thái chỉ báo khớp với spec. Panel chỉ có vùng dialog, không có thanh tiêu đề/menu/thanh trạng thái của MdiShell (B5). Ô `Empf Kz` có nền đỏ khi `006`, xám khi `000`, vàng khi `003` (B3). Bấm `F6 Bestätigen` thì đơn được kiểm tra; bấm các nút F khác thì không có gì xảy ra. Rest hiện tồn kho còn lại trong thùng kho, Ist theo cách lấy của đơn (đếm tay: bằng Soll; cân: tăng dần), theo quyết định của #34. Mã vật tư, tên hàng và số liệu là dữ liệu giả, cùng định dạng với spec. Không có ảnh hay tài liệu gốc nào nằm trong `Assets/`.
- **Inspector work expected:** yes (dựng Canvas, gán reference)
- **Công:** L
- **Test trên kính:** nhanh (so bố cục, đọc chữ panel trên kính)
- **Depends on:** #1 (dữ liệu đơn), #34 (cách hiện Ist/Rest). Có thể dựng bố cục trước khi hai mục này xong.
- **Bổ sung (2026-11-01, rà backlog sau #6/#14/#17/#18):** dữ liệu đơn đã có một nguồn: `orderLines` (`OrderLine` {materialKey, materialName, soll, vpe}) trên counter Mixed/Complex, text WMS sinh từ đó lúc Start (`orderLineTexts`); Basic dùng `CalcForWMS.OrderLine` (Soll = `necessarySize`). Panel mới phải đọc từ đó, không gõ số tay. Nhãn panel EN Mixed/Complex còn ghi "Order quantity"/"Package size" (german-reviewer #14: đổi "Package size" thành VPE khi dựng panel). Basic: ô Anweisung vẫn hiện ba thanh xanh đậm (B4: không highlight). Mat.-Nr giả và tên vật tư theo #17 (`docs/i18n/glossary.csv`).
- **Bổ sung (2026-11-01, người dùng):** các đèn chỉ báo R2 (Referenzwaage, Behälterwaage, Zählen, letzter Pick, …) **không dùng trong kịch bản XR training**: không có bài tập nào yêu cầu cân hay đổi cách đếm theo đèn. Thiết kế lại cho hợp lý: giữ bố cục R2 để giống màn hình thật nhưng không để đèn gây chú ý (ví dụ tất cả tắt/xám, bỏ logic `CountingMethod` khỏi hiển thị), và không dạy/chấm theo đèn ở #20. Cần chốt cách làm (solution-decision hoặc sửa decision 008) trước khi đóng #10.
- **Test report:** docs/test-reports/bl10.md
- **Status:** done (commit 0957d2f, test 28/28 ✅)

### #46 [P3] Dọn field text cũ sau khi có dialog WMS chung
- **Type:** cleanup
- **Scene(s):** BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** Assets/Scripts/WS Scripts/CalcForWMS.cs (`istText`, `sollText`, `restText`), Assets/Scripts/MixPickingContainer.cs (`packageCountText`, `orderLineTexts`), Assets/Scripts/PiecesContainerCounter.cs (`piecesPlacedText`, `packagesPlacedText`, `orderLineTexts`)
- **Problem:** Sau #10 và #21, dialog chung tự hiện Soll/Ist/Rest và dòng đơn; các field text cũ trên counter chỉ còn để chuyển tiếp (decision 008 §10).
- **Done when:** (code) Không counter nào còn ghi trực tiếp vào TMP của panel WMS; chỉ view của dialog ghi. Cả 3 scene chạy như trước (regression B1–B5, M1–M5, X1–X5), Console không lỗi. `scene-ref-auditor` không còn tham chiếu tới các field đã gỡ.
- **Inspector work expected:** no (gỡ field; chạy `scene-ref-auditor` trước)
- **Công:** S
- **Test trên kính:** không
- **Depends on:** #10, #21
- **Decision:** docs/decisions/008-wms-dialog-panel.md
- **Status:** todo (sau #10, #21)

### #19 [Feature] ComplexScene: tách thành hai bước, có ví dụ phép chia có dư
- **Type:** feature
- **Scene(s):** ComplexScene
- **Files:** ComplexScene, Assets/Scripts/PiecesContainerCounter.cs, Assets/Scripts/PiecesOrderValidator.cs, Assets/Scripts/OrderContents.cs, Assets/Scripts/PositionStockSwitcher.cs
- **Problem:** Complex thêm 4 thứ mới cùng lúc: phép chia có dư, hàng lẻ, hai dạng gói của một vật tư, cỡ gói không rõ. Chưa scene nào làm mẫu phép chia có dư. Việc mở VPE được dạy ở Concept nhưng chưa bao giờ được thực hành (hàng lẻ đã nằm sẵn).
- **Done when** (viết lại theo decision 016, 2026-11-02; câu cũ: „Bước (a): package nguyên + hàng lẻ của một dạng gói, có ví dụ mẫu trước. Bước (b): thêm dạng gói thứ hai. Trainee tự lấy hàng lẻ từ một VPE (nếu A3 cho phép mở).“):
  - Vào ComplexScene: dialog `WMS_Kommissionierung` hiện RING D, Mat.-Nr 264857, Soll 55, Ist 0. Thẻ trên giá nói trước rằng đơn có hai position, làm lần lượt.
  - Vào ComplexScene: bảng ví dụ bên phải màn hình giải từng bước 64 ÷ 20 = 3 carton nguyên + 4 chiếc lẻ. Không chỗ nào trong scene in sẵn „2 Kartons“ hay „5 Teile“ của đơn thật.
  - Vào ComplexScene: thùng kho của position 1 chưa hiện. Cụm câu hỏi dưới màn hình hỏi số carton nguyên và số chiếc lẻ, mỗi nút ghi đủ đáp án.
  - Chọn đáp án sai cho số carton: câu hỏi báo sai kèm cách tính (chia Soll cho VPE), không nói đáp án. Thùng kho vẫn chưa hiện, chọn lại được.
  - Trả lời đúng cả hai câu: thùng kho của position 1 hiện ra, có carton RING D đóng, một carton RING D đã mở chứa 12 vòng và carton KABELVERBINDER A. Bấm nút đỏ lúc này không chấm câu hỏi.
  - Trên Quest, lấy 5 chiếc vòng từ carton đã mở, từng chiếc một bằng tay: mỗi lần chỉ một chiếc rời ra, chiếc vòng không co nhỏ khi cầm, carton đã mở đứng yên.
  - Mỗi chiếc vòng đặt vào thùng đơn làm Ist tăng đúng 1. Lấy ra thì Ist giảm đúng 1.
  - Ở position 1, đặt 2 carton RING D + 5 chiếc vòng, bấm nút đỏ: thẻ `Result` báo position 1 đúng và nói bước tiếp. Thẻ nhắc phần còn lại trong carton đã mở (Anbruchmenge) để trong Modul ở thùng kho. Dialog chuyển sang SCHLAUCHSCHELLE B, Soll 156, Ist 0. Bảng ví dụ và cụm câu hỏi ẩn đi. Thùng kho đổi sang SCHLAUCHSCHELLE B. Thùng đơn **không** chạy đi.
  - Ở position 1, đặt 3 carton RING D, bấm nút đỏ: thẻ báo lỗi đầu tiên kèm lý do, không có số đích. Dialog vẫn ở position 1.
  - Ở position 1, có một carton KABELVERBINDER A trong thùng đơn, bấm nút đỏ: thẻ báo sai vật tư, vẫn ở position 1.
  - Ở position 2, carton và chiếc vòng RING D của position 1 trong thùng đơn không làm đổi Ist và không bị báo là vật tư lạ.
  - Ở position 2, đặt 3 carton SCHLAUCHSCHELLE B + 6 chiếc lẻ, bấm nút đỏ: thẻ báo cả đơn đúng. `Congratulations` hiện `Code: P<nnn>`. Thùng đơn chạy theo order lane như decision 014.
  - Ở mọi pha, nút đỏ (F6) và thẻ `Instruction Table/Result` ở đúng chỗ cũ. Mọi đèn R2 tắt (009).
  - Ảnh `overview`, `wms`, `instructions` (Play): bảng ví dụ và cụm câu hỏi không che dialog, giá thẻ hay đường tay tới hai thùng. Các nút cách nút đỏ và mép Menu ≥ 15 cm (đo trong Editor, như 015).
  - Menu vẫn có đúng 5 bài, bài 5 vẫn là „5. Volle Kartons und loses Material“.
  - Mọi chuỗi mới hoặc đổi có key + EN + DE qua `localize-strings` và đã qua `german-reviewer`.
  - (code) Đơn chỉ khai báo trong `PiecesContainerCounter.orderLines` (hai dòng). Container lộ số thứ tự position, tổng số position và một event khi đổi position (như 012). `OrderRule` không đổi.
  - (code) `OrderContents.From` không bao giờ tính carton đã mở (`VPEOpenedDisplay`) là carton nguyên. Có test EditMode: 2 carton + 5 chiếc = đúng, 3 carton = TOO_MANY, carton đã mở trong thùng không thành carton nguyên. `OrderContents.StockInScene` (Rest) vẫn đúng.
  - (code) `PiecesOrderValidator.OrderCompleted` chỉ bắn một lần, ở position cuối. Đáp án câu hỏi chia là khoá cố định, không phải chữ tiếng Đức. Console ComplexScene không có lỗi mới.
- **Inspector work expected:** yes
- **Công:** L
- **Test trên kính:** bắt buộc (mở VPE lấy hàng lẻ bằng tay)
- **Depends on:** #3, #16, #17. A3 (2026-11-01): kiện là carton của một mã vật tư; bước "tự lấy hàng lẻ từ carton đã mở" được phép. Không chặn: A7, A12 (túi PU cho ≤ 7 chiếc), A13 (Modul trong thùng kho).
- **Decision:** docs/decisions/016-complex-two-positions-remainder.md
- **Bổ sung (2026-11-01):** Chuyển từ #22 (đã bỏ) sang đây tình huống **Anbruchmenge**: theo Detail Description mục 5.12, khi mở một VPE mà còn dư thì phần dư phải cất vào một Modul. Bước (a) của mục này tạo đúng tình huống đó, nên nên có bước cất phần dư (hoặc ít nhất nhắc đến).
- **Bổ sung (2026-11-01, rà backlog sau #6/#14/#17):** đơn Complex giờ khai báo bằng `orderLines` trên `PiecesContainerCounter` (Soll + VPE, #6) và chấm bằng `OrderRule` (thành phần carton nguyên + chiếc lẻ, A7 còn mở). Ring D = `ring_d`, VPE 25 ở mọi nơi (#17). Carton Ring D đã mở hiện được tính là carton nguyên (TOO_MANY) và không cầm được; nếu bước (b) cho trainee tự lấy hàng lẻ từ carton mở thì cần nhánh riêng trong `OrderContents.From` (review data-analyst #6).
- **Bổ sung (2026-11-01, decision 008):** từ #10, Ist trên dialog tăng dần khi cân, nên phép chia có dư (55 = 2×25 + 5) không còn được chấm qua việc đặt hàng; #19 nên kiểm phép chia trực tiếp (ví dụ hỏi "Wie viele Kartons?" trước khi cầm kiện).
- **Status:** in progress (branch feat/19-complex-two-steps, 2026-11-02; decision 016 accepted)

### #20 [Feature] Dạy Soll/Ist/Rest, EKZ, Anweisung trước khi dùng
- **Type:** feature
- **Scene(s):** ConceptScene hoặc đầu BasicPickingScene
- **Files:** BasicPickingScene `[Info Panel (1)]/Panel/*` (Soll, Ist, Rest, EKZ, EKZ Hinweis, Anweisung)
- **Problem:** Basic hiện Soll/Ist/Rest, EKZ và Anweisung và bảo trainee "Achte immer auf … EKZ-Hinweise!", nhưng không scene nào giải thích chúng, và bài tập không phụ thuộc vào chúng. Ngoài ra app dùng Rest là "còn thiếu", còn tài liệu nói Rest là tồn kho còn lại trong thùng.
- **Decision:** docs/decisions/010-wms-fields-teaching.md (phương án C: dạy trong Concept trên `WMS_Monitor` chỉ xem, chú thích ngoài viền màn hình; Basic chỉ sửa thẻ 1; làm chung nhánh với #45)
- **Done when:** theo mục *Done when* của decision 010 (thay câu cũ "…hoặc trường nào chưa dùng thì ẩn đi": ẩn trường đã bị 008 bác).
- **Inspector work expected:** yes
- **Công:** M
- **Test trên kính:** nhanh (đọc bước dạy trên kính)
- **Depends on:** #10; câu trả lời B3, B4, B6 (đã có); B16 (mới, không chặn). Làm chung nhánh với #45.
- **Bổ sung (2026-11-01):** B6 đã chốt: Rest = tồn kho còn lại trong thùng kho. B4: ô Anweisung không highlight, nên phần dạy Anweisung chỉ dạy nội dung chữ. B3 (màu ô Empf Kz) còn mở, không chặn phần dạy nghĩa của EKZ.
- **Bổ sung (2026-11-01):** đèn chỉ báo R2 không dùng trong kịch bản XR training (xem #10) → không dạy nghĩa từng đèn, chỉ nói ngắn "vùng này không dùng trong bài tập".
- **Test report:** docs/test-reports/bl20.md
- **Status:** done (commit b950df5, test 17/17 ✅, theo giả định B16 `Empf Kz` = EKZ; cùng #45)

### #21 [Feature] MixedPickingScene: nhặt tuần tự từng vị trí như dialog thật
- **Type:** feature
- **Scene(s):** MixedPickingScene
- **Files:** MixedPickingScene `[Info Panel]/TXT_Description`, `TXT_Description (1)`, Assets/Scripts/MixPickingContainer.cs, Assets/Scripts/MixPickingValidator.cs
- **Problem:** Dialog thật xử lý từng vị trí một (`Pick 1/2`, `2/2`). Mixed hiện hai dòng đơn cạnh nhau trên một panel, và không có bộ đếm riêng cho từng vật tư, nên lỗi chỉ lộ ra khi xác nhận.
- **Done when:** (theo decision 012; đã gộp các dòng Bổ sung, câu bật đèn `Mehrfachpick`/`letzter Pick` bị thay vì trái decision 009) Vào MixedPickingScene: dialog `WMS_Kommissionierung` trong `WMS_Monitor` ở (0.06, 1.26, 1.066), cùng cỡ như Basic/Complex, chỉ hiện KABELVERBINDER A, Soll 100, Ist 0; không còn TV, `Info Panel` hay `PACKAGE PLACED`. Vào scene: thùng kho chỉ chứa carton KABELVERBINDER A; không có túi SCHRAUBE B hay hộp RING D trong scene. Đặt 2 carton KABELVERBINDER A, bấm nút đỏ → thẻ cạnh nút báo position 1 đúng và nói bước tiếp bằng chữ; dialog chuyển sang SCHRAUBE B, Soll 40, Ist 0; thùng kho cũ mờ đi, thùng chỉ chứa túi SCHRAUBE B hiện ra đúng chỗ đó. Ở position 1, bấm nút đỏ với 1 carton → thẻ báo lỗi đầu tiên kèm lý do; dialog vẫn ở position 1, thùng kho không đổi. Ở position 2, các carton KABELVERBINDER A đã nằm trong thùng đơn không làm đổi Ist và không bị báo là vật tư lạ. Ở position 2, bấm nút đỏ với số túi sai → thẻ báo lỗi, dialog vẫn ở position 2. Ở position 2, đặt 2 túi SCHRAUBE B, bấm nút đỏ → thẻ báo cả đơn đúng. Ở mọi position, tất cả đèn R2 (gồm `Mehrfachpick`, `letzter Pick`) tắt. Thẻ hướng dẫn trên bàn nói trước khi bắt đầu rằng đơn có hai position, làm lần lượt. (code) Container có số thứ tự position, tổng số position và một sự kiện khi đổi position; `OrderRule` không đổi; order chỉ khai báo trong `orderLines`. Console không có lỗi mới.
- **Inspector work expected:** yes
- **Công:** L
- **Test trên kính:** nhanh (làm lần lượt 2 vị trí)
- **Depends on:** #10; câu trả lời B8 (đã có, 2026-11-01: dialog mới). Đơn của Mixed theo A5 (nhóm, 2026-11-01): giữ số hiện tại. Nên commit #25 trước (cùng sửa MixedPickingScene và `MixPickingValidator.cs`). Câu hỏi mở: B13, B14, D11, D12, D13, D14.
- **Decision:** docs/decisions/012-mixed-sequential-positions.md (người dùng 2026-11-01: phương án A; nếu tới demo 2026-11-02 phần đổi thùng chưa ổn thì demo bản C, #21 chưa `done`; bỏ hẳn RING D)
- **Bổ sung (2026-11-01, #6):** hai vị trí đã là hai `OrderLine` trên `MixedPickingContainer`; mỗi dòng có `PlacedFull` riêng trong `OrderResult`, nên Ist theo từng vị trí lấy được từ đó.
- **Bổ sung (2026-11-01, decision 008):** Mixed dựng dialog chung của #10 ở đây. Thêm Done when: MixedPickingScene dùng cùng prefab dialog, mỗi lần một vị trí; `Mehrfachpick` bật ở cả hai vị trí, `letzter Pick` chỉ bật ở vị trí 2; mỗi vị trí có Ist riêng; không còn `PACKAGE PLACED`; kết quả hiện trên thẻ cạnh nút đỏ (phần Mixed của #33). Câu hỏi B14 (Mehrfachpick "1/2"), B15 (`F4 Übersicht`).
- **Bổ sung (2026-11-01, ảnh trạm thật `old/Orderpicking.png`):** ở trạm thật mỗi vị trí đi kèm một thùng kho riêng; thùng kho của Mixed hiện chứa cả CABLE CONNECTOR A và SCREW B (`TestEvidence/MixedPickingScene/views/overview.png`). Thêm Done when: lúc vào scene, thùng kho chỉ chứa vật tư của vị trí 1; sau khi xác nhận vị trí 1, thùng kho đó được thay bằng thùng kho chỉ chứa vật tư của vị trí 2. Cách đổi thùng (tự chạy hay đẩy tay) theo D11; giả định một thùng kho = một mã vật tư theo D12.
- **Test report:** docs/test-reports/bl21.md
- **Status:** verified-mcp (17/19 ✅; H1 lần 1 ❌ 2026-11-02 túi SCHRAUBE B biến mất khi cầm → đã sửa, chờ test lại H1 và H2 trên kính; đổi thùng bằng thu nhỏ/phóng to thay cho mờ dần, xem báo cáo)

### #47 [Feature] Bệ băng lăn cho hai thùng, thùng đơn chạy đi sau khi xác nhận đúng
- **Type:** feature
- **Scene(s):** BasicPickingScene, ComplexScene (MixedPickingScene sau #21)
- **Files:** bàn làm việc trong từng scene (`PlasticBox_Order` và thùng kho), validator `OrderConfirm`, `PiecesOrderValidator` (sự kiện kết quả đúng); script mới nhỏ cho chuyển động thùng
- **Problem:** Ở trạm thật (`old/Orderpicking.png`, số 1, 6, 10), thùng kho và thùng đơn nằm trên băng lăn, và sau khi xác nhận thì thùng đơn được đẩy đi (Abschieben, Kommissionierung slide 2). App đặt hai thùng trên bàn phẳng, đơn đúng thì chỉ có thẻ chữ, không có dấu hiệu "xong việc" giống thật.
- **Done when:** Lúc vào scene, hai thùng nằm trên bệ con lăn, cùng vị trí và độ cao với hiện tại (tầm với không đổi). Đơn đúng, bấm nút đỏ → thẻ `Correct` hiện như trước, và thùng đơn trượt ra sau theo băng lăn, rời vùng làm việc trong khoảng 2 s. Đơn sai, bấm nút đỏ → thùng đơn đứng yên. Trong lúc thùng đang trượt, kiện trong thùng không rơi ra và không được đếm lại. Bấm Menu hoặc vào lại scene → thùng đơn ở vị trí ban đầu. (code) Chuyển động bắt đầu từ sự kiện kết quả đúng của validator, không poll trong `Update`; không đổi `OrderRule`. Console không có lỗi mới.
- **Inspector work expected:** yes (dựng bệ bằng primitive, gán thùng và validator)
- **Công:** M
- **Test trên kính:** nhanh (xem thùng chạy đi, kiện không rơi)
- **Depends on:** #35 (dời thẻ và Menu khỏi mép bàn trước), #41 (cùng đụng thùng đơn); D11 (tự chạy hay đẩy tay: nếu đẩy tay thì Done when đổi thành trainee đẩy thùng); D10 (xác nhận đọc ảnh). Làm trước #30.
- **Decision:** docs/decisions/011-workstation-layout-conveyor.md (thùng tự trượt, kinematic, trigger là con của thùng; không có thùng rỗng mới; Done when đầy đủ ở mục *Done when / #47* của 011, thay các câu trên)
- **Test report:** docs/test-reports/bl47.md
- **Status:** done (commit 1e26762, test 10/10 ✅, test tay báo 2026-11-02)

### #48 [P3] Bàn phím tĩnh dưới màn hình WMS
- **Type:** cleanup
- **Scene(s):** BasicPickingScene, ComplexScene (Mixed qua #21, Concept qua #20/#45 — cùng prefab)
- **Files:** Assets/Prefabs/WMS_Monitor.prefab
- **Problem:** Ở trạm thật có bàn phím ngay dưới màn hình (`old/Orderpicking.png`, số 9). Trong app các phím F chỉ có trên màn hình, trông như nút bấm trên màn hình cảm ứng.
- **Done when:** Ảnh `overview` ở Basic và Complex thấy một bàn phím phẳng, tối màu ngay dưới màn hình, không có chữ, không cầm hay bấm được (không collider, không `Grabbable`). Bàn phím không che dialog trong ảnh `wms`/`wms_wide` và không chắn tay khi đặt hàng vào hai thùng. (code) Bàn phím là child của prefab `WMS_Monitor`, không thêm script.
- **Inspector work expected:** yes (primitive trong prefab)
- **Công:** S
- **Test trên kính:** không
- **Depends on:** #10 (prefab `WMS_Monitor`)
- **Ghi chú (2026-11-01, đặt thử trong ComplexScene):** mặt bàn y 0,771, hai thùng cao tới y 0,968, mép dưới màn hình ≈ y 0,98, nên không có chỗ ngay dưới màn hình. Đặt giữa sau hai thùng thì ảnh `overview` chỉ thấy một khe giữa hai thùng, và chỗ đó nằm trên đường trượt của thùng đơn (#47, decision 011). Đặt trước thùng thì vào mép bàn, mà decision 011 giữ mép bàn trống.
- **Status:** dropped (người dùng 2026-11-01: bàn phím bị thùng che và chắn đường trượt của #47; P3, bài tập không dùng tới)


### #49 [Feature] Băng lăn kéo dài qua kho, có trạm khác; sơ đồ dòng thùng ở ConceptScene
- **Type:** feature
- **Scene(s):** BasicPickingScene, MixedPickingScene, ComplexScene, ConceptScene
- **Files:** prefab mới cho mạng băng (mesh gộp) và khung trạm trống; `WmsFieldTour` (bước mới trong scene ConceptScene); `docs/i18n/strings-todo.csv`
- **Problem:** Băng lăn chỉ là một đoạn ngắn dưới bàn sau, nên trainee không thấy thùng kho đến từ đâu, thùng đơn đi đâu, và trạm của mình nằm trong hệ thống nào (người dùng 2026-11-02: băng thật chạy quanh kho, qua các dãy kệ, dọc đường có trạm khác).
- **Done when:** xem mục *Done when* của `docs/decisions/013-warehouse-conveyor-network.md`.
- **Inspector work expected:** yes (đặt prefab vào 3 scene, thêm bước tour ở Concept)
- **Công:** M
- **Test trên kính:** bắt buộc (FPS bằng OVR Metrics, nhìn trái/phải 45°)
- **Depends on:** #47 (nhánh `feat/47-conveyor`); D15, D16, D17 (không chặn, chỉ để chỉnh sau)
- **Decision:** docs/decisions/013-warehouse-conveyor-network.md
- **Test report:** docs/test-reports/bl49.md
- **Status:** done (commit 1e80ba4, test 10/10 ✅, test tay báo 2026-11-02)
### #50 [Feature] Thùng đơn chạy tiếp theo làn đơn cho tới khi khuất (phương án C của decision 013)
- **Type:** feature
- **Scene(s):** BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** Assets/Scripts/OrderBoxConveyor.cs, Assets/Prefabs/WarehouseConveyorNetwork.prefab, docs/decisions/011-workstation-layout-conveyor.md, docs/decisions/013-warehouse-conveyor-network.md
- **Problem:** Test kính #49 H1 (người dùng 2026-11-02): tuyến băng dài hợp lý và không gây phân tán, nhưng khi thấy thùng đơn trượt đi thì trainee tò mò muốn xem nó đi đâu tiếp. Hiện thùng chỉ trượt ~0,9 m rồi đứng yên (011 K1), nên tuyến dài trông như không dẫn tới đâu. Decision 013 đã xét đúng phương án này (C: chuyển động ở ranh giới nhiệm vụ) và hoãn lại cho tới khi có trả lời D11.
- **Done when:** theo mục *Done when* của decision 014: đơn đúng → thùng đơn chạy theo làn đơn qua góc z ≈ 3 và x ≈ 5 (dừng ~1 s ở mỗi góc, không xoay) rồi dừng xếp hàng sau thùng của trạm trống ở x 5; kiện, snap đi cùng, kiện đang cầm ở lại; Mixed position 1 và đơn sai → không chạy; thẻ kết quả, Menu dùng được trong lúc chạy; Concept không đổi; FPS trên Quest giữ như sau #49; (code) không `ContentsChanged`, Ist không đổi, không tắt/xoá kiện, không log mới.
- **Inspector work expected:** có thể (đường đi của thùng theo tuyến của `WarehouseConveyorNetwork`)
- **Công:** M
- **Test trên kính:** bắt buộc (chuyển động ngoài vùng nhìn chính có hút mắt hay gây khó chịu không, FPS)
- **Depends on:** #47, #49; D11 (thùng đơn tự chạy đi hay người đẩy); sửa decision 011 (K1: thùng đứng yên)
- **Bổ sung (người dùng trả lời D11, D15, 2026-11-02, chờ người hướng dẫn xác nhận):** thùng đơn tự chạy trên băng (không ai đẩy) tới khu giao hàng ở cửa kho (Versand) và dừng lần lượt thành hàng chờ người giao hàng tới lấy. Thùng kho đến từ đâu thì chưa chắc (D17), nên #50 không đụng tới thùng kho. Câu „nơi thùng biến mất không được đặt tên“ trong Done when hết hiệu lực nếu `solution-decision` chọn hiện khu Versand.
- **Decision:** docs/decisions/014-order-box-rides-order-lane.md
- **Test report:** docs/test-reports/bl50.md
- **Status:** verified-mcp (12/14 ✅, chờ test tay H1 xem cả chuyến + FPS, H2 kiện đang cầm ở lại; theo giả định D11, D15)

### #51 [P2] Bàn sau đỡ màn hình đứng trên bàn làm việc, không chân chạm sàn
- **Type:** cleanup
- **Scene(s):** ConceptScene, BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** Assets/Prefabs/RearBench.prefab
- **Problem:** Người dùng báo trên kính (2026-11-02, ảnh ConceptScene): bàn sau đỡ màn hình WMS có hai chân sau dài chạm xuống sàn, trông kỳ so với trạm thật, nơi giá màn hình đứng trên bàn làm việc. Nguyên nhân: mặt bàn làm việc (`Desk`, prefab mẫu của SDK) chỉ kéo tới z 1,10, còn chân sau của `RearBench` ở z 1,48.
- **Done when:** Ở cả 4 scene, ảnh nhìn ngang cho thấy cả 4 chân của bàn sau đứng trên mặt bàn làm việc (đáy chân ≈ y 0,77, trong vùng z 0,30–1,10), không chân nào chạm sàn. Màn hình WMS vẫn ở chỗ cũ, đế màn hình nằm trọn trên mặt bàn sau. Ở Basic, Mixed, Complex, thùng kho và thùng đơn vẫn nằm dưới bàn sau mà không chạm chân hay mặt bàn; thùng đơn vẫn chạy hết tuyến của #50.
- **Inspector work expected:** no (sửa prefab bằng code)
- **Công:** S
- **Test trên kính:** nhanh (nhìn bàn sau và màn hình từ chỗ đứng)
- **Depends on:** #47, #50
- **Test report:** docs/test-reports/bl51.md
- **Status:** verified-mcp (3/4 ✅, chờ test tay H1: nhìn bàn sau trên kính)

### #52 [P1] ConceptScene: nút „Weiter“ và A/B/C to hơn, xa người hơn, gần chỗ đọc
- **Type:** feature
- **Scene(s):** ConceptScene
- **Files:** ConceptScene `Interactables/NextButton`, `Interactables/AnswerOption` (PokeInteractable + PointableCanvas), Assets/Scripts/Concept/WmsFieldTour.cs, PackageQuizController
- **Problem:** Người dùng báo trên kính (2026-11-02): bấm „Weiter“ và A/B/C rất khó. Đo trong Editor: mỗi nút chỉ 3,6 × 2,5 cm, nằm ở mép bàn (y 0,79, z 0,34), cách mắt ~0,88 m, phải cúi ~65°, chỉ ~0,34 m trước người nên tay phải co lại. Đồng thời phải nhìn thay đổi trên màn hình WMS (y 1,33) và bảng tour/quiz bên cạnh: mắt đi lên đi xuống giữa nút và chỗ đọc.
- **Done when:** theo mục *Done when* của decision 015: tay phải duỗi thoải mái chạm „Weiter“ và từng nút đáp án, trúng ngay lần đầu; „Weiter“ cùng khung hình với bảng tour; đề, ba đáp án (ghi trên nút, „A: …“) và „Frage n von 4“ cùng khung hình, không chữ bị cắt; thẻ câu hỏi không còn trên giá và không liệt kê đáp án; nút không che dialog, bảng tour, `Info Panel`, kiện; cầm kiện không bấm nhầm; cách nút đỏ và Menu ≥ 15 cm; hover/nhấn thấy rõ, có âm thanh, đáp án chọn có „» “ + dấu hiệu thứ hai; tour 10 bước và quiz chấm như cũ; chữ mới qua `localize-strings` + `german-reviewer`; (code) logic tour/quiz không đổi.
- **Inspector work expected:** có thể (dời/đổi cỡ canvas nút)
- **Công:** S–M
- **Test trên kính:** bắt buộc (tầm với, độ thoải mái của tay)
- **Depends on:** #45, #35
- **Decision:** docs/decisions/015-concept-buttons-next-to-reading.md
- **Test report:** docs/test-reports/bl52.md
- **Status:** verified-mcp (13/16 ✅, chờ test tay H1–H3 trên kính: tầm với, bấm trúng, không bấm nhầm khi cầm kiện)

### #22 [Feature] Bài quyết định: chọn cách đếm và xử lý Mindermenge
- **Type:** feature
- **Scene(s):** chưa quyết định (có thể thêm vào Mixed/Complex)
- **Files:** —
- **Problem:** Phần quan trọng của quy trình thật chưa có trong khóa học: chọn cách đếm theo số lượng/khối lượng (Zählen / Referenzwaage / Behälterwaage) và báo Mindermenge khi Ist ≠ Soll. Cả hai có thể dạy rẻ bằng câu hỏi chọn đáp án, không cần mô phỏng cân.
- **Done when:** Có ít nhất một tình huống chọn cách đếm và một tình huống thiếu hàng, mỗi cái có feedback giải thích.
- **Inspector work expected:** yes
- **Công:** M
- **Test trên kính:** nhanh (chọn đáp án bằng tay)
- **Depends on:** câu trả lời A4, B7; người hướng dẫn xác nhận phần này nằm trong phạm vi prototype
- **Bổ sung (ảnh Play mode):** Thêm một tình huống nữa: **Anbruchmenge**. Theo Detail Description mục 5.12, khi mở một VPE mà còn dư thì phần dư phải được cất vào một Modul. ComplexScene tạo đúng tình huống này (lấy 5 chiếc lẻ từ một VPE) nhưng không nhắc gì đến bước đó. (Đã chuyển sang #19.)
- **Bổ sung (2026-11-01):** Câu D2: phần này **ngoài phạm vi prototype**. Hệ quả cho A4: Ist < Soll thì app chỉ báo sai, thùng kho luôn đủ hàng; không mô phỏng `Mindermenge bestätigen`.
- **Status:** dropped (ngoài phạm vi prototype, câu D2 2026-11-01)

### #23 [Feature] Tổng kết cuối khóa và cập nhật mục tiêu ở WelcomeScene
- **Type:** feature
- **Scene(s):** WelcomeScene, ComplexScene (`[Congratulations]`)
- **Files:** WelcomeScene `[Info Panel]/TXT_Description`, ComplexScene `[Congratulations]/TXT_Description`
- **Problem:** Mục tiêu ở Welcome không nhắc phép tính, hàng lẻ hay đơn nhiều vật tư. Cuối khóa chỉ có "Congratulations!", không tóm tắt các quy tắc đã học.
- **Done when:** Welcome liệt kê đúng những gì khóa học dạy. Panel cuối tóm tắt các quy tắc chính (nếu có #10 thì kèm so sánh với dialog thật).
- **Inspector work expected:** yes (text trong scene)
- **Công:** S
- **Test trên kính:** không
- **Depends on:** #17 (để tóm tắt đúng quy tắc, verified-mcp)
- **Bổ sung (2026-11-01, #17):** gạch đầu dòng đầu tiên của Welcome đã đổi thành "Read the material number and VPE on the label" (EN giữ trong scene tới #12, key `welcome.objective.label`). Các gạch còn lại chưa rà.
- **Status:** todo

### #41 [Feature] EKZ quyết định cách đặt hàng: Basic EKZ 3 (snap), các bài khác EKZ 0
- **Type:** feature
- **Scene(s):** BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** BasicPickingScene `BoxSnapper` … `BoxSnapper (8)` (`SnapInteractable`), `SnapArea` (tắt), Assets/Scripts/WS Scripts/OrderConfirm.cs, Assets/Scripts/WS Scripts/CalcForWMS.cs; component mới đọc trạng thái khớp; ô `Empf Kz` của dialog mới (#10)
- **Problem:** EKZ chỉ là chữ trên WMS, cách đặt hàng không phụ thuộc nó. Ở Basic, ô xám của snap hiện sẵn trong thùng đơn nên trainee chỉ nhắm theo ô; `CalcForWMS` không biết carton có khớp ô hay không. Không bài nào cho so sánh một cấp EKZ "bình thường" với một cấp phải xếp có thứ tự.
- **Done when:**
  - Lúc vào BasicPickingScene, đáy thùng đơn không còn ô xám nào.
  - Ô `Empf Kz` trên dialog của Basic hiện `003`.
  - Thả một carton gần một ô, đúng hướng → carton tự khớp vào ô, có âm thanh hoặc rung.
  - Cầm một carton đã khớp, kéo ra khỏi ô → lấy ra được.
  - (giả định A1) Đủ 300 chiếc, carton nào cũng khớp một ô, bấm xác nhận → `Correct`.
  - (giả định D6) Đủ 300 chiếc nhưng có ít nhất một carton nằm lỏng, bấm xác nhận → `Incorrect`; feedback tiếng Đức (*Sie*) nêu quy tắc EKZ 3, không nêu số kiện hay vị trí ô.
  - Thiếu số lượng, mọi carton đều khớp, bấm xác nhận → `Incorrect`; lý do là số lượng, không phải EKZ.
  - Đặt carton vào rồi lấy ra, khớp hay không khớp, Ist/Rest vẫn hiện theo quy tắc của #34.
  - Một carton đã khớp bị tắt hoặc huỷ thì không còn được tính ở lần xác nhận tiếp theo.
  - (code) Logic đếm số lượng không đọc trạng thái khớp; trạng thái khớp chỉ được kiểm khi bấm xác nhận.
  - (code) Trạng thái khớp đọc qua event của Interaction SDK, không poll trong `Update`, hủy đăng ký trong `OnDisable`; không sửa file SDK.
  - (code) Mọi ô snap còn dùng đều nằm trong bounds trigger của `PlasticBox_Order`.
  - (giả định D5) Ở MixedPickingScene và ComplexScene, dialog hiện `Empf Kz` `000`; đặt lỏng đúng đơn, bấm xác nhận → kết quả như trước khi sửa.
  - Mọi chuỗi mới có key + EN + DE và đã qua `german-reviewer`.
  - Console không có lỗi mới ở cả 4 scene picking.
- **Inspector work expected:** yes (tắt MeshRenderer của `BoxSnapper`, gán component mới, kiểm vị trí ô snap)
- **Công:** M
- **Test trên kính:** bắt buộc (snap bằng tay thật, âm thanh/rung, gỡ carton ra)
- **Depends on:** #1, #2, #24 (làm trước, decision 003), #10 cho ô `Empf Kz`; câu D4–D6 (nhóm đã trả lời, chờ người hướng dẫn xác nhận)
- **Decision:** docs/decisions/003-ekz-placement.md
- **Test report:** docs/test-reports/bl41.md
- **Status:** done (commit 5cd22c3, test 27/27 ✅, theo giả định A1, D4–D6; test tay báo 2026-11-02)

## Feature – Log dữ liệu nghiên cứu

Nguồn: `docs/research-logging-spec.md` (bản nháp, 2026-11-01). Mục tiêu: ghi log theo từng người học, gắn mã ẩn danh `participant_id` để nối với phiếu khảo sát giấy. Làm theo thứ tự #26 → #31, rồi #42. Mục 9 của spec đã chốt hết (2026-11-01): không chia nhóm, mỗi người chạy một lần; người điều phối nhập mã; Menu khoá theo thứ tự build (#42); chỉ ghi hướng đầu; người điều phối xoá log trên kính sau khi chép. Code log viết được ngay, nhưng **thu dữ liệu thật** nên chờ #14 (feedback không in số đích) và #1, #15 `done` (spec mục 7).

### #26 [Feature] ResearchLogger: lõi ghi log theo phiên
- **Type:** feature
- **Scene(s):** — (chưa đụng scene; logger tự tạo khi chạy)
- **Files:** script mới `Assets/Scripts/Research/ResearchLogger.cs` (+ kiểu dữ liệu sự kiện); spec mục 1, 3, 4.1 (`session_*`, `scene_enter/exit`, `app_pause/resume`), 4.6 (`unity_error`, `crash_recovered`)
- **Problem:** App chưa ghi lại gì về người học, nên không có dữ liệu cho nghiên cứu và không biết lỗi kỹ thuật xảy ra với ai.
- **Done when:** Chạy Play mode với một mã thử (lấy từ asset `ResearchSettings`) thì có file `<id>_<session>_events.jsonl` trong `Application.persistentDataPath/research/`: dòng đầu là header (schema_version, app/build, máy, mã), có `session_start`, `scene_enter`/`scene_exit` khi chuyển scene, `unity_error` khi cố tình gây một lỗi. Với tuỳ chọn dev "bỏ `session_end`" bật: dừng Play rồi chạy lại thì file cũ có thêm `crash_recovered`; tuỳ chọn tắt thì dừng Play ghi `session_end` lý do `quit`. Mã `P000` hoặc `P900`–`P999` ghi vào `research/practice/`; mã dải người học (vd `P101`) ghi vào `research/`. Scene vẫn chạy bình thường khi không có logger (API tĩnh an toàn với null). `ProjectSettings` có Company `XRISE`, Product `xpick`, Android package id `com.xrise.xpick`; log Play mode nằm dưới `…/XRISE/xpick/research/`. Tạm dừng app vài giây trong scene (Editor: mất focus; Quest: menu hệ thống) thì `paused_s` tăng cỡ đó còn `duration_s` không đổi. Rời scene thì thêm một dòng vào `_summary.csv` (cột của #26 có giá trị, các cột sau để trống). (code) Không lời gọi logger nào cấp phát bộ nhớ hay ghi đĩa mỗi frame. Phiên `P101` vào MixedPickingScene, crash (tuỳ chọn dev), chạy lại: `TryFindResumePoint("P101", out scene)` trả true với MixedPickingScene; sau khi thoát sạch hoặc mã lạ thì trả false. (code) `unity_error` bắn mỗi frame chỉ ra một dòng gộp mỗi 5 giây. `analysis/make_fake_data.R` sinh dữ liệu giả cho ≥10 người học gồm các ca biên (hai phiên crash rồi tiếp, crash không có phiên sau, tín hiệu pause chồng nhau, visit 0 confirm, file `P9xx` nằm trong `research/`, mã lỗi không có trong skill map, hai build); `analysis/analyse.R` đọc không lỗi và ra ba biểu đồ, một CSV rộng đúng một dòng mỗi người học thật (mã thử bị loại, người hai phiên gắn `multi_session`), cảnh báo cho mã lỗi lạ và dòng vi phạm `correct == (error_codes rỗng)`. Ca thủ công: `analyse.R` đọc được một phiên thật từ Play mode (cần người dùng cài R). `docs/research-logging-spec.md` không còn nhắc `condition`/`run`, vị trí tay, bài kiểm tra giấy, hay hai validator Basic.
- **Inspector work expected:** no (logger dùng DontDestroyOnLoad, tự khởi tạo)
- **Công:** M
- **Test trên kính:** không
- **Depends on:** —
- **Bổ sung (2026-11-01):** thiết kế chi tiết và Done when bổ sung ở `docs/superpowers/specs/2026-11-01-research-logger-design.md` (§9), gồm đổi tên app `XRISE` / `xpick` / `com.xrise.xpick`, khung `_summary.csv`, script R trên dữ liệu giả. "Dừng Play giữa chừng → `crash_recovered`" được thay bằng tuỳ chọn dev bỏ `session_end`. Review bắt buộc bởi `data-analyst`.
- **Test report:** docs/test-reports/bl26.md
- **Status:** done (commit 7533874, test 39/39 ✅)

### #27 [Feature] Bàn phím nhập mã người học ở WelcomeScene
- **Type:** feature
- **Scene(s):** WelcomeScene, ComplexScene (màn cuối)
- **Files:** WelcomeScene (bàn phím + panel hỏi lại, tiếp tục, cảnh báo), script mới gọi `ResearchLogger`, `Assets/Scripts/Research/ResearchLogger.cs` (không auto-start ở scene đầu; ghi quyết định tiếp tục; truy vấn phiên đã kết thúc sạch), `Assets/Scripts/SceneMenuController.cs` (thêm hàm nạp scene theo tên), ComplexScene `[Congratulations]`, `analysis/make_fake_data.R`, `analysis/analyse.R`, `docs/research-logging-spec.md`, `docs/i18n/strings-todo.csv`, `docs/i18n/glossary.csv`
- **Problem:** Cần nhập `participant_id` trong VR mà không gõ nhầm, và trên kính không được lưu tên người học.
- **Done when:** (theo decision 004)
  - Mở bản build: chỉ thấy bàn phím (`P` cố định + 3 chữ số, `Löschen`, `OK`); `Menu`, nút đỏ, thùng, `Info Panel`, `Welcome` đều ẩn. `OK` mờ khi chưa đủ 3 chữ số.
  - Gõ `017`, bấm `OK` → panel `Ist der Code P017 richtig?`; `Korrigieren` quay lại bàn phím, mã vẫn còn. Nút `Ja` không nằm đúng chỗ phím `OK`.
  - Nhập `P101`, bấm `Ja` → bàn phím biến mất, Welcome hiện ra, có file `research/P101_…` với `session_start_mode` `manual`, và không có file `P000` nào được tạo trong lần chạy này.
  - Mở bản build (Play từ WelcomeScene) và không gõ mã → không có file log mới nào.
  - Mở thẳng ConceptScene trong Editor → phiên `P000` tự bắt đầu như trước.
  - Nhập `P950` → bàn phím và panel hỏi lại có chữ "Testmodus" kèm biểu tượng; log vào `research/practice/`; vào các bài sau không còn chữ "Testmodus".
  - `StartSession` trả `false` hoặc `ParticipantId` khác mã vừa gõ → hiện lỗi tiếng Đức, Welcome không mở.
  - `P101` vào MixedPickingScene, crash (tuỳ chọn dev), chạy lại, nhập `P101` → panel `Kurs von P101 fortsetzen?` ghi tên bài tiếng Đức. `Ja` → MixedPickingScene bắt đầu lại từ đầu, log mới ghi quyết định tiếp tục. Không đồng ý → Welcome, log ghi là từ chối. `analyse.R` đọc được cả hai ca (`selftest.R` vẫn `selftest OK`).
  - Nhập một mã thật đã có phiên kết thúc sạch → cảnh báo; phải chọn rõ `Korrigieren` hoặc vẫn tiếp tục.
  - Đang có phiên, chọn Menu `1. Introduction` → Welcome hiện ra, không có bàn phím.
  - Màn cuối ComplexScene: dòng chính tiếng Đức (*Sie*) nói bước tiếp theo (bỏ kính, báo người điều phối), dòng phụ `P101`; không có phiên thì không có dòng mã.
  - Mọi chuỗi mới nằm trong `docs/i18n/strings-todo.csv` (key, EN, DE, `max_chars`) và đã qua `german-reviewer`; `Code`, `Sitzung`, `Testmodus`, `Kurs` và tên 5 bài nằm trong glossary với trạng thái `proposed`.
  - Bấm phím bằng poke tay trên Quest (MCP không bấm được).
- **Inspector work expected:** yes (dựng Canvas bàn phím và các panel, gán nút, ẩn 5 đối tượng gốc của Welcome cho tới khi xác nhận, sửa `[Congratulations]` ở ComplexScene)
- **Công:** L
- **Test trên kính:** bắt buộc (bấm phím số bằng tay, tầm với của bàn phím)
- **Depends on:** #26. Ai nhập mã: **người điều phối**, nhập trước khi đưa kính cho trainee (người dùng chốt 2026-11-01, spec mục 9). Câu hỏi cho người hướng dẫn (không chặn): ngôn ngữ của người điều phối, `Code`/`Teilnehmer-Code`, `Sitzung`/`Session`, `Kurs`/`Schulung`, tên tiếng Đức 5 bài.
- **Decision:** docs/decisions/004-participant-keypad.md
- **Ghi chú (2026-11-01):** nút kết thúc phiên cho người điều phối tách thành #44 (người dùng chọn). Dòng Bổ sung bên dưới đã gộp vào Done when.
- **Bổ sung (design #26, 2026-11-01):** thêm nút ẩn cho người điều phối gọi `ResearchLogger.EndSession("facilitator")`. Mã `P900`–`P999` là mã thử (log vào `practice/`); bàn phím báo "chế độ thử" với các mã này. Làm tiếp sau crash (người dùng chốt): nhập lại mã cũ → nếu `ResearchLogger.TryFindResumePoint` thấy phiên trước bị crash thì hỏi người điều phối "Tiếp tục khóa của `P017`?" và báo bài đang dở. Xem `docs/superpowers/specs/2026-11-01-research-logger-design.md` §4, §6.
- **Bổ sung (review cuối #26, 2026-11-01):** bản build nghiên cứu phải bắt đầu phiên **từ bàn phím**. Hiện `ResearchSettings.autoStart` bật sẵn: app tự mở phiên `P000` lúc khởi động, và `StartSession(realId)` trả `false` khi đang có phiên chạy → cả phiên người học bị ghi dưới `P000` (vào `practice/`, bị loại khỏi phân tích). Chọn một trong hai: tắt `autoStart` trong bản build nghiên cứu, hoặc gọi `EndSession(...)` cho phiên `P000` tự động trước khi `StartSession(realId)`. Header có `session_start_mode` (`auto`/`manual`); `analyse.R` cảnh báo khi mã người học thật có `auto`. Done when nên có ca: nhập `P101` → file `research/P101_…` với `session_start_mode` `manual`.
- **Bổ sung (review cuối #27, 2026-11-01, đã làm trên nhánh):** không nút không đảo ngược nào nằm dưới vùng của `Ja` (panel cảnh báo: `Korrigieren` thế chỗ `Ja`; panel tiếp tục: cả hai nút ở hàng dưới); crash ở WelcomeScene không hỏi tiếp tục; log ghi `session_start.ended_session_warning` khi người điều phối bấm `Trotzdem starten`; màn cuối lưu sẵn `Code: –` thay vì `Code: P000`.
- **Test report:** docs/test-reports/bl27.md
- **Status:** done (commit 20373ef, test 53/53 ✅)

### #44 [Feature] Nút kết thúc phiên cho người điều phối
- **Type:** feature
- **Scene(s):** WelcomeScene, ConceptScene, BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** component hoặc prefab mới gọi `ResearchLogger.EndSession("facilitator")`; đặt ở cả 5 scene (hoặc một object tồn tại qua các scene, chốt khi làm, `unity-reviewer` duyệt)
- **Problem:** Người điều phối cần chủ động kết thúc phiên (người học dừng giữa chừng, đổi người). Hiện không có cách nào trên kính, nên phiên chỉ kết thúc khi thoát app hoặc crash.
- **Done when:** (theo decision 004 mục 9)
  - Trong bất kỳ scene nào, khi đang có phiên: chạm nhanh vào nút → không có gì xảy ra.
  - Giữ khoảng 3 s → hỏi `Sitzung wirklich beenden?`; chọn không → quay lại bài, phiên vẫn chạy.
  - Giữ khoảng 3 s rồi xác nhận → dòng cuối của log là `session_end` lý do `facilitator`; WelcomeScene hiện bàn phím (#27).
  - Khi chưa kích hoạt, nút không hiện chữ hay panel nào trong vùng làm việc, và nằm ngoài tầm tay quanh bàn và Menu.
  - Chuỗi mới qua `localize-strings` và `german-reviewer`.
- **Inspector work expected:** yes (đặt nút/prefab ở 5 scene)
- **Công:** M
- **Test trên kính:** bắt buộc (giữ lâu bằng tay thật, kiểm tra không chạm nhầm khi làm bài)
- **Depends on:** #27 (bàn phím để quay về)
- **Decision:** docs/decisions/004-participant-keypad.md
- **Bổ sung (review cuối #27, 2026-11-01):** sau `EndSession("facilitator")` mã đó có `session_end`, nên lần gõ lại sẽ hiện cảnh báo "đã có phiên hoàn tất" của #27 (hôm nay mọi `session_end` đều tính là hoàn tất, cả `quit`/`facilitator`). Chốt cùng #28 lý do nào được tính là "abgeschlossen".
- **Test report:** docs/test-reports/bl44.md
- **Status:** done (commit fe56f11, test 38/38 ✅)

### #28 [Feature] Log kết quả mỗi lần xác nhận + mã lỗi + summary.csv
- **Type:** feature
- **Scene(s):** ConceptScene, BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** Assets/Scripts/PackageQuizController.cs:25, Assets/Scripts/WS Scripts/OrderConfirm.cs:27, Assets/Scripts/MixPickingValidator.cs:18, Assets/Scripts/PiecesOrderValidator.cs:24; helper mới tính mã lỗi; spec mục 4.2, 4.3, 5
- **Problem:** Validator chỉ có đúng/sai và chữ hiển thị, không ghi lại số lần thử, thời gian, trong thùng có gì hay loại lỗi.
- **Done when:** Mỗi lần bấm xác nhận có một `confirm_pressed` (số lần thử, số đếm từng loại so với yêu cầu, `correct`, `error_codes`), quiz có `quiz_answer`, lần đúng đầu tiên có `task_completed`. Rời scene thì có thêm một dòng trong `_summary.csv`. Thử các trường hợp: thùng trống, thiếu, thừa, vật tư không thuộc đơn, thiếu hoặc thừa hàng lẻ, đủ → mã lỗi đúng. Chữ phản hồi trên màn hình **không đổi** (nếu #14 chưa làm). `PackageCounter.CheckOrder` không còn được gọi sau #1 nên không log. Thêm (decision 006):
  - `confirm_pressed` theo dạng dòng đơn (`{item, soll, vpe, required_full, required_loose, placed_full, placed_loose}` + danh sách vật tư lạ `{item, count}`), `error_codes.item` là khoá vật tư; spec, `make_fake_data.R`, `schema_version` và `scene_skills.csv` (`BasicPickingScene,check_material`) đổi trong cùng commit, data-analyst duyệt, `selftest.R` xanh.
  - ComplexScene: đặt 2 carton GGA thay cho 2 carton Ring D thì `error_codes` gồm `WRONG_MATERIAL` (khoá của vật tư lạ) rồi `TOO_FEW` (khoá Ring D), đúng thứ tự.
  - Thùng trống chỉ cho đúng một mã `EMPTY`. Không `confirm_pressed` nào chứa `WRONG_TYPE`.
  - Ở mọi scene, `correct` là true khi và chỉ khi `error_codes` rỗng.
- **Inspector work expected:** no
- **Công:** M
- **Test trên kính:** không
- **Bổ sung (design #26, 2026-11-01):** định dạng `confirm_pressed` / `quiz_answer` / `task_completed` đã chốt ở design §6 (fake data của #26 dùng nó). Bất biến `correct == (error_codes rỗng)`; thêm mã `WRONG_ANSWER` cho quiz. Lần xác nhận đúng đầu tiên ở ComplexScene gọi `EndSession("completed")`. Khung `_summary.csv` do #26 tạo; #28 chỉ điền các cột kết quả.
- **Bổ sung (#27, 2026-11-01):** ở ComplexScene phải bật panel `Congratulations` **trước** khi gọi `EndSession("completed")`, vì `ParticipantCodeLabel` đọc `ResearchLogger.ParticipantId` trong `OnEnable`; sau `EndSession` thì mã đã bị xoá và dòng `Code: P…` sẽ không hiện.
- **Bổ sung (2026-11-01, chốt khi làm #26, fake data đang dùng các lựa chọn này):** điểm cần chốt lại khi làm #28: (1) header thêm một trường báo build này có phát sự kiện kết quả, để R không phải đoán NA hay 0 theo từng phiên; (2) `error_codes_all` viết dạng `CODE:n;CODE:n`; (3) `quiz_answer.selected` là `null` khi chưa chọn gì (kèm `NO_ANSWER`); (4) ConceptScene có ghi `task_completed` không, và `confirm_count` của Concept có bằng số `quiz_answer` không. Câu hỏi cho trưởng nhóm nghiên cứu (data-analyst): BasicPickingScene có kiểm tra vật tư (`check_material`) không, vì `WRONG_TYPE` có thể xảy ra ở đó; một lượt scene bị cắt bởi crash có được tính là lượt đếm không (hiện: có, gắn cờ `visit_censored_<Scene>`). Khi #14 xong cần danh sách build để tách dữ liệu trước/sau #14.
- **Bổ sung (review cuối #26, 2026-11-01):** không gọi `ResearchLogger.Log` trong `Awake`/`OnEnable` của scene vừa nạp: `SceneManager.activeSceneChanged` (nơi logger mở lượt scene mới) chạy **sau** các hàm đó, nên sự kiện sẽ rơi vào lượt scene trước. Ghi từ `Start` trở đi hoặc từ thao tác của người học.
- **Bổ sung (review cuối #27, 2026-11-01):** chốt lý do kết thúc nào được tính là "abgeschlossen" cho cảnh báo của bàn phím #27. Hôm nay `ResearchLogger.HasEndedSession` coi mọi `session_end` là hoàn tất, kể cả `quit`: người học thoát app sạch giữa khoá rồi quay lại sẽ thấy cảnh báo, không được hỏi tiếp tục (fake P112 trong `make_fake_data.R` là ca này). Cân nhắc chỉ tính `completed` (và có thể `facilitator`), hoặc hiện lý do kết thúc trong cảnh báo; nếu đổi thì sửa chuỗi `entry.warning.finished`, spec 4.1 và `analyse.R` cùng lúc.
- **Bổ sung (review cuối #44, 2026-11-01):** (a) `analyse.R` mất lần kết thúc `facilitator` nếu cùng mã được nhập lại, vì `end_status` chỉ lấy phiên cuối. Thêm `n_facilitator_ends` (hoặc chuỗi `end_status` theo phiên) vào `data_quality`/file rộng, kèm ca selftest "facilitator rồi nhập lại". (b) Ý kiến data-analyst cho câu "lý do kết thúc nào được tính là abgeschlossen": ràng buộc cứng là mọi lần kết thúc giữa khóa phải còn thấy được trong phân tích; ưu tiên là chỉ `completed` tính là xong, còn `facilitator`/`quit` được hỏi tiếp tục như sau crash (`TryFindResumePoint` theo lý do). Việc kèm theo: R (`prev_crash` thành "lần kết thúc trước không phải completed", `repeat_after_crash` thành `repeat_after_interrupt`), spec 4.1, trường cộng thêm `session_start.previous_end_reason`, chuỗi `entry.warning.finished`, các ca trong `make_fake_data.R`. Cho đến khi sửa, chữ cảnh báo "đã hoàn tất" là sai sự thật sau `facilitator`/`quit`.
- **Decision:** docs/decisions/006-validator-result-format.md
- **Depends on:** #26, #6 (rule và kết quả trong `XRPick.Orders`). Dùng chung object kết quả với #14.
- **Bổ sung (2026-11-01, decision 011):** scene inventory (`BasicPickingScene.md:82,84`, `MixedPickingScene.md:73–74`) liệt kê hàm xác nhận hai lần trong `_whenSelect` của nút đỏ, nhưng đó là dữ liệu override cũ nằm ngoài `Array.size`. Đọc lúc chạy (MCP, 2026-11-01) thấy Basic, Mixed, Complex đều gọi đúng **một** lần (`PlayAudio` + validator, + `FlashConfirmKey` ở Basic/Complex), nên `confirm_count` không bị nhân đôi. Khi viết test log vẫn nên đếm số dòng `confirm_pressed` sau một lần bấm. Sau khi đơn đúng, nút đỏ không tính thêm lần xác nhận (#47). Log tại sự kiện đúng chung của validator (#47, #40).
- **Bổ sung (2026-11-01, decision 007):** `quiz_answer` theo quiz mới của #45: thêm `question_id`, `selected` là khoá đáp án ngắn (thay `gg/vpe/both`); cách nối câu hỏi với kỹ năng do data-analyst quyết (có thể cho `calculate_quantity` một điểm đo từ Concept). Xoá `WRONG_TYPE` khỏi enum và `skill_map.csv` nếu A2 đã được người hướng dẫn chốt. Chỉ thêm `BasicPickingScene,check_material` nếu #18 giữ vật gây nhiễu ở Basic. Trường `package_type` của #29 (spec §4.4) đổi thành khoá vật tư ở #29.
- **Bổ sung (2026-11-01, review data-analyst của #6):**
  - ~~**Chặn:**~~ **Đã gỡ ở #14 (2026-11-01):** `OrderConfirm` giờ gọi `OrderRule.Check` (dòng đơn từ `CalcForWMS.OrderLine`, đầu vào `CalcForWMS.PackagesInside`). Ghi chú gốc: BasicPickingScene chưa đi qua `OrderRule` (`OrderConfirm` vẫn so `currentSize == necessarySize`). Đầu #28: cho `OrderConfirm` gọi `OrderRule.Check` với một dòng `{training_b, SCHLAUCHSCHELLE B, soll = necessarySize, vpe = 50}`, `countsLoose = false`, đầu vào `OrderContents.From(...)` (9 carton Basic đã có `PackageData`). Ist trên WMS (`CalcForWMS` cộng theo tag) và rule (theo `PackageData`) phải đếm cùng một tập vật thể. Basic không có vật gây nhiễu (#18 đã xoá), nên **không** thêm `BasicPickingScene,check_material`.
  - Trong scene không đếm chiếc lẻ, ghi `required_loose` là `null` (cùng với `placed_loose`). `materialName` là chữ hiển thị, không ghi vào log.
  - `analyse.R`: loại lần bấm chỉ có `EMPTY` khỏi mẫu số của các kỹ năng khác `calculate_quantity`.
  - Feedback đỏ vẫn in số đích (cầu nối tới #14): không thu dữ liệu thật trước #14; R tách dữ liệu trước/sau #14 theo build.
  - Mô tả `WRONG_TYPE` / `WRONG_MATERIAL` trong `analysis/skill_map.csv` còn theo mô hình GG/VPE: sửa cùng spec.
- **Bổ sung (review data-analyst #21, 2026-11-01):** Mixed giờ xác nhận từng position (decision 012). Khi làm #28: (1) `confirm_pressed` thêm `position` (từ 1), `position_count`, `attempt_in_position` (Basic/Complex: 1/1); `attempt_no` vẫn đếm theo lượt scene. (2) `task_completed` chỉ phát khi position cuối đúng (hoặc thêm `order_complete`); R tính `completed`, `attempts_to_correct`, `time_to_correct_s` theo đó, không theo `correct` của từng lần bấm. (3) Ghi `confirm_pressed` **trước** `MixedPickingContainer.AdvanceToNextLine()`; nếu log đổi position (`LineAdvanced`) thì sự kiện đó đứng sau. (4) `EMPTY` ở position 2 nghĩa là „không có gói nào của position hiện tại“ (màn hình hiện câu TOO_FEW, mã giữ EMPTY): sửa định nghĩa trong spec và `skill_map.csv`. (5) `MixedPickingScene,check_material` trong `scene_skills.csv` gần như không thể sai nữa (thùng kho chỉ chứa vật tư của position hiện tại, RING D đã bỏ): người phụ trách nghiên cứu quyết bỏ dòng hay ghi chú. (6) `container`/`required` chỉ chứa gói của position hiện tại; `make_fake_data.R` + `selftest.R` mô phỏng Mixed 2 position (gồm sai ở position 2 rồi đúng, EMPTY ở position 2).
- **Bổ sung (#41, 2026-11-02):** Basic có thêm lỗi cách đặt EKZ 3 (đủ số nhưng có carton không khớp ô). Lỗi này không nằm trong `OrderRule` (nó do `OrderConfirm` + `EkzPlacementCheck` quyết định), nên #28 cần một mã lỗi riêng cho nó, và phải cập nhật spec log, `make_fake_data.R`, `analyse.R` cùng lúc.
- **Status:** todo

### #29 [Feature] Log thao tác: cầm/thả, thùng, nút, kiểu điều khiển
- **Type:** feature
- **Scene(s):** cả 5 scene
- **Files:** component mới `GrabLogger` gắn vào prefab/package (sự kiện grab của Interaction SDK), Assets/Scripts/PackageCounter.cs:29, :61, Assets/Scripts/MixPickingContainer.cs:27, :44, Assets/Scripts/PiecesContainerCounter.cs:30, :46, Assets/Scripts/PackageInfoOnGrab.cs:24, Assets/Scripts/SceneMenuController.cs:6–44; spec mục 4.1 (`menu_select`), 4.4
- **Problem:** Không biết học viên thao tác ra sao: cầm nhầm, đặt vào rồi lấy ra, làm rơi, mất tracking tay.
- **Done when:** Trong Play mode (đặt kiện bằng code qua MCP) thấy `container_enter`/`container_exit` chỉ khi HashSet thật sự đổi; trên Quest thấy `grab_start`/`grab_end` có tay L/R và thời gian cầm, `object_dropped_floor`, `button_touch`, `menu_select`, `input_mode_change`, `tracking_lost`. `object_id` ổn định giữa các lần chạy.
- **Inspector work expected:** yes (gắn `GrabLogger` vào các package, gán layer/tag sàn)
- **Công:** L
- **Test trên kính:** bắt buộc (cầm tay trái/phải, mất tracking)
- **Bổ sung (review cuối #44, 2026-11-01):** ghi thêm `end_control_opened` (panel hiện) và `end_control_cancelled` (bấm `Fortsetzen`) từ `FacilitatorEndControl`, là sự kiện cộng thêm; R dùng `t` của lần mở cuối để loại thời gian của người điều phối khỏi lượt cuối và đếm số lần mở nhầm; sửa `make_fake_data.R` và `analyse.R` cùng lúc. Từ reviewer cuối: nhả giữ khi `OnApplicationPause`/`OnApplicationFocus(false)`; có thể theo dõi `pointerId` đã bắt đầu giữ; nếu Q1 cho thấy giữ bị rớt thì tắt `_recoilAssist` chỉ trên `PokeInteractable` của prefab.
- **Depends on:** #26. API sự kiện grab/tracking của Meta SDK phải kiểm tra trong project trước (spec mới ghi theo hiểu biết, chưa xác minh).
- **Bổ sung (review cuối #26, 2026-11-01):** không gọi `ResearchLogger.Log` trong `Awake`/`OnEnable` của scene vừa nạp (vd. `input_mode_change` ban đầu, đăng ký `GrabLogger`): `activeSceneChanged` chạy sau các hàm đó nên sự kiện rơi vào lượt scene trước. Ghi từ `Start` trở đi.
- **Status:** todo

### #30 [Feature] Hướng nhìn (theo đầu), luồng tư thế và hiệu năng
- **Type:** feature
- **Scene(s):** cả 5 scene
- **Files:** script mới (gaze proxy + pose sampler + perf sampler), collider/tag cho bảng WMS, thẻ hướng dẫn, thùng, menu; spec mục 4.5, 4.6 (`perf_sample`), 4.7
- **Problem:** Cần dữ liệu về chú ý (đọc WMS bao lâu), độ thoải mái (rớt khung hình, cử động đầu) để đối chiếu với SSQ/NASA-TLX trên giấy.
- **Done when:** Có `_pose.csv` 10 Hz; `gaze_target_enter/exit` khi đầu hướng vào bảng WMS ≥ 200 ms; `perf_sample` mỗi 5 s (FPS, khung hình rớt, pin). Trên Quest 3, FPS khi bật logger không thấp hơn khi tắt (so bằng chính `perf_sample`). Ghi rõ trong spec/báo cáo: đây là hướng đầu, không phải eye tracking.
- **Inspector work expected:** yes (gắn tag/collider cho các vùng nhìn)
- **Công:** L
- **Test trên kính:** bắt buộc (đo FPS trên Quest 3)
- **Depends on:** #26. Spec mục 9 (2026-11-01): chỉ ghi hướng đầu, không ghi vị trí tay. #35, #47 trước (decision 011, người dùng 2026-11-01: không thu dữ liệu nhìn theo bố cục cũ; mỗi vùng nhìn có collider riêng trên prefab giá; thêm phiên bản bố cục vào header log).
- **Bổ sung (review cuối #26, 2026-11-01):** dữ liệu 10 Hz (`_pose.csv`) phải ghi qua một stream mở sẵn suốt phiên (đóng/flush khi pause, đổi scene, kết thúc phiên). `SessionFiles` hiện mở file cho mỗi dòng: ổn cho sự kiện thưa, không ổn cho 10 dòng/giây (cấp phát và ghi đĩa liên tục). Không gọi `ResearchLogger.Log` trong `Awake`/`OnEnable` của scene vừa nạp (sự kiện rơi vào lượt scene trước).
- **Status:** todo

### #31 [Feature] Hướng dẫn lấy log về máy + script gộp với phiếu giấy
- **Type:** feature
- **Scene(s):** —
- **Files:** `docs/research-data-guide.md` (mới, cho người điều phối), script nhỏ R hoặc Python gộp các `_summary.csv` theo `participant_id`
- **Problem:** Người điều phối cần quy trình rõ ràng để chép log từ Quest (Meta Quest Developer Hub / `adb pull`), kiểm tra đủ file, xoá trên kính sau khi chép, và nối với dữ liệu phiếu giấy.
- **Done when:** Một người chưa làm bao giờ làm theo hướng dẫn chép được log của 2 phiên thử về máy; script gộp ra một bảng mỗi người một dòng (hoặc mỗi scene một dòng) kèm cột từ file phiếu giấy mẫu.
- **Inspector work expected:** no
- **Công:** M
- **Test trên kính:** nhanh (cắm kính vào máy để chép log, không cần đeo)
- **Bổ sung (2026-11-01):** phiếu giấy: SSQ trước khi đeo kính và cuối khóa, NASA-TLX một lần cuối khóa. #31 thêm file mẫu nhập phiếu (cột `participant_id`, `ssq_pre_*`, `ssq_post_*`, `tlx_*`) và bước "điền SSQ trước" vào quy trình của người điều phối.
- **Bổ sung (design #26, 2026-11-01):** phần gộp dữ liệu đã nằm trong `analysis/analyse.R` (file rộng mỗi người một dòng, dựng từ `events.jsonl`). #31 chỉ còn: hướng dẫn chép log và nối file rộng với phiếu giấy. Script thuộc `data-analyst`.
- **Bổ sung (review cuối #27, 2026-11-01):** trước mỗi đợt thu dữ liệu: xoá thư mục `research/` trên kính và làm trống `analysis/data/real/` (hiện có log thử `P101`, `P899` của dev). Dev test dùng mã thử `P9xx` (vào `practice/`, bị loại khỏi phân tích); chỉ ca cảnh báo "đã có phiên hoàn tất" cần mã thật, dùng `P899` và xoá ngay sau đó.
- **Bổ sung (review cuối #44, 2026-11-01):** thêm ô "Abbruchgrund" vào phiếu giấy (mệt/chóng mặt, thời gian, chạm nhầm, khác), nối theo `participant_id`; không đưa vào app.
- **Depends on:** #28 (cần `summary.csv`). Spec mục 9 (2026-11-01): người điều phối xoá log trên kính ngay sau khi chép về và kiểm tra đủ file.
- **Status:** todo

### #43 [Feature] Bài kiểm tra ngắn trong VR ở đầu và cuối khóa
- **Type:** feature
- **Scene(s):** WelcomeScene (đầu khóa), ComplexScene hoặc một scene cuối mới (cuối khóa)
- **Files:** chưa xác định; ghi log qua `ResearchLogger` (#26)
- **Problem:** Không có bài kiểm tra kiến thức trên giấy (người dùng xác nhận 2026-11-01), nên log chỉ cho *dấu hiệu* tiến bộ (lỗi theo kỹ năng qua các bài), không có số đo trước–sau. Một bài kiểm tra ngắn giống nhau ở đầu và cuối khóa (vd. đọc một dialog WMS, tính số kiện) sẽ cho số đo đó, đổi lại khóa học dài thêm vài phút.
- **Done when:** (viết lại khi quyết định làm) Ở đầu và cuối khóa có cùng dạng câu hỏi, khác số liệu. Mỗi câu trả lời được log kèm đúng/sai và thời gian trả lời. Script R vẽ được điểm trước–sau theo từng người học.
- **Inspector work expected:** yes
- **Công:** M
- **Test trên kính:** nhanh (trả lời câu hỏi bằng tay)
- **Depends on:** #26, #27; nội dung câu hỏi cần người hướng dẫn duyệt; chữ tiếng Đức qua `localize-strings`.
- **Status:** blocked (để sau: người dùng hoặc người phụ trách nghiên cứu quyết định có làm không, 2026-11-01)

### #42 [Feature] Chế độ nghiên cứu: khoá Menu theo thứ tự bài
- **Type:** feature
- **Scene(s):** WelcomeScene, ConceptScene, BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** Assets/Scripts/SceneMenuController.cs, `Menu` (toggle các bài) trong từng scene, `ResearchLogger` (#26, biết mã người học đang chạy); spec mục 9
- **Problem:** Spec mục 9 (2026-11-01) chốt: khi thu dữ liệu, mỗi người học đi hết khóa một lần, theo đúng thứ tự build. Hiện Menu cho nhảy tới bất kỳ bài nào, nên người học có thể bỏ bài hoặc làm lại, làm hỏng so sánh giữa các người học. Chưa có mục nào làm việc này.
- **Done when:** Đang chạy với một mã nghiên cứu (vd. `P017`), ở WelcomeScene: Menu chỉ cho mở bài kế tiếp (ConceptScene); các mục khác hiện mờ và chạm vào thì không chuyển scene. Ở mỗi scene sau: chỉ mục bài kế tiếp mở được, không quay lại bài trước. Sau crash, người điều phối nhập lại mã cũ và chọn tiếp tục (#27) → Menu mở khoá tới bài đang dở (theo `ResearchLogger.TryFindResumePoint`), bài đó bắt đầu lại từ đầu. Đang chạy với mã thử (`P000`, `P900`–`P999`) hoặc không có logger: mọi mục mở được như hiện tại. Chạm một mục bị khoá thì log có `menu_select` kèm `blocked: true` (khi đã có #29), không có scene nào được tải. (code) Thứ tự lấy từ build order, không viết cứng tên scene thêm một lần nữa.
- **Inspector work expected:** yes (gán trạng thái mờ cho toggle trong từng scene; nếu Menu chưa là prefab thì phải sửa 5 lần)
- **Công:** M
- **Test trên kính:** nhanh (chạm mục bị khoá và mục mở trên Quest)
- **Depends on:** #26 (biết chế độ thử/nghiên cứu), #27 (mã được nhập ở Welcome), #18 và #35 (tên, vị trí, đánh dấu mục trong Menu: làm sau để không sửa Menu hai lần; #37 đã gộp vào #35). Câu hỏi còn lại cho Menu: có cần cho "chơi lại bài hiện tại" khi nghiên cứu không (spec nói mỗi người chạy một lần, nên mặc định là không).
- **Status:** todo
