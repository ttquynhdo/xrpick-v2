# Kết quả test trên kính của người dùng (2026-11-24)

Nguồn: tin nhắn của người dùng sau buổi test kính (build có #137–#167). Chép ý nguyên văn (tiếng Việt). Chưa có video, log hay mã người tham gia.

Cách làm như các file review trước: ghi lại trước, phân tích một lượt khi danh sách đủ, rồi chạy `review-to-backlog` và cập nhật test report của #137–#167.

---

## T1. Cầm hộp thỉnh thoảng bị giật khung hình

- **Nguyên văn:** „thi thoảng việc cầm hộp cứ bị giật giật khung hình“
- **Chưa biết:** scene nào, carton hay túi, lúc nhấc lên hay lúc di chuyển, giật cả khung hình (FPS tụt) hay chỉ hộp nhảy trong tay.

## T2. Bấm nút đỏ thỉnh thoảng trượt, bấm lại mới trúng và kêu

- **Nguyên văn:** „lúc bấm nút đỏ bị trượt qua luôn, bấm lại thì trúng và kêu“
- **Chưa biết:** scene nào, bấm bằng ngón tay (poke) hay tay cầm, lần trượt có ghi `confirm_pressed` trong log không.

## T3. Mixed vị trí 2: rất khó cho đủ 3 túi SCHRAUBE B vào thùng đơn, túi cứ nhảy ra ngoài

- **Nguyên văn:** „ở màn này rất khó để cho được 3 túi vào chung thùng đơn vì cứ bị nhảy ra ngoài“
- **Ảnh** (khung video trên kính): `TestEvidence/review-2026-11-24/T3_mixed_bags.webp` (không commit).
  - Thùng `Auftragsbehälter`: đã có carton KABELVERBINDER A của vị trí 1 ở nửa trái.
  - Hai túi SCHRAUBE B (nhãn „SCHRAUBE B / Mat.-Nr: 738190 / VPE: 20 ST“) đang ở nửa phải, sát thành phải của thùng.
  - Túi lớn chiếm gần hết khoảng trống còn lại của nửa phải. Túi thứ hai nằm thấp hơn và bị túi lớn che một phần.
  - Tay phải (tay ảo xám) đang ở ngay bên phải thùng.
- **Chưa biết:** túi „nhảy ra“ lúc vừa thả tay hay lúc đặt túi thứ 3; túi rơi xuống mặt bàn hay về chỗ đã nhấc lên (thùng kho).

## T4. Tem trên một số carton nằm ngang so với nắp, thò ra ngoài hộp, trông mất cân đối

- **Nguyên văn:** „có mấy tem ở thùng bị ngang so với hộp gây mất cân đối“
- **Ảnh** (khung video trên kính): `TestEvidence/review-2026-11-24/T4_label_across_lid.png` (không commit).
  - Carton SCHLAUCHSCHELLE B (Mat.-Nr 443890, VPE 50), xếp chồng trong thùng xanh. Biển ở thành thùng chỉ thấy đuôi „…hälte…“, chưa rõ là Lager- hay Auftragsbehälter.
  - Tem `Label Top` của carton trên cùng nằm chéo/ngang qua nắp. Hai đầu tem thò ra ngoài mép nắp (bên trái và bên phải). Theo #88, tem phải nằm gọn trong nắp và chiếm 60 % nắp.
  - Carton bên dưới cũng có một tem thò ra ngoài mép, chỉ thấy đuôi „…LLE B / …90“.
- **Chưa biết:** scene nào (SCHLAUCHSCHELLE B có ở Basic và Complex vị trí 2); carton đã được cầm rồi thả ra (bị xoay theo tay) hay vẫn ở nguyên chỗ lúc bắt đầu scene.

## T5. Mixed và Complex: rất khó cho hộp vào thùng đơn; Complex không xếp được cả 2 vị trí vào cùng một thùng đơn

- **Nguyên văn:** „ở 2 bài cuối rất khó để cho các hộp vào trong thùng, đặc biệt bài cuối ko thể để tất cả 2 đơn vào cùng thùng đơn vì cứ cho ring vào thì cho thùng vào lại bị nảy ra“
- **Ý chính:**
  - Ở cả Mixed (bài 4) và Complex (bài 5), việc thả carton vào thùng đơn khó. T3 (túi Mixed bị nảy ra) là một trường hợp của điểm này.
  - Ở Complex, người dùng không xếp được hàng của cả hai vị trí vào cùng một thùng đơn. Sau khi đã cho vòng RING D (vị trí 1) vào, carton (vị trí 1 hoặc vị trí 2) thả vào thùng lại bị nảy ra ngoài.
- **Hệ quả:** ở Complex người học có thể không hoàn thành được đơn. Đây là lỗi chặn bài học (mức P0) và có thể chặn study.
- **Chưa biết:** carton nảy ra rơi xuống mặt bàn hay về thùng kho; thùng đơn lúc đó đã đầy tới đâu (bao nhiêu carton, bao nhiêu vòng).

## T6. Complex: bước cuối chỉ kiểm hàng của một vị trí, không kiểm hàng của cả hai

- **Nguyên văn:** „ở màn cuối đáng lí phải có cùng số hàng của 2 đơn rồi mới được coi là hoàn thành nhưng hiện chỉ tính cho 1 đơn“
- **Mong muốn của người dùng:** đơn chỉ được tính là hoàn thành khi thùng đơn chứa đúng hàng của **cả hai** vị trí (vị trí 1: RING D 2 carton + 5 vòng; vị trí 2: SCHLAUCHSCHELLE B 3 carton + 6 cái).
- **Hiện trạng (đọc từ CLAUDE.md, chưa đối chiếu code):**
  - Complex làm từng vị trí một (#19, decision 016). `PiecesContainerCounter` + `OrderRule` chỉ chấm dòng đơn hiện tại.
  - Hàng của vị trí đã xong vẫn nằm trong thùng nhưng không còn được đếm. Từ #139, việc lấy hàng đó ra hay cho vào cũng không làm đổi thẻ kết quả.
  - Vì vậy, lần bấm cuối ở vị trí 2 chỉ kiểm hàng của vị trí 2. Người học có thể lấy hàng vị trí 1 ra khỏi thùng sau khi vị trí 1 đúng mà vẫn được báo „Richtig!“. Mixed cũng làm như vậy.
- **Liên quan T5:** vì không xếp được cả hai vị trí vào cùng thùng (T5), có thể người dùng đã phải lấy bớt hàng vị trí 1 ra, và app vẫn chấp nhận.
- **Cần quyết định:** đây là đổi luật chấm (decision 016, 012, #139), cần hỏi xem ở trạm thật có kiểm lại hàng của vị trí trước khi xác nhận vị trí sau hay không. Dùng `solution-decision`.

## T7. Nhận xét chung: cầm thùng rồi thả, hộp cứ rơi và nảy, rất phiền

- **Nguyên văn:** „việc cầm thùng cứ rơi rồi nảy rất phiền, tạo ra nhiều lỗi bắt người dùng tương tác qua lại“
- **Ý chính:** đây là nhận xét bao trùm T1, T3, T5. Sau khi thả tay, hộp không nằm yên ở chỗ được thả mà rơi hoặc nảy đi chỗ khác. Người học phải cầm lại, đặt lại nhiều lần, và việc này gây lỗi mới (hộp ra ngoài thùng, đếm sai, phải làm lại).
- **Liên quan:** #157 (một cơ chế quyết định kiện nằm đâu khi thả, gồm #140, #152, #153) và commit aaa7f7a (carton thả nửa trên thành thùng thì rơi vào trong). Đây là vùng code vừa đổi gần nhất, nên #157 chưa thể ghi `done`.
- **Hệ quả cho study:** mỗi lần cầm lại hay đặt lại đều sinh ra `grab_start`/`container_*` thừa trong log, làm nhiễu số liệu tương tác.

## T8. Complex: hai vị trí trông gần như giống nhau (carton + vòng), chỉ khác màu vòng, rất dễ nhầm

- **Nguyên văn:** „ở đơn cuối 2 order line đều là box và ring chỉ có màu ring khác nhau làm rất bối rối“
- **Hiện trạng (CLAUDE.md):**
  - Vị trí 1: RING D (264857, VPE 25), carton + 12 vòng lẻ cạnh carton đã mở `VPE_OpenedPackage`.
  - Vị trí 2: SCHLAUCHSCHELLE B (443890, VPE 50), carton + 8 cái `clamp` lẻ cạnh `SB_OpenedPackage`. Mảnh lẻ dùng **cùng mesh vòng** với RING D, chỉ khác vật liệu `Materials/HoseClampSteel`.
  - Từ #88, mọi carton có cùng kích thước và cùng một mẫu tem; chỉ chữ trên tem khác nhau.
- **Vì sao gây nhầm:** người học thấy cùng một dạng hàng (carton + vòng) ở cả hai vị trí, nên khó biết vị trí 2 là vật tư khác. Muốn phân biệt phải đọc tem hoặc để ý màu.
- **Cần cân nhắc:** việc so tem với LVS là một mục tiêu học (decision 032), nên hàng giống nhau không hẳn là sai. Nhưng mảnh lẻ SCHLAUCHSCHELLE B (cái kẹp ống) dùng mesh vòng là đơn giản hoá, không giống hàng thật. Có thể làm mảnh lẻ vị trí 2 có hình dạng khác (kẹp ống thật). Liên quan #164 (thêm nhiều Mat.-Nr, để sau study).

### Ghi thêm cho T1 và T8 (đo trong ComplexScene, 2026-11-24)

- Mọi vòng RING D và kẹp SCHLAUCHSCHELLE B lẻ ở Complex dùng mesh `Body_01` **16.200 tam giác** mỗi cái (20 mảnh ≈ 324.000 tam giác). Có thể góp phần vào T1 (giật khung hình); cần đo FPS trên Quest.
- Mẫu kẹp ống tự dựng: `Assets/Meshes/HoseClamp.asset` (chưa commit; 352 tam giác, Ø 6 cm, đai thép 9 mm + hộp khoá + vít). Ảnh so với vòng RING D: `TestEvidence/ComplexScene/hoseclamp/preview_v1.png`. Chưa thay vào scene.
- **Đã làm (người dùng yêu cầu, 2026-11-24, chưa commit):** ComplexScene: 12 vòng RING D dùng `Assets/Meshes/RingD.asset` (1.152 tam giác, cùng hình và khung toạ độ), 8 kẹp dùng `HoseClamp.asset` (352 tam giác, BoxCollider theo mesh). Tổng mảnh lẻ từ ≈ 324.000 xuống ≈ 16.600 tam giác. Ảnh: `TestEvidence/ComplexScene/hoseclamp/storage1_rings_after.png`, `storage2_clamps_after.png`. Chưa test cầm/thả kẹp bằng tay.

---

# Phân tích (2026-11-24, một lượt cho T1–T8)

Cách làm: đọc code trên `main` 3e37512 và thử trong Editor qua MCP (ComplexScene, Play, kiện được thả bằng `Grabbable.ProcessPointerEvent`, không có tay thật). Ảnh: `TestEvidence/ComplexScene/review-2026-11-24/`.

## Nhóm A. Kiện nảy lên / không vừa thùng đơn: T3, T5, T7 (+ một phần T1)

- **Cơ chế hiện tại** (`PackageDropper.TryPlan`, `Assets/Scripts/PackageDropper.cs:222-237`; Basic/Mixed cartons: `CartonSettler.TrySettle`):
  - Kiện được thả **thẳng xuống đúng chỗ tay mở**, bắt đầu từ trên miệng thùng, xuống tới đáy hoặc **nóc của bất kỳ kiện nào nằm dưới bất kỳ phần nào của nó**.
  - Không tìm chỗ trống bên cạnh. Chỉ cần mép carton mới đè lên mép carton cũ vài mm là carton mới **nằm chồng lên trên**.
- **Tái hiện** (Complex vị trí 1): 5 vòng nằm phẳng dưới đáy, thả `VPE_1` ở x 0,40 → nằm trên vòng (đáy 0,787). Thả `VPE_2` ở x 0,50 (đè lên `VPE_1` 9 cm) → **nằm trên nóc `VPE_1`**, nóc ở y 0,970, cao hơn miệng thùng (0,968). Nhìn từ chỗ đứng, carton như bị „nảy lên“ khỏi thùng. Ảnh `T5_pos1_rings_then_cartons.png`.
- **Vì sao hay gặp trên kính:** thùng 40 × 40 cm, carton 17,8 × 12,3 cm. Hai carton cạnh nhau chỉ còn 4 cm dư, nên với tay thật rất dễ thả lệch vài cm, đè lên carton bên cạnh. Ở Complex vị trí 2 thùng phải chứa 5 carton + 11 mảnh lẻ; mỗi lần đè là một tầng mới, nên chồng cao dần và vượt miệng thùng → „không cho được cả 2 vị trí vào cùng thùng“ (T5). Túi ở Mixed (T3) cũng vậy: túi to, chỉ có nửa phải thùng.
- **Chưa tái hiện được:** kiện bay hẳn ra ngoài thùng. Code kẹp x/z vào trong thành thùng, nên kiện không thể rơi ra ngoài trừ khi bay về chỗ cầm (`returning`, chỉ khi dưới tay không có thùng/bàn). Cần video hoặc mô tả để biết người dùng có gặp trường hợp này không.
- **Ghi thêm:** mọi kiện bật `Grabbable._throwWhenUnselected`. Kiện là kinematic nên hiện không bị ném, nhưng nên tắt cho chắc.
- **Hướng sửa:** khi chỗ thả đè lên kiện khác, tìm **chỗ trống thấp nhất gần nhất** trong thùng (dò lưới vài cm quanh chỗ thả) trước khi xếp chồng; chỉ xếp chồng khi đáy đã kín. Áp dụng cho `PackageDropper` và `CartonSettler`. Kiểm bằng MCP: thả đủ hàng của Complex vị trí 1 + 2 và Mixed vị trí 1 + 2 ở các chỗ đè nhau → mọi kiện nằm dưới miệng thùng.

## Nhóm B. Giật khung hình khi cầm: T1

- Mỗi vòng RING D / kẹp lẻ ở Complex có mesh 16.200 tam giác (20 mảnh ≈ 324.000). **Đã sửa (chưa commit):** `RingD.asset` 1.152, `HoseClamp.asset` 352 → tổng ≈ 16.600.
- Chưa biết T1 xảy ra ở scene nào. Nếu cả ở Basic/Mixed (không có mảnh lẻ) thì nguyên nhân khác (ví dụ chuyển động thả của Nhóm A trông như giật). Cần thử lại trên kính sau khi sửa Nhóm A + mesh.

## Nhóm C. Nút đỏ trượt: T2

- `BigRedButton/Button` (`PokeInteractable`): `_dragThresholds` bật (DragNormal 0,01, DragTangent 0,01) và `_minThresholds.MinNormal` 0,025. Nghĩa là: ngón tay phải vào vùng hover từ cao hơn 2,5 cm, và nếu ngón tay **trượt ngang hơn 1 cm** trong lúc ấn thì lần ấn bị coi là kéo (drag) và bị huỷ.
- Người đứng ấn một nút trên bàn thường đưa tay chéo từ phía trước, nên dễ trượt ngang > 1 cm → lần ấn đầu „trượt qua“, lần sau ấn thẳng hơn thì trúng. Khớp với T2.
- **Hướng sửa:** tắt `_dragThresholds` (nút bấm, không phải danh sách cuộn), hạ `MinNormal`, có thể bật `_recoilAssist`. Áp dụng ở mọi scene có nút đỏ (Welcome, Basic, Mixed, Complex). Phải thử trên kính.

## Nhóm D. Tem carton lệch: T4

- Carton có scale không đều (0,3 × 0,3 × 0,4). Ba carton ở Complex được xoay 90° (#111: `VPE_3`, `SB_Carton_3`, `SB_Carton_4`) và tem `Label Top` của chúng xoay 270° **bên trong** carton. Vì scale không đều, tem bị kéo thành 14,3 × 5,5 cm (đúng: 10,7 × 7,3) và nằm ngang qua cạnh ngắn của nắp → thò ra hai bên. Đúng chồng carton trong thùng kho 2 ở ảnh T4.
- Các scene khác không có carton nào bị như vậy (kiểm cả 4 scene). `PackageStandardizer.Check()` không bắt lỗi này.
- **Hướng sửa:** đặt lại tem của 3 carton (tem theo trục dài của nắp như các carton khác) và thêm vào `Check()` một kiểm tra „tem xoay 90° trong carton có scale không đều“.

## Nhóm E. Complex chỉ chấm vị trí hiện tại: T6

- Đúng như thiết kế: `PiecesContainerCounter.confirmedPackages` (`PiecesContainerCounter.cs:14-16`) loại hàng của vị trí đã xong khỏi Ist; lần bấm cuối chỉ kiểm vị trí 2. Mixed cũng vậy (decision 012, 016, #139).
- Muốn đổi thành „hoàn thành khi thùng có đủ hàng cả hai vị trí“ là đổi luật chấm → `solution-decision`, và cần hỏi Festo: ở Picking 2, trước khi xác nhận vị trí sau có ai kiểm lại hàng của vị trí trước không? Làm **sau** Nhóm A, vì hiện người học có thể đã phải lấy hàng ra do không vừa thùng.

## Nhóm F. Hai vị trí Complex trông giống nhau: T8

- **Đã sửa (chưa commit):** 8 kẹp SCHLAUCHSCHELLE B có hình kẹp ống có vít (`HoseClamp.asset`), khác hẳn vòng RING D. Chưa thử cầm/thả kẹp bằng tay; collider kẹp to hơn trước (7,3 × 6,0 × 1,2 cm).

## Thứ tự đề xuất

1. Nhóm A (P0, chặn bài học, ảnh hưởng số liệu log) — cùng lúc commit Nhóm B/F (mesh).
2. Nhóm C (P1, nhỏ, cần kính) và Nhóm D (P2, nhỏ, không cần kính) — làm song song, khác file.
3. Một buổi kính cho A + B + C + F.
4. Nhóm E: `solution-decision` + câu hỏi Festo, sau buổi kính.
