/* Global scripts for ApiApp */

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

// Tasks logic
function toggleTask(el) {
    el.classList.toggle('on');
    const row = el.closest('.task-row');
    const name = row.querySelector('.t-name');
    const done = el.classList.contains('on');
    row.classList.toggle('done', done);
    if (name) name.classList.toggle('t-done', done);
    const pending = document.querySelectorAll('#task-panel .chk:not(.on)').length;
    const el2 = document.getElementById('task-cnt');
    if (el2) el2.textContent = pending + ' pendiente' + (pending !== 1 ? 's' : '');
}

// Calendar logic
const calData = {
    22: [{ dot: 'var(--blue)', t: 'Visita — El Eucaliptal', m: '10:00 · Inspección completa' }, { dot: 'var(--amber)', t: 'Tratamiento Varroa', m: 'C-47, C-82' }, { dot: 'var(--gold)', t: 'Preparar centrífuga', m: 'Tarea — Galpón principal' }],
    10: [{ dot: 'var(--gold)', t: 'Cosecha Monte Olivo', m: '8:00 · 8 alzas estimadas' }, { dot: 'var(--blue)', t: 'Visita El Eucaliptal', m: '12:00' }],
    14: [{ dot: 'var(--blue)', t: 'Visita El Eucaliptal', m: '10:00' }, { dot: 'var(--amber)', t: 'Revisión alzas extra', m: 'Galpón' }],
    25: [{ dot: 'var(--blue)', t: 'Visita Paso Carrasco', m: 'Transhumancia — revisar adapatación' }],
};

function selectDay(d) {
    const dateEl = document.getElementById('cal-sel-date');
    if (dateEl) dateEl.textContent = `${d} de abril 2024`;
    const evList = document.getElementById('cal-ev-list');
    if (!evList) return;
    const evs = calData[d] || [];
    if (!evs.length) {
        evList.innerHTML = '<div style="padding:20px;text-align:center;color:var(--txt-4);font-size:13px">Sin actividades para este día.</div>';
        return;
    }
    evList.innerHTML = evs.map(e => `<div class="cal-ev-item"><div class="cal-ev-dot" style="background:${e.dot}"></div><div class="cal-ev-info"><div class="cal-ev-title">${e.t}</div><div class="cal-ev-meta">${e.m}</div></div></div>`).join('');
}

function buildCal() {
    const grid = document.getElementById('cal-grid');
    if (!grid) return;
    grid.innerHTML = '';
    const events = {
        3: [{ cls: 'ce-visit', lbl: 'Monte Olivo' }],
        7: [{ cls: 'ce-salud', lbl: 'Control Varroa' }],
        10: [{ cls: 'ce-cosecha', lbl: 'Cosecha #1' }],
        14: [{ cls: 'ce-visit', lbl: 'El Eucaliptal' }, { cls: 'ce-tarea', lbl: 'Revisión alzas' }],
        18: [{ cls: 'ce-visit', lbl: 'Monte Olivo' }],
        22: [{ cls: 'ce-visit', lbl: 'Eucaliptal' }, { cls: 'ce-tarea', lbl: 'Tratamiento V.' }, { cls: 'ce-tarea', lbl: 'Centrífuga' }],
        25: [{ cls: 'ce-visit', lbl: 'Paso Carrasco' }],
        28: [{ cls: 'ce-cosecha', lbl: 'Cosecha #2' }],
    };
    const offset = 0; // April 2024 starts on Monday
    const days = 30;
    for (let i = 0; i < offset; i++) {
        const c = document.createElement('div');
        c.className = 'cal-cell other-month';
        const d = document.createElement('div');
        d.className = 'cal-date'; d.textContent = 31 - offset + i + 1;
        c.appendChild(d); grid.appendChild(c);
    }
    for (let d = 1; d <= days; d++) {
        const c = document.createElement('div');
        c.className = 'cal-cell' + (d === 22 ? ' today' : '');
        c.onclick = () => selectDay(d);
        const dn = document.createElement('div');
        dn.className = 'cal-date'; dn.textContent = d;
        c.appendChild(dn);
        (events[d] || []).forEach(ev => {
            const e = document.createElement('div');
            e.className = 'cal-event ' + ev.cls;
            e.textContent = ev.lbl;
            c.appendChild(e);
        });
        grid.appendChild(c);
    }
    const remaining = (7 - (offset + days) % 7) % 7;
    for (let i = 1; i <= remaining; i++) {
        const c = document.createElement('div');
        c.className = 'cal-cell other-month';
        const dn = document.createElement('div');
        dn.className = 'cal-date'; dn.textContent = i;
        c.appendChild(dn); grid.appendChild(c);
    }
}
