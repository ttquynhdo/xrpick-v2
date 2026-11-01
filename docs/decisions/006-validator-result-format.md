# Quyết định 006: Định dạng kết quả chung cho validator (đơn hàng và quiz), theo mô hình A2

- **Backlog:** #14, #28 (chốt mục đang chờ trong `docs/coding-conventions.md` §5); làm trước code của #6 (quyết định 005); phải tương thích với #17
- **Trạng thái:** accepted (B + M2 + H1, 2026-11-01)
- **Ngày:** 2026-11-01
- **Ý kiến đã xét:** unity-reviewer, data-analyst, requirements-analyst, instructional-design-reviewer → decision-synthesizer (bản sửa 2, sau góp ý của người dùng về A2)
- **Lựa chọn của người dùng (2026-11-01):**
  - Chấp nhận B (asmdef `XRPick.Orders`), M2 (lỗi theo dòng đơn / khoá vật tư), H1 (màn hình hiện lỗi gốc).
  - Đổi hợp đồng log trong #28, data-analyst duyệt dạng cụ thể.
  - Khoá vật tư là mã ngắn do dự án đặt (ví dụ `connector_a`, `screw_b`, `ring_d`, `training_b`), không hiện cho trainee, tách khỏi Mat.-Nr giả mà #17/#10 hiển thị.
  - Chấp nhận Basic đổi luật (vật tư lạ trong thùng = sai) trước đợt thu dữ liệu thật đầu tiên.
  - Không thiết kế tình huống trainee đặt nguyên carton đã mở vào thùng.
  - Làm #17 trước #6 (thứ tự mới: #17 → #6 → #14 → #8).

## Bài toán
Các validator (Basic, Mixed, Complex) và quiz Concept không trả về gì, chỉ ghi thẳng chữ và màu lên TMP. Cần một dạng kết quả duy nhất để suy ra cùng lúc đúng/sai, feedback cho trainee (#14: nêu loại lỗi và lý do, không in số đích) và `error_codes` trong log (#28), để màn hình và log không thể lệch nhau. Dạng kết quả phải đúng theo A2 (nhóm trả lời, chờ người hướng dẫn): không còn "loại kiện GG/VPE"; kiện là carton của một mã vật tư, VPE là một con số.

## Xung đột chính
- **Nơi đặt rule:** trong `XRPick.Research` (test được ngay, nhưng logic chấm bài nằm trong assembly nghiên cứu), trong `Assembly-CSharp` (rẻ, không test được, khoá dễ trôi) hay asmdef riêng.
- **`item` là gì:** khoá cố định của spec cũ (`gg/vpe/gga/ggb`, mô hình GG/VPE) hay khoá vật tư của dòng đơn (A2).
- **`WRONG_TYPE`:** kỹ năng "phân biệt GG/VPE" (instructional-design) hay không còn tồn tại theo A2.
- **Màn hình:** hiện tất cả lỗi hay chỉ lỗi gốc.

## Phương án đã chọn: B + M2 + H1
1. **Mã lỗi** (từ vựng cố định, khớp đúng chữ với `analysis/skill_map.csv`):
   - Rule đơn hàng phát: `EMPTY`, `WRONG_MATERIAL`, `TOO_FEW`, `TOO_MANY`, `MISSING_LOOSE`, `EXTRA_LOOSE`. Quiz: `NO_ANSWER`, `WRONG_ANSWER`.
   - Mọi carton không thuộc đơn (VPE trong Basic, VPEFull trong Mixed, GGA/Other trong Complex) là `WRONG_MATERIAL` (A3/A6).
   - `WRONG_TYPE` chỉ **giữ chỗ** trong enum và `skill_map.csv`, không rule nào phát. Chỉ bật lại nếu người hướng dẫn bác A2. Nếu A2 được xác nhận, #17 có thể xoá hẳn mã này.
2. **Thứ tự ưu tiên** (khai báo ở một chỗ): `EMPTY > WRONG_MATERIAL > TOO_FEW/TOO_MANY > MISSING_LOOSE/EXTRA_LOOSE` (đọc vật tư → tính số carton → phần lẻ).
3. **Dạng kết quả** (không chứa chữ, không phụ thuộc `attempt_no`):
   - Danh sách lỗi có thứ tự, mỗi lỗi `{Code, Item?}`. `Code` là enum. `Item` là khoá vật tư (chuỗi ổn định, không phụ thuộc ngôn ngữ), `other` khi không xác định được vật tư, `null` với `EMPTY` và mã quiz.
   - Mỗi dòng đơn: khoá vật tư, Soll (chiếc), VPE, số carton nguyên và chiếc lẻ cần (tính ra, quyết định 005), số carton nguyên và chiếc lẻ đã đặt. Phần lẻ là `null` khi scene không đếm chiếc lẻ.
   - Danh sách vật tư lạ trong thùng, mỗi mục `{khoá, số carton}`. Vật tư không có dòng đơn là cấm.
   - `Correct` được **tính** từ "danh sách lỗi rỗng".
4. **Rule:** tất định; kiểm mọi kỹ năng của scene trong mỗi lần bấm, không dừng ở lỗi đầu tiên (mẫu số của `analyse.R`); `EMPTY` đứng một mình; Complex chấm theo thành phần (A7 còn mở, nếu đổi chỉ sửa rule).
5. **Nơi đặt (B):** asmdef thuần C# `XRPick.Orders` (không tham chiếu gì của dự án), có test asmdef riêng. Bảng tên mã cho log được test so với `skill_map.csv`. `PackageType` ở yên chỗ cũ, không chuyển file có GUID.
6. **Khoá vật tư:** mã ngắn do dự án đặt. Vì #17 làm trước #6, khoá nằm trên dữ liệu của carton (do quyết định của #17 chốt), nên không cần bảng tạm `PackageType` → khoá trên counter. Nếu #17 chưa đặt được khoá lên carton thì mới dùng bảng tạm theo scene.
7. **Màn hình (H1):** hiện lỗi đầu tiên (câu lý do + câu việc cần làm), thêm một dòng chung nếu còn lỗi khác. Không in số đích. Tên vật tư là tên trên WMS/nhãn, lấy qua `Loc.Get`, không phải khoá log. Chuỗi mới đi qua `localize-strings`.
8. **Log** (thuộc #28, data-analyst duyệt, trong cùng một thay đổi):
   - Ghi toàn bộ danh sách lỗi; thứ tự có nghĩa (phần tử đầu là lỗi đã hiện cho trainee), nên chưa cần `feedback_key`.
   - Spec §4.2/§6: `container`/`required` với khoá cố định thay bằng danh sách dòng đơn `{item, soll, vpe, required_full, required_loose, placed_full, placed_loose}` và danh sách vật tư lạ `{item, count}`; `error_codes.item` là khoá vật tư; §4.3 ghi `WRONG_TYPE` là giữ chỗ; spec dòng 78 (Basic chỉ dùng quy tắc tổng số chiếc) sửa lại.
   - Tăng `schema_version`; viết lại phần đơn trong `make_fake_data.R`; `selftest.R` xanh; `scene_skills.csv` thêm `BasicPickingScene,check_material`. Chưa có log thật nào có `confirm_pressed`, nên không phải chuyển dữ liệu cũ.
9. **Quiz:** dùng chung enum `Code`, không dùng phần số đếm; `quiz_answer` vẫn là event riêng (`selected` đã ghi lựa chọn).

- **Được:** màn hình, `correct` và `error_codes` đọc cùng một object; đúng theo A2 mà vẫn an toàn nếu A2 bị bác (bật lại một nhánh rule); hợp đồng log đổi một lần, trước khi có dữ liệu thật; hết vấn đề "GGB khác nghĩa giữa các scene"; rule, bất biến `correct == rỗng` và từ vựng mã có test EditMode (xoá điểm "Mất" về test của quyết định 005); assembly nghiên cứu không chứa logic dạy học; mỗi panel một ý.
- **Mất / chấp nhận:**
  - Spec §6 đang ghi "fixed now" bị đổi; `make_fake_data.R` viết lại phần đơn.
  - Thêm một asmdef và một test asmdef.
  - H1 có thể làm trainee phải xác nhận thêm vài lần.
  - **Đơn giản hoá có chủ ý:** thiếu hàng bị báo `TOO_FEW` (sai), trong khi WMS thật yêu cầu xác nhận Mindermenge (`Kommissionierung` slide 7; D2 để ngoài phạm vi).
  - **Basic đổi luật:** carton của vật tư không thuộc đơn trong thùng (ví dụ carton mở "Ring D" cạnh các carton "TRAINING-MATERIAL B") → sai, `WRONG_MATERIAL`. Counter Basic phải nhìn thấy cả carton không có tag `GGPackage` (hiện `CalcForWMS.cs:41, 92` chỉ cộng `GGPackage`).
- **Bị bác:**
  - Đặt trong `XRPick.Research` (A): phương án thay thế chấp nhận được; API giữ nguyên nếu muốn chuyển. Ý "mở rộng `ErrorDecision.cs`" không áp dụng (file đó gộp lỗi Unity).
  - Đặt trong `Assembly-CSharp`, `item` = `PackageType`: không có test cho hợp đồng C#↔R; `PackageType` khác nghĩa giữa scene và sẽ bị #17 đổi.
  - Khoá cố định của spec cũ, `WRONG_TYPE` = GG/VPE: dạy và chấm một khác biệt mà A2 bỏ; hợp đồng log phải đổi lần thứ hai sau #17.
  - Severity cho `TOO_FEW`: D2 coi thiếu là sai; thêm sau nếu mô phỏng Mindermenge.
  - `feedback_key`, gợi ý tăng dần (H3): hoãn; thêm được sau mà không đổi format kết quả.
  - Hiện tất cả lỗi (H2): lỗi hệ quả gây hiểu nhầm, gần như lộ đáp án.
  - `item` = đáp án đã chọn cho quiz: `quiz_answer.selected` đã có.
- **Nếu giả định sai:**
  - Người hướng dẫn bác A2: bật nhánh `WRONG_TYPE` trong rule, đặt nó vào thứ tự ưu tiên; format và log không đổi.
  - A3/A6 ra "chỉ cảnh báo": sửa nhánh vật tư lạ trong rule.
  - A7 chấp nhận cách chia khác: sửa rule phần lẻ.
  - Rule đổi sau khi đã thu dữ liệu thật: tăng `schema_version`.

## Câu hỏi còn mở
- Người hướng dẫn: xác nhận A2/A3/A6 (không chặn: phương án an toàn với cả hai câu trả lời); A7; A8 (Ist > Soll trên WMS thật, không chặn).
- data-analyst (khi làm #28): duyệt dạng cụ thể của dòng đơn trong log.

## Bằng chứng (đã kiểm)
`docs/requirements/questions-for-instructor.md` (A2, A3, A6, A7), `docs/decisions/005-order-single-source.md`, `docs/research-logging-spec.md:74–96, 138–158` (khoá cố định, `WRONG_TYPE` theo mô hình cũ, dòng 78), `analysis/analyse.R` (chỉ đọc `code`; mẫu số `confirm_count` dòng 831–848; mã lạ bị cảnh báo dòng 519–525), `analysis/make_fake_data.R:130–211, 745–755`, `analysis/skill_map.csv`, `analysis/scene_skills.csv`, `Assets/Scripts/Research/ErrorDecision.cs`, `Assets/Tests/EditMode/Research/XRPick.Research.Tests.asmdef`, `Assets/Scripts/PackageType.cs` (enum thường), `CalcForWMS.cs:41, 92`, `OrderConfirm.cs:38`, `PiecesOrderValidator.cs:88–89`, scene inventory Basic (nhãn "VPE / TRAINING-MATERIAL B / 50 Stück", carton mở "VPE / Ring D"). Theo requirements-analyst (chưa kiểm lại): `Kommissionierung` slide 7 (Mindermenge), `Detail Description` 5.12 (Anbruch).
