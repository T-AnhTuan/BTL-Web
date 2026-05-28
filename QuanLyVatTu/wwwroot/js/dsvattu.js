const modal = document.getElementById('addModal');
const btnOpen = document.getElementById('btnOpenAddModal');
const btnClose = document.getElementById('btnCloseModal');
const btnCancel = document.getElementById('btnCancel');
const btnSave = document.getElementById('btnSave');
const tableBody = document.getElementById('vattuTableBody');

// Mở Modal
btnOpen.onclick = () => modal.classList.add('show');

// Đóng Modal
const closeModal = () => modal.classList.remove('show');
btnClose.onclick = closeModal;
btnCancel.onclick = closeModal;
window.onclick = (e) => { if (e.target == modal) closeModal(); }

// Xử lý nút Lưu và chèn dữ liệu động vào bảng
btnSave.onclick = function () {
    const maVatTu = document.getElementById('inputMaVatTu').value;
    const tenVatTu = document.getElementById('inputTenVatTu').value;
    const danhMucId = document.getElementById('inputDanhMucId').value;
    const donViTinh = document.getElementById('inputDonViTinh').value;
    const tonKhoHienTai = document.getElementById('inputTonKhoHienTai').value;
    const giaVonBinhQuan = document.getElementById('inputGiaVonBinhQuan').value;

    if (!maVatTu || !tenVatTu) {
        alert("Vui lòng nhập Mã và Tên vật tư!");
        return;
    }

    // Định dạng tiền tệ cho Giá Vốn
    const formattedGiaVon = new Intl.NumberFormat('vi-VN').format(giaVonBinhQuan) + " đ";

    // Chèn dòng HTML mới
    const newRow = document.createElement('tr');
    newRow.innerHTML = `
            <td>${maVatTu}</td>
            <td style="font-weight: 500;">${tenVatTu}</td>
            <td>${danhMucId}</td>
            <td>${donViTinh}</td>
            <td>${tonKhoHienTai}</td>
            <td class="rbac-note">${formattedGiaVon}</td>
            <td class="action-icons text-center">
                <i class="fa-solid fa-pen icon-edit"></i>
                <i class="fa-solid fa-trash icon-delete"></i>
            </td>
        `;
    tableBody.insertBefore(newRow, tableBody.firstChild);

    // Xóa form và đóng
    document.getElementById('inputMaVatTu').value = '';
    document.getElementById('inputTenVatTu').value = '';
    document.getElementById('inputDonViTinh').value = '';
    document.getElementById('inputTonKhoHienTai').value = '0';
    document.getElementById('inputGiaVonBinhQuan').value = '0';
    closeModal();
}