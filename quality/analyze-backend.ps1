param(
    [ValidateSet('Coverage', 'Analysis')]
    [string]$Mode = 'Coverage',
    [string]$Filter,
    [string]$ProjectKey = $env:SONAR_PROJECT_KEY,
    [string]$ServerUrl = 'http://localhost:9000',
    [string]$ScannerPath = "$env:LOCALAPPDATA\BugBoard26\sonar-tools\dotnet-sonarscanner.exe"
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $repo 'backend/BugBoard26.slnx'
$settings = Join-Path $PSScriptRoot 'coverage.runsettings'

if ($Mode -eq 'Analysis') {
    if ($Filter) { throw 'Analysis richiede tutti i test: non usare Filter.' }
    if (!$ProjectKey -or !$env:SONAR_TOKEN) {
        throw 'Configurare SONAR_PROJECT_KEY e SONAR_TOKEN nella sessione; nessuna analisi avviata.'
    }
    $uri = [Uri]$ServerUrl
    if (!$uri.IsAbsoluteUri -or !$uri.IsLoopback -or $uri.Scheme -notin @('http', 'https')) {
        throw 'Questa configurazione usa soltanto un server SonarQube locale.'
    }
    if (!(Test-Path -LiteralPath $ScannerPath -PathType Leaf)) {
        throw 'SonarScanner non installato: seguire qualita_backend.md.'
    }
    $ServerUrl = $ServerUrl.TrimEnd('/')
    $headers = @{ Authorization = "Bearer $env:SONAR_TOKEN" }
    try {
        $status = Invoke-RestMethod "$ServerUrl/api/system/status" -Headers $headers
        if ($status.status -ne 'UP') { throw 'Server non pronto.' }
        $key = [Uri]::EscapeDataString($ProjectKey)
        $project = Invoke-RestMethod "$ServerUrl/api/components/show?component=$key" -Headers $headers
        if ($project.component.key -ne $ProjectKey -or $project.component.qualifier -ne 'TRK') {
            throw 'Progetto non disponibile.'
        }
    }
    catch {
        throw 'Server/progetto SonarQube locale non disponibile o token non autorizzato. Nessun progetto creato.'
    }
}

# Una directory distinta impedisce di importare report di esecuzioni precedenti.
$work = Join-Path ([IO.Path]::GetTempPath()) ("bugboard26-quality-" + [Guid]::NewGuid().ToString('N'))
$results = Join-Path $work 'results'
New-Item -ItemType Directory -Path $work | Out-Null
Push-Location $work
try {
    if ($Mode -eq 'Analysis') {
        & $ScannerPath begin "/k:$ProjectKey" "/d:sonar.host.url=$ServerUrl" `
            "/d:sonar.token=$env:SONAR_TOKEN" "/d:sonar.projectBaseDir=$repo\backend" `
            '/d:sonar.scanner.scanAll=false' `
            "/d:sonar.cs.opencover.reportsPaths=$results/*/coverage.opencover.xml" `
            '/d:sonar.coverage.exclusions=**/Migrations/**,**/bin/**,**/obj/**,**/*.g.cs,**/*.generated.cs,**/*.Designer.cs'
        if ($LASTEXITCODE -ne 0) { throw 'SonarScanner begin non riuscito.' }
    }

    & dotnet build $solution --configuration Release --no-incremental --disable-build-servers
    if ($LASTEXITCODE -ne 0) { throw 'Build non riuscita: analisi non pubblicata.' }
    $testArgs = @('test', $solution, '--configuration', 'Release', '--no-build',
        '--collect:XPlat Code Coverage', '--settings', $settings,
        '--results-directory', $results, '--logger', 'trx;LogFileName=backend.trx')
    if ($Filter) { $testArgs += @('--filter', $Filter) }
    & dotnet @testArgs
    if ($LASTEXITCODE -ne 0) { throw 'Test non riusciti: analisi non pubblicata.' }

    $reports = @(Get-ChildItem -LiteralPath $results -Directory | ForEach-Object {
        Get-ChildItem -LiteralPath $_.FullName -Filter 'coverage.opencover.xml' -File
    })
    if ($reports.Count -eq 0) { throw 'Report OpenCover assente: analisi non pubblicata.' }
    foreach ($report in $reports) {
        [xml]$coverage = Get-Content -LiteralPath $report.FullName -Raw
        if (!$coverage.CoverageSession.Modules.Module) { throw 'Report OpenCover vuoto.' }
        Write-Host "Coverage: $($report.FullName)"
    }
    if ($Filter) { Write-Warning 'Coverage parziale: sono stati eseguiti soltanto i test filtrati.' }
    if ($Mode -eq 'Analysis') {
        & $ScannerPath end "/d:sonar.token=$env:SONAR_TOKEN"
        if ($LASTEXITCODE -ne 0) { throw 'SonarScanner end non riuscito.' }
        Write-Host 'Report inviato al server locale; controllare elaborazione e Quality Gate nel progetto.'
    }
}
finally {
    Pop-Location
    Write-Host "Risultati e file temporanei: $work"
}
