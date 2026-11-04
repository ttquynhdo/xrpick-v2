# Quyết định 025: ConceptScene dạy bằng vật thật trên trạm (carton STECKER C, ví dụ 3 carton, mẫu EKZ bằng RING D, băng ba làn có thùng chạy ở bước `material_flow`), giữ bố cục của 024

- **Backlog:** #78, #79, #80, #81 và mục mới #83
- **Trạng thái:** accepted (sửa một phần bởi 026, 2026-11-04: thẻ nhãn khi cầm, „không khoá tour“, vật minh hoạ)
- **Ngày:** 2026-11-04
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, data-analyst → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-04):** chấp nhận phương án B; thùng ở bước 11 tự chạy khi tới bước (không có nút „Vorführen“); thẻ 1 có ✓ sau khi cầm cả 4 kiện (thêm STECKER C); bỏ bảng Ablauf khỏi bước 11.
- **Bổ sung / sửa quyết định đã chấp nhận:**
  - **024:** bổ sung lớp „nhìn/làm“ và #81. Thẻ 1 tính 4 kiện (024 chọn 3). `layout_version` = "025". Bảng Ablauf không còn hiện ở bước 11. Bố cục, thanh điều hướng, quiz, ✓ + khung giữ nguyên.
  - **019, phần Concept:** bỏ hai mục mà 019 đã bác cho Concept („trình diễn ba làn ở Concept“, „mũi tên ở Concept vì không có làn để chỉ“). Bảng 10 bước rời khỏi `markers` của `material_flow`. Câu „ở trạm thật thùng được kéo bằng tay“ chuyển vào chú thích bước 11.
  - **010:** câu „không có kiện nào của vật tư này trong scene“ không còn đúng (thêm 1 carton STECKER C). Thứ tự 11 bước, đơn mẫu và quy tắc màn hình giữ nguyên.
  - **018:** giữ nguyên; thêm hai mẫu EKZ 3D, bảng vẫn là chỗ tra cứu ở bước 9–11.
  - **015, 020:** không sửa thêm ngoài phần 024 đã sửa.

## Bài toán
- Sau ConceptScene người học phải: đọc nhãn và khớp với LVS; biết các ô Mat.-Nr/Bez., Verp.-Einh., Soll, Ist, Rest, Empf Kz; tính số carton = Soll : VPE; phân biệt EKZ 3 với EKZ 0; hiểu luồng ba làn; trả lời 4 câu hỏi.
- Ngày 2026-11-04 người dùng đặt nguyên lý: dùng môi trường VR tối đa, người học học bằng cách **nhìn và làm**, không đọc chữ rồi tự hình dung („đọc chữ thì khác gì xem slide“).
- Video test P811: quá nhiều bảng; cụm câu hỏi che màn hình; bảng Ablauf ở +86° không ai nhìn thấy; trong scene không có kiện STECKER C của đơn mẫu, nên không khớp nhãn với LVS bằng tay được; EKZ 000 được hỏi trong quiz nhưng chưa từng được cho xem.
- Quyết định 024 đã sửa bố cục cho #78–#80 nhưng các bước vẫn chủ yếu là chữ; #81 chưa có quyết định.

## Các phương án
| | Đúng nghiệp vụ | Hiệu quả học | Tải nhận thức/UX | Chi phí/rủi ro | Nhất quán |
|---|---|---|---|---|---|
| 0. Chỉ làm 024 + sơ đồ tĩnh ở bước 11 | 0 | − vẫn là đọc; không khớp nhãn được | + | ++ S–M | 0 |
| A. 024 + carton STECKER C + băng ba làn đứng yên có mũi tên | + | 0/+ thấy làn, không thấy luồng | + | + M | + |
| **B. 024 + lớp nhìn/làm, không khoá tour** | + | ++ mỗi câu hỏi có phần cho xem trước | + chuyển động trong vùng nhìn chính, thêm vật không thêm bảng | − M–L | ++ trạm giống Basic |
| C. B + khoá/tự qua bước theo hành động + event mới | + | + | − nguy cơ kẹt (như #76) | −− đổi nghĩa `tour_step`, sửa spec/R | − |
| D. Ba trạm, người học đi lại | 0 | + | − | −− 5+ ngày | −− |

## Quyết định: phương án B
- **Bước `material`, `verp_einh`:** một carton STECKER C cầm được (`PackageData` `stecker_c` / 625417 / 40, nhãn trùng `SampleWmsOrder`), có marker cam; cầm lên thì `Info Panel` hiện 625417 / VPE 40, nhãn và ô WMS thấy cùng lúc. Không cầm vẫn bấm „Weiter“ được.
- **Bước `cartons`:** 3 carton STECKER C tĩnh (không cầm được) hiện lần lượt ~0,6 s, trên đó „40“, „80“, „120 = Soll“; chú thích còn một dòng „120 : 40 = 3 Kartons“.
- **Bước `empf_kz_003` / `empf_kz_000`:** thùng đơn mẫu nhỏ có RING D xếp từng chiếc thẳng hàng („Empf Kz 003“), rồi thêm thùng RING D đổ lộn xộn („Empf Kz 000“, chữ nói „dürfen“, không nói „müssen“). Bảng EKZ của 018 vẫn hiện ở bước 9–11.
- **Bước `material_flow`:** `RollerConveyorThreeLanes` thật ở bố cục như Basic, thùng kho ở work spot, thùng đơn bên phải. Tới bước 11: thùng kho sang làn giữa và chạy ra sau, thùng kế tiếp vào từ trái, rồi thùng đơn ra sau; ≤ ~15 s; dùng thẳng `LaneRide` (không `PositionStockSwitcher`, nên không có `station_move_*`). Nhãn tĩnh trên làn dùng đúng từ của chú thích. Chú thích có câu „ở trạm thật bạn đẩy thùng bằng tay“ (giả định D11/D17), không gọi tên nơi đến (D15).
- Các ô không làm bằng tay được (Soll, Ist, Rest, không dùng) vẫn là chú thích ngắn theo 024, tối đa 2 câu.
- **Log:** 11 `step_id`, 4 `question_id` và mọi event giữ nguyên, không event mới. Cầm STECKER C ghi bằng `grab_start` (#29); đã xem demo suy ra từ `tour_step` `material_flow`. Header `layout_version` "025".

**Được:** nguyên lý „nhìn và làm“ áp ở 4 chỗ (nhãn, số carton, EKZ, luồng ba làn); trạm Concept giống bài 3–5; hợp đồng log giữ nguyên.

**Chấp nhận mất:**
- Công lớn hơn 024 (M–L).
- Người học vẫn có thể bấm „Weiter“ mà không cầm carton (tránh kẹt bài, log không đổi nghĩa).
- Bảng 10 bước không còn hiện; nhóm „Nicht in dieser Übung“ không được luyện hay kiểm tra nên mất ít.
- Mẫu EKZ dùng chiếc lẻ, Basic dùng carton: đúng slide 8 xếp trên nhất quán; EKZ 3 cho carton vẫn là đơn giản hoá D4 (003), không nói thành quy tắc.
- Bước 11 dài thêm ~10–15 s, chưa đo tổng thời gian bài.

**Đề xuất bị bác:**
- Ist → „Nicht in der Übung“: sai, ở Basic/Mixed/Complex Ist tăng thật khi hàng vào thùng.
- Tự qua bước / „Weiter“ chỉ sáng sau khi cầm: rủi ro kẹt, phá cách suy hướng của #82. Dự phòng: làm mờ, không khoá.
- Nút „Vorführen“: người dùng chọn tự chạy.
- Next/Back dưới bảng tour: trái với 024 (thanh cố định dưới màn hình), không có lý do mới.
- Người học tự đặt 3 carton vào thùng đơn: Concept không có validator, EKZ chưa dạy ở bước 5, đặt sai không được báo.
- Sơ đồ tĩnh / thùng đứng yên: chỉ thấy làn, không thấy luồng; giữ làm dự phòng nếu Quest giật.
- EKZ bằng carton thả vào thùng: bảng slide 8 nói về từng chiếc.
- Ba trạm: chi phí cao, trạm thật đứng một chỗ.
- Event `concept_action`, trường `direction`/`advanced_by`/`concept_version`: không cần khi không có tự qua bước; `layout_version` đã có.

## Làm gì nếu giả định sai
- Người học bỏ qua carton STECKER C → làm mờ (không khoá) „Weiter“ ở bước 2 tới lần cầm đầu.
- Bước 11 quá dài hoặc Quest giật → thùng đứng yên + mũi tên (phương án A), bố cục không đổi.
- Người học cần xem 10 bước → đưa bảng Ablauf lại `markers`, đặt trong ±30°.
- D11/D17 sai (thùng thật tự chạy) → bỏ câu „đẩy bằng tay“, demo giữ nguyên.
- D4/D5 đổi → thay vật trong thùng mẫu EKZ; ID câu hỏi không đổi.
- Tất cả là sửa marker, transform hoặc CSV; hợp đồng log không đổi.

## Câu hỏi còn mở (không chặn, kiểm ở buổi test)
- D11/D17 (đã có), D32 (mới): đủ chưa khi người học chỉ thấy đường đi của thùng một lần tự động, hay cần biết ai đẩy thùng nào ở trạm thật.
- Quan sát: người mới có tự cầm carton STECKER C không; cả bài còn ≤ ~5 phút không.

## Thứ tự làm và công
#76 (S) → #78 + #80 (M–L, theo 024) → #79 (S–M) → #83 (M) → #81 (M–L); #83 và #81 đổi chỗ được. Tổng ~6–8 ngày, chưa tính test kính. *Thay bằng thứ tự ở mục Bổ sung 2026-11-04.*

## Bổ sung 2026-11-04 (đo bố cục ConceptScene trong Editor)
Người dùng chấp nhận 2026-11-04. Phương án B giữ nguyên; chỉ đổi thứ tự làm và chốt chỗ đặt vật.

**Đo được:**
- Chỗ của băng đang có kiện: 14 vòng RING D ở (0,47–0,57; 0,9; 0,7) đúng chỗ thùng đơn (0,453; 0,768; 0,712); KABELVERBINDER A ở (−0,62; 0,77; 0,80) trên làn trái; SCHRAUBE B ở (0,23; 0,76; 0,62).
- Menu chiếm x 0,42–0,62, y 1,07–1,23, z 0,21–0,32, tức đúng chỗ „bên phải trong tầm tay“ mà 024 mục 4 dành cho cụm câu hỏi.
- Giữa `Info Panel` (x ≤ −0,49) và viền trái dialog (x ≥ −0,38) chỉ có 11 cm, nên chú thích trái phải dùng ngay phương án dự phòng của 024 (dời `Info Panel`).
- Màn hình ở z ≈ 1,07, tay không với tới. Thanh điều hướng ở z ≈ 0,5 chỉ không che dialog từ mắt (0; 1,6; 0) khi cạnh trên thấp hơn y ≈ 1,22 (cụm quiz hiện tại lên tới 1,28 nên che hàng dưới).

**Sửa:**
1. Bố cục làm trước, một lần, ở mục mới #84 (băng, thùng, kiện, chỗ thùng mẫu EKZ, chỗ thanh và cụm câu hỏi, dời `Info Panel`). Vị trí trong #78–#83 lấy từ test report của #84.
2. Cụm câu hỏi ở bước 11 nằm ở dải giữa, cạnh thanh điều hướng, bên trái Menu, không đặt bên phải. Menu giữ chỗ như ở 5 scene.
3. Thùng kho chứa carton STECKER C tĩnh (vật tư của đơn mẫu). 3 carton của bước `cartons` hiện trong thùng đơn. Như vậy ở bước 11 người học thấy thùng kho mang STECKER C đi và thùng đơn mang 3 carton đi. 4 kiện cầm được nằm ngoài cả hai thùng, nên demo không bao giờ mang chúng theo.
4. Các vòng RING D lẻ chuyển thành vật tĩnh trong thùng mẫu EKZ (#83), hoặc bỏ.
5. Thẻ 1 (#79) đếm theo 4 `MaterialKey` khác nhau (carton RING D hay một vòng lẻ đều tính là RING D).
6. Thanh điều hướng: mỗi nút có vùng chạm riêng, vừa với nút (bài học của #76).
7. Một build duy nhất, `layout_version` "025" (không build nào ghi "024"). R (#82) chỉ tính Back với log "025"; "019" hoặc không có trường thì không tính.
8. ComplexScene `Remainder Quiz` cũng có „Richtig!“ nằm dưới câu sau (và đè lên „Frage 2 von 2“): mục mới #85, cách giữ 1,5 s giống 024 mục 5.

**Thứ tự mới:** #76 (merge sau test tay H1–H3) → #84 → #80 → #78 (+ #85, khác scene, song song được) → #79 → #83 → #81 → #82. #84, #78–#81 và #83 làm tuần tự trên một nhánh, mỗi mục có test report riêng; `german-reviewer` duyệt mọi chuỗi mới một lần; một buổi test kính chung.
