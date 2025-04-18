function showEventDetails(eventId) {
  $.ajax({
    url: "/Organization/HomeOrg/GetEventDetails",
    type: "GET",
    data: { id: eventId },
    success: function (response) {
      if (response && response.success && response.data) {
        const data = response.data;
        const baseUrl = window.location.origin; // Lấy base URL của website

        // Hàm kiểm tra và sửa đường dẫn ảnh
        const getImageUrl = (imgPath) => {
          if (!imgPath) return null;

          // Nếu đã là URL đầy đủ
          if (imgPath.startsWith("http")) return imgPath;

          // Nếu bắt đầu bằng /
          if (imgPath.startsWith("/")) return baseUrl + imgPath;

          // Trường hợp khác
          return baseUrl + "/uploads/events/" + imgPath;
        };

        // Format dates
        const formatDate = (dateStr) => {
          if (!dateStr) return "N/A";
          const date = new Date(dateStr);
          return (
            date.toLocaleDateString("vi-VN") +
            " " +
            date.toLocaleTimeString("vi-VN", {
              hour: "2-digit",
              minute: "2-digit",
            })
          );
        };

        // Cập nhật thông tin
        $("#eventId").val(data.eventId || "N/A");
        $("#eventName").val(data.name || "N/A");
        $("#eventDescription").val(data.description || "N/A");
        $("#eventLocation").val(data.location || "N/A");
        $("#eventStartDate").val(formatDate(data.dayBegin));
        $("#eventEndDate").val(formatDate(data.dayEnd));
        $("#eventTargetMembers").val(
          (data.targetMember?.toLocaleString() || "0") + " người"
        );
        $("#eventTargetFunds").val(
          data.targetFunds
            ? data.targetFunds.toLocaleString() + " VND"
            : "0 VND"
        );
        $("#eventType").val(data.type_event_name || "N/A");
        $("#eventOrganization").val(data.organizationName || "N/A");
        $("#eventStatus").val(data.status ? "Đang hoạt động" : "Đã kết thúc");

        // Cập nhật thống kê
        $("#eventRegistrationCount").val(
          data.registrationCount?.toLocaleString() || "0"
        );
        $("#eventDonationCount").val(
          data.donationCount?.toLocaleString() || "0"
        );
        $("#eventTotalAmount").val(
          data.totalAmount
            ? data.totalAmount.toLocaleString() + " VND"
            : "0 VND"
        );

        // Cập nhật ảnh chính
        const mainImageUrl = getImageUrl(data.imagePath);
        if (mainImageUrl) {
          $("#eventMainImage")
            .attr("src", mainImageUrl)
            .on("error", function () {
              $(this).attr(
                "src",
                "https://via.placeholder.com/600x400?text=Ảnh+không+tồn+tại"
              );
            });
        } else {
          $("#eventMainImage").attr(
            "src",
            "https://via.placeholder.com/600x400?text=Không+có+ảnh"
          );
        }

        // Cập nhật gallery ảnh
        const gallery = $("#eventImageGallery");
        gallery.empty();

        const images = data.listImg ? data.listImg.split(",") : [];
        if (images.length > 0) {
          images.forEach((img) => {
            const imgUrl = getImageUrl(img.trim());
            if (imgUrl) {
              gallery.append(`
                <div class="col-4 col-md-3 mb-2">
                  <img src="${imgUrl}" class="img-thumbnail" style="height: 80px; width: 100%; object-fit: cover;" 
                       onerror="this.src='https://via.placeholder.com/100x80?text=Ảnh+lỗi'"
                       onclick="$('#eventMainImage').attr('src', '${imgUrl}')">
                </div>
              `);
            }
          });

          if (gallery.children().length === 0) {
            gallery.append(
              '<div class="col-12 text-muted">Không có ảnh nào khác</div>'
            );
          }
        } else {
          gallery.append(
            '<div class="col-12 text-muted">Không có ảnh nào khác</div>'
          );
        }

        // Hiển thị modal
        var modal = new bootstrap.Modal(
          document.getElementById("eventDetailsModal")
        );
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
    },
  });
}
