document.addEventListener("DOMContentLoaded", function () {
    console.log("Reception actions script loaded");

    // Handle "Send to Doctor" button click - MOVED OUTSIDE SignalR promise
<<<<<<< HEAD
    document.addEventListener('click', function(e) {
        if (e.target && e.target.closest('.send-to-doctor-btn')) {
            e.preventDefault();
            e.stopPropagation();
            
            console.log("Send to Doctor button clicked");
            
=======
    document.addEventListener('click', function (e) {
        if (e.target && e.target.closest('.send-to-doctor-btn')) {
            e.preventDefault();
            e.stopPropagation();

            console.log("Send to Doctor button clicked");

>>>>>>> finish
            const button = e.target.closest('.send-to-doctor-btn');
            const appointmentId = button.getAttribute('data-appointment-id');
            const patientName = button.getAttribute('data-patient-name');
            const appointmentRow = document.getElementById(`appointment-${appointmentId}`);

            console.log("Appointment ID:", appointmentId);
            console.log("Patient Name:", patientName);

            // Disable button immediately to prevent double-clicks
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i> Sending...';

            // Get anti-forgery token
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            console.log("Anti-forgery token found:", !!token);
            console.log("Token value:", token);

            if (!token) {
                showToast('Anti-forgery token not found. Please refresh the page.', 'error');
                button.disabled = false;
                button.innerHTML = '<i class="fas fa-paper-plane me-1"></i> Send to Doctor';
                return;
            }

            // Create FormData to send the token properly
            const formData = new FormData();
            formData.append('appointmentId', appointmentId);
            formData.append('__RequestVerificationToken', token);

            // Call backend controller with proper headers
            fetch(`/Notifications/AddToQueue`, {
                method: "POST",
                headers: {
                    'RequestVerificationToken': token  // Custom header (backup)
                },
                body: formData  // Send as form data
            })

<<<<<<< HEAD
            .then(response => {
                console.log("Response status:", response.status);
                console.log("Response headers:", [...response.headers.entries()]);
                if (!response.ok) {
                    return response.text().then(text => {
                        console.error("Response text:", text);
                        throw new Error(`Server error: ${response.status} - ${text}`);
                    });
                }
                return response.json();
            })
            .then(data => {
                console.log("Server response:", data);

                if (data.success) {
                    // Hide the appointment row from the table (it's now in queue)
                    if (appointmentRow) {
                        appointmentRow.style.transition = 'opacity 0.3s ease';
                        appointmentRow.style.opacity = '0';
                        setTimeout(() => {
                            appointmentRow.style.display = 'none';
                            // Update status badge for when it's shown again
                            const statusBadge = appointmentRow.querySelector(`#status-${appointmentId}`);
                            if (statusBadge) {
                                statusBadge.className = 'badge bg-info status-badge';
                                statusBadge.textContent = 'Notified';
                            }
                        }, 300);
                    }

                    showToast(`${patientName} sent to doctor successfully`, 'success');
                    
                    // Reload queue
                    loadQueue();
                } else {
                    throw new Error(data.message || 'Failed to send patient to doctor');
                }
            })
            .catch(err => {
                console.error("Error:", err);
                // Re-enable button on error
                button.disabled = false;
                button.innerHTML = '<i class="fas fa-paper-plane me-1"></i> Send to Doctor';
                showToast(`Failed to send patient: ${err.message}`, 'error');
            });
=======
                .then(response => {
                    console.log("Response status:", response.status);
                    console.log("Response headers:", [...response.headers.entries()]);
                    if (!response.ok) {
                        return response.text().then(text => {
                            console.error("Response text:", text);
                            throw new Error(`Server error: ${response.status} - ${text}`);
                        });
                    }
                    return response.json();
                })
                .then(data => {
                    console.log("Server response:", data);

                    if (data.success) {
                        // Hide the appointment row from the table (it's now in queue)
                        if (appointmentRow) {
                            appointmentRow.style.transition = 'opacity 0.3s ease';
                            appointmentRow.style.opacity = '0';
                            setTimeout(() => {
                                appointmentRow.style.display = 'none';
                                // Update status badge for when it's shown again
                                const statusBadge = appointmentRow.querySelector(`#status-${appointmentId}`);
                                if (statusBadge) {
                                    statusBadge.className = 'badge bg-info status-badge';
                                    statusBadge.textContent = 'Notified';
                                }
                            }, 300);
                        }

                        showToast(`${patientName} sent to doctor successfully`, 'success');

                        // Reload queue
                        loadQueue();
                    } else {
                        throw new Error(data.message || 'Failed to send patient to doctor');
                    }
                })
                .catch(err => {
                    console.error("Error:", err);
                    // Re-enable button on error
                    button.disabled = false;
                    button.innerHTML = '<i class="fas fa-paper-plane me-1"></i> Send to Doctor';
                    showToast(`Failed to send patient: ${err.message}`, 'error');
                });
>>>>>>> finish
        }
    });

    // Initialize SignalR connection
    window.appConnectionPromise.then(connection => {
        if (!connection) {
            console.error("Cannot execute reception actions, SignalR connection failed.");
            return;
        }

        console.log("SignalR connected for reception actions");

        // Listen for patient sent to doctor (from any receptionist)
        connection.on("PatientSentToDoctor", (queueData) => {
            console.log(`✅ Patient sent to doctor. Queue updated:`, queueData);
<<<<<<< HEAD
            
=======

>>>>>>> finish
            // Update queue display for receptionist
            const queueList = document.getElementById('queueList');
            if (queueList) {
                updateQueue(queueData);
            }
<<<<<<< HEAD
            
=======

>>>>>>> finish
            // Show notification
            if (queueData && queueData.length > 0) {
                const latestPatient = queueData[queueData.length - 1];
                showToast(`Patient ${latestPatient.patientName} sent to doctor`, 'info');
            }
        });

        // Listen for patient completion from doctor
        connection.on("PatientCompleted", (appointmentId, patientName, updatedQueue) => {
            console.log(`✅ Doctor finished with patient: ${patientName}`);
<<<<<<< HEAD
            
=======

>>>>>>> finish
            // Remove appointment row from table (it's completed, no longer needs to show in scheduled)
            const appointmentRow = document.getElementById(`appointment-${appointmentId}`);
            if (appointmentRow) {
                // Update status badge
                const statusBadge = appointmentRow.querySelector(`#status-${appointmentId}`);
                if (statusBadge) {
                    statusBadge.className = 'badge bg-success status-badge';
                    statusBadge.textContent = 'Completed';
                }
<<<<<<< HEAD
                
=======

>>>>>>> finish
                // Update action button if exists
                const actionCell = appointmentRow.querySelector('.send-to-doctor-btn')?.closest('td');
                if (actionCell) {
                    actionCell.innerHTML = `
                        <button class="btn btn-outline-success btn-sm" disabled>
                            <i class="fas fa-check me-1"></i> Completed
                        </button>`;
                }
            }
<<<<<<< HEAD
            
            // Update queue display
            updateQueue(updatedQueue);
            
=======

            // Update queue display
            updateQueue(updatedQueue);

>>>>>>> finish
            // Show notification
            showToast(`Patient ${patientName} session completed`, 'success');
        });

        // Load initial queue on page load
        loadQueue();

    }).catch(err => {
        console.error("Error during SignalR promise resolution: ", err);
        // Even if SignalR fails, we can still send patients to doctor
        console.log("SignalR failed but button click handler is still active");
    });

    // Load queue immediately (don't wait for SignalR)
    loadQueue();
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
    const queueCount = document.getElementById("queueCount");

    if (!queueList) return; // Exit if we're not on receptionist view

    if (!queue || queue.length === 0) {
        queueList.innerHTML = "";
        if (emptyState) emptyState.style.display = "block";
        if (queueCount) queueCount.textContent = "0 in Queue";
        return;
    }

    if (emptyState) emptyState.style.display = "none";
    if (queueCount) queueCount.textContent = `${queue.length} in Queue`;
<<<<<<< HEAD
    
=======

>>>>>>> finish
    queueList.innerHTML = queue.map((p, index) => {
        const pName = p.patientName || p.PatientName;
        const pId = p.id || p.Id;
        const pPatientId = p.patientId || p.PatientId;
        const pStartTime = p.startTime || p.StartTime;
<<<<<<< HEAD
        
=======

>>>>>>> finish
        return `
        <div class="card mb-2 queue-item shadow-sm">
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
                    <span class="badge bg-info me-2">
                        <i class="fas fa-clock me-1"></i> Waiting
                    </span>
                    <a href="/Appointments/Details/${pId}" class="btn btn-sm btn-outline-primary">
                        <i class="fas fa-eye"></i>
                    </a>
                </div>
            </div>
        </div>
    `;
    }).join("");
}

// Helper function to format time
function formatTime(timeString) {
    if (!timeString) return 'N/A';
    try {
        const time = new Date(timeString);
        return time.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit',
            hour12: true
        });
    } catch (error) {
        return timeString;
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
<<<<<<< HEAD
    
=======

>>>>>>> finish
    const toast = document.createElement('div');
    const alertClass = type === 'success' ? 'success' : type === 'error' ? 'danger' : type === 'warning' ? 'warning' : 'info';
    toast.className = `alert alert-${alertClass} alert-dismissible fade show shadow`;
    toast.role = 'alert';
    toast.innerHTML = `
        <strong><i class="fas fa-bell me-2"></i></strong>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
<<<<<<< HEAD
    
    toastContainer.appendChild(toast);
    
=======

    toastContainer.appendChild(toast);

>>>>>>> finish
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 150);
    }, 5000);
}