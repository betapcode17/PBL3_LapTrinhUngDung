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
async function showVolunteerDetails(volunteerId) {
  try {
    // Hiển thị loading
    Swal.fire({
      title: "Đang tải...",
      allowOutsideClick: false,
      didOpen: () => Swal.showLoading(),
    });

    const response = await fetch(
      `/api/volunteers/GetVolunteerDetails?id=${volunteerId}`
    );
    const result = await response.json();

    if (!result.success) {
      throw new Error(result.message);
    }

    const volunteer = result.data;

    // Đổ dữ liệu vào modal
    document.getElementById(
      "volunteerId"
    ).textContent = `ID: ${volunteer.VolunteerId}`;
    document.getElementById("volunteerName").textContent =
      volunteer.Name || "Chưa cập nhật";
    document.getElementById("volunteerEmail").textContent =
      volunteer.Email || "Chưa cập nhật";
    document.getElementById("volunteerPhone").textContent =
      volunteer.PhoneNumber || "Chưa cập nhật";
    document.getElementById("volunteerDob").textContent =
      volunteer.DateOfBirth || "Chưa cập nhật";
    document.getElementById("volunteerGender").textContent =
      volunteer.Gender || "Chưa cập nhật";
    document.getElementById("volunteerAddress").textContent =
      volunteer.Address || "Chưa cập nhật";

    // Xử lý ảnh đại diện
    if (volunteer.ImagePath) {
      document.getElementById(
        "volunteerImage"
      ).src = `/uploads/${volunteer.ImagePath}`;
    }

    // Đóng loading và hiển thị modal
    Swal.close();
    new bootstrap.Modal(document.getElementById("volunteerModal")).show();
  } catch (error) {
    Swal.fire("Lỗi", error.message, "error");
  }
}
