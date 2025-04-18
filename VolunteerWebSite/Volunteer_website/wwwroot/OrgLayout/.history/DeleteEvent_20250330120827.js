// Hàm xác nhận xóa
function confirmDelete(eventId) {
  Swal.fire({
    title: "Bạn chắc chắn muốn xóa?",
    text: "Bạn không thể hoàn tác hành động này!",
    icon: "warning",
    showCancelButton: true,
    confirmButtonColor: "#d33",
    cancelButtonColor: "#3085d6",
    confirmButtonText: "Xóa",
    cancelButtonText: "Hủy",
  }).then((result) => {
    if (result.isConfirmed) {
      deleteEvent(eventId);
    }
  });
}

// Hàm xử lý xóa
function deleteEvent(eventId) {
  // Kiểm tra dữ liệu
  if (!eventId) {
    Swal.fire("Lỗi", "ID sự kiện không hợp lệ", "error");
    return;
  }

  // Gọi API
  fetch(`/Organization/HomeOrg/DeleteEvent?id=${encodeURIComponent(eventId)}`, {
    method: "POST", // Sử dụng POST thay vì GET
  })
    .then((response) => {
      if (!response.ok) throw new Error("Lỗi kết nối");
      return response.json();
    })
    .then((data) => {
      if (data.success) {
        Swal.fire("Thành công!", "Đã xóa sự kiện", "success").then(() =>
          window.location.reload()
        );
      } else {
        throw new Error(data.message || "Xóa thất bại");
      }
    })
    .catch((error) => {
      Swal.fire("Lỗi!", error.message || "Có lỗi xảy ra", "error");
    });
}
