# Backlog

Nguồn: `/code-review` toàn bộ `Assets/Scripts` (2026-11-01), đã đối chiếu với code và scene (commit `db4160f`).
Mục #14–#23: review thiết kế khóa học (`instructional-design-reviewer`, cả 5 scene, 2026-11-01, chưa có ảnh Play mode). Các mã A2, B8… là câu hỏi trong `docs/requirements/questions-for-instructor.md`.
Mục #24–#25 và các dòng **Bổ sung (ảnh Play mode)**: review lần 2 có ảnh Play mode của 5 góc nhìn chuẩn ở cả 5 scene (`TestEvidence/<Scene>/views/`, chỉ có trên máy, không commit), 2026-11-01. Đã đối chiếu lại với code/scene.
Mục #26–#31: log dữ liệu nghiên cứu theo từng người học, từ `docs/research-logging-spec.md` mục 8 (2026-11-01).
Mỗi mục = 1 phiên làm việc = 1 nhánh. Không đánh số lại; mục xong thì ghi `done (commit …)`.

Mức ưu tiên: **P0** lỗi khi chạy / đếm sai · **P1** logic huấn luyện sai · **P2** bảo trì, mở đường cho việc khác · **P3** dọn dẹp.

---

## P0 – Lỗi khi chạy / đếm sai

### #1 [P0] Thống nhất cách kiểm tra đơn hàng ở BasicPickingScene
- **Type:** logic
- **Scene(s):** BasicPickingScene
- **Files:** Assets/Scripts/PackageCounter.cs:112, Assets/Scripts/WS Scripts/OrderConfirm.cs:24, Assets/Scripts/WS Scripts/CalcForWMS.cs
- **Problem:** Nút xác nhận gọi **cả hai** `PackageCounter.CheckOrder()` (đúng khi có 2 package GG) và `OrderConfirm.OrderConfirmation()` (đúng khi `currentSize == necessarySize`; trong scene `necessarySize = 300`, mỗi `BoxInfo.packageSize = 50`, tức là 6 package). Hai quy tắc này không khớp nhau, nên trainee có thể thấy chữ "Correct!" cùng lúc với object "incorrect" hiện lên. Có thể nó chỉ "trùng khớp" nhờ lỗi đếm trùng ở #2.
- **Done when:** Bạn quyết định đơn đúng là gì (số package hay số chiếc). Sau đó chỉ còn một quy tắc, và với đúng đơn đó thì cả text lẫn panel WMS đều báo đúng; với đơn sai thì đều báo sai.
- **Inspector work expected:** yes. Có thể phải bỏ một call khỏi `_whenSelect` của nút, hoặc sửa `necessarySize`/`requiredGGCount`.
- **Depends on:** —
- **Ghi chú (2026-11-01):** Tài liệu requirements không nêu con số cụ thể. Về nguyên lý, tài liệu tính Soll và Ist theo **số chiếc**, còn Rest là tồn kho của thùng. Trong scene có 6 GG đang active × 50 = 300 = `necessarySize`, nên có vẻ ý đồ ban đầu là 6 package, còn `requiredGGCount = 2` là phần sót lại. Cần người hướng dẫn chốt con số.
- **Bổ sung (ảnh Play mode):** Đã kiểm tra trong Editor: cả 3 reference của `PackageCounter` (`packagePlacedText`, `orderFeedbackText`, `instructionText`) trỏ vào `Info Panel/Unity Canvas/*`, mà panel này đang **tắt** (Basic dùng `Info Panel (1)`). Nghĩa là trainee không bao giờ thấy feedback của `PackageCounter`. Chỉ có kết quả của `OrderConfirm` (thẻ `Correct`/`Incorrect` trên bàn) là nhìn thấy được.
- **Status:** blocked (chờ người hướng dẫn xác nhận)

### #2 [P0] CalcForWMS: đếm theo package, không theo collider
- **Type:** bug
- **Scene(s):** BasicPickingScene
- **Files:** Assets/Scripts/WS Scripts/CalcForWMS.cs:23, :32–46
- **Problem:** Mỗi lần một collider vào hoặc ra, script cộng hoặc trừ `packageSize` một lần mà không có HashSet. Vì vậy package có nhiều collider bị cộng nhiều lần. `other.GetComponent<BoxInfo>()` chỉ tìm trên chính collider và không kiểm tra null, nên collider con sẽ gây NullReferenceException. Ngoài ra `Update` ghi lại 3 text mỗi frame dù giá trị không đổi, gây lãng phí trên Quest.
- **Done when:** Thả 1 package GG vào thùng thì IST tăng đúng 50, lấy ra thì giảm đúng 50. Thả và lấy nhiều lần, giá trị vẫn khớp. Console không có lỗi. Text chỉ được cập nhật khi có thay đổi.
- **Inspector work expected:** no (chỉ cần test)
- **Depends on:** #1
- **Status:** todo

## P1 – Logic huấn luyện sai

### #3 [P1] ComplexScene: package sai loại phải làm đơn sai
- **Type:** logic
- **Scene(s):** ComplexScene
- **Files:** Assets/Scripts/PiecesContainerCounter.cs:37, :59, :71–85, :104
- **Problem:** Bộ lọc loại đang bị comment out. Mọi `PackageData` được thêm vào set, nhưng chỉ GGA, GGB và LooseItemDisplay được đếm. Scene có một package `VPEFull`: bỏ nó vào thùng cùng 2 GGB và 5 chiếc lẻ thì vẫn được báo "Correct!". Hàm `IsAcceptedType` còn sót lại cũng liệt kê sai loại.
- **Done when:** Có 2 GGB + 5 chiếc lẻ + 1 VPEFull trong thùng thì bị báo sai, và feedback nói rõ có package sai loại. Bỏ VPEFull ra thì được báo đúng.
- **Inspector work expected:** no
- **Depends on:** —
- **Status:** todo

### #4 [P1] Xoá trạng thái "thành công" khi kiểm tra lại mà đơn sai
- **Type:** logic
- **Scene(s):** BasicPickingScene, ComplexScene
- **Files:** Assets/Scripts/PackageCounter.cs:119–142, Assets/Scripts/PiecesOrderValidator.cs:45, :50–56
- **Problem:** Trainee làm đúng một lần, sau đó lấy bớt package ra rồi bấm kiểm tra lại. Lúc này feedback chuyển sang đỏ, nhưng `instructionText` vẫn ghi "successfully completed" (BasicPicking), hoặc `congratulationsPanel` vẫn hiện (Complex).
- **Done when:** Trong cả hai scene, sau chuỗi đúng → lấy bớt → kiểm tra, màn hình chỉ còn thông báo sai: không còn chữ thành công và không còn panel chúc mừng.
- **Inspector work expected:** no
- **Depends on:** —
- **Status:** todo

### #5 [P1] Menu ở BasicPickingScene thiếu nút sang ConceptScene
- **Type:** bug
- **Scene(s):** BasicPickingScene
- **Files:** Assets/Scenes/BasicPickingScene.unity (menu toggles), Assets/Scripts/SceneMenuController.cs:14
- **Problem:** Các scene khác đều có toggle gọi `OpenConcept`. Trong BasicPickingScene chỉ có `OpenWelcome`, `OpenBasicPicking`, `OpenMixedPicking`, `OpenComplex`. Nếu menu có nút Concept thì nút đó không làm gì.
- **Done when:** Bấm Concept trong menu của BasicPickingScene thì ConceptScene được mở.
- **Inspector work expected:** yes. Thêm call `SceneMenuController.OpenConcept` vào `onValueChanged` của toggle Concept (đây là việc trong Editor, không phải sửa code).
- **Depends on:** —
- **Status:** todo

### #14 [P1] Một mẫu feedback chung: khi sai thì nói lý do và giữ hướng dẫn
- **Type:** logic
- **Scene(s):** BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** Assets/Scripts/MixPickingValidator.cs:38–39, :44–64, Assets/Scripts/PiecesOrderValidator.cs:39–56, BasicPickingScene `Instruction Table/Correct|Incorrect/Secondary Text`
- **Problem:** Khi làm sai, Mixed và Complex xoá `instructionText`, nên trainee mất luôn đề bài. Feedback chỉ liệt kê số đếm, không nói *vì sao* sai (sai loại, thừa, cần hàng lẻ). Basic chỉ ghi "Deine Auswahl war inkorrekt!", không có số nào. Khi đúng, mỗi scene chỉ sang bài tiếp theo một kiểu: Basic ghi "EKZ 8" (không có trong menu), Complex không nói gì.
- **Bổ sung (ảnh Play mode):** Feedback sai hiện tại **lộ đáp án**: Mixed in `GG Connector A: x / 2`, `GG Screw B: x / 2`, `VPE: x / 0` (Assets/Scripts/MixPickingValidator.cs:47–51, :58–62). Complex in `Packages: x / 2`, `Loose pieces: x / 5` (Assets/Scripts/PiecesOrderValidator.cs:50–53). Trainee sai một lần là biết số đúng, không cần tự tính. Ngoài ra chỗ hiện feedback mỗi scene một kiểu: Mixed/Complex/Concept ghi lên panel, Basic dùng thẻ riêng trên bàn.
- **Done when:** Ở cả 3 scene: làm sai thì hướng dẫn vẫn còn, feedback nêu **loại lỗi** (sai loại kiện, sai vật tư, thừa, thiếu, thiếu hàng lẻ) và lý do, nhưng **không in số đích**. Làm đúng thì feedback nói đúng ở đâu và tên mục tiếp theo trong menu (Basic → Mixed → Complex → kết thúc). Feedback hiện ở cùng một chỗ trong mọi scene.
- **Inspector work expected:** yes, với Basic (text nằm trong scene, không trong code)
- **Depends on:** #4 (sửa cùng các dòng), #1 cho phần Basic
- **Status:** todo

### #15 [P1] ComplexScene: bộ đếm không được lộ đáp án
- **Type:** logic
- **Scene(s):** ComplexScene
- **Files:** Assets/Scripts/PiecesContainerCounter.cs:95, :100
- **Problem:** Bộ đếm hiện `PACKAGES PLACED: x / 2` và `PIECES PLACED: x / 55`. Trainee không cần tự tính 55 = 2×25 + 5, vì màn hình đã nói cần 2 package. Đây là scene khó nhất nhưng lại được hỗ trợ nhiều hơn Mixed.
- **Done when:** Trong Play mode bộ đếm chỉ hiện số đã đặt (vd. `Ist: 30 ST`), không hiện số package cần. Kết quả đúng/sai không đổi.
- **Inspector work expected:** no
- **Depends on:** — (nếu làm #6 trước thì sửa luôn ở đó)
- **Status:** todo

### #16 [P1] ComplexScene: sửa thông tin Ring D bị mâu thuẫn
- **Type:** logic
- **Scene(s):** ComplexScene
- **Files:** ComplexScene `[Info Panel]/TXT_Description`, `Cube (1)/Text (TMP)`, `GGB/[VPE_1..3]/Cube/Text (TMP)`
- **Problem:** Panel WMS ghi `Package type VPE & GG`, `Package size 25 pcs / package`, nhưng VPE trên kệ ghi 20 pcs, GG ghi 25 pcs. Một dòng đơn mà có hai cỡ gói, trainee không biết dùng số nào để tính.
- **Done when:** Mọi nhãn và panel WMS của Ring D thống nhất (một dòng cho mỗi dạng gói, hoặc chỉ một dạng), và phép tính của đơn khớp với nhãn.
- **Inspector work expected:** yes (sửa text trong scene)
- **Depends on:** câu trả lời A5, A6
- **Status:** blocked (chờ người hướng dẫn xác nhận)

### #17 [P1] Chốt nghĩa GG/VPE rồi làm lại ConceptScene và sửa nhãn lệch logic
- **Type:** logic
- **Scene(s):** ConceptScene, BasicPickingScene (nhãn), MixedPickingScene, ComplexScene
- **Files:** Assets/Scripts/PackageQuizController.cs, Assets/Scripts/PackageInfoOnGrab.cs, ConceptScene `[PlasticBag_VPE_3]` (nhãn ghi "GG Screw B"), BasicPickingScene các package nhãn `VPE … 50 Stück` nhưng đếm theo tag `GGPackage` (Assets/Scripts/WS Scripts/CalcForWMS.cs:32)
- **Problem:** ConceptScene dạy GG và VPE là hai *loại kiện*, chỉ VPE được mở. Tài liệu yêu cầu định nghĩa VPE là *số cái trong một thùng/túi* và không nhắc "GG". Ngoài ra tên object, nhãn và cách code đếm không khớp nhau, nên trainee học một quy tắc rồi gặp ngược lại ở bài sau.
- **Done when:** Nghĩa GG/VPE đã được người hướng dẫn chốt và ghi vào `docs/i18n/glossary.csv`. ConceptScene dạy đúng nghĩa đó. Ở mọi scene, nhãn trên package khớp với cách validator đếm nó.
- **Inspector work expected:** yes (nhãn, tag, `PackageData`)
- **Depends on:** câu trả lời A2, A3; #1
- **Bổ sung (ảnh Play mode):**
  - **Quiz ở Concept ít giá trị kiểm tra:** Khi cầm kiện GG, panel hiện ngay `Rule: GG packages must NOT be opened.` (`[Info Panel]/TXT_Rule`, giá trị `packageRule` của `PackageInfoOnGrab`). Hộp VPE thì đã được mô hình hoá ở trạng thái mở sẵn. Vì vậy câu hỏi "Which package can be opened?" chỉ kiểm tra trainee có nhớ dòng vừa đọc vài giây trước hay không.
  - **Thông tin cho A2:** Tài liệu Detail Description (mục Informationen Kommissionierung) có quy tắc: túi 10 cái gồm hơn 2 thành phần thì không được mở; phải lấy nguyên túi và chỉnh lại Restmenge. Có thể quy tắc "GG không được mở" bắt nguồn từ đây.
- **Status:** blocked (chờ người hướng dẫn xác nhận)

### #18 [P1] Đưa BasicPickingScene về cùng chuỗi bài
- **Type:** logic
- **Scene(s):** BasicPickingScene
- **Files:** BasicPickingScene: menu (prefab override `2. EKZ 2`, `3. EKZ 3`, `4. EKZ 8`), `Instruction Table/*` (`1. Lesen des LVS` … `3. Versenden des Auftrags`), các package cũ dưới `Interactables` (`[VPE_OpenedPackage]`, Cable Connector A), panel cũ `[Info Panel]` (CABLE CONNECTOR A 100 pcs)
- **Problem:** Basic được làm lại riêng thành bài "EKZ 3": menu đặt tên theo EKZ, mẫu hướng dẫn khác các scene còn lại, bước 3 ghi "versenden" (gửi đi) thay vì xác nhận, còn sót package và panel của kịch bản cũ. Trainee đi từ Concept sang Basic gặp một giao diện hoàn toàn khác.
- **Done when:** Menu giống hệt các scene khác (tên bài, thứ tự, có Concept). Hướng dẫn theo cùng mẫu tiêu đề + 3 bước. Không còn package/panel nào không thuộc đơn hiện tại, trừ khi được đặt có chủ ý làm vật gây nhiễu.
- **Inspector work expected:** yes (chủ yếu sửa scene)
- **Depends on:** #1, #5; người hướng dẫn xác nhận có muốn đặt tên bài theo EKZ không
- **Ghi chú:** Cần Play mode kiểm tra package cũ có nhìn thấy và cầm được không. Trong ảnh `overview`/`container` không thấy các package này.
- **Bổ sung (ảnh Play mode):**
  - **Chỉ dẫn bắt làm việc không làm được:** Panel WMS ra lệnh `Bitte Material umpacken!` (`[Info Panel (1)]/Anweisung Visuals/Text (TMP) (1)`) và `EKZ Hinweis: Leerräume auffüllen`, nhưng scene không cho làm hai việc đó. Thẻ hướng dẫn lại bảo "Achte immer auf die Anweisungen", nên trainee học cách bỏ qua chỉ dẫn. Hoặc bỏ hai dòng này, hoặc dạy chúng (#20).
  - **Menu lệch số EKZ:** Mục menu `2. EKZ 2` trong khi WMS hiện `EKZ: 003`.
  - **Gợi ý bài tiếp theo bỏ qua Mixed:** Thẻ `Correct` ghi "nächste Aufgabe (EKZ 8)", tức là Complex, bỏ qua Mixed.
  - **Nhãn nút F12:** Nút đỏ ghi `F12` và thẻ ghi "(F12)". Theo spec dialog, F12 là `Aufkl. korr.` (sửa nhãn), còn xác nhận là `F6 Bestätigen` (suy ra). Cần câu trả lời B8/B9 rồi thống nhất với #10.
- **Status:** blocked (chờ người hướng dẫn xác nhận)

### #24 [P1] BasicPickingScene: thêm kiện thừa để phép tính có ý nghĩa
- **Type:** logic
- **Scene(s):** BasicPickingScene
- **Files:** BasicPickingScene `[GG_Package (18)]`…`[GG_Package (23)]` (`BoxInfo.packageSize` 50 ×6), `CalcForWMS.necessarySize` 300
- **Problem:** Thùng kho chỉ có đúng 6 kiện TRAINING-MATERIAL B, và đơn cần đủ cả 6 (300 ÷ 50). Trainee bỏ hết vào thùng là đúng, không phải chọn hay tính gì (ảnh `TestEvidence/BasicPickingScene/views/container.png`). Tải đọc WMS cao nhất khóa học nhưng việc thực hành lại dễ nhất.
- **Done when:** Thùng kho có nhiều kiện hơn số đơn cần. Bỏ hết vào thì bị báo sai, bỏ đúng số tính được thì được báo đúng.
- **Inspector work expected:** yes (nhân bản kiện, sửa `necessarySize` nếu cần)
- **Depends on:** #1 (chốt đơn đúng là bao nhiêu)
- **Status:** blocked (chờ #1)

### #25 [P1] Một mẫu thẻ hướng dẫn chung cho cả 5 scene
- **Type:** logic
- **Scene(s):** WelcomeScene, ConceptScene, BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** các text `Instruction Table/Info Canvas/*` và `Instruction Table/Confirm/*` trong từng scene
- **Problem:** Vị trí và kiểu thẻ đã giống nhau, nhưng nội dung thì không.
  - **Tiêu đề thẻ đầu:** `Welcome!` / `Observe` / `Aufgabe` / `Task`.
  - **Bước tính toán:** nằm ở bước 2 trong Mixed ("Calculate how many…"), nhưng ở bước 1 trong Complex ("1. Read WMS – Determine how many full packages and loose items…").
  - **Động từ cho nút đỏ:** Welcome dạy "Touch … fingertip", Concept "click", Mixed/Complex "press", Basic "drücke".
  - Trainee phải đọc lại cấu trúc mỗi bài.
- **Done when:** Mọi scene picking dùng cùng mẫu: `Aufgabe/Task`, 1. đọc WMS, 2. tính và lấy hàng, 3. kiểm tra và xác nhận. Cả 5 scene dùng một động từ cho thao tác bấm nút. So ảnh `instructions.png` của các scene thấy cùng cấu trúc.
- **Inspector work expected:** yes (text trong scene)
- **Depends on:** #18 cho phần Basic. Nên làm **trước** #12 để chỉ phải dịch một lần.
- **Status:** todo

## P2 – Bảo trì

### #6 [P2] Một nguồn duy nhất cho số lượng yêu cầu của đơn
- **Type:** refactor
- **Scene(s):** BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** Assets/Scripts/MixPickingValidator.cs:31–64, Assets/Scripts/PiecesContainerCounter.cs:100, :106, Assets/Scripts/PiecesOrderValidator.cs:37, :52–53, Assets/Scripts/PackageCounter.cs:115
- **Problem:** Validator viết cứng các số `2 / 2 / 0`, `GGB==2 && GGA==0 && Loose==5`, "selected 2 GG", và không gọi `IsOrderCorrect()` / `IsCorrect()`. Ai đổi số trong Inspector thì text PACKAGE và validator sẽ lệch nhau.
- **Done when:** Đổi một số yêu cầu trong Inspector (ví dụ `requiredGGA = 3`) thì cả text đếm, feedback và kết quả đúng/sai đều theo số mới mà không cần sửa code. Số chỉ khai báo ở một chỗ cho mỗi scene, có thể là field của counter hoặc một ScriptableObject `OrderDefinition`.
- **Inspector work expected:** có thể. Nếu dùng ScriptableObject thì phải tạo asset và gán vào. Field cũ cần `[FormerlySerializedAs]`.
- **Depends on:** #1, #3
- **Status:** todo

## P3 – Dọn dẹp

### #7 [P3] Kiểm tra null cho reference Inspector trong validator
- **Type:** cleanup
- **Scene(s):** BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** Assets/Scripts/PackageCounter.cs:119, Assets/Scripts/MixPickingValidator.cs:33–63, Assets/Scripts/PiecesOrderValidator.cs:39–56
- **Problem:** `feedbackText`, `instructionText` và `congratulationsPanel` được dùng mà không kiểm tra null. Hiện tại trong scene chúng **đều đã được gán**, nên chưa gây lỗi. Nhưng nếu sau này quên gán thì lỗi chỉ hiện ra đúng lúc trainee làm đúng.
- **Done when:** Xoá tạm một reference trong Inspector rồi bấm xác nhận: Console báo `LogError` có tên GameObject, không có NullReferenceException. Sau đó gán lại.
- **Inspector work expected:** no
- **Depends on:** #4 (sửa cùng các dòng đó)
- **Status:** todo

### #8 [P3] Xoá code chết trong scripts
- **Type:** cleanup
- **Scene(s):** —
- **Files:** Assets/Scripts/SceneMenuController.cs:48–102, Assets/Scripts/PackageInfoDisplay.cs:1–41, Assets/Scripts/MixPickingContainer.cs:2, Assets/Scripts/MixPickingValidator.cs:44–64, Assets/Scripts/WS Scripts/OrderConfirm.cs:11–20, Assets/Scripts/WS Scripts/BoxInfo.cs:6–16
- **Problem:** Có các khối code bị comment out, `using System.Xml.Schema` không dùng, hai nhánh if/else giống hệt nhau, và các `Start`/`Update` rỗng. Chúng làm code khó đọc. Còn `MenuSelectionController.cs` đã bị comment toàn bộ và `ScaleItemCounter.cs` không được scene nào dùng: phải quyết định giữ hay xoá cả `.cs` lẫn `.meta`. Trước khi xoá, chạy `scene-ref-auditor`.
- **Done when:** Code compile không lỗi, cả 5 scene chạy như trước, và không có khối code bị comment dài nào.
- **Inspector work expected:** no
- **Depends on:** #2, #3, #6 (làm sau để tránh xung đột)
- **Status:** todo

### #9 [P3] Xoá các UnityEvent call trỏ vào "None"
- **Type:** cleanup
- **Scene(s):** WelcomeScene, BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** WelcomeScene.unity:556–575 (`MixedPickingValidator.ConfirmOrder`, target trống), các call `PackageInfoDisplay.ShowDefaultInfo` có `m_Target: {fileID: 0}` trong Basic/Mixed/Complex
- **Bổ sung (2026-11-01, từ scene inventory):** Sự kiện cầm package (`ISDK_HandGrabInteraction`) trong MixedPickingScene còn gọi `PackageInfoDisplay.ShowGGInfo` / `ShowVPEInfo`. Hai method này đã bị comment out trong code, và target cũng đang trống. Cần kiểm tra BasicPickingScene và ComplexScene xem có tình trạng tương tự không.
- **Problem:** Các call còn sót lại khi copy prefab. Chúng không làm gì nhưng gây nhầm lẫn khi đọc Inspector.
- **Done when:** Trong Inspector không còn dòng event nào có target "None (Object)". Các nút vẫn chạy như trước.
- **Inspector work expected:** yes. Xoá từng dòng event trong Editor.
- **Depends on:** —
- **Status:** todo

---

## Feature

### #11 [Feature] Nền tảng đa ngôn ngữ DE/EN và nút chọn ngôn ngữ
- **Type:** feature
- **Scene(s):** tất cả (nút chọn ngôn ngữ đặt ở WelcomeScene)
- **Files:** Packages/manifest.json (`com.unity.localization`), String Table Collection `UI` (locale `de` mặc định, `en`), script lưu lựa chọn ngôn ngữ
- **Problem:** End user là người Đức bản ngữ và không đọc được tiếng Anh, trong khi toàn bộ app hiện là tiếng Anh. Đội phát triển lại cần tiếng Anh để test.
- **Done when:** Menu có nút **Deutsch / English**. Chuyển ngôn ngữ thì đổi được ít nhất một text mẫu ở mỗi scene. Khởi động lại app vẫn giữ lựa chọn. Mặc định là Deutsch.
- **Inspector work expected:** yes (cài package, tạo Localization Settings và locale, gắn `LocalizeStringEvent`)
- **Depends on:** —
- **Status:** todo

### #12 [Feature] Chuyển toàn bộ text hiển thị sang bảng dịch (EN + DE)
- **Type:** refactor
- **Scene(s):** tất cả
- **Files:** khoảng 128 TMP text trong 5 scene; các chuỗi feedback trong `PackageCounter`, `MixPickingValidator`, `PiecesOrderValidator`, `PackageQuizController`, `PackageInfoDisplay`, `SampleFormulaDisplay`
- **Problem:** Text đang viết cứng bằng tiếng Anh trong code và scene, nên không dịch được.
- **Done when:** Ở chế độ Deutsch không còn chữ tiếng Anh nào, trừ chú thích WMS. Mọi chuỗi tiếng Đức đã qua `german-reviewer`. Có file `docs/i18n/review-status.csv`.
- **Inspector work expected:** yes (gắn key cho từng TMP text)
- **Depends on:** #11. Nên làm sau #4, #6 và #25 để không phải dịch các chuỗi sắp bị sửa.
- **Bổ sung (2026-11-01, review thiết kế khóa học):** Khi dịch, thống nhất luôn: một thuật ngữ cho LVS/WMS (tài liệu dùng LVS); một tên cho mỗi vật tư (Mixed đang có "Cable Connector A", "GG Connector A", "GG Screw packages"…); đơn vị `ST` trên panel WMS và `Stück` trong câu; Basic đang xưng *du* và có lỗi chính tả "Vervolsständige" → chuyển sang *Sie*. Chuỗi ghép từng mảnh (`SampleFormulaDisplay.cs:31–36`, `PiecesOrderValidator.cs:40–42`) phải thành câu trọn vẹn có placeholder.
- **Status:** todo

### #13 [Feature] File export EN–DE cho người bản ngữ duyệt
- **Type:** feature
- **Scene(s):** —
- **Files:** `docs/i18n/review/` (CSV UTF-8 BOM + hướng dẫn bằng tiếng Đức), theo skill `localize-strings` mục 4–5
- **Problem:** Người hướng dẫn hoặc một người Đức bản ngữ cần duyệt toàn bộ bản dịch trong Excel, rồi gửi lại để cập nhật.
- **Done when:** Tạo được file review mở trong Excel, umlaut hiển thị đúng. Import lại một file mẫu đã điền thì trạng thái chuyển sang `confirmed`.
- **Inspector work expected:** no
- **Depends on:** #12
- **Status:** todo

### #10 [Feature] Dựng panel WMS giống dialog Kommissionierung thật
- **Type:** feature
- **Scene(s):** BasicPickingScene trước, sau đó MixedPickingScene và ComplexScene (tuỳ câu trả lời B8)
- **Files:** panel WMS hiện có trong scene, `Assets/Scripts/WS Scripts/CalcForWMS.cs`. Spec nằm ở chỗ bảo mật (`docs/requirements/wms-specs/kommissionierung-dialog.md`, không commit)
- **Problem:** Panel WMS hiện tại không giống màn hình thật mà trainee sẽ dùng ở kho, nên bài tập chưa sát thực tế.
- **Bổ sung (ảnh Play mode):** Hiện có 3 kiểu panel trong 3 scene thực hành: Basic là dialog xám tiếng Đức (`Soll/Ist/Rest`, `EKZ`, `VPE: 50`), còn Mixed/Complex là panel đen tiếng Anh (`Order quantity`, `Package type`, `Package size`). Cùng một khái niệm có tên khác nhau giữa các bài (`Soll` ↔ `Order quantity`, `VPE: 50` ↔ `50 pcs / package`). Hơn nữa, màn hình giống thật lại xuất hiện *trước* màn hình đơn giản. Khi làm mục này thì dùng **một mẫu** cho cả 3 scene, với tên trường thật.
- **Done when:** Trong Play mode, đặt panel cạnh ảnh gốc: bố cục, chữ và trạng thái chỉ báo khớp với spec. Soll/Ist/Rest và `F6 Bestätigen` hoạt động theo quy tắc đã chốt. Không có ảnh hay tài liệu gốc nào nằm trong `Assets/`.
- **Inspector work expected:** yes (dựng Canvas, gán reference)
- **Depends on:** #1, câu trả lời của người hướng dẫn (file câu hỏi trong `docs/requirements/`, mục B1–B10)
- **Status:** blocked (chờ người hướng dẫn xác nhận)

### #19 [Feature] ComplexScene: tách thành hai bước, có ví dụ phép chia có dư
- **Type:** feature
- **Scene(s):** ComplexScene (có thể thêm một scene mới)
- **Files:** ComplexScene, Assets/Scripts/PiecesContainerCounter.cs, Assets/Scripts/PiecesOrderValidator.cs
- **Problem:** Complex thêm 4 thứ mới cùng lúc: phép chia có dư, hàng lẻ, hai dạng gói của một vật tư, cỡ gói không rõ. Chưa scene nào làm mẫu phép chia có dư. Việc mở VPE được dạy ở Concept nhưng chưa bao giờ được thực hành (hàng lẻ đã nằm sẵn).
- **Done when:** Bước (a): package nguyên + hàng lẻ của **một** dạng gói, có ví dụ mẫu trước. Bước (b): thêm dạng gói thứ hai. Trainee tự lấy hàng lẻ từ một VPE (nếu A3 cho phép mở).
- **Inspector work expected:** yes
- **Depends on:** #3, #16, #17
- **Status:** blocked (chờ người hướng dẫn xác nhận)

### #20 [Feature] Dạy Soll/Ist/Rest, EKZ, Anweisung trước khi dùng
- **Type:** feature
- **Scene(s):** ConceptScene hoặc đầu BasicPickingScene
- **Files:** BasicPickingScene `[Info Panel (1)]/Panel/*` (Soll, Ist, Rest, EKZ, EKZ Hinweis, Anweisung)
- **Problem:** Basic hiện Soll/Ist/Rest, EKZ và Anweisung và bảo trainee "Achte immer auf … EKZ-Hinweise!", nhưng không scene nào giải thích chúng, và bài tập không phụ thuộc vào chúng. Ngoài ra app dùng Rest là "còn thiếu", còn tài liệu nói Rest là tồn kho còn lại trong thùng.
- **Done when:** Có một bước dạy/làm mẫu ngắn cho từng trường trước lần đầu chúng xuất hiện, hoặc trường nào chưa dùng thì ẩn đi. Rest mang đúng nghĩa đã chốt.
- **Inspector work expected:** yes
- **Depends on:** #10; câu trả lời B3, B4, B6
- **Status:** blocked (chờ người hướng dẫn xác nhận)

### #21 [Feature] MixedPickingScene: nhặt tuần tự từng vị trí như dialog thật
- **Type:** feature
- **Scene(s):** MixedPickingScene
- **Files:** MixedPickingScene `[Info Panel]/TXT_Description`, `TXT_Description (1)`, Assets/Scripts/MixPickingContainer.cs, Assets/Scripts/MixPickingValidator.cs
- **Problem:** Dialog thật xử lý từng vị trí một (`Pick 1/2`, `2/2`). Mixed hiện hai dòng đơn cạnh nhau trên một panel, và không có bộ đếm riêng cho từng vật tư, nên lỗi chỉ lộ ra khi xác nhận.
- **Done when:** Trainee làm vị trí 1, xác nhận, rồi panel chuyển sang vị trí 2. Mỗi vị trí có Ist riêng.
- **Inspector work expected:** yes
- **Depends on:** #10; câu trả lời B8
- **Status:** blocked (chờ người hướng dẫn xác nhận)

### #22 [Feature] Bài quyết định: chọn cách đếm và xử lý Mindermenge
- **Type:** feature
- **Scene(s):** chưa quyết định (có thể thêm vào Mixed/Complex)
- **Files:** —
- **Problem:** Phần quan trọng của quy trình thật chưa có trong khóa học: chọn cách đếm theo số lượng/khối lượng (Zählen / Referenzwaage / Behälterwaage) và báo Mindermenge khi Ist ≠ Soll. Cả hai có thể dạy rẻ bằng câu hỏi chọn đáp án, không cần mô phỏng cân.
- **Done when:** Có ít nhất một tình huống chọn cách đếm và một tình huống thiếu hàng, mỗi cái có feedback giải thích.
- **Inspector work expected:** yes
- **Depends on:** câu trả lời A4, B7; người hướng dẫn xác nhận phần này nằm trong phạm vi prototype
- **Bổ sung (ảnh Play mode):** Thêm một tình huống nữa: **Anbruchmenge**. Theo Detail Description mục 5.12, khi mở một VPE mà còn dư thì phần dư phải được cất vào một Modul. ComplexScene tạo đúng tình huống này (lấy 5 chiếc lẻ từ một VPE) nhưng không nhắc gì đến bước đó.
- **Status:** blocked (chờ người hướng dẫn xác nhận)

### #23 [Feature] Tổng kết cuối khóa và cập nhật mục tiêu ở WelcomeScene
- **Type:** feature
- **Scene(s):** WelcomeScene, ComplexScene (`[Congratulations]`)
- **Files:** WelcomeScene `[Info Panel]/TXT_Description`, ComplexScene `[Congratulations]/TXT_Description`
- **Problem:** Mục tiêu ở Welcome không nhắc phép tính, hàng lẻ hay đơn nhiều vật tư. Cuối khóa chỉ có "Congratulations!", không tóm tắt các quy tắc đã học.
- **Done when:** Welcome liệt kê đúng những gì khóa học dạy. Panel cuối tóm tắt các quy tắc chính (nếu có #10 thì kèm so sánh với dialog thật).
- **Inspector work expected:** yes (text trong scene)
- **Depends on:** #17 (để tóm tắt đúng quy tắc)
- **Status:** todo

## Feature – Log dữ liệu nghiên cứu

Nguồn: `docs/research-logging-spec.md` (bản nháp, 2026-11-01). Mục tiêu: ghi log theo từng người học, gắn mã ẩn danh `participant_id` để nối với phiếu khảo sát giấy. Làm theo thứ tự #26 → #31. Các câu hỏi mở ở mục 9 của spec cần người phụ trách nghiên cứu trả lời trước khi bắt đầu thu dữ liệu thật, nhưng không chặn #26.

### #26 [Feature] ResearchLogger: lõi ghi log theo phiên
- **Type:** feature
- **Scene(s):** — (chưa đụng scene; logger tự tạo khi chạy)
- **Files:** script mới `Assets/Scripts/Research/ResearchLogger.cs` (+ kiểu dữ liệu sự kiện); spec mục 1, 3, 4.1 (`session_*`, `scene_enter/exit`, `app_pause/resume`), 4.6 (`unity_error`, `crash_recovered`)
- **Problem:** App chưa ghi lại gì về người học, nên không có dữ liệu cho nghiên cứu và không biết lỗi kỹ thuật xảy ra với ai.
- **Done when:** Chạy Play mode với một mã thử (đặt tạm trong Inspector) thì có file `<id>_<session>_events.jsonl` trong `Application.persistentDataPath/research/`: dòng đầu là header (schema_version, app/build, máy, mã), có `session_start`, `scene_enter`/`scene_exit` khi chuyển scene, `unity_error` khi cố tình gây một lỗi. Dừng Play giữa chừng rồi chạy lại thì có `crash_recovered`. Mã `P000` ghi vào thư mục riêng. Scene vẫn chạy bình thường khi không có logger (API tĩnh an toàn với null).
- **Inspector work expected:** no (logger dùng DontDestroyOnLoad, tự khởi tạo)
- **Depends on:** —
- **Status:** todo

### #27 [Feature] Bàn phím nhập mã người học ở WelcomeScene
- **Type:** feature
- **Scene(s):** WelcomeScene
- **Files:** WelcomeScene (panel bàn phím số + panel xác nhận), script mới gọi `ResearchLogger` bắt đầu phiên; spec mục 2
- **Problem:** Cần nhập `participant_id` trong VR mà không gõ nhầm, và trên kính không được lưu tên người học.
- **Done when:** Mở app: chỉ thấy bàn phím số (`P` cố định + 3 chữ số, xoá, xác nhận). Xác nhận → hỏi lại "`P017` – đúng chưa?" → *Yes* mới mở phần còn lại của WelcomeScene và bắt đầu phiên; *Back* quay lại nhập. Màn hình cuối khóa hiện lại mã. Thử bằng poke tay trên Quest (MCP không bấm được).
- **Inspector work expected:** yes (dựng Canvas, gán nút, ẩn phần Welcome cho tới khi xác nhận)
- **Depends on:** #26; quyết định "ai nhập mã" (spec mục 9)
- **Status:** todo

### #28 [Feature] Log kết quả mỗi lần xác nhận + mã lỗi + summary.csv
- **Type:** feature
- **Scene(s):** ConceptScene, BasicPickingScene, MixedPickingScene, ComplexScene
- **Files:** Assets/Scripts/PackageQuizController.cs:25, Assets/Scripts/PackageCounter.cs:104, Assets/Scripts/WS Scripts/OrderConfirm.cs:22, Assets/Scripts/MixPickingValidator.cs:18, Assets/Scripts/PiecesOrderValidator.cs:24; helper mới tính mã lỗi; spec mục 4.2, 4.3, 5
- **Problem:** Validator chỉ có đúng/sai và chữ hiển thị, không ghi lại số lần thử, thời gian, trong thùng có gì hay loại lỗi.
- **Done when:** Mỗi lần bấm xác nhận có một `confirm_pressed` (số lần thử, số đếm từng loại so với yêu cầu, `correct`, `error_codes`), quiz có `quiz_answer`, lần đúng đầu tiên có `task_completed`. Rời scene thì có thêm một dòng trong `_summary.csv`. Thử các trường hợp: thùng trống, thiếu, thừa, sai loại, sai vật tư, đủ → mã lỗi đúng. Chữ phản hồi trên màn hình **không đổi**. Basic ghi cả hai kết quả (`rule_package_counter`, `rule_calc_for_wms`) cho tới khi xong #1.
- **Inspector work expected:** no
- **Depends on:** #26. Helper mã lỗi dùng lại được cho #14; nếu làm #14 trước thì dùng chung.
- **Status:** todo

### #29 [Feature] Log thao tác: cầm/thả, thùng, nút, kiểu điều khiển
- **Type:** feature
- **Scene(s):** cả 5 scene
- **Files:** component mới `GrabLogger` gắn vào prefab/package (sự kiện grab của Interaction SDK), Assets/Scripts/PackageCounter.cs:29, :61, Assets/Scripts/MixPickingContainer.cs:27, :44, Assets/Scripts/PiecesContainerCounter.cs:30, :46, Assets/Scripts/PackageInfoOnGrab.cs:24, Assets/Scripts/SceneMenuController.cs:6–44; spec mục 4.1 (`menu_select`), 4.4
- **Problem:** Không biết học viên thao tác ra sao: cầm nhầm, đặt vào rồi lấy ra, làm rơi, mất tracking tay.
- **Done when:** Trong Play mode (đặt kiện bằng code qua MCP) thấy `container_enter`/`container_exit` chỉ khi HashSet thật sự đổi; trên Quest thấy `grab_start`/`grab_end` có tay L/R và thời gian cầm, `object_dropped_floor`, `button_touch`, `menu_select`, `input_mode_change`, `tracking_lost`. `object_id` ổn định giữa các lần chạy.
- **Inspector work expected:** yes (gắn `GrabLogger` vào các package, gán layer/tag sàn)
- **Depends on:** #26. API sự kiện grab/tracking của Meta SDK phải kiểm tra trong project trước (spec mới ghi theo hiểu biết, chưa xác minh).
- **Status:** todo

### #30 [Feature] Hướng nhìn (theo đầu), luồng tư thế và hiệu năng
- **Type:** feature
- **Scene(s):** cả 5 scene
- **Files:** script mới (gaze proxy + pose sampler + perf sampler), collider/tag cho bảng WMS, thẻ hướng dẫn, thùng, menu; spec mục 4.5, 4.6 (`perf_sample`), 4.7
- **Problem:** Cần dữ liệu về chú ý (đọc WMS bao lâu), độ thoải mái (rớt khung hình, cử động đầu) để đối chiếu với SSQ/NASA-TLX trên giấy.
- **Done when:** Có `_pose.csv` 10 Hz; `gaze_target_enter/exit` khi đầu hướng vào bảng WMS ≥ 200 ms; `perf_sample` mỗi 5 s (FPS, khung hình rớt, pin). Trên Quest 3, FPS khi bật logger không thấp hơn khi tắt (so bằng chính `perf_sample`). Ghi rõ trong spec/báo cáo: đây là hướng đầu, không phải eye tracking.
- **Inspector work expected:** yes (gắn tag/collider cho các vùng nhìn)
- **Depends on:** #26; quyết định có cần vị trí tay không (spec mục 9)
- **Status:** todo

### #31 [Feature] Hướng dẫn lấy log về máy + script gộp với phiếu giấy
- **Type:** feature
- **Scene(s):** —
- **Files:** `docs/research-data-guide.md` (mới, cho người điều phối), script nhỏ R hoặc Python gộp các `_summary.csv` theo `participant_id`
- **Problem:** Người điều phối cần quy trình rõ ràng để chép log từ Quest (Meta Quest Developer Hub / `adb pull`), kiểm tra đủ file, xoá trên kính sau khi chép, và nối với dữ liệu phiếu giấy.
- **Done when:** Một người chưa làm bao giờ làm theo hướng dẫn chép được log của 2 phiên thử về máy; script gộp ra một bảng mỗi người một dòng (hoặc mỗi scene một dòng) kèm cột từ file phiếu giấy mẫu.
- **Inspector work expected:** no
- **Depends on:** #28 (cần `summary.csv`); quyết định thời hạn lưu/xoá (spec mục 9)
- **Status:** todo
