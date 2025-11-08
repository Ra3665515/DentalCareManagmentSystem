


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
