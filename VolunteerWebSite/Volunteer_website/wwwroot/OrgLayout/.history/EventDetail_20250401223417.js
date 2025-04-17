function showEventDetails(eventId) {
  $.ajax({
    url: "/Organization/HomeOrg/GetEventDetails",
    type: "GET",
    data: { id: eventId },
    success: function (response) {
      if (response && response.success && response.data) {
        const data = response.data;
        
        // Format dates
        const formatDate = (dateStr) => {
          if (!dateStr) return "N/A";
          const date = new Date(dateStr);
          return date.toLocaleDateString('vi-VN') + ' ' + date.toLocaleTimeString('vi-VN', {hour: '2-digit', minute:'2-digit'});
        };

        // Cập nhật thông tin
        $("#eventId").val(data.eventId || "N/A");
        $("#eventName").val(data.name || "N/A");
        $("#eventDescription").val(data.description || "N/A");
        $("#eventLocation").val(data.location || "N/A");
        $("#eventStartDate").val(formatDate(data.dayBegin));
        $("#eventEndDate").val(formatDate(data.dayEnd));
        $("#eventTargetMembers").val((data.targetMember?.toLocaleString() || "0") + " người");
        $("#eventTargetFunds").val(data.targetFunds ? data.targetFunds.toLocaleString() + " VND" : "0 VND");
        $("#eventType").val(data.type_event_name || "N/A");
        $("#eventOrganization").val(data.organizationName || "N/A");
        $("#eventStatus").val(data.status ? "Đang hoạt động" : "Đã kết thúc");
        
        // Cập nhật thống kê
        $("#eventRegistrationCount").val(data.registrationCount?.toLocaleString() || "0");
        $("#eventDonationCount").val(data.donationCount?.toLocaleString() || "0");
        $("#eventTotalAmount").val(data.totalAmount ? data.totalAmount.toLocaleString() + " VND" : "0 VND");
        
        // Cập nhật ảnh chính
        if (data.imagePath) {
          $("#eventMainImage").attr("src", data.imagePath);
        } else {
          $("#eventMainImage").attr("src", "https://via.placeholder.com/600x400?text=No+Image");
        }
        
        // Cập nhật gallery ảnh
        const gallery = $("#eventImageGallery");
        gallery.empty();
        
        // Giả sử data.listImg là mảng các đường dẫn ảnh
        const images = data.listImg ? data.listImg.split(',') : [];
        if (images.length > 0) {
          images.forEach(img => {
            gallery.append(`
              <div class="col-4 col-md-3">
                <img src="${img.trim()}" class="img-thumbnail" style="height: 80px; width: 100%; object-fit: cover;" 
                     onclick="$('#eventMainImage').attr('src', '${img.trim()}')">
              </div>
            `);
          });
        } else {
          gallery.append('<p class="text-muted">Không có ảnh nào khác</p>');
        }
        
        // Hiển thị modal
        var modal = new bootstrap.Modal(document.getElementById('eventDetailsModal'));
        modal.show();
        
      } else {
        Swal.fire({
          icon: "error",
          title: "Lỗi",
          text: response.message || "Không thể tải chi tiết sự kiện.",
        });
      }
    },
    error: function (xhr) {
      Swal.fire({
        icon: "error",
        title: "Lỗi",
        text: xhr.responseJSON?.message || "Không thể kết nối đến server.",
      });
    }
  });
}