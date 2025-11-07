document.addEventListener("DOMContentLoaded", function () {
    window.appConnectionPromise.then(connection => {
        if (!connection) {
            console.error("Cannot execute reception actions, SignalR connection failed.");
            return;
        }

        // Listen for patient completion from doctor
        connection.on("PatientCompleted", (appointmentId, name) => {
            console.log(`✅ Doctor finished with patient: ${name}`);
            
            // Update the appointment row if it exists
            const appointmentRow = document.getElementById(`appointment-${appointmentId}`);
            if (appointmentRow) {
                const statusBadge = appointmentRow.querySelector(`#status-${appointmentId}`);
                if (statusBadge) {
                    statusBadge.className = 'badge bg-success status-badge';
                    statusBadge.textContent = 'Completed';
                }
                
                // Update the queue action button - find the parent cell
                const queueButtons = appointmentRow.querySelectorAll('.add-to-queue-btn, button[disabled]');
                queueButtons.forEach(btn => {
                    const parentCell = btn.closest('td');
                    if (parentCell) {
                        parentCell.innerHTML = `
                            <button class="btn btn-outline-success btn-sm" disabled>
                                <i class="fas fa-check me-1"></i> Completed
                            </button>`;
                    }
                });
            }
            
            // Update queue display
            loadQueue();
            
            // Show notification
            if (typeof showToast === 'function') {
                showToast(`Patient ${name} completed`, 'success');
            } else {
                alert(`✅ Doctor finished with patient: ${name}`);
            }
        });

        // Listen for new patient added to queue (from other receptionists or same receptionist)
        connection.on("AddPatientToQueue", (fullQueueData) => {
            console.log(`Queue updated. Full queue data:`, fullQueueData);

            // --- Logic for Receptionist View ---
            const queueList = document.getElementById('queueList');
            if (queueList) {
                console.log("Updating receptionist's queue view.");
                updateQueue(fullQueueData);
            }

            // --- Logic for Doctor View ---
            const doctorQueueList = document.getElementById('doctorQueueList');
            const doctorWaitingState = document.getElementById('doctorWaitingState');

            if (doctorQueueList) {
                console.log("Updating doctor's queue view.");
                if (fullQueueData && fullQueueData.length > 0) {
                    if(doctorWaitingState) doctorWaitingState.style.display = 'none';
                    doctorQueueList.innerHTML = `
                        <h6 class="text-muted">Waiting Queue</h6>
                        ${fullQueueData.map((p, index) => `
                            <div class="d-flex justify-content-between align-items-center p-2 mb-2 border-bottom queue-item-small">
                                <div class="flex-grow-1">
                                    <div class="d-flex align-items-center">
                                        <span class="badge bg-primary me-2">#${index + 1}</span>
                                        <span class="fw-bold">${p.patientName}</span>
                                    </div>
                                    <small class="text-muted ms-4">Scheduled: ${formatTime(p.startTime)}</small>
                                </div>
                                <a href="/Appointments/Details/${p.id}" class="btn btn-sm btn-outline-info">
                                    <i class="fas fa-info-circle"></i>
                                </a>
                            </div>
                        `).join("")}
                    `;
                } else {
                    if(doctorWaitingState) doctorWaitingState.style.display = 'block';
                    doctorQueueList.innerHTML = "";
                }
            }
        });

        // Handle "Add to Queue" button click
        document.addEventListener('click', function(e) {
            if (e.target && e.target.closest('.add-to-queue-btn')) {
                e.preventDefault();
                e.stopPropagation();
                
                const button = e.target.closest('.add-to-queue-btn');
                const appointmentId = button.getAttribute('data-appointment-id');
                const patientName = button.getAttribute('data-patient-name');
                const parentCell = button.closest('td');
                const appointmentRow = document.getElementById(`appointment-${appointmentId}`);

                // Disable button immediately to prevent double-clicks
                button.disabled = true;
                button.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i> Adding...';

                // Get anti-forgery token
                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

                // Call backend controller
                fetch(`/Notifications/AddToQueue?appointmentId=${appointmentId}`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": token || ""
                    }
                })
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`Server error: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    console.log("Server response:", data);

                    if (data.success) {
                        // Update the appointment row status
                        if (appointmentRow) {
                            const statusBadge = appointmentRow.querySelector(`#status-${appointmentId}`);
                            if (statusBadge) {
                                statusBadge.className = 'badge bg-info status-badge';
                                statusBadge.textContent = 'Notified';
                            }
                        }

                        // Update button in parent cell
                        if (parentCell) {
                            parentCell.innerHTML = `
                                <button class="btn btn-outline-info btn-sm" disabled>
                                    <i class="fas fa-clock me-1"></i> In Queue
                                </button>`;
                        }

                        // Show success notification
                        showToast(`${patientName} added to queue successfully`, 'success');
                        
                        // Note: SignalR will broadcast the update to all connected clients
                        // including this one, which will update the queue display
                    } else {
                        throw new Error(data.message || 'Failed to add patient to queue');
                    }
                })
                .catch(err => {
                    console.error("Fetch error:", err);
                    // Re-enable button on error
                    button.disabled = false;
                    button.innerHTML = '<i class="fas fa-plus me-1"></i> Add to Queue';
                    showToast(`Failed to add patient to queue: ${err.message}`, 'error');
                });
            }
        });

        // Load initial queue on page load
        loadQueue();

    }).catch(err => {
        console.error("Error during SignalR promise resolution: ", err);
    });
});

// Load the current queue from the server
function loadQueue() {
    fetch('/Notifications/GetQueue')
        .then(response => response.json())
        .then(data => {
            console.log("Queue loaded:", data);
            updateQueue(data);
        })
        .catch(err => console.error("Error loading queue:", err));
}

function updateQueue(queue) {
    const queueList = document.getElementById("queueList");
    const emptyState = document.getElementById("emptyQueueState");
    const totalCount = document.getElementById("totalQueueCount");
    const waitingCount = document.getElementById("waitingQueueCount");
    const completedCount = document.getElementById("completedQueueCount");

    if (!queueList) return; // Exit if we're not on a page with queue

    if (!queue || queue.length === 0) {
        queueList.innerHTML = "";
        if (emptyState) emptyState.style.display = "block";
        if (totalCount) totalCount.textContent = "0 Total";
        if (waitingCount) waitingCount.textContent = "0 Waiting";
        if (completedCount) completedCount.textContent = "0 Completed";
        return;
    }

    if (emptyState) emptyState.style.display = "none";
    
    const notifiedCount = queue.filter(p => p.status === "Notified" || p.Status === "Notified").length;
    const completedPatients = queue.filter(p => p.status === "Completed" || p.Status === "Completed").length;
    
    queueList.innerHTML = queue.map((p, index) => {
        const pName = p.patientName || p.PatientName;
        const pStatus = p.status || p.Status;
        const pId = p.id || p.Id;
        const pStartTime = p.startTime || p.StartTime;
        
        return `
        <div class="card mb-2 queue-item">
            <div class="card-body d-flex align-items-center p-3">
                <span class="badge bg-primary me-3 queue-number fs-6">#${index + 1}</span>
                <div class="flex-grow-1">
                    <h6 class="mb-1 fw-bold">${pName}</h6>
                    <small class="text-muted">
                        <i class="far fa-clock me-1"></i>
                        Scheduled: ${formatTime(pStartTime)}
                    </small>
                </div>
                <div>
                    ${pStatus === "Notified" ?
                        `<span class="badge bg-info"><i class="fas fa-clock me-1"></i> Waiting</span>` :
                        `<span class="badge bg-success"><i class="fas fa-check me-1"></i> Completed</span>`
                    }
                    <a href="/Appointments/Details/${pId}" class="btn btn-sm btn-outline-primary ms-2">
                        <i class="fas fa-eye"></i> Details
                    </a>
                </div>
            </div>
        </div>
    `;
    }).join("");

    if (totalCount) totalCount.textContent = `${queue.length} Total`;
    if (waitingCount) waitingCount.textContent = `${notifiedCount} Waiting`;
    if (completedCount) completedCount.textContent = `${completedPatients} Completed`;
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

function transferToDoctor(appointmentId, patientName) {
    window.appConnectionPromise.then(connection => {
        if (connection) {
            connection.invoke("TransferToDoctor", appointmentId, patientName)
                .then(() => {
                    showToast(`Patient ${patientName} sent to doctor`, 'info');
                })
                .catch(err => console.error("Transfer error:", err));
        }
    });
}

function completeCurrentPatient() {
    const currentPatientElement = document.getElementById('doctorCurrentPatient');
    if (!currentPatientElement) return;
    
    // Get the current patient's appointment ID (you'll need to store this in a data attribute)
    const appointmentId = currentPatientElement.getAttribute('data-appointment-id');
    if (!appointmentId) {
        alert("No patient currently selected");
        return;
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
            showToast(`Patient ${data.patientName} completed successfully`, 'success');
            // The SignalR event will handle UI updates
        } else {
            throw new Error(data.message);
        }
    })
    .catch(err => {
        console.error("Error completing patient:", err);
        showToast(`Failed to complete patient: ${err.message}`, 'error');
    });
}

function startCurrentAppointment() {
    alert("Starting appointment... (This functionality can be implemented based on your requirements)");
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