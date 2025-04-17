function confirmDelete(eventId) {
  let deleteUrl = `/Organization/HomeOrg/DeleteEvent?id=${eventId}`;
  document.getElementById("confirmDeleteBtn").setAttribute("href", deleteUrl);
  let deleteModal = new bootstrap.Modal(document.getElementById("deleteModal"));
  deleteModal.show();
}

// Hiển thị thông báo sau khi xóa
function showToast(type) {
  let toastElement = document.getElementById(
    type === "success" ? "successToast" : "errorToast"
  );
  let toast = new bootstrap.Toast(toastElement);
  toast.show();
}

// Giả lập gọi khi xóa thành công (thực tế dùng AJAX hoặc kiểm tra response)
document
  .getElementById("confirmDeleteBtn")
  .addEventListener("click", function (event) {
    event.preventDefault(); // Ngăn chặn chuyển hướng để kiểm tra toast

    let isSuccess = Math.random() > 0.5; // Giả lập kết quả ngẫu nhiên
    showToast(isSuccess ? "success" : "error");
  });
