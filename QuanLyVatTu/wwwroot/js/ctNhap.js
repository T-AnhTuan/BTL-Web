document.addEventListener('DOMContentLoaded', function () {

    // Tính tổng tiền ngay khi vừa load trang
    tinhTongTien();

    // 1. XỬ LÝ SỰ KIỆN NÚT "THÊM VẬT TƯ" VÀO DATABASE
    const btnThemVatTu = document.getElementById('btnThemVatTu');

    if (btnThemVatTu) {
        btnThemVatTu.addEventListener('click', function (e) {
            e.preventDefault();

            // Lấy thông tin từ các ô input
            const phieuNhapId = document.getElementById('hdfPhieuNhapId').value;
            const selectVatTu = document.getElementById('cboVatTu');
            const txtSoLuong = document.getElementById('txtSoLuong');
            const txtDonGia = document.getElementById('txtDonGia');

            if (!selectVatTu.value || !txtSoLuong.value || !txtDonGia.value) {
                alert("Vui lòng nhập đầy đủ: Vật tư, Số lượng và Đơn giá!");
                return;
            }

            // Gói dữ liệu thành JSON để gửi lên Server
            const data = {
                PhieuNhapId: parseInt(phieuNhapId),
                VatTuId: parseInt(selectVatTu.value),
                SoLuong: parseInt(txtSoLuong.value),
                DonGia: parseFloat(txtDonGia.value)
            };

            // Gọi API lưu vào Database
            fetch('/NhapXuat/ThemChiTiet', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            })
                .then(response => response.json())
                .then(result => {
                    if (result.success) {
                        // Nếu lưu thành công, tải lại trang để Model vẽ lại bảng với dữ liệu mới
                        window.location.reload();
                    } else {
                        alert("Lỗi: " + result.message);
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    alert("Đã xảy ra lỗi khi kết nối tới máy chủ.");
                });
        });
    }

    // 2. XỬ LÝ SỰ KIỆN XÓA DÒNG CHI TIẾT TỪ DATABASE
    const deleteButtons = document.querySelectorAll('.btn-delete-row');
    deleteButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            if (confirm("Bạn có chắc chắn muốn xóa vật tư này khỏi phiếu nhập?")) {
                const chiTietId = this.getAttribute('data-id');

                fetch('/NhapXuat/XoaChiTiet/' + chiTietId, {
                    method: 'POST'
                })
                    .then(response => response.json())
                    .then(result => {
                        if (result.success) {
                            window.location.reload(); // Tải lại trang sau khi xóa
                        } else {
                            alert("Lỗi: " + result.message);
                        }
                    })
                    .catch(error => console.error('Error:', error));
            }
        });
    });

    // 3. HÀM TÍNH TOÁN TỔNG KẾT DƯỚI CHÂN BẢNG
    function tinhTongTien() {
        let tongSL = 0;
        let tongTien = 0;
        let soMatHang = 0;

        const rows = document.querySelectorAll('#chiTietBody tr:not(#emptyRow)');
        rows.forEach(row => {
            soMatHang++;

            // Lấy số lượng (Xóa dấu chấm phân cách ngàn)
            const slText = row.querySelector('.num-soluong').innerText.replace(/\./g, '');
            tongSL += parseInt(slText || 0);

            // Lấy thành tiền từ thuộc tính data-value
            const tien = parseFloat(row.querySelector('.num-thanhtien').getAttribute('data-value'));
            tongTien += tien;
        });

        document.getElementById('lblTongMatHang').innerText = soMatHang;
        document.getElementById('lblTongSoLuong').innerText = tongSL.toLocaleString('vi-VN');
        document.getElementById('lblTongTien').innerText = tongTien.toLocaleString('vi-VN') + ' ₫';
    }
});