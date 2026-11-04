# Buổi test trên kính: mọi case đang chờ

Danh sách này gom mọi case `Bạn ⏳` trong `docs/test-reports/` (ngày 2026-11-04, `main` 590f959) để làm hết trong **một buổi trên Quest**. Thứ tự đi theo bài học, nên mỗi scene chỉ cần vào một lần. Trong ngoặc là mục backlog và mã case.

Cần: Quest 3, máy tính có MQDH/`adb` (xem `docs/research-data-guide.md` bước 0), khoảng 60–90 phút. Ghi ✅ / ❌ vào từng ô. Với ❌, ghi một câu là thấy gì; nếu được thì chụp ảnh trên kính.

Trong cả buổi, để ý độ mượt theo decision 022: giật khi cầm/thả, khi bấm nút đỏ, khi thùng chạy hay khi đổi scene thì ghi lại đúng lúc nào.

---

## 0. Build và cài app

- [ ] Build app lên Quest. Ghi thời gian build: ____ phút (#11 L9)
- [ ] Mở app: WelcomeScene hiện bàn phím nhập mã, không tự chạy phiên. Frame đầu không nhấp nháy (#11 L9)

## 1. Lượt 1 – mã thử `P901` (Menu mở hết)

### WelcomeScene
- [ ] Trước khi nhập mã: chạm `English` → scene tải lại, chữ tiếng Anh. Chạm `Deutsch` → chữ Đức lại (#11 L10)
- [ ] Nhập `P901`, bắt đầu
- [ ] Cầm thùng bằng tay thật → ✓ hiện ở thẻ 1. Chạm nút đỏ bằng ngón tay → nghe tiếng bấm, ✓ ở thẻ 2, mục „2. Grundlagen“ trong Menu sáng xanh (#39 H1, #28 T1)
- [ ] Nút đỏ ở cùng chỗ như các bài sau, bấm được bằng tay (#69 R5)
- [ ] Đọc Menu từ chỗ đứng, chạm một mục để sang bài (#64 F6)

### ConceptScene
- [ ] Đọc nhãn carton và túi từ chỗ đứng, không cúi (#36 H1)
- [ ] Cầm carton lên rồi đặt xuống, tay cầm tự nhiên (#36 H2)
- [ ] Cầm túi SCHRAUBE B (ngay dưới bảng câu hỏi) mà không chạm nhầm A/B/C (#36 H3)
- [ ] Tay phải duỗi thoải mái chạm được „Weiter“ và từng đáp án. Bấm „Weiter“ 10 lần, lần nào cũng trúng (#52 H1)
- [ ] Hover/nhấn nút thì thấy đổi màu, có tiếng (#52 H2)
- [ ] Cầm lần lượt 3 kiện rồi đặt lại, không bấm nhầm „Weiter“, đáp án hay Menu (#52 H3)
- [ ] Đi hết tour. Đọc bảng EKZ (bước 9) và bảng 10 bước (bước 11) từ chỗ đứng: đọc được mọi dòng, không phải quay người quá nhiều (#57 E8b, #73 M1)
- [ ] Sau bước 11, poke A/B/C thật: chấm ngay, nút không bị kẹt ở dáng đang chọn (#73 M3)
- [ ] Nếu có người chưa biết app: để họ tự trả lời 4 câu, xem họ có chạm nhầm không (#73 M2)

### BasicPickingScene
- [ ] Nhìn bàn sau và màn hình từ chỗ đứng: thấy hợp lý (#51 H1)
- [ ] Đọc nhãn carton từ chỗ đứng (#36 H1)
- [ ] Cầm carton đầu tiên → thẻ 2 có khung xanh. Liếc lại giá thẻ thấy rõ bước hiện tại (#40 H1, H3)
- [ ] Đặt carton vào thùng đơn rồi lấy ra → Ist +50 rồi −50, khung thẻ đổi đúng. Carton khớp vào ô (#36 H2, #40 H2, #56 H2, #29 H1)
- [ ] Đặt carton thứ 6 vào ô cuối, không quá 2 lần thử (#56 H1)
- [ ] Bấm nút đỏ khi còn sai một lần, rồi khi đã đúng. Đọc thẻ kết quả mỗi lần (#59 H1)
- [ ] Đọc ba thẻ trên giá (#62 T10)
- [ ] Bấm đúng **lúc đang cầm một carton ở tay kia** → carton ở lại trên tay (#72 T6)
- [ ] Xem thùng kho rồi thùng đơn chạy đi: không vật nào xuyên bàn sau, màn hình hay cột giá thẻ, cả hai thùng đều với tới được (#72 T7)

### MixedPickingScene
- [ ] Cầm carton (#40 H1). Đọc thẻ trên giá ở position 1 (#62 T10)
- [ ] Bấm nút đỏ một lần → sau buổi, log có đúng một `confirm_pressed` cho lần đó (#28 T2)
- [ ] Position 1 đúng: đọc thẻ kết quả và dialog. Thùng kho đổi: không giật, không vật nào chạy về phía mặt, mái che che được thùng mới (#71 H1, H3)
- [ ] Đang cầm một kiện lúc thùng kho đổi → kiện ở lại trên tay (#71 T5, H2)
- [ ] Position 2: cầm túi mới `PlasticBag_GG_4` bằng tay, bỏ vào thùng đơn rồi lấy ra (#67 H1). Đọc thẻ kết quả, dialog, thẻ trên giá (#71 H1, #62 T10)
- [ ] Xong đơn: xem cả chuyến thùng đơn chạy đi. Đang cầm một kiện lúc thùng chạy → kiện ở lại trên tay (#71 H1, H2)
- [ ] Chọn bài 5 trong Menu (#71 H4 bắt đầu ở đây)

### ComplexScene
- [ ] Đọc nhãn, nhất là carton SCHLAUCHSCHELLE B ở position 2 (#36 H1)
- [ ] Bảng câu hỏi và bảng ví dụ không che dialog, giá thẻ, đường tay; đủ tầm với (#19 C14)
- [ ] Đọc các thẻ ở cả 2 position: không chồng lên nhau, thẻ 3 không thò ra khỏi tấm gỗ (#53 T4, #62 T10)
- [ ] Lấy từng vòng RING D bằng tay: vòng không co nhỏ, mỗi vòng Ist +1 / −1. Bỏ một vòng vào thùng đơn rồi lấy ra (#19 C6, C7, B1)
- [ ] Bấm sai một lần, đúng một lần. Đọc thẻ kết quả (#59 H1)
- [ ] Thùng kho 2 chạy vào bình thường, không nhảy (vì vào từ Menu ở Mixed) (#71 H4)
- [ ] Làm hết đơn → màn hình chúc mừng, hiện `Code: P901`

## 2. Lượt 2 – mã nghiên cứu `P899` (Menu khoá)

`P899` là mã thật, nên **xoá file của nó sau buổi** (mục 3).

- [ ] Vào app lại, nhập `P899`. Trong Welcome, chạm mục Menu mờ → không có gì xảy ra (#42 H1, #29 H2)
- [ ] Làm xong Welcome → mục kế sáng lên, chạm vào → sang Concept (#42 H1)
- [ ] Chọn tiếp tới Mixed. Giữa bài, tắt hẳn app (giả crash)
- [ ] Mở lại, nhập `P899`, chọn `Fortsetzen` → vào Mixed từ đầu, Menu khoá cho tới khi xong bài (#42 H2, K4)
- [ ] Kết thúc bằng cách giữ đĩa tối 3 s → `Beenden`

## 3. Sau buổi – trên máy tính

- [ ] Theo `docs/research-data-guide.md`, chép log 2 phiên thử về máy, kiểm đủ file. Ghi lại chỗ nào khó hiểu (#31 M1)
- [ ] Xoá trên kính các file của `P899` (`research/`) và của `P901` (`research/practice/`)
- [ ] Gửi Claude thư mục log đã chép. Claude đọc để kiểm #28 T2 (một `confirm_pressed`), #29 H1 (`grab_*`, `container_enter/exit` từ tay thật) và #29 H2 (`menu_select` `blocked: true`)
- [ ] Nhập 2 phiếu thử vào `analysis/paper_forms.csv` bằng Excel. Mã thử bị loại khỏi phân tích, nên nhờ Claude đổi mã khi test, hoặc làm với log thật đầu tiên (#31 M2)

Gửi lại danh sách này đã đánh dấu. Claude sẽ cập nhật các test report và backlog: case nào ✅ hết thì mục đó thành `done`.
