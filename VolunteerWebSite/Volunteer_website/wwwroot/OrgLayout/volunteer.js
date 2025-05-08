//tham số url có dạng /Areas/controller/action
function showVolunteerDetails(regId, url) {
  $.ajax({
    url: url,
    type: "GET",
    data: { id: regId },
    success: function (response) {
      if (response && response.success && response.data) {
        const data = response.data;

        // Cập nhật thông tin
        $("#volunteerId").text(`ID: ${data.volunteerId || "N/A"}`);
        $("#volunteerName").text(data.name || "N/A");
        $("#volunteerEmail").text(data.email || "N/A");
        $("#volunteerPhone").text(data.phoneNumber || "N/A");
        $("#volunteerDob").text(data.dateOfBirth || "N/A");
        $("#volunteerGender").text(data.gender || "N/A");
        $("#volunteerAddress").text(data.address || "N/A");

        // Cập nhật hình ảnh nếu có
        if (data.imagePath) {
          $("#volunteerImage").attr("src", data.imagePath);
        } else {
          $("#volunteerImage").attr(
            "src",
            "https://via.placeholder.com/200?text=No+Image"
          );
        }

        // Hiển thị modal
        $("#volunteerDetailsModal").modal("show");
      } else {
        Swal.fire({
          icon: "error",
          title: "Error",
          text: response.message || "Invalid response from server.",
        });
      }
    },
    error: function (xhr) {
      Swal.fire({
        icon: "error",
        title: "Error",
        text: xhr.responseJSON?.message || "Unable to fetch volunteer details.",
      });
    },
  });
}
