function searchEvents() {
  var input = document.getElementById("searchBox").value.toLowerCase();
  var rows = document.getElementById("eventTable").getElementsByTagName("tr");

  for (var i = 0; i < rows.length; i++) {
    var name = rows[i].getElementsByTagName("td")[1]?.innerText.toLowerCase();
    if (name && name.includes(input)) {
      rows[i].style.display = "";
    } else {
      rows[i].style.display = "none";
    }
  }
}


//hàm tìm kiếm xem xét nhiều cột (ví dụ: Name, Email, Address)
function search() {
    var input = document.getElementById("searchBox").value.toLowerCase();
    var rows = document.getElementById("dataTable").getElementsByTagName("tr");

    for (var i = 0; i < rows.length; i++) {
        var cells = rows[i].getElementsByTagName("td"); // đọc dữ liệu từ bảng "dataTable"
        var match = false;

        for (var j = 0; j < cells.length - 1; j++) { // Bỏ qua cột cuối (Actions)
            var cellText = cells[j]?.innerText.toLowerCase();
            if (cellText && cellText.includes(input)) {
                match = true;
                break;
            }
        }

        if (match) {
            rows[i].style.display = "";
        } else {
            rows[i].style.display = "none";
        }
    }
}
