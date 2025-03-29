<script>
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
</script>