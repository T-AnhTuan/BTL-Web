const supModal = document.getElementById('addSupplierModal');
const btnOpenSup = document.getElementById('btnOpenModal');
const btnCloseSup = document.getElementById('btnCloseModal');
const btnCancelSup = document.getElementById('btnCancel');
const btnSaveSup = document.getElementById('btnSave');
const supplierTitle = supModal.querySelector('.modal-header h3');

// Các thẻ Input
const inputId = document.getElementById('inputId'); // Bắt buộc phải có thẻ này trong HTML
const inputTen = document.getElementById('inputTen');
const inputDiaChi = document.getElementById('inputDiaChi');
const inputSDT = document.getElementById('inputSDT');
const inputEmail = document.getElementById('inputEmail');
const selectTrangThai = document.getElementById('selectTrangThai');

// 1. Hàm Mở Modal (Cho cả Thêm và Sửa)
function openSupModal(mode = 'create') {
    if (mode === 'create') {
        resetSupplierForm();
        supplierTitle.innerHTML = '<i class="fa-solid fa-industry" style="color:#2563eb; margin-right:8px;"></i> Thêm Mới Nhà Cung Cấp';
        btnSaveSup.innerHTML = '<i class="fa-solid fa-floppy-disk"></i> Lưu Thay Đổi';
    } else {
        supplierTitle.innerHTML = '<i class="fa-solid fa-pen-to-square" style="color:#2563eb; margin-right:8px;"></i> Cập Nhật Nhà Cung Cấp';
        btnSaveSup.innerHTML = '<i class="fa-solid fa-floppy-disk"></i> Cập Nhật';
    }
    supModal.classList.add('show');
}

// 2. Hàm Đóng và Reset Modal
function closeSupModal() {
    supModal.classList.remove('show');
    resetSupplierForm();
}

function resetSupplierForm() {
    if (inputId) inputId.value = '';
    inputTen.value = '';
    inputDiaChi.value = '';
    inputSDT.value = '';
    inputEmail.value = '';
    selectTrangThai.value = '1';
}

// Gắn sự kiện đóng/mở cơ bản
if (btnOpenSup) btnOpenSup.addEventListener('click', () => openSupModal('create'));
if (btnCloseSup) btnCloseSup.addEventListener('click', closeSupModal);
if (btnCancelSup) btnCancelSup.addEventListener('click', closeSupModal);

// 3. BẮT SỰ KIỆN SỬA / XÓA TRÊN BẢNG (Dùng Delegation để không bị trượt khi click vào icon)
document.addEventListener('DOMContentLoaded', () => {
    const tableBody = document.querySelector('tbody');

    if (tableBody) {
        tableBody.addEventListener('click', async (e) => {
            // ----- XỬ LÝ NÚT SỬA -----
            const btnEdit = e.target.closest('.btn-edit'); // Tìm class btn-edit
            if (btnEdit) {
                const id = btnEdit.dataset.id;
                try {
                    // Gọi API lấy dữ liệu chi tiết
                    const response = await fetch(`/DanhMuc/GetNhaCungCap?id=${id}`);
                    const result = await response.json();

                    if (result.success) {
                        // Đổ dữ liệu vào Modal
                        if (inputId) inputId.value = result.data.id;
                        inputTen.value = result.data.tenNhaCungCap || '';
                        inputDiaChi.value = result.data.diaChi || '';
                        inputSDT.value = result.data.soDienThoai || '';
                        inputEmail.value = result.data.email || '';
                        selectTrangThai.value = String(result.data.trangThai ?? 1);

                        openSupModal('edit');
                    } else {
                        alert(result.message || 'Không tìm thấy thông tin nhà cung cấp.');
                    }
                } catch (err) {
                    console.error(err);
                    alert('Lỗi kết nối khi lấy dữ liệu: ' + err.message);
                }
                return; // Dừng hàm lại
            }

            // ----- XỬ LÝ NÚT XÓA -----
            const btnDelete = e.target.closest('.btn-delete'); // Tìm class btn-delete
            if (btnDelete) {
                const id = btnDelete.dataset.id;
                const row = btnDelete.closest('tr');
                const name = row?.children[2]?.textContent?.trim() || 'nhà cung cấp này';

                if (!confirm(`Bạn có chắc muốn xóa vĩnh viễn "${name}"?`)) {
                    return;
                }

                try {
                    btnDelete.disabled = true; // Khóa nút tránh click 2 lần
                    const response = await fetch(`/DanhMuc/XoaNhaCungCap?id=${id}`, {
                        method: 'POST' // Bảo mật: Xóa nên dùng POST
                    });
                    const result = await response.json();

                    if (result.success) {
                        alert('Xóa nhà cung cấp thành công!');
                        location.reload(); // Tải lại trang để cập nhật bảng
                    } else {
                        alert(result.message || 'Không xóa được nhà cung cấp.');
                        btnDelete.disabled = false;
                    }
                } catch (err) {
                    btnDelete.disabled = false;
                    alert('Lỗi kết nối khi xóa dữ liệu.');
                }
            }
        });
    }
});

// 4. XỬ LÝ LƯU (CẢ THÊM VÀ SỬA)
if (btnSaveSup) {
    btnSaveSup.addEventListener('click', async () => {
        // Kiểm tra dữ liệu bắt buộc
        if (!inputTen.value.trim()) {
            alert("Vui lòng nhập tên Nhà cung cấp!");
            inputTen.focus();
            return;
        }

        // Gom dữ liệu
        const data = {
            Id: inputId ? inputId.value : '', // Nếu có ID là Sửa, ko có là Thêm
            TenNhaCungCap: inputTen.value.trim(),
            DiaChi: inputDiaChi.value.trim(),
            SoDienThoai: inputSDT.value.trim(),
            Email: inputEmail.value.trim(),
            TrangThai: parseInt(selectTrangThai.value)
        };

        try {
            btnSaveSup.disabled = true;
            btnSaveSup.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang xử lý...';

            const response = await fetch('/DanhMuc/LuuNhaCungCap', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (result.success) {
                alert(result.message || 'Lưu thành công!');
                location.reload();
            } else {
                alert(result.message || 'Có lỗi xảy ra khi lưu.');
                btnSaveSup.disabled = false;
                btnSaveSup.innerHTML = inputId && inputId.value ? '<i class="fa-solid fa-floppy-disk"></i> Cập Nhật' : '<i class="fa-solid fa-floppy-disk"></i> Lưu Thay Đổi';
            }
        } catch (error) {
            console.error(error);
            alert("Lỗi kết nối đến máy chủ!");
            btnSaveSup.disabled = false;
        }
    });
}