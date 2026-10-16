# Verifica finale backend - 16 ottobre 2026

## Esito e ambito

**Suite backend finale: 143 superati, 0 falliti, 0 ignorati.**
Sono 11 casi di dominio e 132 di integrazione; il totale non somma le ripetizioni.
Build Release finale: 0 errori, 0 avvisi. Audit NuGet finale: nessun pacchetto
segnalato dalle fonti interrogate. Questo non equivale a un'analisi di sicurezza
completa o a un risultato SonarQube.

La verifica usa PostgreSQL reale dedicato ai test, non il database di sviluppo.
Persistenza Docker, analisi SonarQube, importazione coverage sul server e Quality
Gate **non eseguiti/verificati su questo PC**. Il gruppo ha indicato che queste
prove saranno eseguite dal collega che dispone dell'ambiente necessario.

Sono stati modificati solo tre file backend e questa cartella di report.
Frontend, Dockerfile, Compose, file quality e documenti condivisi restano
invariati. Nessun commit, branch o push eseguito.

## Commit e cronologia reale

All'inizio dell'attivita' il checkout era pulito sul commit
`15ca2e23065d7a878865a6085a82f58a5912a717`. Dopo la pull manuale dell'utente,
il riferimento e' `e13995140dff382dc3d548208c6c1e60be91fe5d`.
La pull ha aggiunto documentazione e predisposizione quality, senza cambiare
i sorgenti backend rispetto al commit precedente.

Le prove definitive usano la configurazione quality ricevuta con la pull.
Il risultato finale e' relativo a **e139951 + modifiche locali non committate**,
non al solo commit e139951. Il diff esatto e' in
[diff-backend.patch](evidenze/diff-backend.patch); gli hash dei sorgenti e dei
file quality sono in [sorgenti-analizzati.json](evidenze/sorgenti-analizzati.json).

Tutte le esecuzioni seguenti sono del 16 ottobre 2026. Orari UTC; per
Europe/Rome in questa data aggiungere due ore.

| Stato sorgenti | Inizio / fine UTC | Test passati | Coverage/configurazione |
| --- | --- | --- | --- |
| 15ca2e2, originale prima della pull | 08:50:15 / 08:51:51 | 143/143 | Collector CLI, Cobertura/OpenCover, senza il successivo file quality |
| 15ca2e2 + pin locale OpenAPI | 08:53:12 / 08:54:38 | 143/143 | Stessa configurazione CLI precedente |
| e139951, backend e quality senza modifiche | 09:09:55 / 09:11:33 | 143/143 | Script quality esistente, suite completa |
| e139951 + solo pin OpenAPI | 09:13:18 / 09:14:54 | 143/143 | Stesso script e stesse esclusioni |
| e139951 + pin OpenAPI + correzione READONLY | 09:20:09 / 09:21:58 | 143/143 | Stesso script e stesse esclusioni; risultato finale |

Non confrontare le percentuali delle esecuzioni ad hoc prima della pull con
quelle dello script quality: il perimetro e le esclusioni non erano gli stessi.
Per le prime due prove sono conservati comando, data, commit e output console
in `evidenze/prima-pull-*`. Per le ultime tre sono conservati anche TRX e XML
OpenCover originali. Nessun test e' stato aggiunto per aumentare il conteggio.

## Comando e isolamento database

Per le ultime tre esecuzioni e' stato usato, dalla radice:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File quality/analyze-backend.ps1 -Mode Coverage
```

Lo script esegue build Release non incrementale e la suite completa tramite
`dotnet test`, senza `-Filter`, con `quality/coverage.runsettings`.
Il codice di uscita e' 0 in tutte e tre le esecuzioni.
I comandi e gli istanti precisi sono nei rispettivi `esecuzione.json`.

Ambiente effettivo: .NET SDK 10.0.401, PostgreSQL 16.15 nativo su
`127.0.0.1:55432`, database iniziale `bugboard26_tests`, cluster creato in una
directory temporanea distinta. La factory crea e poi elimina un database
`bugboard26_test_<guid>` per ogni caso e una cartella immagini temporanea propria.
Non sono stati usati EF InMemory, SQLite o la connessione di sviluppo.

Al termine: 0 database temporanei dei casi e 0 cartelle immagini dei test
residui. Il server temporaneo e' stato arrestato; la verifica successiva
`pg_ctl status` restituisce 3, cioe' server non in esecuzione. La cache dei binari
e il cluster temporaneo arrestato restano fuori dal repository.
Vedere [database-test.json](evidenze/database-test.json),
[prerequisiti.json](evidenze/prerequisiti.json) e
[stato-postgresql.txt](evidenze/stato-postgresql.txt).

## Coverage locale verificata

| Stato | Sequence point | Branch point |
| --- | --- | --- |
| Commit e139951 originale | 780/822 = 94,89% | 245/283 = 86,57% |
| e139951 + solo pin OpenAPI | 780/822 = 94,89% | 245/283 = 86,57% |
| Finale con correzione READONLY | 782/824 = 94,90% | 247/285 = 86,66% |

Sono metriche Coverlet/OpenCover, **non metriche SonarQube**. Il lieve aumento
segue il nuovo controllo di ruolo, non nuove esclusioni o nuovi casi numerici.
Restano 42 sequence point e 38 branch point non visitati: coverage elevata non
significa verifica esaustiva. Fra le aree non completamente coperte ci sono
health check, errori I/O delle immagini e input di errore dei servizi di hashing.
Non sono qualificati come bug solo perche' non coperti.

Controllato un solo allegato originale per run, evitando il doppio conteggio
delle copie annidate del logger TRX. I report contengono un solo assembly,
`BugBoard26.Api`, e 29 file sorgente. Le esclusioni preesistenti riguardano
Migrations, bin/obj, `.g.cs`, `.generated.cs`, `.Designer.cs` e
`GeneratedCodeAttribute`; test e librerie esterne non fanno parte della misura.
Controller, servizi e Program restano nel perimetro. Nessuna esclusione aggiunta.

SHA-256 di `quality/coverage.runsettings`, identico nelle tre run:
`473A29EEF5502C98EC33943AE1A6E394C29B8EFB775353E6169026A389D06BAB`.
Riepilogo ricavato dagli XML: [riepilogo-test.json](evidenze/riepilogo-test.json).

## Problemi valutati e correzioni

### Dipendenza OpenAPI

L'audit iniziale rileva `Microsoft.OpenApi 2.0.0` transitivo con avviso alto
NU1903, [GHSA-v5pm-xwqc-g5wc / CVE-2026-49451](https://github.com/microsoft/OpenAPI.NET/security/advisories/GHSA-v5pm-xwqc-g5wc).
L'advisory riguarda la terminazione del parser per riferimenti circolari in
documenti OpenAPI non affidabili. Non e' stata dimostrata l'esposizione di un
percorso applicativo che accetti tali documenti: non si afferma un attacco riuscito.

Correzione minima in `backend/BugBoard26.Api/BugBoard26.Api.csproj`: riferimento
esplicito a `Microsoft.OpenApi 2.7.5`, versione corretta della stessa linea 2.x.
Nessun aggiornamento generalizzato delle altre dipendenze. Build e suite finali
passano, e l'audit NuGet non segnala pacchetti vulnerabili. Audit prima/dopo
conservati nelle rispettive directory delle evidenze. L'audit non sostituisce
SAST, verifica del sistema operativo o analisi delle librerie native.

La generazione di `/openapi/v1.json` in Development non ha uno smoke HTTP
dedicato completato in questa verifica: e' un controllo da includere nella
prova Docker del collega dopo la modifica della dipendenza.

### READONLY sulle proprie notifiche: decisione ricevuta e applicata

RF-17, nella specifica condivisa, richiede consultazione senza modificare dati.
Prima della correzione il PATCH read consentiva a READONLY di aggiornare IsRead
sulle proprie notifiche, comportamento anche previsto dal test precedente.
Un test passato descriveva quindi il codice, non risolveva il requisito.

Il gruppo ha confermato esplicitamente in questa sessione: READONLY non deve
effettuare scritture, nemmeno sulle proprie notifiche. La decisione e' stata
richiesta prima di cambiare permessi; non viene introdotta un'eccezione a RF-17.

In `NotificationsController.MarkAsRead` il controllo sul ruolo segue quello
sull'utente attivo e precede ricerca e modifica della notifica:

- READONLY autenticato: PATCH read restituisce 403, senza scritture.
- Consultazione delle proprie notifiche: resta consentita con GET 200.
- ADMIN e USER: lettura delle proprie notifiche invariata; notifiche altrui
  sempre protette dal controllo di proprieta'.

In `NotificationsIntegrationTests` il caso READONLY della theory positiva e'
stato sostituito da un caso negativo dedicato. Verifica 403, confronto completo
del database, GET successivo 200, proprieta' delle notifiche e IsRead ancora false.
I casi positivi ADMIN/USER e quelli di rifiuto delle notifiche altrui passano.
Il totale resta 143. Nessuna modifica a frontend o documenti condivisi.

## SonarQube e Quality Gate: non disponibili

Verificati la predisposizione ricevuta e i prerequisiti locali:

- Server locale `http://localhost:9000/api/system/status` non raggiungibile.
- Scanner assente sia nel percorso previsto dallo script sia nel PATH.
- Project key e token non configurati; esistenza/accessibilita' del progetto
  non verificabili. Non si afferma che il progetto non esista sul PC del collega.
- `-Mode Analysis` eseguito come preflight: uscita 1 per prerequisiti mancanti,
  prima di scanner, build o invio dell'analisi.

Vedere [sonar-preflight.txt](evidenze/sonar-preflight.txt). **Nessuna analisi
SonarQube eseguita**, nessuna importazione della coverage dimostrata, nessuna
segnalazione/rating Sonar disponibile. Quality Gate: **NON DISPONIBILE**, non
"superato" e neppure un gate fallito. Gli XML locali non provano l'importazione.

Il collega deve usare il progetto locale esistente, generare coverage della
suite completa nella stessa run Analysis, controllare il task del server,
l'attribuzione della coverage ai sorgenti e il gate dell'analysis ID corrente.
I passaggi sono in [comandi-ripetizione.md](comandi-ripetizione.md).

## Persistenza Docker: non verificata qui

Docker CLI assente anche nel percorso standard. Non sono stati avviati container,
creati dati di prova Docker, interrogati o eliminati volumi/dati esistenti.
I due volumi nel Compose e i permessi previsti dal Dockerfile sono stati letti,
non validati durante l'esecuzione di un container.

Il test esistente che legge un'immagine da una nuova WebApplicationFactory con
stesso database e directory passa nella suite, ma **non dimostra** persistenza
di un volume Docker, riavvio di un container o sua ricreazione.

Il collega deve creare solo una issue e un'immagine di prova riconoscibili,
confrontare dati e SHA-256 dell'immagine prima/dopo restart e dopo ricreazione
del solo backend, conservare identita' del container PostgreSQL e dei mount,
e verificare la scrittura da utente non root. Nessuna eliminazione di volumi.
La procedura non eseguita e' in [comandi-ripetizione.md](comandi-ripetizione.md).

## Consegna e passaggio al collega

Le attivita' ancora necessarie sono le evidenze reali Docker, l'analisi Sonar
con importazione coverage e Quality Gate, e il controllo OpenAPI in Development.
Non risultano test backend falliti. L'ambiguita' READONLY e' risolta per il
backend secondo la decisione del gruppo; resta l'allineamento dei materiali
condivisi e dell'interfaccia da parte dei rispettivi colleghi.

Il collega della documentazione deve usare i dati finali e non presentare
le percentuali preliminari del 9 ottobre come coverage della suite completa.
I file LaTeX, README condiviso, evidenze.md, consegna.md e presentazione non
sono stati modificati da questa attivita'. Per l'elenco pronto da trasferire:
[passaggio-collega.md](passaggio-collega.md).

Non attribuire questi esiti a un futuro commit ancora sconosciuto: il codice
finale e' il working tree sopra identificato. Dopo il commit manuale, registrare
separatamente il nuovo SHA e ripetere le prove se cambiano i sorgenti.

## Inventario delle evidenze

- `evidenze/commit-e139951/`: console, metadati, TRX, OpenCover, audit originali.
- `evidenze/working-tree-e139951/`: checkpoint con il solo pin di sicurezza.
- `evidenze/working-tree-final-e139951/`: risultati finali con READONLY corretto.
- `evidenze/prima-pull-*`: metadati e console delle prove precedenti alla pull.
- `evidenze/riepilogo-test.json`: contatori e coverage estratti, non stime.
- `evidenze/sorgenti-analizzati.json` e `diff-backend.patch`: stato locale.
- `evidenze/prerequisiti.json`, `database-test.json`, `stato-postgresql.txt`:
  ambiente e limiti osservati.
- `evidenze/sha256.csv`: impronte dei file di report, escluso il manifest stesso.

I TRX e gli XML sono conservati senza riscrivere percorsi o contenuti. Contengono
percorsi locali originali, quindi per importare coverage su un altro checkout
occorre rigenerarla, non riusare questi XML come se appartenessero alla nuova run.
Le copie archiviate sono state controllate per pattern di token; nessun valore
SONAR_TOKEN e' stato configurato, scritto nel repository o incluso nei report.
