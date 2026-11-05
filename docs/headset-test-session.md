# Buổi test trên kính: pilot EN và mọi case đang chờ

Danh sách này gom mọi case `Bạn ⏳` trong `docs/test-reports/` (cập nhật 2026-11-05, sau #97) để làm trong **một buổi trên Quest**, kèm phiên pilot tiếng Anh của bạn (decision 023). Trong ngoặc là mục backlog và mã case.

Buổi test gồm ba lượt, mỗi lượt có một mục đích riêng:

| Lượt | Mã | Ngôn ngữ | Mục đích | Dữ liệu |
|---|---|---|---|---|
| 1 | `P801` (mã pilot) | English | học như một người học thật, từ đầu đến cuối | **được phân tích** (pilot) |
| 2 | `P901` (mã thử) | Deutsch | kiểm luồng, chữ tiếng Đức có chồng/tràn không, các thao tác cố ý | bị loại |
| 3 | `P899` (mã pilot, chỉ để thử) | Deutsch | crash rồi học tiếp, cảnh báo „đã có phiên kết thúc“ | **xoá sau buổi** |

Thao tác cố ý (bấm sai, cầm kiện trong lúc thùng chạy, bỏ thừa carton, …) chỉ làm ở lượt 2, để lượt 1 giữ đúng như một người học thật. Thứ 6, bạn của bạn dùng `P802` với English, làm như lượt 1.

Cần: Quest 3, máy tính có MQDH/`adb` (xem `docs/research-data-guide.md` bước 0), khoảng 120 phút. Ghi ✅ / ❌ vào từng ô. Với ❌, ghi một câu là thấy gì; nếu được thì chụp ảnh trên kính.

Cả ba lượt đều để ý độ mượt theo decision 022: giật khi cầm/thả, khi bấm nút đỏ, khi thùng chạy hay khi đổi scene thì ghi lại đúng lúc nào.

**Đã bỏ so với bản 2026-11-04:** từ #92, ConceptScene chỉ còn carton STECKER C (không còn túi SCHRAUBE B, carton KABELVERBINDER A hay vòng RING D để cầm), và bảng 10 bước không còn hiện ở bước 11. Các case cũ nói về những vật đó (#36 H3, #52 H3 „3 kiện“, #89 H3 „4 loại kiện“, #100 M1 „kiện sai loại“, #100 M2 „carton RING D mẫu“) được viết lại cho STECKER C bên dưới.

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
- [ ] Welcome: đọc bảng mục tiêu (hai cụm, mỗi gạch đầu dòng một dòng, chữ không nghiêng) (#86 W86-H, EN). Bảng không che Menu, giá thẻ, thùng (#86 W86-5)
- [ ] Welcome: cầm thùng bằng tay thật → ✓ ở thẻ 1. Chạm nút đỏ bằng ngón tay → nghe tiếng bấm, ✓ ở thẻ 2, mục bài 2 trong Menu sáng xanh (#39 H1, #28 T1). Nút đỏ bấm được bằng tay (#69 R5)
- [ ] Chạm thử một mục Menu mờ trước khi xong bài → không có gì xảy ra. Xong bài → mục kế sáng, chạm vào để sang bài (#42 H1, #29 H2, #64 F6)
- [ ] Ở mỗi bài, đọc thẻ trên giá từ chỗ đứng (thẻ kem „What this is about“ có bị lóa trên gỗ không) (#87 H1, EN)
- [ ] Học hết Concept, Basic, Mixed, Complex theo hướng dẫn trên thẻ, tới màn hình chúc mừng có `Code: P801`
- [ ] Sau mỗi lần bấm nút đỏ đúng: có đọc kịp thẻ kết quả trước khi thùng chạy không, 4 s có vừa không, mắt có tự nhìn theo thùng không (#99 H1)
- [ ] Trong lúc học, ghi những chỗ bạn thấy khó hiểu, khó với tới hay khó đọc (để làm round 3)
- [ ] Điền SSQ sau và NASA-TLX

## 2. Lượt 2 – mã thử `P901`, Deutsch

Menu mở hết, nên đi thẳng tới scene cần thử. Ở mỗi scene, chú ý chữ tiếng Đức: không chồng lên nhau, không tràn ra khỏi thẻ hay bảng, không bị cắt.

- [ ] Trước khi gõ mã: chọn `Deutsch`. Gõ `P901` → có nhãn „Testmodus“

### WelcomeScene và Menu
- [ ] Đọc bảng mục tiêu bằng tiếng Đức, mỗi gạch đầu dòng một dòng (#86 W86-H, DE)
- [ ] Đọc Menu từ chỗ đứng: tên bài in đậm có „» “ nằm trên một dòng (#64 F6)
- [ ] Đọc các thẻ trên giá (#87 H1, DE)

### ConceptScene
- [ ] Đọc nhãn trên nắp carton STECKER C trong thùng kho từ chỗ đứng, không cúi (#36 H1, #88 H2)
- [ ] Lấy một carton từ thùng kho → ✓ ở thẻ 1, thanh tour hiện dưới màn hình, khung xanh sang thẻ 2 (#89 H3, #92 H1)
- [ ] Cầm carton ở 30–40 cm, xoay: thẻ Mat.-Nr/VPE cạnh carton đọc được và không che nhãn (#90 H1). Cầm 2 carton bằng 2 tay → 2 thẻ không chồng nhau (#90 H2). Tay cầm tự nhiên khi cầm, thả, cầm lại (#36 H2, #88 H1)
- [ ] Thanh tour: bấm được cả 3 nút mà không bước tới (#89 H1). Bấm „Weiter“ 10 lần, lần nào cũng trúng ngay (#52 H1). Hover/nhấn thì đổi màu, có tiếng (#52 H2). Đọc chữ trên thanh mà không cúi người (#89 H2)
- [ ] Bước `material`: nhận ra phải cầm STECKER C (dòng có bàn tay + mũi tên cam nảy). Cầm lên → dòng đổi sang „so sánh nhãn với LVS“ (#90 H3)
- [ ] Bước `cartons`: bỏ 3 carton vào thùng đơn, nhìn các số 40 / 80 / 120 trên carton và phép chia cạnh màn hình (#91, chỉ nhìn lại)
- [ ] Bỏ carton thứ 4 vào thùng đơn → thẻ vàng báo thừa hiện ngay trước carton, đọc được từ chỗ đứng (~1 m), không bị thanh tour hay Menu che. Lấy carton ra → thẻ mất. Cầm carton đi qua thùng mà không thả → không có thẻ (#100 M1, M2, viết lại cho STECKER C)
- [ ] Bước 8–9: đứng ở chỗ làm việc, đọc chú thích Empf Kz, thấy hai thùng mẫu trên bàn sau mà không phải quay người (#94 H2). Đọc 3 nắp carton và hai thẻ mẫu, nhận ra cùng hình với bảng EKZ (#101 H1). Đọc ký hiệu bàn tay gạch chéo + dòng lý do (#92 H2). Đọc mọi dòng của bảng EKZ (#57 E8b)
- [ ] Bước 5 (Lagerbehälter): ghi thời gian bạn ở bước này: ____ s (#92 H2)
- [ ] Bước 11 với carton thừa trong thùng đơn → thùng chưa chạy; lấy carton thừa ra → thùng kho rẽ vào làn giữa, thùng đơn chạy đi. Đọc kịp dòng trên thanh trong 4 s, thấy cả chuyến mà không phải nghiêng đầu (#89 H4, #100 M3). Bấm „Von vorn“ → carton về thùng kho, cầm lại được (#100 M3)
- [ ] Chạm „Zu den Fragen“: cụm câu hỏi ở giữa, đọc được, không che dialog. Chạm „Zurück“ của cụm → thanh tour quay lại (#93 H1)
- [ ] Trả lời 4 câu bằng ngón tay. Trong lúc đó cầm một carton rồi đặt lại, không chạm nhầm A/B/C, „Weiter“ hay Menu (#52 H3, #36 H3, viết lại cho STECKER C)

### BasicPickingScene
- [ ] Nhìn bàn sau, màn hình và băng chuyền từ chỗ đứng: thấy hợp lý, không có ray thừa (#51 H1, #95)
- [ ] Đọc nhãn carton từ chỗ đứng (#36 H1)
- [ ] Cầm carton đầu tiên → ✓ ở thẻ 1, thẻ 2 có khung xanh. Liếc lại giá thẻ thấy rõ bước hiện tại (#40 H1, H3, #97 H1)
- [ ] Đặt carton vào thùng đơn rồi lấy ra → Ist +50 rồi −50, khung thẻ đổi đúng. Carton khớp vào ô (#36 H2, #40 H2, #56 H2, #29 H1, #88 H1)
- [ ] Đặt carton thứ 6 vào ô cuối, không quá 2 lần thử (#56 H1)
- [ ] Bấm nút đỏ khi còn sai một lần (thẻ 2, 3 không có ✓), rồi khi đã đúng (✓ ở cả 3 thẻ). Đọc thẻ kết quả mỗi lần (#59 H1, #97 H1, H2)
- [ ] Đọc ba thẻ trên giá (#62 T10)
- [ ] Trong 4 s chờ sau khi bấm đúng: thử cầm một carton trong thùng kho (không cầm được) (#99 H3). Mũi tên cam hiện trên làn giữa (#99 H1)
- [ ] Bấm đúng **lúc đang cầm một carton ở tay kia** → carton ở lại trên tay (#72 T6)
- [ ] Xem thùng kho rồi thùng đơn chạy đi: không vật nào xuyên bàn sau, màn hình hay cột giá thẻ, cả hai thùng đều với tới được (#72 T7)

### MixedPickingScene
- [ ] Cầm carton (#40 H1). Đọc thẻ trên giá và dòng „Position 1 · Position 2“ ở position 1 (#62 T10, #97 H2)
- [ ] Position 1 (Empf Kz 003): xếp 3 carton vào ô bằng tay thật, thả gần ô → khớp, có tiếng (#94 H1)
- [ ] Bấm nút đỏ một lần → sau buổi, log có đúng một `confirm_pressed` cho lần đó (#28 T2)
- [ ] Position 1 đúng: ✓ ở 3 thẻ (#97 H1). Đọc thẻ kết quả và dialog. Trong 4 s chờ: carton trong thùng kho không cầm được, carton trên bàn cầm được (#99 H3). Thùng kho đổi: không giật, không vật nào chạy về phía mặt, mái che che được thùng mới (#71 H1, H3)
- [ ] Đang cầm một kiện lúc thùng kho đổi → kiện ở lại trên tay (#71 T5, H2)
- [ ] Position 2: đổ 3 túi vào nửa phải thùng đơn bằng tay (#94 H1). Cầm túi `PlasticBag_GG_4`, bỏ vào thùng đơn rồi lấy ra (#67 H1, #88 H1). Đọc thẻ kết quả, dialog, thẻ trên giá, dòng position có ✓ ở Position 1 (#71 H1, #62 T10, #97 H2)
- [ ] Xong đơn: xem cả chuyến thùng đơn chạy đi. Đang cầm một kiện lúc thùng chạy → kiện ở lại trên tay (#71 H1, H2)
- [ ] Chọn bài 5 trong Menu (#71 H4 bắt đầu ở đây)

### ComplexScene
- [ ] Đọc nhãn, nhất là carton SCHLAUCHSCHELLE B ở position 2 (#36 H1)
- [ ] Bảng câu hỏi và bảng ví dụ không che dialog, giá thẻ, đường tay; đủ tầm với (#19 C14)
- [ ] Trả lời 2 câu; cụm thu lại, nhặt hàng position 1 mà không bị che, đọc thẻ 2 „2 Kartons + 5 Stück“ (#93 H2)
- [ ] Đọc ký hiệu bàn tay gạch chéo trên carton đã mở và dòng lý do trên thùng kho, ở cả 2 position (#92 H2)
- [ ] Đọc các thẻ ở cả 2 position: không chồng lên nhau, thẻ 3 không thò ra khỏi tấm gỗ (#53 T4, #62 T10)
- [ ] Lấy từng vòng RING D bằng tay: vòng không co nhỏ, mỗi vòng Ist +1 / −1. Bỏ một vòng vào thùng đơn rồi lấy ra (#19 C6, C7, B1, #88 H1)
- [ ] Bấm sai một lần, đúng một lần. Đọc thẻ kết quả, ✓ ở 3 thẻ khi đúng (#59 H1, #97 H1)
- [ ] Thùng kho 2 chạy vào bình thường, không nhảy (vì vào từ Menu ở Mixed) (#71 H4)
- [ ] Làm hết đơn → màn hình chúc mừng hiện **sau khi** thùng đơn đã chạy đi, không che làn giữa (#99 H2). Trong lúc chờ, chạm vào chỗ panel sẽ hiện → không có gì xảy ra (#99 T9b). Chữ tổng kết tiếng Đức không tràn (#86 X7)

## 3. Lượt 3 – `P899`, Deutsch (crash và học tiếp)

- [ ] Gõ `P899`, đi tới Mixed theo khoá Menu. Giữa bài, tắt hẳn app (giả crash)
- [ ] Mở lại, gõ `P899`, chọn `Fortsetzen` → vào Mixed từ đầu, Menu khoá cho tới khi xong bài (#42 H2, K4)
- [ ] Kết thúc bằng cách giữ đĩa tối 3 s → `Beenden`
- [ ] Gõ `P899` lần nữa → hiện cảnh báo „bereits eine abgeschlossene Sitzung“ → `Korrigieren`

## 4. Sau buổi – trên máy tính

- [ ] Theo `docs/research-data-guide.md`, chép log về máy, kiểm đủ file: `P801` trong `research/pilot/`, `P901` trong `research/practice/`. Ghi lại chỗ nào khó hiểu (#31 M1)
- [ ] Xoá các file của `P899` (trong `research/pilot/`) **ở cả máy tính lẫn kính**, để chúng không vào kết quả pilot. Sau đó xoá log trên kính
- [ ] Nhập phiếu của `P801` vào `paper_forms.csv` bằng Excel (#31 M2)
- [ ] Gửi Claude thư mục log đã chép. Claude kiểm từ log #28 T2, #29 H1, #29 H2 (đọc file, không cần R)
- [ ] Chạy `analyse.R` để có biểu đồ pilot (#75 P1): **chỉ được sau #102**, vì Windows đang chặn `tidyr`/`readr`. Giữ log `P901` của Concept cho bước 4 của #102

Gửi lại danh sách này đã đánh dấu. Claude sẽ cập nhật các test report và backlog: case nào ✅ hết thì mục đó thành `done`.
