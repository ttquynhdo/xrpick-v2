# Quyết định 043: Năm sửa lỗi sau buổi test kính P811 (nút poke, túi, bàn tập Welcome, vật mỏng ở miệng thùng, thẻ kết quả ngắn), làm chung một build

- **Backlog:** #177, #178, #179, #180, #181
- **Trạng thái:** accepted
- **Ngày:** 2026-11-26
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, german-reviewer, data-analyst; tổng hợp bởi decision-synthesizer (2026-11-25)
- **Lựa chọn của người dùng (2026-11-26):** phương án A (một build, một lần tăng `layout_version` lên "041"). Hai câu phụ chưa được trả lời riêng, nên theo khuyến nghị: câu nhìn băng lăn giữ ở mọi thẻ đúng; với `missing_loose`/`extra_loose` làm theo thứ tự ở mục 2 và hỏi lại người dùng nếu cả hai cách đều không vừa.
- **Sửa một phần:** 033 (thẻ đúng chỉ còn câu nhìn băng lăn; bỏ „Lesen Sie dann … Position … ab“), 168/#168 luật 5 (chỉ cho vật mỏng, mục 3), 042 mục 3 (khay „Modul“ nay cũng nằm trong thùng kho ở Welcome, mục 5).

## Bài toán

Buổi test kính P811 (build a45f3fd, layout "040", 2026-11-25) tìm ra năm lỗi:
- nút poke trên canvas (Next/Back của tour, Menu, A/B/C) thường không phản ứng khi ấn chéo;
- túi trong suốt dẹt, khó cầm khi nằm dưới đáy thùng kho;
- bàn tập Welcome trông trống và lệch, khay vòng trông như cầm được nhưng không cầm được;
- vòng hoặc kẹp thả lên carton sát miệng thùng đơn bị giấu trong carton;
- thẻ kết quả sau F12 quá dài.

Với #181, người dùng đã chốt luật (2026-11-25): thân thẻ ≤ 90 ký tự DE (tính cả tên vật tư), ≤ 3 dòng trên thẻ rộng 180, không lặp tiêu đề, không nói lại điều cảnh đã cho thấy. Thẻ đúng chỉ nói việc làm tiếp, thẻ sai giữ lý do + việc cần làm.

## Các phương án đã xét

| | Tóm tắt | Vì sao không chọn |
|---|---|---|
| **A (chọn)** | Một bộ thay đổi theo thứ tự, một lần tăng "041" | — |
| B | Hai build: (#177 + #181 + #179) rồi (#180 + #178) | Thêm một buổi kính, pilot chia hai layout với n ≈ 1 mỗi layout |
| C | Như A, nhưng #179 = bỏ khay hoặc thêm NoGrabSymbol + câu lý do | Bàn vẫn trống (chỉ sửa nửa #179), thêm chữ là thêm tải |
| D | Như A + event `poke_cancelled` | Đổi format log ngay trước study, trái quy tắc gom việc R, vẫn không bắt được lần hụt hover |
| E | Chỉ #177 + #181 | #178–#180 còn nguyên |

## Quyết định

Một bộ thay đổi, tăng `layout_version` một lần lên **"041"**, theo thứ tự:

1. **#177** (sửa cả 5 scene, không chạy song song với mục khác sửa scene).
   - Trước khi sửa, test poke MCP tái hiện đường chéo để biết là Cancel hay không Hover.
   - Nếu là Cancel: áp giá trị của #169 cho mọi canvas `PokeInteractable` qua `PokeButtonFixer` (`MinNormal` 0,002, `_cancelSelectTangent` 0,05, một giá trị chung). `Check()` báo khi có giá trị khác.
   - Chỉ nới `_enterHoverNormal` nếu test cho thấy nút không hề hover.
   - Không bật lại ScrollRect, không đụng nút đỏ.
2. **#181** (chỉ CSV + lời gọi `Loc.Get`; làm song song với bước 1 được).
   - Giữ nguyên mọi key, nên `feedback_key` không đổi.
   - Thẻ đúng Basic, Mixed cuối, Complex cuối: thân = câu nhìn băng lăn của decision 033 (46 ký tự).
   - `*.position_done`: câu giữ hàng trong thùng (#139) + câu nhìn băng lăn, ≤ 90. Bỏ „Position {n} ist erledigt“ và „Lesen Sie dann … Position {next} ab“ (dải vị trí + khung thẻ 1 đã chỉ ra). Bỏ placeholder ở cả DE và EN và bỏ đối số trong lời gọi `Loc.Get` ở `MixPickingValidator`/`PiecesOrderValidator` (`Loc.Get` báo lỗi khi text thiếu placeholder được truyền vào).
   - Thẻ sai giữ lý do + việc làm, không có số mục tiêu (decision 006):
     - `ekz3_loose`: bỏ phần nhắc lại luật.
     - `wrong_material`: giữ lý do + bước so Mat.-Nr. với LVS; chữ cuối do `german-reviewer` chốt, ≤ 90.
     - `missing_loose`/`extra_loose`: giữ công thức có ngoặc nếu vừa 90 ký tự với SCHLAUCHSCHELLE B. Nếu không vừa, trỏ về „Beispiel“, nhưng chỉ khi đã kiểm caption `complex.example` còn hiện ở cả hai vị trí. Nếu cả hai đều không được, hỏi lại người dùng (ngoại lệ > 90 ký tự, hay công thức không ngoặc).
   - Giữ `more_errors` (thân + dòng này ≤ 4 dòng), nên quy tắc R không đổi.
   - Cột `max_chars` của các dòng này = 90. Đo bằng `TextComfortAudit.CheckShown`.
3. **#180** (script + test EditMode, không sửa scene).
   - Vật mỏng (`IsThin`, ≤ 2 cm) thả lên kiện mà nằm trên sẽ vượt `Rim − RimClearance` thì vẫn nằm trên, nhô quá miệng thùng tối đa bằng độ dày của nó (≤ 1 cm).
   - Kiện thường giữ nguyên luật 4–5 của #168; luật 6 (nhấc kiện thì vật trên rơi) giữ nguyên.
   - Vùng đếm không nới. Chỉ nới tối đa 1 cm nếu test Editor cho thấy vòng nhô lên không còn được đếm; khi đó ghi rõ là sửa một phần lựa chọn của #168, chỉ cho vật mỏng.
4. **#178** (làm sau #177 vì cùng sửa Mixed/Complex/Welcome). **Người dùng đổi 2026-11-26** (thay cho vùng cầm vô hình mà synthesizer đề xuất):
   - Túi vít SCHRAUBE B (Mixed, Welcome) to lên bằng túi RING D: 14 × 14 cm (trước 12 × 10).
   - Cả hai loại túi dày thêm 1 cm: 4,5 cm (trước 3,5). Làm qua `PouchBuilder` (mesh, `Contents`, `BoxCollider` = hình bao túi như #173); nhãn 10 × 6 cm nằm trên mặt túi giữ nguyên.
   - Sửa một phần decision 029/042 về cỡ túi; người dùng chấp nhận.
   - **Túi chồng cao trong thùng kho (người dùng 2026-11-26):** thêm túi để người học chỉ lấy túi ở lớp trên, gần miệng thùng, không phải thò tay xuống đáy. Thùng kho trong lòng ~40 × 40 cm, sâu ~19 cm; túi 4,5 cm, nên tối đa 4 lớp (18 cm). Vẫn ở nửa sau thùng (#173):
     - Mixed vị trí 2: 8 túi SCHRAUBE B, 2 chồng × 4 lớp (cần 3: hai túi lớp trên cùng + một túi lớp dưới đó). Rest trong LVS tự tăng theo (`OrderContents.StockInScene`).
     - Complex vị trí 2: 4 túi RING D, 1 chồng × 4 lớp, ở góc sau phải cạnh khay (cần 2: hai túi trên cùng).
     - Nếu 4 lớp nhô quá `Rim − 2 mm` khi đo trong Editor: 3 lớp (Mixed 6 túi, Complex 3 túi).
     - Welcome: 4 túi tập thành một chồng (xem mục 5).
     - Hiệu năng: 8 túi vít ≈ 19 nghìn tam giác ở Mixed vị trí 2; kiểm FPS trong buổi test kính. Ở thùng đơn Mixed, 3 túi 14 cm không còn nằm cạnh nhau trong nửa phải (42 cm), nên túi thứ ba có thể nằm lên túi khác theo `DropPlacementRule`; chấp nhận nếu vẫn dưới miệng thùng.
   - Vùng cầm vô hình (~8 cm trên túi, bị luật thả bỏ qua) để làm đường lùi nếu test kính thấy túi vẫn khó cầm. Hand pose để sau.
5. **#179** (sau #178, cùng WelcomeScene).
   - Carton tập, túi tập (và ban đầu khay vòng, xem bổ sung) nằm trong một `PlasticBox_Storage` 40 cm đặt trên bàn ở vị trí WorkSpot như trạm thật.
   - Không có băng lăn, không thêm chữ, không có NoGrabSymbol. Khay vẫn không cầm được, giống Complex (042 mục 3); trong thùng, khay đọc là một phần của thùng.
   - Carton, túi, vòng giữ cỡ chuẩn (029, #175).
   - **Bổ sung (người dùng 2026-11-26): thêm thùng đơn, bài tập là chuyển hàng.**
     - Bên phải thùng kho có một `PlasticBox_Order` trống, đúng chỗ thùng đơn ở Basic/Mixed/Complex. Không có băng lăn, thùng không chạy.
     - Hai thùng có `Box Sign` „Lagerbehälter“ / „Auftragsbehälter“ như 4 scene kia (dùng lại key `concept.lane.*`, không thêm chuỗi; lựa chọn của Claude, để thuật ngữ và bố cục trạm giống nhau từ bài 1).
     - Bước 1 xong (✓ thẻ 1, `WelcomePracticeFeedback`) khi **ít nhất một** món tập (carton hoặc túi) nằm trong thùng đơn sau khi buông tay, không còn tính lúc cầm lên. Bỏ ra lại thì ✓ vẫn giữ.
     - Thẻ 1 đổi chữ, ≤ 90 ký tự, qua `localize-strings`. Đề xuất: tiêu đề „1. Ware umlegen“, thân „Legen Sie ein Teil aus dem Lagerbehälter in den Auftragsbehälter.“
     - Bài xong (mở Menu, `ILessonCompletion`) = một món nằm trong thùng đơn + bấm nút đỏ.
     - Log: `welcome_practice_done` có `action` mới `place` (thay `grab` từ "041"); Welcome vẫn không ghi `grab_*`/`container_*` (món tập có `PracticeItem`). Spec ghi trong cùng thay đổi, R gom vào mục R kế tiếp.
     - Nút đỏ Welcome không được nằm chồng lên thùng đơn; đặt như ở Basic (trước thùng đơn, bên trái).
     - **Hàng xếp thành chồng gần miệng thùng (người dùng 2026-11-26),** để người học lấy ở lớp trên, không thò tay xuống đáy: 2 carton tập chồng lên nhau (≈ 18 cm), 4 túi tập thành một chồng (4 × 4,5 = 18 cm, như #178), nóc chồng dưới `Rim − 2 mm`. Mọi món thêm vào đều là món tập (`PackageData` + `PracticeItem`, trong `otherItems` của `WelcomePracticeFeedback`). **Người dùng 2026-11-26: bỏ khay và 3 vòng tập khỏi Welcome** (khay dưới đáy thì vòng vẫn sát đáy; kê khay lên thì cần thêm code và chồng khay lại trông như cầm được). Welcome chỉ còn carton và túi; người học gặp vòng và khay lần đầu ở Complex. Vì vậy câu „khay vẫn không cầm được“ ở trên không còn áp dụng cho Welcome.
6. **Khay Complex dày lên (người dùng 2026-11-26), sau #180 (cùng `PackageDropper`/`DropPlacementRule`) và sau #178 (cùng ComplexScene).**
   - Khay „Modul“ ở cả hai thùng kho Complex dày lên (thân đặc, lòng khay nông ở trên), để mặt khay cách miệng thùng khoảng 5 cm. Kẹp và vòng nằm trong lòng khay, người học không thò tay xuống đáy. Chiều cao chính xác đo trong Editor.
   - Luật thả coi khay là một khối cố định: thêm khay vào danh sách vật bên dưới khi tính chỗ nằm (không phải kiện hàng: không cầm, không đếm, không dời, không log). Vòng/kẹp thả trên khay nằm trên mặt khay; thả lệch ra ngoài thì bị đẩy sang bên như #168. Không có bước này thì vòng thả lại sẽ nằm trong khối khay (như #180).
   - `Storage Label` dời lên theo thành sau của khay. Khay vẫn không cầm được (042 mục 3); không thêm NoGrabSymbol.
   - Mesh khay riêng cho Complex (Welcome không còn dùng khay).

**Log:** không thêm event, không đổi field, trừ `welcome_practice_done` `action` `place` (mục 5). Spec ghi các điểm khác giữa "040" và "041": mức `drop_rule` mới cho #180 (không gộp vào `settle`), thẻ ngắn (`card_text`), nút poke mới (tách thời gian tour/quiz/menu), tác động lên `container_unheld_lines`. Mọi sửa R ghi vào mục R kế tiếp (#176 đã đóng).

## Được và mất

- **Được:** nút phản ứng ngay lần ấn đầu; thẻ đọc kịp trong 4 s; hàng đã thả luôn thấy được; túi cầm từ trên xuống được; bàn Welcome cân đối, không còn vật trông cầm được mà không cầm được, người học tập lấy hàng từ thùng ngay bài 1; study chạy một layout.
- **Mất / chấp nhận:**
  - Dữ liệu pilot "040" (P801, P811) không gộp với "041" ở tour, menu, thẻ và Complex (n rất nhỏ).
  - Vật mỏng có thể nhô vài mm trên miệng thùng (tài liệu không cấm; thấy hàng quan trọng hơn).
  - Thẻ position_done mất số vị trí (dải vị trí + khung thẻ 1 đã cho thấy).
  - Bài 1 hơi khó hơn vì túi nằm trong thùng (đỡ nhờ #178 làm trước #179).
  - Nếu #178 hỏng trên kính, gỡ riêng nó được.

## Làm gì nếu giả định sai

- Test MCP #177 cho thấy nút không hề hover: đổi `_enterHoverNormal` thay vì tangent, vẫn qua `PokeButtonFixer`.
- Tangent 0,05 gây chọn nhầm mục Menu trên kính: hạ lại giá trị, hoặc thu `Surface` của Menu.
- Túi to và dày hơn vẫn khó cầm trên kính: thêm vùng cầm vô hình, rồi đến đặt túi lên bậc hoặc hand pose. Nếu làm sau buổi pilot thì tăng "042".
- Vòng nhô quá miệng không còn được đếm: nới vùng đếm ≤ 1 cm và ghi lại là sửa một phần #168.
- Người hướng dẫn nói không được để hàng cao hơn miệng thùng (D48): chuyển sang hạ carton trên để có chỗ cho vật mỏng.
- Thùng ở Welcome làm bài 1 quá khó trên kính: kéo túi lên lớp trên hoặc ra nửa trước thùng (chỉ ở Welcome); đường lùi cuối là phương án C.
- Thẻ vẫn vượt 3 dòng: rút chữ tiếp, không thu font (#132).

## Câu hỏi còn mở

- Người hướng dẫn: D48 (hàng nhô khỏi miệng thùng đơn), D49 (sau F12 người lấy hàng thấy/nghe gì; bổ sung D44). A13 đã có, không chặn.

## Bằng chứng

- Luật #181 và *Done when* → `docs/backlog.md` #181.
- Decision 033 mục 2 (một câu nhìn băng lăn ở 3 scene) → `docs/decisions/033-confirm-pause-container-change-cue.md`.
- `Loc.Get` báo lỗi khi thiếu placeholder → `Assets/Scripts/Localization/Loc.cs` dòng 44–56.
- Nguyên nhân #180: `Finish` hạ `bottom` xuống `Rim − RimClearance − Height` → `Assets/Scripts/Orders/DropPlacementRule.cs` dòng 193–203; tái hiện qua MCP (nóc carton 0,966 → vòng 4 mm nằm ở 0,962, trong carton).
- Khay „Modul“ không cầm được → decision 042 mục 3; quy tắc cầm → decision 032.
- Video và log P811 → `analysis/data/20261125/` (không commit), khung hình ở `TestEvidence/P811-2026-11-25/`.
- Chưa kiểm: vòng nhô ≤ 1 cm vẫn chạm trigger bộ đếm; Cancel hay không Hover ở #177; caption `complex.example` còn hiện ở vị trí 2.
