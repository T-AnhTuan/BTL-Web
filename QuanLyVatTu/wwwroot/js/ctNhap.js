// =========================================================================
// HÀM 1: HOÀN TẤT LƯU PHIẾU (GỬI LÊN SERVER)
// =========================================================================
function hoanTatPhieu() {
    let arrChiTiet = [];

    document.querySelectorAll('#chiTietBody tr:not(#emptyRow)').forEach(row => {
        let idVatTu = row.querySelector('.td-vattu').getAttribute('data-id');
        let soLuong = row.querySelector('.num-soluong').innerText.replace(/,/g, ''); // Fix nếu có dấu phẩy
        let donGia = row.querySelector('.num-dongia').getAttribute('data-value');

        arrChiTiet.push({
            VatTuId: parseInt(idVatTu),
            SoLuong: parseInt(soLuong),
            DonGia: parseFloat(donGia)
        });
    });

    if (arrChiTiet.length === 0) {
        alert("Lưới rỗng! Vui lòng thêm vật tư xuống lưới trước khi lưu.");
        return;
    }

    const hdfId = document.getElementById('hdfPhieuNhapId');
    const txtMa = document.getElementById('txtMaPhieu');
    const hdfNgay = document.getElementById('hdfNgayNhap');
    const hdfKho = document.getElementById('hdfKhoId');
    const hdfNcc = document.getElementById('hdfNhaCungCapId');
    const hdfNote = document.getElementById('hdfGhiChu');

    if (!hdfId || !txtMa) {
        alert("Lỗi Giao diện: Không tìm thấy thẻ ẩn chứa Mã Phiếu!");
        return;
    }

    const payload = {
        Id: parseInt(hdfId.value) || 0,
        MaPhieu: txtMa.value,
        NgayNhap: hdfNgay.value,
        KhoId: parseInt(hdfKho.value),
        NhaCungCapId: parseInt(hdfNcc.value),
        GhiChu: hdfNote ? hdfNote.value : "",
        ChiTiet: arrChiTiet
    };

    const btnLuu = document.querySelector('button[onclick="hoanTatPhieu()"]');
    const oldText = btnLuu.innerHTML;
    btnLuu.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Đang lưu...';
    btnLuu.disabled = true;

    fetch('/PhieuNhap/LuuPhieuNhapHoanChinh', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    })
        .then(response => response.json())
        .then(res => {
            if (res.success) {
                alert(res.message);
                window.location.href = '/PhieuNhap/PhieuNhap';
            } else {
                alert(res.message);
                btnLuu.innerHTML = oldText;
                btnLuu.disabled = false;
            }
        })
        .catch(err => {
            alert("Lỗi kết nối máy chủ! Vui lòng thử lại.");
            btnLuu.innerHTML = oldText;
            btnLuu.disabled = false;
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
            const donViTinh = selectedOption.getAttribute('data-dvt') || "---"; // SỬA Ở ĐÂY: Lấy biến ĐVT
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