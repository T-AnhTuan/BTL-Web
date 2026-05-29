// 1. Script mở Menu Thông Báo
function toggleNoti(event) {
    event.stopPropagation(); // Không cho click văng ra ngoài
    var notiMenu = document.getElementById("notiMenu");
    var profileMenu = document.getElementById("profileMenu");

    // Nếu User Profile đang mở thì đóng nó lại cho đỡ vướng
    if (profileMenu.classList.contains('show')) {
        profileMenu.classList.remove('show');
    }

    notiMenu.classList.toggle("show");
}

// 2. Script mở Menu User Profile
function toggleDropdown(event) {
    event.stopPropagation(); // Không cho click văng ra ngoài
    var profileMenu = document.getElementById("profileMenu");
    var notiMenu = document.getElementById("notiMenu");

    // Nếu Bảng Thông báo đang mở thì đóng nó lại
    if (notiMenu.classList.contains('show')) {
        notiMenu.classList.remove('show');
    }

    profileMenu.classList.toggle("show");
}

// 3. Đóng tất cả các bảng menu xổ xuống nếu người dùng bấm ra khoảng trắng
window.onclick = function (event) {
    // Đóng menu User
    if (!event.target.closest('.user-dropdown')) {
        var pMenu = document.getElementById("profileMenu");
        if (pMenu && pMenu.classList.contains('show')) pMenu.classList.remove('show');
    }

    // Đóng menu Thông báo (Trừ khi người dùng đang bấm chọn gì đó bên trong rọt menu)
    if (!event.target.closest('.notification-wrapper') && !event.target.closest('.notification-dropdown')) {
        var nMenu = document.getElementById("notiMenu");
        if (nMenu && nMenu.classList.contains('show')) nMenu.classList.remove('show');
    }
}