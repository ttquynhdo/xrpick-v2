# Quyết định 027: Mẫu thẻ nhiệm vụ chung (thẻ „Aufgabe“ nền kem, slot đều do công cụ đặt, ván co theo thẻ) và bảng mục tiêu Welcome hai cụm

- **Backlog:** #87 (mẫu thẻ), #86 (bảng Welcome) (một quyết định chung)
- **Trạng thái:** accepted
- **Ngày:** 2026-11-04
- **Ý kiến đã xét:** instructional-design-reviewer, xr-ux-reviewer, unity-reviewer, german-reviewer → decision-synthesizer. Không hỏi requirements-analyst (không quy tắc nghiệp vụ nào đổi, chỉ hình thức và chữ).
- **Lựa chọn của người dùng (2026-11-04):** chấp nhận phương án A, thẻ mục tiêu **nền kem**. Rút gọn dần chữ qua các bài (fading) để thành mục backlog riêng.

## Bài toán
Test kính P801 (`docs/review-notes-p801.md` R1, R2): thẻ „Aufgabe“ trông giống hệt thẻ bước; khoảng cách giữa các thẻ không đều trong một bài và giữa các bài (slot đặt tay, #62/#63); ván gỗ cùng cỡ ở mọi bài (Welcome 3 thẻ trên ván rất to; Mixed/Complex chữ dày). Bảng „Das lernen Sie“ ở Welcome: chữ sát lề trái, dòng sát nhau, mục 1 bản EN xuống dòng, 5 mục không chia nhóm.

Mục tiêu: nhìn lướt là thấy mục tiêu bài rồi đến các bước; thẻ cách đều; ván vừa với số thẻ; bảng Welcome hai cụm, mỗi mục một dòng ở DE và EN.

## Xung đột giữa các đề xuất
| Vấn đề | Ý kiến | Cách giải |
|---|---|---|
| Xếp thẻ cho đều | xr-ux: VerticalLayoutGroup trong prefab. unity: giữ mỗi thẻ một canvas, công cụ Editor đặt slot một lần; layout group phải dựng lại 5 scene, dễ vỡ tham chiếu của highlighter/`ConceptCardProgress`/`PositionTexts`, rebuild layout lúc chạy trên Quest | Công cụ Editor (unity A). Đổi lại: chạy lại công cụ khi sửa chữ |
| Thẻ mục tiêu đổi chữ hay đổi kiểu | instructional: „Ziel dieser Lektion“, thân 1 câu mục tiêu. german: `common.card.task_title` dùng ở 5 scene, có trong glossary; „Lernziel“ nghe như trường học | Chỉ đổi kiểu, giữ „Aufgabe“. Dự phòng nếu cần tín hiệu chữ: „Ihre Aufgabe“ |
| Bỏ thẻ Aufgabe, đưa mục tiêu lên tiêu đề ván | instructional PA2 | Bác: tiêu đề dễ bị đọc như trang trí, thứ tự mục tiêu → bước yếu đi |
| Fading qua các bài | instructional PA3; xr-ux: không đổi nội dung bước | Để thành mục backlog riêng (#98) |
| Tên đầy đủ „Lagerverwaltungssystem (LVS)“ trên bảng Welcome | CSV ghi chú: viết đầy đủ lần đầu. german: một dòng mỗi mục thì phải bỏ | Tên đầy đủ đặt một lần ở tiêu đề cụm 1, các mục chỉ ghi „LVS“ (câu hỏi cho người hướng dẫn, không chặn) |

## Phương án đã chọn: A
### Mẫu thẻ (#87, cả 5 scene)
1. Thẻ „Aufgabe“ (thẻ mục tiêu): **nền kem**, viền trung tính, chữ tối, không đánh số; giữ chữ „Aufgabe“ (`common.card.task_title`). Không bao giờ có khung xanh, không bị làm mờ, không có ✓ (như hiện nay). Màu không là tín hiệu duy nhất: khác thêm ở viền và vị trí đầu cột. Kiểu nằm trong `Assets/Scripts/InstructionCardLook.cs` (dùng chung với `InstructionStepHighlighter`/`ConceptCardProgress`).
2. Thẻ bước giữ nền đen và khuôn `1. Angaben im LVS ablesen` / `2. … kommissionieren` / `3. Auftrag bestätigen`. Xanh #0391DD chỉ dùng cho khung bước hiện tại.
3. Mỗi thẻ vẫn là một canvas world-space riêng (không layout group). Một công cụ Editor (`XRPick/…`, chạy được qua MCP) đo chiều cao mỗi thẻ theo chữ dài nhất (DE/EN, cả hai vị trí của `PositionTexts`), rồi đặt slot với **một khoảng hở chung cho cả 5 scene** và co ván gỗ theo tổng chiều cao với một lề dưới cố định. Chiều cao thẻ cố định (quy tắc #62), nên khi đổi vị trí không thẻ nào nhảy chỗ. Complex giữ ván cao hơn để thẻ 3 nằm trên thùng của vị trí 2. Sửa chữ thẻ về sau → chạy lại công cụ (thay cho „re-measure the slots“ bằng tay).
4. Thân thẻ Mixed và Complex tối đa 2 câu: bỏ phần giải thích viết tắt và cụm „der im Lagerverwaltungssystem (LVS) angezeigt wird“; giữ nguyên nhãn WMS (`Mat.-Nr`, `Verp.-Einh.`, `Empf Kz` với `<nobr>`), câu „Drücken Sie dann den roten Knopf (F6).“ và thuật ngữ „lose Teile“. Thân thẻ vẫn là mệnh lệnh có Sie (kiểu telegram chỉ cho tiêu đề bước).

### Bảng Welcome (#86)
5. Hai cụm, mỗi cụm có câu dẫn riêng kết thúc bằng „:“: cụm 1 „Sie verstehen …:“ (bài 2, đọc LVS, so nhãn; tên đầy đủ „Lagerverwaltungssystem (LVS)“ ở đây), cụm 2 „Sie können:“ (bài 3–5). Không dùng danh từ „Wissen“/„Können“, bỏ „Am Ende des Kurses …“.
6. Mục xếp theo thứ tự bài, mỗi mục vừa một dòng ở DE và EN (rút chữ, không giảm font dưới cỡ thân thẻ). Mục EKZ nói chung „nach EKZ“, không „EKZ 3“ (D4).
7. Lề trái bằng lề phải, ít nhất bằng chiều cao một chữ; dòng giãn hơn mặc định; tiêu đề cụm cùng kiểu với thẻ mục tiêu. Hai dòng hướng dẫn thao tác (vàng) thành khối riêng dưới hai cụm. Bảng không che Menu, giá thẻ hay thùng.
8. Không ghép câu từ mảnh chữ; mỗi dòng/cụm là một key trong `docs/i18n/strings-todo.csv`. Mọi chữ mới qua `localize-strings` + `german-reviewer`.

## Được / mất
- **Được:** phân biệt mục tiêu và bước khi nhìn lướt (signalling); thẻ đều trong bài và giữa các bài; thẻ không nhảy khi đổi vị trí; không layout rebuild lúc chạy trên Quest; ít đụng script và tham chiếu.
- **Mất:** thẻ chỉ đều ở thời điểm chạy công cụ (sửa chữ phải chạy lại); không có tín hiệu chữ „Ziel“; bảng Welcome không còn tên đầy đủ trong từng mục.

## Ràng buộc phải giữ
- Không đổi tên/xoá script hay `.meta` (`InstructionCardLook`, `InstructionStepHighlighter`, `ConceptCardProgress`, `PositionTexts`).
- Không thêm `LocalizeStringEvent` lên TMP do script ghi.
- Đổi RectTransform dưới prefab instance bằng code: `anchoredPosition3D` + Undo/SetDirty.
- Sửa prefab `InstructionShelf` thì kiểm override ở cả 5 scene (vị trí/góc, Complex scale y).
- Khung xanh, làm mờ và ✓ `Done Check` (#39, #40, #79) vẫn bám đúng thẻ; badge dời lên trên khung lúc chạy phải test lại.

## Nếu giả định sai
- Thẻ kem bị lóa trên gỗ khi đeo kính → nền đen + viền/dải đầu thẻ trung tính (chỉ sửa `InstructionCardLook`).
- Người học vẫn không nhận ra thẻ mục tiêu → đổi „Aufgabe“ thành „Ihre Aufgabe“ (một key + glossary).

## Câu hỏi còn mở
- D33 (người hướng dẫn, không chặn): chỉ ghi „LVS“ trong các mục có đủ không khi tên đầy đủ đứng ở tiêu đề cụm 1. Giả định: đủ.
