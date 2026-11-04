# Buổi test trên kính: pilot EN và mọi case đang chờ

Danh sách này gom mọi case `Bạn ⏳` trong `docs/test-reports/` (2026-11-04) để làm trong **một buổi trên Quest**, kèm phiên pilot tiếng Anh của bạn (decision 023). Trong ngoặc là mục backlog và mã case.

Buổi test gồm ba lượt, mỗi lượt có một mục đích riêng:

| Lượt | Mã | Ngôn ngữ | Mục đích | Dữ liệu |
|---|---|---|---|---|
| 1 | `P801` (mã pilot) | English | học như một người học thật, từ đầu đến cuối | **được phân tích** (pilot) |
| 2 | `P901` (mã thử) | Deutsch | kiểm luồng, chữ tiếng Đức có chồng/tràn không, các thao tác cố ý | bị loại |
| 3 | `P899` (mã pilot, chỉ để thử) | Deutsch | crash rồi học tiếp, cảnh báo „đã có phiên kết thúc“ | **xoá sau buổi** |

Thao tác cố ý (bấm sai, cầm kiện trong lúc thùng chạy, …) chỉ làm ở lượt 2, để lượt 1 giữ đúng như một người học thật. Thứ 6, bạn của bạn dùng `P802` với English, làm như lượt 1.

Cần: Quest 3, máy tính có MQDH/`adb` (xem `docs/research-data-guide.md` bước 0), khoảng 90–120 phút. Ghi ✅ / ❌ vào từng ô. Với ❌, ghi một câu là thấy gì; nếu được thì chụp ảnh trên kính.

Cả ba lượt đều để ý độ mượt theo decision 022: giật khi cầm/thả, khi bấm nút đỏ, khi thùng chạy hay khi đổi scene thì ghi lại đúng lúc nào.

---

## 0. Build và cài app

- [ ] Build app lên Quest. Ghi thời gian build: ____ phút (#11 L9)
- [ ] Mở app: WelcomeScene hiện bàn phím nhập mã, không tự chạy phiên. Frame đầu không nhấp nháy (#11 L9)
- [ ] Xoá log cũ trên kính (`research-data-guide.md` mục 1, bước 1)
- [ ] Điền SSQ trước cho chính bạn (pilot dùng phiếu như người thật)

## 1. Lượt 1 – pilot `P801`, English

Làm như một người học thật, không cố ý thử lỗi.

- [ ] Trước khi gõ mã: chạm `English` → màn hình tải lại, chữ tiếng Anh. Chạm `Deutsch` → chữ Đức lại, rồi `English` lần nữa (#11 L10)
- [ ] Gõ `P801`, xác nhận. App vẫn ở tiếng Anh, không có nhãn „Testmodus“ (#75)
- [ ] Welcome: cầm thùng bằng tay thật → ✓ ở thẻ 1. Chạm nút đỏ bằng ngón tay → nghe tiếng bấm, ✓ ở thẻ 2, mục bài 2 trong Menu sáng xanh (#39 H1, #28 T1). Nút đỏ bấm được bằng tay (#69 R5)
- [ ] Chạm thử một mục Menu mờ trước khi xong bài → không có gì xảy ra. Xong bài → mục kế sáng, chạm vào để sang bài (#42 H1, #29 H2, #64 F6)
- [ ] Học hết Concept, Basic, Mixed, Complex theo hướng dẫn trên thẻ, tới màn hình chúc mừng có `Code: P801`
- [ ] Trong lúc học, ghi những chỗ bạn thấy khó hiểu, khó với tới hay khó đọc (để làm round 3)
- [ ] Điền SSQ sau và NASA-TLX

## 2. Lượt 2 – mã thử `P901`, Deutsch

Menu mở hết, nên đi thẳng tới scene cần thử. Ở mỗi scene, chú ý chữ tiếng Đức: không chồng lên nhau, không tràn ra khỏi thẻ hay bảng, không bị cắt.

- [ ] Trước khi gõ mã: chọn `Deutsch`. Gõ `P901` → có nhãn „Testmodus“

### WelcomeScene và Menu
- [ ] Đọc Menu từ chỗ đứng: tên bài in đậm có „» “ nằm trên một dòng (#64 F6)

### ConceptScene
- [ ] Đọc nhãn carton và túi từ chỗ đứng, không cúi (#36 H1)
- [ ] Cầm carton lên rồi đặt xuống, tay cầm tự nhiên (#36 H2)
- [ ] Cầm túi SCHRAUBE B (ngay dưới bảng câu hỏi) mà không chạm nhầm A/B/C (#36 H3)
- [ ] Tay phải duỗi thoải mái chạm được „Weiter“ và từng đáp án. Bấm „Weiter“ 10 lần, lần nào cũng trúng (#52 H1)
- [ ] Hover/nhấn nút thì thấy đổi màu, có tiếng (#52 H2)
- [ ] Cầm lần lượt 3 kiện rồi đặt lại, không bấm nhầm „Weiter“, đáp án hay Menu (#52 H3)
- [ ] Đi hết tour. Đọc bảng EKZ (bước 9) và bảng 10 bước (bước 11) từ chỗ đứng: đọc được mọi dòng, không phải quay người quá nhiều (#57 E8b, #73 M1)
- [ ] Sau bước 11, poke A/B/C thật: chấm ngay, nút không bị kẹt ở dáng đang chọn (#73 M3, #73 M2)

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
- [ ] Làm hết đơn → màn hình chúc mừng, chữ tổng kết tiếng Đức không tràn

## 3. Lượt 3 – `P899`, Deutsch (crash và học tiếp)

- [ ] Gõ `P899`, đi tới Mixed theo khoá Menu. Giữa bài, tắt hẳn app (giả crash)
- [ ] Mở lại, gõ `P899`, chọn `Fortsetzen` → vào Mixed từ đầu, Menu khoá cho tới khi xong bài (#42 H2, K4)
- [ ] Kết thúc bằng cách giữ đĩa tối 3 s → `Beenden`
- [ ] Gõ `P899` lần nữa → hiện cảnh báo „bereits eine abgeschlossene Sitzung“ → `Korrigieren`

## 4. Sau buổi – trên máy tính

- [ ] Theo `docs/research-data-guide.md`, chép log về máy, kiểm đủ file: `P801` trong `research/pilot/`, `P901` trong `research/practice/`. Ghi lại chỗ nào khó hiểu (#31 M1)
- [ ] Xoá các file của `P899` (trong `research/pilot/`) **ở cả máy tính lẫn kính**, để chúng không vào kết quả pilot. Sau đó xoá log trên kính
- [ ] Nhập phiếu của `P801` vào `paper_forms.csv` bằng Excel (#31 M2)
- [ ] Gửi Claude thư mục log đã chép. Claude chạy `analyse.R` và gửi bạn biểu đồ pilot (#75), đồng thời kiểm từ log #28 T2, #29 H1, #29 H2

Gửi lại danh sách này đã đánh dấu. Claude sẽ cập nhật các test report và backlog: case nào ✅ hết thì mục đó thành `done`.
