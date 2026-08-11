// Sidebar menu: prevent navigation for placeholder links
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.app-menu-link[href="#"]').forEach(function (link) {
        link.addEventListener('click', function (e) {
            e.preventDefault();
        });
    });
});
