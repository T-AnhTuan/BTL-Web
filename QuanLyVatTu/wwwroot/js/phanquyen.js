// Bọc toàn bộ code trong DOMContentLoaded để đảm bảo HTML đã tải xong hết mới chạy JS
document.addEventListener('DOMContentLoaded', function () {

    // =========================================================
    // 1. XỬ LÝ CHỨC NĂNG "TOÀN QUYỀN" (CHỌN TẤT CẢ)
    // =========================================================

    // Bước 1: Lấy danh sách tất cả các thẻ <tr> (Các hàng) nằm trong phần thân của bảng (tbody)
    const rows = document.querySelectorAll('.permissions-table tbody tr');

    // Bước 2: Dùng vòng lặp forEach duyệt qua từng hàng một
    rows.forEach(row => {
        // Trên hàng hiện tại đang xét, đi tìm cái Checkbox có class 'chk-all-row' (Ô Toàn quyền)
        const checkAllBtn = row.querySelector('.chk-all-row');

        // Nếu hàng này không có ô Toàn quyền (ví dụ hàng báo lỗi trống), thì thoát khỏi vòng lặp, bỏ qua
        if (!checkAllBtn) return;

        // Trên cùng hàng đó, tìm TẤT CẢ các ô Checkbox có class 'chk-child' (Đó là 4 ô: Xem, Thêm, Sửa, Xóa)
        const childChecks = row.querySelectorAll('.chk-child');

        // Bước 3: Tạo một hàm nhỏ tên là 'evaluateCheckAll' để đánh giá xem có nên bật ô Toàn quyền hay không
        const evaluateCheckAll = () => {
            // Hàm every() sẽ đi hỏi 4 ô con: "Có phải tất cả các em đều đang được tích (.checked) không?". Trả về true/false
            const allChecked = Array.from(childChecks).every(c => c.checked);
            // Lấy kết quả true/false đó gán cho ô Toàn Quyền
            checkAllBtn.checked = allChecked;
        };

        // Vừa mở trang lên, gọi hàm này chạy 1 lần để máy tính kiểm tra ngay dữ liệu từ Database
        evaluateCheckAll();

        // Bước 4: Bắt sự kiện khi người dùng DÙNG CHUỘT BẤM VÀO ô "Toàn quyền"
        checkAllBtn.addEventListener('change', function () {
            // Lấy trạng thái của ô Toàn quyền lúc này (đang tích hay bị gỡ tích)
            const isChecked = this.checked;

            // Đi bắt 4 ô con, ép chúng nó phải có trạng thái giống y hệt ô Toàn quyền
            childChecks.forEach(chk => {
                chk.checked = isChecked;
            });
        });

        // Bước 5: Bắt sự kiện khi người dùng DÙNG CHUỘT BẤM VÀO 1 ô con bất kỳ (Xem, Thêm, Sửa hoặc Xóa)
        childChecks.forEach(chk => {
            // Cứ mỗi khi ô con bị thay đổi, lại gọi hàm 'evaluateCheckAll' để xét xem có đủ 4 ô chưa, nếu đủ thì tự bật ô Toàn quyền
            chk.addEventListener('change', evaluateCheckAll);
        });
    });


    // =========================================================
    // 2. XỬ LÝ POPUP (MODAL) THÊM VAI TRÒ
    // =========================================================

    // Tìm các thẻ HTML của Modal dựa vào ID
    const roleModal = document.getElementById('addRoleModal');
    const btnOpenRole = document.getElementById('btnOpenRoleModal'); // Nút "Thêm vai trò" ở ngoài giao diện
    const btnCloseRole = document.getElementById('btnCloseRoleModal'); // Nút (X) góc trên
    const btnCancelRole = document.getElementById('btnCancelRole'); // Nút "Hủy Bỏ"
    const btnSaveRole = document.getElementById('btnSaveRole'); // Nút "Lưu Vai Trò"

    // Hàm Mở Modal: Gán display = 'flex' để hộp thoại hiện ra giữa màn hình
    if (btnOpenRole) {
        btnOpenRole.addEventListener('click', function () {
            roleModal.style.display = 'flex';
        });
    }

    // Hàm Đóng Modal: Gán display = 'none' để giấu hộp thoại đi
    const closeRoleModal = () => {
        if (roleModal) roleModal.style.display = 'none';
    };

    // Gắn sự kiện click cho 2 nút Đóng và Hủy để gọi hàm giấu hộp thoại
    if (btnCloseRole) btnCloseRole.addEventListener('click', closeRoleModal);
    if (btnCancelRole) btnCancelRole.addEventListener('click', closeRoleModal);

    // Sự kiện đặc biệt: Đóng modal khi user bấm chuột ra ngoài vùng viền đen tối của Modal
    window.addEventListener('click', function (e) {
        if (e.target == roleModal) {
            closeRoleModal();
        }
    });

    // Sự kiện: Bấm nút "Lưu Vai Trò"
    if (btnSaveRole) {
        btnSaveRole.addEventListener('click', function () {
            // Lấy dữ liệu người dùng nhập ở ô Tên vai trò
            const newRoleName = document.getElementById('newRoleName').value.trim();

            // Kiểm tra rỗng
            if (newRoleName === "") {
                alert("Vui lòng nhập tên vai trò!");
                return;
            }

            // Tạm thời thông báo, tính năng gửi Ajax lưu vào Database sẽ phát triển sau
            alert("Tính năng thêm mới Vai trò đang được nâng cấp!");
            closeRoleModal();
        });
    }
});