# Checklist backlog chờ test tay (2026-11-17)

Các item đang `verified-mcp` trong `docs/backlog.md`: MCP/code đã ✅, chỉ còn case tay (kính/Quest hoặc việc bạn làm).
Cách điền cột **Kết quả**: `✅` (→ item thành `done`), `❌ + ghi chú` (lỗi), `bỏ` (không cần test nữa).
Bỏ qua #135 (chỉ tool Editor, không có case tay).

| # | Item | Case tay còn chờ | Kết quả | Ghi chú |
|---|---|---|---|---|
| #11 | Đa ngôn ngữ DE/EN, nút chọn ngôn ngữ | L9 build Concept trên Quest, L10 chạm nút ngôn ngữ | OK| |
| #13 | File export EN–DE cho người duyệt | E1 mở file trong Excel, umlaut đúng |OK | |
| #19 | Complex hai bước, ví dụ chia có dư | C6, C7, C14, B1 (cầm vòng thật, Ist ±1, tầm với) | OK| |
| #28 | Log mỗi lần xác nhận | T1 cầm thùng + bấm nút ở Welcome, T2 Mixed 1 lần bấm = 1 `confirm_pressed` | | |
| #29 | Log thao tác cầm/thả, Menu | H1 cầm thật vào/ra thùng, H2 chạm mục Menu khoá | | |
| #31 | Hướng dẫn lấy log + gộp phiếu giấy | M1 chép log 2 phiên, M2 nhập 2 phiếu + chạy `analyse.R` | | |
| #36 | Nhãn kiện đọc được từ chỗ đứng | H1–H3 đọc nhãn, cầm carton/túi |OK | |
| #39 | Welcome: phản hồi khi cầm thùng và chạm nút | H1 tay thật, nghe tiếng, thấy ✓ | OK| |
| #40 | Nổi bước hiện tại trên thẻ | H1 cầm thật, H2 thả/lấy thật, H3 liếc giá thẻ |OK | |
| #42 | Khoá Menu theo thứ tự bài | H1 chạm mục mờ, H2 crash giữa Mixed rồi `Fortsetzen` (P899, xoá log sau) | OK| |
| #51 | Bàn sau đứng trên bàn làm việc | H1 nhìn bàn sau trên kính |OK | |
| #52 | Concept: nút Weiter/A/B/C dễ bấm | H1–H3 tầm với, bấm trúng, không bấm nhầm | OK| |
| #53 | Complex: thẻ 1 nhắc trả lời câu hỏi | T4 đọc thẻ ở cả 2 vị trí | OK| |
| #56 | Basic: 6 carton EKZ 3 sát nhau | H1 đặt carton thứ 6, H2 Ist ±50 |OK | |
| #57 | Concept: bảng tổng quan EKZ | E8b đọc mọi dòng từ chỗ đứng |OK | |
| #59 | Thẻ kết quả cạnh nút đỏ | H1 đọc thẻ sau sai + đúng (3 scene) | OK| |
| #62 | Thẻ hướng dẫn: Ist = Soll, EKZ, ví dụ | T10 đọc thẻ ở 3 scene |OK | |
| #64 | Menu: chữ to hơn | F6 đọc Menu, chạm 1 mục | OK| |
| #67 | Mixed: số đơn không dễ hơn Basic | H1 cầm túi vào/ra thùng |OK | |
| #69 | Welcome: nút đỏ cùng chỗ | R5 bấm bằng tay trên Quest |OK | |
| #71 | Ba làn, thùng kho chạy ra/vào | T5, H1–H4 (đang cầm khi thùng chạy, FPS, không xuyên vật, bài 5 từ Menu) |OK | |
| #72 | Basic: ba làn, thùng chạy đi | T6 đang cầm khi xác nhận, T7 không xuyên vật |OK | |
| #75 | Mã pilot P800–P899 | P1 một phiên `P801` EN, chạy `analyse.R` |OK | |
| #86 | Welcome: bảng mục tiêu hai cụm | W86-H đọc bảng DE/EN; X7 luồng Complex tới `Congratulations` | OK| |
| #87 | Mẫu thẻ nhiệm vụ chung | H1 đọc thẻ 5 bài DE/EN |OK | |
| #88 | Chuẩn kiện và nhãn | H1 cầm/thả 4 bài, H2 đọc nắp carton trong thùng kho |OK | |
| #89 | Concept: thanh tour | H1–H4 (một phần đã đổi bởi #129) |OK | |
| #90 | Concept: nhãn cạnh kiện đang cầm | H1–H3 (thẻ cạnh kiện đã bỏ ở #104 → có thể `bỏ`) |OK | |
| #91 | Concept bước `cartons` | xem lại khi đeo kính |OK | |
| #92 | Concept: một chuỗi thao tác tại thùng kho | H1 lấy carton kho → đơn, H2 đọc ký hiệu + lý do |OK | |
| #93 | Cụm câu hỏi thu lại (Concept + Complex) | H1 „Zu den Fragen“/„Zurück“, H2 Complex nhặt vị trí 1 |OK | |
| #94 | EKZ 0 / EKZ 3 | H1 Mixed xếp 3 carton vào ô + đổ túi, H2 Concept bước 8–9 |OK | |
| #95 | Băng chuyền: ray thừa | xem lại băng ở buổi test chung |OK | |
| #97 | Dấu ✓ trên thẻ ở Basic/Mixed/Complex | H1 ✓ đúng lúc, H2 đọc dòng position + ✓ | OK| |
| #99 | Chờ đọc phản hồi rồi thùng mới chạy | H1–H3; T9b (MCP chưa chạy) | | |
| #100 | Concept: báo kiện sai/thừa | M1–M3 | | |
| #101 | Concept: mẫu EKZ 003 | H1 đọc nắp + thẻ mẫu | | |
| #103 | Vùng cầm carton quá rộng | T5–T8 | | |
| #104 | Concept: dòng hành động, bước 11 | H1 | | |
| #105 | Concept: khung/đường dẫn tới ô LVS | H1 | | |
| #107 | Concept: mẫu 000 ↔ 003 | H1 thấy khác biệt xếp ↔ lộn xộn | | |
| #108 | Basic/Mixed: thấy chỗ đặt EKZ 003 | H1 thả carton vào ô, lên carton khác | | |
| #109 | Thẻ kết quả ở chỗ đang nhìn | H1, F2-đọc (kịp trong 4 s), F7-tay | | |
| #111 | Biển thùng, ghi chú carton đã mở | H1 Complex, H2 Concept bước 5/6/11, H3 Basic lớp trên/dưới | | |
| #113 | Complex: bảng kết thúc không đè LVS | H1 | | |
| #115 | Mixed: collider túi SCHRAUBE B | H1 cầm ở cổ và thân | | |
| #116 | Welcome: tiêu đề, thẻ 1 | H1 đọc bảng + thẻ 1 (DE) | | |
| #117 | Chuyển bài: tô sáng Menu | H1 cuối Concept chọn bài 3 | | |
| #118 | Concept tour bước 1/5–6/9/11 | I4b nhìn đúng phía bước 9, I5b đủ thời gian bước 11 | | |
| #120 | Complex: bảng chúc mừng | E8 đọc bảng cuối | | |
| #121 | Welcome: mục tiêu trước, việc sau | W12 „Zu den Aufgaben“ không nhầm nút đỏ | | |
| #122 | Complex: bảng ví dụ công thức | E6 đọc bảng | | |
| #123 | Complex: câu hỏi Empf Kz 000 | Q14 chạm câu 3, đọc chữ trên nút | | |
| #124 | Menu dễ nhận ra | M5 chọn mục nền xanh không cần hỏi | | |
| #126 | Nút bấm không ăn | H1 mỗi nút 10 lần → 10/10 | | |
| #129 | Concept: tour + câu hỏi gần tay | N9 | | |
| #130 | Complex vị trí 1: câu hỏi gần tay | M8 bấm A/B/C tại chỗ | | |
| #131 | Thẻ không xoay theo đầu | K1 nhìn quanh thẻ, K2 tìm carton sai không cần dấu | | |
| #132 | Cỡ chữ cố định ≥ 0,8° | K1 đọc + chạm, K2 ghi chú carton đã mở | | |
| #136 | Concept: mũi tên tắt sau lần cầm đầu | H1 | | |
| #137 | Concept: đơn mẫu Empf Kz 000 | H1 đọc bước 8–9 | | |
| #138 | Concept: mũi tên đứng thẳng | H1 nhìn mũi tên khi nhún | | |
| #139 | Mixed/Complex: để nguyên hàng vị trí trước | H1 lấy kiện cũ ra rồi đặt lại → thẻ vẫn hiện | | |

Điền xong thì gửi lại file này, tôi sẽ cập nhật `docs/backlog.md` và các `docs/test-reports/bl<nn>.md`.
