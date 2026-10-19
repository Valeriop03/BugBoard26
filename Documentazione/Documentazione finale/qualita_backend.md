# Qualita' backend - SonarQube locale

## Esito corrente - 19 ottobre 2026

Analisi reale su `6aa175a` con correzioni locali non committate: **166/166 test
backend**, PostgreSQL dedicato e OpenCover importato su 29 file principali.
SonarQube Community Build 26.10.0.133078, scanner .NET 11.3.0. Coverage
complessiva 93,7%, linee 95,7%, rami 88,2%; **Quality Gate OK** con condizioni
valutate. Zero bug, zero vulnerabilita' e quattro code smell aperti.
Non e' una certificazione di assenza di ulteriori difetti.

Report, TRX, OpenCover, task e misure: [audit corrente](verifica-backend/audit-20261019.md).
Stato Git e hash distinguono il codice analizzato dal solo commit iniziale.
Nessun profilo, gate o esclusione modificato per migliorare i numeri.
RF-17 e persistenza Docker hanno ora evidenze reali nei report collegati.

## Aggiornamento documentale - 16 ottobre 2026, storico

Le sezioni seguenti conservano la configurazione e gli esiti **storici del 9 ottobre**. Non sono una nuova verifica della disponibilità attuale del server o di Docker. Il gruppo conferma il collaudo frontend reale di Valerio del 16 ottobre con backend Docker e PostgreSQL, riepilogato nel documento unico; questo non sostituisce suite backend, coverage completa e verifica di persistenza dopo ricreazione.

Amico 1 depositerà questi esiti e il report SonarQube effettivo in `verifica-backend/` alla radice, con configurazione, problemi rilevanti, importazione della coverage e Quality Gate se usato. I risultati non sono ancora disponibili. RF-17 è una decisione del gruppo: READONLY consulta le proprie notifiche ma non le segna lette. Backend e test ad amico 1; frontend da allineare e verificare. Questo stato tecnico rimane aperto e non è un chiarimento docente. Nessuna analisi è stata eseguita in questa revisione documentale. Stato e acquisizione delle evidenze sono in [evidenze.md](evidenze.md).

## Scelta e stato al 9 ottobre 2026, storico

Una sola soluzione: **SonarQube Server locale**, per non creare account/progetti cloud o pubblicare il codice. Il PC dispone di .NET 10 e coverlet.collector 6.0.4; non risultano disponibili un server SonarQube su localhost:9000, uno scanner installato o un token. La configurazione e' predisposta; **nessuna analisi Sonar e nessun Quality Gate sono stati eseguiti**.

Il server e un progetto C# gia' esistente devono essere forniti dal responsabile, con un token autorizzato ad analizzarlo. Lo script accetta solo URL locali e controlla l'esistenza del progetto prima di invocare lo scanner: non crea account/progetti e non installa server. Verificare la compatibilita' della versione del server con scanner e C#/.NET 10.

## Coverage ripetibile

Dalla radice, avviare il **Compose di test gia' esistente**, senza toccare PostgreSQL di sviluppo:

```powershell
docker compose -p bugboard26-tests -f backend/BugBoard26.Tests/docker-compose.tests.yml up -d --wait
powershell -NoProfile -ExecutionPolicy Bypass -File quality/analyze-backend.ps1 -Mode Coverage
```

Lo script esegue build Release non incrementale, `dotnet test --no-build --collect:"XPlat Code Coverage"` con `quality/coverage.runsettings`, e produce TRX e `coverage.opencover.xml` in `%TEMP%/bugboard26-quality-<id>/results`. Non aggiorna csproj, manifest o pacchetti. `ExecutionPolicy Bypass` e' limitato al processo; controllare lo script prima di eseguirlo, senza cambiare la policy globale.

OpenCover e' importato dal parametro C# `sonar.cs.opencover.reportsPaths`. Il filtro `[BugBoard26.Api]*` misura l'assembly applicativo, non test o librerie esterne. Esclusioni: migrazioni, bin/obj, `.g.cs`, `.generated.cs`, `.Designer.cs` e attributo `GeneratedCodeAttribute`. Controller, servizi, Program e modelli restano inclusi; non si saltano proprieta' o metodi async per migliorare i numeri. Il pattern del report seleziona gli allegati originali, non le copie annidate create dal logger TRX.

Senza PostgreSQL, solo per diagnostica parziale:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File quality/analyze-backend.ps1 -Mode Coverage -Filter 'Category!=Integration'
```

## Analisi sul server esistente

Installazione manuale dello scanner, fuori dal repository (versione fissata; comando non eseguito in questa revisione):

```powershell
dotnet tool install dotnet-sonarscanner --version 11.3.0 --allow-roll-forward --tool-path "$env:LOCALAPPDATA\BugBoard26\sonar-tools"
```

Con server locale e database di test pronti, inserire la chiave del progetto esistente e il token **solo nella sessione**:

```powershell
$env:SONAR_PROJECT_KEY = Read-Host 'Chiave del progetto locale esistente'
$secret = Read-Host 'Token SonarQube' -AsSecureString
$ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secret)
try { $env:SONAR_TOKEN = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) }
finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr); $secret.Dispose() }
try {
    powershell -NoProfile -ExecutionPolicy Bypass -File quality/analyze-backend.ps1 -Mode Analysis
}
finally { Remove-Item Env:SONAR_TOKEN }
```

Per una porta locale diversa aggiungere `-ServerUrl 'http://localhost:PORTA'`. Non inserire token in file, comandi letterali, screenshot o log condivisi. Lo scanner riceve il token come argomento di processo: trattare la sessione come sensibile. File scanner e report restano temporanei e non vanno versionati o condivisi indiscriminatamente.

Il flusso e' `begin -> build -> test con OpenCover -> end`, con base sorgenti `backend` e analisi multi-linguaggio disabilitata. Analysis rifiuta test filtrati e si ferma se build, test o report falliscono. In quel caso non invia risultati: correggere il problema e ripetere l'intero comando. Non lanciare `end` da solo su un'esecuzione incompleta. Dopo un invio riuscito controllare sul server l'elaborazione del report, l'importazione coverage e il Quality Gate: il successo dello scanner non equivale a un gate superato. Confrontare i risultati di esecuzioni complete con le stesse esclusioni.

## Risultati reali - 9 ottobre 2026

Esiti della **prova locale preliminare** sul commit `15ca2e2`, non un conteggio finale della consegna. La nuova revisione ha ricontrollato configurazione e disponibilita': server locale sulla porta 9000, scanner, token e PostgreSQL di test non disponibili; il comando Docker non risponde ed e' stato interrotto. Non e' stata ripetuta la suite mentre i colleghi verificano i loro file.

- Build Release riuscita; **11 test di dominio superati, 0 falliti, 0 ignorati** con OpenCover. Report controllato: solo assembly API, 29 sorgenti; migrazioni e bin/obj assenti, codice applicativo presente.
- Coverage **parziale**, non della suite completa: 39/822 sequence point (4,74%) e 19/283 branch point (6,71%). Sono metriche OpenCover della prova filtrata, non metriche Sonar o un giudizio sulla coverage completa.
- Test di integrazione non eseguiti: Docker Desktop fallisce nell'inizializzazione del socket `dockerInference`. Nessun reset o intervento sui dati di sviluppo.
- Il controllo preliminare Analysis senza token/chiave rifiuta l'esecuzione prima di build o invio. Server/progetto/scanner e importazione Sonar restano da verificare; nessun risultato, rating o Quality Gate attribuito a Sonar.
- Avviso NU1903 su `Microsoft.OpenApi` 2.0.0 gia' nella build; aggiornamento dipendenze fuori ambito.

## Report da completare

Dopo il collaudo dei colleghi, ripetere l'analisi completa sullo stesso checkout stabile. Annotare commit, data, versione server/scanner, comando, esito test e percorso del report; sul server controllare importazione coverage, problemi rilevanti e Quality Gate. Conservare report condivisibili senza token in `verifica-backend/`; richiamarli dal documento senza duplicarli inutilmente. La predisposizione non sostituisce il report backend richiesto dal paragrafo 8 della traccia.

Al momento non ci sono segnalazioni Sonar da classificare. L'avviso di dipendenza NU1903 e l'allineamento alla decisione del gruppo RF-17 sono punti tecnici aperti, non risultati Sonar. Quando disponibili, distinguere bug/sicurezza dalle preferenze stilistiche, motivare priorita' e falsi positivi; nessuna correzione automatica in questa task.

Riferimenti ufficiali: [scanner .NET e ordine dei passaggi](https://docs.sonarsource.com/sonarqube-server/analyzing-source-code/scanners/dotnet/using), [coverage C# / OpenCover](https://docs.sonarsource.com/sonarqube-server/analyzing-source-code/test-coverage/dotnet-test-coverage), [configurazione collector 6.0.4](https://github.com/coverlet-coverage/coverlet/blob/v6.0.4/Documentation/VSTestIntegration.md), [pacchetto scanner 11.3.0](https://www.nuget.org/packages/dotnet-sonarscanner/11.3.0).
