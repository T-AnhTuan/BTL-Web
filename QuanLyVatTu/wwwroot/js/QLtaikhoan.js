document.addEventListener('DOMContentLoaded', function () {
    const userModal = document.getElementById('addUserModal');
    const btnOpenUser = document.getElementById('btnOpenModal');
    const btnCloseUser = document.getElementById('btnCloseModal');
    const btnCancelUser = document.getElementById('btnCancel');
    const btnSaveUser = document.getElementById('btnSave');

    // DOM Elements
    const inputId = document.getElementById('inputId');
    const txtUsername = document.getElementById('txtUsername');
    const txtPassword = document.getElementById('txtPassword');
    const txtFullName = document.getElementById('txtFullname'); // ĐÃ SỬA: Khớp ID với file HTML
    const cbRole = document.getElementById('cbRole');
    const cbStatus = document.getElementById('cbStatus');
    const modalTitle = document.getElementById('modalTitle');
    const pwHint = document.getElementById('pwHint');

    if (!btnOpenUser || !userModal) return;

    // Mở Modal (Thêm mới)
    btnOpenUser.onclick = () => {
        if (inputId) inputId.value = "0";
        if (txtUsername) { txtUsername.value = ""; txtUsername.disabled = false; }
        if (txtPassword) txtPassword.value = "";
        if (txtFullName) txtFullName.value = "";
        if (cbRole && cbRole.options.length > 0) cbRole.selectedIndex = 0;
        if (cbStatus) cbStatus.value = "1";

        if (modalTitle) modalTitle.innerHTML = '<i class="fa-solid fa-user-plus" style="color:#3b82f6; margin-right:8px;"></i> THÊM TÀI KHOẢN MỚI';
        if (pwHint) pwHint.style.display = 'none';

        userModal.classList.add('show');
    };

    // Đóng Modal
    const closeUserModal = () => userModal.classList.remove('show');
    if (btnCloseUser) btnCloseUser.onclick = closeUserModal;
    if (btnCancelUser) btnCancelUser.onclick = closeUserModal;
    window.onclick = (e) => { if (e.target == userModal) closeUserModal(); }

    // ==========================================
    // 1. CHỨC NĂNG THÊM & SỬA TÀI KHOẢN
    // ==========================================
    if (btnSaveUser) {
        btnSaveUser.onclick = () => {
            if (!txtUsername || !txtFullName) return; // Chặn lỗi không tìm thấy input

            if (!txtUsername.value.trim() || !txtFullName.value.trim()) {
                alert("Vui lòng nhập đầy đủ Tên đăng nhập và Họ tên.");
                return;
            }
            if (inputId.value === "0" && !txtPassword.value.trim()) {
                alert("Vui lòng nhập mật khẩu khởi tạo cho tài khoản mới.");
                return;
            }

            const data = {
                id: parseInt(inputId.value),
                tenDangNhap: txtUsername.value.trim(),
                matKhau: txtPassword.value.trim(),
                hoTen: txtFullName.value.trim(),
                vaiTroId: parseInt(cbRole.value),
                trangThai: parseInt(cbStatus.value)
            };

            fetch('/QuanTri/LuuTaiKhoan', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            })
                .then(response => response.json())
                .then(result => {
                    if (result.success) {
                        alert("Lưu tài khoản thành công!");
                        closeUserModal();
                        location.reload();
                    } else {
                        alert(result.message || "Có lỗi xảy ra khi lưu.");
                    }
                })
                .catch(err => {
                    console.error(err);
                    alert("Lỗi hệ thống: Không thể kết nối tới máy chủ.");
                });
        };
    }

    // ==========================================
    // 2. CHỨC NĂNG LẤY DỮ LIỆU ĐỂ SỬA
    // ==========================================
    document.querySelectorAll('.btn-edit').forEach(btn => {
        btn.onclick = function () {
            const id = this.getAttribute('data-id');
            fetch(`/QuanTri/GetTaiKhoanById?id=${id}`)
                .then(res => res.json())
                .then(result => {
                    if (result.success) {
                        const acc = result.data;
                        if (inputId) inputId.value = acc.id;
                        if (txtUsername) { txtUsername.value = acc.tenDangNhap; txtUsername.disabled = true; }
                        if (txtFullName) txtFullName.value = acc.hoTen;
                        if (cbRole) cbRole.value = acc.vaiTroId;
                        if (cbStatus) cbStatus.value = acc.trangThai;

                        if (txtPassword) txtPassword.value = "";
                        if (pwHint) pwHint.style.display = 'block';
                        if (modalTitle) modalTitle.innerHTML = '<i class="fa-solid fa-user-pen" style="color:#3b82f6; margin-right:8px;"></i> CẬP NHẬT TÀI KHOẢN';

                        userModal.classList.add('show');
                    } else {
                        alert(result.message);
                    }
                })
                .catch(err => alert("Lỗi khi tải thông tin tài khoản."));
        }
    });

    // ==========================================
    // 3. CHỨC NĂNG KHÓA / MỞ KHÓA TÀI KHOẢN
    // ==========================================
    document.querySelectorAll('.btn-lock').forEach(btn => {
        btn.onclick = function () {
            const id = this.getAttribute('data-id');
            if (confirm("Bạn có chắc chắn muốn thay đổi trạng thái (Khóa/Mở) của tài khoản này?")) {
                fetch(`/QuanTri/KhoaMoTaiKhoan?id=${id}`, { method: 'POST' })
                    .then(res => res.json())
                    .then(result => {
                        if (result.success) {
                            location.reload();
                        } else {
                            alert(result.message);
                        }
                    })
                    .catch(err => alert("Lỗi hệ thống."));
            }
        }
    });

    // ==========================================
    // 4. CHỨC NĂNG XÓA TÀI KHOẢN
    // ==========================================
    document.querySelectorAll('.btn-delete').forEach(btn => {
        btn.onclick = function () {
            const id = this.getAttribute('data-id');
            if (confirm("CẢNH BÁO: Bạn có chắc chắn muốn xóa tài khoản này? Nếu tài khoản đã có dữ liệu giao dịch, nó sẽ chỉ bị khóa thay vì xóa vĩnh viễn.")) {
                fetch(`/QuanTri/XoaTaiKhoan?id=${id}`, { method: 'POST' })
                    .then(res => res.json())
                    .then(result => {
                        if (result.success) {
                            alert("Đã xóa tài khoản thành công!");
                            location.reload();
                        } else {
                            alert(result.message);
                            location.reload();
                        }
                    })
                    .catch(err => alert("Lỗi hệ thống."));
            }
        }
    });

    // ==========================================
    // 5. CHỨC NĂNG LỌC DỮ LIỆU (TÌM KIẾM TRỰC TIẾP TRÊN BẢNG)
    // ==========================================
    const filterSelects = document.querySelectorAll('.filter-select');
    if (filterSelects.length >= 2) {
        const filterRole = filterSelects[0]; // Dropdown Vai trò
        const filterStatus = filterSelects[1]; // Dropdown Trạng thái

        // Hàm xử lý lọc dữ liệu
        function applyFilters() {
            // Lấy nội dung chữ của Vai trò đang chọn (Ví dụ: "Nhân viên kho")
            const selectedRoleText = filterRole.value ? filterRole.options[filterRole.selectedIndex].text : "";
            // Lấy giá trị của Trạng thái (1 hoặc 0)
            const selectedStatusValue = filterStatus.value;

            // Lấy tất cả các dòng dữ liệu trong bảng
            const rows = document.querySelectorAll('.data-table tbody tr');

            rows.forEach(row => {
                // Nếu bảng đang trống (hiện dòng "Chưa có tài khoản nào") thì bỏ qua
                if (row.cells.length === 1) return;

                // Cột thứ 4 (index 3) là Vai trò, Cột thứ 5 (index 4) là Trạng thái
                const roleCellText = row.cells[3].innerText.trim();
                const statusCellText = row.cells[4].innerText.trim();

                // Chuyển đổi chữ trên giao diện thành mã (1 và 0) để so sánh
                let currentStatusValue = "";
                if (statusCellText === "Hoạt động") currentStatusValue = "1";
                if (statusCellText === "Bị khóa") currentStatusValue = "0";

                // Kiểm tra điều kiện: Nếu dropdown chưa chọn gì ("") hoặc giá trị khớp thì là true
                const isRoleMatch = selectedRoleText === "" || roleCellText === selectedRoleText;
                const isStatusMatch = selectedStatusValue === "" || currentStatusValue === selectedStatusValue;

                // Nếu khớp cả 2 điều kiện thì hiển thị, ngược lại thì ẩn đi
                if (isRoleMatch && isStatusMatch) {
                    row.style.display = ""; // Hiển thị hàng
                } else {
                    row.style.display = "none"; // Ẩn hàng
                }
            });
        }

        // Bắt sự kiện khi người dùng thay đổi lựa chọn ở 2 dropdown
        filterRole.addEventListener('change', applyFilters);
        filterStatus.addEventListener('change', applyFilters);
    }
});