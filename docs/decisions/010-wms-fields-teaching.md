# Quyết định 010: Dạy các trường WMS trong ConceptScene trên màn hình chỉ xem; Basic chỉ nhắc ngắn

- **Backlog:** #20 (gộp cùng nhánh với #45; quyết định luôn phần "đặt `WMS_Monitor` vào Concept" của #45)
- **Trạng thái:** accepted
- **Ngày:** 2026-11-01
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, german-reviewer, data-analyst → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-01):** chấp nhận C; làm #20 và #45 chung một nhánh (sửa `ConceptScene.unity` một lần); đơn mẫu trong Concept hiện `Empf Kz` `003`; không cho khung sáng hay đánh dấu nào trên vùng dialog (chỉ chú thích ngoài viền màn hình).
- **Cập nhật của người dùng (2026-11-01, trong lúc làm, theo đề xuất của một phiên khác):**
  1. EKZ = Empfindlichkeitskennzeichen (có trong bảng thuật ngữ của Detail Description). `Empf Kz` gần chắc là viết tắt của nó (suy ra từ tài liệu); B16 chỉ còn là câu xác nhận.
  2. Bỏ đoạn giảng riêng cho `Anweisung`. Ô này gộp vào chú thích chung "không dùng trong bài" (R1, R2, R6, `Karton`, `Gew.`, `Anweisung`). Tài liệu chỉ nói nghĩa màu của ô, liên quan dập ghim ở bàn đóng gói; app không có bài nào như vậy. B17 không còn gắn với #20.

## Bài toán
Basic hiện đủ dialog `WMS_Kommissionierung` ngay khi vào scene (Soll 300, Ist 0, Rest 150, `Empf Kz` 003, `Anweisung` trống), nhưng chưa scene nào giải thích các trường. Thẻ 1 của Basic chỉ bảo trainee đọc Material, Soll, VPE và EKZ. Mục tiêu: trước khi dùng dialog ở Basic, trainee biết Soll/Ist/Rest, Empf Kz (EKZ 0 và 3) và Anweisung nghĩa là gì, và biết vùng nào không dùng trong bài. Ràng buộc: Rest theo nghĩa đã chốt (B6, 001), không ghi gì lên màn hình (quy tắc màn hình, backlog đầu file), không dạy từng đèn R2 (009), không ẩn trường (008 đã bác "mở dần từng vùng").

## Xung đột giữa các đề xuất
| Vấn đề | Một bên | Bên kia | Đánh đổi |
|---|---|---|---|
| Dạy ở đâu | ID, UX, data, german: ConceptScene (pretraining) | unity: thẻ ở Basic trước, chuyển sang Concept sau | Học tốt hơn, log Basic sạch ↔ giao sớm nhưng làm hai lần |
| Bước dạy ở đầu Basic | ID 2, UX 2: bước "Kennenlernen" có `Weiter` | data: cần sự kiện ranh giới pha, quiz trong Basic làm hỏng `confirm_count` (`analyse.R:493`) | Học liền kề ↔ nhiễu RQ1/RQ3, nhìn qua lại ~43°, chồng #40 |
| Khung trên dialog | UX 3, ID 2: khung quanh R3/R4 | Quy tắc màn hình | Người dùng chọn: không cho phép |
| Ẩn trường không dùng | *Done when* cũ cho phép | 008 đã bác | Không còn là lựa chọn |
| Ví dụ EKZ 3 | 003: ví dụ slide 8 | german: EKZ 3 cho carton là giản lược theo D4, chưa xác nhận; "lose" hai nghĩa | Dùng ví dụ chiếc lẻ, không phụ thuộc D4 |
| Ist | glossary: đếm tay thì Ist = Soll sẵn | app: cân thì Ist tăng dần (B7a) | Chỉ giảng cho trường hợp cân |
| Anweisung | tài liệu chỉ nói nghĩa màu, màu chưa xác nhận | B4, 008: ô trống, không tô | Một câu là đủ |

## Các phương án
| | Đúng nghiệp vụ | Hiệu quả học | Tải nhận thức/UX | Chi phí/rủi ro | Nhất quán | Veto |
|---|---|---|---|---|---|---|
| 0. Ẩn các trường không dùng ở Basic | 0 | − | + | + | − | V5 (008) |
| A. Concept: monitor chỉ xem + chú thích ngoài viền + giảng từng đoạn + câu nhận biết EKZ | + | + | + / − Concept dài hơn | − nguồn đơn tĩnh mới, `Weiter`, UI câu hỏi | + | — |
| B. Bước "Kennenlernen" ở đầu Basic | + | 0/+ (đơn 300/50 làm hộ phép tính) | − nhìn qua lại ~43°, thẻ dày | − ranh giới pha, sửa R | − | — |
| **C. A + nhắc ngắn trên thẻ Basic** | + | ++ | + | − như A + một chuỗi ở Basic | + | — |
| D. B ngay, A sau khi #45 chốt | + | 0 | − | 0, làm hai lần | − | — |

## Phương án được chọn: C
- **Concept:**
  - Đặt `WMS_Monitor` với dialog `WMS_Kommissionierung` ở chế độ chỉ xem. Dialog đọc một nguồn đơn mẫu cố định (class mới implement `IWmsOrderSource`, giá trị tĩnh, không bắn `ContentsChanged`). Soll, VPE và Rest của đơn mẫu khác 300/50/150 của Basic; `Empf Kz` `003`. `WmsDialogView` không đổi.
  - Chú thích nằm **ngoài** viền màn hình. Lời giảng 1–2 câu (≤ ~15 từ) trên bảng sát mép màn hình (không đặt trên bàn). Bấm `Weiter` để sang đoạn sau, mỗi đoạn một trường: `Mat.-Nr`/`Verp.-Einh.` (phần của #45) → `Soll` → `Ist` → `Rest` → `Empf Kz` → một chú thích chung "không dùng trong bài" cho R1, R2, R6, `Karton`, `Gew.`, `Anweisung` (cập nhật 2).
  - Câu nhận biết EKZ 0/3 sau phần giảng (theo 003), dùng ví dụ chiếc lẻ.
- **Cách làm (2026-11-01):** đơn mẫu STECKER C (dữ liệu giả, khoá `stecker_c`, Mat.-Nr 625417, Soll 120, Ist 40, Rest 360, Verp.-Einh. 40, Gew. 0,6, `Empf Kz` 003; không có kiện nào của vật tư này trong scene). Màn hình đặt cùng chỗ như Basic. Bảng Etikett (#45) bên trái màn hình, bảng giảng bên phải; carton KABELVERBINDER A dời sang trái để không che hàng dưới của dialog. Chú thích là mũi tên cam ngoài viền: một mũi tên chỉ hàng (bên trái) và một chỉ cột (trên hoặc dưới); nhóm "không dùng" là hai ngoặc (R1–R2 bên trái, R6 bên phải) và ba mũi tên dưới (`Karton`, `Gew.`, `Anweisung`). `Empf Kz` có hai đoạn (003, rồi 000). Trên bàn: 4 thẻ (Aufgabe, 1. Etikett lesen, 2. LVS kennenlernen, 3. Fragen beantworten), nút poke `Weiter` trước thẻ 2, nút A/B/C trước thẻ 3 (đáp án in trên thẻ câu hỏi vì ô nút nhỏ), thẻ phản hồi cạnh nút đỏ. Bốn câu hỏi: số carton từ Soll/VPE, vật tư theo Mat.-Nr, EKZ 003, EKZ 000.
- **Basic:** thẻ 1 gọi các trường bằng đúng nhãn trên màn hình (khuôn *Thuật ngữ (Nhãn)* đã có) và thêm một nhắc ngắn rằng Rest là tồn kho trong Lagerbehälter. Không thêm bước, không quiz, không khung trên dialog.
- **Ngôn ngữ:** giảng theo việc ô dùng để làm gì, không giải nghĩa từ thường ngày. Lần đầu nhắc hệ thống viết "Lagerverwaltungssystem (LVS)". Đáp án EKZ mô tả bằng động từ, không dùng "lose" đứng một mình. Sửa ghi chú Istmenge trong `glossary.csv` cho khớp hành vi cân.

- **Được:** pretraining thật, Basic chỉ còn tải của nhiệm vụ; học trên đúng dialog sẽ dùng; quy tắc màn hình giữ trọn; log Basic so được giữa các build; không chồng #40; câu nhận biết EKZ của 003 có chỗ.
- **Mất / chấp nhận:**
  - Concept nặng hơn; #20 làm chung nhánh với #45.
  - EKZ 3 chỉ ở mức nhận biết tới khi #41 xong (giản lược có ghi rõ).
  - Ist chỉ giảng cho trường hợp cân; trường hợp đếm tay không có ở bài nào (như 008).
  - Concept lẫn tiếng Anh và tiếng Đức tới khi #45/#12 bản địa hoá hết.
- **Bị bác:** bước "Kennenlernen" ở Basic (ID 2, UX 2); thẻ dạy ở Basic trước rồi chuyển (unity B, D); khung quanh R3/R4 (người dùng không cho); câu "Warum ändert sich Rest nicht?" sau `Richtig!` (để sau, chạm #14/#33); ranh giới pha trong log (data B, không cần vì Basic không có bước mới); dạy Anweisung theo màu; ẩn trường (V5).

## Done when
- Mở ConceptScene: thấy `WMS_Monitor` với dialog hiện một đơn mẫu; Soll, VPE và Rest khác 300, 50, 150; `Empf Kz` hiện `003`. Console không có `WmsDialogView: orderSource is not assigned`.
- Trên vùng màn hình của dialog không có chữ, khung hay mũi tên nào ngoài các nhãn WMS; mọi chú thích nằm ngoài viền màn hình.
- Bấm `Weiter` thì chú thích chuyển sang trường tiếp theo theo thứ tự `Mat.-Nr`/`Verp.-Einh.` → `Soll` → `Ist` → `Rest` → `Empf Kz` → "không dùng" (R1, R2, R6, `Karton`, `Gew.`, `Anweisung`); mỗi đoạn nói một trường.
- Đoạn `Rest` nói đây là tồn kho còn lại trong Lagerbehälter sau khi lấy hàng; không chuỗi nào dùng Rest với nghĩa "còn thiếu".
- Đoạn `Ist` nói Ist tăng dần khi cân, không viết thành quy tắc chung.
- Đoạn "không dùng" nói ô `Anweisung` trống trong bài tập và không nêu tên đèn nào.
- Đoạn `Empf Kz` dạy EKZ 0 và EKZ 3 bằng con số và ví dụ chiếc lẻ, không dựa vào màu; có một câu ngắn rằng còn các cấp EKZ khác.
- Câu nhận biết EKZ: chọn sai thì feedback nêu lý do; đáp án mô tả bằng động từ; mỗi câu có ID ổn định, mỗi đáp án có khoá ngắn.
- Vào BasicPickingScene: thẻ 1 gọi các trường bằng đúng nhãn trên màn hình (`Soll`, `Verp.-Einh.`, `Empf Kz`), có nhắc ngắn rằng Rest là tồn kho trong Lagerbehälter, không in con số nào của đơn.
- Basic không có bước dạy hay quiz mới; dialog Basic và Complex hiện như trước khi sửa.
- Mọi chuỗi mới có key + EN + DE qua `localize-strings` và đã qua `german-reviewer`; nhãn WMS không bị dịch.
- Không ghi sự kiện log mới (để #28).
- Console không có lỗi mới ở ConceptScene, BasicPickingScene, ComplexScene.

## Nếu giả định sai
- **B16 = `Empf Kz` không phải EKZ:** sửa chuỗi một đoạn và một câu hỏi; ID câu hỏi giữ nguyên.
- **D4 sai (carton đặt lỏng):** câu hỏi dùng ví dụ chiếc lẻ nên không đổi; chỉ #41 phải xem lại.
- **Concept quá dài khi thử trên Quest:** gộp hai đoạn `Empf Kz`, hoặc bỏ câu hỏi EKZ thứ hai.
- **Chú thích không đọc được trên Quest:** dời bảng hoặc phóng to chú thích; không đổi scale dialog ở Concept nếu Basic không đổi theo.
- **Người dùng sau này cho phép đánh dấu trên dialog:** thêm khung vào child R3/R4 (Concept trước), phần còn lại giữ nguyên.

## Câu hỏi còn mở
- Người hướng dẫn: B16 (xác nhận `Empf Kz` = EKZ; suy ra từ tài liệu, gần chắc chắn), B3, D4, D5 (vẫn tạm). Không câu nào chặn #20. B17 (nội dung ô Anweisung) chỉ cần nếu sau này có bài với túi PU.
- Ghi chú cho #28: khi log câu hỏi của Concept, theo các yêu cầu của data-analyst: `question_id` cố định, `selected` là khoá ngắn, `attempt_no` theo từng câu, có mẫu số (`question_shown` hoặc danh sách câu cố định, câu không trả lời ra NA), map skill theo `question_id` (thêm skill `read_wms`, có thể `handle_ekz`); sửa spec, `make_fake_data.R`, `analyse.R`, `selftest.R` cùng lúc.

## Bằng chứng
- Thẻ 1 Basic → `.claude/cache/scene-inventory/BasicPickingScene.md:52` → đã kiểm
- Concept: panel tiếng Anh, chưa có monitor → `ConceptScene.md:25–40` → đã kiểm
- Prefab có child `R1_Modus` … `R6_Funktionstasten` → `WMS_Kommissionierung.prefab` → đã kiểm
- `WmsDialogView` cần `orderSource` implement `IWmsOrderSource`, log lỗi nếu null → `Assets/Scripts/WmsDialogView.cs:56–64` → đã kiểm
- Ẩn/mở dần từng vùng bị bác; child theo vùng để #20 dùng → `008-wms-dialog-panel.md:28, 38` → đã kiểm
- Không dạy từng đèn R2 → `009-wms-indicators-off.md:17` → đã kiểm
- #20 dạy EKZ 0/3, câu nhận biết đặt ở #20 → `003-ekz-placement.md:39, 75` → đã kiểm
- Nghĩa Soll/Ist/Rest, bảng EKZ → Kommissionierung slide 7–8 (báo cáo requirements-analyst) → đã kiểm
- Rest = tồn kho thùng kho; Istmenge khi đếm tay → `docs/i18n/glossary.csv:10, 12–13` → đã kiểm
- `quiz_answer` gộp với `confirm_pressed` → `analysis/analyse.R:493` → đã kiểm
- Góc ~43° giữa dialog và thẻ → tính toán của xr-ux-reviewer → chưa kiểm
- Chỗ trống trên bàn Concept cho monitor và bảng; chữ chú thích đọc được trên Quest → chưa kiểm
