# Quyết định 004: Bàn phím nhập mã người tham gia và cách bắt đầu phiên log ở WelcomeScene

- **Backlog:** #27 (nút kết thúc phiên tách thành #44)
- **Trạng thái:** accepted (phương án A, 2026-11-01)
- **Ngày:** 2026-11-01
- **Ý kiến đã xét:** unity-reviewer, german-reviewer, data-analyst, xr-ux-reviewer, instructional-design-reviewer → decision-synthesizer
- **Lựa chọn của người dùng (2026-11-01):** chấp nhận A; nút kết thúc phiên tách thành mục riêng (#44); cảnh báo "mã đã có phiên hoàn tất" và ghi quyết định tiếp tục vào log làm luôn trong #27; chấp nhận việc mọi bản build, kể cả demo, phải gõ một mã khi mở app.

## Bài toán
Người điều phối nhập mã giả danh (`P` + 3 chữ số) trên kính, xác nhận lại, rồi khóa học mới mở và phiên log của mã đó mới bắt đầu. Không gõ nhầm, không lưu tên. Sau crash nhập lại mã cũ để học tiếp bài đang dở. Trở ngại: `ResearchSettings.autoStart` đang bật sẵn phiên `P000`, nên `StartSession(realId)` trả `false` và cả phiên người học rơi vào `practice/`.

## Xung đột chính
- Tắt `P000` tự động bằng cấu hình theo build (dễ quên) hay giữ rồi kết thúc nó (file rác, lý do kết thúc sai nghĩa, lỗi lặng lẽ).
- Bàn phím trong WelcomeScene (phải ẩn 5 đối tượng gốc) hay scene setup riêng (đổi build order, vượt phạm vi).
- Sau *Ja*: hiện Welcome ngay hay thêm màn bàn giao/Start.
- Nhãn phím bằng ký hiệu (ít chuỗi) hay chữ ngắn (font có sẵn, dễ hiểu).

## Phương án đã chọn: A
1. **Bắt đầu phiên:** logger không tự bắt đầu phiên khi app mở ở scene đầu tiên của build order; mở thẳng scene khác trong Editor thì vẫn tự bắt đầu `P000`. Điều kiện dựa vào build order, không viết tên scene thêm một lần nữa. Không có asset hay công tắc riêng cho bản build.
2. **Kiểm tra sau khi bắt đầu:** Welcome chỉ mở khi `StartSession` trả `true` và `ParticipantId` bằng mã vừa gõ; nếu không, hiện lỗi tiếng Đức cho người điều phối.
3. **Bàn phím:** chính giữa phía trước, trong tầm poke; ô `P _ _ _` to ngay trên lưới phím kiểu điện thoại; `Löschen` xoá từng chữ số; `OK` chỉ bật khi đủ 3 chữ số (không dùng `Bestätigen`, vì glossary gắn từ này với nhãn WMS `F6/F12 Bestätigen`). Trước *Ja* chỉ bàn phím hiện ra: ẩn `Menu`, `BigRedButton`, `Interactables Obj`, `Info Panel`, `Welcome`.
4. **Hỏi lại:** `Ist {participant_id} richtig?` với `Korrigieren` (trái) và `Ja` (phải), ở chỗ của bàn phím nhưng `Ja` không trùng vị trí `OK`. Mã thử (`ResearchIds.IsTestId`) có chữ "Testmodus – wird nicht ausgewertet" kèm biểu tượng, chỉ trên bàn phím và panel hỏi lại.
5. **Tiếp tục sau crash:** gọi `TryFindResumePoint` trước `StartSession`; nếu có, hỏi `Kurs von {participant_id} fortsetzen?` / `Abgebrochen in: {scene_name}` với tên bài tiếng Đức có key riêng. Tải scene qua một hàm mới **thêm** vào `SceneMenuController` (không đổi hàm cũ); tên scene lạ coi như không có điểm tiếp tục. Quyết định có/không tiếp tục được ghi vào log của phiên mới; `make_fake_data.R` và `analyse.R` sửa cùng lúc.
6. **Mã thật đã có phiên kết thúc sạch:** cảnh báo, người điều phối chọn rõ `Korrigieren` hoặc vẫn tiếp tục. Cần thêm một hàm truy vấn trong `XRPick.Research`.
7. **Nạp lại WelcomeScene khi đang có phiên:** không hiện lại bàn phím (điều kiện `IsSessionActive`).
8. **Màn cuối ComplexScene:** dòng chính là bước tiếp theo cho trainee (bỏ kính, báo người điều phối), dùng *Sie*; mã ở dòng phụ nhỏ; đọc `ParticipantId` trước mọi `EndSession`, ẩn dòng mã nếu `null`.
9. **Nút kết thúc phiên cho người điều phối:** không thuộc #27, tách thành #44 (giữ ~3 s, xác nhận lần hai, `EndSession("facilitator")`, về bàn phím).

- **Được:** mỗi người học đúng một file `research/P…` với `session_start_mode` `manual`; không có file `P000` rác; không có cấu hình build để quên; `crash_recovered` trỏ đúng sang phiên kế tiếp; phiên bắt đầu sai thì Welcome không mở.
- **Mất / chấp nhận:** mọi bản build phải gõ một mã khi mở; phải ẩn/hiện 5 đối tượng gốc (ca test "chỉ thấy bàn phím"); tên bài trên panel tiếp tục có thể chưa khớp nhãn Menu tới khi #18/#37 xong.
- **Bị bác:** cấu hình hai asset/công tắc build (dễ quên); giữ auto rồi kết thúc `P000` (file rác, lỗi lặng lẽ); cổng chặn ở mọi scene (để #42); scene setup riêng (vượt phạm vi); bấm bằng ray (trái "poke tay"); màn bàn giao/Start (lợi ích học nhỏ, cân nhắc ở #39); nhãn bằng ký hiệu (chưa rõ font có glyph).
- **Ghi chú khi làm (2026-11-01, sau review cuối #27):** chuỗi đã qua `german-reviewer` là `Unterbrochen in: {scene_name}` (key `entry.resume.where`), không phải `Abgebrochen in` như ở mục 5. WelcomeScene không bao giờ là điểm tiếp tục (crash ở WelcomeScene → không hỏi tiếp tục). Không có nút không đảo ngược nào nằm dưới vùng của `Ja`: ở panel cảnh báo `Korrigieren` thế chỗ `Ja`, `Trotzdem starten` xuống hàng dưới; ở panel tiếp tục cả hai nút ở hàng dưới. Việc bấm `Trotzdem starten` được ghi vào log (`session_start.ended_session_warning`).
- **Nếu giả định sai:** cần bản demo không gõ mã → thêm tuỳ chọn ép auto-start trong `ResearchSettings`, mặc định tắt; ẩn đối tượng hay bị lộ → chuyển sang scene setup riêng, giữ nguyên bàn phím và script. Trước #28, đề nghị tiếp tục có thể chỉ sai vào ComplexScene vì phiên chưa có `completed`: ghi chú cho người điều phối ở #31.

## Câu hỏi còn mở
- Người hướng dẫn: ngôn ngữ của người điều phối; gọi mã là `Code` hay `Teilnehmer-Code`; `Sitzung` hay `Session`; `Kurs` hay `Schulung`; tên tiếng Đức của 5 bài (chung với #18/#37). Không chặn #27: chuỗi tiếng Đức dùng *Sie*, trạng thái `proposed`.

## Bằng chứng (đã kiểm trong code)
`ResearchLogger.cs` (StartSession trả false khi đang có phiên; auto-start ở `AfterSceneLoad`; `DontDestroyOnLoad`; `EndReasons`; `ParticipantId` null khi không có phiên; `MarkCrashedSessions` trong `BeginSession`), `SessionFiles.TryFindResumePoint` (false cho cả "không có file" lẫn "kết thúc sạch"), `analysis/analyse.R` (cảnh báo mode `auto`), `ResearchIds.IsTestId`, `SceneMenuController` (chỉ `OpenX(bool)`), scene inventory WelcomeScene (5 đối tượng gốc đang bật) và ComplexScene (`Congratulations` inactive, tiếng Anh).
