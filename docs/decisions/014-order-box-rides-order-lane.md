# Quyết định 014: Thùng đơn chạy tiếp theo làn đơn, rẽ ở góc không xoay, dừng xếp hàng sau trạm trống; chưa gọi tên nơi đến

- **Backlog:** #50
- **Trạng thái:** accepted
- **Ngày:** 2026-11-02
- **Thay thế:** 011 phần K1 (câu „thùng trượt ~2 s rồi đứng yên“) và câu *Done when* thứ 5 của 013 („trượt ~0,9 m trong ~2 s rồi đứng yên“). Các phần khác của 011 và 013 giữ nguyên.
- **Ý kiến đã xét:** requirements-analyst, unity-reviewer, instructional-design-reviewer, xr-ux-reviewer, data-analyst → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-02):** phương án B. Tốc độ như hiện tại. Ở góc làm theo cách thật nhất (góc trong scene là góc vuông có tấm chuyển hướng, nên thùng không xoay). Thùng dừng xếp hàng sau thùng của trạm trống ở x 5.

## Bài toán
Test kính #49 H1: tuyến băng dài hợp lý, nhưng sau đơn đúng thùng đơn chỉ trượt ~0,9 m rồi đứng yên (011 K1), nên trainee không thấy nó đi đâu. Mục tiêu: sau đơn đúng, trainee thấy thùng đơn rời trạm và tự đi tiếp theo băng.

Căn cứ:
- Nhóm trả lời D11 (thùng đơn tự chạy đi, không ai đẩy) và D15 (tới khu giao hàng ở cửa kho, xếp hàng chờ), người hướng dẫn chưa xác nhận.
- Bảng bấm giờ cũ (sheet „OIS2 Kommi2“, bước 10) ghi thùng đơn được kéo sang làn giữa rồi tự chạy đi; tài liệu không nói nó đi đâu, chỉ nhắc Behälterpuffer và Packbereich ở chỗ khác.
- 013 hoãn phương án C (chuyển động trên tuyến dài) cho tới D11; điều kiện đó nay đã có câu trả lời của nhóm.

## Xung đột giữa các đề xuất
| Vấn đề | Một bên | Bên kia | Kết luận |
|---|---|---|---|
| Có khu Versand ngay không | UX, ID: có, nếu D15 được xác nhận | requirements: tài liệu nêu đích khác; vẽ Versand là dạy điều chưa có nguồn | Chưa làm, chưa gọi tên nơi đến |
| Cuối đường | unity: `SetActive(false)` | data, UX: không tắt/xoá (exit giả, biến mất trông như lỗi) | Thùng dừng hẳn, không tắt |
| Âm thanh băng lăn | UX: có | ID: không thêm âm thanh | Chưa làm, mở lại nếu trainee bỏ lỡ chuyển động |
| Log sự kiện chuyến chạy | data: thêm ngay | chưa có scene script nào ghi log (#28 chưa làm) | Ghi thành ràng buộc cho #28/#30 |
| Cách dựng | unity PA 1: điểm đường trong `OrderBoxConveyor` | unity PA 2: `ConveyorPathRunner` đọc prefab mạng | PA 1; PA 2 khi làm Versand hoặc D17 |

## Phương án được chọn: B
- **Basic, Mixed (position cuối), Complex:** đơn đúng → thùng đơn cùng các kiện trong trigger (và 6 `BoxSnapper` ở Basic) chạy theo làn đơn:
  1. thẳng theo +z từ trạm tới góc thứ nhất (z ≈ 3,0);
  2. dừng ~1 s, rẽ sang +x, **không xoay** (góc vuông có tấm chuyển hướng, thùng thật giữ nguyên hướng);
  3. chạy tới góc thứ hai (x ≈ 5,0), dừng ~1 s, rẽ sang +z, không xoay;
  4. dừng hẳn sau thùng của trạm trống ở x 5 (khe ~5 cm), như một hàng chờ.
- Mỗi đoạn có tăng tốc/giảm tốc nhẹ; tốc độ trung bình ~0,45 m/s như cú trượt hiện tại (0,9 m / 2 s). Cả chuyến ~8,4 m, khoảng 20 s.
- Thùng không tắt, không xoá; kiện không bị tắt. Không thêm chữ, âm thanh hay vật mới. Không đụng counter, `OrderRule`, validator, log.
- Tour Concept giữ câu „Nach dem Bestätigen fährt der Auftragsbehälter weiter.“ (vẫn đúng), không gọi tên nơi đến.
- Điểm đường và tốc độ là field Inspector của `OrderBoxConveyor`, cùng giá trị ở ba scene.

**Được:** trainee thấy „đơn đúng → hàng rời trạm và đi tiếp“, đúng điều người dùng thấy thiếu khi test kính; khớp „fährt automatisch weg“; không dạy nơi đến chưa có nguồn.

**Mất / chấp nhận:**
- Câu hỏi „đi đâu“ mới được trả lời một nửa.
- Thùng dừng ở chỗ **vẫn nhìn thấy** (lệch phải ~50–60°), không khuất hẳn: không có vật che trước đoạn làn đơn nối về kệ, và người dùng chọn xếp hàng sau trạm trống thay vì thêm vật che.
- Chuyển động ngoại vi ~20 s lúc trainee đọc thẻ kết quả; bài đã xong nên chấp nhận, phải test trên Quest.
- Bước „kéo thùng sang làn giữa“ vẫn bị bỏ (giản lược có từ 011).

**Bị bác:** khu Versand + gọi tên (chờ người hướng dẫn xác nhận D15); thẳng ~3 m rồi ẩn (biến mất đột ngột); `SetActive(false)` cuối đường; âm thanh (để sau); log `order_box_ride_start/end` ngay (để #28); chỉ thêm chữ trên thẻ.

## Done when
- Ở Basic, Mixed (position cuối) và Complex, đơn đúng, bấm nút đỏ → thẻ kết quả hiện trước hoặc cùng lúc thùng đơn bắt đầu chạy.
- Thùng chạy theo làn đơn qua góc z ≈ 3 và góc x ≈ 5, dừng ~1 s ở mỗi góc, không xoay, cùng tuyến và cùng tốc độ ở ba scene.
- Thùng dừng hẳn ngay sau thùng của trạm trống ở x 5, không chồng lên nó.
- Trong lúc chạy và sau khi dừng, mọi kiện vẫn nằm trong thùng, không rơi, không rung; thùng và kiện không xuyên bàn sau, màn hình hay vật khác (ảnh Play mode quay phải ~45° và ~60°).
- Ở Basic, 6 chỗ snap đi cùng thùng tới điểm dừng.
- Kiện đang cầm trên tay lúc bấm xác nhận ở lại trong tay.
- Mixed, xác nhận đúng ở position 1 → thùng không chạy. Đơn sai → thùng đứng yên.
- Trong lúc thùng chạy đọc được thẻ kết quả; chọn một bài trong Menu → scene mới mở, Console không lỗi; vào lại scene → thùng ở vị trí ban đầu.
- Sau khi đúng, bấm nút đỏ lần nữa không đổi kết quả.
- ConceptScene: bước `material_flow` không đổi, không gọi tên nơi đến.
- Trên Quest, FPS cả ba scene giữ như sau #49 trong suốt chuyến chạy (ca thử tay).
- (code) Trong lúc chạy và sau khi dừng: không `ContentsChanged`, Ist trên WMS không đổi, không kiện nào bị tắt hay xoá, không log mới; chuyển động bắt đầu từ `OrderCompleted`, chỉ coroutine; điểm đường thiếu → `Debug.LogError(..., this)`.

## Nếu giả định sai
- Người hướng dẫn xác nhận D15 (Versand, Behälterpuffer hay Packbereich): thêm khu nơi đến ở xa theo quy tắc bố cục, có thể sửa tuyến 013, sửa câu tour qua `localize-strings` + `german-reviewer`; cân nhắc `ConveyorPathRunner`. Quyết định mới, `Thay thế: 014`.
- D11 bị bác (đẩy tay): về K2 của 011.
- Trên Quest chuyển động hút mắt hay gây khó chịu: rút ngắn điểm đường hoặc về 0,9 m (chỉ Inspector).
- Trainee bỏ lỡ chuyển động: thêm âm thanh băng lăn định vị.
- Kiện rung/rơi ở quãng dài: cho kiện kinematic trong lúc chạy (kiểm lại không có trigger exit).

## Ràng buộc cho việc sau
- #28/#30: thời gian chuyến chạy (~20 s sau `task_completed`) phải log được hoặc suy được, để R tách khỏi thời gian làm bài và khỏi số liệu hướng nhìn/FPS. Hành vi này chốt trước người tham gia thật đầu tiên.

## Câu hỏi còn mở
- Người hướng dẫn: xác nhận D11, D15; D18 (ý nghĩa „In 40% der Fälle“ ở bước 10 của bảng bấm giờ; nhóm hiểu là tần suất bước đó xảy ra).

## Bằng chứng
- 011 K1, 013 hoãn C tới D11 → `docs/decisions/011-workstation-layout-conveyor.md`, `docs/decisions/013-warehouse-conveyor-network.md` → đã kiểm
- Bảng bấm giờ bước 9–10 (thùng kho và thùng đơn tự chạy đi, ghi chú tần suất) → `docs/requirements/old/LOU2_TC-K2+TMA_OIS2.xlsx`, sheet „OIS2 Kommi2“ → đã kiểm
- Hình học làn đơn (đo mesh trong Editor, 2026-11-02): làn trạm x ≈ 0,33 tới z ≈ 3,0; làn ngang z ≈ 3,0 tới x ≈ 5,0; làn dọc x ≈ 5,0 (z 2,8–7,2); thùng trạm trống x 4,82–5,18, z 4,64–5,36; góc là tấm vuông không con lăn → `TestEvidence/MixedPickingScene/bl50/topdown_corner.png` → đã kiểm
- `OrderBoxConveyor`: chỉ tịnh tiến, rider = vật có `PackageData` trong trigger (trừ kiện đang cầm) + `carriedWithContainer` → `Assets/Scripts/OrderBoxConveyor.cs` → đã kiểm
