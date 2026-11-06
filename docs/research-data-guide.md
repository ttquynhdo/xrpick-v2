# Hướng dẫn thu dữ liệu nghiên cứu (cho người điều phối)

Tài liệu này dành cho người điều phối buổi học VR và người nhập phiếu. Làm theo từng bước, không cần biết lập trình. Chữ trong app là tiếng Đức, nên các nút được ghi đúng như trên kính, kèm nghĩa trong ngoặc.

Không cài thêm gì lên Quest (decision 022). Trên máy tính chỉ cần công cụ chép file (bước 0).

---

## 0. Chuẩn bị một lần

**Trên máy tính:**
1. Cài **Meta Quest Developer Hub (MQDH)** cho Windows hoặc macOS từ trang của Meta (cài trên máy tính, không phải trên kính). MQDH có sẵn công cụ `adb` để chép file. Nếu cửa sổ lệnh báo không tìm thấy `adb`, cài thêm *Android SDK Platform-Tools* của Google (một thư mục giải nén, cũng chỉ trên máy tính) và chạy lệnh trong thư mục đó.
2. Có thư mục dự án `xrpick-v2` (để chạy phân tích R về sau, xem `analysis/README.md`).

**Trên kính:**
- Kính đã ở chế độ nhà phát triển (Developer Mode) và đã cài app `xpick`, như khi test.
- Kiểm tra một lần: cắm kính vào máy bằng cáp USB-C, đeo kính và chọn **Allow** (Cho phép) khi kính hỏi cho phép gỡ lỗi USB. Trong MQDH, kính phải hiện trong *Device Manager*.

**Giấy tờ:**
- Mỗi người học có một bộ phiếu đã in sẵn **mã người học**: `P001`–`P799` cho đợt nghiên cứu thật, `P800`–`P899` cho pilot (xem dưới). Bộ phiếu gồm: phiếu đồng ý, SSQ trước, SSQ sau, NASA-TLX, ô *Abbruchgrund* (lý do dừng).
- Danh sách tên ↔ mã chỉ nằm trên giấy, ở trưởng nhóm nghiên cứu. Không viết tên lên phiếu khảo sát và không gõ tên vào máy.
- **Mã pilot** (`P800`–`P899`, decision 023): chạy giống hệt người học thật (khoá Menu, học tiếp, cảnh báo „đã có phiên kết thúc“), nhưng **giữ ngôn ngữ đã chọn** (English hoặc Deutsch). Log nằm trong thư mục con `pilot/` và được phân tích riêng, không bao giờ trộn với đợt thật.
- Mã thử (`P000`, `P900`–`P999`) chỉ dùng để thử máy: app hiện „Testmodus – wird nicht ausgewertet“ (chế độ thử, không phân tích), và phân tích tự loại chúng.

## 1. Trước mỗi đợt thu (một lần cho cả đợt)

1. **Xoá log cũ trên kính**, kể cả log thử của nhóm phát triển. Cắm kính, mở cửa sổ lệnh và chạy:

   ```bash
   adb shell rm -r /sdcard/Android/data/com.xrise.xpick/files/research
   ```

   (Nếu báo „No such file or directory“ thì đã sạch rồi.)
2. **Trên máy tính:** tạo một thư mục mới cho đợt này, ví dụ `analysis/data/wave1/`. Không để log thử hay log của đợt khác lẫn vào.
3. **Thử máy một lần** bằng mã thử `P901`: nhập mã, làm hết bài 1, rồi kết thúc (bước 4). Trong lúc làm, để ý cầm thùng hay bấm nút có bị giật hình không (log không ghi độ mượt, decision 022). Nếu thấy giật thì báo nhóm phát triển trước khi thu thật.
4. Kiểm tra **ngày giờ trên Quest** đúng giờ địa phương. Phân tích so giờ kết thúc trên phiếu với giờ trong log.
5. Sạc đầy kính.

## 2. Trước khi đeo kính (mỗi người học)

1. Người học ký phiếu đồng ý.
2. Người học điền **SSQ trước** (cảm giác trước khi đeo kính).
3. Người điều phối ghi ngày và **mã người điều phối** (ví dụ `F1`, không ghi tên) lên phiếu.

## 3. Bắt đầu phiên trên kính

1. Mở app `xpick`. Màn hình đầu (WelcomeScene) hiện bàn phím „Teilnehmercode eingeben“ (Nhập mã người tham gia).
2. Ngôn ngữ:
   - Mã thật (`P001`–`P799`): app luôn chạy tiếng Đức, không cần chọn. Chọn English rồi gõ mã thật thì app tự đổi về tiếng Đức.
   - Mã pilot (`P800`–`P899`): **chọn ngôn ngữ trước khi gõ mã**, bằng nút `Deutsch` / `English` dưới bàn phím. Chạm nút sẽ tải lại màn hình. Sau khi phiên bắt đầu thì không đổi được nữa.
3. Gõ đúng mã trên phiếu, ví dụ `P017`, rồi chạm **OK**.
4. App hỏi „Ist der Code P017 richtig?“ (Mã P017 đúng không?):
   - **Ja** (Có): bắt đầu khoá học.
   - **Korrigieren** (Sửa): quay lại bàn phím.
5. Các trường hợp đặc biệt:
   - „Kurs von P017 fortsetzen?“ (Học tiếp khoá của P017?): phiên trước của mã này bị ngắt giữa chừng (kính tắt, app đóng). Chọn **Fortsetzen** (Học tiếp) để vào lại bài bị ngắt, hoặc **Neu beginnen** (Bắt đầu lại) để học từ bài 1. Ghi lựa chọn lên phiếu.
   - „Für P017 gibt es bereits eine abgeschlossene Sitzung.“ (Mã P017 đã có một phiên kết thúc): thường là gõ nhầm mã. Chọn **Korrigieren** và kiểm tra lại mã. Chỉ chọn **Trotzdem starten** (Vẫn bắt đầu) khi chắc chắn đúng người, và ghi lý do lên phiếu.
   - „Die Sitzung konnte nicht gestartet werden …“ (Không bắt đầu được phiên): đóng app hẳn rồi mở lại.
6. Đưa kính cho người học. Trong khi học, không chạm vào kính hay Menu thay người học.

## 4. Kết thúc phiên

**Người học học xong:** ở bài 5, màn hình chúc mừng hiện trước màn hình WMS, kèm dòng `Code: P017`.
1. Đối chiếu mã đó với mã trên phiếu.
2. Ghi **giờ kết thúc** lên phiếu, theo giờ địa phương, dạng `HH:MM`.
3. Phiên tự kết thúc. Không cần bấm gì thêm.

**Cần dừng giữa chừng** (mệt, chóng mặt, hết giờ…):
1. Người điều phối (hoặc người học theo hướng dẫn) giữ tay **3 giây** trên đĩa tối nhỏ ở bên tay trái người học, cao khoảng ngang vai. Chạm nhanh thì không có gì xảy ra.
2. Panel „Sitzung wirklich beenden?“ (Thật sự kết thúc phiên?) hiện ra:
   - **Beenden** (Kết thúc): phiên kết thúc, app về màn hình nhập mã.
   - **Fortsetzen** (Học tiếp): đóng panel, học tiếp.
3. Ghi giờ kết thúc và đánh dấu **một** ô **Abbruchgrund** (lý do dừng): müde/schwindlig (mệt/chóng mặt), Zeit (hết giờ), Fehlbedienung (chạm nhầm), Andere (khác, ghi rõ trong ô ghi chú).

**Kính tắt hoặc app đóng đột ngột:** không sao, log đã ghi tới lúc đó. Khi nhập lại cùng mã, app sẽ hỏi có học tiếp không (bước 3.5). Ghi việc này vào ô ghi chú; không chọn một Abbruchgrund không đúng.

## 5. Sau khi tháo kính

1. Người học điền **SSQ sau** ngay sau khi tháo kính, **kể cả khi dừng giữa chừng**. Đây là dữ liệu quan trọng nhất về cảm giác khó chịu. Sau đó điền **NASA-TLX** nếu người học đồng ý.
2. Kiểm tra bộ phiếu đủ trang và có đúng một mã.

## 6. Chép log về máy (cuối mỗi buổi, hoặc sau mỗi người)

1. Cắm kính vào máy tính bằng cáp USB-C. Đeo kính và chọn **Allow** nếu kính hỏi.
2. Mở cửa sổ lệnh trong thư mục của đợt (ví dụ `analysis/data/wave1/`) và chạy:

   ```bash
   adb pull /sdcard/Android/data/com.xrise.xpick/files/research .
   ```

   Lệnh này chép cả thư mục `research/` (cùng thư mục con `pilot/` của mã pilot và `practice/` của mã thử) về máy. Nếu MQDH của bạn có trình quản lý file cho kính, có thể kéo thư mục đó về thay cho lệnh trên (tên menu có thể khác tuỳ phiên bản MQDH). Lệnh `adb pull` luôn dùng được.
3. **Kiểm tra trước khi xoá:**
   - Trong `research/` (với mã pilot: `research/pilot/`), mỗi người học hôm nay có ít nhất một file `P017_<chuỗi dài>_events.jsonl`, kích thước khác 0 KB. Thường có kèm `P017_<chuỗi dài>_summary.csv`; sau khi kính tắt đột ngột có thể thiếu file summary, điều đó không sao. Một người có thể có nhiều cặp (ví dụ khi phiên bị ngắt rồi học tiếp).
   - Số file và kích thước trên máy tính khớp với trên kính. Xem danh sách trên kính bằng:

     ```bash
     adb shell ls -l /sdcard/Android/data/com.xrise.xpick/files/research
     ```

   - So danh sách mã với chồng phiếu của buổi: không thiếu, không thừa.
   - Nếu đã nhập phiếu (bước 7): chạy phân tích (bước 8) và đọc `warnings.txt`. Mọi cảnh báo „log for Pxxx but no paper form“, „paper form for Pxxx but no log“, „unreadable line“, „session(s) without header“, „end time … min from the last logged event“ đều phải giải thích được.
4. **Chỉ xoá khi mọi người học có log trên kính đã học xong hẳn.** Nếu có người bị ngắt giữa chừng và sẽ học tiếp sau, **chưa xoá**: chức năng học tiếp và cảnh báo „đã có phiên kết thúc“ đọc chính các file trên kính. Khi đã đủ điều kiện, **xoá log trên kính** (quy định bảo vệ dữ liệu, không để dữ liệu người học trên kính):

   ```bash
   adb shell rm -r /sdcard/Android/data/com.xrise.xpick/files/research
   ```

5. Không sửa, không đổi tên, không mở-rồi-lưu các file log bằng Excel. Phân tích đọc đúng tên file gốc.
6. Sao lưu thư mục của đợt vào nơi lưu trữ của nhóm nghiên cứu. Thư mục `analysis/data/` không bao giờ được đưa lên git.

## 7. Nhập phiếu giấy

1. Chép file mẫu `analysis/paper_forms_template.csv` thành `analysis/data/wave1/paper_forms.csv`: một file cho cả đợt, đặt **cạnh** thư mục `research/`, không đặt bên trong.
2. Mở bằng Excel. Nếu Excel tiếng Đức dồn hết vào cột A, mở bằng *Daten → Aus Text/CSV* và chọn dấu phẩy (chi tiết trong `analysis/README.md`, mục *Paper forms*).
3. Mỗi người học **một dòng**, gõ đúng từ phiếu:
   - `participant_id`: đúng như trên phiếu (`P017`, chữ P viết hoa, 3 chữ số). Sai một chữ là không nối được, chỉ có cảnh báo.
   - `session_date`, `facilitator` (mã như `F1`), `end_time_local` (`HH:MM`).
   - 16 câu SSQ trước (`ssq_pre_01_…` tới `ssq_pre_16_…`) và 16 câu SSQ sau, mỗi câu 0–3, đúng thứ tự trên phiếu.
   - 6 thang NASA-TLX (`tlx_mental` … `tlx_frustration`), 0–100. Thang „Leistung“ (`tlx_performance`) đi từ gut tới schlecht, 0 = tốt.
   - `abbruchgrund`: để trống, hoặc `mued_schwindlig`, `zeit`, `fehlbedienung`, `andere`.
   - `note`: ghi chú tự do (không ghi tên). Ghi chú không bao giờ được chép ra kết quả phân tích.
   - **Ô chưa trả lời để trống, không gõ 0.**
4. Lưu lại dạng **CSV** (dấu phẩy hay dấu chấm phẩy đều được), giữ tên file. Một file `.xlsx` sẽ không được đọc.
5. Phân tích báo trong `warnings.txt` khi có mã có phiếu mà không có log, có log mà không có phiếu, hoặc có giá trị ngoài khoảng.

## 8. Chạy phân tích

Làm theo `analysis/README.md`. Kết quả của đợt thật nằm trong `analysis/output/`, kết quả pilot trong `analysis/output/pilot/` (cùng các file và biểu đồ). Đọc `warnings.txt` trước, vì nó ghi mọi chỗ dữ liệu có vấn đề. Phiếu giấy của mã pilot nhập chung vào `paper_forms.csv`; phân tích tự đưa chúng vào kết quả pilot.

- `wide.csv`: mỗi người một dòng, chỉ dữ liệu từ log.
- `wide_with_paper.csv`: như trên, nối thêm điểm SSQ (trước, sau, thay đổi) và NASA-TLX. File này chứa **dữ liệu sức khoẻ**: chỉ lưu ở nơi lưu trữ của nhóm nghiên cứu, không gửi đi, không đưa lên git.

## Danh sách kiểm tra nhanh mỗi người học

- [ ] Phiếu đồng ý đã ký
- [ ] SSQ trước đã điền
- [ ] Nhập đúng mã trên kính, chạm **Ja**
- [ ] Ghi giờ kết thúc, hoặc ghi Abbruchgrund nếu dừng giữa chừng
- [ ] SSQ sau và NASA-TLX đã điền
- [ ] Log đã chép về máy và kiểm đủ, rồi mới xoá trên kính
- [ ] Phiếu đã nhập vào `paper_forms.csv`
