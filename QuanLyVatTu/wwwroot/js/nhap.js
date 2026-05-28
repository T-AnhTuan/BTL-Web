const receiptModal = document.getElementById('addReceiptModal');
const btnOpenReceipt = document.getElementById('btnOpenModal');
const btnCloseReceipt = document.getElementById('btnCloseModal');
const btnCancelReceipt = document.getElementById('btnCancel');
const btnSaveReceipt = document.getElementById('btnSave');

btnOpenReceipt.onclick = () => receiptModal.classList.add('show');

const closeReceiptModal = () => receiptModal.classList.remove('show');
btnCloseReceipt.onclick = closeReceiptModal;
btnCancelReceipt.onclick = closeReceiptModal;

window.onclick = (e) => { if (e.target == receiptModal) closeReceiptModal(); }

btnSaveReceipt.onclick = () => {
    alert("Khung giao diện tạo Phiếu Nhập đã xong! Chờ Back-end gắn logic lưu Header và chuyển trang thêm chi tiết.");
    closeReceiptModal();
}