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
          data.imagePath || "/default-profile.png"
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

  function loadEventHistory(volunteerId) {
    $.get("/Organization/Evaluation/GetVolunteerEvents?id=" + volunteerId)
      .done(function (events) {
        var tbody = $("#eventHistory");
        tbody.empty();

        if (events && events.length > 0) {
          events.forEach(function (evt) {
            tbody.append(`
                            <tr>
                                <td>${evt.name || "--"}</td>
                                <td>${
                                  evt.startDate
                                    ? new Date(
                                        evt.startDate
                                      ).toLocaleDateString()
                                    : "--"
                                }</td>
                                <td><span class="badge badge-success">Hoàn thành</span></td>
                            </tr>
                        `);
          });
        } else {
          tbody.append(`
                        <tr>
                            <td colspan="3" class="text-center no-data">Chưa tham gia sự kiện nào</td>
                        </tr>
                    `);
        }
      })
      .fail(function () {
        console.error("Không thể tải lịch sử sự kiện");
      });
  }

  function resetVolunteerInfo() {
    $("#volunteerName, #volunteerEmail, #volunteerPhone").text("--");
    $("#volunteerGender, #volunteerAge").text("--");
    $("#volunteerImage").attr("src", "/default-profile.png");
    $("#eventHistory").html(`
            <tr>
                <td colspan="3" class="text-center no-data">Chưa có dữ liệu</td>
            </tr>
        `);
  }
});
// Path: Event.js
$(document).ready(function () {
  // Lắng nghe sự kiện thay đổi trên dropdown Volunteer
  $("#volunteerSelect").change(function () {
    var volunteerId = $(this).val(); // Lấy VolunteerId từ dropdown
    var eventSelect = $("#eventSelect");

    // Xóa các option hiện tại trong dropdown Event (trừ option mặc định)
    eventSelect.find("option:not(:first)").remove();

    if (volunteerId) {
      // Gửi yêu cầu AJAX để lấy danh sách sự kiện
      $.ajax({
        url: "/Organization/Evaluation/GetEventsByVolunteer", // URL của action
        type: "GET",
        data: { volunteerId: volunteerId },
        success: function (data) {
          // Thêm các sự kiện vào dropdown
          if (data && data.length > 0) {
            $.each(data, function (index, evt) {
              eventSelect.append(
                `<option value="${evt.eventId}">
                                          ${evt.name} - ${
                  evt.dayBegin || "Not specified"
                }
                                      </option>`
              );
            });
          } else {
            eventSelect.append(
              '<option value="" disabled>No events available</option>'
            );
          }
        },
        error: function () {
          alert("Error loading events. Please try again.");
        },
      });
    }
  });
});
// Lắng nghe sự kiện thay đổi trên dropdown Event
