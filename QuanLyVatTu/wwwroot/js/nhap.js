const receiptModal = document.getElementById('addReceiptModal');
const btnOpenReceipt = document.getElementById('btnOpenModal');
const btnCloseReceipt = document.getElementById('btnCloseModal');
const btnCancelReceipt = document.getElementById('btnCancel');
// Chọn form bên trong modal
const formNhap = receiptModal.querySelector('form');

// Mở Modal
btnOpenReceipt.addEventListener('click', () => receiptModal.classList.add('show'));

// Đóng Modal
const closeReceiptModal = () => {
    receiptModal.classList.remove('show');
    formNhap.reset(); // Xóa trắng dữ liệu khi đóng
};
btnCloseReceipt.addEventListener('click', closeReceiptModal);
btnCancelReceipt.addEventListener('click', closeReceiptModal);

// Bấm ra ngoài khoảng đen để đóng
window.addEventListener('click', (e) => {
    if (e.target == receiptModal) closeReceiptModal();
});

// Khi submit form (Bấm nút Tiếp tục)
formNhap.addEventListener('submit', function (e) {
    // Client-side validation cơ bản
    const ngayNhap = formNhap.querySelector('[name="NgayNhap"]').value;
    const ncc = formNhap.querySelector('[name="NhaCungCapId"]').value;
    const kho = formNhap.querySelector('[name="KhoId"]').value;

    if (!ngayNhap || !ncc || !kho) {
        e.preventDefault(); // Chặn submit
        alert("Vui lòng điền đầy đủ thông tin bắt buộc (*)");
        return false;
    }

    // Đổi chữ nút thành "Đang xử lý..." để chống click 2 lần
    const btnSubmit = formNhap.querySelector('button[type="submit"]');
    btnSubmit.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang xử lý...';
    btnSubmit.disabled = true;

    // Form sẽ tự động post dữ liệu lên hàm TaoPhieuNhap trong Controller
});
