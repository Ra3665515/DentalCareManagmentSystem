document.addEventListener("DOMContentLoaded", function () {
    window.appConnectionPromise.then(connection => {
        if (!connection) {
            console.error("Cannot execute doctor actions, SignalR connection failed.");
            return;
        }

        // Listen for new patients added to queue
        connection.on("AddPatientToQueue", (appointmentId, patientName, appointmentTime) => {
            console.log(`🧍‍♂️ New patient added to queue: ${patientName} at ${appointmentTime}`);
            
            // Show notification to doctor
            if (typeof showToast === 'function') {
                showToast(`New patient in queue: ${patientName} (${appointmentTime})`, 'info');
            } else {
                alert(`🧍‍♂️ New patient added to queue: ${patientName}`);
            }

            // Play notification sound (optional)
            playNotificationSound();
            
            // Update the doctor's queue view if the function exists
            if (typeof refreshDoctorQueue === 'function') {
                refreshDoctorQueue();
            }
        });

        connection.on("ReceiveNewPatient", (appointmentId, name) => {
            console.log(`🧍‍♂️ New patient added: ${name}`);
            if (typeof showToast === 'function') {
                showToast(`New patient: ${name}`, 'info');
            } else {
                alert(`🧍‍♂️ New patient added: ${name}`);
            }
        });

        connection.on("PatientTransferred", (appointmentId, name) => {
            console.log(`➡️ Patient transferred: ${name}`);
            if (typeof showToast === 'function') {
                showToast(`Patient transferred: ${name}`, 'warning');
            } else {
                alert(`➡️ Patient transferred: ${name}`);
            }
        });

        const completeBtn = document.getElementById("btnComplete");
        if (completeBtn) {
            completeBtn.addEventListener("click", () => {
                const id = document.getElementById("completeId").value;
                const name = document.getElementById("completeName").value;
                if (!id || !name) {
                    alert("Please enter appointment ID and patient name");
                    return;
                }
                connection.invoke("CompleteSession", id, name);
                alert(`✅ Completed session for ${name}`);
            });
        }
    }).catch(err => {
        console.error("Error during SignalR promise resolution: ", err);
    });
});

// Helper function to play notification sound
function playNotificationSound() {
    try {
        const audio = new Audio('/sounds/notification.mp3'); // You can add a notification sound file
        audio.volume = 0.5;
        audio.play().catch(err => console.log('Could not play notification sound:', err));
    } catch (err) {
        console.log('Notification sound not available');
    }
}

// Helper function to show toast notifications
function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) {
        const container = document.createElement('div');
        container.id = 'toastContainer';
        container.style.position = 'fixed';
        container.style.top = '20px';
        container.style.right = '20px';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
    }
    
    const toast = document.createElement('div');
    toast.className = `alert alert-${type === 'success' ? 'success' : type === 'error' ? 'danger' : type === 'warning' ? 'warning' : 'info'} alert-dismissible fade show`;
    toast.role = 'alert';
    toast.innerHTML = `
        <strong><i class="fas fa-bell me-2"></i></strong>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.getElementById('toastContainer').appendChild(toast);
    
    setTimeout(() => {
        toast.remove();
    }, 5000);
}

// Function to refresh doctor's queue (to be called when new patient is added)
function refreshDoctorQueue() {
    // Reload the queue data - you can implement this based on your needs
    console.log('Refreshing doctor queue...');
    // Example: window.location.reload(); // or load via AJAX
}