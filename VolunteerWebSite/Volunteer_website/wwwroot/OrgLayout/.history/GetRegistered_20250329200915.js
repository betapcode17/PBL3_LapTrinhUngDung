// Confirmation dialog for approve/reject actions
function confirmAction(regId, status, actionType) {
  const actionText = actionType === "approve" ? "approve" : "reject";
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
      // Submit the form
      window.location.href = `@Url.Action("UpdateStatus")?regId=${regId}&status=${status}`;
    }
  });
}

// Auto-close alerts after 5 seconds
window.setTimeout(function () {
  $(".alert")
    .fadeTo(500, 0)
    .slideUp(500, function () {
      $(this).remove();
    });
}, 5000);
