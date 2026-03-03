"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.on("ReceiveNotification", function (message) {
    // Show toast notification
    toastr.info(message, "Thông báo mới");

    // Update notification badge
    const badge = document.getElementById("notificationBadge");
    if (badge) {
        let count = parseInt(badge.innerText) || 0;
        count++;
        badge.innerText = count;
        badge.style.display = "inline-block";
    }

    // Update notification dropdown list
    const list = document.getElementById("notificationList");
    if (list) {
        // Remove "No new notifications" if it exists
        const noNotif = list.querySelector("span.text-muted");
        if (noNotif) {
            noNotif.parentElement.remove();
        }

        // Create new notification item
        const newLi = document.createElement("li");

        const newA = document.createElement("a");
        newA.className = "dropdown-item text-wrap fw-bold";
        newA.href = "#";

        const newSmallMsg = document.createElement("small");
        newSmallMsg.innerText = message;

        const br = document.createElement("br");

        const newSmallTime = document.createElement("small");
        newSmallTime.className = "text-muted";
        newSmallTime.innerText = "Vừa xong";

        newA.appendChild(newSmallMsg);
        newA.appendChild(br);
        newA.appendChild(newSmallTime);
        newLi.appendChild(newA);

        // Insert after the header (which is the first li)
        if (list.children.length > 1) {
            list.insertBefore(newLi, list.children[1]);
        } else {
            list.appendChild(newLi);
        }

        // Keep only 5 items max (1 header + 5 items)
        if (list.children.length > 6) {
            list.removeChild(list.lastChild);
        }
    }
});

connection.start().then(function () {
    console.log("Connected to NotificationHub");
}).catch(function (err) {
    return console.error(err.toString());
});

// Configure Toastr options
toastr.options = {
    "closeButton": true,
    "debug": false,
    "newestOnTop": true,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "preventDuplicates": false,
    "onclick": null,
    "showDuration": "300",
    "hideDuration": "1000",
    "timeOut": "5000",
    "extendedTimeOut": "1000",
    "showEasing": "swing",
    "hideEasing": "linear",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
};
