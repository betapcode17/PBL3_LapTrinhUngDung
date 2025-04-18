function confirmDelete(eventId) {
  let deleteUrl = `/Organization/HomeOrg/DeleteEvent?id=${eventId}`;
  document.getElementById("confirmDeleteBtn").setAttribute("href", deleteUrl);
  let deleteModal = new bootstrap.Modal(document.getElementById("deleteModal"));
  deleteModal.show();
}
