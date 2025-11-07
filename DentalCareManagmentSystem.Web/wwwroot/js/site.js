window.addEventListener('DOMContentLoaded', event => {
    // Toggle the side navigation
    const sidebarToggle = document.body.querySelector('#sidebarToggle');
    if (sidebarToggle) {
        if (localStorage.getItem('sb-sidenav-toggle') === 'true') {
            document.body.classList.toggle('sb-sidenav-toggled');
        }
        sidebarToggle.addEventListener('click', event => {
            event.preventDefault();
            document.body.classList.toggle('sb-sidenav-toggled');
            localStorage.setItem('sb-sidenav-toggle', document.body.classList.contains('sb-sidenav-toggled'));
        });
    }

    // Function to update time and date in the navbar
    function updateTime() {
        const now = new Date();
        const timeElement = document.getElementById('navbarTime');
        const dateElement = document.getElementById('navbarDate');

        if (timeElement) {
            timeElement.textContent = now.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: true });
        }
        if (dateElement) {
            dateElement.textContent = now.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' });
        }
    }

    // Update time immediately and then every second
    updateTime();
    setInterval(updateTime, 1000);
});