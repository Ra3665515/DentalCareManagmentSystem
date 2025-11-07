document.addEventListener("DOMContentLoaded", function () {
    window.appConnectionPromise.then(connection => {
        if (!connection) {
            console.error("Cannot execute doctor actions, SignalR connection failed.");
            return;
        }

        // Listen for new patients added to queue
        connection.on("AddPatientToQueue", (fullQueueData) => {
            console.log(`🧍‍♂️ Queue updated with ${fullQueueData?.length || 0} patients`, fullQueueData);
            
            if (fullQueueData && fullQueueData.length > 0) {
                const latestPatient = fullQueueData[fullQueueData.length - 1];
                const patientName = latestPatient.patientName || latestPatient.PatientName;
                const startTime = formatTime(latestPatient.startTime || latestPatient.StartTime);
                
                // Show notification to doctor
                showToast(`New patient in queue: ${patientName} (${startTime})`, 'info');
                
                // Play notification sound (optional)
                playNotificationSound();
            }
            
            // Update the doctor's queue view
            updateDoctorQueueView(fullQueueData);
        });

        connection.on("ReceiveNewPatient", (appointmentId, name) => {
            console.log(`🧍‍♂️ New patient added: ${name}`);
            showToast(`New patient: ${name}`, 'info');
        });

        connection.on("PatientTransferred", (appointmentId, name) => {
            console.log(`➡️ Patient transferred: ${name}`);
            showToast(`Patient transferred: ${name}`, 'warning');
        });

        connection.on("PatientCompleted", (appointmentId, name) => {
            console.log(`✅ Patient completed: ${name}`);
            showToast(`Patient ${name} completed`, 'success');
            
            // Reload queue after completion
            loadDoctorQueue();
        });

        // Complete button handler
        const completeBtn = document.getElementById("btnComplete");
        if (completeBtn) {
            completeBtn.addEventListener("click", () => {
                const id = document.getElementById("completeId")?.value;
                const name = document.getElementById("completeName")?.value;
                if (!id || !name) {
                    alert("Please enter appointment ID and patient name");
                    return;
                }
                
                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                
                fetch(`/Notifications/CompletePatient?appointmentId=${id}`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": token || ""
                    }
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert(`✅ Completed session for ${name}`);
                    } else {
                        throw new Error(data.message);
                    }
                })
                .catch(err => {
                    console.error("Error completing patient:", err);
                    alert(`Failed to complete session: ${err.message}`);
                });
            });
        }

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
    const waitingPatientsCount = document.getElementById('waitingPatientsCount');

    if (!doctorQueueList) return; // Not on doctor's page

    if (!queueData || queueData.length === 0) {
        if (doctorWaitingState) doctorWaitingState.style.display = 'block';
        doctorQueueList.innerHTML = "";
        if (waitingPatientsCount) waitingPatientsCount.textContent = "0 Waiting";
        return;
    }

    if (doctorWaitingState) doctorWaitingState.style.display = 'none';
    if (waitingPatientsCount) waitingPatientsCount.textContent = `${queueData.length} Waiting`;

    doctorQueueList.innerHTML = `
        <h6 class="text-muted mb-3">
            <i class="fas fa-users me-2"></i>Waiting Queue (${queueData.length})
        </h6>
        ${queueData.map((p, index) => {
            const pName = p.patientName || p.PatientName;
            const pId = p.id || p.Id;
            const pPatientId = p.patientId || p.PatientId;
            const pStartTime = p.startTime || p.StartTime;
            
            return `
            <div class="card mb-2 queue-item-small">
                <div class="card-body p-3">
                    <div class="d-flex justify-content-between align-items-center">
                        <div class="flex-grow-1">
                            <div class="d-flex align-items-center mb-2">
                                <span class="badge bg-primary me-2 fs-6">#${index + 1}</span>
                                <h6 class="mb-0 fw-bold">${pName}</h6>
                            </div>
                            <small class="text-muted">
                                <i class="far fa-clock me-1"></i>
                                Scheduled: ${formatTime(pStartTime)}
                            </small>
                        </div>
                        <div class="btn-group">
                            <a href="/Appointments/Details/${pId}" class="btn btn-sm btn-outline-primary" title="View Appointment">
                                <i class="fas fa-eye"></i>
                            </a>
                            <a href="/Patients/Details/${pPatientId}" class="btn btn-sm btn-outline-info" title="View Patient">
                                <i class="fas fa-user"></i>
                            </a>
                            <button class="btn btn-sm btn-success" onclick="startAppointment('${pId}', '${pName}')" title="Start Appointment">
                                <i class="fas fa-play"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        ';
        }).join("")}
    `;
}

// Start appointment function
function startAppointment(appointmentId, patientName) {
    if (confirm(`Start appointment with ${patientName}?`)) {
        // Redirect to appointment details or treatment page
        window.location.href = `/Appointments/Details/${appointmentId}`;
    }
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