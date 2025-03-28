document.addEventListener("DOMContentLoaded", function () {
  const toggleButton = document.querySelector('[data-toggle="offcanvas"]');
  const body = document.body; // Chọn body để thêm class

  toggleButton.addEventListener("click", function () {
    body.classList.toggle("sidebar-icon-only");
  });
});
