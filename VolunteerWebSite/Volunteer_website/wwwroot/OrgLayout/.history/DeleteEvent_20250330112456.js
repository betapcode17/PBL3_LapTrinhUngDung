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
  // Kiểm tra dữ liệu đầu vào
  if (!eventId) {
    Swal.fire("Error", "Invalid event ID", "error");
    return;
  }

  const url = `/Organization/HomeOrg/DeleteEvent?id=${encodeURIComponent(
    eventId
  )}`;

  fetch(url, {
    method: "GET",
  })
    .then((response) => {
      if (!response.ok)
        throw new Error(`HTTP error! Status: ${response.status}`);
      return response.json();
    })
    .then((data) => {
      if (data.success) {
        Swal.fire("Deleted!", "Your event has been deleted.", "success").then(
          () => {
            window.location.reload(); // Reload page after success
          }
        );
      } else {
        throw new Error(data.message || "Failed to delete event");
      }
    })
    .catch((error) => {
      console.error("Error:", error);
      Swal.fire("Error!", error.message || "Could not delete event", "error");
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
