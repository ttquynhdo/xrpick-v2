# Localization (DE / EN)

Người dùng cuối là người Đức bản ngữ. Đội phát triển dùng tiếng Anh.

- **de** là ngôn ngữ mặc định của app. **en** là chế độ cho đội phát triển: dùng để test và hiểu luồng.
- **Tiếng Đức trong tài liệu requirements là bản gốc của thuật ngữ.** Tiếng Anh chỉ là bản giải thích.

## Quy ước tiếng Đức (Styleguide)
- **Xưng hô:** luôn dùng **Sie** (trang trọng). Viết `Bitte legen Sie …`, không viết `Leg …`.
- **Câu hướng dẫn:** ngắn, câu mệnh lệnh lịch sự, giọng một trainer trong kho. Tránh văn sách giáo khoa và kiểu dịch máy.
- **Thuật ngữ:** chỉ dùng từ trong [glossary.csv](glossary.csv). Muốn thêm từ mới thì thêm vào glossary trước, với trạng thái `proposed`.
- **Nhãn WMS** (`do_not_translate = yes`, ví dụ `Soll`, `Ist`, `Rest`, `F6 Bestätigen`): giữ **nguyên tiếng Đức ở mọi chế độ ngôn ngữ**, vì trainee phải nhận ra đúng chữ trên màn hình thật. Ở chế độ en được phép thêm chú thích, ví dụ `Soll (target qty)`.
- **Chính tả:** viết `ä ö ü ß` thật, không thay bằng `ae oe ue ss`. Danh từ viết hoa.
- **Số:** dấu phẩy thập phân (`0,4 kg`), đơn vị đặt sau số và cách một khoảng trắng (`5 ST`, `18 kg`).
- **Độ dài:** tiếng Đức thường dài hơn tiếng Anh khoảng 30%. Phải kiểm tra chữ có tràn khỏi panel VR không.

## Bảng dịch trong Unity (#11)
- Chuỗi nằm trong String Table Collection `UI` (`Assets/Localization/Tables/`, locale `de` và `en`). Code lấy chuỗi qua `Loc.Get(key, ("name", value))`, text trong scene dùng `LocalizeStringEvent`.
- **Không bật „Smart“ cho entry nào.** `Loc.Get` tự điền placeholder `{name}`. Nếu một entry bật Smart thì Smart Format sẽ báo lỗi (`FormattingException`) vì không nhận được argument.

## Glossary
Mỗi dòng trong `glossary.csv` là một thuật ngữ:
- `article` ghi giống (der/die/das).
- `status` là `proposed` cho tới khi người Đức bản ngữ duyệt, sau đó đổi thành `confirmed`.
- Định nghĩa viết bằng lời của đội, **không trích nguyên văn tài liệu bảo mật**. Cột `source` chỉ ghi tên tài liệu và slide.

## Review bởi người bản ngữ
Người hướng dẫn hoặc một người Đức bản ngữ sẽ duyệt **file export EN–DE**. Quy trình nằm trong skill `localize-strings` (phần "Review export"): `python -I docs/i18n/review_tool.py export` tạo file `docs/i18n/review/review-<ngày>.csv` + hướng dẫn tiếng Đức; `python -I docs/i18n/review_tool.py import <file> --reviewer <tên>` đưa sửa đổi vào `strings-todo.csv`. Chuỗi chưa được duyệt vẫn dùng được trong prototype, nhưng không được coi là bản cuối.
