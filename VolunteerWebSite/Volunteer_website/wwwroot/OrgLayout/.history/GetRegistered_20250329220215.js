// Confirmation dialog for approve/reject actions
function confirmAction(regId, actionType) {
  const actionText = actionType === "approve" ? "approve" : "reject";
  const status = actionType === "approve"; // true for approve, false for reject
  const actionColor = actionType === "approve" ? "#28a745" : "#dc3545";

  Swal.fire({
    title: `Are you sure to ${actionText} this registration?`,
    text: "This action cannot be undone!",
    icon: "warning",
    showCancelButton: true,
    confirmButtonColor: actionColor,
    cancelButtonColor: "#6c757d",
    confirmButtonText: `Yes, ${actionText} it!`,
    cancelButtonText: "Cancel",
  }).then((result) => {
    if (result.isConfirmed) {
      updateRegistrationStatus(regId, status);
    }
  });
}

function updateRegistrationStatus(regId, status) {
  // Create correct URL using Url.Action
  const baseUrl =
    '@Url.Action("UpdateStatus", "HomeOrg", new { area = "Organization" })';
  const url = `${baseUrl}?regId=${encodeURIComponent(regId)}&status=${status}`;

  fetch(url, {
    method: "GET",
  })
    .then((response) => {
      if (!response.ok) {
        throw new Error(response.statusText);
      }
      return response.json();
    })
    .then((data) => {
      if (data.success) {
        Swal.fire({
          title: "Success!",
          text: data.message || "Status updated successfully",
          icon: "success",
          confirmButtonText: "OK",
        }).then(() => {
          window.location.reload();
        });
      } else {
        throw new Error(data.message || "Failed to update status");
      }
    })
    .catch((error) => {
      console.error("Error:", error);
      Swal.fire({
        title: "Error!",
        text: error.message || "An error occurred while updating status",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
}

// Auto-close alerts after 5 seconds
$(document).ready(function () {
  setTimeout(function () {
    $(".alert")
      .fadeTo(500, 0)
      .slideUp(500, function () {
        $(this).remove();
      });
  }, 5000);
});
