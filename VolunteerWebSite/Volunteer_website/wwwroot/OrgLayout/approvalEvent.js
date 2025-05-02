async function handleEventAction(EventId, endPoint) {
    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        const response = await fetch(
            `${endPoint}`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    Accept: "application/json", // Yêu cầu server chỉ trả về JSON

                    "RequestVerificationToken": token // Thêm token vào header
                },
                body: JSON.stringify({ EventId }) // Gửi EventId trong body
            }
        );

        // Kiểm tra nếu response không phải JSON
        const contentType = response.headers.get("content-type");
        if (!contentType || !contentType.includes("application/json")) {
            console.error("Non-JSON response:", await response.text());
            throw new Error("Phản hồi không hợp lệ từ server");
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

function confirmAccept(EventId) {
    console.log(EventId),
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, accept!',
        cancelButtonText: 'No, cancel!',
    }).then((result) => {
        if (result.isConfirmed) {
            handleEventAction(EventId, "/Admin/EventManager/acceptEvent");
        }
    });
}

function confirmReject(EventId) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Reject!',
        cancelButtonText: 'No, cancel!',
    }).then((result) => {
        if (result.isConfirmed) {
            handleEventAction(EventId, "/Admin/EventManager/rejectEvent");
        }
    });
}
