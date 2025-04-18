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

  // Hàm tải thông tin chi tiết
  function loadVolunteerDetails(volunteerId) {
    $.get("/Organization/GetVolunteerDetails?id=" + volunteerId)
      .done(function (data) {
        // Cập nhật thông tin cơ bản
        $("#volunteerName").text(data.name || "--");
        $("#volunteerEmail").text(data.email || "--");
        $("#volunteerPhone").text(data.phoneNumber || "--");
        $("#volunteerAddress").text(data.address || "--");

        // Cập nhật ảnh
        if (data.imagePath) {
          $("#volunteerImage").attr("src", data.imagePath);
        }

        // Cập nhật giới tính
        if (data.gender !== null) {
          $("#volunteerGender").text(data.gender ? "Nam" : "Nữ");
        } else {
          $("#volunteerGender").text("--");
        }

        // Cập nhật tuổi
        if (data.age) {
          $("#volunteerAge").text(data.age + " tuổi");
        } else {
          $("#volunteerAge").text("--");
        }

        // Tải lịch sử tham gia
        loadEventHistory(volunteerId);
      })
      .fail(function () {
        toastr.error("Không thể tải thông tin tình nguyện viên");
      });
  }

  // Hàm tải lịch sử sự kiện
  function loadEventHistory(volunteerId) {
    $.get("/Organization/GetVolunteerEvents?id=" + volunteerId)
      .done(function (events) {
        var tbody = $("#eventHistory");
        tbody.empty();

        if (events.length > 0) {
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
        toastr.error("Không thể tải lịch sử sự kiện");
      });
  }
  function resetVolunteerInfo() {
    $(
      "#volunteerName, #volunteerEmail, #volunteerPhone, #volunteerAddress"
    ).text("--");
    $("#volunteerGender, #volunteerAge").text("--");
    $("#volunteerImage").attr("src", "/default-profile.png");
    $("#eventHistory").html(`
            <tr>
                <td colspan="3" class="text-center no-data">Chưa có dữ liệu</td>
            </tr>
        `);
  }
});
