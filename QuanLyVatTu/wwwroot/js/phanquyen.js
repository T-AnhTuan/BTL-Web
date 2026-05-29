/// 1. Hàm Xử lý khi click "Toàn Quyền"
function toggleAll(btnNode) {
    var row = btnNode.closest('tr');
    var childChecks = row.querySelectorAll('.chk-child');

    childChecks.forEach(function (chk) {
        if (chk.checked !== btnNode.checked) {
            chk.checked = btnNode.checked;
            chk.dispatchEvent(new Event('change', { bubbles: true }));
        }
    });
}

// 2. Hàm Xử lý khi click các ô quyền con
function checkChild(chkNode) {
    var row = chkNode.closest('tr');
    var childChecks = row.querySelectorAll('.chk-child');
    var checkAllBtn = row.querySelector('.chk-all-row');

    var isAllChecked = Array.from(childChecks).every(function (chk) {
        return chk.checked === true;
    });

    if (checkAllBtn) {
        checkAllBtn.checked = isAllChecked;
    }
}

// 3. Xử lý UI Modal và Khởi tạo
document.addEventListener('DOMContentLoaded', function () {
    const btnOpenRole = document.getElementById('btnOpenAddRole');
    const roleModal = document.getElementById('addRoleModal');
    const btnCloseRole = document.getElementById('btnCloseRoleModal');
    const btnCancelRole = document.getElementById('btnCancelRole');
    const btnSaveRole = document.getElementById('btnSaveRole');

    // Mở Modal
    if (btnOpenRole) {
        btnOpenRole.addEventListener('click', function () {
            roleModal.style.display = 'flex';
        });
    }

    // Đóng Modal
    const closeRoleModal = () => {
        if (roleModal) roleModal.style.display = 'none';
    };

    if (btnCloseRole) btnCloseRole.addEventListener('click', closeRoleModal);
    if (btnCancelRole) btnCancelRole.addEventListener('click', closeRoleModal);

    window.addEventListener('click', function (e) {
        if (e.target == roleModal) closeRoleModal();
    });

    // SỰ KIỆN: Bấm nút "Lưu Vai Trò" (Đã được khôi phục)
    if (btnSaveRole) {
        btnSaveRole.addEventListener('click', function () {
            const newRoleName = document.getElementById('newRoleName').value.trim();
            const newRoleDesc = document.getElementById('newRoleDesc') ? document.getElementById('newRoleDesc').value.trim() : '';

            if (newRoleName === "") {
                alert("Vui lòng nhập tên vai trò!");
                return;
            }

            // Gọi API Thêm vai trò
            fetch('/QuanTri/ThemVaiTro', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ TenVaiTro: newRoleName, MoTa: newRoleDesc })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        window.location.reload(); // Load lại trang để thấy vai trò mới
                    } else {
                        alert(data.message || "Có lỗi xảy ra khi thêm vai trò!");
                    }
                })
                .catch(err => {
                    console.error(err);
                    alert("Lỗi kết nối đến máy chủ!");
                });
        });
    }

    // 4. Tự động kiểm tra: Nếu 4 quyền con đã tích, bật luôn ô Toàn quyền
    var rows = document.querySelectorAll('.matrix-table tbody tr');
    rows.forEach(function (row) {
        var childChecks = row.querySelectorAll('.chk-child');
        if (childChecks.length > 0) {
            var checkAllBtn = row.querySelector('.chk-all-row');
            var isAllChecked = Array.from(childChecks).every(chk => chk.checked === true);
            if (checkAllBtn) checkAllBtn.checked = isAllChecked;
        }
    });
});
// 3. Xử lý UI các nút bật tắt Modal (Thêm Vai trò)
document.addEventListener('DOMContentLoaded', function () {
    const btnOpenRole = document.getElementById('btnOpenAddRole');
    const roleModal = document.getElementById('addRoleModal');
    const btnCloseRole = document.getElementById('btnCloseRoleModal');
    const btnCancelRole = document.getElementById('btnCancelRole');

    // Mở Modal
    if (btnOpenRole) {
        btnOpenRole.addEventListener('click', function () {
            roleModal.style.display = 'flex';
        });
    }

    // Đóng Modal
    const closeRoleModal = () => {
        if (roleModal) roleModal.style.display = 'none';
    };

    if (btnCloseRole) btnCloseRole.addEventListener('click', closeRoleModal);
    if (btnCancelRole) btnCancelRole.addEventListener('click', closeRoleModal);

    // Đóng khi click ngoài vùng modal
    window.addEventListener('click', function (e) {
        if (e.target == roleModal) {
            closeRoleModal();
        }
    });

    // 4. CHẠY MỘT LẦN KHI LOAD TRANG: 
    // Quét toàn bộ bảng xem hàng nào đã đủ 4 quyền thì bật ô "Toàn quyền" lên
    var rows = document.querySelectorAll('.matrix-table tbody tr');
    rows.forEach(function (row) {
        var childChecks = row.querySelectorAll('.chk-child');
        if (childChecks.length > 0) { // Bỏ qua nếu là dòng "Chưa khởi tạo chức năng"
            var checkAllBtn = row.querySelector('.chk-all-row');
            var isAllChecked = Array.from(childChecks).every(chk => chk.checked === true);
            if (checkAllBtn) {
                checkAllBtn.checked = isAllChecked;
            }
        }
    });
});