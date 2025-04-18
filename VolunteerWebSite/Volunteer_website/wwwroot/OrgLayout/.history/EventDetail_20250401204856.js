function showEventDetails(eventId) {
  $.ajax({
    url: "/Organization/HomeOrg/GetEventDetails", // Đảm bảo URL khớp với route của bạn
    type: "GET",
    data: { id: eventId },
    success: function (response) {
      if (response && response.success && response.data) {
        const data = response.data;

        // Cập nhật thông tin cơ bản
        $("#eventId").text(`ID: ${data.eventId || "N/A"}`);
        $("#eventName").text(data.name || "N/A");
        $("#eventDescription").text(data.description || "N/A");
        $("#eventLocation").text(data.location || "N/A");
        $("#eventStartDate").text(data.dayBegin || "N/A");
        $("#eventEndDate").text(data.dayEnd || "N/A");
        $("#eventTargetMembers").text(data.targetMember || "N/A");
        $("#eventTargetFunds").text(
          data.targetFunds?.toLocaleString() || "N/A"
        ); // Định dạng số
        $("#eventType").text(data.typeEventName || "N/A");
        $("#eventOrganization").text(data.organizationName || "N/A");
        $("#eventStatus").text(data.status ? "Active" : "Inactive");

        // Cập nhật thống kê
        $("#eventRegistrationCount").text(data.registrationCount || "0");
        $("#eventDonationCount").text(data.donationCount || "0");
        $("#eventTotalAmount").text(data.totalAmount?.toLocaleString() || "0"); // Định dạng số

        // Cập nhật hình ảnh
        if (data.imagePath) {
          $("#eventImage").attr("src", data.imagePath);
        } else {
          $("#eventImage").attr(
            "src",
            "https://via.placeholder.com/400x200?text=No+Event+Image"
          );
        }

        // Hiển thị modal
        $("#eventDetailsModal").modal("show");
      } else {
        Swal.fire({
          icon: "error",
          title: "Error",
          text: response.message || "Failed to load event details.",
        });
      }
    },
    error: function (xhr) {
      Swal.fire({
        icon: "error",
        title: "Error",
        text: xhr.responseJSON?.message || "Unable to fetch event details.",
      });
    },
  });
}
