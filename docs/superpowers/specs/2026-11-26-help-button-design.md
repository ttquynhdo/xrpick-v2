# Nút Hilfe (gọi người điều phối) — thiết kế

- **Ngày:** 2026-11-26 · **Trạng thái:** người dùng duyệt 2026-11-26
- **Backlog:** #182 · **Build:** vào cùng build "041" với decision 043 (một lần tăng `layout_version`)

## Vì sao

Khi người học bí, họ cần cách gọi người điều phối mà không ngại, không phải tháo kính hay gọi to. Đồng thời người dùng muốn log biết chính xác lúc nào người học được giúp, để tách lượt làm "có trợ giúp" khỏi kết quả lần đầu đúng.

## Mục tiêu / không làm / phải tránh

- **Mục tiêu:**
  - người học bấm không ngại (một lần, không phải chọn gì);
  - không bấm liên tục (nút cho thấy đã nhận);
  - người điều phối biết ngay (xem màn hình stream, chuông nhẹ là dự phòng);
  - log có giờ thật để khớp với phiếu của người điều phối.
- **Không làm:**
  - khoá theo thời gian;
  - người học tự chọn loại giúp;
  - nút "đã xong";
  - câu nói bằng giọng.
- **Phải tránh:**
  - bấm nhầm trong lúc làm bài (nút xa vùng làm việc, xem #177);
  - chữ tiếng Anh trước người học Đức;
  - thêm tải đọc trong scene.

## Cơ sở (tìm tài liệu 2026-11-25)

Không có nghiên cứu nào so sánh trực tiếp các cách thiết kế. Tiền lệ gần nhất:
- Hệ thống gia sư thông minh ghi từng lần xin gợi ý và suy ra lúc kết thúc từ hành động tiếp theo.
- Katz 2017 không ghi các lần người hướng dẫn can thiệp và tự nhận đó là hạn chế.
- Thang gợi nhắc (system of least prompts) do người giúp phân loại, không do người học tự chọn.
- Chưa có bằng chứng rằng khoá nút có tác dụng.
- Thời điểm giúp quan trọng (An 2026; Goldberg 2015). Vì vậy dùng quy trình của người điều phối thay cho khoá.

## Các cách đã xét

| | Cách | Kết quả |
|---|---|---|
| A | Bấm một lần, log chỉ ghi lúc bắt đầu, kết thúc suy ra từ hành động tiếp theo | **Chọn**, kèm phiếu thang bậc |
| B | Như A + người học bấm „Weiter“ khi giúp xong | Bỏ: thêm thao tác, người điều phối đã ghi giờ |
| C | Người học tự chọn loại giúp trong app | Bỏ: chưa được kiểm chứng, thêm thao tác trong VR |

## Thiết kế

**Người học**
- Mỗi scene có một nút poke „Hilfe“, chỉ hiện khi có session đang chạy (Welcome: từ lúc nhập mã xong, không hiện trên bàn phím nhập mã).
- Vị trí nằm trong tầm tay nhưng ngoài vùng làm việc: không gần nút đỏ, không gần thanh tour/câu hỏi, không gần Menu. Cùng một chỗ ở cả 5 scene. Đề xuất: bên trái bàn, dưới kệ thẻ hướng dẫn; tư thế cụ thể do người làm chọn, kiểm bằng `TextComfortAudit`.
- Bấm thì:
  - phát chuông nhẹ một lần;
  - nút chuyển thành „Hilfe kommt …“, mờ đi và không bấm được.
- Nút trở lại bình thường khi xảy ra một trong các việc sau:
  - một lượt bấm nút đỏ được chấm, hoặc một câu trả lời quiz;
  - đổi scene;
  - sau 60 s (lựa chọn của Claude, để người học bí lại vẫn gọi được).
- Chữ: `help.button` „Hilfe“ / „Help“; `help.waiting` „Hilfe kommt …“ / „Help is coming …“, qua `localize-strings`.
- Nút poke theo giá trị chung của #177 và `PokeButtonFixer`.

**Người điều phối**
- Xem stream, thấy ngay khi người học bấm, ra giúp.
- Ghi một dòng trên phiếu: giờ (đồng hồ), mã người học, scene/bước, mức giúp, ghi chú.
- Mức giúp:
  1. chỉ chỗ;
  2. giải thích;
  3. làm mẫu.
- Quy trình: hỏi „Was haben Sie schon versucht?“, rồi giúp từ mức 1.
- Phiếu là một file trong `docs/` (bảng in được). Ngôn ngữ của phiếu theo người điều phối.

**Log** (hợp đồng C# ↔ R)
- Event mới `help_requested` gồm:
  - `scene`;
  - giờ thật (theo định dạng giờ hiện có của log);
  - ngữ cảnh: bước tour / số câu hỏi / vị trí đơn hàng, theo những gì scene có;
  - `repeat`: false. Một lần bấm khi đang „Hilfe kommt …“ không xảy ra được, vì nút đã khoá.
- Thêm `help_reset` {`reason`: `attempt` | `scene_change` | `timeout`} để R biết lúc nút mở lại.
- Gọi qua `ResearchLogger` (code scene gọi research, không ngược lại; không dùng phương thức chạy mỗi frame trong `Research/`).
- `layout_version` "041" (chung với decision 043). Spec `docs/research-logging-spec.md` cập nhật trong cùng thay đổi. Phần R gom vào mục R kế tiếp:
  - lượt làm có chấm điểm đầu tiên sau `help_requested` = "có trợ giúp";
  - tách khỏi kết quả lần đầu đúng;
  - đếm số lần gọi giúp theo scene.

## Để người làm tự quyết

- Tư thế chính xác của nút trong từng scene, và cách làm prefab (gợi ý: prefab dùng chung như `FacilitatorEndSession`).
- File âm thanh chuông.
- Cách tìm ngữ cảnh cho từng scene.
