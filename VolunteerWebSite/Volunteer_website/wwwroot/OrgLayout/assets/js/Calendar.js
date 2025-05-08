document.addEventListener("DOMContentLoaded", function () {
  var calendarEl = document.getElementById("calendar");
  var eventListContent = document.getElementById("eventListContent");

  var calendar = new FullCalendar.Calendar(calendarEl, {
    initialView: "dayGridMonth",
    headerToolbar: {
      left: "prev today",
      center: "title",
      right: "next",
    },
    events: "/Organization/Calendar/GetEventsCalendar",
    editable: true,
    eventResizable: true,
    eventDrop: function (info) {
      var eventId = info.event.id;
      var startDate = info.event.start.toISOString().split("T")[0];
      var endDate = info.event.end
        ? info.event.end.toISOString().split("T")[0]
        : null;

      updateEvent(eventId, startDate, endDate);
    },
    eventResize: function (info) {
      var eventId = info.event.id;
      var startDate = info.event.start.toISOString().split("T")[0];
      var endDate = info.event.end
        ? info.event.end.toISOString().split("T")[0]
        : null;

      updateEvent(eventId, startDate, endDate);
    },
    eventClick: function (info) {
      alert(
        "Event: " +
          info.event.title +
          "\nDescription: " +
          info.event.extendedProps.description
      );
    },
    firstDay: 0,
  });

  calendar.render();

  function updateEvent(eventId, startDate, endDate) {
    const token =
      document.querySelector('input[name="__RequestVerificationToken"]')
        ?.value || "";
    fetch("/Organization/Calendar/UpdateEventDate", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        RequestVerificationToken: token,
      },
      body: JSON.stringify({
        eventId: eventId,
        startDate: startDate,
        endDate: endDate,
      }),
    })
      .then((response) => response.json())
      .then((data) => {
        if (data.success) {
          alert(data.message);
          updateEventList();
        } else {
          alert(data.message);
          calendar.refetchEvents(); // Revert changes on failure
        }
      })
      .catch((error) => {
        console.error("Error:", error);
        alert("An error occurred while updating the event");
        calendar.refetchEvents(); // Revert changes on error
      });
  }

  function updateEventList() {
    fetch("/Organization/Calendar/GetEventsCalendar")
      .then((response) => response.json())
      .then((events) => {
        eventListContent.innerHTML = "";
        const eventsByDate = {};
        events.forEach((event) => {
          const date = event.start.split("T")[0];
          if (!eventsByDate[date]) eventsByDate[date] = [];
          eventsByDate[date].push(event);
        });

        for (const date in eventsByDate) {
          const dateEvents = eventsByDate[date];
          const dateHeader = document.createElement("div");
          dateHeader.className = "event-date";
          dateHeader.textContent = new Date(date).toLocaleDateString("en-US", {
            weekday: "long",
            year: "numeric",
            month: "long",
            day: "numeric",
          });
          eventListContent.appendChild(dateHeader);

          dateEvents.forEach((event) => {
            const eventItem = document.createElement("div");
            eventItem.className = "event-item d-flex";
            eventItem.innerHTML = `<span class="event-title">${event.title}</span>`;
            eventListContent.appendChild(eventItem);
          });
        }
      })
      .catch((error) => console.error("Error fetching events:", error));
  }

  updateEventList();
});
