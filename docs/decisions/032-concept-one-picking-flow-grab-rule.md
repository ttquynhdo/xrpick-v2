# Quyết định 032: ConceptScene chỉ có một chuỗi thao tác tại Lagerbehälter; vật trông như kiện thì cầm được, ngoại lệ có ký hiệu và một dòng lý do

- **Backlog:** #92 (viết lại)
- **Trạng thái:** accepted
- **Ngày:** 2026-11-05
- **Thay thế (một phần):**
  - 025: demo đếm 3 carton tĩnh ở bước `cartons`, và 4 kiện cầm được nằm ngoài hai thùng.
  - 026 mục 5: bán trong suốt, viền, „Nur zum Ansehen“ khi chạm. Thay hẳn.
  - 030 mục 2–3: lời nhắc sai vật liệu không còn vật nào để kích hoạt ở Concept. Code giữ nguyên.
  - 031: câu „kiểu chỉ để xem làm trong #92“ và câu „thẻ mẫu không có key mới“ (thẻ 003 có thêm một dòng lý do).
- **Ý kiến đã xét:** hai vòng. Vòng 1 xét quy tắc vật cầm được, có bản nháp chưa được chấp nhận. Vòng 2 gộp vòng 1 với mối lo của người dùng về „hai việc cầm nắm rời nhau, kiện ở mép bàn“. Cả hai vòng đều qua requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, data-analyst rồi decision-synthesizer.
- **Lựa chọn của người dùng (2026-11-05):**
  - Chấp nhận phương án A, kèm mục „Mục tiêu bài Concept“.
  - Câu quiz Mat.-Nr viết lại theo kiểu „cho Mat.-Nr, chọn nhãn khớp“ (mặc định, người dùng không phản đối).
  - Giữ 6 carton trong thùng kho.
  - Biển tên thùng chỉ hiện ở bước 5, 6, 11.

## Bài toán
Người dùng muốn:
- Mọi vật trông như kiện đều cầm được, vì cầm nắm là lợi thế của VR.
- Vật nào không cầm được phải có lý do thật, giải thích **một lần** bằng ký hiệu kèm một dòng chữ, không phải mỗi lần chạm lại hiện thông báo.

Ở ConceptScene còn hai việc cầm nắm rời nhau:
- Thẻ 1 bắt cầm 4 kiện xếp hàng ở mép bàn. Mép bàn cách mắt khoảng 0,87 m, phải cúi khoảng 68°.
- Bước 5 lại diễn ra ở thùng kho và thùng đơn.

Ngoài ra còn 4 nhóm vật trông như kiện mà không cầm được: 6 carton thùng kho, 3 carton của demo đếm, 2 thùng mẫu EKZ, và carton đã mở ở Complex.

## Tài liệu nói gì (requirements-analyst)
- **Trước khi lấy hàng:** người lấy hàng xem dialog và Lagerbehälter, rồi đếm hoặc cân (Kommissionierung slide 2–3).
- **Thuật ngữ (Glossar, Detail Description):** Lagerbehälter là thùng chứa hàng kho. Rest là tồn trong Lagerbehälter. Thùng rỗng ở trạm trở thành Auftragsbehälter.
- **Không có trong tài liệu:**
  - hàng nằm ngoài Lagerbehälter;
  - việc „chọn đúng vật liệu giữa nhiều loại“.
- **Carton:** được đặt lỏng vào Auftragsbehälter, „EKZ gerecht“ (slide 6/9).
- **Ist:** chạy theo từng lần đặt chỉ khi cân (slide 7). Caption Ist „Beim Wiegen …“ đã nói đúng điều này.
- **Anbruchmenge:** phải được lưu trong một Modul (Detail Description 5.12). Câu hiện có „… im Modul im Lagerbehälter“ khớp ý này. A13 vẫn là câu hỏi mở.
- **Đọc nhãn và so với dialog:** tài liệu không nói, cũng không cấm. Đây là đơn giản hoá cho việc học (câu hỏi D39).

## Các phương án
- **A. Một chuỗi tại Lagerbehälter:** bỏ 4 kiện ở mép bàn, 6 carton STECKER C cầm được, người học tự đặt hàng. **Chọn.**
- **B. A + khay mẫu** chứa 3 vật liệu khác, đặt trước Lagerbehälter: bị bác.
  - Khay vẫn nằm sát mép bàn, phải cúi khoảng 64°.
  - Vẫn còn hai nhóm đồ cầm.
  - Khay không có ở trạm thật.
- **C. Giữ 4 kiện, thẻ 1 thành „tìm đúng vật liệu“:** loại. Cách này dạy lấy hàng ngoài Lagerbehälter, trái định nghĩa trong Glossar.
- **D. Chỉ cho carton thùng kho cầm được, giữ kiện ở mép bàn** (bản nháp vòng 1): không giải quyết sự rời rạc.

## Quyết định: phương án A

### Mục tiêu bài Concept
| Nội dung | Mức | Kiểm tra |
|---|---|---|
| Các ô LVS: Mat.-Nr, Bez., Verp.-Einh., Soll, Ist, Rest, Empf Kz | phải hiểu và dùng được | gián tiếp qua quiz |
| So nhãn carton với LVS | phải làm được | quiz Mat.-Nr (câu mới) |
| Soll : Verp.-Einh. = số carton | phải làm được | quiz `cartons_from_soll` |
| Empf Kz 003 (từng cái, có thứ tự) / 000 (để tự do) | phải phân biệt được | quiz `ekz_003`, `ekz_000` |
| Lagerbehälter / Auftragsbehälter | phải biết, và tự đặt hàng giữa hai thùng | chỉ làm, không chấm |
| 3 làn, đường đi của thùng | biết qua | không |
| 8 cấp EKZ | biết là có; bài tập chỉ dùng 0 và 3 | không |
| Ô LVS không dùng (Anweisung …) | biết là bỏ qua | không |

Mỗi vật và mỗi chữ trong scene phải gắn với một dòng của bảng này. Nội dung ở mức „biết qua“ được trình bày nhẹ (chỉ ở bước của nó) và không được chấm.

### Vật nào cầm được, nằm ở đâu
- Mép bàn không còn kiện nào. Bỏ `KABELVERBINDER A`, `SCHRAUBE B`, `STECKER_C_Package` và carton RING D đã mở (cùng 15 vòng) khỏi Concept.
- 6 carton `Stock STECKER C` trong `PlasticBox_Storage` thành kiện cầm được:
  - `PackageData` `stecker_c` / 625417 / 40, `Grabbable`, rigidbody kinematic, collider, như `STECKER_C_Package` trước đây;
  - mỗi carton có tên duy nhất, cố định.
- `STECKER C Marker` (mũi tên cam) chuyển lên carton ở đầu thùng kho.

### Chuỗi thao tác
1. **Thẻ 1 („1. Etikett lesen“):** lấy một carton trong Lagerbehälter và đọc nhãn.
   - Thẻ có ✓ khi cầm carton STECKER C lần đầu. `ConceptCardProgress` tính số vật liệu từ các kiện có trong scene, nên chỉ còn STECKER C thì con số tự thành 1.
   - Thanh tour mở khi thẻ 1 xong, hoặc sau 60 s như cũ.
2. **Bước `material`, `verp_einh`:** cầm chính carton đó, so nhãn với LVS. `action_held` và `HeldLabelTags` giữ nguyên.
3. **Bước `cartons`:** bỏ `Carton Count Demo`. Người học tự đặt carton từ Lagerbehälter vào Auftragsbehälter.
   - Mỗi carton đặt vào, Ist lên 40, 80, 120, qua `SampleWmsOrder.SetPlacedPieces` của #100.
   - Thẻ tổng kiểu ô LVS („40“, „80“, „120 = Soll“) hiện trên carton vừa đặt.
   - Đặt carton thứ 4 thì hiện thẻ vàng `too_many`.
   - Dòng hành động mới nói rõ: lấy carton từ Lagerbehälter, đặt vào Auftragsbehälter.
   - Nút „Weiter“ không bị khoá.
4. **Rest:** cố định 120, là phần còn lại dự kiến (tồn − Soll), giống Basic.
5. **Bước 11 `material_flow`:**
   - Nếu Auftragsbehälter chưa đủ 3 carton, cảnh tự chuyển các carton còn thiếu từ Lagerbehälter sang cho người học thấy, Ist thành 120, rồi thùng đơn mới chạy cùng các carton đó.
   - Đặt thừa thì chờ `IsClear` như hiện nay.
   - „Von vorn“, sang bước khác hoặc tắt scene: cả 6 carton về đúng chỗ trong Lagerbehälter và cầm được. Thùng đơn rỗng, Ist 0.

### Biển tên thùng
- Biển „Lagerbehälter“ và „Auftragsbehälter“ (key có sẵn `concept.lane.*`) là con của hai thùng, nên ở bước 11 chạy cùng thùng.
- Chỉ hiện ở bước 5, 6 và 11.
- Ở bước 11 hai biển này thay biển tên thùng trên mái che và mép bàn sau. „Mittlere Bahn“ giữ nguyên.
- Mỗi biển một từ. Biển không được chồng lên thẻ gợi ý của #100.

### Vật không cầm được: chỉ còn hai nhóm
- **Ký hiệu:** một ký hiệu, một nghĩa trong mọi scene: bàn tay gạch chéo, đối lập với icon bàn tay „hãy làm“ trên thanh tour.
  - Không dùng vàng hay icon `warning` (nghĩa là „sai“ ở Box Hints). Không dùng đỏ (màu nút xác nhận).
  - Vật vẫn đặc. Không bán trong suốt, không có chữ khi chạm.
- **Thùng mẫu EKZ (031):** ký hiệu trên cả hai thẻ mẫu. Thẻ 003 có thêm một dòng lý do kiểu „Beispiel, nicht zum Greifen“. Concept là nơi người học gặp ký hiệu này lần đầu, nên giải thích ở đây.
- **Carton đã mở ở Complex (`VPE_OpenedPackage`, `SB_OpenedPackage`):**
  - ký hiệu trên vách ngoài hoặc nắp, hướng về chỗ đứng, không che vòng;
  - dòng lý do trên `Storage Label` của thùng kho, cùng ý với câu A13 hiện có;
  - **Đã sửa ở #111 (2026-11-06):** ký hiệu và dòng lý do gộp thành một tờ dán phẳng trên vách trước của carton đã mở (`Opened Carton Note`, „Nehmen Sie nur die losen Teile heraus, der Karton bleibt hier.“, không còn „Modul“); `Opened Carton Mark` và thẻ nổi trên `Storage Label` bỏ đi;
  - câu về Anbruch ở dòng kết quả vị trí 1 bỏ đi, để chỉ nói một lần;
  - vòng bên trong vẫn cầm được. `VPEOpenedDisplay` và Rest không đổi.
- **Cảnh nền:** kệ hàng ở xa và các trạm trống cách khoảng 7 m, ngoài khu bàn làm việc, không cần ký hiệu.

### Quiz
- Câu `material_by_matnr` được viết lại: cho một Mat.-Nr như trên LVS, hỏi nhãn nào khớp, các nhãn in sẵn trên nút A/B/C.
- Câu mới có `question_id` mới.
- Số trong quiz khác số của bài mẫu, như cũ.

### Thẻ việc
- Thẻ „Aufgabe“ nêu đúng các việc chính: đọc nhãn và LVS, tính số carton, đặt hàng từ Lagerbehälter vào Auftragsbehälter.
- Thân thẻ 1 nói lấy carton từ Lagerbehälter.
- Tiêu đề ba thẻ bước giữ nguyên.

### Log
- `layout_version` "032".
- Concept ghi `container_*` (event đã có, `container` = `order` / `storage`), chỉ từ "032".
- Spec bỏ câu nói `TOO_MANY` do demo gây ra.
- `make_fake_data.R` thêm một người "032" có grab carton kho, `container_*` ở Concept, và `TOO_MANY` do người học gây ra.
- `analyse_set.R` sửa chú thích 13d và đọc `container_*` của Concept.
- Spec và R sửa trong cùng một change, `selftest.R` phải xanh, `data-analyst` duyệt.
- Không có sự kiện mới cho ký hiệu hay việc chạm.

## Được và mất
- **Được:**
  - Một nhiệm vụ liền mạch, đúng nơi người thật lấy hàng: đọc nhãn → so với LVS → chia 120 : 40 → tự đặt 3 carton, Ist phản hồi ngay.
  - Hết chỗ cúi ở mép bàn. Ít vật và ít chữ hơn.
  - Concept và Complex cùng một quy tắc.
  - Chỉ còn hai nhóm vật không cầm được, mỗi nhóm có lý do thật.
- **Mất:**
  - Concept không còn cho xem trước túi và carton đã mở. Mixed vị trí 2 và Complex dạy chúng đúng lúc dùng.
  - Lời nhắc sai vật liệu không còn vật để kích hoạt ở Concept.
  - Câu quiz Mat.-Nr đổi, nên số liệu Concept trước và sau không gộp được. R tách theo `layout_version`.

## Đề xuất bị bác
- **Khay mẫu** (unity, data, xr-ux, ID B): xem mục Các phương án.
- **„Rest chạy theo số carton“:** Rest là phần còn lại dự kiến, giống Basic.
- **Sửa `ConceptCardTracker`:** không cần, số vật liệu được tính từ scene.
- **Đổi tiêu đề thẻ bước sang mẫu Basic:** để sau, vì thêm chữ mà không thêm gì cho việc học.
- **Thùng mẫu EKZ chỉ có ký hiệu, không chữ:** quy tắc của người dùng cần giải thích một lần, mà Concept là nơi gặp ký hiệu đầu tiên.
- **Sự kiện `concept_count_changed`:** đã có `container_*` đủ dựng lại bước đếm.
- **Giữ demo rồi „Jetzt Sie“:** để dự phòng (xem mục dưới).

## Nếu giả định sai
- **D12 sai** (một Lagerbehälter chứa được nhiều Mat.-Nr): đặt vật liệu thứ hai ngay trong Lagerbehälter. Lời nhắc sai vật liệu của #100 dùng lại được ngay.
- **Người lấy hàng không đọc nhãn (D39):** bỏ câu so nhãn ở thẻ 1 và bước `material`. Chuỗi đếm vẫn đúng.
- **Test kính cho thấy người học không tự đặt được carton, hoặc bước 5 quá dài:** thêm một lần xem demo trước khi tự làm.
- **Ký hiệu không được hiểu:** đổi sprite, hoặc thêm một dòng ở bước 8.
- **A13 sai:** sửa một dòng trong CSV.
- **Test kính thấy thiếu phần xem trước túi hoặc carton mở:** thêm lại một vật trong Lagerbehälter của một bước khác, không đụng chuỗi chính.

## Câu hỏi mở
D39 (mới, đọc nhãn trước khi lấy), D12, A13, cân hay đếm với STECKER C (D40, mới).

Xem `docs/backlog.md` #92.
