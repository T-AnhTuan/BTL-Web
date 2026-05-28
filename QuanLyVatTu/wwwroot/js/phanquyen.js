// 1. Xử lý đổi tên Vai trò khi click
const roleItems = document.querySelectorAll('.role-list li');
const roleTitleAlert = document.querySelector('.info-alert strong');

roleItems.forEach(item => {
    item.addEventListener('click', function () {
        roleItems.forEach(li => li.classList.remove('active'));
        this.classList.add('active');
        roleTitleAlert.innerText = this.innerText;
    });
});

// 2. Xử lý nút Lưu Cấu Hình Phân Quyền
document.getElementById('btnSaveConfig').addEventListener('click', function () {
    alert("Đã gửi yêu cầu lưu phân quyền! ");
});

// 3. Xử lý Popup Thêm Vai Trò
const roleModal = document.getElementById('addRoleModal');
const btnOpenRole = document.getElementById('btnOpenRoleModal');
const btnCloseRole = document.getElementById('btnCloseRoleModal');
const btnCancelRole = document.getElementById('btnCancelRole');
const btnSaveRole = document.getElementById('btnSaveRole');

// Mở Modal
btnOpenRole.onclick = () => roleModal.classList.add('show');

// Đóng Modal
const closeRoleModal = () => roleModal.classList.remove('show');
btnCloseRole.onclick = closeRoleModal;
btnCancelRole.onclick = closeRoleModal;
window.onclick = (e) => { if (e.target == roleModal) closeRoleModal(); }

// Bấm lưu Vai trò
btnSaveRole.onclick = () => {

    closeRoleModal();
}