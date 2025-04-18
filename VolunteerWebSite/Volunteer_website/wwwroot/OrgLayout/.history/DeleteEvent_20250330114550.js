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
  // Kiểm tra tồn tại CSRF token trước khi sử dụng
  const tokenElement = document.querySelector(
    'input[name="__RequestVerificationToken"]'
  );

  if (!tokenElement) {
    console.error("CSRF token element not found");
    Swal.fire("Error", "Security token missing", "error");
    return;
  }

  fetch(`/Organization/HomeOrg/DeleteEvent?id=${encodeURIComponent(eventId)}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: tokenElement.value,
    },
  })
    .then(async (response) => {
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const contentType = response.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        return response.json();
      }
      throw new Error("Unexpected response format");
    })
    .then((data) => {
      if (data?.success) {
        showSuccessToast("Event deleted successfully");
        setTimeout(() => window.location.reload(), 1500);
      } else {
        throw new Error(data?.message || "Delete operation failed");
      }
    })
    .catch((error) => {
      console.error("Delete error:", error);
      showErrorToast(
        error.message || "Could not delete event. Please try again."
      );
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
