# Ambiente locale e verifiche reali - 19 ottobre 2026

## Riferimento e risultato

Commit analizzato: `6aa175a357dbdf1c35702686b985d46bff58c43e`, branch `main`.
Sorgenti backend, configurazione quality, Dockerfile e Compose senza modifiche
locali durante le prove. Sono aggiunti soltanto questo aggiornamento e le sue
evidenze in questa cartella. Nessun commit, branch, tag o push eseguito.
Il report del 16 ottobre resta una testimonianza storica: i blocchi Docker e
Sonar descritti allora sono stati superati con le prove qui registrate.

| Verifica | Esito reale |
| --- | --- |
| Suite completa con coverage, PostgreSQL Docker dedicato | 143 superati, 0 falliti, 0 ignorati |
| Prima analisi Sonar, suite completa | 142 superati, 1 errore di connessione; analisi NON pubblicata |
| Seconda analisi Sonar, suite completa | 143 superati, 0 falliti, 0 ignorati; analisi pubblicata |
| Compute Engine | SUCCESS, elaborazione completata |
| Importazione OpenCover | 29 file principali con coverage, verificata nel log e nelle misure server |
| Persistenza Docker dopo restart backend | Dati e byte immagine invariati |
| Persistenza Docker dopo ricreazione del solo backend | Dati e byte immagine invariati; container backend effettivamente diverso |
| Utente e volume immagini | uid 1654, non root; volume montato scrivibile nelle tre fasi |
| OpenAPI nell'immagine Docker | GET /openapi/v1.json: 200 nelle tre fasi |

I test sono sempre gli stessi 143: non sommare le esecuzioni per dichiarare
un numero maggiore di test. Nessun test o controllo e' stato modificato.

## Installazione e isolamento

Installati Docker Desktop 4.95.0 per l'utente corrente, motore Linux/WSL2,
Compose 5.6.0 e SonarScanner .NET 11.3.0. .NET SDK 10.0.401, Node e WSL erano
gia' disponibili. Installer Docker ufficiale verificato tramite SHA256 e firma
Authenticode valida Docker Inc prima dell'esecuzione.

SonarQube Community Build `26.10.0.133078-community` usa PostgreSQL
`16.15-alpine` separato, rete e volumi `bugboard26-quality-local_*` dedicati.
Porta Sonar pubblicata soltanto su `127.0.0.1:9000`; database Sonar non
pubblicato sull'host. Il requisito Elasticsearch `vm.max_map_count` e' stato
portato da 262144 a 524288 nel kernel Docker WSL, senza disabilitare i controlli
di bootstrap. Lo script di avvio lo ricontrolla dopo eventuali riavvii WSL.

Configurazione e script personali si trovano fuori dal repository:
`%LOCALAPPDATA%\BugBoard26\quality-local\`. Password iniziale Sonar cambiata;
password amministrativa, password database Sonar e token sono protetti tramite
Windows DPAPI e non inclusi nelle evidenze. Il progetto locale privato creato
e' `bugboard26-backend`. Non e' stato usato SonarCloud.

PostgreSQL dei test e' il servizio gia' previsto dal repository: progetto
Compose `bugboard26-tests`, PostgreSQL 16.15 su `127.0.0.1:55432`, database
iniziale `bugboard26_tests`. La factory crea ed elimina un database distinto
per ciascun caso. Dopo l'ultima suite: zero database temporanei dei casi.
Nessun test automatico ha usato il database di sviluppo.

Il Compose di sviluppo esistente e' stato avviato senza modificarlo, con
progetto predefinito `docker`, backend su `127.0.0.1:5256`, PostgreSQL su
`127.0.0.1:5432`. Prima dell'installazione non erano presenti container o volumi
Docker. Nessun volume e' stato eliminato o svuotato.

## Comandi, date e tentativo fallito

Tutti gli orari seguenti sono UTC; il 19 ottobre Europe/Rome e' UTC+2.

| Esecuzione | Inizio / fine UTC | Exit code |
| --- | --- | --- |
| Coverage completa | 09:59:24 / 10:00:38 | 0 |
| Primo tentativo Analysis | 10:01:52 / 10:02:54 | 1 |
| Secondo tentativo Analysis | 10:05:05 / 10:06:14 | 0 |
| Elaborazione server del secondo tentativo | 10:06:14 / 10:06:17 | SUCCESS |

Comandi effettivi, dalla radice del repository:

```powershell
docker compose -p bugboard26-tests -f backend/BugBoard26.Tests/docker-compose.tests.yml up -d --wait
powershell -NoProfile -ExecutionPolicy Bypass -File quality/analyze-backend.ps1 -Mode Coverage
docker compose -f docker/docker-compose.yml up -d --build
```

Per Analysis e' stato usato lo stesso script quality con `-Mode Analysis`,
configurando `SONAR_TOKEN` e `SONAR_PROJECT_KEY` soltanto nel processo tramite
il wrapper personale DPAPI. Nessun filtro, esclusione, profilo o gate aggiunto
o modificato per ottenere risultati migliori.

La prima analisi si e' fermata nel test
`ConcurrentReplacements_LeaveOnlyTheReferencedFile`: Npgsql ha ricevuto un
errore socket Windows durante la connessione a `127.0.0.1:55432`
(indirizzo/porta gia' in uso). Non e' dimostrata una causa applicativa; la suite
completa precedente e quella successiva sono passate senza modifiche. Il log
fallito e il suo TRX sono conservati. Non e' stato ripetuto soltanto il caso
fallito e non e' stata pubblicata la coverage della suite fallita. Se l'errore
ricompare, va indagato l'ambiente di rete locale: non dichiarare la suite immune
da problemi intermittenti sulla base di questa sola ripetizione.

Build Coverage: 0 avvisi e 0 errori. Build Analysis: 7 avvisi degli analizzatori
Sonar e 0 errori. La presenza di ulteriori code smell sul server comprende anche
segnalazioni degli analizzatori Roslyn esterni.

## SonarQube: misure e limiti del gate

Compute Engine task: `36a6d0be-3630-427a-8fd0-ee6130c86c78`.
Analysis ID: `53587a71-a106-4f53-acf0-39a31c071ff9`.
La cronologia server conferma la revisione Git `6aa175a...`.

| Metrica | Valore |
| --- | --- |
| Coverage complessiva Sonar | 92,8% |
| Line coverage Sonar | 94,9%, 42 linee non coperte su 824 |
| Branch coverage Sonar | 86,7%, 38 condizioni non coperte su 285 |
| Sequence point OpenCover | 782/824 = 94,90% |
| Branch point OpenCover | 247/285 = 86,66% |
| Bug / vulnerabilita' / code smell | 0 / 2 / 10 |
| Security hotspot / duplicazione | 0 / 0,0% |
| Rating affidabilita' / sicurezza / manutenibilita' | A / C / A |

La coverage complessiva Sonar combina linee e condizioni: non confonderla con
la sola line coverage o con i sequence point OpenCover.

**Quality Gate restituito dal server: OK, con conditions vuoto.** E' la prima
analisi del progetto locale, senza un periodo di nuovo codice valutabile. Il
gate standard Sonar way e' invariato e la sua definizione e' conservata. Questo
OK non attesta l'assenza di problemi sull'intero codice e NON chiude le due
segnalazioni di sicurezza esistenti. Non sono state risolte o accettate
artificialmente segnalazioni nell'interfaccia Sonar.

Valutazione delle segnalazioni, senza modificare codice in questa attivita':

- S2068, due vulnerabilita': credenziali dimostrative database e amministratore
  in appsettings.json. Sono effettivi valori di sviluppo pubblici, non segreti
  nuovi scoperti nel PC; restano rischiosi se usati per un'installazione esposta.
  Prima di un rilascio non locale occorre configurare segreti esterni, cambiare
  le credenziali e la chiave JWT e impedire fallback insicuri. Non basta cambiare
  la configurazione di un amministratore gia' creato per ruotarne la password.
- S6932, tre occorrenze: lettura esplicita del multipart. La scelta attuale
  consente controlli di accesso e risposte 400 uniformi per multipart malformati
  o eccessivi, tramite il filtro esistente. Non applicare un binding automatico
  soltanto per rimuovere l'avviso senza riverificare questi comportamenti.
- S6964: [Required] su OriginalIssueId non nullable non distingue campo omesso
  da zero. L'endpoint rifiuta comunque il riferimento inesistente senza scrivere;
  rimane un miglioramento del contratto/errore da concordare, non una modifica
  fatta durante l'installazione.
- S6966 e segnalazioni Roslyn: avvio asincrono, accesso tipizzato agli header,
  AttributeUsage e indicazioni sui test/entry point. Non e' emerso da esse un
  nuovo guasto funzionale; non e' stato eseguito refactoring cosmetico.

## Persistenza su container reali

Prova riuscita sulla issue ID 2, titolo `VERIFICA-BACKEND-20261019-100446`.
Le evidenze delle tre fasi sono rispettivamente delle 10:04:49, 10:04:56 e
10:05:02 UTC; contengono timestamp esatti, risposta issue, ID container,
ID immagine Docker, volume e SHA256 della PNG.

```powershell
docker compose -f docker/docker-compose.yml restart backend
docker compose -f docker/docker-compose.yml up -d --no-deps --force-recreate backend
```

Restart: stesso ID container backend. Ricreazione: ID backend diverso. In tutte
le fasi stesso ID container PostgreSQL, stesso volume immagini
`docker_bugboard26-issue-images`, stessi dati issue e stesso SHA256 immagine:
`4BA8D4FD5AB42544FEEFF22D50E84412502433CC95952FD1DF9A5293588DDBEA`.
Content-Type `image/png`, uid 1654, directory scrivibile, OpenAPI HTTP 200.
L'ID immagine Docker costruita e' registrato nei JSON, con checkout sorgenti
pulito al momento del build. Non e' un'immagine di release firmata o taggata.

Il primo script di prova aveva gia' creato la issue ID 1, riconoscibile dal
prefisso `VERIFICA-BACKEND-`, ma si era bloccato su una connessione IPv6 a
localhost:5256 (SynSent), mentre Compose pubblica su IPv4. E' stato interrotto
soltanto quel processo di verifica e ripetuta la prova con 127.0.0.1 e timeout.
Nessuna modifica al backend o a Compose. Entrambe le issue di prova e le immagini
restano nel database locale; nessun dato altrui e' stato cancellato.

Questa verifica dimostra la persistenza dopo restart e ricreazione del solo
backend, non dopo eliminazione volumi, reinstallazione Docker, guasto disco o
restore da backup. E' distinta dai test che riavviano l'API con WebApplicationFactory.

## Ripetere sul PC e accedere al server

Aprire Docker Desktop e attendere il motore Linux. In un terminale nuovo Docker
e' nel PATH; un terminale aperto prima dell'installazione puo' richiedere riapertura.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "$env:LOCALAPPDATA\BugBoard26\quality-local\start-sonar.ps1"
docker compose -p bugboard26-tests -f backend/BugBoard26.Tests/docker-compose.tests.yml up -d --wait
powershell -NoProfile -ExecutionPolicy Bypass -File "$env:LOCALAPPDATA\BugBoard26\quality-local\analyze.ps1" -Repository (Get-Location).Path
```

Attendere `http://127.0.0.1:9000/api/system/status` con status UP prima di Analysis.
Il wrapper personale non e' parte del repository: su un altro PC seguire i
comandi generali di comandi-ripetizione.md e configurare un proprio server/token.
I file DPAPI sono utilizzabili soltanto dall'account Windows che li ha creati.

Dashboard: `http://127.0.0.1:9000/dashboard?id=bugboard26-backend`.
Per mostrare le proprie credenziali soltanto nel terminale locale, senza
trascrizioni o condivisione dello schermo:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "$env:LOCALAPPDATA\BugBoard26\quality-local\access.ps1"
```

Il token personale usato per questa analisi scade il 17 aprile 2027; rigenerarlo
localmente prima di ulteriori analisi dopo la scadenza. Non inviarlo in chat.
Il PC dispone di 16 GB RAM: chiudere i container non necessari quando non si
eseguono verifiche. Per fermare Sonar senza eliminare volumi:

```powershell
docker stop bugboard26-quality-local-sonarqube-1 bugboard26-quality-local-database-1
```

## Evidenze e attivita' residue

Le evidenze di questa sessione sono in `evidenze/20261019/`, con manifest SHA256
separato; i file del 16 ottobre restano invariati. Sono conservati TRX e OpenCover
originali delle suite riuscite, TRX fallito, log, metadata esecuzioni, misure,
problemi, stato Compute Engine, definizione e stato gate, cronologia analisi e
le tre fasi della prova Docker. Nessuna cartella scanner completa o file
credenziali e' stato copiato. Il controllo dei segreti locali sulle evidenze
copiate non ha trovato le nuove password o il token.

Restano da coordinare con il gruppo: valutazione delle segnalazioni residue,
prova frontend/backend integrata sul checkout finale, prove di usabilita' reali,
coerenza diagrammi CASE, aggiornamento del documento condiviso/PDF e della
presentazione, pacchetto finale e consegna. Non sono stati eseguiti nuovi test
frontend, prove con utenti esterni, backup/restore o una seconda analisi dopo
correzioni. I report condivisi e la presentazione non sono stati modificati.
