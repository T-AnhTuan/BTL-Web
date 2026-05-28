const supModal = document.getElementById('addSupplierModal');
const btnOpenSup = document.getElementById('btnOpenModal');
const btnCloseSup = document.getElementById('btnCloseModal');
const btnCancelSup = document.getElementById('btnCancel');
const btnSaveSup = document.getElementById('btnSave');
const supplierTitle = supModal.querySelector('.modal-header h3');

const inputId = document.getElementById('inputId');
const inputMaNCC = document.getElementById('inputMaNCC');
const inputTen = document.getElementById('inputTen');
const inputDiaChi = document.getElementById('inputDiaChi');
const inputSDT = document.getElementById('inputSDT');
const inputEmail = document.getElementById('inputEmail');
const selectTrangThai = document.getElementById('selectTrangThai');

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

function closeSupModal() {
    supModal.classList.remove('show');
}

function resetSupplierForm() {
    inputId.value = '0';
    inputMaNCC.value = '';
    inputTen.value = '';
    inputDiaChi.value = '';
    inputSDT.value = '';
    inputEmail.value = '';
    selectTrangThai.value = '1';
}

btnOpenSup.addEventListener('click', () => openSupModal('create'));
btnCloseSup.addEventListener('click', closeSupModal);
btnCancelSup.addEventListener('click', closeSupModal);

window.addEventListener('click', (e) => {
    if (e.target === supModal) {
        closeSupModal();
    }
});

btnSaveSup.addEventListener('click', async () => {
    const data = {
        Id: parseInt(inputId.value || '0', 10),
        MaNCC: inputMaNCC.value.trim(),
        TenNhaCungCap: inputTen.value.trim(),
        DiaChi: inputDiaChi.value.trim(),
        SoDienThoai: inputSDT.value.trim(),
        Email: inputEmail.value.trim(),
        TrangThai: parseInt(selectTrangThai.value || '1', 10)
    };

    if (!data.MaNCC) {
        alert('Vui lòng nhập mã nhà cung cấp.');
        inputMaNCC.focus();
        return;
    }

    if (!data.TenNhaCungCap) {
        alert('Vui lòng nhập tên nhà cung cấp.');
        inputTen.focus();
        return;
    }

    try {
        btnSaveSup.disabled = true;
        const response = await fetch('/DanhMuc/LuuNhaCungCap', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        const result = await response.json();
        if (result.success) {
            alert(data.Id === 0 ? 'Thêm nhà cung cấp thành công!' : 'Cập nhật nhà cung cấp thành công!');
            location.reload();
        } else {
            alert(result.message || 'Có lỗi xảy ra khi lưu nhà cung cấp.');
        }
    } catch (err) {
        alert('Lỗi kết nối: ' + err.message);
    } finally {
        btnSaveSup.disabled = false;
    }
});

document.addEventListener('click', async (e) => {
    const editButton = e.target.closest('.edit-btn');
    if (editButton) {
        const id = editButton.dataset.id;

        try {
            const response = await fetch(`/DanhMuc/GetNhaCungCapById?id=${encodeURIComponent(id)}`);
            const result = await response.json();

            if (!result.success) {
                alert(result.message || 'Không lấy được dữ liệu nhà cung cấp.');
                return;
            }

            inputId.value = result.data.id;
            inputMaNCC.value = result.data.maNCC || '';
            inputTen.value = result.data.tenNhaCungCap || '';
            inputDiaChi.value = result.data.diaChi || '';
            inputSDT.value = result.data.soDienThoai || '';
            inputEmail.value = result.data.email || '';
            selectTrangThai.value = String(result.data.trangThai ?? 1);
            openSupModal('edit');
        } catch (err) {
            alert('Lỗi kết nối khi lấy dữ liệu: ' + err.message);
        }

        return;
    }

    const deleteButton = e.target.closest('.delete-btn');
    if (deleteButton) {
        const id = deleteButton.dataset.id;
        const row = deleteButton.closest('tr');
        const name = row?.children[2]?.textContent?.trim() || 'nhà cung cấp này';

        if (!confirm(`Bạn có chắc muốn xóa "${name}"?`)) {
            return;
        }

        try {
            deleteButton.disabled = true;
            const response = await fetch(`/DanhMuc/XoaNhaCungCap?id=${encodeURIComponent(id)}`, {
                method: 'POST'
            });
            const result = await response.json();

            if (result.success) {
                alert('Xóa nhà cung cấp thành công!');
                location.reload();
            } else {
                alert(result.message || 'Không xóa được nhà cung cấp.');
                deleteButton.disabled = false;
            }
        } catch (err) {
            deleteButton.disabled = false;
            alert('Lỗi kết nối khi xóa: ' + err.message);
        }
    }
});
