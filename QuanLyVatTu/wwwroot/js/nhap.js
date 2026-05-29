const receiptModal = document.getElementById('addReceiptModal');
const btnOpenReceipt = document.getElementById('btnOpenModal');
const btnCloseReceipt = document.getElementById('btnCloseModal');
const btnCancelReceipt = document.getElementById('btnCancel');
// Chọn form bên trong modal
const formNhap = receiptModal ? receiptModal.querySelector('form') : null;

// ==========================================
// 1. XỬ LÝ LỌC TỰ ĐỘNG (AUTO FILTER)
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const filterForm = document.getElementById('filterForm');
    if (filterForm) {
        // Lấy tất cả các ô input và select trong form lọc
        const filterElements = filterForm.querySelectorAll('input, select');
        filterElements.forEach(element => {
            // Khi người dùng thay đổi giá trị (chọn ngày, chọn dropdown)
            element.addEventListener('change', function () {
                // Tự động submit form lên server
                filterForm.submit();
            });
        });
    }
});

// ==========================================
// 2. XỬ LÝ MODAL TẠO PHIẾU
// ==========================================
if (btnOpenReceipt && receiptModal) {
    // Mở Modal
    btnOpenReceipt.addEventListener('click', () => receiptModal.classList.add('show'));

    // Đóng Modal
    const closeReceiptModal = () => {
        receiptModal.classList.remove('show');
        if (formNhap) formNhap.reset(); // Xóa trắng dữ liệu khi đóng
    };

    if (btnCloseReceipt) btnCloseReceipt.addEventListener('click', closeReceiptModal);
    if (btnCancelReceipt) btnCancelReceipt.addEventListener('click', closeReceiptModal);

    // Bấm ra ngoài khoảng đen để đóng
    window.addEventListener('click', (e) => {
        if (e.target == receiptModal) closeReceiptModal();
    });

    // Khi submit form (Bấm nút Tiếp tục)
    if (formNhap) {
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
            if (btnSubmit) {
                btnSubmit.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang xử lý...';
                btnSubmit.disabled = true;
            }

            // Cho phép form tự động submit theo đường dẫn asp-action
        });
    }
}