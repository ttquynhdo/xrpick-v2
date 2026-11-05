# Quyết định 019: Ba làn ở trạm, thùng chạy tự động ở Mixed/Complex, bảng 10 bước ở Concept

- **Backlog:** #71 (Mixed, Complex, Concept, Welcome); Basic tách thành #72
- **Trạng thái:** accepted (một câu về thời gian sửa bởi 033)
- **Ngày:** 2026-11-03
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, data-analyst → decision-synthesizer (hai vòng)
- **Lựa chọn của người dùng (2026-11-03):** phương án D' (ba làn, học viên không tự kéo thùng, có cách cho thấy cả quy trình thật). Basic tách thành mục riêng. Thùng kho 1 của Complex trượt vào từ trái sau quiz. Thùng kế tiếp ẩn, chỉ chạy vào khi cần. **Làn giữa là dải con lăn ở giữa hai thùng** (người dùng đọc ảnh, sửa bản nháp coi khe chỉ là cầu nối). Làn trái va cột giá thẻ thì dời cột. Chữ Đức theo bản nháp, qua `german-reviewer`.
- **Quy tắc mới (người dùng, 2026-11-03):** không chờ người hướng dẫn trả lời. Câu hỏi chưa có trả lời là **giả định**, kiểm trong buổi test với người hướng dẫn tuần sau.
- **Sửa các quyết định đã chấp nhận:** 012 (đổi thùng bằng fade/scale → chạy trên làn); 016 (thùng 1 phóng to → trượt vào từ trái; mở thùng bằng quiz giữ nguyên); 013 (hình học băng ở trạm, tuyến làn thùng kho); 014 (thùng đơn sang làn giữa trước khi chạy; điểm dừng sau trạm trống ở x 5 giữ nguyên); 011 (khe giữa hai thùng nới ra thành làn; K1 tự động giữ nguyên).

## Bài toán
Bảng bấm giờ „OIS2 Kommi2“ (`old/LOU2_TC-K2+TMA_OIS2.xlsx`, hàng 10–19) mô tả 10 bước ở trạm. Bước 1: thùng kho được kéo từ trái vào chỗ làm việc. Bước 3: thùng rỗng kéo vào từ phải. Bước 9: sau khi xác nhận, thùng kho tự chạy đi, thùng tiếp theo vào từ trái. Bước 10: thùng đơn được kéo từ làn phải sang làn giữa rồi tự chạy đi. App hiện đổi thùng kho bằng thu nhỏ/phóng to và chỉ có hai làn. Người dùng muốn học viên hiểu trạm có ba làn và thấy bước nhỏ của mình nằm trong quy trình 10 bước, mà không phải tự kéo thùng.

## Xung đột giữa các đề xuất
| Vấn đề | Một bên | Bên kia | Kết luận |
|---|---|---|---|
| Ba làn ngay hay chờ D26 | unity C | requirements, ID, data: vai trò làn chỉ là suy ra | Người dùng chọn ba làn; vai trò làn là giả định |
| Học viên tự kéo | ID B, xr-ux C, data B | ID (độ khó ở Complex), xr-ux (với xa, cúi), data (mẫu số log) | Không kéo; bước kéo tự động và có nhãn trên bảng 10 bước |
| Cho thấy 10 bước bằng gì | ID: bảng nhóm 3 loại; data: trình diễn ở Concept | ID, xr-ux: không bắt đọc khi thùng chạy | Bảng tĩnh ở Concept + câu báo trước trên thẻ kết quả |
| WMS đổi position lúc nào | data: thứ tự log `confirm_pressed` → `LineAdvanced` → start → arrived | xr-ux, ID: LVS và thẻ 1 đổi khi thùng mới đã dừng | Logic đổi ngay; phần hiển thị chờ thùng dừng |
| Thùng kế tiếp | xr-ux A: chờ sẵn, nhìn thấy | rủi ro gây nhiễu | Ẩn, chỉ chạy vào khi cần (người dùng) |
| Làn giữa ở đâu | bản nháp: làn dưới thùng kho, khe chỉ là cầu nối | ảnh: dải con lăn chạy ra sau dưới bàn phím, mũi tên 10 chỉ ra sau theo nó | Làn giữa = dải con lăn; khe nới ra bằng bề ngang thùng (người dùng) |

## Các phương án
- **0. Giữ scale + làn trang trí:** không cho thấy bước 9. Loại.
- **A. Hai làn, thùng kho đổi trên làn:** không cho thấy ba làn. Loại.
- **D'. Ba làn tự động + bảng 10 bước ở Concept + câu báo trước:** chọn.
- **C. D' + học viên tự kéo:** trái lựa chọn của người dùng (thao tác ít nhất). Loại.

## Quyết định: phương án D'

**1. Hình học (bố cục/prefab)**
- **Chỗ làm việc (thùng 1 trên ảnh):** thùng kho đứng trước mặt, bên trái làn giữa. Không phải làn đi ra.
- **Làn giữa = dải con lăn giữa thùng kho và thùng đơn.** Khe trong app (0,15 m, có `CentreStrip`) nới ra bằng bề ngang một thùng (≈0,40 m). Thùng kho dịch sang trái, thùng đơn dịch sang phải (mỗi bên ≈0,12 m). Màn hình vẫn đứng trên làn giữa như trên ảnh; làn giữa chạy ra sau dưới bàn sau. Vị trí mới chốt bằng ảnh Play mode (tầm với, không chạm `Menu`).
- **Làn trái (mới):** từ phía sau bên trái chạy tới, rẽ sang phải vào chỗ làm việc (mũi tên 1). Nếu va cột `InstructionShelf` thì dời cột giá thẻ (người dùng). Thêm vì bài tập dùng nó (quy tắc bố cục trạm).
- **Làn phải:** giữ thùng đơn. Không mô phỏng thùng rỗng vào từ phải (bước 3): thùng đơn có sẵn từ đầu bài.
- **Mạng 013 (dựng lại mesh gộp, không thêm primitive, không collider):** làn thùng kho từ kệ vào làn trái; làn giữa chạy ra sau tới điểm rẽ ở z ≈ 3: thùng kho rẽ về phía kệ, thùng đơn theo tuyến cũ của 014.
- **Tuyến 014:** ở lần xác nhận đúng cuối, thùng kho rời trạm trước; sau đó thùng đơn trượt ngang sang làn giữa (bước 10), chạy ra sau, theo tuyến 014 tới chỗ dừng sau trạm trống ở x 5, không xoay, ≈0,45 m/s.

**2. Hành vi ở Mixed/Complex**
- Thùng kho kế tiếp ẩn tới khi cần, hiện ra ở đoạn làn trái khuất dưới/sau bàn sau, chạy tới rồi rẽ vào chỗ làm việc. Bắt đầu cùng lúc với thùng cũ đi (bước 9 „parallel“).
- Thùng kho cũ cùng kiện bên trong (kiện đang cầm trên tay không bị mang; carton mở của Complex đi theo) trượt ngang sang làn giữa rồi chạy ra sau, dưới bàn sau. Thùng chỉ tắt khi đã khuất, không biến mất trong tầm nhìn.
- Kiện trong thùng đang chạy không cầm được, cầm được lại khi thùng dừng, đúng kích thước (không đổi scale, hết lỗi `GrabFreeTransformer`). Nút đỏ không có tác dụng khi thùng chạy. Một kiện đếm một lần. Đường đi của thùng kho không cắt trigger thùng đơn. Coroutine dừng sạch ở `OnDisable`.
- Thẻ kết quả hiện trước, rồi mới có chuyển động. **Logic** (`AdvanceToNextLine` / `LineAdvanced`, counter, validator) đổi position ngay khi xác nhận (giữ thứ tự log). **Hiển thị** (dialog WMS, highlight thẻ 1 của #40) đổi khi thùng mới đã dừng (giả định D27). Từ lúc bấm tới lúc thùng mới dừng ≈≤ 4 s *(sửa bởi decision 033: chờ 4 s để đọc thẻ rồi mới chạy, tổng khoảng 7 s)*.
- **Complex:** thùng kho 1 ẩn lúc đầu; trả lời đúng cả hai câu quiz thì nó chạy vào từ làn trái thay cho phóng to (`RevealFirstSet` vẫn là điểm kích hoạt).

**3. Cho thấy 10 bước (không thêm thao tác)**
- **ConceptScene, bảng tĩnh „Ablauf am Kommissionierplatz“** (mẫu #57: không script, không raycast, hiện qua `WmsFieldTour.steps[].markers` ở bước `material_flow`, „Von vorn“ ẩn lại). 10 bước bằng câu ngắn, sơ đồ ba làn bằng UI Image, ba nhóm (nhãn nháp, qua `german-reviewer`):
  - „Üben Sie hier“: 2, 4 (zählen), 6, 9.
  - „In der Übung automatisch, am echten Platz von Hand“: 1, 3, 10 và việc thùng kho tự đi ở 9. Bắt buộc ghi rõ ở trạm thật là người kéo.
  - „Nicht in dieser Übung“: 4 (wiegen), 5, 7, 8.
  Không chồng `EKZ Overview` / `Sample Description`, ngoài ±15° quanh màn hình.
- **`concept.tour.material_flow.body`** viết lại theo ba làn: thùng kho đến từ trái; sau mỗi lần xác nhận đi qua làn giữa; thùng đơn đứng bên phải tới khi xong đơn rồi sang làn giữa và đi. Không nói nơi đến (D15).
- **Mixed/Complex:** câu báo trước trên thẻ kết quả, cùng từ với bảng. Position chưa cuối: ý „Der Lagerbehälter fährt über die mittlere Bahn weg, der nächste kommt von links.“ Position cuối: ý „Der Auftragsbehälter wechselt auf die mittlere Bahn und fährt weg.“ Quá 3 dòng (Complex position 1 có câu A13) thì đưa sang thẻ bước 3.
- **Không làm:** phụ đề trong lúc thùng chạy; trình diễn ba làn ở Concept; mở rộng #40 cho bước tự động.
- **WelcomeScene:** không đổi.

**4. Log:** #71 không ghi log. Ràng buộc cho #28/#30, chốt trước người tham gia thật đầu tiên: mỗi chuyển động do máy (thùng kho rời/tới, thùng đơn sang làn giữa và chạy, thùng 1 Complex tới sau quiz) có mốc start và arrived, theo thứ tự `confirm_pressed` → `LineAdvanced` → start → arrived; header có `layout_version`; không thu dữ liệu thật trên hai build khác nhau trong cùng một đợt.

**Được:** học viên thấy ba làn và luồng thật (vào trái → làm ở chỗ 1 → ra làn giữa; thùng đơn bên phải → làn giữa → đi); có bảng 10 bước cho biết bước nào mình làm, bước nào máy làm thay, bước nào bỏ; không thêm thao tác; không bắt đọc khi thùng chạy; không thùng chờ gây nhiễu.

**Mất / chấp nhận:** vai trò làn là giả định, sai thì sửa mesh, tuyến và chữ; công lớn (L); bố cục bàn đổi (khe rộng hơn, hai thùng dịch ra, cột giá thẻ có thể dời); chờ ≈4 s mỗi lần đổi, chuyến cuối dài thêm ≈2 s; bước kéo tay chỉ được xem và đọc trên bảng.

**Bị bác:** giữ scale + làn trang trí; hai làn (bản 019 đầu); học viên tự kéo; thùng vào từ phía sau cùng làn; thùng chờ nhìn thấy được; trình diễn ở Concept; mũi tên marker ở Concept (không có làn để chỉ); log ngay trong #71 (chuyển sang #28); dòng tổng kết luồng thùng ở Complex (tổng kết chỉ ghi việc học viên tự làm).

## Done when
Xem `docs/backlog.md` #71 (Mixed/Complex), #72 (Basic) và #73 (phần ConceptScene: bảng 10 bước, chữ `material_flow`; tách khỏi #71 ngày 2026-11-03 để gộp với các việc khác ở ConceptScene).

## Nếu giả định sai (sau buổi test)
- D26a/b sai (ví dụ thùng kho về qua làn trái, hoặc dải giữa không phải làn): sửa mesh, tuyến, bố cục và chữ trên bảng; cơ chế chạy giữ nguyên.
- D11/D17: thùng kho do người đặt hoặc kéo: đổi nhóm của bước 1 trên bảng; cân nhắc cho học viên kéo ở một bài ít yếu tố mới.
- D26e: một thùng kho ở lại cho nhiều lượt pick: không cho thùng đi khi position sau cùng thùng.
- D27: LVS đổi ngay khi xác nhận: bỏ phần chờ của hiển thị.
- D18: bước 10 chỉ có ở một số đơn: thêm một câu trên bảng.
- FPS không đạt hoặc chuyển động gây khó chịu: rút ngắn đường chạy (Inspector).
- C1: khu khác, không theo OIS2 Kommi2: xem lại cả quyết định.

## Giả định cho buổi test với người hướng dẫn
D26a (vai trò ba làn), D26b (dải con lăn giữa là làn giữa), D26c (F6 cuối kết thúc thùng đơn), D26d (camera quét mã, app không có bước quét), D26e (một thùng kho mỗi position), D26f/D15 (nơi đến của thùng đơn không gọi tên), D11, D17 (thùng tự chạy, bước kéo tay là tự động trong app), D18 (bước 10 ở mọi đơn), D27 (LVS đổi khi thùng mới đã tới), C1, D12.
