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
