# Quyết định 013: Băng lăn kéo dài tĩnh qua kho ở ba scene thực hành, gọi tên dòng thùng một lần ở ConceptScene

- **Backlog:** #49
- **Trạng thái:** accepted (câu Done when „0,9 m … rồi đứng yên“ sửa bởi 014, 2026-11-02)
- **Ngày:** 2026-11-02
- **Ý kiến đã xét:** unity-reviewer, requirements-analyst, xr-ux-reviewer, instructional-design-reviewer → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-02):** phương án B, có 1–2 khung trạm trống ở xa, làm ngay (nhánh riêng `feat/49-warehouse-conveyor` tách từ `feat/47-conveyor`, không merge trước buổi demo).

## Bài toán
Ở Basic, Mixed và Complex, băng lăn chỉ là một đoạn ngắn (z 0,47–1,85) và kết thúc dưới bàn sau. Trainee không thấy thùng kho (Lagerbehälter) đến từ đâu, thùng đơn (Auftragsbehälter) đi đâu sau khi xác nhận, và trạm của mình nằm trong hệ thống nào. Người dùng cho biết (2026-11-02): băng thật chạy quanh kho, qua các dãy kệ, và dọc đường có các trạm picking khác.

Ràng buộc:
- Decision 011 (K1): thùng đơn trượt ~0,9 m rồi đứng yên; không có thùng rỗng mới.
- Quy tắc bố cục trạm: vật thêm vào phải ở xa, ngoài vùng nhìn chính, không cầm được.
- Hiệu năng Quest, log nghiên cứu.

## Xung đột giữa các đề xuất
| Vấn đề | Một bên | Bên kia | Đánh đổi |
|---|---|---|---|
| Có chuyển động trên tuyến dài không | unity A, UX 1, ID 3: chỉ hình tĩnh | unity B, UX 2, ID 2: thùng đơn đi tiếp tới khi khuất; thùng kho trượt vào lúc đầu bài | Thấy nhân quả ↔ phải sửa 011, vướng D11, Rest lúc `Start`, log, chuyển động ngoại vi hút mắt |
| Có gọi tên khái niệm không | ID: phải gọi tên bằng tiếng Đức ít nhất một lần | UX: không có chữ trong vùng làm việc | Gọi tên ở ConceptScene, không đặt ở trạm |
| Thùng đơn đi đâu | ID, UX: Packbereich / Versand | requirements: tài liệu không nói | Không đặt tên điểm cuối khi chưa có xác nhận |
| Trạm khác dọc băng | UX, ID: khung trống, ≥ 4 m | requirements: chỉ có lời người dùng | Không người, không chuyển động |
| Cách dựng | unity: gộp mesh / instancing, không collider | UX: từ ~2 m trở ra dùng khối/dải | Tương thích, gộp lại |

## Các phương án
| | Đúng nghiệp vụ | Hiệu quả học | Tải nhận thức/UX | Chi phí/rủi ro | Nhất quán | Veto |
|---|---|---|---|---|---|---|
| 0. Băng ngắn + mũi tên | 0 | − không đạt mục tiêu | − tín hiệu nhân tạo | ++ | 0 | không đạt mục tiêu |
| A. Mạng băng tĩnh ở xa | + được phép | − có minh hoạ, không gọi tên | + ngoại vi, tĩnh | + không đụng `OrderBoxConveyor`; phải gộp mesh | + | — |
| **B. A + một bước sơ đồ ở ConceptScene** | + nếu không đặt tên điểm cuối của thùng đơn | ++ gọi tên trước, minh hoạ sau, không chấm điểm | + chữ chỉ ở Concept | − thêm một bước tour, chuỗi mới | + | — |
| C. B + chuyển động ở ranh giới nhiệm vụ | − khuếch đại giản lược chưa có nguồn (D11) | ++ | − chuyển động ngoại vi | −− sửa 011, Rest, log | 0 | V5 khi chưa sửa 011 |

## Phương án được chọn: B
- **Basic, Mixed, Complex:** mạng băng tĩnh.
  - Làn thùng kho đi từ khu kệ tới trạm; làn thùng đơn đi tiếp ra xa; một đoạn sau nối hai làn thành vòng.
  - Một prefab chung, mesh đã gộp, không collider, không `PackageData`, không chuyển động.
  - Không phần nào lọt vào ±15° quanh tâm màn hình phía trên mép dưới viền.
  - 1–2 khung trạm picking trống (bàn sau, màn hình tắt, hai thùng trên băng), cách trainee ≥ 4 m, không người.
  - Đoạn băng ở trạm và chuyển động của #47 (0,9 m, ~2 s, đứng yên) giữ nguyên.
- **ConceptScene:** thêm một bước cuối trong `WmsFieldTour` (bảng `Sample Description`). Bước này là sơ đồ dòng thùng bằng chữ, dùng `Lagerbehälter`, `Auftragsbehälter` của glossary, theo *Sie*, qua `localize-strings` và `german-reviewer`. Điểm cuối của thùng đơn ghi trung tính, chưa ghi `Packbereich`/`Versand`.
- **Được:** gọi tên ở Concept trước, minh hoạ ở ba bài thực hành sau. Không đụng counter, `OrderRule`, `OrderBoxConveyor`, log.
- **Mất / chấp nhận:**
  - Không thấy thùng đi tới tận cuối tuyến.
  - Sơ đồ chữ trừu tượng hơn chuyển động.
  - Tour ở Concept dài thêm một bước.
  - Phải kiểm FPS trên Quest.
- **Đề xuất bị bác:**
  - Chuyển động ở ranh giới nhiệm vụ (C): hoãn cho tới khi có trả lời D11, kèm sửa 011.
  - Băng ngắn + mũi tên: giữ làm phương án lùi nếu FPS không đạt.
  - Chỉ hình, không dạy: không có bước gọi tên.
  - Câu trên thẻ Aufgabe và biển chữ ở xa: thêm chữ vào vùng làm việc.

## Done when
- Ở Basic, Mixed, Complex, ảnh `overview` không cho thấy phần băng mới nào trong ±15° quanh tâm màn hình phía trên mép dưới viền màn hình.
- Quay đầu ~45° sang trái, trainee thấy làn trái chạy về phía dãy kệ. Quay ~45° sang phải, thấy làn phải chạy tiếp ra xa.
- Ba scene thực hành có cùng hình học băng, dựng từ một prefab chung.
- Phần băng mới không có collider, không có `PackageData`, không chuyển động.
- Đơn đúng ở cả ba scene: thùng đơn vẫn trượt ~0,9 m trong ~2 s rồi đứng yên, không có `ContentsChanged` (các case #47 vẫn đạt).
- Trên Quest, ở cả ba scene, FPS không thấp hơn trước khi thêm băng (OVR Metrics, ca thử tay).
- Ở ConceptScene, sau các bước WMS, bấm „Weiter“ → bảng `Sample Description` hiện sơ đồ dòng thùng với `Lagerbehälter` và `Auftragsbehälter`; mọi chữ có key + EN + DE và đã qua `german-reviewer`.
- Sơ đồ ở ConceptScene không đặt tên nơi đến của thùng đơn khi người hướng dẫn chưa trả lời.

## Nếu giả định sai
- FPS không đạt: rút ngắn mạng, bỏ khung trạm, hoặc lùi về băng ngắn + mũi tên. Chỉ thay prefab.
- Bảng `Sample Description` không đủ chỗ: rút gọn chữ, hoặc đưa lại cho ID/UX tìm chỗ khác, không đặt vào ±15° của 010.
- Người hướng dẫn nói băng không qua kho hoặc không có trạm khác: sửa hoặc bỏ phần mạng xa; đoạn ở trạm giữ nguyên.
- D11 = thùng tự chạy đi tiếp: mở lại phương án C bằng một bản sửa 011. D11 = đẩy tay: K2 theo 011, mạng tĩnh không đổi.

## Câu hỏi còn mở
- Người hướng dẫn: D15 (thùng đơn đi đâu sau trạm), D16 (băng có nối AKL với nhiều trạm, có trạm khác dọc tuyến không), D17 (thùng kho có tự đến và tự về theo băng không; bổ sung D10/D11).

## Bằng chứng
- 011: thùng đứng yên sau khi trượt, không thùng rỗng mới, D10/D11 mở → `docs/decisions/011-workstation-layout-conveyor.md` → đã kiểm
- Chuyển động #47: sự kiện đúng, travel 0,9 m, 2 s, rider chỉ là vật có `PackageData` → `Assets/Scripts/OrderBoxConveyor.cs` → đã kiểm
- `RollerConveyor.prefab` có 67 MeshRenderer, không collider → đã kiểm (synthesizer)
- Quy tắc bố cục trạm → `docs/backlog.md` dòng 11 → đã kiểm
- "ggf. Abschieben"; "Behälter voll" là thao tác của người → Kommissionierung slide 2, 10 → đã kiểm
- "Transport in der Anlage" → Kommissionierung slide 19 → đã kiểm
- AKL, Behälterpuffer, Lagerbehälter, Leerbehälter → bảng thuật ngữ Kommissionierung-Detail Description → đã kiểm
- Glossary có `Auftragsbehälter`, `Lagerbehälter` → `docs/i18n/glossary.csv` → đã kiểm
- Băng thật chạy qua kho, có trạm khác → lời người dùng, không có trong tài liệu → chưa kiểm
- Vị trí kệ/pallet so với tuyến, FPS trên Quest, chỗ trên bảng tour → chưa kiểm
