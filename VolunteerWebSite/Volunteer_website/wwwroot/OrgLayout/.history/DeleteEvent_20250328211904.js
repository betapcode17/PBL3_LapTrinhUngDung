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

document
  .getElementById("confirmDeleteBtn")
  .addEventListener("click", function (event) {
    event.preventDefault();

    let isSuccess = Math.random() > 0.5;
    showToast(isSuccess ? "success" : "error");
  });
