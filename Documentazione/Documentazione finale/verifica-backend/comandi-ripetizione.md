# Ripetere la verifica backend

Comandi dalla radice del repository. Questa guida non attesta l'esecuzione
dei comandi Docker o Sonar: gli esiti effettivi sono in `report.md`.
Non modificare esclusioni, filtri, Quality Profile o Quality Gate per ottenere
un risultato migliore. Non usare il database di sviluppo per i test automatici.

## Suite completa e coverage

Con Docker disponibile, avviare soltanto il PostgreSQL di test gia' configurato:

```powershell
docker compose -p bugboard26-tests -f backend/BugBoard26.Tests/docker-compose.tests.yml up -d --wait
powershell -NoProfile -ExecutionPolicy Bypass -File quality/analyze-backend.ps1 -Mode Coverage
```

In alternativa usare un PostgreSQL dedicato: database `bugboard26_tests`, porta
55432, utente con CREATEDB. Configurare `BUGBOARD26_TEST_CONNECTION_STRING` nella
sessione secondo il README dei test. La factory crea ed elimina database distinti
per ogni caso e non usa come ripiego la connessione dell'API.

Non passare `-Filter`. Lo script esegue build Release non incrementale, tutta la
suite e OpenCover con `quality/coverage.runsettings`. Annotare commit, diff locale,
ora, codice di uscita e directory temporanea stampata al termine. Conservare
`backend.trx`, log e allegato OpenCover originale da `results/<guid>/`, evitando
di contare anche la copia dello stesso allegato prodotta dal logger TRX.

Per la sola suite senza coverage:

```powershell
dotnet test backend/BugBoard26.slnx --configuration Release
```

Per ripetere l'audit dipendenze dopo il restore:

```powershell
dotnet list backend/BugBoard26.slnx package --vulnerable --include-transitive --format json --no-restore
```

## SonarQube: da eseguire quando i prerequisiti sono disponibili

Usare il server locale e il progetto esistente concordati dal gruppo. Nessun
account o progetto nuovo e nessun invio cloud sono necessari. Controllare prima
`Documentazione finale/qualita_backend.md`, versione e compatibilita' del server.
Se manca lo scanner, il comando gia' documentato dal progetto e':

```powershell
dotnet tool install dotnet-sonarscanner --version 11.3.0 --allow-roll-forward --tool-path "$env:LOCALAPPDATA\BugBoard26\sonar-tools"
```

Questo comando di installazione non e' stato eseguito durante questa verifica.
Inserire project key e token soltanto nella sessione locale, non in file o chat:

```powershell
$env:SONAR_PROJECT_KEY = Read-Host 'Project key esistente'
$secret = Read-Host 'Token SonarQube' -AsSecureString
$ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secret)
try { $env:SONAR_TOKEN = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) }
finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr); $secret.Dispose() }
try {
    powershell -NoProfile -ExecutionPolicy Bypass -File quality/analyze-backend.ps1 -Mode Analysis
}
finally { Remove-Item Env:SONAR_TOKEN }
```

Per un'altra porta locale aggiungere `-ServerUrl`. Non avviare transcript della
sessione sensibile e non versionare directory di lavoro dello scanner. Esaminare
e ripulire i log prima di conservarli: i token non devono comparire nei report.
Lo script mantiene il flusso begin/build/test/end, misura solo il backend e
indica `sonar.cs.opencover.reportsPaths` per il report generato nella stessa run.

Dopo `end`, il semplice invio non dimostra l'importazione o il superamento del
gate. Individuare `report-task.txt` nella directory temporanea della run e:

1. Controllare il task Compute Engine tramite il relativo `ceTaskUrl`; attendere
   `SUCCESS`, non accettare `PENDING`, `IN_PROGRESS`, `FAILED` o `CANCELED`.
2. Conservare analysis ID, versione server/scanner, data, commit e diff locale.
3. Nei log verificare il parsing OpenCover e l'attribuzione ai sorgenti API.
   Controllare che non risultino report mancanti, zero file importati o percorsi
   non riconosciuti. La presenza dell'XML sul disco non prova l'importazione.
4. Sul progetto controllare `coverage`, `line_coverage`, `branch_coverage`,
   `lines_to_cover` e `uncovered_lines`; salvare misure e problemi significativi.
   Le percentuali Sonar non sono automaticamente quelle dei sequence point
   OpenCover: hanno denominatori diversi.
5. Consultare `api/qualitygates/project_status?analysisId=<analysis-id>` e
   conservare stato e condizioni del gate dell'analisi appena completata.
   Non usare un gate storico di un altro commit.

Riferimenti ufficiali: [scanner .NET](https://docs.sonarsource.com/sonarqube-server/analyzing-source-code/scanners/dotnet/using),
[coverage .NET](https://docs.sonarsource.com/sonarqube-server/analyzing-source-code/test-coverage/dotnet-test-coverage).

## Persistenza Docker: prova manuale ancora necessaria

Questa prova e' distinta dai test WebApplicationFactory: richiede veri container
e deve confrontare dati e byte dell'immagine dopo restart E ricreazione backend.
Non eseguire `down -v`, reset, prune o eliminazioni di volumi. Non cambiare nome
progetto Compose, database, credenziali o collegamenti dei volumi esistenti.
Se l'avvio abituale usa `-p` o `COMPOSE_PROJECT_NAME`, conservare quel valore in
tutti i comandi. Se il nome del progetto non e' noto, identificarlo prima di
avviare container: non accettare come prova un nuovo database vuoto per errore.

Con Docker disponibile, controllare contesto e container gia' presenti, poi
avviare il Compose esistente. Attendere `/api/health` prima di procedere:

```powershell
docker info
docker context show
docker compose -f docker/docker-compose.yml ps
docker compose -f docker/docker-compose.yml up -d --build
docker compose -f docker/docker-compose.yml exec backend sh -c 'id && test -w /var/lib/bugboard26/issue-images'
```

Il comando `id` deve indicare un utente non root. `test -w` deve riuscire sul
volume montato: il controllo statico nel Dockerfile non basta a dimostrarlo.
Usare PowerShell 7 per i comandi seguenti. Il token resta solo in memoria.

```powershell
$base = 'http://localhost:5256'
$run = 'VERIFICA-BACKEND-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0,8)
$evidence = Join-Path $env:TEMP $run
New-Item -ItemType Directory -Path $evidence | Out-Null
$credential = Get-Credential -Message 'ADMIN esistente: non modificare account o password'
$loginBody = @{ email=$credential.UserName; password=$credential.GetNetworkCredential().Password } | ConvertTo-Json
$login = Invoke-RestMethod "$base/api/auth/login" -Method Post -ContentType 'application/json' -Body $loginBody
$headers = @{ Authorization = 'Bearer ' + $login.token }
$loginBody = $null
$credential = $null
$picture = Join-Path $evidence ($run + '.png')
[IO.File]::WriteAllBytes($picture, [Convert]::FromBase64String('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAANSURBVBhXY/jPwPAfAAUAAf+mXJtdAAAAAElFTkSuQmCC'))
$body = @{ title=$run; description=('Prova persistenza riconoscibile: ' + $run); type='Bug'; assignedToId=$null } | ConvertTo-Json
$created = Invoke-RestMethod "$base/api/issues" -Method Post -Headers $headers -ContentType 'application/json' -Body $body
$id = $created.id
$uploaded = Invoke-RestMethod "$base/api/issues/$id/image" -Method Post -Headers $headers -Form @{image=(Get-Item -LiteralPath $picture)}
if (!$uploaded.hasImage) { throw 'Upload non confermato.' }
$expected = Invoke-RestMethod "$base/api/issues/$id" -Headers $headers | ConvertTo-Json -Depth 8 -Compress
$expectedHash = (Get-FileHash -LiteralPath $picture -Algorithm SHA256).Hash
$pgId = docker compose -f docker/docker-compose.yml ps -q postgres
$backendId = docker compose -f docker/docker-compose.yml ps -q backend

function Save-PersistenceEvidence([string]$phase) {
    $ready = $false
    for ($i=0; $i -lt 60; $i++) {
        try {
            $health = Invoke-RestMethod "$base/api/health" -TimeoutSec 2
            $ready = $true
            break
        } catch { Start-Sleep -Seconds 1 }
    }
    if (!$ready) { throw 'Backend non pronto.' }
    $actual = Invoke-RestMethod "$base/api/issues/$id" -Headers $headers | ConvertTo-Json -Depth 8 -Compress
    $download = Join-Path $evidence ($phase + '.png')
    Invoke-WebRequest "$base/api/issues/$id/image" -Headers $headers -OutFile $download
    $hash = (Get-FileHash -LiteralPath $download -Algorithm SHA256).Hash
    $currentBackend = docker compose -f docker/docker-compose.yml ps -q backend
    $currentPg = docker compose -f docker/docker-compose.yml ps -q postgres
    if ($actual -ne $expected -or $hash -ne $expectedHash -or $currentPg -ne $pgId) { throw 'Persistenza o identita del database non confermata.' }
    $mounts = (docker inspect $currentBackend --format '{{json .Mounts}}' | ConvertFrom-Json) | Select-Object Type,Name,Destination,RW
    @{ phase=$phase; observedUtc=[DateTimeOffset]::UtcNow.ToString('o'); run=$run; issueId=$id; issue=($actual | ConvertFrom-Json); imageSha256=$hash; backendContainer=$currentBackend; postgresContainer=$currentPg; mounts=$mounts } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $evidence ($phase + '.json'))
}

Save-PersistenceEvidence 'prima'
docker compose -f docker/docker-compose.yml restart backend
Save-PersistenceEvidence 'dopo-restart'
if ((docker compose -f docker/docker-compose.yml ps -q backend) -ne $backendId) { throw 'Il restart ha cambiato container: controllare la prova.' }
docker compose -f docker/docker-compose.yml up -d --no-deps --force-recreate backend
Save-PersistenceEvidence 'dopo-ricreazione'
if ((docker compose -f docker/docker-compose.yml ps -q backend) -eq $backendId) { throw 'Ricreazione del backend non dimostrata.' }
$headers.Clear()
$login = $null
```

Controllare nei tre JSON che il volume immagini abbia lo stesso nome e mount
scrivibile. Conservare solo evidenze senza token nella cartella di verifica,
indicando data, commit realmente nell'immagine e codici di uscita Docker.
I comandi qui sopra non salvano credenziali, token o l'intero `docker inspect`.
Ripetere il login se il token scade; non confondere un 401 con perdita di dati.
La issue di prova e la sua immagine restano riconoscibili nel database: non
eliminare o modificare altri dati per ripulire la prova.

Prima di chiudere la verifica della dipendenza OpenAPI, controllare anche
`GET /openapi/v1.json` in Development sull'immagine avviata: la sola compilazione
non verifica la generazione del documento.
