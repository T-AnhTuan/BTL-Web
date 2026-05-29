// =========================================================================
// HÀM: HOÀN TẤT LƯU TOÀN BỘ PHIẾU (CẢ HEADER VÀ DETAILS)
// =========================================================================
function hoanTatPhieu() {
    // 1. Quét lưới để lấy danh sách chi tiết vật tư
    let arrChiTiet = [];
    document.querySelectorAll('#chiTietBody tr:not(#emptyRow)').forEach(row => {
        let tdVatTu = row.querySelector('.td-vattu');
        let tdSoLuong = row.querySelector('.num-soluong');
        let tdDonGia = row.querySelector('.num-dongia');

        if (tdVatTu && tdSoLuong && tdDonGia) {
            let idVatTu = tdVatTu.getAttribute('data-id');
            let soLuong = tdSoLuong.innerText.replace(/,/g, '').trim();
            let donGia = tdDonGia.getAttribute('data-value') || tdDonGia.innerText.replace(/,/g, '').trim();

            arrChiTiet.push({
                VatTuId: parseInt(idVatTu),
                SoLuong: parseInt(soLuong),
                DonGia: parseFloat(donGia)
            });
        }
    });
    
    if (arrChiTiet.length == 0) {
        alert("Lưới rỗng! Vui lòng thêm vật tư xuống lưới trước khi lưu.");
        return;
    }

    // 2. Lấy thông tin chung của phiếu (Header)
    // Lưu ý: Bạn cần kiểm tra xem ID các thẻ HTML này có khớp với file View của bạn không nhé!
    const txtMaPhieu = document.getElementById('txtMaPhieu') ? document.getElementById('txtMaPhieu').value : "";
    const txtNgayNhap = document.getElementById('txtNgayNhap') ? document.getElementById('txtNgayNhap').value : new Date().toISOString();
    const cboKho = document.getElementById('cboKhoId');
    const cboNhaCungCap = document.getElementById('cboNhaCungCapId');
    const txtGhiChu = document.getElementById('txtGhiChu');

    if (!cboKho || !cboKho.value || !cboNhaCungCap || !cboNhaCungCap.value) {
        alert("Vui lòng chọn đầy đủ Kho nhập và Nhà cung cấp ở phần thông tin chung!");
        return;
    }

    // 3. Đóng gói toàn bộ thành 1 cục dữ liệu (Khớp với PhieuNhapToanBoDto bên C#)
    const payload = {
        MaPhieu: txtMaPhieu,
        NgayNhap: txtNgayNhap,
        KhoId: parseInt(cboKho.value),
        NhaCungCapId: parseInt(cboNhaCungCap.value),
        GhiChu: txtGhiChu ? txtGhiChu.value : "",
        ChiTiets: arrChiTiet
    };

    // 4. Gửi một lần duy nhất lên Server
    fetch('/PhieuNhap/LuuToanBoPhieu', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
    })
        .then(response => {
            if (!response.ok) throw new Error("Lỗi mạng hoặc Server từ chối!");
            return response.json();
        })
        .then(data => {
            if (data.success) {
                // Chuyển hướng về trang danh sách nếu thành công
                window.location.href = data.redirectUrl || '/PhieuNhap/PhieuNhap';
            } else {
                alert("Lưu thất bại: " + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert("Có lỗi xảy ra khi lưu phiếu. Vui lòng xem Console (F12) để biết chi tiết.");
        });
}
// =========================================================================
// HÀM 2: TÍNH TỔNG TIỀN VÀ SỐ LƯỢNG MẶT HÀNG
// =========================================================================
function tinhTongTien() {
    let tongSL = 0, tongTien = 0, tongMatHang = 0;

    document.querySelectorAll('#chiTietBody tr:not(#emptyRow)').forEach(row => {
        tongMatHang++;
        let slText = row.querySelector('.num-soluong')?.innerText.replace(/,/g, '') || "0";
        tongSL += parseInt(slText);

        let tienThuc = row.querySelector('.num-thanhtien')?.getAttribute('data-value') || "0";
        tongTien += parseFloat(tienThuc);
    });

    const lblMH = document.getElementById('lblTongMatHang');
    const lblSL = document.getElementById('lblTongSoLuong');
    const lblTien = document.getElementById('lblTongTien');

    if (lblMH) lblMH.innerText = tongMatHang;
    if (lblSL) lblSL.innerText = tongSL.toLocaleString();
    if (lblTien) lblTien.innerText = tongTien.toLocaleString() + ' ₫';
}

// =========================================================================
// KHI TRÌNH DUYỆT TẢI XONG TRANG
// =========================================================================
document.addEventListener('DOMContentLoaded', function () {

    tinhTongTien();

    const btnThemVatTu = document.getElementById('btnThemVatTu');

    if (btnThemVatTu) {
        btnThemVatTu.addEventListener('click', function (e) {
            e.preventDefault();

            const cbo = document.getElementById('cboVatTu');
            const txtSL = document.getElementById('txtSoLuong');
            const txtGia = document.getElementById('txtDonGia');
            const tbody = document.getElementById('chiTietBody');

            const sl = txtSL.value.trim();
            const gia = txtGia.value.trim();

            if (!cbo.value || sl === '' || gia === '') {
                alert("Vui lòng chọn vật tư, nhập đủ số lượng và đơn giá!");
                return;
            }

            // ĐỌC DỮ LIỆU TỪ DROPDOWN VẬT TƯ
            const selectedOption = cbo.options[cbo.selectedIndex];
            const vatTuId = cbo.value;
            const maVatTu = selectedOption.getAttribute('data-code') || "---";
            const donViTinh = selectedOption.getAttribute('data-dvt') || "---"; 
            const tenVatTu = selectedOption.text;

            const soLuong = parseInt(sl);
            const donGia = parseFloat(gia);
            const thanhTien = soLuong * donGia;

            const emptyRow = document.getElementById('emptyRow');
            if (emptyRow) emptyRow.remove();

            const stt = tbody.querySelectorAll('tr').length + 1;
            const tr = document.createElement('tr');

            tr.innerHTML = `
                <td class="text-center">${stt}</td>
                <td>${maVatTu}</td>
                <td class="td-vattu" data-id="${vatTuId}">${tenVatTu}</td>
                <td class="text-center">${donViTinh}</td>
                <td class="text-center num-soluong">${soLuong.toLocaleString()}</td>
                <td class="text-center num-dongia" data-value="${donGia}">${donGia.toLocaleString()} ₫</td>
                <td class="text-center num-thanhtien" data-value="${thanhTien}" style="font-weight: 700; color: #e11d48;">${thanhTien.toLocaleString()} ₫</td>
                <td class="text-center">
                    <button type="button" class="btn btn-danger btn-sm btn-delete-row" style="padding: 6px 10px;">
                        <i class="fa fa-trash"></i>
                    </button>
                </td>
            `;

            tbody.appendChild(tr);

            tr.querySelector('.btn-delete-row').addEventListener('click', function () {
                tr.remove();
                tinhTongTien();
                if (tbody.querySelectorAll('tr').length === 0) {
                    tbody.innerHTML = `<tr id="emptyRow"><td colspan="8" class="text-center" style="color: #94a3b8; font-style: italic; padding: 30px;">Chưa có vật tư nào trong phiếu. Vui lòng chọn vật tư và thêm xuống lưới!</td></tr>`;
                }
            });

            txtSL.value = '';
            txtGia.value = '';
            tinhTongTien();
        });
    }

    // Sự kiện xóa cho các dòng có sẵn
    document.querySelectorAll('.btn-delete-row').forEach(btn => {
        btn.addEventListener('click', function () {
            this.closest('tr').remove();
            tinhTongTien();
            const tbody = document.getElementById('chiTietBody');
            if (tbody && tbody.querySelectorAll('tr').length === 0) {
                tbody.innerHTML = `<tr id="emptyRow"><td colspan="8" class="text-center" style="color: #94a3b8; font-style: italic; padding: 30px;">Chưa có vật tư nào trong phiếu. Vui lòng chọn vật tư và thêm xuống lưới!</td></tr>`;
            }
        });
    });
});