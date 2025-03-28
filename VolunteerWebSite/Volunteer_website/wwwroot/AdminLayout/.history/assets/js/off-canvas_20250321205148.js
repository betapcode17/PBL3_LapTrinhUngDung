document.addEventListener("DOMContentLoaded", function () {
  const toggleButton = document.querySelector('[data-toggle="offcanvas"]');
  const sidebar = document.getElementById("sidebar"); // Chọn sidebar bằng ID

  toggleButton.addEventListener("click", function () {
    sidebar.classList.toggle("sidebar-icon-only"); // Thêm hoặc xóa class
  });
});
