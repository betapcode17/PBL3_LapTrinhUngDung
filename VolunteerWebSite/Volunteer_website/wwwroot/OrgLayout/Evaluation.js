$(document).ready(function () {
  // Xử lý khi chọn tình nguyện viên
  $("#volunteerSelect").change(function () {
    var volunteerId = $(this).val();
    if (volunteerId) {
      loadVolunteerDetails(volunteerId);
    } else {
      resetVolunteerInfo();
    }
  });

  function loadVolunteerDetails(volunteerId) {
    $.get("/Organization/Evaluation/GetVolunteerDetails?id=" + volunteerId)
      .done(function (data) {
        // Cập nhật thông tin
        $("#volunteerName").text(data.name || "--");
        $("#volunteerEmail").text(data.email || "--");
        $("#volunteerPhone").text(data.phoneNumber || "--");

        // Cập nhật ảnh
        $("#volunteerImage").attr(
          "src",
          data.imagePath
            ? "~/" + data.imagePath
            : "~/images/DefaultImg/default-person.jpg"
        );

        // Cập nhật giới tính và tuổi
        $("#volunteerGender").text(
          data.gender ? "Nam" : data.gender === false ? "Nữ" : "--"
        );
        $("#volunteerAge").text(data.age ? data.age + " tuổi" : "--");

        // Tải lịch sử tham gia
        loadEventHistory(volunteerId);
      })
      .fail(function () {
        console.error("Không thể tải thông tin tình nguyện viên");
      });
  }

  function resetVolunteerInfo() {
    $("#volunteerName, #volunteerEmail, #volunteerPhone").text("--");
    $("#volunteerGender, #volunteerAge").text("--");
    $("#volunteerImage").attr("src", "/OrgLayout/assets/images/pic-1.jpg");
    $("#eventHistory").html(`
            <tr>
                <td colspan="3" class="text-center no-data">Chưa có dữ liệu</td>
            </tr>
        `);
  }
});
$(document).ready(function () {
  // Lắng nghe sự kiện thay đổi trên dropdown Volunteer
  $("#volunteerSelect").change(function () {
    var volunteerId = $(this).val();
    var eventSelect = $("#eventSelect");

    // Xóa các option hiện tại
    eventSelect.find("option:not(:first)").remove();

    if (volunteerId) {
      $.ajax({
        url: "/Organization/Evaluation/GetEventsByVolunteer",
        type: "GET",
        data: { volunteerId: volunteerId },
        success: function (data) {
          if (data && data.length > 0) {
            $.each(data, function (index, item) {
              eventSelect.append(
                `<option value="${item.regId}" 
                               data-event-id="${item.eventId}"
                               data-event-name="${item.eventName}">
                                  ${item.eventName} (Reg: ${item.regId})
                              </option>`
              );
            });
          } else {
            eventSelect.append(
              '<option value="" disabled>Không có sự kiện nào</option>'
            );
          }
        },
        error: function () {
          alert("Lỗi khi tải sự kiện. Vui lòng thử lại.");
        },
      });
    }
  });

  // Lắng nghe sự kiện thay đổi trên dropdown Event
  $("#eventSelect").change(function () {
    var selectedOption = $(this).find("option:selected");
    var regId = selectedOption.val();
    var eventId = selectedOption.data("event-id");
    var eventName = selectedOption.data("event-name");

    // Cập nhật các trường ẩn hoặc hiển thị thông tin
    $("#RegId").val(regId);
    $("#EventId").val(eventId);

    // Có thể cập nhật các thông tin khác nếu cần
    console.log(
      "Selected RegID:",
      regId,
      "EventID:",
      eventId,
      "EventName:",
      eventName
    );
  });
});
