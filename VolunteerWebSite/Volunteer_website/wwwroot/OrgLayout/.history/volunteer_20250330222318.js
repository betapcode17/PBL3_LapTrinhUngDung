function showVolunteerDetails(regId) {
    // Gọi AJAX để lấy thông tin volunteer
    $.ajax({
        url: '/Organization/HomeOrg/GetVolunteerDetails', // Đường dẫn tới action
        type: 'GET',
        data: { id: regId },
        success: function (response) {
            // Giả sử response là dữ liệu volunteer từ controller
            let content = `
                <p><strong>ID:</strong> ${response.data.VolunteerId}</p>
                <p><strong>Name:</strong> ${response.data.Name}</p>
                <p><strong>Email:</strong> ${response.data.Email}</p>
                <p><strong>Phone:</strong> ${response.data.PhoneNumber}</p>
                <p><strong>Date of Birth:</strong> ${response.data.DateOfBirth || 'N/A'}</p>
                <p><strong>Gender:</strong> ${response.data.Gender}</p>
                <p><strong>Address:</strong> ${response.data.Address || 'N/A'}</p>
            `;
            if (response.data.ImagePath) {
                content += `<img src="${response.data.ImagePath}" alt="Volunteer Image" class="img-fluid" style="max-width: 200px;">`;
            }
            
            // Điền nội dung vào modal
            $('#volunteerDetailsContent').html(content);
            
            // Hiển thị modal
            $('#volunteerDetailsModal').modal('show');
        },
        error: function (xhr, status, error) {
            // Xử lý lỗi
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Unable to fetch volunteer details. Please try again.',
            });
        }
    });
}