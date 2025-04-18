function showVolunteerDetails(volunteerId) {
  console.log("Đang tải thông tin tình nguyện viên:", volunteerId);

  // Hiển thị loading
  Swal.fire({
    title: "Đang tải...",
    allowOutsideClick: false,
    didOpen: () => Swal.showLoading(),
  });

  fetch(`/Organization/HomeOrg/GetVolunteerDetails?id=${volunteerId}`)
    .then((response) => {
      if (!response.ok) throw new Error("Lỗi kết nối");
      return response.json();
    })
    .then((data) => {
      Swal.close();
      if (data.success) {
        // Hiển thị modal
        new bootstrap.Modal("#volunteerModal").show();
      } else {
        throw new Error(data.message || "Lỗi khi tải dữ liệu");
      }
    })
    .catch((error) => {
      Swal.fire("Lỗi", error.message, "error");
    });
}
