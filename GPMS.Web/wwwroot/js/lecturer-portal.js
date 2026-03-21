// Lecturer Portal Interactions

document.addEventListener("DOMContentLoaded", () => {
    initNotifications();
    initCountdowns();
    initEvaluationForm();
});

function initNotifications() {
    const bell = document.getElementById('notificationToggle');
    const dropdown = document.getElementById('notificationDropdown');
    
    if (bell && dropdown) {
        bell.addEventListener('click', (e) => {
            e.stopPropagation();
            dropdown.classList.toggle('d-none');
        });

        document.addEventListener('click', (e) => {
            if (!dropdown.contains(e.target) && !bell.contains(e.target)) {
                dropdown.classList.add('d-none');
            }
        });
    }
}

function initCountdowns() {
    const timers = document.querySelectorAll('.countdown-timer');
    
    if(timers.length === 0) return;

    setInterval(() => {
        timers.forEach(timer => {
            const deadlineRaw = timer.getAttribute('data-deadline');
            if(!deadlineRaw) return;

            const deadline = new Date(deadlineRaw).getTime();
            const now = new Date().getTime();
            const diff = deadline - now;

            if (diff < 0) {
                timer.innerText = "Deadline Passed";
                timer.classList.add("urgent");
                return;
            }

            const days = Math.floor(diff / (1000 * 60 * 60 * 24));
            const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
            const seconds = Math.floor((diff % (1000 * 60)) / 1000);

            // If less than 24 hours, style it as urgent
            if(days === 0 && hours < 24) {
                if(!timer.classList.contains('urgent')) {
                    timer.classList.add('urgent');
                }
                timer.innerHTML = `<span class="material-symbols-outlined align-middle" style="font-size:16px;">schedule</span> ${hours}h ${minutes}m ${seconds}s`;
            } else {
                timer.innerText = `${days} Days ${hours}h`;
            }
        });
    }, 1000);
}

function initEvaluationForm() {
    const scoreInputs = document.querySelectorAll('.eval-score-input');
    const totalDisplay = document.getElementById('totalScoreDisplay');
    
    if(scoreInputs.length > 0 && totalDisplay) {
        scoreInputs.forEach(input => {
            input.addEventListener('input', calculateTotal);
        });
    }

    function calculateTotal() {
        let total = 0;
        let valid = true;
        scoreInputs.forEach(input => {
            const val = parseFloat(input.value);
            if(!isNaN(val)) {
                total += val;
            }
        });
        totalDisplay.innerText = total.toFixed(1);
    }
}
