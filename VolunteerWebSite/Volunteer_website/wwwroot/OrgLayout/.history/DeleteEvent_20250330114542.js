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
