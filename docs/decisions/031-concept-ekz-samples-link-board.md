# Quyết định 031: ConceptScene bước 8–9: mẫu EKZ 003/000 giống bài thực hành, thẻ mẫu mang pictogram của bảng EKZ

- **Backlog:** #101 (bổ sung cho #94; liên quan #92, #57)
- **Trạng thái:** accepted
- **Ngày:** 2026-11-05
- **Thay thế (một phần):** 028 (câu „Mẫu 003 có 2 carton“ trong phần bổ sung 2026-11-05). Decision 018 giữ nguyên.
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, data-analyst → decision-synthesizer.
- **Lựa chọn của người dùng (2026-11-05):** chấp nhận phương án B với S1 (mẫu 000 giữ túi), 3 carton cho mẫu 003.

## Bài toán
Mục tiêu: học xong ConceptScene, người học biết EKZ có 8 mức (0, 2, 3, 4, 5, 6, 8, 9), biết bài luyện chỉ dùng 0 và 3, và nối được mỗi mức với ô Empf Kz trên LVS và với một mẫu. Hiện có ba chỗ chưa đạt:
- Mẫu 003 chỉ có 2 carton, nên không thấy được „xếp có thứ tự, cùng hướng“. Từ chỗ đứng chỉ đọc được 1 nắp.
- Mẫu 000 là 2 túi to, nằm gần như ngay ngắn.
- Bảng `EKZ Overview` nằm bên phải (khoảng +60°), mẫu và chú thích nằm bên trái (−24° đến −36°): muốn so phải quay đầu khoảng 96°.

## Tài liệu nói gì (requirements-analyst)
- Kommissionierung slide 8 (bảng EKZ-Vorgabe): EKZ 0 có thể là hàng đổ rời, các Teile nằm rời; EKZ 3 đặt từng cái, đúng hướng, nằm có thứ tự, giữ thứ tự khi vận chuyển.
- Slide 6: vật liệu rời được cho vào PU-Beutel và dán nhãn; carton đặt vào thùng đơn „EKZ gerecht“.
- Tài liệu cho phép mọi phương án dưới đây, không phương án nào trái tài liệu. Carton thay cho Teile ở EKZ 3 vẫn là giả định (D4/D35).

## Xung đột giữa các đề xuất
| Vấn đề | Một bên | Bên kia | Đánh đổi |
|---|---|---|---|
| Vật trong mẫu 000 | UX, unity, người dùng: vòng RING D đổ đống | ID: giữ túi như Mixed vị trí 2; vòng chỉ sau khi D36 có câu trả lời | Gần ảnh slide 8 ↔ đúng dù D36 trả lời thế nào |
| Số carton mẫu 003 | unity, người dùng: 4 | ID, UX: 3 một cột như Mixed vị trí 1 | Hàng dài hơn ↔ vừa thùng hiện tại, giống bài 4 |
| Nối bảng với mẫu | ID P1, UX A: pictogram của bảng trên thẻ mẫu | ID P2, UX B: dời bảng sang chỗ `Info Panel` bên trái | Rẻ, signalling ↔ cạnh nhau nhưng bên trái chật, chưa đo |
| Lúc bảng hiện | UX A: chỉ bước 10–11 | 018: bước 9–11 | Bước 10 nói về vùng LVS không dùng, không về EKZ |
| Thêm bước tour | UX C, data B: 12 bước | data A, unity: giữ 11 | Đo riêng thời gian ở bảng ↔ sửa spec, R, `layout_version` |

## Các phương án
- **A. Chỉ đổi đồ trong mẫu:** rẻ nhất, nhưng không có gì nối mẫu với bảng.
- **B. A + thẻ mẫu mang pictogram và nhãn của dòng tương ứng trên bảng; bảng giữ bước 9–11 (018):** chọn.
- **C. A + dời bảng sang trái, mũi tên từ dòng 0/3 xuống mẫu:** dự phòng (xem „Nếu giả định sai“).
- **D. Thêm bước tour „xem bảng“:** loại (018 đã bác thêm bước; quay đầu vẫn còn; phải sửa log contract và R).

## Quyết định: phương án B
- **Mẫu 003:** 3 carton KABELVERBINDER A thẳng một cột, cùng hướng, `Label Top` của cả 3 quay cùng một phía về chỗ đứng; cùng bố cục với 3 ô của Mixed vị trí 1. Thùng giữ cỡ hiện tại nếu 3 carton vừa (đo trong Editor).
- **Mẫu 000:** túi SCHRAUBE B nằm nghiêng theo nhiều hướng, chồng lên nhau, như Mixed vị trí 2 (S1).
- **Thẻ mẫu:** thẻ 003 mang pictogram giống dòng EKZ 3 của bảng và „Empf Kz 003 = EKZ 3“; thẻ 000 mang pictogram dòng EKZ 0 và „Empf Kz 000 = EKZ 0“. Chỉ nhãn LVS và số, không key mới (như „EKZ n“ trên bảng).
- **Bảng:** giữ vị trí và cách hiện của 018 (bước 9–11, mũi tên bước 9).
- **Không đổi:** 11 bước tour, `step_id`/`step_index`, chữ và đáp án `ekz_003`/`ekz_000`, `layout_version` "029", script. Mẫu không có `PackageData`, `Grabbable`, collider hay log (028).
- **Kiểu „chỉ để xem“:** bán trong suốt + viền làm trong #92. Hai mẫu nằm sau bàn, ngoài tầm tay, nên không cần chữ „Nur zum Ansehen“ khi chạm cho riêng hai mẫu này.

## Được và mất
- **Được:** thấy được „có thứ tự, cùng hướng“; mẫu là ví dụ mẫu của Mixed vị trí 1 và 2; cùng một hình ở ô LVS, mẫu và bảng; không chuỗi mới, không đổi log/R.
- **Mất:** bảng vẫn bên phải, mối nối chỉ dựa vào việc nhận lại pictogram; mẫu 000 không cho thấy „chi tiết nhỏ đổ rời“; 3 carton thay vì 4.

## Đề xuất bị bác
- Dời bảng sang trái (ID P2, UX B): chưa đo chỗ, dễ che shelf hoặc chú thích; giữ làm dự phòng.
- Vòng RING D ở mẫu 000: chờ D36; Concept đã có 15 vòng RING D cầm được ở mép bàn, thêm vòng chỉ để xem trông y hệt gây đúng lỗi của #92.
- Bảng chỉ ở bước 10–11: bước 10 không nói về EKZ.
- Thêm bước tour; log khi chạm (trái decision 022).
- Gộp mesh: chỉ cần nếu sau này dùng vòng.
- Thu gọn 6 mức còn lại trên bảng; thêm „orientiert“ vào dòng 3; sửa dòng 6: không đổi trong mục này (018, D22).

## Giả định
- D4/D38: carton được đặt từng cái, thẳng hàng ở EKZ 3.
- D36 chưa có câu trả lời: túi đúng với cả hai khả năng.

## Nếu giả định sai
- Test kính thấy người học không nối được bảng với mẫu: chuyển sang C (dời bảng và mũi tên bước 9), quyết định mới thay 018.
- D36 = „lose schütten“: mục riêng đổi mẫu 000 sang vòng/chi tiết nhỏ đổ đống, sửa caption 000 và quiz `ekz_000` qua `localize-strings` + `german-reviewer`; bump `layout_version` nếu build đã dùng với người tham gia.
- D36 = „im PU-Beutel“: giữ S1; xem lại vòng rời ở Complex (000) thành mục riêng.
- D4/D38 sai: mẫu 003 theo phần „Nếu giả định sai“ của 028.

## Done when
- Ở ConceptScene bước 8, mẫu 003 có 3 carton KABELVERBINDER A thẳng một cột, cùng hướng, nhãn nắp quay cùng một phía.
- Ảnh từ chỗ đứng (mắt (0, 1,6, 0), nhìn về chú thích Empf Kz) đọc được cả 3 nắp carton thành một hàng.
- Ở bước 8 và 9, mẫu 000 có túi SCHRAUBE B nằm nghiêng theo các hướng khác nhau, chồng lên nhau; không carton hay túi nào xuyên thành thùng mẫu.
- Ở bước 8 và 9, thẻ mẫu 003 có pictogram giống dòng EKZ 3 trên bảng và chữ „Empf Kz 003 = EKZ 3“; thẻ mẫu 000 có pictogram dòng EKZ 0 và „Empf Kz 000 = EKZ 0“; ở EN thẻ giống hệt.
- Ở bước 9, bảng hiện bên phải như 018, dòng 0 và 3 mang đúng pictogram đã thấy trên thẻ mẫu.
- Ở bước 1–7, 10 và 11 không thấy mẫu nào.
- (code) Tour vẫn 11 `step_id` cùng index; `ekz_003`/`ekz_000` không đổi chữ hay đáp án; `layout_version` vẫn "029".
- (code) Mẫu không có `PackageData`, `Grabbable` hay collider, không nằm trong `Hint Zone` nào; `PackageStandardizer.Check()` không báo mẫu.

## Câu hỏi mở
D36, D35, D38, D22 (đã có trong danh sách câu hỏi cho người hướng dẫn).

Xem `docs/backlog.md` #101.
