// Khởi tạo modal instance một lần duy nhất
const volunteerModal = new bootstrap.Modal(
  document.getElementById("volunteerModal")
);

// Hàm hiển thị chi tiết tình nguyện viên
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

        // Xử lý ảnh - thêm fallback khi null
        const volunteerImage = document.getElementById("volunteerImage");
        volunteerImage.src = volunteer.ImagePath
          ? `/uploads/${volunteer.ImagePath}`
          : "/images/default-avatar.jpg";
        volunteerImage.alt = `Ảnh đại diện của ${
          volunteer.Name || "tình nguyện viên"
        }`;

        // Xử lý giới tính
        const genderBadge = document.getElementById("volunteerGender");
        if (volunteer.Gender !== null && volunteer.Gender !== undefined) {
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

        eventsContainer.innerHTML = ""; // Clear trước khi thêm mới

        if (volunteer.RegisteredEvents?.length > 0) {
          noEventsMsg.style.display = "none";

          volunteer.RegisteredEvents.forEach((event) => {
            const eventItem = document.createElement("div");
            eventItem.className = "list-group-item";
            eventItem.innerHTML = `
              <div class="d-flex justify-content-between">
                <div>
                  <h6 class="mb-1">${event.EventName || "Chưa có tên"}</h6>
                  <small class="text-muted">${
                    event.EventDate || "Chưa có ngày"
                  }</small>
                </div>
                <span class="badge bg-light text-dark">${
                  event.EventId || ""
                }</span>
              </div>
            `;
            eventsContainer.appendChild(eventItem);
          });
        } else {
          noEventsMsg.style.display = "block";
        }

        // Hiển thị modal
        volunteerModal.show();
      }
    })
    .catch((error) => {
      console.error("Error:", error);
      Swal.fire("Lỗi", "Không thể tải thông tin", "error");
    });
}

// Xóa các sự kiện modal cũ nếu có
const modalElement = document.getElementById("volunteerModal");
modalElement.removeEventListener("shown.bs.modal", () => {});
modalElement.removeEventListener("hidden.bs.modal", () => {});

// Thêm sự kiện modal mới
modalElement.addEventListener("shown.bs.modal", () => {
  modalElement.setAttribute("aria-hidden", "false");
});

modalElement.addEventListener("hidden.bs.modal", () => {
  modalElement.setAttribute("aria-hidden", "true");
});
