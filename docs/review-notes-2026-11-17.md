# Góp ý của bạn người dùng sau khi thử bản mới nhất (2026-11-17)

Nguồn: tin nhắn của một bạn đã thử app (tiếng Anh, chép nguyên văn ở mỗi mục) và hai video quay trên kính `5. Thesis/07_presentations/material.mp4` (8 s) và `orange.mp4` (9 s). Trên video mọi chữ là tiếng Đức, nên đó là phiên DE. Ảnh ghép 1 khung/giây: `TestEvidence/review-2026-11-17/material_sheet.jpg`, `orange_sheet.jpg` (không commit).

Cách làm như các file review trước: chỉ ghi lại, chưa sửa, chưa mở mục backlog. Khi danh sách đủ, chạy `review-to-backlog` cho cả file.

**Đã chuyển vào backlog (người dùng 2026-11-17: làm ngay):** F1 → #136, F3 hướng (a) → #137, F2 phần (a) (câu nhắc trên thẻ kết quả) → #139. F2 phần (b) (kiện thả ngoài thùng lơ lửng) → #153 (2026-11-17, cùng R16 của `docs/review-notes-checklist-2026-11-17.md`).

Mỗi mục có: **góp ý** (nguyên văn), **video**, **hiện trạng** (đọc từ code/scene trên `main` 247cb14), **ghi chú**.

---

## F1. Mũi tên cam vẫn còn khi app đã bảo cầm carton (ConceptScene)

- **Góp ý:** "there is some orange sign that I suppose is there to show what object you are supposed to grab, but it is still there, when the app tells you to grab the object"
- **Video `orange.mp4`:**
  - Tour đang ở „Schritt 3 von 11“ (bước `verp_einh`, khung cam quanh Verp.-Einh. 40).
  - Người thử đã cầm một carton STECKER C rồi thả lơ lửng trên thùng kho. Carton là kinematic, không có trọng lực, nên nằm yên ở chỗ được thả.
  - Mũi tên cam `STECKER C Marker` vẫn nằm trên mép thùng kho, chỉ vào các carton. Người thử đưa tay sát mũi tên.
- **Hiện trạng:**
  - `MarkerHideOnGrab` (`Assets/Scripts/Concept/MarkerHideOnGrab.cs`) tắt mũi tên khi một trong 6 carton bị cầm (Select).
  - Tour bật lại mũi tên mỗi lần đổi bước, vì mũi tên là marker của cả bước `material` (2) lẫn `verp_einh` (3).
  - Mũi tên chỉ tự tắt ngay nếu lúc bước hiện ra có carton **đang trong tay**. Carton đã được cầm rồi thả ra thì không tính.
  - Vì vậy: cầm carton ở bước 2, thả ra, bấm „Weiter“ → mũi tên hiện lại ở bước 3, dù người học đã biết carton nào cần cầm.
  - Dòng hành động `concept.tour.verp_einh.action` cũng bảo cầm carton, nên mũi tên và chữ nói cùng một điều.
- **Ghi chú:**
  - Mũi tên có hai vai trò cùng lúc: chỉ đúng vật (bước 2) và gắn với chữ của bước 3. Người thử hiểu nó là „cầm cái này“, nên khi đã cầm rồi thì nó thành thừa (redundancy).
  - Có thể cho mũi tên tắt hẳn trong lượt vào scene đó sau lần cầm đầu tiên; hoặc chỉ dùng mũi tên ở bước 2.
  - Liên quan decision 036 mục 5 (#105) và 037 mục 5 (mũi tên cam chỉ có nghĩa „nhìn / cầm cái này“).

## F2. Kiện của vị trí trước vẫn nằm trong thùng đơn (ComplexScene, cũng đúng cho Mixed)

- **Góp ý:** "The objects from the previous box do not disappear, so you kind of have to get the old ones out of the box"
- **Video `material.mp4`:**
  - ComplexScene vị trí 2: LVS hiện SCHLAUCHSCHELLE B, Mat.-Nr 443890, Soll 156, Empf Kz 000.
  - Trong thùng đơn (bên phải) vẫn còn các carton RING D và vòng của vị trí 1.
  - Người thử lấy các carton cũ ra và thả trong không trung, nên chúng treo lơ lửng phía trên màn hình và gần Menu. Vòng rời cũng treo trong không khí.
- **Hiện trạng:**
  - Đây là thiết kế có chủ ý (decision 012 cho Mixed, 016 cho Complex): kiện của vị trí đã xong ở lại thùng đơn nhưng không còn được đếm. Logic `MixedPickingContainer` / `PiecesContainerCounter` với `OrderSequence` chỉ chấm vị trí hiện tại.
  - Lý do: ở trạm thật, một thùng đơn chứa hàng của mọi vị trí trong cùng một đơn. Thùng đơn chỉ rời trạm khi cả đơn xong.
  - Không có chữ hay dấu hiệu nào nói với người học là hàng cũ cứ để nguyên. Thẻ 2 và thẻ kết quả `complex.feedback.position_done` / `mixed.feedback.position_done` chỉ nói thùng kho đổi.
  - Kiện không có trọng lực (kinematic, CLAUDE.md *ConceptScene box hints*), nên kiện bị lấy ra và thả ngoài thùng sẽ treo giữa không trung.
- **Ghi chú:**
  - Có hai vấn đề:
    - (a) người học tưởng phải dọn hàng cũ, tốn thời gian và làm sai lệch `read_s` / thời gian vị trí 2;
    - (b) vật thả ra ngoài treo lơ lửng, trông giống lỗi.
  - Hướng có thể xét: một câu ngắn ở thẻ kết quả hoặc thẻ 2 („Die Ware der ersten Position bleibt im Auftragsbehälter.“), hoặc một dấu hiệu nhìn thấy được (hàng cũ dồn sang một bên hay mờ đi). Hai cách này đánh đổi với việc giữ giống trạm thật.
  - Nhóm thấy chữ „(positions)“ khó hiểu. Chữ tiếng Đức „Position“ là nhãn của LVS; câu EN nên nói „order line“.
  - (b) đụng tới cả 4 scene có kiện cầm được. Cần xem có nên cho kiện rơi về chỗ cũ / xuống bàn khi thả ngoài thùng không (`CartonSettler` hiện chỉ xếp carton được thả **trong** thùng đơn ở Basic và Mixed).

## F3. Bài đầu tiên: LVS ghi EKZ 3, nhưng carton được bỏ vào lộn xộn (ConceptScene)

- **Góp ý:** "Oh and in the first tutorial the LWS says EKZ 3, when it should be EKZ 0, since there you can just put the boxes in unordered"
  - „LWS“ = LVS (Lagerverwaltungssystem). „first tutorial“ là bài có LVS đầu tiên, tức bài 2 „Grundlagen“ (ConceptScene); suy ra từ ngữ cảnh, chưa hỏi lại người thử.
- **Video:** `orange.mp4` cho thấy ô Empf Kz của Concept hiện **003** nền vàng.
- **Hiện trạng:**
  - Đơn mẫu STECKER C trong Concept (`SampleWmsOrder`) mang Empf Kz 003. Tour bước 8 (`empf_kz_003`) dạy: 003 = EKZ 3, „đặt từng cái, có thứ tự, như carton trong ví dụ“. Bước 9 (`empf_kz_000`) nói đơn khác có thể hiện 000 = để lộn xộn.
  - Concept không có ô đặt (`BoxSnapper`) hay kiểm tra cách xếp. Người học bỏ 3 carton STECKER C vào thùng đơn theo bất kỳ cách nào, `ConceptBoxHints` chỉ báo sai vật liệu hoặc thừa, không bao giờ báo „xếp sai“. Chỉ `LaneFlowDemo.FillOrder` tự đặt carton còn thiếu vào `orderSlots` có thứ tự.
  - Vậy màn hình nói „phải xếp có thứ tự“ mà bài lại cho bỏ lộn xộn: người học thấy quy tắc EKZ 3 không có hậu quả, đúng lúc vừa được dạy quy tắc đó.
- **Ghi chú:**
  - Hai hướng ngược nhau:
    - (a) đổi đơn mẫu sang 000 như người thử nói: khớp với cách bỏ lộn xộn, nhưng phải đổi lại tour bước 8–9 (hiện dạy 003 trên chính ô LVS), thùng mẫu và quiz `ekz_003`/`ekz_000`;
    - (b) giữ 003 và làm cho bài khớp: viền chỗ đặt trong thùng đơn như Basic (#108) hoặc ít nhất một câu/dấu hiệu khi carton nằm lộn xộn.
  - Liên quan decision 028 (Concept: mẫu 003 / 000), 031, 032, 036, 037 (viền chỗ đặt ở Basic/Mixed), và F2 (cách người học hiểu thùng đơn).
