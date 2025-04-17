function showVolunteerDetails(volunteerId) {
  fetch(`/Organization/HomeOrg/GetVolunteerDetails?id=${volunteerId}`)
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        const volunteer = data.data;

        // Thông tin cơ bản
        document.getElementById("volunteerName").textContent =
          volunteer.Name || "Chưa cập nhật";
        document.getElementById(
          "volunteerId"
        ).textContent = `ID: ${volunteer.VolunteerId}`;
        document.getElementById("volunteerEmail").textContent =
          volunteer.Email || "Chưa cập nhật";
        document.getElementById("volunteerPhone").textContent =
          volunteer.PhoneNumber || "Chưa cập nhật";
        document.getElementById("volunteerAddress").textContent =
          volunteer.Address || "Chưa cập nhật";

        // Xử lý ảnh
        if (volunteer.ImagePath) {
          document.getElementById(
            "volunteerImage"
          ).src = `/uploads/${volunteer.ImagePath}`;
        }

        // Xử lý giới tính
        const genderBadge = document.getElementById("volunteerGender");
        if (volunteer.Gender !== null) {
          genderBadge.textContent = volunteer.Gender ? "Nam" : "Nữ";
          genderBadge.className = volunteer.Gender
            ? "badge bg-primary"
            : "badge bg-pink";
        } else {
          genderBadge.textContent = "Chưa xác định";
          genderBadge.className = "badge bg-secondary";
        }

        // Xử lý ngày sinh
        const dobBadge = document.getElementById("volunteerDob");
        if (volunteer.DateOfBirth) {
          const dob = new Date(volunteer.DateOfBirth);
          dobBadge.textContent = dob.toLocaleDateString("vi-VN");
        } else {
          dobBadge.textContent = "Chưa cập nhật";
        }

        // Xử lý sự kiện
        const eventsContainer = document.getElementById("volunteerEvents");
        const noEventsMsg = document.getElementById("noEventsMessage");

        if (
          volunteer.RegisteredEvents &&
          volunteer.RegisteredEvents.length > 0
        ) {
          noEventsMsg.style.display = "none";
          eventsContainer.innerHTML = "";

          volunteer.RegisteredEvents.forEach((event) => {
            const eventItem = document.createElement("div");
            eventItem.className = "list-group-item";
            eventItem.innerHTML = `
                            <div class="d-flex justify-content-between">
                                <div>
                                    <h6 class="mb-1">${event.EventName}</h6>
                                    <small class="text-muted">${event.EventDate}</small>
                                </div>
                                <span class="badge bg-light text-dark">${event.EventId}</span>
                            </div>
                        `;
            eventsContainer.appendChild(eventItem);
          });
        } else {
          noEventsMsg.style.display = "block";
        }

        // Hiển thị modal
        new bootstrap.Modal("#volunteerModal").show();
      }
    })
    .catch((error) => {
      Swal.fire("Lỗi", "Không thể tải thông tin", "error");
    });
}
