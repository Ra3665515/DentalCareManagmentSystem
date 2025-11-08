document.addEventListener("DOMContentLoaded", function () {
    window.appConnectionPromise.then(connection => {
        if (!connection) {
            console.error("Cannot execute doctor actions, SignalR connection failed.");
            return;
        }

        // Listen for patients sent to doctor
        connection.on("PatientSentToDoctor", (queueData) => {
            console.log(`🧍‍♂️ Patient sent to doctor. Queue updated:`, queueData);

            if (queueData && queueData.length > 0) {
                const latestPatient = queueData[queueData.length - 1];
                const patientName = latestPatient.patientName || latestPatient.PatientName;
                const startTime = formatTime(latestPatient.startTime || latestPatient.StartTime);

                // Show notification to doctor
                showToast(`New patient waiting: ${patientName} (${startTime})`, 'info');

                // Play notification sound
                playNotificationSound();
            }

            // Update the doctor's queue view
            updateDoctorQueueView(queueData);
        });

        connection.on("PatientCompleted", (appointmentId, patientName, updatedQueue) => {
            console.log(`✅ Patient completed: ${patientName}`);

            // Update appointment row status in the table
            const appointmentRow = document.getElementById(`appointment-${appointmentId}`);
            if (appointmentRow) {
                const statusBadge = appointmentRow.querySelector(`#status-${appointmentId}`);
                if (statusBadge) {
                    statusBadge.className = 'badge bg-success status-badge';
                    statusBadge.textContent = 'Completed';
                }
            }

            // Update queue view with remaining patients
            updateDoctorQueueView(updatedQueue);

            showToast(`Session with ${patientName} completed`, 'success');
        });

        // Handle "Complete Session" button clicks in Patient Details
        document.addEventListener('click', function (e) {
            if (e.target && e.target.closest('.complete-session-btn')) {
                e.preventDefault();
                const button = e.target.closest('.complete-session-btn');
                const appointmentId = button.getAttribute('data-appointment-id');
                const patientName = button.getAttribute('data-patient-name');

                completePatientSession(appointmentId, patientName);
            }
        });

        // Load initial queue on page load if doctor view exists
        if (document.getElementById('doctorQueueList')) {
            loadDoctorQueue();
        }

    }).catch(err => {
        console.error("Error during SignalR promise resolution: ", err);
    });
});

// Load the current queue for doctor view
function loadDoctorQueue() {
    fetch('/Notifications/GetQueue')
        .then(response => response.json())
        .then(data => {
            console.log("Doctor queue loaded:", data);
            updateDoctorQueueView(data);
        })
        .catch(err => console.error("Error loading doctor queue:", err));
}

// Update doctor's queue view
function updateDoctorQueueView(queueData) {
    const doctorQueueList = document.getElementById('doctorQueueList');
    const doctorWaitingState = document.getElementById('doctorWaitingState');
    const doctorQueueCount = document.getElementById('doctorQueueCount');

    if (!doctorQueueList) return; // Not on doctor's page

    if (!queueData || queueData.length === 0) {
        if (doctorWaitingState) doctorWaitingState.style.display = 'block';
        doctorQueueList.innerHTML = "";
        if (doctorQueueCount) doctorQueueCount.textContent = "0 Waiting";
        return;
    }

    if (doctorWaitingState) doctorWaitingState.style.display = 'none';
    if (doctorQueueCount) doctorQueueCount.textContent = `${queueData.length} Waiting`;

    doctorQueueList.innerHTML = queueData.map((p, index) => {
        const pName = p.patientName || p.PatientName;
        const pId = p.id || p.Id;
        const pPatientId = p.patientId || p.PatientId;
        const pStartTime = p.startTime || p.StartTime;
        const pPhone = p.patientPhone || p.PatientPhone;

        return `
        <div class="card mb-3 queue-item-doctor shadow-sm">
            <div class="card-body p-3">
                <div class="d-flex justify-content-between align-items-start mb-2">
                    <div class="flex-grow-1">
                        <div class="d-flex align-items-center mb-2">
                            <span class="badge bg-primary me-2 fs-6">#${index + 1}</span>
                            <h6 class="mb-0 fw-bold">${pName}</h6>
                        </div>
                        <small class="text-muted d-block">
                            <i class="far fa-clock me-1"></i>
                            Scheduled: ${formatTime(pStartTime)}
                        </small>
                        <small class="text-muted d-block">
                            <i class="fas fa-phone me-1"></i>
                            ${pPhone || 'N/A'}
                        </small>
                    </div>
                    <span class="badge bg-info">Waiting</span>
                </div>
                <div class="d-flex gap-2 mt-3">
                    <a href="/Patients/Details/${pPatientId}" 
                       class="btn btn-primary btn-sm flex-grow-1">
                        <i class="fas fa-user me-1"></i> Patient Details
                    </a>
                    <button class="btn btn-success btn-sm complete-session-btn" 
                            data-appointment-id="${pId}"
                            data-patient-name="${pName}"
                            title="Complete Session">
                        <i class="fas fa-check me-1"></i> Complete
                    </button>
                </div>
            </div>
        </div>
    `;
    }).join("");
}

// Complete patient session
function completePatientSession(appointmentId, patientName) {
    // Show confirmation toast instead of alert
    showConfirmationToast(
        `Complete session with ${patientName}?`,
        () => {
            // User confirmed - proceed with completion
            const button = document.querySelector(`.complete-session-btn[data-appointment-id="${appointmentId}"]`);
            if (button) {
                button.disabled = true;
                button.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i> Completing...';
            }

            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            fetch(`/Notifications/CompletePatient?appointmentId=${appointmentId}`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token || ""
                }
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        showToast(`Session with ${patientName} completed successfully. Redirecting...`, 'success');
                        // SignalR will handle UI updates, then redirect after a short delay
                        setTimeout(() => {
                            window.location.href = '/Appointments/TodaysAppointments';
                        }, 1500);
                    } else {
                        throw new Error(data.message);
                    }
                })
                .catch(err => {
                    console.error("Error completing patient:", err);
                    showToast(`Failed to complete session: ${err.message}`, 'error');
                    if (button) {
                        button.disabled = false;
                        button.innerHTML = '<i class="fas fa-check me-1"></i> Complete';
                    }
                });
        }
    );
}

// Helper function to format time
function formatTime(timeString) {
    if (!timeString) return 'N/A';
    try {
        const time = new Date(timeString);
        return time.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: true });
    } catch {
        return timeString;
    }
}

// Helper function to play notification sound
function playNotificationSound() {
    try {
        const audio = new Audio('/sounds/notification.mp3');
        audio.volume = 0.5;
        audio.play().catch(err => console.log('Could not play notification sound:', err));
    } catch (err) {
        console.log('Notification sound not available');
    }
}

// Helper function to show toast notifications
function showToast(message, type = 'info') {
    let toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toastContainer';
        toastContainer.style.position = 'fixed';
        toastContainer.style.top = '20px';
        toastContainer.style.right = '20px';
        toastContainer.style.zIndex = '9999';
        document.body.appendChild(toastContainer);
    }

    const toast = document.createElement('div');
    const alertClass = type === 'success' ? 'success' : type === 'error' ? 'danger' : type === 'warning' ? 'warning' : 'info';
    toast.className = `alert alert-${alertClass} alert-dismissible fade show shadow`;
    toast.role = 'alert';
    toast.innerHTML = `
        <strong><i class="fas fa-bell me-2"></i></strong>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    toastContainer.appendChild(toast);

    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 150);
    }, 5000);
}

// Helper function to show confirmation toast
function showConfirmationToast(message, onConfirm, onCancel = null) {
    let toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toastContainer';
        toastContainer.style.position = 'fixed';
        toastContainer.style.top = '20px';
        toastContainer.style.right = '20px';
        toastContainer.style.zIndex = '9999';
        document.body.appendChild(toastContainer);
    }

    const toast = document.createElement('div');
    toast.className = 'alert alert-warning alert-dismissible fade show shadow';
    toast.style.minWidth = '350px';
    toast.role = 'alert';

    const confirmId = 'confirm-' + Date.now();
    const cancelId = 'cancel-' + Date.now();

    toast.innerHTML = `
        <div>
            <strong><i class="fas fa-question-circle me-2"></i></strong>
            ${message}
        </div>
        <div class="mt-3 d-flex gap-2">
            <button type="button" class="btn btn-success btn-sm" id="${confirmId}">
                <i class="fas fa-check me-1"></i> Confirm
            </button>
            <button type="button" class="btn btn-secondary btn-sm" id="${cancelId}">
                <i class="fas fa-times me-1"></i> Cancel
            </button>
        </div>
    `;

    toastContainer.appendChild(toast);

    // Handle confirm button
    document.getElementById(confirmId).addEventListener('click', function () {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 150);
        if (onConfirm) onConfirm();
    });

    // Handle cancel button
    document.getElementById(cancelId).addEventListener('click', function () {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 150);
        if (onCancel) onCancel();
    });

    // Auto-remove after 30 seconds if no action taken
    setTimeout(() => {
        if (toast.parentElement) {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 150);
            if (onCancel) onCancel();
        }
    }, 30000);
}