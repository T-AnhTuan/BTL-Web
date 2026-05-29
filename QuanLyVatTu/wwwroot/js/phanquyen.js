// 1. XỬ LÝ NÚT TÍCH CHỌN TOÀN QUYỀN
function toggleAll(source) {
    // Tìm thẻ <tr> chứa cái checkbox "Toàn quyền" vừa được click
    const row = source.closest('tr');

    // Tìm tất cả các checkbox con (Xem, Thêm, Sửa, Xóa) trong cùng cái hàng đó
    const childCheckboxes = row.querySelectorAll('.chk-child');

    // Đổi trạng thái của các checkbox con theo trạng thái của nút Toàn quyền
    childCheckboxes.forEach(chk => {
        chk.checked = source.checked;
    });
}

// 2. XỬ LÝ MODAL THÊM VAI TRÒ MỚI
document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('addRoleModal');
    const btnOpen = document.getElementById('btnThemVaiTro'); // Cần đảm bảo nút thêm ở sidebar có id này
    const btnClose = document.getElementById('btnCloseRoleModal');
    const btnCancel = document.getElementById('btnCancelRole');
    const btnSave = document.getElementById('btnSaveRole');

    // Mở modal
    if (btnOpen) {
        btnOpen.addEventListener('click', function (e) {
            e.preventDefault();
            modal.style.display = 'flex';
            document.getElementById('newRoleName').value = '';
            document.getElementById('newRoleDesc').value = '';
        });
    }

    // Đóng modal
    const closeModal = () => modal.style.display = 'none';
    if (btnClose) btnClose.addEventListener('click', closeModal);
    if (btnCancel) btnCancel.addEventListener('click', closeModal);

    // Lưu Vai trò mới xuống Server bằng Fetch API
    if (btnSave) {
        btnSave.addEventListener('click', async function () {
            const tenVaiTro = document.getElementById('newRoleName').value.trim();
            const moTa = document.getElementById('newRoleDesc').value.trim();

            if (!tenVaiTro) {
                alert('Vui lòng nhập tên vai trò!');
                return;
            }

            // Đổi text nút thành Đang lưu để tránh click nhiều lần
            btnSave.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang xử lý...';
            btnSave.disabled = true;

            try {
                const response = await fetch('/QuanTri/TaoMoiVaiTro', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        TenVaiTro: tenVaiTro,
                        MoTa: moTa
                    })
                });

                const result = await response.json();

                if (result.success) {
                    alert(result.message);
                    // Tải lại trang để thấy vai trò mới ở menu bên trái
                    window.location.reload();
                } else {
                    alert('Lỗi: ' + result.message);
                    btnSave.innerHTML = 'Lưu Vai Trò';
                    btnSave.disabled = false;
                }
            } catch (error) {
                console.error('Error:', error);
                alert('Có lỗi xảy ra khi kết nối đến máy chủ!');
                btnSave.innerHTML = 'Lưu Vai Trò';
                btnSave.disabled = false;
            }
        });
    }
});