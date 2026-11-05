# Kết quả buổi test kính P802 (2026-11-05)

Nguồn: `05_data/raw/P802_f0bb845f-…_events.jsonl` + `_summary.csv` (log), `07_presentations/com.xrise.xpick-20261105-194250-0.mp4` (quay màn hình kính, 13 phút).
Build `727eee1`, `layout_version` 033, Quest 3, tay (hands), ngôn ngữ **EN**, ID pilot (P8xx → `research/pilot/`).
Đồng bộ: **giờ video ≈ t trong log + 19 s**. Video gần như không có tiếng (trung bình −49,5 dB), không có think-aloud.

## 1. Tổng quan

| Bài | Thời gian | Lần xác nhận / trả lời | Đúng ngay lần đầu | Lỗi (log) | Lần cầm |
|---|---|---|---|---|---|
| 1. Welcome | 49 s | – | – (cầm hộp 17 s, bấm nút 31 s) | – | – |
| 2. Concept (tour + quiz) | 374 s (6,2 phút) | 8 câu trả lời quiz | 1/4 câu | WRONG_ANSWER ×4 | 18 |
| 3. Basic (1 vị trí) | 70 s | 4 | không | EMPTY, EKZ_PLACEMENT, TOO_FEW | 15 |
| 4. Mixed (2 vị trí) | 118 s | 5 (3 + 2) | 0/2 vị trí | EKZ_PLACEMENT, TOO_FEW ×2 | 26 |
| 5. Complex (quiz + 2 vị trí) | 114 s | quiz 4 lần; xác nhận 4 (3 + 1) | quiz 1/2; vị trí 1/2 | TOO_MANY, MISSING_LOOSE ×2 | 25 |
| **Tổng** | **724 s (12,1 phút)** | | | | |

- Xong cả khoá (`session_end` = `completed`), đi đúng thứ tự qua Menu, không bị khoá, không tạm dừng, không tháo kính, 0 lỗi Unity.
- Ở cả 5 vị trí của bài thực hành, chỉ **1/5 vị trí đúng ngay lần đầu** (Complex vị trí 2). Lần nào cũng sửa được sau 2–4 lần.

## 2. Diễn biến từng bài (log + video)

### Welcome
Nhập mã P802, chọn English, cầm hộp (17 s) và bấm nút đỏ (31 s), dấu xanh hiện đúng, chọn „2. Basics“ ở 48 s.

### Concept
- Mở tour sau ~19 s (khi cầm carton STECKER C). Tour 11 bước mất 271 s (t 67 → 338).
- Bước lâu nhất: `cartons` (63 s) — người học tự xếp 3 carton STECKER C vào thùng đơn, nhãn „40 / 80 / 120 = Soll“ hiện đúng (video 3:40–4:20). Các bước khác 9–44 s.
- Bấm „Back“ 2 lần (Rest ↔ Empf Kz 003; Weg der Behälter → not_used); sau đó Ist về 0 và thùng được nạp lại, chạy demo băng chuyền lại — đúng thiết kế.
- **Quiz: 8 lần chạm cho 4 câu, chỉ câu `ekz_000` đúng lần đầu.** Kiểu trả lời là *thử lần lượt*: câu 1 (Soll 100, VPE 25) chọn A „2“ → C „25“ → B „4“ trong 6 s; câu 2 và 3 sai rồi đúng cách nhau ~1,2 s (video 6:43–7:04). Tức là không đọc lời giải sai, mà quét nút đến khi đúng.
- Sau quiz bấm „Start over“ một lần rồi mới vào Menu.

### Basic (Soll 300, VPE 50 → 6 carton, Empf Kz 003)
1. **13 s sau khi vào bài, bấm nút đỏ khi thùng đơn còn trống** (EMPTY) — chưa cầm gì (video 7:35).
2. Xếp 6 carton nhưng 1 carton nằm chồng/nghiêng lên trên → EKZ_PLACEMENT („Not all cartons are placed in order“) (video 8:04).
3. Lấy carton đó ra, để trên bàn, **bấm nút đỏ khi đang sửa** → TOO_FEW (5/6).
4. Đặt lại phẳng → đúng (53 s). Thùng chạy đi đúng, mũi tên làn giữa hiện.

### Mixed
- Vị trí 1 (KABELVERBINDER A 150/50, Empf Kz 003): 3 carton nhưng chồng lên nhau → EKZ_PLACEMENT; lấy 1 ra → bấm → TOO_FEW (2/3); xếp 3 carton vào 3 ô → đúng (47 s sau khi vào bài). Giống hệt chuỗi lỗi ở Basic.
- Vị trí 2 (SCHRAUBE B 60/20, Empf Kz 000): cho **2 túi** → TOO_FEW, thêm túi thứ 3 → đúng. Thùng đổi vị trí và chạy đi đúng.

### Complex
- Quiz phần dư: câu „bao nhiêu carton“ đúng ngay (2); câu „bao nhiêu cái lẻ“ chọn B „20“ → C „55“ → A „5“ trong ~2 s (thử lần lượt, video 10:41–10:44).
- Vị trí 1 (RING D 55/25 → 2 carton + 5 cái): **dù vừa trả lời đúng „2 carton“, vẫn đặt 3 carton** (Ist 75) → TOO_MANY; trả 1 carton lại → bấm khi chưa có cái lẻ → MISSING_LOOSE; thêm 5 vòng → đúng.
- Vị trí 2 (SCHLAUCHSCHELLE B 156/50 → 3 carton + 6 cái): **đúng ngay lần đầu** (33 s).
- Bảng Congratulations hiện sau khi thùng chạy đi, có „Code: P802“. Sau đó người học chạm vào đĩa facilitator (video 12:36–12:42), không có gì xảy ra vì phiên đã kết thúc.

## 3. Nhận xét (để phân tích chung sau, chưa sửa gì)

1. **Nút đỏ được dùng như công cụ “kiểm tra thử”.** 13 lần xác nhận cho 5 vị trí; nhiều lần bấm khi rõ ràng chưa xong (thùng trống, carton đang nằm trên bàn, chưa có cái lẻ). Phản hồi lỗi giúp sửa nhanh, nhưng chỉ số `first_attempt_correct` sẽ thấp vì chiến lược này chứ không hẳn vì hiểu sai.
2. **Quiz cho phép đoán bằng cách quét A/B/C.** Ở Concept và Complex, các lần sai–sai–đúng chỉ cách nhau 1–2 s, không đọc dòng giải thích. Cần quyết định: khoá tạm vài giây sau câu sai, hoặc chỉ tính lần chạm đầu cho nghiên cứu (R đã có `attempt_in_question`).
3. **Hiểu → làm chưa chuyển sang được ở Complex:** trả lời đúng „2 carton“ nhưng vẫn lấy 3. Có thể là thói quen “lấy nhiều cho đủ” từ Basic/Mixed, hoặc nhìn Soll 55 → làm tròn lên.
4. **Lỗi EKZ 003 lặp lại ở Basic và Mixed theo cùng một kiểu** (chồng carton → lấy ra → bấm khi còn thiếu). Thông báo „not placed in order“ chưa nói rõ phải làm gì (đặt từng carton vào ô trống, không chồng).
5. **Mixed vị trí 2: 2 túi thay vì 3** (60 : 20) — lỗi phép chia hoặc chưa đọc VPE mới sau khi đổi vị trí.
6. Concept chiếm hơn một nửa thời gian (6,2 / 12,1 phút), chủ yếu ở bước xếp carton; các bài thực hành 1–2 phút mỗi bài, khớp ước tính „khoảng 5 phút“ là dư.
7. Kỹ thuật: không lỗi, không giật thấy được trên video, băng chuyền / đổi thùng / thẻ bước / dấu xanh chạy đúng.

## 4. Kết quả `analyse.R` (pilot set)

Chạy ngày 2026-11-05: log được chép vào `analysis/data/wave-2026-11-05/research/pilot/`, kết quả ở `analysis/output/wave-2026-11-05/pilot/` (git-ignored). 5 visit khớp với summary.csv (0 chỗ lệch), không có cảnh báo, `data_quality`: completed, EN, 1 layout (033), 0 lỗi.

**Đúng ngay lần đầu theo mục tiêu (`objectives_summary.csv`, n = 1, chỉ để mô tả):**

| Mục tiêu | Mục | Đúng lần đầu |
|---|---|---|
| D2 biết thùng lưu / thùng đơn | Concept: tự cho carton vào thùng đơn | có (đủ Soll sau 185 s) |
| D3 phân biệt Empf Kz 003 / 000 | quiz ekz_003, ekz_000 | 1/2 |
| P0 làm xong cả 3 bài thực hành | | có |
| P1 so nhãn với LVS | quiz label_by_matnr | 0/1 |
| P2 xếp theo Empf Kz 003 | Basic vt 1, Mixed vt 1 | 0/2 |
| P3 nhiều vị trí nối nhau | Mixed vt 1, vt 2 | 0/2 |
| P4a tính số carton | 6 mục | 3/6 |
| P4b tính số cái lẻ | Complex quiz loose, Complex vt 2 | 1/2 |

**Thời gian từng vị trí (`process_by_position.csv`, giây: đọc / lấy hàng / kiểm tra / tổng):** Basic 23 / 27 / 3 / 53 · Mixed vt 1 12 / 24 / 11 / 47 · Mixed vt 2 3 / 45 / 3 / 51 · Complex vt 1 14 / 47 / 1 / 63 · Complex vt 2 6 / 18 / 2 / 27. Ở Mixed vt 2 người học chỉ đọc 2,6 s rồi lấy hàng ngay, và đây là chỗ cho thiếu túi.

**Tour Concept (`tour_steps.csv`):** lâu nhất là `cartons` 63 s, `material_flow` 45 s, `intro` 44 s, `verp_einh` 38 s; ngắn nhất là `soll` 9 s. Bấm Back ở `empf_kz_003` và `material_flow`, mỗi chỗ 1 lần.

**Thùng chờ (`station_waits.csv`):** lần nào thùng cũng chờ đúng 4,0 s sau khi xác nhận đúng (Complex thùng 1 chờ 0,6 s), không có độ trễ bất thường.

**Một điểm dữ liệu cần xem lại:** `data_quality.open_grabs` = 7, tức có 7 lần cầm không có `grab_end`. Trong đó 6 lần là 6 carton Basic đã nằm trong ô `BoxSnapper`, và 1 lần là một `clamp` ở Complex (có lẽ còn đang cầm khi phiên kết thúc). Có thể khi carton bị hút vào ô snap thì log mất sự kiện thả tay. Như vậy số lần cầm và thời lượng cầm ở Basic không đáng tin, nhưng kết quả xác nhận (outcome) không bị ảnh hưởng.

## 5. Giới hạn
- Một người, bản EN; không có tiếng nói nên không biết người học nghĩ gì khi bấm thử.
- Video chỉ xem theo khung hình mỗi 6 s (và 1 s ở các đoạn lỗi), có thể bỏ sót thao tác ngắn.
- n = 1 nên mọi tỉ lệ ở mục 4 chỉ để mô tả (khoảng tin cậy Wilson rất rộng). Thứ tự bài cố định nên không tách được việc học với độ khó của bài.
