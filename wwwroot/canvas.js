const canvas = document.getElementById("presas");
canvas.width = 1000;
canvas.height = 500;
const ctx = canvas.getContext("2d");

const fondo = new Image();
fondo.src = "images/imageCfe.jpg";

// Cortinas
const cortinas = [
    { x: 250, baseY: 169, height: 60, topW: 12, baseW: 98 }, // Angostura
    { x: 442, baseY: 287, height: 70, topW: 7, baseW: 65 },  // Chicoasén
    { x: 589, baseY: 342, height: 60, topW: 10, baseW: 60 }, // Malpaso
    { x: 710, baseY: 365, height: 30, topW: 30, baseW: 50 }, // Peñitas
    { x: 810, baseY: 384, height: 30, topW: 7, baseW: 30 },  // Juan de Grijalva
];

// Embalses
const embalses = [
    { x: 35, baseY: 500, width: 220, nivel: 30, dir: 1 },
    { x: 350, baseY: 450, width: 90, nivel: 40, dir: 1 },
    { x: 485, baseY: 380, width: 103, nivel: 35, dir: 1 },
    { x: 615, baseY: 380, width: 80, nivel: 35, dir: 1 },
    { x: 720, baseY: 180, width: 90, nivel: 35, dir: 1 },
];

const golfo = { x: 875, y: 390, width: 82, height: 20 };

let t = 0;
let datosPresas = null;

// Niveles actuales
let nivelesActuales = {
    angostura: { nivel: 0, porcentaje: 0, hora: '00:00' },
    chicoasen: { nivel: 0, porcentaje: 0, hora: '00:00' },
    malpaso: { nivel: 0, porcentaje: 0, hora: '00:00' },
    penitas: { nivel: 0, porcentaje: 0, hora: '00:00' },
    juanDeGrijalva: { nivel: 0, porcentaje: 0, hora: '00:00' }
};

// NUEVO: Coordenadas de etiquetas ajustadas
const labelCoords = [
    { x: 120, y: 70, name: 'Angostura' },
    { x: 350, y: 140, name: 'Chicoasén' },
    { x: 480, y: 220, name: 'Malpaso' },
    { x: 620, y: 270, name: 'J. Grijalva' },
    { x: 750, y: 300, name: 'Peñitas' }
];

const JSON_URL = 'http://localhost:5270/api/data';

async function fetchAndAnimate() {
    try {
        const response = await fetch(JSON_URL);
        const data = await response.json();
        console.log("datos recibidos", data);

        datosPresas = data.presas;

        if (fondo.complete) {
            loop();
        }

    } catch (error) {
        console.error('Error al cargar datos. ¿Está el servicio de C# corriendo?', error);
    }
}

function animarAgua() {
    const presasNombres = ['angostura', 'chicoasen', 'malpaso', 'juanDeGrijalva', 'penitas'];

    presasNombres.forEach((nombrePresa, i) => {
        if (i >= embalses.length) return;

        const e = embalses[i];
        const c = cortinas[i];
        const datosRef = datosPresas[nombrePresa];

        if (datosRef) {
            // --- Animación del agua: usa solo el porcentaje ---
            const porcentaje = parseFloat(datosRef.porcentaje) || 0;
            const nivelMapeado = (porcentaje / 100) * c.height;
            e.nivel = Math.max(0, Math.min(nivelMapeado, c.height));

            // --- Datos para etiquetas ---
            nivelesActuales[nombrePresa].nivel = (datosRef.nivel !== null && datosRef.nivel !== undefined)
                ? parseFloat(datosRef.nivel).toFixed(2)
                : 'N/D';

            nivelesActuales[nombrePresa].porcentaje = porcentaje.toFixed(1);
            nivelesActuales[nombrePresa].hora = datosRef.hora || 'N/A';
        } else {
            e.nivel = 0;
            nivelesActuales[nombrePresa].nivel = 'N/D';
            nivelesActuales[nombrePresa].porcentaje = 'N/D';
            nivelesActuales[nombrePresa].hora = 'N/A';
        }
    });
}


// --- Dibujo de cortinas ---
function drawCortina(ctx, c) {
    const yTop = c.baseY - c.height;
    const halfTop = c.topW / 2;
    const halfBase = c.baseW / 2;

    ctx.beginPath();
    ctx.moveTo(c.x - halfBase, c.baseY);
    ctx.lineTo(c.x - halfTop, yTop);
    ctx.lineTo(c.x + halfTop, yTop);
    ctx.lineTo(c.x + halfBase, c.baseY);
    ctx.closePath();

    ctx.fillStyle = "#c28b57";
    ctx.fill();
    ctx.lineWidth = 2;
    ctx.strokeStyle = "#5d4226";
    ctx.stroke();
}

// --- Dibujo del agua ---
function drawAgua(ctx, e, c) {
    const nivel = Math.min(e.nivel, c.height);
    const top = c.baseY - nivel;

    const amplitude = 3;
    const wavelength = 10;

    ctx.beginPath();
    ctx.moveTo(e.x, top);

    for (let x = 0; x <= e.width; x += 5) {
        const y = top + Math.sin((x + t) / wavelength) * amplitude;
        ctx.lineTo(e.x + x, y);
    }

    ctx.lineTo(e.x + e.width, c.baseY);
    ctx.lineTo(e.x, c.baseY);
    ctx.closePath();

    ctx.fillStyle = "rgba(41, 138, 218, 0.91)";
    ctx.fill();
}

// --- Golfo ---
function drawGolfoOndas(ctx, g) {
    const top = g.y;
    const amplitude = 3;
    const wavelength = 11;

    ctx.beginPath();
    ctx.moveTo(g.x, top);

    for (let x = 0; x <= g.width; x += 5) {
        const y = top + Math.sin((x + t) / wavelength) * amplitude;
        ctx.lineTo(g.x + x, y);
    }

    ctx.lineTo(g.x + g.width, g.y + g.height);
    ctx.lineTo(g.x, g.y + g.height);
    ctx.closePath();

    ctx.fillStyle = "rgba(41, 138, 218, 0.91)";
    ctx.fill();
}

// --- Línea de colina ---
function drawColina(ctx) {
    ctx.beginPath();
    ctx.moveTo(42, 115);
    ctx.bezierCurveTo(225, 199, 320, 140, 338, 190);
    ctx.bezierCurveTo(400, 340, 482, 240, 510, 315);
    ctx.bezierCurveTo(695, 365, 600, 335, 790, 380);
    ctx.bezierCurveTo(900, 400, 850, 365, 910, 412);

    ctx.lineWidth = 3;
    ctx.strokeStyle = "black";
    ctx.stroke();
}

// --- Líneas azules ---
function drawLineaAzul(ctx, t) {
    const points = [];

    for (let t0 = 0; t0 <= 1; t0 += 0.02) {
        const x = cubicBezier(t0, 272, 285, 320, 344);
        const y = cubicBezier(t0, 145, 185, 140, 190);
        points.push({ x, y });
    }

    for (let t1 = 0; t1 <= 1; t1 += 0.02) {
        const x = cubicBezier(t1, 344, 410, 482, 430);
        const y = cubicBezier(t1, 190, 340, 230, 340);
        points.push({ x, y });
    }

    ctx.beginPath();
    for (let i = 0; i < points.length; i++) {
        const amplitude = 1;
        const yOffset = Math.sin(i * 0.5 - t * 0.3) * amplitude;

        if (i === 0) ctx.moveTo(points[i].x, points[i].y + yOffset);
        else ctx.lineTo(points[i].x, points[i].y + yOffset);
    }
    ctx.lineWidth = 8;
    ctx.strokeStyle = "rgba(86, 165, 230, 0.91)";
    ctx.stroke();
}

function drawLineaAzul2(ctx, t) {
    const points = [];
    for (let t0 = 0; t0 <= 1; t0 += 0.02) {
        const x = cubicBezier(t0, 342, 362, 480, 514);
        const y = cubicBezier(t0, 295, 298, 242, 311);
        points.push({ x, y });
    }
    for (let t1 = 0; t1 <= 1; t1 += 0.02) {
        const x = cubicBezier(t1, 514, 695, 598, 928);
        const y = cubicBezier(t1, 311, 355, 340, 401);
        points.push({ x, y });
    }

    ctx.beginPath();
    for (let i = 0; i < points.length; i++) {
        const amplitude = 3;
        const yOffset = Math.sin(i * 0.5 - t * 0.1) * amplitude;
        if (i === 0) ctx.moveTo(points[i].x, points[i].y + yOffset);
        else ctx.lineTo(points[i].x, points[i].y + yOffset);
    }
    ctx.lineWidth = 9;
    ctx.strokeStyle = "rgba(86, 165, 230, 0.91)";
    ctx.stroke();
}

// --- Cubic Bezier ---
function cubicBezier(t, p0, p1, p2, p3) {
    const c = 3 * (p1 - p0);
    const b = 3 * (p2 - p1) - c;
    const a = p3 - p0 - c - b;
    return a * t * t * t + b * t * t + c * t + p0;
}

// --- Datos sobre el canvas ---
function drawDataOverlay(ctx) {
    if (!datosPresas) return;

    const presasNombres = ['angostura', 'chicoasen', 'malpaso', 'juanDeGrijalva', 'penitas'];

    // --- Encabezado general ---
    ctx.fillStyle = '#4bc119ff';
    ctx.font = 'bold 16px Arial';
    ctx.fillText(`Hora: ${nivelesActuales.angostura.hora}`, 10, 20);

    ctx.font = '14px Arial';
    ctx.fillText(`Nivel Dinámico (m.s.n.m / %)`, 10, 40);

    presasNombres.forEach((key, i) => {
        if (i >= labelCoords.length) return;

        const datos = nivelesActuales[key];
        const coord = labelCoords[i];

        // --- Nombre de la presa ---
        ctx.fillStyle = '#0a2333';
        ctx.font = 'bold 13px Arial';
        ctx.fillText(coord.name, coord.x, coord.y);

        // --- Nivel m.s.n.m. ---
        ctx.fillStyle = '#1e1d1dff';
        ctx.font = '12px Arial';
        if (datos.nivel !== null && datos.nivel !== 'N/D') {
            ctx.fillText(`Nivel: ${datos.nivel} m.s.n.m.`, coord.x, coord.y + 15);
        } else {
            ctx.fillText(`Nivel: N/D`, coord.x, coord.y + 15);
        }

        // --- Porcentaje ---
        let colorPct = 'black';
        if (datos.porcentaje !== 'N/D') {
            const pctNum = parseFloat(datos.porcentaje);
            if (pctNum < 50) colorPct = 'red';
            else if (pctNum > 80) colorPct = 'black';
        }
        ctx.fillStyle = colorPct;
        ctx.font = 'bold 14px Arial';
        ctx.fillText(`${datos.porcentaje}%`, coord.x, coord.y + 30);
    });
}



function drawScene() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.drawImage(fondo, 0, 0, canvas.width, canvas.height);

    ctx.save();
    ctx.beginPath();
    ctx.moveTo(0, 0);
    ctx.moveTo(48, 115);
    ctx.bezierCurveTo(180, 195, 320, 140, 335, 190);
    ctx.bezierCurveTo(400, 340, 490, 240, 510, 315);
    ctx.bezierCurveTo(670, 365, 600, 335, 790, 380);
    ctx.bezierCurveTo(900, 400, 850, 370, 890, 600);
    ctx.lineTo(canvas.width, 0);
    ctx.closePath();
    ctx.clip();

    drawLineaAzul(ctx, t);
    drawLineaAzul2(ctx, t);

    embalses.forEach((e, i) => drawAgua(ctx, e, cortinas[i]));
    cortinas.forEach(c => drawCortina(ctx, c));

    ctx.restore();

    drawGolfoOndas(ctx, golfo);

    ctx.beginPath();
    ctx.moveTo(48, 115);
    ctx.bezierCurveTo(180, 195, 320, 140, 335, 190);
    ctx.bezierCurveTo(400, 340, 482, 240, 510, 315);
    ctx.bezierCurveTo(670, 365, 600, 335, 790, 380);
    ctx.bezierCurveTo(900, 400, 850, 370, 910, 412);
    ctx.lineTo(1000, 500);
    ctx.lineTo(0, 500);
    ctx.closePath();

    ctx.fillStyle = "white";
    ctx.fill(); // <-- rellenar antes de las etiquetas

    drawColina(ctx);
    drawDataOverlay(ctx); // <-- dibujar etiquetas después
}


function loop() {
    animarAgua();
    drawScene();
    t += 0.2;
    requestAnimationFrame(loop);
}

fondo.onload = () => fetchAndAnimate();
