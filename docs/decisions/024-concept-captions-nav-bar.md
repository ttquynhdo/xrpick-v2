# Quyết định 024: ConceptScene: chú thích sát từng ô, thanh „Zurück / Schritt n von 11 / Weiter“ cố định dưới màn hình, cụm câu hỏi chỉ hiện ở bước cuối, thẻ việc có ✓ và khung bước hiện tại

- **Backlog:** #78, #79, #80 (một quyết định chung); #77 gộp vào
- **Trạng thái:** accepted
- **Ngày:** 2026-11-04
- **Ý kiến đã xét:** unity-reviewer, data-analyst, xr-ux-reviewer, instructional-design-reviewer → decision-synthesizer. Không hỏi requirements-analyst (không đổi quy tắc nghiệp vụ) và german-reviewer (mọi chuỗi mới qua `localize-strings` + `german-reviewer` khi làm).
- **Lựa chọn của người dùng (2026-11-04):** chấp nhận phương án B; thẻ 1 có ✓ sau khi cầm cả 3 kiện (giữ chữ „jede Verpackung“); Concept dùng ✓ + khung xanh; sau câu đúng giữ „Richtig!“ 1,5 s rồi tự sang câu mới.
- **Sửa quyết định đã chấp nhận:** 010 (bảng giảng bên phải, mũi tên hàng + cột), 015 (vị trí „Weiter“, „Zurück“ hoãn, vị trí cụm quiz), 020 (không ẩn hẳn cụm khi khoá, „Richtig!“ còn dưới câu sau). 017 mở rộng sang Concept, thêm ✓; Basic/Mixed/Complex không đổi. 022 giữ nguyên.
- **Chưa xét:** #81 (hình ba làn ở bước `material_flow`) được thêm sau khi brief đã gửi; cần bổ sung riêng, theo bố cục của quyết định này.

## Bài toán
Video test kính 2026-11-04 (P811, English) ở ConceptScene:
- Vào scene thấy 6 tấm cùng lúc, không biết bắt đầu ở đâu.
- Lời giải thích nằm trên bảng tour bên phải (+35°), cách các ô ở nửa trái dialog (≈ −10°) 0,7–0,9 m: phải quay đầu ≈ 45° qua lại.
- Mũi tên cột ở viền trên nằm sát „Modus / Auftragsart“ nên bị đọc nhầm.
- Cụm câu hỏi che hàng dưới của dialog suốt tour.
- Nút „Weiter“ đứng riêng một chỗ, không rõ nghĩa; không có cách lùi bước.
- Thẻ việc không có dấu xong như Welcome.
- „Richtig!“ của câu trước còn nằm dưới câu mới.

## Xung đột giữa các đề xuất
| Vấn đề | Bên A | Bên B | Đánh đổi |
|---|---|---|---|
| Chỗ đặt lời giải thích | unity A, UX B, ID 3: giữ bảng tour bên phải, kéo sát | UX A: chú thích ngắn sát từng ô; ID 1: một thẻ chạy theo ô; ID 2: hai bảng trái/phải | Rẻ, giữ chữ đã duyệt ↔ đạt „không quay đầu“ |
| Vị trí Weiter/Zurück | unity A, ID 1: gắn với thẻ giải thích | UX A: thanh cố định dưới màn hình cạnh „Schritt n von 11“ | Nút sát chữ ↔ nút đứng yên, ít bấm hụt |
| `Info Panel` | ID 1/2: ẩn hoặc dời | UX A: giữ, đặt tiêu đề „1. Etikett lesen“ | Chỗ cho chú thích trái ↔ giữ bảng nhãn cho câu Mat.-Nr |
| Dấu trên thẻ | unity: ✓ như Welcome | ID, UX: ✓ + khung xanh (017) | Đơn giản ↔ trợ giúp giảm dần |
| Thẻ 1 xong khi nào | Done when cũ: kiện đầu tiên | ID: thẻ ghi „jede“ | Dễ xong ↔ khớp lời dặn |
| „Richtig!“ khi sang câu | UX: xoá ngay / nút „Nächste Frage“ | ID: giữ 1–1,5 s rồi chuyển | Rẻ ↔ kịp thấy kết quả |
| Back trong log | data 1: format giữ, R suy ra hướng | data 2: trường `via` | Contract giữ ↔ không suy luận |

Cả bốn đồng ý: ẩn hẳn quiz đến `material_flow`; bỏ mũi tên cột cạnh R1; Back gọi lại `ShowStep`; „Von vorn“ không xoá ✓; quiz đã mở không khoá lại; mỗi bước hiện ra ghi đúng một `tour_step`.

## Các phương án
- **0. Tối thiểu** (chỉ sửa „Richtig!“): không giải quyết video.
- **A. Giữ bảng tour bên phải**, kéo sát viền, Zurück/Weiter dưới bảng: S–M, nhưng các ô nửa trái vẫn cách ≈ 45°; dự phòng.
- **B. Chú thích bám ô + thanh điều hướng cố định dưới màn hình:** chọn.
- **C. Một thẻ + nút chạy theo ô, ba pha:** nút nhảy chỗ sau mỗi lần bấm, phải ẩn `Info Panel`.
- **D. Hai bảng cố định trái/phải:** thêm tấm, nút rời chữ.

## Quyết định: phương án B
1. **Chú thích.** Bước nói một ô (material, verp_einh, soll, ist, rest, empf_kz_003, empf_kz_000): một chú thích ngắn, tiêu đề ghi đúng nhãn trên màn hình + 1–2 câu (≤ ~15 từ như 010), ngoài viền, cùng phía và ngang hàng với ô (nửa trái → bên trái, Rest → bên phải). Bước intro, cartons, not_used, material_flow: chú thích ở một chỗ cố định bên phải viền. Chú thích là marker của từng bước (`steps[].markers`). Bảng `Sample Description` không còn là chỗ đọc.
2. **Mũi tên.** Ô cột trái chỉ còn mũi tên hàng; bỏ mũi tên cột ở y 585. Ist, Verp.-Einh. dựa vào tiêu đề + mũi tên hàng; chỉ thêm mũi tên cột nếu đo thấy nó gần ô của nó hơn mọi nhãn khác. Không vẽ gì lên dialog.
3. **Thanh điều hướng** cố định ngay dưới màn hình, không che dialog: [Zurück] „Schritt n von 11“ [Weiter]. Cùng cỡ, „Zurück“ kiểu phụ (nền nhạt), khe ≥ 8 cm. Bước 1: „Zurück“ mờ, không bấm được. Bước 11: „Weiter“ ghi „Von vorn“. Vùng chạm `Surface` phủ cả hai nút (#76).
4. **Quiz.** Cụm tắt hẳn đến lần đầu tới `material_flow`, rồi hiện bên phải, trong tầm tay, không che dialog, chú thích bước 11 hay bảng EKZ/Ablauf. Đã mở thì luôn mở (Zurück, „Von vorn“). Lời báo trước chuyển sang thẻ 3 và chú thích bước `material_flow`.
5. **„Richtig!“.** Câu đúng chưa phải câu cuối: „Richtig! …“ dưới câu vừa trả lời ≈ 1,5 s, chạm lúc đó không chấm; rồi câu mới với dòng hướng dẫn trung tính. Câu cuối giữ `all_done` như 020.
6. **✓ trên thẻ** (badge `Done Check` như Welcome, góc thẻ, không tắt): thẻ 1 sau khi đã cầm cả 3 kiện; thẻ 2 khi tour tới bước 11 lần đầu; thẻ 3 khi `LessonCompleted`.
7. **Khung xanh bước hiện tại** (cách của 017, theo thao tác): vào scene → thẻ 1; lần đầu bấm Weiter/Zurück hoặc thẻ 1 xong → thẻ 2; quiz mở → thẻ 3; xong câu 4 → tắt.
8. **Điểm bắt đầu.** `Info Panel` (≈ −34°, trong tầm nhìn) có tiêu đề „1. Etikett lesen“ như thẻ 1; giữ nguyên chỗ.
9. **Log.** Không thêm event/trường (data PA 1). Back gọi `ShowStep` → đúng một `tour_step`; `quiz_unlocked` một lần (`!IsUnlocked`). Đổi `layout_version` thành "024" (→ "025" theo phần bổ sung của decision 025, mục 7: 024 và 025 ra cùng một build) (C# `ResearchEventData.cs`, spec, ghi chú `analyse_set.R`). Spec thêm: „Back = `step_index` giảm 1; chạm trong lúc giữ ‚Richtig!‘ không được chấm“. `make_fake_data.R` sinh chuỗi Back, `selftest.R` xanh, `data-analyst` duyệt. Chỉ số tour trong R là mục riêng (#82).

**Được:** đọc chữ cùng lúc với ô; quiz không che WMS; nút có nghĩa nhờ cạnh số bước, đi thành cặp, đứng yên; ít tấm lúc vào; lời báo trước của 020 vẫn còn; format log giữ; đổi chỗ chú thích chỉ là transform.

**Mất / chấp nhận:** viết lại 11 đoạn chữ (M–L, `german-reviewer`), rủi ro rút gọn làm rơi ý (chặn bằng Done when của 010); nút sát số bước chứ không sát chú thích; bảng EKZ (+60°) xa chú thích Empf Kz trái (bảng tra cứu, chú thích chỉ hướng sang phải); Concept có ✓ + khung, khác Basic (chủ ý giảm dần); tầm với của thanh và chỗ trống bên trái chưa đo; dữ liệu Concept trước/sau không gộp.

**Bị bác:** A (dự phòng); gộp prefab „tour station“ (rủi ro wiring); Back dạng mũi tên nhỏ/chạm số bước; thẻ + nút chạy theo ô (C); hai bảng (D); khoá/làm mờ pha 2 (dự phòng); nút „Nächste Frage“; trường `via`; event `tour_back`/`concept_card_done`; ✓ thẻ 1 sau kiện đầu tiên.

## Done when
Xem `docs/backlog.md` #78, #79, #80.

## Nếu giả định sai
- Không đủ chỗ giữa viền và `Info Panel`: dời `Info Panel` sang trái vài cm; vẫn không được thì chú thích trái đặt trên/dưới hàng; cuối cùng quay về A.
- Thanh khó với hoặc che dialog: nâng/kéo lại gần; cuối cùng gắn thanh vào chỗ cố định bên phải.
- Chú thích ngắn không đủ hiểu: thêm dòng, hoặc giữ đoạn dài ở chú thích bên phải cho bước đó.
- Người mới chỉ sai Ist/Verp.-Einh.: thêm mũi tên cột cho hai bước này ở viền dưới.
- Người học bỏ thẻ 1: đổi chữ thành „eine Verpackung“, ✓ sau kiện đầu tiên.
- Vẫn không biết bắt đầu đâu: làm mờ (không khoá) thanh điều hướng đến lần cầm kiện đầu tiên.
- Bấm nhầm Zurück/Weiter: tăng khe hoặc thu nhỏ „Zurück“.

## Ảnh hưởng tới backlog
- #76 xong trước hoặc cùng lúc (cùng `AnswerOption`; nút mới cần vùng chạm đúng).
- #77 gộp vào đây (chữ intro và thẻ 2 viết lại; thẻ 2 hiện nói „rechts neben dem Bildschirm“).
- #81 bổ sung theo bố cục này (chưa xét trong quyết định).
- #13: key mới.
- #82 mới: chỉ số tour trong R (`data-analyst`).
- Kiểm ComplexScene `Remainder Quiz` có cùng kiểu „Richtig!“ dính sang câu sau không; nếu có, mở mục nhất quán riêng.
- Chụp lại `TestEvidence/ConceptScene/views/overview.png` và `quiz.png` trước khi làm (ảnh hiện có là trước #73).
- Sau khi làm: cập nhật CLAUDE.md (ConceptScene, decision 024).

## Câu hỏi cho người hướng dẫn (giả định, kiểm ở buổi test)
D30 (chú thích ngắn có đủ không), D31 (cầm cả 3 kiện hay một kiện). Giả định: đủ; cả 3.

## Bằng chứng
- Back gọi lại `ShowStep` là đủ; Next wrap `% steps.Length` → `Assets/Scripts/Concept/WmsFieldTour.cs` (`NextStep`, `ShowStep`) → đã kiểm
- Quiz mở một lần nhờ `!IsUnlocked`; „Richtig!“ ghi rồi đổi câu ở frame sau; `LessonCompleted` có sẵn → `Assets/Scripts/PackageQuizController.cs` → đã kiểm
- Thẻ 1 „jede Verpackung“, thẻ 2 „rechts neben dem Bildschirm“ → `docs/i18n/strings-todo.csv` → đã kiểm
- Mũi tên cột sát „Modus / Auftragsart“ → `TestEvidence/ConceptScene/views/wms_step8.png`, ảnh người dùng 2026-11-04 → đã kiểm
- `layout_version` R đọc và tách nhóm → `analysis/analyse_set.R`, `Assets/Scripts/Research/ResearchEventData.cs` → đã kiểm
- Góc ≈ 45° giữa ô trái và bảng tour; thẻ 1 ở −60° ngoài tầm nhìn lúc vào → tính của xr-ux-reviewer; FOV Quest chưa xác nhận
- Chỗ trống giữa viền và `Info Panel`, tầm với của thanh, khe tối thiểu theo Meta → chưa kiểm (đo khi làm)
