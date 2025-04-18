// Confirmation dialog for delete event
function confirmDelete(eventId) {
  Swal.fire({
    title: "Are you sure to delete this event?",
    text: "You won't be able to revert this!",
    icon: "warning",
    showCancelButton: true,
    confirmButtonColor: "#dc3545",
    cancelButtonColor: "#6c757d",
    confirmButtonText: "Yes, delete it!",
    cancelButtonText: "Cancel",
  }).then((result) => {
    if (result.isConfirmed) {
      deleteEvent(eventId);
    }
  });
}
function deleteEvent(eventId) {
  fetch(`/Organization/HomeOrg/DeleteEvent?id=${encodeURIComponent(eventId)}`, {
    method: "POST", // Luôn dùng POST cho hành động xóa
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: document.querySelector(
        'input[name="__RequestVerificationToken"]'
      ).value,
    },
  })
    .then(async (response) => {
      const contentType = response.headers.get("content-type");

      // Kiểm tra nếu response là JSON
      if (contentType && contentType.includes("application/json")) {
        return response.json();
      }

      // Nếu không phải JSON (thường là HTML)
      const text = await response.text();
      throw new Error("Unexpected response from server");
    })
    .then((data) => {
      if (data && data.success) {
        showSuccessToast("Event deleted successfully");
        setTimeout(() => window.location.reload(), 1500);
      } else {
        throw new Error(data?.message || "Delete failed");
      }
    })
    .catch((error) => {
      console.error("Delete error:", error);
      showErrorToast(error.message || "Cannot delete event. Please try again.");
    });
}
// Hiển thị toast thông báo (có thể sử dụng thay cho SweetAlert nếu muốn)
function showToast(type, message) {
  const toastElement = document.getElementById(
    type === "success" ? "successToast" : "errorToast"
  );

  // Cập nhật nội dung toast nếu cần
  if (message) {
    const toastBody = toastElement.querySelector(".toast-body");
    if (toastBody) {
      toastBody.textContent = message;
    }
  }

  const toast = new bootstrap.Toast(toastElement);
  toast.show();
}
