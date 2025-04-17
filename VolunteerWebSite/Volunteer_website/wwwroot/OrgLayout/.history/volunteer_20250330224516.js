function showVolunteerDetails(regId) {
  $.ajax({
    url: "/Organization/HomeOrg/GetVolunteerDetails",
    type: "GET",
    data: { id: regId },
    success: function (response) {
      if (response && response.success) {
        if (response.data) {
          let data = response.data;
          let content = `
              <p><strong>ID:</strong> ${data.volunteerId || "N/A"}</p>
              <p><strong>Name:</strong> ${data.name || "N/A"}</p>
              <p><strong>Email:</strong> ${data.email || "N/A"}</p>
              <p><strong>Phone:</strong> ${data.phoneNumber || "N/A"}</p>
              <p><strong>Date of Birth:</strong> ${
                data.dateOfBirth || "N/A"
              }</p>
              <p><strong>Gender:</strong> ${data.gender || "N/A"}</p>
              <p><strong>Address:</strong> ${data.address || "N/A"}</p>
            `;

          if (data.imagePath) {
            content += `<img src="${data.imagePath}" alt="Volunteer Image" class="img-fluid" style="max-width: 200px;">`;
          }

          $("#volunteerDetailsContent").html(content);
          $("#volunteerDetailsModal").modal("show");
        } else {
          Swal.fire({
            icon: "error",
            title: "No Data",
            text: "Volunteer details not available",
          });
        }
      } else {
        Swal.fire({
          icon: "error",
          title: "Error",
          text: response.message || "Invalid response from server",
        });
      }
    },
    error: function (xhr) {
      Swal.fire({
        icon: "error",
        title: "Error",
        text: xhr.responseJSON?.message || "Unable to fetch volunteer details",
      });
    },
  });
}
