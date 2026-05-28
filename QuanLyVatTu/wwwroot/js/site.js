// 1. BIỂU ĐỒ ĐƯỜNG (XU HƯỚNG NHẬP XUẤT)
const ctxTrend = document.getElementById('trendChart').getContext('2d');
const trendChart = new Chart(ctxTrend, {
    type: 'line',
    data: {
        // BACK-END: Truyền mảng các ngày trong tháng vào đây (VD: ['01/02', '02/02', ...])
        labels: ['(Trống)', '(Trống)', '(Trống)', '(Trống)', '(Trống)'],
        datasets: [
            {
                label: 'Nhập Kho',
                // BACK-END: Truyền mảng số lượng nhập tương ứng vào đây
                data: [0, 0, 0, 0, 0],
                borderColor: '#3b82f6', // Màu xanh dương
                backgroundColor: 'rgba(59, 130, 246, 0.1)',
                fill: true,
                tension: 0.4
            },
            {
                label: 'Xuất Kho',
                // BACK-END: Truyền mảng số lượng xuất tương ứng vào đây
                data: [0, 0, 0, 0, 0],
                borderColor: '#10b981', // Màu xanh lá
                backgroundColor: 'rgba(16, 185, 129, 0.1)',
                fill: true,
                tension: 0.4
            }
        ]
    },
    options: {
        responsive: true,
        plugins: { legend: { position: 'top' } },
        scales: { y: { beginAtZero: true } }
    }
});

// 2. BIỂU ĐỒ TRÒN (CƠ CẤU DANH MỤC)
const ctxCategory = document.getElementById('categoryChart').getContext('2d');
const categoryChart = new Chart(ctxCategory, {
    type: 'pie',
    data: {
        // BACK-END: Truyền danh sách tên các danh mục vào đây
        labels: ['Chưa có dữ liệu'],
        datasets: [{
            // BACK-END: Truyền số lượng/giá trị tồn kho theo danh mục vào đây
            data: [100], // Để 100% màu xám cho biểu đồ trống
            backgroundColor: ['#e2e8f0', '#3b82f6', '#f59e0b', '#10b981', '#6366f1'],
            borderWidth: 1
        }]
    },
    options: {
        responsive: true,
        plugins: {
            legend: { position: 'right' }
        }
    }
});