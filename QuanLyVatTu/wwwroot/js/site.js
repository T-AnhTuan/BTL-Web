document.addEventListener("DOMContentLoaded", function () {
    try {
        // ==========================================
        // 1. VẼ BIỂU ĐỒ ĐƯỜNG (Chart.js)
        // ==========================================
        const canvasTrend = document.getElementById('trendChart');
        if (canvasTrend && typeof trendLabels !== 'undefined' && trendLabels.length > 0) {
            new Chart(canvasTrend.getContext('2d'), {
                type: 'bar',
                data: {
                    labels: trendLabels,
                    datasets: [
                        {
                            label: 'Nhập kho',
                            data: trendIn,
                          //  borderColor: '#28a745',
                            backgroundColor: '#28a745',
                            fill: true,
                            tension: 0.3
                        },
                        {
                            label: 'Xuất kho',
                            data: trendOut,
                            //borderColor: '#fd7e14',
                            backgroundColor: '#fd7e14',
                            fill: true,
                            tension: 0.3
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: { y: { beginAtZero: true } }
                }
            });
        }

        // ==========================================
        // 2. VẼ BIỂU ĐỒ TRÒN (Phân bổ danh mục) - THÊM MỚI
        // ==========================================
        const canvasCategory = document.getElementById('categoryChart');
        if (canvasCategory && typeof categoryLabels !== 'undefined' && categoryLabels.length > 0) {
            new Chart(canvasCategory.getContext('2d'), {
                type: 'doughnut', // Dạng vành khuyên nhìn hiện đại hơn
                data: {
                    labels: categoryLabels,
                    datasets: [{
                        data: categoryValues,
                        backgroundColor: [
                            '#0d6efd', // Xanh dương
                            '#198754', // Xanh lá
                            '#ffc107', // Vàng
                            '#dc3545', // Đỏ
                            '#6f42c1', // Tím
                            '#0dcaf0'  // Xanh nhạt
                        ],
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom', // Đưa chú thích xuống dưới cho cân đối
                        }
                    }
                }
            });
        }
        // ==========================================
        // 2. HIỂN THỊ CẢNH BÁO TỒN KHO
        // ==========================================
        const tbodyLowStock = document.getElementById('lowStockBody');
        if (tbodyLowStock && typeof lowStockData !== 'undefined') {
            if (lowStockData.length > 0) {
                tbodyLowStock.innerHTML = lowStockData.map(item => `
                    <tr>
                        <td>${item.MaVatTu || 'N/A'}</td>
                        <td>${item.TenVatTu || 'N/A'}</td>
                        <td style="color: #dc3545; font-weight: bold;">${item.SoLuong || 0}</td>
                        <td>${item.DinhMuc || 0}</td>
                        <td class="text-center"><span style="background: #dc3545; color: white; padding: 3px 8px; border-radius: 4px; font-size: 12px;">Sắp hết</span></td>
                    </tr>
                `).join('');
            } else {
                tbodyLowStock.innerHTML = `<tr><td colspan="5" style="text-align: center; color: #198754; padding: 20px;">Kho đang an toàn, không có vật tư sắp hết!</td></tr>`;
            }
        }

        // ==========================================
        // 3. HIỂN THỊ THÔNG BÁO HOẠT ĐỘNG
        // ==========================================
        const tbodyActivity = document.getElementById('recentActivityBody');
        if (tbodyActivity && typeof activityData !== 'undefined') {
            if (activityData.length > 0) {
                tbodyActivity.innerHTML = activityData.map(item => {
                    const timeStr = new Date(item.ThoiGian).toLocaleString('vi-VN');
                    const badgeColor = item.Loai === "Nhập Kho" ? "#198754" : (item.Loai === "Xuất Kho" ? "#dc3545" : "#0dcaf0");

                    return `
                        <tr>
                            <td style="color: #6c757d; font-size: 13px;">${timeStr}</td>
                            <td><span style="background: ${badgeColor}; color: white; padding: 3px 8px; border-radius: 4px; font-size: 12px;">${item.Loai}</span></td>
                            <td>${item.NoiDung}</td>
                            <td><i class="fas fa-user-circle text-muted"></i> ${item.NguoiThucHien || 'Hệ thống'}</td>
                        </tr>`;
                }).join('');
            } else {
                tbodyActivity.innerHTML = `<tr><td colspan="4" style="text-align: center; color: #6c757d; padding: 20px;">Chưa có hoạt động nào gần đây.</td></tr>`;
            }
        }

    } catch (error) {
        console.error("Lỗi khi load dữ liệu Dashboard:", error);
    }
});