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
