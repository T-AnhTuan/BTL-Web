const catModal = document.getElementById('addCategoryModal');
const btnOpenCat = document.getElementById('btnOpenModal');
const btnCloseCat = document.getElementById('btnCloseModal');
const btnCancelCat = document.getElementById('btnCancel');
const btnSaveCat = document.getElementById('btnSave');
const inputId = document.getElementById('inputId');
const inputMa = document.getElementById('inputMa');
const inputTen = document.getElementById('inputTen');
const inputMoTa = document.getElementById('inputMoTa');
const selectTrangThai = document.getElementById('selectTrangThai');
const modalTitle = catModal.querySelector('.modal-header h3');

const openCatModal = (mode = 'create') => {
    if (mode === 'create') {
        inputId.value = '0';
        inputMa.value = '';
        inputTen.value = '';
        inputMoTa.value = '';
        selectTrangThai.value = '1';
        modalTitle.textContent = 'Thêm Danh Mục Mới';
        btnSaveCat.textContent = 'Lưu danh mục';
    } else {
        modalTitle.textContent = 'Cập Nhật Danh Mục';
        btnSaveCat.textContent = 'Cập nhật';
    }

    catModal.classList.add('show');
};

const closeCatModal = () => catModal.classList.remove('show');

btnOpenCat.addEventListener('click', () => openCatModal('create'));
btnCloseCat.addEventListener('click', closeCatModal);
btnCancelCat.addEventListener('click', closeCatModal);

window.addEventListener('click', (e) => {
    if (e.target === catModal) {
        closeCatModal();
    }
});

btnSaveCat.addEventListener('click', async () => {
    const data = {
        Id: parseInt(inputId.value || '0', 10),
        MaDanhMuc: inputMa.value.trim(),
        TenDanhMuc: inputTen.value.trim(),
        MoTa: inputMoTa.value.trim(),
        TrangThai: parseInt(selectTrangThai.value || '1', 10)
    };

    if (!data.MaDanhMuc) {
        alert('Vui lòng nhập mã danh mục.');
        inputMa.focus();
        return;
    }

    if (!data.TenDanhMuc) {
        alert('Vui lòng nhập tên danh mục.');
        inputTen.focus();
        return;
    }

    try {
        btnSaveCat.disabled = true;
        const response = await fetch('/DanhMuc/LuuDanhMuc', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        const result = await response.json();
        if (result.success) {
            alert('Lưu thành công!');
            location.reload();
        } else {
            alert(result.message || 'Có lỗi xảy ra!');
        }
    } catch (err) {
        alert('Lỗi kết nối: ' + err.message);
    } finally {
        btnSaveCat.disabled = false;
    }
});

const searchInput = document.getElementById('searchInput');
const statusSelect = document.getElementById('statusSelect');
const selectAll = document.getElementById('selectAll');
const tableRows = Array.from(document.querySelectorAll('#tableBody tr[data-id]'));

function filterRows() {
    const keyword = (searchInput?.value || '').trim().toLowerCase();
    const status = statusSelect?.value || '';

    tableRows.forEach((row) => {
        const code = row.children[2]?.textContent?.toLowerCase() || '';
        const name = row.children[3]?.textContent?.toLowerCase() || '';
        const rowStatus = row.children[7]?.textContent?.includes('Hoạt động') ? '1' : '0';
        const matchesKeyword = !keyword || code.includes(keyword) || name.includes(keyword);
        const matchesStatus = !status || rowStatus === status;
        row.style.display = matchesKeyword && matchesStatus ? '' : 'none';
    });
}

searchInput?.addEventListener('input', filterRows);
statusSelect?.addEventListener('change', filterRows);
selectAll?.addEventListener('change', () => {
    document.querySelectorAll('.rowCheckbox').forEach((checkbox) => {
        checkbox.checked = selectAll.checked;
    });
});

document.querySelectorAll('.edit-btn').forEach((button) => {
    button.addEventListener('click', async () => {
        const id = button.dataset.id;

        try {
            const response = await fetch(`/DanhMuc/GetDanhMucById?id=${encodeURIComponent(id)}`);
            const result = await response.json();

            if (!result.success) {
                alert(result.message || 'Không lấy được dữ liệu danh mục.');
                return;
            }

            inputId.value = result.data.id;
            inputMa.value = result.data.maDanhMuc || '';
            inputTen.value = result.data.tenDanhMuc || '';
            inputMoTa.value = result.data.moTa || '';
            selectTrangThai.value = String(result.data.trangThai ?? 1);
            openCatModal('edit');
        } catch (err) {
            alert('Lỗi kết nối: ' + err.message);
        }
    });
});

document.querySelectorAll('.delete-btn').forEach((button) => {
    button.addEventListener('click', async () => {
        const id = button.dataset.id;
        const row = button.closest('tr');
        const name = row?.children[3]?.textContent?.trim() || 'danh mục này';

        if (!confirm(`Bạn có chắc muốn xóa "${name}"?`)) {
            return;
        }

        try {
            button.disabled = true;
            const response = await fetch(`/DanhMuc/XoaDanhMuc?id=${encodeURIComponent(id)}`, {
                method: 'POST'
            });
            const result = await response.json();

            if (result.success) {
                alert('Xóa thành công!');
                location.reload();
            } else {
                alert(result.message || 'Không xóa được danh mục.');
                button.disabled = false;
            }
        } catch (err) {
            button.disabled = false;
            alert('Lỗi kết nối: ' + err.message);
        }
    });
});
