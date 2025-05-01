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
  // Kiểm tra dữ liệu đầu vào
  if (!regId || typeof status !== "boolean") {
    Swal.fire("Lỗi", "Dữ liệu không hợp lệ", "error");
    return;
  }

  // Sửa URL cho đúng định dạng
  const url = `/Organization/HomeOrg/UpdateStatus?regId=${encodeURIComponent(
    regId
  )}&status=${status}`;

  fetch(url)
    .then((response) => {
      if (!response.ok) throw new Error(`Lỗi HTTP! Status: ${response.status}`);
      return response.json();
    })
    .then((data) => {
      if (data.success) {
        Swal.fire(
          "Thành công",
          "Cập nhật trạng thái thành công",
          "success"
        ).then(() => window.location.reload());
      } else {
        throw new Error(data.message || "Cập nhật thất bại");
      }
    })
    .catch((error) => {
      console.error("Chi tiết lỗi:", error);
      Swal.fire("Lỗi", error.message || "Có lỗi xảy ra khi cập nhật", "error");
    });
}
// Hàm hiển thị modal chi tiết tình nguyện viên
// Định nghĩa hàm trước khi sử dụng
// Đặt đoạn này TRƯỚC khi nút được click
