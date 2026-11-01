# Quyết định 002: Cơ chế đa ngôn ngữ DE/EN

- **Backlog:** #11, #12, #13
- **Trạng thái:** accepted
- **Ngày:** 2026-11-01
- **Ý kiến đã xét:** unity-reviewer, german-reviewer, xr-ux-reviewer → decision-synthesizer; requirements-analyst (tra thuật ngữ sau đó)
- **Lựa chọn của người dùng (2026-11-01):** chấp nhận khuyến nghị. Trainee không bao giờ cần tự đổi ngôn ngữ giữa bài.

## Bài toán
Trainee là người Đức bản ngữ, không đọc được tiếng Anh. Trong khi đó Welcome, Concept, Mixed và Complex đang toàn tiếng Anh, còn Basic là tiếng Đức nhưng xưng *du*. Developer lại cần tiếng Anh để test. Cần một cơ chế chung cho 5 scene, gồm khoảng 130 TMP trong scene và khoảng 53 chỗ gán text trong code. Yêu cầu: mặc định `de`, nhớ lựa chọn sau khi khởi động lại, và một nguồn chuỗi duy nhất mà người Đức bản ngữ duyệt được trong Excel rồi import lại.

## Xung đột đã xét
| Vấn đề | Các ý kiến | Cách giải |
|---|---|---|
| Nền tảng | Unity nghiêng về bộ dịch tự viết (CSV chính là file Excel); German chọn package Unity Localization | Dùng package. Code chỉ gọi qua lớp mỏng `Loc.Get`, nên nếu cần thì đổi sang bộ tự viết được mà không sửa validator |
| Số nhiều (plural) | Smart String có sẵn, nhưng người duyệt dễ làm hỏng cú pháp | Không dùng plural. Viết dạng nhãn: `Pakete: {placed} / {required}` |
| File review Excel | Cả hai phương án đều phải tự viết bước xuất/nhập 8 cột | Viết một bộ chuyển đổi, đọc được cả `;` và `,` |
| Vị trí nút | #11 tự mâu thuẫn ("đặt ở WelcomeScene" và "Menu có nút"); UX: Menu khó với tới, dễ chạm nhầm (#37) | Nút chỉ đặt ở WelcomeScene |
| Đổi ngôn ngữ giữa bài | Đổi ngay tại chỗ sẽ để lại text runtime ở ngôn ngữ cũ | Đổi xong thì tải lại scene. Người dùng xác nhận trainee không đổi giữa bài |

## Quyết định
1. **Nền tảng:** package `com.unity.localization` với String Table Collection `UI`, locale `de` (mặc định) và `en`.
   - Text trong scene dùng `LocalizeStringEvent`.
   - Text do code tạo ra đi qua một lớp mỏng `Loc.Get(key, args)`; không serialize `LocalizedString` trong từng script.
   - Lựa chọn ngôn ngữ lưu trong `PlayerPrefs`.
2. **Thử trước ở ConceptScene**, rồi mới gắn hàng loạt. Bước thử phải chạy được trên Quest và ghi lại: thời gian build, có Addressables hay không, có nhấp nháy ở frame đầu hay không.
3. **Nút `Deutsch / English` chỉ có ở WelcomeScene.** Không có trong Menu. Đổi ngôn ngữ thì tải lại scene, nên không bao giờ có màn hình trộn hai thứ tiếng.
4. **Quy tắc chuỗi:**
   - Câu trọn vẹn, placeholder có tên, không ghép từng mảnh.
   - Không dùng Smart String plural.
   - Rich text bọc cả câu, không tô màu từng từ.
   - Nhãn WMS (`do_not_translate=yes`) giống từng ký tự ở cả hai locale.
   - Chú thích tiếng Anh cho nhãn WMS là key riêng, chỉ hiện ở `en`.
5. **Review:** tự viết bộ xuất/nhập theo 8 cột của skill `localize-strings`. Bộ nhập phải:
   - đọc được file Excel tiếng Đức lưu với dấu `;`;
   - từ chối file có ký tự hỏng `�`;
   - báo lỗi khi placeholder trong DE khác EN.
6. **Thiếu key:** `Debug.LogError` ghi tên key, và TMP hiện tên key thay vì để trống.

## Được / mất
- **Được:**
  - Một nguồn chuỗi, khớp với backlog, skill `localize-strings` và `docs/coding-conventions.md` §6.
  - Rủi ro của package lộ ra ngay ở một scene thử.
  - Có đường lùi rẻ: lớp `Loc.Get`.
  - Trainee không thể vô tình chuyển sang tiếng Anh.
- **Mất:**
  - Thêm một package, kéo theo Addressables.
  - Có hai đường truy cập chuỗi: component trong scene và `Loc.Get` trong code.
  - Vẫn phải tự viết bộ chuyển đổi review.
  - Developer muốn đổi ngôn ngữ phải quay về Welcome một lần.
- **Bị bác:**
  - Bộ tự viết (B): giữ làm đường lùi.
  - Hard-code hai bản EN/DE: người duyệt không sửa được nếu không có dev, nên không đạt #13.
  - Nút trong Menu: rủi ro chạm nhầm (#37), chỉ có lợi cho developer.

## Nếu giả định sai
- **Bước thử ở ConceptScene thất bại** (Addressables lỗi trên Quest, build quá chậm, nhấp nháy không tránh được):
  - Gỡ package và chuyển sang bộ tự viết.
  - Chỉ phải gắn lại component ở một scene, và `Loc.Get` đổi nguồn ở một chỗ.
  - Sửa `localize-strings` §0 cho khớp, và ghi một quyết định mới (`Thay thế: 002`).
- **Developer thấy quay về Welcome quá phiền:** thêm phím tắt chỉ có trong bản dev, không đụng tới UI của trainee.

## Phụ thuộc và câu hỏi còn mở
- **Phím xác nhận:**
  - Theo tài liệu requirements, dialog mới (TMA/CSC4) dùng `F6 Bestätigen` hoặc bàn đạp chân; dialog cũ (KOM 1/2) dùng `F12 Bestätigen`. Trong dialog mới, `F12` là `Aufkl. korr.`.
  - BasicPickingScene đang ghi `F12` trên một panel kiểu dialog mới.
  - Chọn dialog nào là câu **B8**. Câu này chặn chuỗi của nút xác nhận trong #12, không chặn #11.
- **Thuật ngữ:**
  - `VPE` (Verpackungseinheit) là **số chiếc** trong một Karton hoặc PU-Beutel, không phải tên đồ vật.
  - Đồ vật thì tài liệu gọi là Karton/Kartonage/Schachtel hoặc Tüte/Beutel (10er Tüte).
  - Hàng lẻ: đơn vị `Stück`/`ST`, còn hàng không đóng gói sẵn là `loses Material`.
  - "Paket", "Packstück", "Gebinde" không có trong tài liệu.
  - `GG` không được định nghĩa (câu **A2**).
  - Đã thêm các thuật ngữ còn thiếu vào glossary với trạng thái `proposed`.
- **Ghi log đổi ngôn ngữ:** `docs/research-logging-spec.md` yêu cầu ghi `language`. Việc này làm cùng #26–#31; #11 chỉ cần đọc được ngôn ngữ hiện tại ở một chỗ.
- **Thứ tự:** #12 làm sau #4, #6, #25 như backlog đã ghi.
