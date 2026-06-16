/* Scripts globales — ZanganosSA */

function toggleNotifPanel() {
    document.getElementById('notif-panel').classList.toggle('open');
}

function toast(msg, isError = false) {
    const el = document.getElementById('toast-el');
    const msgEl = document.getElementById('toast-msg');
    if (el && msgEl) {
        msgEl.textContent = msg;
        el.style.borderLeftColor = isError ? 'var(--red)' : 'var(--green)';
        el.style.borderColor     = isError ? 'var(--red-bdr)' : 'var(--green-bdr)';
        el.classList.add('show');
        setTimeout(() => el.classList.remove('show'), 3500);
    }
}

function toastError(msg) { toast(msg, true); }

// Hex background effect
(function () {
    const cv = document.getElementById('hex-bg');
    if (!cv) return;
    const cx = cv.getContext('2d');
    const R = 26, GAP = 3;
    let mx = -9999, my = -9999;
    let hexes = [];

    function build() {
        hexes = [];
        const W = (2 * R + GAP) * 0.75, H = (Math.sqrt(3) * R + GAP);
        for (let c = -1; c < Math.ceil(cv.width / W) + 2; c++) {
            for (let r = -1; r < Math.ceil(cv.height / H) + 2; r++) {
                hexes.push({ x: c * W, y: r * H + (c % 2 ? H / 2 : 0), g: 0 });
            }
        }
    }

    function hex(x, y, r) {
        cx.beginPath();
        for (let i = 0; i < 6; i++) {
            const a = Math.PI / 180 * (60 * i - 30);
            i ? cx.lineTo(x + r * Math.cos(a), y + r * Math.sin(a)) : cx.moveTo(x + r * Math.cos(a), y + r * Math.sin(a));
        }
        cx.closePath();
    }

    function resize() {
        cv.width = window.innerWidth; cv.height = window.innerHeight; build();
    }

    const isDark = () => document.documentElement.getAttribute('data-theme') !== 'light';

    function draw() {
        cx.clearRect(0, 0, cv.width, cv.height);
        hexes.forEach(h => {
            const d = Math.hypot(mx - h.x, my - h.y);
            h.g += ((d < 120 ? (1 - d / 120) * 0.7 : 0) - h.g) * 0.12;
            const dark = isDark();
            const base = dark ? 0.032 : 0.05;
            const a = base + h.g * (dark ? 0.6 : 0.35);
            hex(h.x, h.y, R - 1.5);
            cx.fillStyle = dark ? `rgba(242,190,48,${(a * .4).toFixed(3)})` : `rgba(184,130,10,${(a * .3).toFixed(3)})`;
            cx.fill();
            cx.strokeStyle = dark ? `rgba(242,190,48,${a.toFixed(3)})` : `rgba(184,130,10,${(a * .8).toFixed(3)})`;
            cx.lineWidth = h.g > 0.06 ? 1.3 : 0.6;
            cx.stroke();
        });
        requestAnimationFrame(draw);
    }

    window.addEventListener('resize', resize);
    window.addEventListener('mousemove', e => { mx = e.clientX; my = e.clientY; });
    resize(); draw();
})();

// Theme toggle — persiste en localStorage
(function () {
    const saved = localStorage.getItem('theme') || 'dark';
    document.documentElement.setAttribute('data-theme', saved);
    const btn = document.getElementById('theme-btn');
    if (btn) btn.textContent = saved === 'dark' ? '🌙' : '☀️';
})();

document.getElementById('theme-btn')?.addEventListener('click', () => {
    const dark = document.documentElement.getAttribute('data-theme') === 'dark';
    const next = dark ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', next);
    localStorage.setItem('theme', next);
    document.getElementById('theme-btn').textContent = dark ? '☀️' : '🌙';
});
