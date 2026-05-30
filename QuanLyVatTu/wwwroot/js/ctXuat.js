// --- Simple non-blocking toast ---
function showToast(message, type = 'info', timeout = 3000) {
    let container = document.getElementById('toastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toastContainer';
        Object.assign(container.style, {
            position: 'fixed', right: '16px', bottom: '16px', zIndex: 9999,
            display: 'flex', flexDirection: 'column', gap: '8px'
        });
        document.body.appendChild(container);
    }
    const toast = document.createElement('div');
    toast.innerText = message;
    Object.assign(toast.style, {
        padding: '10px 14px', borderRadius: '6px', color: '#fff',
        boxShadow: '0 2px 8px rgba(0,0,0,0.15)', fontSize: '13px',
        maxWidth: '360px', wordBreak: 'break-word', opacity: '1', transition: 'opacity 220ms'
    });
    toast.style.background = type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#333';
    container.appendChild(toast);
    setTimeout(() => { toast.style.opacity = '0'; setTimeout(() => toast.remove(), 240); }, timeout);
}

// --- Helper: chuẩn hoá số ---
function normalizeNumber(s) {
    if (s == null) return '';
    let t = String(s).trim().replace(/\s+/g, '');
    if (t.indexOf('.') !== -1 && t.indexOf(',') !== -1) t = t.replace(/\./g, '').replace(',', '.');
    else if (t.indexOf(',') !== -1 && t.indexOf('.') === -1) t = t.replace(',', '.');
    t = t.replace(/[^0-9.\-]/g, '');
    return t;
}

// --- Build chi tiết từ DOM (dùng chung) ---
function buildArrChiTiet() {
    const rows = Array.from(document.querySelectorAll('#chiTietBody tr'));
    const arr = [];
    rows.forEach(row => {
        if (row.id === 'emptyRow') return;

        let tdVatTu = row.querySelector('.td-vattu') || row.querySelector('.td-mavat') || null;
        if (!tdVatTu) tdVatTu = Array.from(row.children).find(c => c.hasAttribute('data-id') || c.querySelector('[data-id]')) || null;
        if (!tdVatTu) return;

        let idVatTu = tdVatTu.getAttribute('data-id') || (tdVatTu.dataset && tdVatTu.dataset.id) || '';
        if (!idVatTu) {
            const child = tdVatTu.querySelector('[data-id]');
            if (child) idVatTu = child.getAttribute('data-id') || (child.dataset && child.dataset.id) || '';
        }
        if (!idVatTu) return;

        const tdSoLuong = row.querySelector('.num-soluong') || row.querySelector('[data-field="soluong"]') || null;
        const tdDonGia = row.querySelector('.num-dongia') || row.querySelector('.num-thanhtien') || row.querySelector('[data-field="dongia"]') || null;

        const getText = el => el ? (el.getAttribute('data-value') || el.innerText || el.value || '') : '';
        const soLuong = parseInt(normalizeNumber(getText(tdSoLuong)) || '0', 10) || 0;
        const donGia = parseFloat(normalizeNumber(getText(tdDonGia)) || '0') || 0;

        arr.push({ VatTuId: Number(idVatTu), SoLuong: soLuong, DonGia: donGia });
    });
    return arr;
}

// ==========================================
// HÀM TÍNH TỔNG TIỀN TỰ ĐỘNG
// ==========================================
function tinhTongTien() {
    const tbody = document.getElementById('chiTietBody');
    if (!tbody) return;

    const rows = tbody.querySelectorAll('tr');
    let tongMatHang = 0;
    let tongSoLuong = 0;
    let tongTien = 0;

    rows.forEach(tr => {
        if (tr.id === 'emptyRow') return;

        let inputSL = tr.querySelector('input[type="hidden"].so-luong') || tr.querySelector('input.so-luong');
        let inputGia = tr.querySelector('input[type="hidden"].don-gia') || tr.querySelector('input.don-gia');

        if (inputSL && inputGia) {
            let strSL = String(inputSL.value || "0");
            let strGia = String(inputGia.value || "0");

            const sl = parseFloat(strSL.replace(/,/g, '')) || 0;
            const gia = parseFloat(strGia.replace(/,/g, '')) || 0;

            if (sl > 0) tongMatHang += 1;
            tongSoLuong += sl;
            tongTien += (sl * gia);
        }
    });

    const lblMatHang = document.getElementById('lblTongMatHang');
    const lblSoLuong = document.getElementById('lblTongSoLuong');
    const lblTongTien = document.getElementById('lblTongTien');

    if (lblMatHang) lblMatHang.innerText = tongMatHang;
    if (lblSoLuong) lblSoLuong.innerText = tongSoLuong.toLocaleString('vi-VN');
    if (lblTongTien) lblTongTien.innerText = tongTien.toLocaleString('vi-VN') + ' đ';
}


// ==========================================
// HÀM LƯU TOÀN BỘ PHIẾU (ĐÃ FIX ÉP KIỂU JSON)
// ==========================================
function hoanTatPhieu() {
    const khoId = document.getElementById('hdfKhoId')?.value;
    const kh = document.getElementById('hdfKhachHang')?.value;
    let ngayXuat = document.getElementById('hdfNgayXuat')?.value;
    const ghiChu = document.getElementById('hdfLyDoXuat')?.value || "";

    if (!khoId || !kh || !ngayXuat) {
        showToast('Lỗi: Không tìm thấy thông tin Header (Kho, Khách hàng, Ngày)!', 'error');
        return;
    }

    // --- FIX LỖI "One or more validation errors" ---
    let datePart = ngayXuat.split(' ')[0];
    if (datePart.includes('/')) {
        const parts = datePart.split('/');
        if (parts.length === 3) {
            ngayXuat = `${parts[2]}-${parts[1]}-${parts[0]}`;
        }
    } else {
        ngayXuat = datePart;
    }
    // -----------------------------------------------

    const chiTiets = [];
    const tbody = document.getElementById('chiTietBody');
    const rows = tbody ? tbody.querySelectorAll('tr') : [];
    let hasError = false;

    rows.forEach((tr, index) => {
        if (tr.id === 'emptyRow') return;

        let vatTuId = tr.getAttribute('data-id');
        if (!vatTuId) {
            const hiddenId = tr.querySelector('input[type="hidden"].vat-tu-id');
            if (hiddenId) vatTuId = hiddenId.value;
        }

        let sl = 0;
        let gia = 0;

        let inputSL = tr.querySelector('input[type="hidden"].so-luong') || tr.querySelector('input.so-luong');
        let inputGia = tr.querySelector('input[type="hidden"].don-gia') || tr.querySelector('input.don-gia');

        if (inputSL && inputGia) {
            sl = parseFloat(String(inputSL.value || "0").replace(/,/g, ''));
            gia = parseFloat(String(inputGia.value || "0").replace(/,/g, ''));
        } else {
            const tdSL = tr.querySelector('.num-soluong');
            const tdGia = tr.querySelector('.num-dongia');
            if (tdSL && tdGia) {
                sl = parseFloat(tdSL.getAttribute('data-value') || "0");
                gia = parseFloat(tdGia.getAttribute('data-value') || "0");
            }
        }

        if (vatTuId && sl > 0 && gia >= 0) {
            chiTiets.push({
                VatTuId: parseInt(vatTuId),
                SoLuong: Math.round(sl),
                DonGia: gia
            });
        } else {
            hasError = true;
        }
    });

    if (chiTiets.length === 0) {
        showToast('Lỗi: Lưới không có vật tư hoặc dữ liệu bị trống!', 'error');
        return;
    }

    if (hasError) {
        showToast('Số lượng và Đơn giá phải lớn hơn 0!', 'error');
        return;
    }

    const payload = {
        KhoId: parseInt(khoId),
        KhachHang: kh,
        NgayXuat: ngayXuat,
        LyDoXuat: ghiChu,
        ChiTiets: chiTiets
    };

    console.log("Chuẩn bị gửi:", payload);

    fetch('/PhieuXuat/LuuToanBoPhieu', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    })
        .then(async response => {
            const text = await response.text();
            let data;
            try {
                data = JSON.parse(text);
            } catch (err) {
                throw new Error(`Mã lỗi Server ${response.status}: Vui lòng kiểm tra Console (F12)`);
            }

            if (!response.ok) {
                let errMsg = data.message || data.title || 'Lỗi dữ liệu từ Server!';
                if (data.errors) {
                    const validationErrors = Object.values(data.errors).flat().join('; ');
                    errMsg += '\nChi tiết: ' + validationErrors;
                }
                throw new Error(errMsg);
            }
            return data;
        })
        .then(data => {
            if (data.success) {
                showToast('Lưu toàn bộ phiếu thành công!', 'success');
                setTimeout(() => {
                    if (data.redirectUrl) window.location.href = data.redirectUrl;
                    else window.location.href = '/PhieuXuat/';
                }, 1500);
            } else {
                showToast(data.message || 'Lỗi logic C#!', 'error');
            }
        })
        .catch(err => {
            console.error('Lỗi Fetch:', err);
            showToast(err.message, 'error');
        });
}

// =========================================================================
// KHI TRÌNH DUYỆT TẢI XONG TRANG
// =========================================================================
document.addEventListener('DOMContentLoaded', function () {
    const btn = document.getElementById('btnSave');
    if (btn) {
        btn.addEventListener('click', hoanTatPhieu);
    }
    tinhTongTien();

    const cboVatTu = document.getElementById('cboVatTu');
    const txtGia = document.getElementById('txtDonGia'); // Ô nhập đơn giá
    const txtSL = document.getElementById('txtSoLuong'); // Ô nhập số lượng
    const btnThemVatTu = document.getElementById('btnThemVatTu');
    const tbody = document.getElementById('chiTietBody');

    // --- TÍNH NĂNG ĐƯỢC CHÈN VÀO: TỰ ĐỘNG LẤY GIÁ VỐN KHI CHỌN VẬT TƯ ---
    if (cboVatTu && txtGia) {
        cboVatTu.addEventListener('change', function () {
            const vatTuId = this.value;

            if (!vatTuId) {
                txtGia.value = '';
                txtGia.placeholder = "Tự lấy giá vốn...";
                return;
            }

            // Đổi placeholder báo hiệu đang tải
            txtGia.value = '';
            txtGia.placeholder = "Đang tải giá...";

            // Gọi API lên C# để lấy giá vốn
            fetch(`/VatTu/GetGiaVon?id=${vatTuId}`)
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        txtGia.value = data.giaVon.toLocaleString('vi-VN')+ ' VND'; // Gắn số tiền vào ô Đơn giá
                    } else {
                        txtGia.value = '0';
                        showToast(data.message || 'Không lấy được giá vốn', 'error');
                    }
                })
                .catch(err => {
                    console.error("Lỗi lấy giá:", err);
                    txtGia.value = '0';
                    showToast('Lỗi kết nối khi lấy giá vốn!', 'error');
                });
        });
    }
    // ------------------------------------------------------------------

    if (btnThemVatTu) {
        // Lắng nghe sự kiện click vào nút Thêm Vật Tư
        btnThemVatTu.addEventListener('click', function (e) {
            e.preventDefault();

            // Cắt bỏ khoảng trắng dư thừa ở 2 đầu chữ
            const sl = txtSL.value.trim();
            const gia = txtGia.value.trim();

            // Kiểm tra: Nếu chưa chọn vật tư hoặc chưa nhập đủ số liệu thì cảnh báo và dừng lại
            if (!cboVatTu.value || sl === '' || gia === '') {
                showToast("Vui lòng chọn vật tư, nhập đủ số lượng và chờ lấy đơn giá!", 'error');
                return;
            }

            // ĐỌC DỮ LIỆU TỪ DROPDOWN VẬT TƯ
            const selectedOption = cboVatTu.options[cboVatTu.selectedIndex];
            const vatTuId = cboVatTu.value;
            const maVatTu = selectedOption.getAttribute('data-code') || "---";
            const donViTinh = selectedOption.getAttribute('data-dvt') || "---";
            const tenVatTu = selectedOption.text;

            const cleanSoLuong = sl.replace(/\./g, '').replace(',', '.').replace(/[^0-9]/g, '');
            const cleanGia = gia.replace(/\./g, '').replace(',', '.').replace(/[^0-9]/g, '');
            // Ép kiểu chữ thành số nguyên (Số lượng) và số thực (Đơn giá)
            const soLuong = parseInt(cleanSoLuong)||0;
            const donGia = parseFloat(cleanGia)||0;
            // Tính thành tiền
            const thanhTien = soLuong * donGia;

            // Xóa dòng chữ "Chưa có vật tư nào..." nếu nó đang tồn tại
            const emptyRow = document.getElementById('emptyRow');
            if (emptyRow) emptyRow.remove();

            // Tính số thứ tự tự động dựa trên số dòng đang có trong bảng
            const stt = tbody.querySelectorAll('tr').length + 1;

            // Tạo ra một thẻ <tr> (dòng) mới
            const tr = document.createElement('tr');
            tr.setAttribute('data-id', vatTuId);

            tr.innerHTML = `              
                <td class="text-center">${stt}</td>              
                <td>${maVatTu}</td>              
                <td class="td-vattu">${tenVatTu}</td>              
                <td class="text-center">${donViTinh}</td>              
                <td class="text-center num-soluong">
                    ${soLuong.toLocaleString('vi-VN')}
                    <input type="hidden" class="so-luong" value="${soLuong}" />
                </td>              
                <td class="text-center num-dongia">
                    ${donGia.toLocaleString('vi-VN')} ₫
                    <input type="hidden" class="don-gia" value="${donGia}" />
                </td>              
                <td class="text-center num-thanhtien" style="font-weight: 700; color: #e11d48;">
                    ${thanhTien.toLocaleString('vi-VN')} ₫
                </td>              
                <td class="text-center">                  
                    <button type="button" class="btn btn-danger btn-sm btn-delete-row" style="padding: 6px 10px;">                      
                        <i class="fa fa-trash"></i>                  
                    </button>              
                </td>          
            `;

            // Đẩy dòng <tr> vừa tạo xong vào trong bảng hiển thị
            tbody.appendChild(tr);

            // Gắn sự kiện Xóa cho cái nút thùng rác
            tr.querySelector('.btn-delete-row').addEventListener('click', function () {
                tr.remove();
                tinhTongTien();
                if (tbody.querySelectorAll('tr').length === 0) {
                    tbody.innerHTML = `<tr id="emptyRow"><td colspan="8" class="text-center" style="color: #94a3b8; font-style: italic; padding: 30px;">Chưa có vật tư nào trong phiếu. Vui lòng chọn vật tư và thêm xuống lưới!</td></tr>`;
                }
            });

            // Sau khi thêm xong, làm trống 2 ô nhập liệu
            txtSL.value = '';
            txtGia.value = '';

            // Focus lại cbo để người dùng dễ nhập tiếp
            cboVatTu.value = '';

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