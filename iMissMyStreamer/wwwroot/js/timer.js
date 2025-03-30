let timerInterval;

window.startTimer = function (lastStreamTime, streamerShortName) {
    const display = document.getElementById("timer-display");
    if (!display) return;

    // Clear any existing interval to avoid stacking
    if (timerInterval) {
        clearInterval(timerInterval);
    }

    const streamTime = new Date(lastStreamTime);

    const update = () => {
        const now = new Date();
        const elapsed = new Date(now - streamTime);
        const days = Math.floor((now - streamTime) / (1000 * 60 * 60 * 24));
        const hours = elapsed.getUTCHours();
        const minutes = elapsed.getUTCMinutes();
        const seconds = elapsed.getUTCSeconds();

        display.textContent = `${days} days, ${hours} hours, ${minutes} minutes, ${seconds} seconds without ${streamerShortName}.`;
    };

    update(); // show immediately
    timerInterval = setInterval(update, 1000);
};
