"use strict";

// ⭐ Kiểm tra SignalR đã load chưa
(function initSignalR() {
    if (typeof signalR === 'undefined') {
        console.warn('SignalR chưa load, đợi 100ms...');
        setTimeout(initSignalR, 100);
        return;
    }

    console.log('SignalR đã load thành công!');

    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/signalRServer")
        .configureLogging(signalR.LogLevel.Information)
        .withAutomaticReconnect()
        .build();

    // ✅ Handler cho News - Cải tiến với whitelist
    connection.on("LoadNews", function () {
        console.log('📢 Received LoadNews signal');
        
        const currentPath = window.location.pathname.toLowerCase();
        console.log('Current path:', currentPath);
        
        // Danh sách các page cần reload khi có LoadNews
        const newsRelatedPages = [
            '/',                    // Home page
            '/home',               
            '/home/index',
            '/news',
            '/news/index',
            '/news/mynews',
            '/news/details',
            '/news/search'
        ];
        
        // Check nếu path khớp với bất kỳ page nào
        const shouldReload = newsRelatedPages.some(page => {
            return currentPath === page || currentPath.startsWith(page + '/');
        });
        
        console.log('Should reload?', shouldReload);
        
        if (shouldReload) {
            console.log('✅ Reloading page...');
            location.reload();
        } else {
            console.log('❌ Not a news-related page, skipping reload');
        }
    });

    // Handler cho Category
    connection.on("LoadCategories", function () {
        console.log('📢 Received LoadCategories signal');
        
        const currentPath = window.location.pathname.toLowerCase();
        const categoryPages = [
            '/category',
            '/category/index',
            '/category/manage'
        ];
        
        const shouldReload = categoryPages.some(page => 
            currentPath === page || currentPath.startsWith(page + '/')
        );

        if (shouldReload) {
            console.log('✅ Reloading category page...');
            location.reload();
        }
    });

    // Handler cho Tag
    connection.on("LoadTags", function () {
        console.log('📢 Received LoadTags signal');
        
        const currentPath = window.location.pathname.toLowerCase();
        const tagPages = [
            '/tag',
            '/tag/index',
            '/tag/manage'
        ];
        
        const shouldReload = tagPages.some(page => 
            currentPath === page || currentPath.startsWith(page + '/')
        );

        if (shouldReload) {
            console.log('✅ Reloading tag page...');
            location.reload();
        }
    });

    // Handler cho Account
    connection.on("LoadAccounts", function () {
        console.log('📢 Received LoadAccounts signal');
        
        const currentPath = window.location.pathname.toLowerCase();
        const accountPages = [
            '/account/manage',
            '/account/index',
            '/admin/users'
        ];
        
        const shouldReload = accountPages.some(page => 
            currentPath === page || currentPath.startsWith(page + '/')
        );

        if (accountPages) {
            console.log('✅ Reloading account page...');
            location.reload();
        }
    });

    // 🔍 Theo dõi connection state
    connection.onreconnecting((error) => {
        console.warn('🔄 SignalR reconnecting...', error);
    });

    connection.onreconnected((connectionId) => {
        console.log('✅ SignalR reconnected!', connectionId);
    });

    connection.onclose((error) => {
        console.error('❌ SignalR disconnected:', error);
        setTimeout(() => startConnection(), 5000);
    });

    function startConnection() {
        connection.start()
            .then(() => {
                console.log('✅ SignalR connected! ConnectionId:', connection.connectionId);
            })
            .catch(err => {
                console.error('❌ SignalR connection error:', err.toString());
                setTimeout(() => startConnection(), 5000);
            });
    }

    startConnection();
})();