# Finalizzazione tecnica - 19 ottobre 2026

## Stato verificato

Riferimento: commit `6aa175a357dbdf1c35702686b985d46bff58c43e` **con modifiche
backend locali non committate**. Non attribuire i risultati al solo commit.
Impronte dei sorgenti e metadata sono in `evidenze/finalizzazione-20261019/`.
I precedenti report restano storici e non sono stati sovrascritti.

| Verifica finale | Risultato |
| --- | --- |
| Backend, suite completa con PostgreSQL e coverage | 158 superati, 0 falliti, 0 ignorati |
| Composizione backend | 11 casi dominio invariati, 147 integrazione |
| Build backend durante Analysis | 0 errori, 3 avvisi Sonar S6932 |
| Frontend build produzione | Riuscita, bundle iniziale 380,10 kB |
| Frontend test ChromeHeadless senza watch | 33 superati, exit code 0 |
| Collaudo browser/API reali | 16 checkpoint superati, nessun errore JavaScript non gestito |
| Sonar Compute Engine | SUCCESS |
| Quality Gate | OK, condizioni effettivamente valutate |
| Bug / vulnerabilita' / code smell Sonar | 0 / 0 / 4 |
| Rating affidabilita' / sicurezza / manutenibilita' | A / A / A |
| Coverage Sonar complessiva / linee / rami | 93,6% / 95,5% / 87,9% |
| Coverage del nuovo codice nel gate | 100%, zero nuove violazioni e duplicazione 0% |
| Backend Docker di sviluppo dopo rebuild/ripristino | Login e OpenAPI 200; dati e immagine della precedente prova invariati |

Non sommare checkpoint E2E, casi xUnit e test Jasmine come se fossero una sola
suite. Non sono state aggiunte esclusioni, modificati gate o profili, oppure
accettate artificialmente segnalazioni sul server.

## Correzioni e motivazioni

### Configurazione e primo amministratore

Rimosse credenziali database, password iniziale e chiave JWT dai due file
appsettings del backend. Il seeder non contiene piu' credenziali di ripiego:
per inizializzare un database senza ADMIN richiede DefaultAdmin:Email e
DefaultAdmin:Password configurati. Se un ADMIN esiste gia', non lo ricrea,
non gli cambia password e non richiede di nuovo le credenziali di bootstrap.

L'avvio rifiuta esplicitamente connessione assente, chiave JWT assente o
inferiore a 32 byte UTF-8, prima delle migrazioni e dell'accettazione di
richieste. Il controllo usa la configurazione finale dopo Build, cosi' anche
gli override della factory sono applicati prima della validazione.

**La demo Docker continua a partire come prima.** Il Compose esistente passa
gia' i parametri necessari e mantiene l'ADMIN iniziale richiesto dalla traccia.
Il Compose non e' stato modificato, ne' sono state ruotate credenziali dei
database esistenti. I suoi valori predefiniti pubblici sono dimostrativi:
non costituiscono una configurazione sicura per un servizio esposto su Internet.
Anche la chiave JWT deve essere privata in un eventuale deployment reale.

Per dotnet run non esiste piu' una configurazione sensibile incorporata:
impostare variabili d'ambiente ASP.NET Core o user-secrets. Aggiunto soltanto
UserSecretsId al progetto API, senza nuovi pacchetti. User-secrets e' il
meccanismo locale di sviluppo di .NET, non un vault cifrato di produzione.

### Contratto duplicati

OriginalIssueId diventa nullable con Required e Range(1, int.MaxValue), come
gli altri campi obbligatori nullable nei contratti del progetto. Prima un
campo omesso diventava zero ed era rifiutato come riferimento inesistente;
ora omissione, null, zero e valori negativi producono un 400 di validazione.
Le issue e le notifiche restano invariate, verificato via snapshot PostgreSQL.
Non e' stata aggiunta una regola applicativa o una funzionalita' di duplicazione.

### Modifiche puntuali di qualita'

Avvio con RunAsync, header tipizzato XContentTypeOptions e AttributeUsage sul
filtro multipart. Nella fixture: SuppressFinalize nel dispose e helper di
deserializzazione statico. Nessun refactoring architetturale.

La factory genera chiave JWT e password bootstrap proprie, senza dipendere
dai segreti del PC o dal database di sviluppo. Restano stabili per la durata
del singolo caso e per i suoi host secondari, ma sono distinte fra casi.
Gli override espliciti servono a verificare configurazioni non valide.

## Test aggiunti e verifiche negative

Quindici nuovi casi, motivati dalle correzioni, non dal raggiungimento di un
numero: nove configurazioni di avvio mancanti/non valide, login del bootstrap
ADMIN con JWT utilizzabile e risposta utenti senza password, conservazione
dell'ADMIN esistente quando mancano le credenziali iniziali, quattro input
duplicato non validi con snapshot dati invariato.

File principali modificati:

- API: Program.cs, Data/DatabaseSeeder.cs, appsettings.json,
  appsettings.Development.json e UserSecretsId nel csproj.
- Contratto DuplicateIssueRequest.cs e header in IssuesController.cs.
- ReadImageFormInActionAttribute.cs.
- IssueApiFactory.cs, ApiIntegrationTest.cs,
  IssueDuplicateValidationIntegrationTests.cs e nuovo
  StartupConfigurationIntegrationTests.cs.

Frontend, Dockerfile, Compose, dipendenze e file quality non modificati.
I test di dominio e le aspettative dei test preesistenti sono mantenuti.

## Sonar: risultato reale e residui

Server locale Community Build 26.10.0.133078, scanner .NET 11.3.0, progetto
privato bugboard26-backend. Task `2cebf7d3-35dd-4181-9317-710f2247b8b4`, analisi
`76df9411-d667-4071-b5ca-8cdcc27a02eb`. Analisi inviata alle 10:32:52 UTC.
Il server ha importato la coverage di 29 file principali. Le percentuali
Sonar complessive non vanno confuse con i sequence point OpenCover.

Rispetto all'analisi iniziale: vulnerabilita' da 2 a 0, code smell da 10 a 4.
Questa volta il gate non ha conditions vuoto: valuta nuovo codice rispetto
alla prima analisi delle 12:06:04 Europe/Rome e tutte le condizioni presenti
sono OK. Il 100% riguarda il nuovo codice, non l'intero backend.

Rimangono tre S6932 sulla lettura esplicita del multipart e un ASP0027 sulla
classe Program pubblica parziale usata dai test. La lettura manuale e' una
scelta per controllare permessi e restituire 400 uniformi sui multipart non
validi; cambiarla richiede verificare nuovamente questi comportamenti. La
classe parziale e' ridondante nel nuovo SDK, non un malfunzionamento.
Le segnalazioni sono lasciate aperte e visibili, senza soppressioni.

Sonar registra HEAD come revisione, ma analizza i file locali modificati:
sono conservati stato Git e SHA256. Gli avvisi di blame mancante su file
modificati/non tracciati sono attesi prima del commit manuale e limitano
l'attribuzione SCM; non sono stati nascosti committando automaticamente.

## Collaudo integrato e isolamento

Chrome headless, Angular reale su 127.0.0.1:4200 e immagine backend ricostruita.
Durante l'E2E l'API occupava temporaneamente 5256 ma usava soltanto il database
`bugboard26_test_e2e_3470f6ccde8d` nell'istanza PostgreSQL dedicata ai test,
con un volume immagini dedicato. Nessun mock HTTP e nessuna scrittura E2E
nel database di sviluppo.

Verificati: redirect anonimo, login ADMIN, creazione USER e READONLY dalla UI,
form issue con suggerimento e PNG, dettaglio con immagine scaricata, USER
assegnatario che risolve, data/notifica e ripetizione idempotente, rifiuto USER
non assegnatario senza scritture, lettura notifiche proprie da USER e ADMIN,
READONLY senza comandi di modifica, GET delle proprie notifiche e PATCH read
403 senza variazioni, ricerca/filtro/export reali, duplicazione e archiviazione
ADMIN con liste e riferimenti coerenti. Controllata lista mobile 390x844 senza
overflow orizzontale; screenshot desktop/mobile ispezionati.

Per la notifica propria READONLY e' stata inserita una fixture direttamente
nel solo database E2E: non e' una funzionalita' applicativa o un risultato di
risoluzione simulato. La UI non invia PATCH automatici durante la consultazione.

Il container E2E e' stato arrestato e il backend di sviluppo ripristinato
dall'utente dopo il blocco del comando automatico di ripristino. Verificati
nuovamente health, login, OpenAPI, ID/titolo/UpdatedAt della precedente issue 2
e SHA256 dell'immagine gia' presente. Corrispondono alle evidenze iniziali.
I dati E2E restano solo nell'istanza test e nel volume dedicato; il container
E2E e' fermo. Nessun volume o dato preesistente eliminato. Il PostgreSQL test
usa tmpfs: questi dati E2E non sono un archivio persistente di consegna.

La prova completa restart/ricreazione e' quella del report precedente; dopo
questo rebuild e' verificata la conservazione dei dati originali, non si
dichiara una seconda prova restart/ricreazione completa mai eseguita.

## Tentativi intermedi e limiti

La prima versione dei controlli di avvio precedeva l'applicazione degli
override della factory: 26/28 test mirati falliti. Spostata la validazione
sulla configurazione finale: 28/28 superati. Una prima suite completa aveva
156/158 superati: due host secondari ricevevano una chiave JWT rigenerata e
restituivano 401. Corretto il ciclo di vita della chiave nella fixture, senza
cambiare le aspettative: suite finale 158/158.

Nello script browser temporaneo sono stati corretti due errori del collaudo:
il pulsante login si chiama Entra e l'archiviazione naviga all'archivio, non
lascia il dettaglio disabilitato. I tentativi parziali sono conservati e non
attribuiti a bug applicativi. La ripetizione completa ha 16 checkpoint PASS.

Il primo comando npm test non inoltrava i flag al processo Angular in questa
sessione PowerShell ed e' rimasto in watch dopo 33 SUCCESS. Arrestato soltanto
quel runner e ripetuto tramite node/ng.js: 33 SUCCESS, exit code 0, senza watch.

Questi controlli non sono prove con persone reali, test di carico, certificazione
WCAG, analisi di sicurezza completa o collaudo su tutti i browser. I risultati
sono riferiti al checkout locale indicato, non a una release gia' pubblicata.

## Ripetere e avviare

Con Docker Desktop pronto, dalla radice:

```powershell
docker compose -p bugboard26-tests -f backend/BugBoard26.Tests/docker-compose.tests.yml up -d --wait
dotnet test backend/BugBoard26.slnx --configuration Release
powershell -NoProfile -ExecutionPolicy Bypass -File quality/analyze-backend.ps1 -Mode Coverage
docker compose -f docker/docker-compose.yml up -d --build
npm --prefix frontend run build
```

Dalla cartella frontend, per i test con uscita effettiva:

```powershell
$env:CHROME_BIN = 'C:\Program Files\Google\Chrome\Application\chrome.exe'
node node_modules/@angular/cli/bin/ng.js test --watch=false --browsers=ChromeHeadless
```

Per Analysis su questo PC e' disponibile il wrapper locale DPAPI descritto
nel report precedente. Sugli altri PC configurare server, progetto e token
secondo la procedura quality esistente; non inviare token in chat o salvarli
nel repository. Attendere sempre il task server e verificare il gate.

Per avvio nativo con dotnet run impostare prima ConnectionStrings:DefaultConnection
e Jwt:Key; per un database nuovo anche DefaultAdmin:Email e DefaultAdmin:Password.
I nomi di ambiente corrispondenti usano doppio underscore. In alternativa:

```powershell
$project = 'backend/BugBoard26.Api/BugBoard26.Api.csproj'
$credential = Get-Credential -Message 'Credenziali amministratore iniziale per il database locale'
$connection = Read-Host 'Connessione PostgreSQL locale' -AsSecureString
$ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($connection)
try {
    $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
    $bytes = New-Object byte[] 48
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    @{
        'ConnectionStrings:DefaultConnection' = $plain
        'Jwt:Key' = [Convert]::ToBase64String($bytes)
        'DefaultAdmin:Email' = $credential.UserName
        'DefaultAdmin:Password' = $credential.GetNetworkCredential().Password
    } | ConvertTo-Json | dotnet user-secrets set --project $project
    if ($LASTEXITCODE -ne 0) { throw 'Configurazione user-secrets non riuscita.' }
}
finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
    $connection.Dispose()
    $plain = $null
    $credential = $null
}
```

Usare questa inizializzazione solo se non esiste gia' una configurazione locale
da preservare. Non eseguire durante transcript o condivisione del terminale.
Una nuova chiave JWT invalida i vecchi token: eseguire nuovamente il login.
Cambiare configurazione non ruota le password di utenti o PostgreSQL esistenti.
Il README condiviso deve essere allineato a questi prerequisiti nella successiva
revisione documentale; in questa fase non e' stato modificato.

## Passaggio alla revisione finale

Funzionalita' assegnate: 1, 2, 3, 6, 8, 11, 13, 14, 15, 16; non implementare
commenti, dashboard, etichette, cronologia, scadenze o altri requisiti non assegnati.
Le evidenze tecniche sono ora disponibili per il documento finale. Restano:

- Usabilita' con persone: i CSV esistenti hanno solo intestazioni. Review assistita
  in questa cartella, ma nessuna sessione o risposta inventata. Vedere la scheda
  operativa allegata per raccogliere i risultati reali.
- Conformita' CASE: .puml/export presenti, .vpd non dichiarati sincronizzati;
  verificare la modellazione nel tool o chiarire l'accettazione di PlantUML,
  senza attribuire ai docenti una decisione non ricevuta.
- Revisione completa dei documenti condivisi: percorsi report, conteggi, esiti
  test plan, configurazione nativa, decisione READONLY, numeri Sonar, PDF e slide.
- Prova cronometrata della presentazione, revisione del pacchetto, release e
  consegna Teams da parte di ciascun componente. Nessuna di queste azioni Git
  o di invio e' stata eseguita automaticamente.
