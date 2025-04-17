// Khởi tạo modal instance một lần duy nhất
const volunteerModal = new bootstrap.Modal(document.getElementById('volunteerModal'));
let isModalLoading = false;

async function showVolunteerDetails(volunteerId) {
  // Tránh gọi API liên tục khi đang loading
  if (isModalLoading) return;
  
  try {
    isModalLoading = true;
    
    // 1. Hiển thị trạng thái loading
    showLoadingState();
    
    // 2. Gọi API lấy thông tin
    const response = await fetchWithTimeout(
      `/Organization/HomeOrg/GetVolunteerDetails?id=${volunteerId}`,
      { timeout: 5000 }
    );
    
    if (!response.ok) {
      throw new Error(`Lỗi HTTP: ${response.status} - ${response.statusText}`);
    }

    const data = await response.json();
    
    // 3. Validate dữ liệu
    if (!data?.success || !data.data?.VolunteerId) {
      throw new Error(data?.message || "Dữ liệu trả về không hợp lệ");
    }

    // 4. Cập nhật giao diện
    await updateVolunteerUI(data.data);
    
    // 5. Hiển thị modal
    volunteerModal.show();
    
  } catch (error) {
    console.error("Lỗi khi tải thông tin:", error);
    handleLoadError(error, volunteerId);
  } finally {
    isModalLoading = false;
  }
}

// Hàm fetch với timeout
function fetchWithTimeout(url, options = {}) {
  const { timeout = 3000 } = options;
  
  return Promise.race([
    fetch(url, options),
    new Promise((_, reject) =>
      setTimeout(() => reject(new Error('Request timeout')), timeout)
  ]);
}

// Hiển thị trạng thái loading
function showLoadingState() {
  const loadingHTML = `
    <div class="text-center py-4">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p class="mt-2">Đang tải thông tin...</p>
    </div>
  `;
  
  document.getElementById("volunteerEvents").innerHTML = loadingHTML;
  
  // Reset các field khác
  const fields = ["volunteerName", "volunteerId", "volunteerEmail", 
                 "volunteerPhone", "volunteerAddress", "volunteerGender", 
                 "volunteerDob"];
  fields.forEach(id => {
    document.getElementById(id).textContent = "...";
  });
}

// Cập nhật giao diện với dữ liệu
async function updateVolunteerUI(volunteer) {
  // Thông tin cơ bản
  document.getElementById("volunteerName").textContent = volunteer.Name || "Chưa cập nhật";
  document.getElementById("volunteerId").textContent = `ID: ${volunteer.VolunteerId}`;
  document.getElementById("volunteerEmail").textContent = volunteer.Email || "Chưa cập nhật";
  document.getElementById("volunteerPhone").textContent = volunteer.PhoneNumber || "Chưa cập nhật";
  document.getElementById("volunteerAddress").textContent = volunteer.Address || "Chưa cập nhật";

  // Xử lý ảnh đại diện
  const imgElement = document.getElementById("volunteerImage");
  if (volunteer.ImagePath) {
    imgElement.src = `/uploads/${volunteer.ImagePath}`;
    imgElement.onerror = () => {
      imgElement.src = '/images/default-avatar.jpg';
    };
  } else {
    imgElement.src = '/images/default-avatar.jpg';
  }
  imgElement.alt = `Ảnh đại diện của ${volunteer.Name || 'tình nguyện viên'}`;

  // Xử lý giới tính
  const genderBadge = document.getElementById("volunteerGender");
  if (volunteer.Gender !== null && volunteer.Gender !== undefined) {
    genderBadge.textContent = volunteer.Gender ? "Nam" : "Nữ";
    genderBadge.className = volunteer.Gender ? "badge bg-primary" : "badge bg-pink";
  } else {
    genderBadge.textContent = "Chưa xác định";
    genderBadge.className = "badge bg-secondary";
  }

  // Xử lý ngày sinh
  const dobBadge = document.getElementById("volunteerDob");
  if (volunteer.DateOfBirth) {
    try {
      const dob = new Date(volunteer.DateOfBirth);
      if (!isNaN(dob)) {
        dobBadge.textContent = dob.toLocaleDateString("vi-VN");
      } else {
        dobBadge.textContent = "Chưa hợp lệ";
      }
    } catch (e) {
      console.error("Lỗi định dạng ngày sinh:", e);
      dobBadge.textContent = volunteer.DateOfBirth; // Hiển thị nguyên bản
    }
  } else {
    dobBadge.textContent = "Chưa cập nhật";
  }

  // Xử lý sự kiện đã tham gia
  await renderVolunteerEvents(volunteer.RegisteredEvents || []);
}

// Render danh sách sự kiện
async function renderVolunteerEvents(events) {
  const container = document.getElementById("volunteerEvents");
  const noEventsMsg = document.getElementById("noEventsMessage");
  
  // Xóa nội dung cũ
  container.innerHTML = '';
  
  if (events.length > 0) {
    noEventsMsg.style.display = "none";
    
    // Sắp xếp sự kiện theo ngày mới nhất
    const sortedEvents = [...events].sort((a, b) => {
      return new Date(b.EventDate?.split(' - ')[0] || 0) - 
             new Date(a.EventDate?.split(' - ')[0] || 0);
    });
    
    // Thêm từng sự kiện
    sortedEvents.forEach(event => {
      const eventItem = document.createElement("div");
      eventItem.className = "list-group-item";
      eventItem.innerHTML = `
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <h6 class="mb-1">${event.EventName || 'Sự kiện không tên'}</h6>
            <small class="text-muted">${formatEventDate(event.EventDate)}</small>
          </div>
          <span class="badge bg-light text-dark">${event.EventId || ''}</span>
        </div>
      `;
      container.appendChild(eventItem);
    });
  } else {
    noEventsMsg.style.display = "block";
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
  return isNaN(date) ? dateStr : date.toLocaleDateString("vi-VN", {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  });
}

// Xử lý khi có lỗi
function handleLoadError(error, volunteerId) {
  // Hiển thị thông báo lỗi
  Swal.fire({
    icon: 'error',
    title: 'Lỗi tải dữ liệu',
    html: `
      <p>${error.message || 'Không thể tải thông tin'}</p>
      <small class="text-muted">Mã tình nguyện viên: ${volunteerId}</small>
    `,
    confirmButtonText: 'Đóng',
    backdrop: true
  });
  
  // Reset giao diện
  resetVolunteerUI();
}

// Reset giao diện về trạng thái ban đầu
function resetVolunteerUI() {
  const fields = ["volunteerName", "volunteerEmail", "volunteerPhone", 
                 "volunteerAddress", "volunteerDob", "volunteerGender"];
  fields.forEach(id => {
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
  if (document.querySelector('#volunteerModal.show')) {
    volunteerModal.hide();
    setTimeout(() => showVolunteerDetails(volunteerId), 300);
  } else {
    showVolunteerDetails(volunteerId);
  }
}