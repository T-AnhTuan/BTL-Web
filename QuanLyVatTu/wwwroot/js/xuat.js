const exportModal = document.getElementById('addExportModal');
const btnOpenExport = document.getElementById('btnOpenModal');
const btnCloseExport = document.getElementById('btnCloseModal');
const btnCancelExport = document.getElementById('btnCancel');
const btnSaveExport = document.getElementById('btnSave');

// Mở Modal
btnOpenExport.onclick = () => exportModal.classList.add('show');

// Đóng Modal
const closeExportModal = () => exportModal.classList.remove('show');
btnCloseExport.onclick = closeExportModal;
btnCancelExport.onclick = closeExportModal;

// Bấm ra ngoài khoảng đen để đóng
window.onclick = (e) => { if (e.target == exportModal) closeExportModal(); }

// Xử lý nút Tiếp Tục (Lưu)
btnSaveExport.onclick = () => {
    alert("Khung giao diện tạo Phiếu Xuất đã sẵn sàng! Chờ Dev Back-end nối Database.");
    closeExportModal();
}