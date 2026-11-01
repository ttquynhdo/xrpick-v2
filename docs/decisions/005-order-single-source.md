# Quyết định 005: Một nguồn duy nhất cho số lượng đơn, khai báo theo đơn vị WMS (Soll + VPE)

- **Backlog:** #6
- **Trạng thái:** accepted (phương án B, 2026-11-01)
- **Ngày:** 2026-11-01
- **Ý kiến đã xét:** requirements-analyst, unity-reviewer, instructional-design-reviewer, data-analyst → decision-synthesizer (không hỏi xr-ux-reviewer và german-reviewer: #6 không đổi bố cục, chuỗi mới đi qua `localize-strings`)
- **Lựa chọn của người dùng (2026-11-01):** chấp nhận B. Các dòng feedback `x / N` đang có được giữ làm cầu nối tới #14, với N lấy từ nguồn; không thêm số đích mới.

## Bài toán
Ở Mixed và Complex, đơn (Soll) bị chép ở nhiều chỗ: field trên counter, literal trong validator (`gga == 2 && vpe == 0 && ggb == 2`, `GGBCount == 2 && … LooseItemCount == 5`), số trong feedback (`"/ 2"`, `"/ 5"`, `"2 GG"`), và text WMS tĩnh trong scene (`100 pcs`, `40 pcs`, `55 pcs`). Đổi đơn ở một chỗ thì các chỗ khác có thể không khớp. Basic đã dùng một nguồn là `CalcForWMS.necessarySize`, tính bằng chiếc.

## Xung đột chính
- **Đơn vị:** giữ `requiredGGA/GGB/VPE` tính bằng kiện (ít sửa) hay khai báo Soll bằng chiếc + VPE như WMS hiển thị. Theo tài liệu, Soll tính bằng chiếc và VPE là một con số: `Kommissionierung.pptx` slide 7–8.
- **Nơi đặt dữ liệu:** field trên counter (rẻ, không có reference mới) hay ScriptableObject `OrderDefinition` (một dạng dữ liệu cho #10/#21/#28, nhưng phải nối reference ở 3 scene và đụng vào Basic/decision 001).
- **Text WMS:** để sau hay sinh ngay từ cùng nguồn. Nếu để sau thì nội dung được đánh giá lệch với panel khi đơn đổi.

## Phương án đã chọn: B
1. **Dòng đơn trên counter (Mixed, Complex):** mỗi counter giữ một danh sách dòng đơn `[Serializable]` thuần, mỗi dòng gồm {loại `PackageType`, Soll bằng chiếc, VPE (chiếc/kiện)}. Số carton = Soll ÷ VPE, số chiếc lẻ = phần dư; cả hai được **tính ra**, không khai báo.
2. **Rule thuần C#:** một hàm static nhận số đếm và các dòng đơn, trả kết quả. Không còn literal của đơn trong 4 script Mixed/Complex. Rule phải giữ:
   - Complex chấm theo **thành phần** (carton nguyên + chiếc lẻ), không chỉ theo tổng số chiếc.
   - Loại mà scene có đếm nhưng không thuộc đơn thì đơn sai (Mixed: VPEFull; Complex: GGA và Other). Điều kiện `OtherCount == 0` không được mất (#3, A3/A6).
   - So đúng số lượng: thừa cũng là sai.
3. **Validator và feedback** chỉ đọc từ counter. Các số `x / N` đang có lấy N từ nguồn, không thêm số đích mới (#14 sẽ bỏ chúng). Bộ đếm trong thùng vẫn chỉ hiện số đã đặt (#15).
4. **Text WMS** (`[Info Panel]/TXT_Description`) của Mixed/Complex được sinh từ dòng đơn lúc chạy. Giữ nhãn và đơn vị như hiện có, không đặt nhãn mới. Chuỗi mới hoặc đã đổi đi qua `localize-strings` (key + EN + DE trong `docs/i18n/strings-todo.csv`).
5. **Khoá dữ liệu** là `PackageType`, ổn định và không phụ thuộc ngôn ngữ. Mỗi counter biết nó đếm những loại nào, nhờ đó #28 phân biệt được `0` (loại cấm) và `null` (không áp dụng).
6. **Basic giữ nguyên** `necessarySize`: đã là nguồn duy nhất, tính bằng chiếc; Rest và decision 001 không bị đụng tới.
7. **Field cũ:** `requiredGGA/GGB/VPE` (kiện) đổi nghĩa thành chiếc, nên dùng **field mới**, không dùng `[FormerlySerializedAs]` để mang số kiện sang Soll. Phải nhập lại giá trị trong Inspector và lưu scene: Mixed 100/50 (Cable Connector A) và 40/20 (Screw B); Complex 55/25 (Ring D). VPE của dòng đơn Complex thay cho `piecesPerGGB`; `piecesPerGGA` ở lại vì GGA không thuộc đơn, chỉ dùng tính Ist.

- **Được:** đơn khai báo đúng như trainee đọc trên WMS; validator, feedback và panel cùng đọc một chỗ; không có asset hay reference mới; Basic và decision 001 không đổi; rule tách khỏi Unity, sau này test được.
- **Mất / chấp nhận:** Mixed/Complex chưa cùng dạng với Basic, nên #28 viết hai ánh xạ sang `required`; phải nhập lại số trong Inspector ở 2 scene; nhãn trên carton (`50 pcs`, `20 pcs`, `25 pcs`) vẫn là bản sao tĩnh của VPE (thuộc #17/#10), nên chỉ việc đổi Soll được kiểm, không kiểm đổi VPE; text WMS sinh lúc chạy có thể bị thay khi làm #10; rule chưa có test EditMode vì test asmdef chưa thấy được `Assembly-CSharp` (coding-conventions §10), tạm kiểm bằng UnityMCP.
- **Bị bác:**
  - Field tính bằng kiện, text WMS gõ tay: đổi số kiện thì panel vẫn sai.
  - Chỉ sửa validator, text WMS làm sau: không đạt mục tiêu panel theo cùng nguồn.
  - ScriptableObject ngay bây giờ: đúng hướng nhưng để #10, khi panel chung cần dữ liệu.
  - Đơn đọc từ JSON lúc chạy: R không biết đơn nào được dùng.
  - Đưa Basic vào cùng mô hình: không cần.
- **Nếu giả định sai:**
  - A5 ra số khác: chỉ đổi số trong Inspector.
  - Người hướng dẫn chấp nhận cách lấy khác cho cùng tổng (A7): chỉ sửa rule ở một chỗ.
  - A3/A6 ra "chỉ cảnh báo": đổi nhánh "loại không thuộc đơn" trong rule.
  - #10 cần ScriptableObject: bọc danh sách dòng đơn vào asset; validator và rule không đổi.

- **Bổ sung (2026-11-01, decision 006):** mỗi dòng đơn có thêm một **khoá vật tư** (do #17 đặt lên carton); `PackageType` chỉ còn dùng để nhận diện vật thể trong scene cho tới #17. Rule đặt trong asmdef `XRPick.Orders` nên có test EditMode (xoá điểm "chưa có test" ở trên). Thứ tự làm đổi thành #17 → #6. Không đảo ngược các mục 1–7.

## Câu hỏi còn mở
- Người hướng dẫn: A7 (cách lấy khác cho cùng tổng số chiếc), xác nhận A5 và A3/A6. Không chặn #6.
- `PackageType.GGB` là Screw B ở Mixed nhưng là Ring D ở Complex, nên khoá chỉ ổn định trong từng scene. #28 và #17 cần lưu ý.
- `order_id` trong log: để #28 quyết.

## Bằng chứng (đã kiểm)
`MixPickingContainer.cs:10-12, 99-103` (field 2/2/0 và `IsOrderCorrect()` không được gọi), `MixPickingValidator.cs:74-105`, `PiecesContainerCounter.cs:8-15, 112-115`, `PiecesOrderValidator.cs:88-89`, `CalcForWMS.cs:31, 106`, scene inventory Mixed/Complex (`TXT_Description`; Mixed GGA×6, GGB×3, VPEFull×1; Complex GGA×6, GGB×3, Loose×13), `docs/research-logging-spec.md:74, 145, 150` (`required` dùng key cố định, `null` khi không áp dụng), `docs/coding-conventions.md` §5, §10. Nguồn yêu cầu theo requirements-analyst: `Kommissionierung.pptx` slide 6–8 và spec dialog WMS.
