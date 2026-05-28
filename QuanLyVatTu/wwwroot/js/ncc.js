const supModal = document.getElementById('addSupplierModal');
const btnOpenSup = document.getElementById('btnOpenModal');
const btnCloseSup = document.getElementById('btnCloseModal');
const btnCancelSup = document.getElementById('btnCancel');
const btnSaveSup = document.getElementById('btnSave');

// Mở Modal
btnOpenSup.onclick = () => supModal.classList.add('show');

// Đóng Modal
const closeSupModal = () => supModal.classList.remove('show');
btnCloseSup.onclick = closeSupModal;
btnCancelSup.onclick = closeSupModal;
window.onclick = (e) => { if (e.target == supModal) closeSupModal(); }

// Xử lý nút lưu (tạm thời)
btnSaveSup.onclick = () => {
    alert("Form giao diện đã hoàn tất! Chờ team Back-end kết nối Database nhé.");
    closeSupModal();
}