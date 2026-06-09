Write-Host "=============================================" -ForegroundColor Cyan
Write-Host " INICIANDO TEST DE INTEGRACION EN TIEMPO REAL" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# 1. Iniciar la aplicacion web en segundo plano (sin recompilar)
Write-Host "[1/6] Iniciando servidor web de desarrollo ASP.NET Core..." -ForegroundColor Yellow
$process = Start-Process dotnet -ArgumentList "run --no-build" -NoNewWindow -PassThru -WorkingDirectory "c:\Users\eric0\OneDrive\Desktop\ZanganosSA\ColmenaEmpresa\ColmenaEmpresa\ColmenaEmpresaMVC"

# Esperar hasta 30s que el servidor responda (polling activo)
Write-Host "      Esperando que el servidor arranque" -NoNewline -ForegroundColor Gray
$maxWait = 30
$started = $false
for ($i = 0; $i -lt $maxWait; $i++) {
    Start-Sleep -Seconds 1
    Write-Host "." -NoNewline -ForegroundColor Gray
    try {
        $ping = Invoke-WebRequest -Uri "http://localhost:5058/" -UseBasicParsing -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($ping.StatusCode -eq 200) {
            $started = $true
            break
        }
    } catch {}
}
Write-Host ""

if (-not $started) {
    Write-Host "      [ERROR] El servidor no respondio en $maxWait segundos. Abortando." -ForegroundColor Red
    Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $PID } | Stop-Process -Force -ErrorAction SilentlyContinue
    exit 1
}
Write-Host "      [OK] Servidor activo en http://localhost:5058" -ForegroundColor Green

$success = $true

try {
    # 2. Test Dashboard
    Write-Host "[2/6] Verificando pagina de inicio (Dashboard)..." -ForegroundColor Yellow
    $resDashboard = Invoke-WebRequest -Uri "http://localhost:5058/" -UseBasicParsing
    if ($resDashboard.StatusCode -eq 200) {
        Write-Host "      [OK] Dashboard cargado exitosamente (HTTP 200)" -ForegroundColor Green
    } else {
        Write-Host "      [ERROR] Error al cargar Dashboard (HTTP $($resDashboard.StatusCode))" -ForegroundColor Red
        $success = $false
    }

    # 3. Test Apiarios
    Write-Host "[3/6] Verificando pagina de Apiarios..." -ForegroundColor Yellow
    $resApiarios = Invoke-WebRequest -Uri "http://localhost:5058/Apiarios" -UseBasicParsing
    if ($resApiarios.Content -like "*Monte Olivo*" -and $resApiarios.Content -like "*La Rinconada*") {
        Write-Host "      [OK] Listado de Apiarios cargado con datos en memoria" -ForegroundColor Green
    } else {
        Write-Host "      [ERROR] Error en listado de Apiarios (datos no encontrados)" -ForegroundColor Red
        $success = $false
    }

    # 4. Test Detalle Apiario
    Write-Host "[4/6] Verificando vista Detalle de Apiario (ID: 1)..." -ForegroundColor Yellow
    $resDetalle = Invoke-WebRequest -Uri "http://localhost:5058/Apiarios/Detalle/1" -UseBasicParsing
    if ($resDetalle.Content -like "*Detalle de Apiario*" -and $resDetalle.Content -like "*La Rinconada*") {
        Write-Host "      [OK] Vista de Detalle cargada exitosamente" -ForegroundColor Green
        Write-Host "      [OK] Datos del apiario y colmenas asociadas presentes" -ForegroundColor Green
    } else {
        Write-Host "      [ERROR] Error al cargar vista Detalle de Apiario" -ForegroundColor Red
        $success = $false
    }

    # 5. Test Colmenas
    Write-Host "[5/6] Verificando listado de Colmenas..." -ForegroundColor Yellow
    $resColmenas = Invoke-WebRequest -Uri "http://localhost:5058/Colmenas" -UseBasicParsing
    if ($resColmenas.Content -like "*C-01*" -and $resColmenas.Content -like "*C-47*") {
        Write-Host "      [OK] Listado de Colmenas cargado con exito" -ForegroundColor Green
    } else {
        Write-Host "      [ERROR] Error en listado de Colmenas (datos no encontrados)" -ForegroundColor Red
        $success = $false
    }

    # 6. Test Inspecciones
    Write-Host "[6/6] Verificando pagina de Inspecciones..." -ForegroundColor Yellow
    $resInspecciones = Invoke-WebRequest -Uri "http://localhost:5058/Inspecciones" -UseBasicParsing
    if ($resInspecciones.StatusCode -eq 200) {
        Write-Host "      [OK] Historial de Inspecciones responde correctamente" -ForegroundColor Green
    } else {
        Write-Host "      [ERROR] Error en Inspecciones (HTTP $($resInspecciones.StatusCode))" -ForegroundColor Red
        $success = $false
    }

} catch {
    Write-Host "      [ERROR] Excepcion HTTP: $_" -ForegroundColor Red
    $success = $false
} finally {
    Write-Host "Deteniendo servidor de desarrollo..." -ForegroundColor Yellow
    Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $PID } | Stop-Process -Force -ErrorAction SilentlyContinue
}

Write-Host "=============================================" -ForegroundColor Cyan
if ($success) {
    Write-Host "   TEST COMPLETADO: TODO FUNCIONA CORRECTAMENTE!" -ForegroundColor Green
} else {
    Write-Host "   TEST FALLIDO: REVISAR ERRORES DETALLADOS" -ForegroundColor Red
}
Write-Host "=============================================" -ForegroundColor Cyan
