# Quyết định 018: Bảng tổng quan EKZ ở ConceptScene, hiện từ bước tour „Empf Kz 000“

- **Backlog:** #57
- **Trạng thái:** accepted
- **Ngày:** 2026-11-03
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, german-reviewer → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-03):** phương án B1. Câu *Done when* „mỗi cấp một câu nói cách đặt các Teile“ được sửa: EKZ 8/9 nói về Anweisung.

## Bài toán
ConceptScene chỉ dạy và kiểm tra EKZ 0 và EKZ 3; bước tour `empf_kz_000` kết thúc bằng „Es gibt weitere EKZ.“, không nêu cấp nào khác. Người xem video demo (2026-11-03) muốn có danh sách các cấp EKZ và ý nghĩa của chúng. Mục tiêu: học xong Concept, trainee biết EKZ có các cấp 0, 2, 3, 4, 5, 6, 8, 9, mỗi cấp nghĩa là gì, và chỉ 0 và 3 có trong bài luyện. Ràng buộc: tour vẫn 10 bước, quiz vẫn 4 câu, chữ có key DE/EN (Sie), không lấy gì từ `docs/requirements/`, không vẽ lên dialog WMS, không thêm tải nhận thức thừa.

## Tài liệu
Kommissionierung slide 8 (bảng EKZ-Vorgabe, là ảnh): mỗi cấp có hàng Piktogramm / Transport / Handhabung / Lagerung / Beispiel. Diễn giải hàng Handhabung:
- 0: có thể là hàng rời, đổ vào được. 2: đặt bằng tay, mỗi lần nhiều cái.
- 3: như 0, các Teile đặt từng cái và đúng hướng.
- 4 / 5 / 6: như 3, các Teile không được chạm nhau trong một / hai / không chiều nào, có lớp lót phù hợp ngăn cách. Nghĩa chính xác của „Ebene“ chưa rõ (xem câu hỏi).
- 8: Teile có bảo vệ ESD, theo chỉ dẫn riêng, không mở khi chưa có chỉ dẫn. 9: đóng gói đặc biệt, không mở hay xử lý khi chưa có chỉ dẫn.

Bảng không có EKZ 1 và 7, tài liệu không giải thích vì sao. Nguồn chính thức CA0050 („EKZ Sensitivity Code“) chỉ được nhắc tên, không có trong thư mục. Không tìm thấy slide nói „EKZ nên được luyện, nếu không thì phải có danh sách“. Tài liệu cho phép bảng danh sách, đánh dấu cấp có trong bài luyện, và trỏ bước tour tới bảng; không bắt buộc và không mâu thuẫn với điều nào.

## Xung đột giữa các đề xuất
| Vấn đề | Một bên | Bên kia | Đánh đổi |
|---|---|---|---|
| Khi nào bảng hiện | unity A, ID PA1: luôn hiện | ID PA2, UX PA1: từ bước 8 rồi ở lại; unity B: chỉ đúng bước 8 | Rẻ ↔ thuật ngữ trước khi được dạy; một bước ↔ không xem lại khi làm quiz |
| Sửa code | unity: không cần | ID, UX: hook „đã mở thì ở lại“, hiệu ứng grow | Không regression ↔ bảng còn sau „Von vorn“ |
| Nhãn cấp | german: „EKZ n“ | UX: kèm mã `000`/`003` | Nối với ô `Empf Kz` ↔ thêm ký hiệu mỗi dòng |
| EKZ 8/9 | requirements, ID, german: quy tắc „nicht ohne Anweisung öffnen“ | *Done when* cũ: „cách đặt các Teile“ | Phải sửa *Done when* để không dạy sai |
| Thêm câu quiz | cả 5: không | — | — |

## Các phương án
- **0. Giữ „Es gibt weitere EKZ.“:** không đạt mục tiêu.
- **A. Bảng tĩnh luôn hiện:** không code, nhưng hiện suốt 7 bước đầu trước khi EKZ được dạy.
- **B1. Bảng tĩnh trong `markers` của 3 bước cuối:** chọn.
- **B2. B1 + bảng ở lại mãi sau lần đầu (hook trong `WmsFieldTour`, có thể grow):** dự phòng.
- **C. Thêm bước tour liệt kê các cấp:** vượt 140 ký tự và ô 320 × 160 px, phá „tour 10 bước“, mất khi sang bước. Loại.

## Quyết định: phương án B1
- **Bảng:** một world-space canvas tĩnh `EKZ-Übersicht`, copy cấu trúc của `Sample Description`, không script, không raycast target.
- **Vị trí:** sát bên phải bảng tour, mục tiêu tâm ≈(1.00, 1.38, 0.72) (≈ +54° từ mắt, ≈ 1,25 m), xoay mặt về phía mắt, mép dưới cao hơn mép trên canvas `Menu`. Vị trí cuối cùng chốt bằng ảnh Play mode.
- **Cách hiện:** bảng được thêm vào `steps[].markers` của 3 bước `empf_kz_000`, `not_used`, `material_flow`. Bước `empf_kz_000` có thêm một mũi tên cam (cùng kiểu marker đang dùng) chỉ từ bảng tour sang bảng mới. `ShowStep` tắt mọi marker rồi bật marker của bước hiện tại (`WmsFieldTour.cs:98–103`), nên bảng dùng chung 3 bước hiện liền. Sau „Von vorn“ bảng tắt tới khi lại tới bước 8.
- **Nội dung:** 8 dòng theo thứ tự số, chia 4 nhóm theo logic cộng dồn của slide 8: 0, 2 | 3 | 4, 5, 6 | 8, 9. Mỗi dòng: nhãn „EKZ n“, pictogram tự dựng bằng UI Image theo một kiểu chung (hộp nhìn từ trên, chỉ đổi cách xếp bên trong; ESD và tờ chỉ dẫn tự vẽ), một dòng chữ ngắn dạng mô tả (từ khóa hoặc câu ngắn, không mệnh lệnh „Legen Sie …“).
  - EKZ 0 và EKZ 3 dùng đúng cách nói của tour và quiz („ungeordnet … einfüllen“, „einzeln und geordnet einlegen“).
  - EKZ 4/5/6 nói các Teile **không được chạm nhau** (theo slide; dòng glossary cũ „touching“ được sửa).
  - EKZ 8/9 nói không mở hay xử lý khi chưa có Anweisung.
  - EKZ 0 và EKZ 3 có khung và nhãn chữ „In der Übung“ (khớp tiêu đề tour „Nicht in der Übung“). Không ✓ (font thiếu glyph), không dùng màu #0391DD (nghĩa „bước hiện tại“, #40), không cấp nào chỉ phân biệt bằng màu.
- **Tour:** `concept.tour.empf_kz_000.body` thay „Es gibt weitere EKZ.“ bằng một câu ngắn chỉ tới bảng, cả body ≤ 140 ký tự.
- **Quiz:** không thêm câu.

**Được:** danh sách đủ 8 cấp mà không phải luyện 6 cấp mới; bảng xuất hiện đúng lúc tour nói tới và còn đó khi làm quiz EKZ; tour 10 bước, quiz 4 câu, `WmsFieldTour` và `PackageQuizController` không đổi code; chữ là TMP cố định nên `LocalizationBinder.BindOpenScene` gắn `LocalizeStringEvent`, EN chạy tự động; muốn đảo lại chỉ sửa `markers` trong Inspector.

**Mất / chấp nhận:** sau „Von vorn“ bảng tắt tới bước 8; bảng bật ra tức thì, mũi tên ở bước 8 thay vai trò báo hiệu; không ghi mã `000`/`003` trên mỗi dòng; nghĩa các cấp 2, 4, 5, 6, 8, 9 chỉ dựa vào slide 8, người hướng dẫn chưa xác nhận (chữ để `proposed`, sửa bằng CSV); phía phải bàn đông hơn (bảng tour, „Weiter“, Menu, bảng mới), bảng không bấm được nhưng phải kiểm không chồng lên Menu.

**Bị bác:** luôn hiện (A); chỉ đúng bước 8 (không xem lại khi làm quiz); bảng sinh bằng script (thêm GUID và code cho 8 dòng tĩnh); thêm bước tour (C); dải bảng trên màn hình WMS (phải ngẩng đầu, dễ bị hiểu là một phần WMS); câu mệnh lệnh cho mọi cấp; câu quiz mới (kiểm tra nội dung không luyện, nghĩa chưa xác nhận; nếu cần sau này chỉ câu EKZ 8/9, gộp vào #43).

## Done when
Xem `docs/backlog.md` #57.

## Nếu giả định sai
- Test kính thấy trainee không nhận ra bảng vừa hiện, hoặc muốn xem lại sau „Von vorn“: chuyển sang B2 (hook trong `WmsFieldTour`, có thể thêm grow), quyết định mới thay thế 018.
- Ở +54° chữ không đọc được hoặc bảng chạm Menu: dời bảng (chỉ transform) hoặc rút chữ xuống từ khóa; không giảm font dưới mức đọc được.
- Người hướng dẫn sửa nghĩa một cấp: sửa hàng CSV và pictogram của cấp đó.
- Người hướng dẫn bỏ cấp không có trong CA0050: tắt dòng đó, xoá key.
- D4/D5 (decision 003) đổi: chuyển khung „In der Übung“ sang cấp mới; câu body bước 8 không phụ thuộc.
- Người dùng muốn bảng luôn hiện: bỏ bảng khỏi `markers`, bật sẵn trong scene.

## Câu hỏi cho người hướng dẫn
D21, D22, D23, D24, D25; câu cũ D4–D6 và từ „einfüllen“/„schütten“ (glossary).
