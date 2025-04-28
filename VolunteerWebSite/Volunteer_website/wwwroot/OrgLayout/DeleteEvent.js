<<<<<<< HEAD
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
async function deleteEvent(eventId) {
  try {
    const response = await fetch(
      `/Organization/HomeOrg/DeleteEvent?id=${eventId}`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Accept: "application/json", // Yêu cầu server chỉ trả về JSON
        },
      }
    );

    // Kiểm tra nếu response không phải JSON
    const contentType = response.headers.get("content-type");
    if (!contentType || !contentType.includes("application/json")) {
      const html = await response.text();
      throw new Error(
        html.includes("<!DOCTYPE html>")
          ? "Lỗi hệ thống. Vui lòng tải lại trang"
          : "Phản hồi không hợp lệ từ server"
      );
    }

    const data = await response.json();

    if (data.success) {
      Swal.fire("Thành công", data.message, "success");
      setTimeout(() => window.location.reload(), 1500); // Reload sau 1.5s
    } else {
      throw new Error(data.message);
    }
  } catch (error) {
    Swal.fire("Lỗi", error.message, "error");
    console.error("Chi tiết lỗi:", error);
  }
}
=======
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
      // Tạo form ẩn để submit
      const form = document.createElement("form");
      form.method = "POST";
      form.action = `/Organization/EventManager/Delete?id=${eventId}`;

      // Thêm CSRF token
      const csrfToken = document.querySelector(
        'input[name="__RequestVerificationToken"]'
      ).value;
      const csrfInput = document.createElement("input");
      csrfInput.type = "hidden";
      csrfInput.name = "__RequestVerificationToken";
      csrfInput.value = csrfToken;
      form.appendChild(csrfInput);

      document.body.appendChild(form);
      form.submit();
    }
  });
}
>>>>>>> 2ebca8439f845ee34dc7a2f7af26f953bb8a182e
