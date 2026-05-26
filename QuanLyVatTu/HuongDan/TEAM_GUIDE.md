# 👥 Hướng Dẫn Cho Đội Team

## 🎯 Giới Thiệu

Hệ thống Quản Lý Vật Tư đã được cấu hình với:
- ✅ Hệ thống đăng nhập an toàn
- ✅ Phân quyền theo vai trò (Role-Based)
- ✅ Quản lý người dùng
- ✅ Kết nối Azure SQL Server

---

## 👤 Các Vai Trò

### 1. **Admin** (Quản trị viên)
**Quyền:**
- ✅ Xem trang chủ
- ✅ Quản lý tài khoản (Tạo, Sửa, Xóa)
- ✅ Gán vai trò cho người dùng
- ✅ Quản lý vật tư
- ✅ Xem báo cáo

**Truy cập:**
```
/Home/Index - Trang chủ
/TaiKhoan/Index - Danh sách tài khoản
/TaiKhoan/Create - Tạo tài khoản
/TaiKhoan/Edit - Chỉnh sửa tài khoản
/TaiKhoan/Delete - Xóa tài khoản
```

### 2. **Manager** (Quản lý kho)
**Quyền:**
- ✅ Xem trang chủ
- ✅ Quản lý vật tư
- ✅ Quản lý nhập/xuất kho
- ✅ Xem báo cáo
- ❌ Không thể quản lý tài khoản

**Truy cập:**
```
/Home/Index - Trang chủ
/TaiKhoan/MyProfile - Thông tin cá nhân
(Các tính năng khác khi được implement)
```

### 3. **Staff** (Nhân viên)
**Quyền:**
- ✅ Xem trang chủ
- ✅ Xem vật tư
- ✅ Thực hiện nhập/xuất kho
- ❌ Không thể quản lý tài khoản
- ❌ Không thể xem báo cáo

**Truy cập:**
```
/Home/Index - Trang chủ
/TaiKhoan/MyProfile - Thông tin cá nhân
```

---

## 🚀 Hướng Dẫn Sử Dụng

### 🔐 Đăng Nhập

1. Truy cập URL ứng dụng
2. Bạn sẽ tự động được chuyển đến trang đăng nhập
3. Nhập:
   - **Tên đăng nhập**: (Username do Admin cấp)
   - **Mật khẩu**: (Password cá nhân)
4. Nhấn **"Đăng nhập"**
5. (Tuỳ chọn) Chọn **"Ghi nhớ đăng nhập"** để session kéo dài 7 ngày

### 📝 Lần Đầu Đăng Nhập

**Tài khoản mặc định:**
- Username: `admin`
- Password: `Admin@123`

⚠️ **Hãy thay đổi mật khẩu sau khi đăng nhập lần đầu!**

### 👥 Quản Lý Tài Khoản (Chỉ Admin)

#### Xem Danh Sách
1. Vào menu: **Quản lý** → **Tài Khoản**
2. Xem danh sách toàn bộ người dùng

#### Tạo Tài Khoản Mới
1. Vào **Quản lý** → **Tài Khoản** → **Tạo Tài Khoản**
2. Điền thông tin:
   - **Tên đăng nhập**: Tên duy nhất (VD: john.doe)
   - **Email**: Email hợp lệ
   - **Mật khẩu**: Mật khẩu mạnh
   - **Họ tên**: Tên đầy đủ
   - **Vai trò**: Chọn Admin/Manager/Staff
3. Nhấn **"Tạo"**

#### Chỉnh Sửa Tài Khoản
1. Vào danh sách tài khoản
2. Tìm tài khoản cần sửa
3. Nhấn **"Chỉnh sửa"**
4. Cập nhật thông tin cần thiết
5. Nhấn **"Lưu"**

#### Xóa Tài Khoản
1. Vào danh sách tài khoản
2. Tìm tài khoản cần xóa
3. Nhấn **"Xóa"**
4. Xác nhận xóa

#### Disable Tài Khoản Tạm Thời
Thay vì xóa, bạn có thể:
1. Vào **Chỉnh sửa** tài khoản
2. Uncheck ô **"Hoạt động"** (IsActive)
3. Nhấn **"Lưu"**
4. User sẽ không thể đăng nhập

### 👤 Thông Tin Cá Nhân

Bất kỳ người dùng nào cũng có thể:
1. Vào menu: **Tài Khoản** → **Thông Tin Cá Nhân**
2. Xem:
   - Tên đăng nhập
   - Email
   - Vai trò hiện tại
   - Lần đăng nhập gần đây

### 🚪 Đăng Xuất

1. Vào menu góc phải
2. Nhấn **"Đăng Xuất"**
3. Bạn sẽ được redirect đến trang đăng nhập

---

## 🔐 Bảo Mật & Best Practices

### Mật Khẩu
- ✅ Luôn sử dụng mật khẩu mạnh
- ✅ Không chia sẻ mật khẩu với ai
- ✅ Thay đổi mật khẩu định kỳ
- ✅ Không lưu mật khẩu trong note

### Tài Khoản
- ✅ Logout sau khi dùng xong
- ✅ Không để máy tính mở khi rời khỏi
- ✅ Báo admin nếu quên mật khẩu
- ✅ Báo admin nếu tài khoản bị lộ

### Dữ Liệu
- ✅ Chỉ truy cập dữ liệu trong quyền hạn
- ✅ Không chia sẻ thông tin nhạy cảm
- ✅ Xoá bản nháp sau khi dùng xong
- ✅ Báo cáo nếu phát hiện dữ liệu bất thường

---

## ❓ Câu Hỏi Thường Gặp

### Q1: Quên mật khẩu?
**A:** Liên hệ Admin để đặt lại mật khẩu

### Q2: Bị khóa tài khoản?
**A:** Liên hệ Admin để mở khóa

### Q3: Không thể truy cập tính năng?
**A:** Kiểm tra vai trò của bạn hoặc liên hệ Admin

### Q4: Tài khoản bị lộ?
**A:** Báo ngay cho Admin và đổi mật khẩu

### Q5: Tính năng nào dành cho tôi?
**A:** 
- Admin: Tất cả tính năng
- Manager: Quản lý kho
- Staff: Xem & nhập/xuất kho

---

## 📱 Giao Diện Chính

### Menu Chính
```
┌─────────────────────────────┐
│  Quản Lý Vật Tư             │
├─────────────────────────────┤
│ 🏠 Trang Chủ                │
│ 📦 Vật Tư                   │
│ 📥 Nhập Kho                 │
│ 📤 Xuất Kho                 │
│ 📊 Báo Cáo                  │
│ ⚙️  Quản Lý                  │
│ 👤 Tài Khoản                │
└─────────────────────────────┘
```

### User Menu (Góc Phải)
```
Welcome, [Your Name]
├── 👤 Thông Tin Cá Nhân
├── 🔐 Đổi Mật Khẩu (Sắp tới)
└── 🚪 Đăng Xuất
```

---

## 🆘 Gặp Vấn Đề?

### Vấn Đề: Không thể đăng nhập
**Nguyên nhân có thể:**
- Username/Password sai
- Tài khoản bị disable
- Tài khoản không tồn tại

**Giải pháp:**
1. Kiểm tra username/password (có phân biệt chữ hoa/thường)
2. Liên hệ Admin nếu quên password
3. Chắc chắn rằng tài khoản đã được tạo

### Vấn Đề: Lỗi "Quyền truy cập bị từ chối"
**Nguyên nhân:**
- Vai trò của bạn không có quyền
- Liên kết bị hỏng

**Giải pháp:**
1. Kiểm tra vai trò của bạn
2. Yêu cầu Admin cấp quyền
3. Quay lại trang chủ

### Vấn Đề: Session bị timeout
**Nguyên nhân:**
- Không có hoạt động trong 7 ngày
- Browser tắt

**Giải pháp:**
1. Đăng nhập lại
2. Chọn "Ghi nhớ đăng nhập" để session dài hơn
3. Nếu vẫn lỗi, xóa cache & cookies

---

## 📞 Liên Hệ Hỗ Trợ

### Admin Contact
```
Name: [Admin Name]
Email: admin@quatuvatu.com
Phone: [Admin Phone]
```

### Báo Cáo Vấn Đề
1. Mô tả vấn đề rõ ràng
2. Cung cấp username/email
3. Cung cấp thời gian xảy ra
4. Cung cấp screenshot nếu có

---

## 📚 Tài Liệu Liên Quan

- **Tổng quan**: Xem `README.md`
- **Hướng dẫn chi tiết**: Xem `LOGIN_GUIDE.md`
- **Lệnh útile**: Xem `COMMANDS_REFERENCE.md`

---

## ✅ Kiểm Tra Lần Đầu

**Hãy thực hiện các bước sau để kiểm tra:**

- [ ] Bạn có thể truy cập URL ứng dụng
- [ ] Bạn có thể xem trang đăng nhập
- [ ] Bạn có thể đăng nhập với tài khoản được cấp
- [ ] Bạn có thể xem trang chủ
- [ ] Bạn có thể xem thông tin cá nhân
- [ ] Bạn có thể đăng xuất
- [ ] Bạn có thể thấy role-based menu (phụ thuộc vai trò)

---

## 🎓 Đào Tạo Nhanh (5 phút)

### Cho Admin
1. Biết cách tạo/sửa/xóa tài khoản
2. Biết cách gán vai trò
3. Biết khi nào disable account
4. Biết cách handle quên mật khẩu

### Cho Manager
1. Biết các tính năng có sẵn
2. Biết cách xem báo cáo
3. Biết cách thực hiện nhập/xuất

### Cho Staff
1. Biết cách đăng nhập/đăng xuất
2. Biết các tính năng có sẵn
3. Biết cách báo cáo vấn đề

---

## 🎯 Quy Trình Hàng Ngày

### Sáng
1. 08:00 - Đăng nhập hệ thống
2. Kiểm tra thông báo/danh sách việc hôm nay
3. Bắt đầu công việc

### Giữa Ngày
1. Cập nhật dữ liệu khi cần
2. Xuất báo cáo nếu cần
3. Liên hệ admin nếu gặp vấn đề

### Tối
1. Hoàn thành công việc ngày
2. Cập nhật dữ liệu cuối cùng
3. Đăng xuất trước khi rời

---

## 📋 Checklist Lần Đầu

**Sau khi nhận tài khoản:**
- [ ] Nhận credentials từ Admin
- [ ] Đăng nhập thành công
- [ ] Đổi mật khẩu ban đầu (admin/Admin@123)
- [ ] Xem thông tin cá nhân
- [ ] Làm quen với giao diện
- [ ] Kiểm tra quyền hạn của bạn
- [ ] Báo cáo bất kỳ vấn đề nào cho Admin

---

## 🎊 Chúc Mừng!

Bạn đã sẵn sàng sử dụng hệ thống Quản Lý Vật Tư!

**Hãy nhớ:**
- 🔐 Giữ bảo mật mật khẩu
- 📧 Kiểm tra email định kỳ
- 📞 Liên hệ admin nếu cần
- 📚 Đọc hướng dẫn nếu chưa rõ

---

**Version**: 1.0  
**Last Updated**: 2024  
**Status**: ✅ Ready
