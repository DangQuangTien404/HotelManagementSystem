# Báo cáo lỗi hệ thống: MyTasks không hiển thị công việc dọn phòng

## 1) Mô tả lỗi
Nhân viên (tài khoản `a`) đã được Admin phân công dọn phòng thành công (phòng trên sơ đồ chuyển sang trạng thái **Cleaning**) nhưng khi đăng nhập vào trang **MyTasks** thì danh sách công việc vẫn trống, không xuất hiện nút **Xác nhận xong**.

## 2) Nguyên nhân gốc rễ

### 2.1 Xung đột namespace
Thư mục trang được đặt tên dễ gây nhầm lẫn với model `Staff`, dẫn đến khả năng trình biên dịch hiểu sai kiểu khi truy vấn nếu không chỉ định kiểu đầy đủ.

### 2.2 Lấy UserId từ Claims chưa tương thích
Dự án dùng cookie authentication thuần, một số nơi dùng API rút gọn không ổn định trong ngữ cảnh hiện tại. Cách an toàn là lấy trực tiếp:

```csharp
User.FindFirst(ClaimTypes.NameIdentifier)?.Value
```

### 2.3 Lệch chuỗi trạng thái
Dữ liệu phân công và điều kiện lọc truy vấn không đồng nhất trạng thái (ví dụ `In Progress` vs giá trị khác), khiến truy vấn không trả về bản ghi.

## 3) Quy trình kiểm tra
1. Xác nhận thao tác phân công của Admin đã ghi bản ghi vào `RoomCleanings`.
2. Kiểm tra `CleanedBy` có đúng `User.Id` của nhân viên đăng nhập.
3. Kiểm tra trạng thái được lưu có khớp hoàn toàn chuỗi dùng trong điều kiện `WHERE`.
4. Đăng xuất/đăng nhập lại để làm mới cookie claims.

## 4) Giải pháp kỹ thuật

| Thành phần | Giải pháp |
|---|---|
| Model | Dùng kiểu đầy đủ `HotelManagementSystem.Data.Models.Staff` (khi cần) để tránh nhầm namespace. |
| Authentication | Dùng `User.FindFirst(ClaimTypes.NameIdentifier)?.Value` để lấy UserId ổn định. |
| Database/Query | Đồng nhất trạng thái xử lý, ưu tiên `In Progress` cho cả lúc gán và lúc lọc dữ liệu. |
| Session | Logout/Login lại sau khi cập nhật quyền hoặc claims. |

## 5) Kết luận và khuyến nghị
Để tránh tái diễn:
1. Đảm bảo `Users.Id` khớp với `RoomCleanings.CleanedBy`.
2. Đồng bộ trạng thái giữa `Rooms.Status` và `RoomCleanings.Status` theo cùng bộ giá trị chuẩn.
3. Chuẩn hóa hằng số trạng thái trong code (enum/constant) để tránh lỗi sai chính tả chuỗi.
