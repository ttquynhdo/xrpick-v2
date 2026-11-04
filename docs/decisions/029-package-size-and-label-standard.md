# Quyết định 029: Mỗi loại kiện một cỡ ở mọi bài, mỗi carton một nhãn trên nắp, chữ nhãn cỡ cố định

- **Backlog:** #88
- **Trạng thái:** accepted
- **Ngày:** 2026-11-04
- **Thay thế (một phần):** #36 (nhãn mặt trước + nhãn nắp). Mục tiêu của #36 (đọc được nhãn từ chỗ đứng) giữ nguyên.
- **Ý kiến đã xét:** requirements-analyst, instructional-design-reviewer, xr-ux-reviewer, unity-reviewer → decision-synthesizer.
- **Lựa chọn của người dùng (2026-11-04):** chấp nhận phương án A.

## Bài toán
Test kính P801 (`docs/review-notes-p801.md` R3, R13, R15): ở Concept kiện nhỏ hơn Mixed/Complex và khó cầm; chữ nhãn auto-size nên cỡ chữ khác nhau giữa các kiện và chạy sát mép; sticker túi SCHRAUBE B chìa ra ngoài mép túi; mỗi carton có hai nhãn (mặt trước + nắp, #36), rối mắt.

## Xung đột giữa các đề xuất
| Vấn đề | Bên A | Bên B | Đánh đổi |
|---|---|---|---|
| Số nhãn mỗi carton | xr-ux: một nhãn trên nắp (từ chỗ đứng mắt nhìn xuống ≈ 46°, cách ≈ 1,15 m; mặt trước bị nhìn chéo) | instructional-design: thêm nhãn nhỏ cùng mẫu ở một mặt dài | Bớt dư thừa ↔ đọc được khi carton nghiêng hoặc đang cầm |
| Cách làm | unity B: script Editor chuẩn hoá tại chỗ | unity A: prefab Carton/Bag của dự án làm nguồn duy nhất | Không đổi tham chiếu ↔ dễ bảo trì |
| Đọc nhãn khi cầm ở Concept | instructional-design: nhãn ảo của #90 | xr-ux: cầm lên thì luôn đọc được; tiêu chí là đọc từ chỗ đứng | Phụ thuộc #90 hay không |

## Các phương án
| | Đúng nghiệp vụ | Hiệu quả học | Tải nhận thức/UX | Chi phí/rủi ro | Nhất quán |
|---|---|---|---|---|---|
| **A. Một nhãn trên nắp, cỡ carton theo Mixed, chữ cố định có lề, túi một cỡ, nhãn trong mép túi; script Editor tại chỗ + test bounds và nhãn = `PackageData`** | 0 (tài liệu im lặng) | + | ++ | + | ++ |
| B. Như A + nhãn nhỏ ở một mặt dài | 0 | ++ | 0 (vẫn hai nhãn, đúng điều R15 chê) | 0 | + |
| C. Như A nhưng thay mọi kiện bằng prefab mới | 0 | + | ++ | − (mất tham chiếu `EkzPlacementCheck`, `PositionStockSwitcher`, rider) | ++ |
| D. Chỉ sửa cỡ chữ và nhãn túi | 0 | 0 | − | ++ | − |

## Quyết định: phương án A
1. Mỗi loại kiện (carton, túi) có một cỡ ở Concept, Basic, Mixed, Complex; cỡ chuẩn = carton/túi của Mixed. Cả vật tĩnh `Stock STECKER C` và `Carton Count Demo` cũng theo cỡ này.
2. Mỗi carton chỉ một nhãn, trên nắp (`Label Top`), khoảng 80 % nắp, giữa nắp, dòng chữ song song mép trước thùng; bỏ sticker mặt trước `Cube`.
3. Chữ nhãn cỡ cố định (không auto-size), lề mỗi bên ≥ 8 % chiều rộng nhãn; một cỡ cho cả 3 dòng ở mọi kiện, chọn sao cho „SCHLAUCHSCHELLE B“ vừa. Nội dung giữ như decision 007 (`NAME` / `Mat.-Nr: …` / `VPE: n ST`), khớp `PackageData`. Viền mảnh quanh nhãn là chi tiết tuỳ chọn nếu vẫn vừa.
4. Túi: một cỡ; nhãn lùi vào trong mép túi ≥ 5 mm, dán sát mặt túi (không thành tấm cứng).
5. Carton trong thùng kho (Basic, Mixed, Complex, Concept) nằm ngửa nắp lên.
6. Làm bằng script Editor (`Assets/Scripts/Editor/`, có dry-run như `LocalizationBinder`), sửa tại chỗ, không thay instance, không sửa thư mục art GVOZDY; sửa con của prefab-instance thì ghi override (`PrefabUtility.RecordPrefabInstancePropertyModifications` + SetDirty). Chuẩn hoá cỡ bằng scale của mesh con, không đổi scale root nếu làm lệch trigger/collider. Kèm EditMode test hoặc lệnh kiểm qua MCP: báo lỗi khi bounds lệch cỡ chuẩn hoặc chữ nhãn khác `PackageData`.
7. Đọc nhãn khi đang cầm ở Concept do #90 lo (thẻ nhãn cạnh kiện); không có nhãn ảo ở Mixed/Complex.

**Được:** bỏ nhãn dư thừa và chữ sát mép; Concept cầm dễ như Mixed; không đổi tham chiếu; chạy lại được.
**Mất:** carton nằm nghiêng hoặc đang cầm có thể không thấy nhãn (bù bằng mục 5 và #90).

**Bị bác:** B (chính là dư thừa R15 chê; giữ làm dự phòng); C (rủi ro tham chiếu, có thể làm sau thành mục riêng); D (không giải quyết cỡ kiện).

## Nếu giả định sai
- Trên Quest không đọc được nắp từ chỗ đứng, hoặc người hướng dẫn nói nhãn thật ở mặt bên (A14): chạy lại script thêm nhãn mặt cùng mẫu (chuyển sang B).
- Carton cỡ Mixed không vừa 6 ô 2 × 3 ở Basic: chỉ dựng lại lưới `BoxSnapper`, không thu nhỏ carton.
- Carton cỡ Mixed không vừa hàng trước bàn hoặc `Carton Count Demo` ở Concept: dời chỗ đặt, không thu nhỏ carton.

## Câu hỏi còn mở
- A14 (người hướng dẫn): nhãn trên carton/túi kho thật nằm ở đâu, đọc lúc nào, cỡ kiện điển hình. Không chặn.
- Cỡ chữ (chiều cao chữ hoa 6 / 8 / 10 mm): chọn khi thử trên Quest ở cảnh `labels`.

## Bằng chứng
- Tài liệu yêu cầu không nói vị trí, cỡ nhãn hay cỡ kiện nhập (requirements-analyst, Kommissionierung slide 3–4, 9–10).
- P801 R3, R13, R15: `docs/review-notes-p801.md`.
- Kiện là instance prefab `Assets/GVOZDY/Cardboard Boxes with Tape/Prefabs/box_1_*`; `Assets/Prefabs/` không có prefab kiện (unity-reviewer).
- Bounds thật của carton Mixed và độ vừa 2 × 3 ở Basic: chưa đo, đo trước khi làm.
