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
            
            // Show notification
            if (typeof showToast === 'function') {
                showToast(`Patient ${name} completed`, 'success');
            } else {
                alert(`✅ Doctor finished with patient: ${name}`);
            }
        });

        // Listen for new patient added to queue (from other receptionists)
        connection.on("AddPatientToQueue", (appointmentId, patientName, appointmentTime) => {
            console.log(`Patient ${patientName} was added to the queue.`);

            // --- Logic for Receptionist View ---
            // The receptionist has the main appointment table. We just remove the row.
            const appointmentRow = document.getElementById(`appointment-${appointmentId}`);
            if (appointmentRow) {
                console.log(`Removing appointment ${appointmentId} from receptionist view.`);
                appointmentRow.style.transition = 'opacity 0.5s ease';
                appointmentRow.style.opacity = '0';
                setTimeout(() => appointmentRow.remove(), 500);
            }

            // --- Logic for Doctor View ---
            // The doctor has the #doctorQueueList element. We refresh it.
            const doctorQueueList = document.getElementById('doctorQueueList');
            const doctorWaitingState = document.getElementById('doctorWaitingState');

            if (doctorQueueList) {
                console.log("Refreshing doctor's queue view.");
                fetch('/Notifications/GetQueue')
                    .then(response => response.json())
                    .then(queueData => {
                        if (queueData && queueData.length > 0) {
                            // Hide the "No patients waiting" message
                            if(doctorWaitingState) doctorWaitingState.style.display = 'none';

                            // Populate the queue list
                            doctorQueueList.innerHTML = `
                                <h6 class="text-muted">Waiting Queue</h6>
                                ${queueData.map(p => `
                                    <div class="d-flex justify-content-between align-items-center p-2 queue-item-small">
                                        <div>
                                            <span class="fw-bold">${p.patientName}</span>
                                            <small class="text-muted ms-2">Scheduled: ${new Date(p.startTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</small>
                                        </div>
                                        <button class="btn btn-sm btn-success" onclick="transferToDoctor('${p.id}', '${p.patientName}')">
                                            <i class="fas fa-arrow-right me-1"></i> Call In
                                        </button>
                                    </div>
                                `).join("")}
                            `;
                        } else {
                            // Show the "No patients waiting" message
                            if(doctorWaitingState) doctorWaitingState.style.display = 'block';
                            doctorQueueList.innerHTML = "";
                        }
                    })
                    .catch(err => console.error("Failed to get queue for doctor view:", err));
            }
        });

        const addBtn = document.getElementById("btnAddPatient");
        if (addBtn) {
            addBtn.addEventListener("click", () => {
                const name = document.getElementById("patientName").value;
                const id = document.getElementById("appointmentId").value;
                if (!name || !id) {
                    alert("Please enter patient name and appointment ID");
                    return;
                }
                connection.invoke("AddNewPatient", id, name);
                alert(`🟢 Patient ${name} added successfully`);
            });
        }

        const transferBtn = document.getElementById("btnTransfer");
        if (transferBtn) {
            transferBtn.addEventListener("click", () => {
                const name = document.getElementById("patientName").value;
                const id = document.getElementById("appointmentId").value;
                if (!name || !id) {
                    alert("Please enter patient name and appointment ID");
                    return;
                }
                connection.invoke("TransferToDoctor", id, name);
                alert(`➡️ Patient ${name} transferred to doctor`);
            });
        }

        // Handle "Add to Queue" button click
        document.querySelectorAll(".add-to-queue-btn").forEach(btn => {
            btn.addEventListener("click", function (e) {
                e.preventDefault();
                e.stopPropagation();
                
                const appointmentId = this.getAttribute('data-appointment-id');
                const patientName = this.getAttribute('data-patient-name');
                const button = this;
                const parentCell = button.closest('td');
                const appointmentRow = document.getElementById(`appointment-${appointmentId}`);

                // Disable button immediately to prevent double-clicks
                button.disabled = true;
                button.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i> Adding...';

                // Get anti-forgery token
                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

                // 1️⃣ Call backend controller with appointmentId as query parameter
                fetch(`/Notifications/AddToQueue?appointmentId=${appointmentId}`, {
                    method: "POST",
                    headers: token ? {
                        "RequestVerificationToken": token
                    } : {}
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            // Notify other clients via SignalR
                            // The UI update will be handled by the SignalR listener
                            connection.invoke("AddNewPatient", appointmentId, data.patientName);
                        }
                    })
                    .catch(err => {
                        console.error("Fetch error:", err);
                        // Re-enable button on error
                        button.disabled = false;
                        button.innerHTML = '<i class="fas fa-plus me-1"></i> Add to Queue';
                        alert('❌ Failed to add patient to queue. Please try again.');
                    });
            });
        });
    }).catch(err => {
        console.error("Error during SignalR promise resolution: ", err);
    });
});


function updateQueue(queue) {
    const queueList = document.getElementById("queueList");
    const emptyState = document.getElementById("emptyQueueState");
    const totalCount = document.getElementById("totalQueueCount");
    const waitingCount = document.getElementById("waitingQueueCount");
    const completedCount = document.getElementById("completedQueueCount");

    if (!queue || queue.length === 0) {
        queueList.innerHTML = "";
        if (emptyState) emptyState.style.display = "block";
        if (totalCount) totalCount.textContent = "0 Total";
        if (waitingCount) waitingCount.textContent = "0 Waiting";
        if (completedCount) completedCount.textContent = "0 Completed";
        return;
    }

    if (emptyState) emptyState.style.display = "none";
    queueList.innerHTML = queue.map((p, index) => `
        <div class="card mb-2 queue-item">
            <div class="card-body d-flex align-items-center p-2">
                <span class="badge bg-primary me-3 queue-number">#${index + 1}</span>
                <div class="flex-grow-1">
                    <h6 class="mb-0">${p.PatientName}</h6>
                    <small class="text-muted">Arrived: ${new Date(p.ArrivalTime).toLocaleTimeString()}</small>
                </div>
                ${p.Status === "Waiting" ?
            `<button class="btn btn-sm btn-success" onclick="transferToDoctor('${p.AppointmentId}', '${p.PatientName}')">
                        <i class="fas fa-arrow-right me-1"></i> Send to Doctor
                    </button>` :
            `<span class="badge bg-success"><i class="fas fa-check me-1"></i> Sent</span>`
        }
            </div>
        </div>
    `).join("");

    if (totalCount) totalCount.textContent = `${queue.length} Total`;
    if (waitingCount) waitingCount.textContent = `${queue.filter(p => p.Status === "Waiting").length} Waiting`;
    if (completedCount) completedCount.textContent = `${queue.filter(p => p.Status === "Completed").length} Completed`;
}

function updateDoctorView(waitingPatients, currentPatient) {
    const doctorWaitingState = document.getElementById("doctorWaitingState");
    const doctorCurrentPatient = document.getElementById("doctorCurrentPatient");
    const doctorQueueList = document.getElementById("doctorQueueList");
    const waitingPatientsCount = document.getElementById("waitingPatientsCount");
    const currentQueueNumber = document.getElementById("currentQueueNumber");

    if (!waitingPatientsCount) return;

    // Update waiting patients count
    waitingPatientsCount.textContent = `${waitingPatients.length} Waiting`;

    // Handle current patient display
    if (currentPatient) {
        if (doctorWaitingState) doctorWaitingState.style.display = "none";
        if (doctorCurrentPatient) doctorCurrentPatient.classList.remove("d-none");
        
        const currentPatientName = document.getElementById("currentPatientName");
        const currentAppointmentTime = document.getElementById("currentAppointmentTime");
        const currentArrivalTime = document.getElementById("currentArrivalTime");
        const currentPatientQueueNumber = document.getElementById("currentPatientQueueNumber");
        
        if (currentPatientName) currentPatientName.textContent = currentPatient.PatientName;
        if (currentAppointmentTime) currentAppointmentTime.textContent = `Appointment: ${new Date(currentPatient.AppointmentTime).toLocaleTimeString()}`;
        if (currentArrivalTime) currentArrivalTime.textContent = `Arrived: ${new Date(currentPatient.ArrivalTime).toLocaleTimeString()}`;
        if (currentPatientQueueNumber) currentPatientQueueNumber.textContent = `Queue #${currentPatient.QueueNumber}`;
        if (currentQueueNumber) currentQueueNumber.textContent = `#${currentPatient.QueueNumber}`;
    } else {
        if (doctorWaitingState) doctorWaitingState.style.display = "block";
        if (doctorCurrentPatient) doctorCurrentPatient.classList.add("d-none");
        if (currentQueueNumber) currentQueueNumber.textContent = "-";
    }

    // Update doctor's queue list
    if (doctorQueueList) {
        if (waitingPatients && waitingPatients.length > 0) {
            doctorQueueList.innerHTML = `
                <h6 class="text-muted">Next in Queue</h6>
                ${waitingPatients.map(p => `
                    <div class="d-flex justify-content-between align-items-center p-2 queue-item-small">
                        <div>
                            <span class="fw-bold">${p.PatientName}</span>
                            <small class="text-muted ms-2">Arrived: ${new Date(p.ArrivalTime).toLocaleTimeString()}</small>
                        </div>
                        <span class="badge bg-secondary">#${p.QueueNumber}</span>
                    </div>
                `).join("")}
            `;
        } else {
            doctorQueueList.innerHTML = "";
        }
    }
}

function transferToDoctor(appointmentId, patientName) {
    window.appConnectionPromise.then(connection => {
        if (connection) {
            connection.invoke("TransferToDoctor", appointmentId, patientName)
                .then(() => {
                    if (typeof showToast === 'function') {
                        showToast(`Patient ${patientName} sent to doctor`, 'info');
                    } else {
                        alert(`➡️ Patient ${patientName} sent to doctor`);
                    }
                })
                .catch(err => console.error("Transfer error:", err));
        }
    });
}

function completeCurrentPatient() {
    // Logic to mark the current patient as complete
    // This would typically involve getting the current patient's appointmentId
    // and invoking a SignalR method or making a fetch call.
    alert("Completing patient...");
}

function startCurrentAppointment() {
    // Logic to start the appointment
    alert("Starting appointment...");
}

// Helper function to show toast notifications
function showToast(message, type = 'info') {
    // Get or create toast container
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
    toast.className = `alert alert-${type === 'success' ? 'success' : type === 'error' ? 'danger' : 'info'} alert-dismissible fade show`;
    toast.role = 'alert';
    toast.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    toastContainer.appendChild(toast);
    
    setTimeout(() => {
        toast.remove();
    }, 5000);
}