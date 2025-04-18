// Khởi tạo modal instance một lần duy nhất
const volunteerModal = new bootstrap.Modal(
  document.getElementById("volunteerModal")
);
let isModalLoading = false;

// Hàm fetch với timeout
function fetchWithTimeout(url, options = {}) {
  const { timeout = 3000 } = options;

  const controller = new AbortController();
  const signal = controller.signal;

  const fetchPromise = fetch(url, { ...options, signal });
  const timeoutPromise = new Promise((_, reject) =>
    setTimeout(() => {
      controller.abort();
      reject(new Error("Request timeout"));
    }, timeout)
  );

  return Promise.race([fetchPromise, timeoutPromise]);
}

async function showVolunteerDetails(volunteerId) {
  if (isModalLoading) return;
  if (typeof isModalLoading === "undefined" || isModalLoading) return;
  try {
    isModalLoading = true;
    showLoadingState();

    const response = await fetchWithTimeout(
      `/Organization/HomeOrg/GetVolunteerDetails?id=${volunteerId}`,
      {
        timeout: 5000,
        headers: {
          "Content-Type": "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();

    if (!data?.success || !data.data?.VolunteerId) {
      throw new Error(data?.message || "Invalid data format");
    }

    await updateVolunteerUI(data.data);
    volunteerModal.show();
  } catch (error) {
    console.error("Error loading volunteer details:", error);
    handleLoadError(error, volunteerId);

    // Đóng modal nếu có lỗi
    if (volunteerModal._isShown) {
      volunteerModal.hide();
    }
  } finally {
    isModalLoading = false;
  }
}

function showLoadingState() {
  const container = document.getElementById("volunteerEvents");
  if (!container) {
    console.error("volunteerEvents container not found");
    return;
  }

  container.innerHTML = `
    <div class="text-center py-4">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p class="mt-2">Đang tải thông tin...</p>
    </div>
  `;

  // Reset các field
  const fields = [
    "volunteerName",
    "volunteerId",
    "volunteerEmail",
    "volunteerPhone",
    "volunteerAddress",
    "volunteerGender",
    "volunteerDob",
  ];
  fields.forEach((id) => {
    const el = document.getElementById(id);
    if (el) el.textContent = "...";
  });
}

async function updateVolunteerUI(volunteer) {
  if (!volunteer) return;

  // Helper function để set text content an toàn
  const setText = (id, text, fallback = "Chưa cập nhật") => {
    const el = document.getElementById(id);
    if (el) el.textContent = text || fallback;
  };

  // Thông tin cơ bản
  setText("volunteerName", volunteer.Name);
  setText("volunteerId", `ID: ${volunteer.VolunteerId}`, "ID: --");
  setText("volunteerEmail", volunteer.Email);
  setText("volunteerPhone", volunteer.PhoneNumber);
  setText("volunteerAddress", volunteer.Address);

  // Xử lý ảnh
  const imgElement = document.getElementById("volunteerImage");
  if (imgElement) {
    imgElement.src = volunteer.ImagePath
      ? `/uploads/${volunteer.ImagePath}`
      : "/images/default-avatar.jpg";
    imgElement.alt = `Ảnh đại diện của ${volunteer.Name || "tình nguyện viên"}`;
    imgElement.onerror = () => {
      imgElement.src = "/images/default-avatar.jpg";
    };
  }

  // Xử lý giới tính
  const genderBadge = document.getElementById("volunteerGender");
  if (genderBadge) {
    if (volunteer.Gender !== null && volunteer.Gender !== undefined) {
      genderBadge.textContent = volunteer.Gender ? "Nam" : "Nữ";
      genderBadge.className = volunteer.Gender
        ? "badge bg-primary"
        : "badge bg-pink";
    } else {
      genderBadge.textContent = "Chưa xác định";
      genderBadge.className = "badge bg-secondary";
    }
  }

  // Xử lý ngày sinh
  const dobBadge = document.getElementById("volunteerDob");
  if (dobBadge) {
    if (volunteer.DateOfBirth) {
      try {
        const dob = new Date(volunteer.DateOfBirth);
        dobBadge.textContent = !isNaN(dob)
          ? dob.toLocaleDateString("vi-VN")
          : "Chưa hợp lệ";
      } catch (e) {
        console.error("Date parsing error:", e);
        dobBadge.textContent = "Chưa hợp lệ";
      }
    } else {
      dobBadge.textContent = "Chưa cập nhật";
    }
  }

  // Render sự kiện
  await renderVolunteerEvents(volunteer.RegisteredEvents || []);
}

async function renderVolunteerEvents(events) {
  const container = document.getElementById("volunteerEvents");
  const noEventsMsg = document.getElementById("noEventsMessage");

  if (!container) return;

  try {
    container.innerHTML = "";

    if (events.length > 0) {
      if (noEventsMsg) noEventsMsg.style.display = "none";

      // Sắp xếp sự kiện
      const sortedEvents = [...events].sort((a, b) => {
        const getDate = (event) => {
          const dateStr = event.EventDate?.split(" - ")[0];
          return dateStr ? new Date(dateStr) : new Date(0);
        };
        return getDate(b) - getDate(a);
      });

      // Thêm sự kiện vào DOM
      sortedEvents.forEach((event) => {
        const eventItem = document.createElement("div");
        eventItem.className = "list-group-item";
        eventItem.innerHTML = `
          <div class="d-flex justify-content-between align-items-center">
            <div>
              <h6 class="mb-1">${event.EventName || "Sự kiện không tên"}</h6>
              <small class="text-muted">${formatEventDate(
                event.EventDate
              )}</small>
            </div>
            <span class="badge bg-light text-dark">${event.EventId || ""}</span>
          </div>
        `;
        container.appendChild(eventItem);
      });
    } else {
      if (noEventsMsg) noEventsMsg.style.display = "block";
    }
  } catch (error) {
    console.error("Error rendering events:", error);
    if (container) {
      container.innerHTML = `
        <div class="list-group-item text-center py-3 text-danger">
          Đã xảy ra lỗi khi tải sự kiện
        </div>
      `;
    }
  }
}

// Định dạng ngày sự kiện
function formatEventDate(dateString) {
  if (!dateString) return "Chưa có ngày";

  try {
    if (dateString.includes(" - ")) {
      const [start, end] = dateString.split(" - ");
      return `${formatSingleDate(start)} → ${formatSingleDate(end)}`;
    }
    return formatSingleDate(dateString);
  } catch (e) {
    console.error("Lỗi định dạng ngày:", e);
    return dateString; // Trả về nguyên bản nếu có lỗi
  }
}

function formatSingleDate(dateStr) {
  const date = new Date(dateStr);
  return isNaN(date)
    ? dateStr
    : date.toLocaleDateString("vi-VN", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      });
}

// Xử lý khi có lỗi
function handleLoadError(error, volunteerId) {
  // Hiển thị thông báo lỗi
  Swal.fire({
    icon: "error",
    title: "Lỗi tải dữ liệu",
    html: `
      <p>${error.message || "Không thể tải thông tin"}</p>
      <small class="text-muted">Mã tình nguyện viên: ${volunteerId}</small>
    `,
    confirmButtonText: "Đóng",
    backdrop: true,
  });

  // Reset giao diện
  resetVolunteerUI();
}

// Reset giao diện về trạng thái ban đầu
function resetVolunteerUI() {
  const fields = [
    "volunteerName",
    "volunteerEmail",
    "volunteerPhone",
    "volunteerAddress",
    "volunteerDob",
    "volunteerGender",
  ];
  fields.forEach((id) => {
    document.getElementById(id).textContent = "Chưa cập nhật";
  });

  document.getElementById("volunteerId").textContent = "ID: --";
  document.getElementById("volunteerImage").src = "/images/default-avatar.jpg";

  const eventsContainer = document.getElementById("volunteerEvents");
  eventsContainer.innerHTML = `
    <div class="list-group-item text-center py-3 text-muted">
      Không thể tải thông tin sự kiện
    </div>
  `;
  document.getElementById("noEventsMessage").style.display = "none";
}

// Gọi hàm khi click vào hàng trong bảng
function onVolunteerRowClick(volunteerId) {
  if (!volunteerId) return;

  // Đóng modal nếu đang mở
  if (document.querySelector("#volunteerModal.show")) {
    volunteerModal.hide();
    setTimeout(() => showVolunteerDetails(volunteerId), 300);
  } else {
    showVolunteerDetails(volunteerId);
  }
}
