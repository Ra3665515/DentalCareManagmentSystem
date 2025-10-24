// Load tab content (Diagnosis, Images, Treatment Plans)
function loadTabContent(tabName, url) {
    const container = document.getElementById(tabName + "Content");
    if (!container) return;

    container.innerHTML = "<p class='text-muted fst-italic'>Loading...</p>";

    fetch(url + "?patientId=" + patientId)
        .then(r => r.text())
        .then(html => container.innerHTML = html)
        .catch(err => container.innerHTML = "<p class='text-danger'>Failed to load data.</p>");
}

// Upload patient image
function uploadPatientImage() {
    const input = document.getElementById("patientImageInput");
    if (input.files.length === 0) return alert("Select a file first.");

    const formData = new FormData();
    formData.append("patientId", patientId);
    formData.append("imageFile", input.files[0]);

    fetch("/Patients/UploadPatientImage", {
        method: "POST",
        body: formData
    })
        .then(r => {
            if (r.ok) {
                input.value = "";
                loadTabContent("images", "/Patients/GetPatientImages");
            } else {
                alert("Failed to upload image.");
            }
        })
        .catch(() => alert("Error uploading image."));
}

// Add treatment plan
function addTreatmentPlan() {
    const input = document.getElementById("treatmentPlanInput");
    if (!input.value.trim()) return alert("Enter a plan first.");

    fetch("/TreatmentPlans/AddPlan", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: `patientId=${patientId}&plan=${encodeURIComponent(input.value.trim())}`
    })
        .then(r => {
            if (r.ok) {
                input.value = "";
                // إعادة تحميل الـ Partial مباشرة
                fetch(`/Patients/GetTreatmentPlans?patientId=${patientId}`)
                    .then(res => res.text())
                    .then(html => document.getElementById("treatmentList").innerHTML = html);
            } else alert("Failed to add treatment plan.");
        })
        .catch(() => alert("Error sending request."));
}


//// Reload the MVC Grid after adding or updating patients
//function reloadPatientsGrid() {
//    fetch('/Patients/GetPatientsGrid') // endpoint يرجع Partial View للـ Grid
//        .then(response => response.text())
//        .then(html => {
//            document.getElementById('patientsGridContainer').innerHTML = html;

//            // إعادة تهيئة الـ MVC Grid على الـ HTML الجديد
//            const gridEl = document.querySelector('#patientsGridContainer .mvc-grid');
//            if (gridEl) {
//                new MvcGrid(gridEl);
//                console.log('MVC Grid re-initialized successfully');
//            } else {
//                console.error('Grid element not found after reload');
//            }
//        })
//        .catch(err => console.error('Failed to reload grid', err));
//}

//// Add patient via AJAX
//function addPatient() {
//    const name = document.getElementById('patientNameInput').value.trim();
//    if (!name) return alert('Please enter a patient name');

//    fetch('/Patients/Add', {
//        method: 'POST',
//        headers: { 'Content-Type': 'application/json' },
//        body: JSON.stringify({ fullName: name })
//    })
//        .then(res => {
//            if (res.ok) {
//                document.getElementById('patientNameInput').value = '';
//                reloadPatientsGrid();
//            } else {
//                alert('Failed to add patient');
//            }
//        })
//        .catch(() => alert('Error sending request'));
//}



//// Initialize MVC Grid on page load
//document.addEventListener('DOMContentLoaded', () => {
//    const gridEl = document.querySelector('#patientsGridContainer .mvc-grid');
//    if (gridEl) {
//        new MvcGrid(gridEl);
//        console.log('MVC Grid initialized on page load');
//    }

//    // Load diagnosis tab initially
//    loadTabContent("diagnosis", "/Patients/GetDiagnosisNotes");
//});
