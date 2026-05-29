document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('addCategoryModal');
    const btnOpenModal = document.getElementById('btnOpenModal');
    const btnCloseModal = document.getElementById('btnCloseModal');
    const btnCancel = document.getElementById('btnCancel');
    const btnSave = document.getElementById('btnSave');

    // Các ô input
    const inputId = document.getElementById('inputId');
    const inputMa = document.getElementById('inputMa');
    const inputTen = document.getElementById('inputTen');
    const inputMoTa = document.getElementById('inputMoTa');
    const selectTrangThai = document.getElementById('selectTrangThai');

    // 1. Mở Modal Thêm mới
    if (btnOpenModal) {
        btnOpenModal.addEventListener('click', function () {
            // Reset form
            inputId.value = '0';
            inputMa.value = '';
            inputTen.value = '';
            inputMoTa.value = '';
            selectTrangThai.value = '1';

            document.querySelector('#addCategoryModal h3').innerText = 'Thêm Danh Mục Mới';
            modal.style.display = 'flex';
        });
    }

    // Đóng modal
    const closeModal = () => modal.style.display = 'none';
    if (btnCloseModal) btnCloseModal.addEventListener('click', closeModal);
    if (btnCancel) btnCancel.addEventListener('click', closeModal);

    // 2. Xử lý nút SỬA (Lấy dữ liệu đổ vào Modal)
    document.querySelectorAll('.btn-edit').forEach(button => {
        button.addEventListener('click', function () {
            const id = this.getAttribute('data-id');

            // Gọi API lấy dữ liệu chi tiết
            fetch(`/DanhMuc/GetDanhMucById?id=${id}`)
                .then(response => response.json())
                .then(res => {
                    if (res.success) {
                        // Đổ dữ liệu vào Form
                        inputId.value = res.data.id;
                        inputMa.value = res.data.maDanhMuc;
                        inputTen.value = res.data.tenDanhMuc;
                        inputMoTa.value = res.data.moTa || '';
                        selectTrangThai.value = res.data.trangThai;

                        // Đổi tiêu đề và mở Modal
                        document.querySelector('#addCategoryModal h3').innerText = 'Cập Nhật Danh Mục';
                        modal.style.display = 'flex';
                    } else {
                        alert(res.message || 'Không lấy được thông tin danh mục.');
                    }
                })
                .catch(err => {
                    console.error('Error:', err);
                    alert('Lỗi kết nối đến máy chủ.');
                });
        });
    });

    // 3. Xử lý nút XÓA
    document.querySelectorAll('.btn-delete').forEach(button => {
        button.addEventListener('click', function () {
            const id = this.getAttribute('data-id');

            if (confirm('Bạn có chắc chắn muốn xóa danh mục này không? Thao tác này không thể hoàn tác!')) {
                fetch(`/DanhMuc/XoaDanhMuc?id=${id}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                })
                    .then(response => response.json())
                    .then(res => {
                        if (res.success) {
                            alert('Xóa danh mục thành công!');
                            location.reload(); // Tải lại trang để cập nhật bảng
                        } else {
                            alert(res.message || 'Không thể xóa danh mục này.');
                        }
                    })
                    .catch(err => console.error('Error:', err));
            }
        });
    });

    // 4. Xử lý nút LƯU (Dùng cho cả Thêm mới và Sửa)
    if (btnSave) {
        btnSave.addEventListener('click', function () {
            // Validate sơ bộ
            if (inputMa.value.trim() === '' || inputTen.value.trim() === '') {
                alert('Vui lòng nhập đầy đủ Mã và Tên danh mục!');
                return;
            }

            const dataObj = {
                Id: parseInt(inputId.value) || 0,
                MaDanhMuc: inputMa.value.trim(),
                TenDanhMuc: inputTen.value.trim(),
                MoTa: inputMoTa.value.trim(),
                TrangThai: parseInt(selectTrangThai.value)
            };

            fetch('/DanhMuc/LuuDanhMuc', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(dataObj)
            })
                .then(response => response.json())
                .then(res => {
                    if (res.success) {
                        alert(res.message || 'Lưu danh mục thành công!');
                        location.reload(); // Tải lại bảng
                    } else {
                        alert(res.message || 'Có lỗi xảy ra khi lưu.');
                    }
                })
                .catch(err => {
                    console.error('Error:', err);
                    alert('Lỗi kết nối đến máy chủ.');
                });
        });
    }
});