document.getElementById("volunteerDetailsModal").focus();
function showVolunteerDetails(regId) {
  $.ajax({
    url: "/Organization/HomeOrg/GetVolunteerDetails",
    type: "GET",
    data: { id: regId },
    success: function (response) {
      // Kiểm tra xem response có thuộc tính success và data không
      if (response && response.success && response.data) {
        let data = response.data;
        let content = `
                    <p><strong>ID:</strong> ${data.VolunteerId || "N/A"}</p>
                    <p><strong>Name:</strong> ${data.Name || "N/A"}</p>
                    <p><strong>Email:</strong> ${data.Email || "N/A"}</p>
                    <p><strong>Phone:</strong> ${data.PhoneNumber || "N/A"}</p>
                    <p><strong>Date of Birth:</strong> ${
                      data.DateOfBirth || "N/A"
                    }</p>
                    <p><strong>Gender:</strong> ${data.Gender || "N/A"}</p>
                    <p><strong>Address:</strong> ${data.Address || "N/A"}</p>
                `;
        if (data.ImagePath) {
          content += `<img src="${data.ImagePath}" alt="Volunteer Image" class="img-fluid" style="max-width: 200px;">`;
        }

        $("#volunteerDetailsContent").html(content);
        $("#volunteerDetailsModal").modal("show");
      } else {
        // Nếu response không đúng định dạng, hiển thị lỗi
        Swal.fire({
          icon: "error",
          title: "Error",
          text: response.message || "Invalid response from server.",
        });
      }
    },
    error: function (xhr, status, error) {
      Swal.fire({
        icon: "error",
        title: "Error",
        text: "Unable to fetch volunteer details. Please try again.",
      });
    },
  });
}
