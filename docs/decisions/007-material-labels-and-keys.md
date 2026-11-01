# Quyết định 007: Bỏ GG/VPE như "loại kiện": một mẫu nhãn và khoá vật tư trên mọi carton; ConceptScene dạy "đọc nhãn rồi tính số carton"

- **Backlog:** #17 (tách: phần ConceptScene thành #45, gộp với #32; mục Menu sang #18)
- **Trạng thái:** accepted (phương án B, 2026-11-01), theo giả định A2 (nhóm trả lời, chờ người hướng dẫn)
- **Ngày:** 2026-11-01
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, german-reviewer, data-analyst → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-01):** chấp nhận B, tách #17 (dữ liệu và nhãn) với #45 (ConceptScene, gộp #32), thứ tự #18 → #17 → #6 → #14 → #8; #45 chạy song song được với #6.

## Bài toán
App dạy GG và VPE như hai loại kiện, với quy tắc "chỉ VPE được mở". Tài liệu coi VPE là một con số (số chiếc trong một carton hoặc túi, `Kommissionierung` slide 8) và không có chữ "GG" ở đâu. Nhãn, dữ liệu và cách đếm lệch nhau giữa các scene: Basic ghi nhãn "VPE…" nhưng đếm theo tag `GGPackage`; `GGB` là Screw B ở Mixed nhưng là Ring D ở Complex; Ring D có VPE 20 ở Concept/Mixed nhưng 25 ở Complex.

## Xung đột chính
- Dữ liệu vật tư trên `PackageData` (rẻ) hay ScriptableObject (một nguồn, phải gán lại mọi carton).
- Quiz Concept viết lại thành câu vận dụng hay chỉ đổi chữ.
- Nhãn ghi `VPE` (slide 8) hay `Verp.-Einh.` (chữ trên ô WMS; VPE = Verp.-Einh. mới là suy ra của spec).
- Thứ tự với #18 (dọn Basic, Menu) và #32 (TMP phản hồi quiz Concept).

## Phương án đã chọn: B
1. **Mẫu nhãn chung ở cả 4 scene**, 3 dòng:
   - tên vật tư dạng `Bez.` bằng tiếng Đức (dữ liệu giả, B10);
   - `Mat.-Nr: <6–7 chữ số giả>` (không có dấu chấm sau "Nr", như trên WMS);
   - `VPE: <n> …`, đơn vị (`ST` hay `Stück`) do `localize-strings` / german-reviewer chốt.
   Ghi `VPE`, không ghi `Verp.-Einh.`, cho tới khi người hướng dẫn trả lời (A9). Không còn chữ "GG" ở chỗ nào trainee thấy. Carton đã mở dùng cùng nhãn với carton của vật tư đó.
2. **Một vật tư, một bộ dữ liệu trên toàn khoá học:** cùng khoá, Mat.-Nr, tên, VPE. Ring D có VPE **25** ở mọi scene (A5, #16); Concept và Mixed đang ghi 20, phải sửa. "Mỗi vật tư một cỡ VPE" là đơn giản hoá được phép.
3. **Dữ liệu trên `PackageData`:** thêm khoá vật tư, Mat.-Nr hiển thị, VPE (`[SerializeField] private` + property; đặt tên sao cho sau này chuyển sang ScriptableObject được bằng `[FormerlySerializedAs]`).
   - Mọi carton, túi và chiếc lẻ ở Basic, Mixed, Complex và Concept đều có `PackageData`.
   - Chiếc lẻ `LooseItemDisplay` mang khoá của carton cha (`ring_d`).
   - Khoá chỉ theo vật tư (không gộp VPE), snake_case ASCII, giống nhau ở mọi scene; không suy ra từ số int của `PackageType`, tên GameObject hay nhãn.
   - Thiếu khoá → `Debug.LogError` nêu tên object.
   - Không xoá hay đánh số lại giá trị nào của enum `PackageType` (serialize theo int).
   - Nhãn vẫn là chữ tĩnh; test so chữ nhãn với `PackageData` bằng `scene_inventory.py`.
4. **#17 không đổi cách đếm.** Basic vẫn đếm theo tag `GGPackage` và `BoxInfo.packageSize` (`PackageData.vpe` là bản sao tạm). Mixed/Complex giữ nguyên logic. Chuyển counter sang khoá vật tư và `WRONG_MATERIAL` là việc của #6 (quyết định 005, 006).
5. **ConceptScene (#45, gộp #32):**
   - Panel giải thích Mat.-Nr và VPE, mỗi lần một khối ngắn. Bỏ `TXT_Rule` hiện sẵn và dòng `= N packages` của `SampleFormulaDisplay`.
   - Thả kiện thì thông tin kiện biến mất (D3); phản hồi quiz ở ô riêng, không bị ghi đè (#32).
   - Quiz có ít nhất 2 câu vận dụng, dùng số khác với ví dụ: "Soll X, VPE Y: wie viele Kartons?" (chia hết) và "welcher Karton gehört zu Mat.-Nr …".
   - Mỗi câu có ID ổn định, mỗi đáp án có khoá ngắn không phụ thuộc ngôn ngữ.
   - Phản hồi nói lý do; *Sie*; chuỗi qua `localize-strings`.
   - Không câu nào hỏi kiện nào được mở.
6. **Log:** #17 và #45 không ghi, không đổi log (quiz hiện chưa gọi `ResearchLogger`). Spec §4.2, `make_fake_data.R`, `analyse.R`, cách nối ID câu hỏi với kỹ năng, xoá `WRONG_TYPE` (sau khi A2 được chốt): #28, data-analyst duyệt.
7. **Phạm vi và thứ tự:**
   - **#17:** dữ liệu vật tư, khoá, nhãn ở Basic, Mixed, Complex và trên 3 kiện của Concept; câu mục tiêu trên WelcomeScene.
   - **#45 (mới):** quiz và panel ConceptScene, gộp #32.
   - **#18:** đổi mục Menu "GG vs VPE" / "2. Package Types" thành `2. Grundlagen` ở cả 5 scene (D1); quyết object sót lại ở Basic (xoá hay giữ làm vật gây nhiễu). Nếu giữ, #17 gắn khoá cho chúng và #28 mới thêm `BasicPickingScene,check_material`.
   - Thứ tự: #18 → #17 → #6 → #14 → #8; #45 song song được với #6.

- **Được:** bỏ quy tắc dạy sai; Concept có giải thích, minh hoạ, luyện và kiểm tra cho đúng kỹ năng Basic/Mixed/Complex dùng; nhãn đọc giống WMS, cùng một mẫu; #6 có khoá trên carton, không cần bảng tạm; hợp đồng log đổi một lần ở #28.
- **Mất / chấp nhận:** công L, nhiều chỉnh tay trong Inspector (giảm rủi ro bằng cách chia bước, mỗi bước có regression); VPE 50 lặp tạm ở `BoxInfo` và `PackageData`, nhãn là bản sao tĩnh (hết lặp ở #6/#10); giữ `Info Panel` thay vì callout gắn kiện, nên split attention chỉ giảm một phần; phần dư (chia có dư) không dạy ở Concept mà để #19 ở Complex.
- **Bị bác:**
  - Giữ câu "kiện nào được mở?": dạy sai quy tắc (mở phụ thuộc số lượng, Detail Description 5.12; quy tắc cấm mở chỉ cho túi 10 cái hơn 2 thành phần, 4.1).
  - Chỉ đổi chữ, bỏ quiz: Concept mất phần luyện/kiểm tra.
  - Quiz Anbruch/Modul: app không mô phỏng Modul.
  - Quy tắc túi 10 cái ở Concept: không túi nào trong app đúng điều kiện; hỏi người hướng dẫn (A11).
  - Callout gắn kiện: hoãn tới khi đo được world scale (#36).
  - ScriptableObject `MaterialDefinition` ngay: để #10, như 005 đã hoãn.
  - Đổi `skill_map` / `quiz_answer.selected` ngay: quiz chưa ghi log, để #28.
  - `Verp.-Einh.` trên nhãn: chưa có căn cứ nhãn thật ghi như vậy.
- **Nếu giả định sai:**
  - Người hướng dẫn bác A2: khoá và mẫu nhãn vẫn đúng; thêm một dòng trên nhãn và bật lại nhánh `WRONG_TYPE` (006).
  - Nhãn thật ghi `Verp.-Einh.` hoặc VPE thật luôn là 1: chỉ đổi chữ/số trong dữ liệu.
  - Quy tắc túi 10 cái quan trọng: thêm một bài sau, không đổi dữ liệu.

## Câu hỏi còn mở
- Người hướng dẫn: A2 (nghĩa "GG"), A9 (nhãn ghi VPE hay Verp.-Einh.; VPE trên ảnh WMS luôn là 1), A10 (tên túi của nhà cung cấp; nghĩa ô WMS `Karton:`), A11 (quy tắc túi 10 cái ở phần Grundlagen).
- #18: giữ hay xoá object sót lại ở Basic.

## Bằng chứng (đã kiểm)
`PackageQuizController.cs:34–55` (VPE là đáp án đúng, không log), `PackageInfoOnGrab.cs`, `PackageType.cs`, scene inventory Concept/Basic/Mixed/Complex (nhãn, `TXT_Rule`, menu "2. Package Types"), `_extracted/Kommissionierung/text.md` slide 8, Detail Description 4.1 và 5.12, `wms-specs/kommissionierung-dialog.md` (`Mat.-Nr:`, `Karton:`, `Verp.-Einh.` luôn 1, VPE↔Verp.-Einh. là suy ra), `docs/i18n/glossary.csv`, `questions-for-instructor.md` (A2/A3/A5/A6, B10, D3, D5), backlog #16/#18/#32/#36, quyết định 005, 006.
