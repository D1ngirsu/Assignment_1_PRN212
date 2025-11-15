"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/signalRServer").build();

// Handler cho News
connection.on("LoadNews", function () {
    if (window.location.pathname === '/News/Index' || window.location.pathname === '/News/MyNews' || window.location.pathname === '/News/Report') {
        location.reload();  // Reload nếu đang ở list News, MyNews, hoặc Report
    }
});

// Handler cho Category
connection.on("LoadCategories", function () {
    if (window.location.pathname === '/Category/Index') {
        location.reload();  // Reload nếu đang ở list Category
    }
});

// Handler cho Tag
connection.on("LoadTags", function () {
    if (window.location.pathname === '/Tag/Index') {
        location.reload();  // Reload nếu đang ở list Tag
    }
});

// Handler cho Account (nếu cần realtime cho ManageAccounts)
connection.on("LoadAccounts", function () {
    if (window.location.pathname === '/Account/ManageAccounts') {
        location.reload();  // Reload nếu đang ở ManageAccounts
    }
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});