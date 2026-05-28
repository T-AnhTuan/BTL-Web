const exportModal = document.getElementById('addExportModal');
const btnOpenExport = document.getElementById('btnOpenModal');
const btnCloseExport = document.getElementById('btnCloseModal');
const btnCancelExport = document.getElementById('btnCancel');
// Chọn form
const formXuat = exportModal.querySelector('form');

// Mở Modal
btnOpenExport.addEventListener('click', () => exportModal.classList.add('show'));

// Đóng Modal
const closeExportModal = () => {
    exportModal.classList.remove('show');
    formXuat.reset();
};
btnCloseExport.addEventListener('click', closeExportModal);
btnCancelExport.addEventListener('click', closeExportModal);

// Bấm ra ngoài khoảng đen để đóng
window.addEventListener('click', (e) => {
    if (e.target == exportModal) closeExportModal();
});

// Xử lý Submit Form
formXuat.addEventListener('submit', function (e) {
    const ngayXuat = formXuat.querySelector('[name="NgayXuat"]').value;
    const khachHang = formXuat.querySelector('[name="KhachHang"]').value;
    const kho = formXuat.querySelector('[name="KhoId"]').value;

    if (!ngayXuat || !khachHang || !kho) {
        e.preventDefault();
        alert("Vui lòng điền đầy đủ các thông tin bắt buộc (*)");
        return false;
    }

    const btnSubmit = formXuat.querySelector('button[type="submit"]');
    btnSubmit.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang xử lý...';
    btnSubmit.disabled = true;
});
