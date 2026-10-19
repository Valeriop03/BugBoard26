# BugBoard26

BugBoard26 e' una web app per la gestione collaborativa di issue, sviluppata per il progetto di Ingegneria del Software 2025/2026.

Questo repository e' organizzato con frontend e backend separati:

- `backend`: ASP.NET Core Web API e test.
- `frontend`: Angular.
- `docker`: Compose per PostgreSQL e backend.
- `Documentazione`: requisiti, design, diagrammi e documentazione finale.

## Requisiti assegnati

Il gruppo implementa solo le funzionalita' assegnate: autenticazione con ruoli, creazione e consultazione issue con immagine opzionale, filtri e ricerca, cambio stato per assegnatario/admin, notifiche di risoluzione, export CSV, archiviazione, suggerimento assegnatario, modalita' readonly e gestione duplicati. Stato del codice e verifiche ancora aperte: [Documentazione finale](Documentazione/Documentazione%20finale/readme.md).

## Prerequisiti

- Docker Desktop avviato con container Linux e Docker Compose v2.
- Node.js 22 e npm per il frontend Angular.
- .NET SDK 10 per i test o per avviare il backend con `dotnet run`. Non serve sul PC se il backend viene eseguito soltanto in Docker.

Eseguire i comandi seguenti dalla radice del repository, salvo dove indicato.

## Avvio di database e backend con Docker

```powershell
docker info
docker compose -f docker/docker-compose.yml config
docker compose -f docker/docker-compose.yml up -d --build
docker compose -f docker/docker-compose.yml ps
docker compose -f docker/docker-compose.yml logs --tail=50 backend
```

Compose aspetta che PostgreSQL risponda al controllo `pg_isready` prima di avviare il backend. L'API applica le migrazioni e inizializza l'admin prima di accettare richieste.

Se `docker info` non riesce a contattare il motore, aprire Docker Desktop e attendere che il motore Linux sia avviato prima di continuare. Non usare il reset di fabbrica per risolvere un problema di avvio senza aver prima salvato i dati.

Il backend e' disponibile su `http://localhost:5256/api`, come previsto da `frontend/src/app/api.config.ts`. Per controllarne l'avvio:

```powershell
Invoke-RestMethod http://localhost:5256/api/health
```

`up -d` non garantisce che l'API abbia gia' terminato le migrazioni. Attendere nei log il messaggio `Now listening on` e verificare che `/api/health` risponda prima del login.

`/api/health` conferma che l'API risponde, ma non interroga PostgreSQL: una lettura autenticata delle issue verifica anche l'accesso ai dati.

Il Dockerfile usa l'SDK .NET 10 per restore e publish, poi copia il risultato in un'immagine con il solo runtime ASP.NET Core. Il processo gira con l'utente non root dell'immagine. Nel container l'API ascolta sulla porta `8080`, pubblicata solo sul PC locale come `5256`.

Il setup locale usa HTTP. Il middleware HTTPS gia' presente puo' registrare un avviso sulla porta HTTPS non configurata: questo Compose non fornisce un certificato HTTPS.

## Frontend

In un altro terminale:

```powershell
cd frontend
npm ci
npm start
```

Aprire `http://localhost:4200/login`, accedere e consultare la lista issue. Il frontend rimane fuori da Docker e chiama l'API sulla porta `5256`. I terminali con `npm start` o `dotnet run` devono restare aperti durante l'utilizzo; si fermano con `Ctrl+C`.

Prova manuale del collegamento frontend/backend:

1. Accedere con l'account locale indicato in "Migrazioni e account iniziale", oppure con un account gia' presente nel database.
2. Verificare negli strumenti di sviluppo del browser, scheda Rete, che `POST /api/auth/login` risponda `200`.
3. Nella lista issue verificare che `GET /api/issues` risponda `200` con l'header `Authorization: Bearer ...`. Su un database nuovo la lista vuota e' un risultato valido.

## Configurazione locale

Compose passa la configurazione al backend tramite variabili d'ambiente ASP.NET Core, ad esempio `ConnectionStrings__DefaultConnection` e `Jwt__Key`. I valori seguenti possono essere impostati nella sessione PowerShell prima dell'avvio:

| Variabile per Compose | Valore locale predefinito |
| --- | --- |
| `POSTGRES_DB` | `bugboard26` |
| `POSTGRES_USER` | `bugboard26` |
| `POSTGRES_PASSWORD` | `bugboard26` |
| `POSTGRES_PORT` | `5432` (porta sul PC) |
| `JWT_KEY` | `bugboard26-development-secret-key-change-before-release` |
| `JWT_ISSUER` | `BugBoard26` |
| `JWT_AUDIENCE` | `BugBoard26Client` |
| `JWT_EXPIRES_MINUTES` | `120` |
| `DEFAULT_ADMIN_EMAIL` | `admin@bugboard26.local` |
| `DEFAULT_ADMIN_PASSWORD` | `Admin123!` |

Dal container il database ha host `postgres` e porta `5432`; dal PC ha host `localhost` e la porta indicata da `POSTGRES_PORT`. `localhost` dentro il container backend indicherebbe il backend stesso.

Se la porta database sul PC e' gia' occupata, scegliere una porta libera, per esempio:

```powershell
$env:POSTGRES_PORT = '5433'
docker compose -f docker/docker-compose.yml up -d --build
```

Il backend Docker continua a usare `postgres:5432`. Se invece si usa `dotnet run`, la stringa di connessione deve indicare la porta scelta sul PC. Le variabili con nomi come `POSTGRES_PASSWORD` sono lette da Compose; per `dotnet run` usare i nomi ASP.NET Core, come `ConnectionStrings__DefaultConnection`.

Le credenziali e la chiave JWT predefinite sono per lo sviluppo locale. Cambiare `POSTGRES_DB`, utente o password nelle variabili non riconfigura un database gia' inizializzato nel volume.

## Migrazioni e account iniziale

`Program.cs` chiama `DatabaseSeeder.SeedAdminAsync`. Il seeder esegue `Database.MigrateAsync()` e applica le migrazioni presenti in `backend/BugBoard26.Api/Migrations` che non risultano gia' applicate. Poi crea l'admin solo se non esiste gia' un utente con ruolo ADMIN.

Non occorre lanciare `dotnet ef database update` prima dell'avvio: le migrazioni vengono applicate sia in Docker sia con `dotnet run`. Dopo aver ricevuto nuove migrazioni o modifiche backend, ricostruire e avviare il servizio:

```powershell
docker compose -f docker/docker-compose.yml up -d --build backend
```

L'immagine runtime non contiene l'SDK o `dotnet-ef`; la creazione di nuove migrazioni si svolge sul PC di sviluppo con l'SDK. Il repository contiene il manifest dei tool in `dotnet-tools.json`.

Su un database senza admin, il login iniziale e':

- Email: `admin@bugboard26.local`
- Password: `Admin123!`

Se il volume contiene gia' un admin, usare le credenziali di quell'account. Modificare `DEFAULT_ADMIN_EMAIL` o `DEFAULT_ADMIN_PASSWORD` non cambia gli utenti gia' salvati.

## Backend con dotnet run

Per usare il backend sul PC mantenendo PostgreSQL in Docker:

```powershell
docker compose -f docker/docker-compose.yml stop backend
docker compose -f docker/docker-compose.yml up -d --wait postgres
dotnet run --project backend/BugBoard26.Api/BugBoard26.Api.csproj --launch-profile http
```

Prima di `dotnet run` configurare `ConnectionStrings:DefaultConnection` e una
`Jwt:Key` privata di almeno 32 byte tramite variabili d'ambiente o .NET
user-secrets. Per un database senza ADMIN servono anche `DefaultAdmin:Email`
e `DefaultAdmin:Password`. I file appsettings non contengono piu' segreti o
credenziali di ripiego. Compose passa gia' tutti questi valori: il suo avvio
dimostrativo non richiede questa configurazione nativa aggiuntiva.

Procedura con input mascherato:
[Configurazione nativa](Documentazione/Documentazione%20finale/verifica-backend/finalizzazione-20261019.md#ripetere-e-avviare).
User-secrets e' per lo sviluppo, non un vault cifrato di produzione. Una nuova
chiave JWT richiede un nuovo login; cambiare configurazione non modifica
password di utenti o database esistenti.

Il profilo `http` ascolta su `http://localhost:5256`. Non avviare contemporaneamente il backend Docker e quello sul PC sulla stessa porta.

Con questo avvio le immagini sono in `backend/BugBoard26.Api/uploads/issue-images`; la variabile `IssueImages__Directory` permette di scegliere un'altra directory scrivibile, non pubblica. Docker usa invece il volume `bugboard26-issue-images` in `/var/lib/bugboard26/issue-images`. Cambiare modalita' di avvio riusa il database, ma **non trasferisce i file immagine**: il riferimento puo' esistere mentre GET image risponde 404 nella directory diversa.

Con database, porta o credenziali diversi dai valori predefiniti, impostare la connessione prima di `dotnet run`, per esempio se e' stata scelta la porta `5433`:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Host=localhost;Port=5433;Database=bugboard26;Username=bugboard26;Password=bugboard26'
```

## Test

I test backend comprendono test di dominio e di integrazione. Quelli di integrazione richiedono un PostgreSQL dedicato sulla porta `55432` e avviano l'API in memoria; non serve avviare separatamente il backend:

```powershell
docker compose -p bugboard26-tests -f backend/BugBoard26.Tests/docker-compose.tests.yml up -d --wait
dotnet test backend/BugBoard26.slnx
docker compose -p bugboard26-tests -f backend/BugBoard26.Tests/docker-compose.tests.yml down
```

Il database di test e' distinto da quello di sviluppo. Per isolamento, connessioni personalizzate e filtri dei test vedere [backend/BugBoard26.Tests/README.md](backend/BugBoard26.Tests/README.md).

I test di stato e notifiche sono in `Integration/IssueStatusIntegrationTests.cs` e `Integration/NotificationsIntegrationTests.cs`: verificano permessi, risoluzione, date e notifica persistite, richiesta ripetuta dello stesso stato, isolamento per destinatario e lettura delle proprie notifiche. Non sostituiscono tutte le prove manuali; copertura e limiti sono in [Documentazione finale](Documentazione/Documentazione%20finale/readme.md).

Se PostgreSQL non e' disponibile, si possono eseguire solo i test di dominio; questo non sostituisce la verifica dei test di integrazione:

```powershell
dotnet test backend/BugBoard26.slnx --filter 'Category!=Integration'
```

Per verificare la compilazione frontend dalla radice:

```powershell
npm --prefix frontend run build
```

## Arresto e dati persistenti

```powershell
docker compose -f docker/docker-compose.yml down
```

I volumi nominati `bugboard26-postgres-data` e `bugboard26-issue-images` conservano rispettivamente dati relazionali e immagini. L'arresto senza `-v` conserva entrambi; non usare `down -v` o eliminare i volumi. Usare lo stesso file Compose e nome di progetto per riutilizzarli. Il Dockerfile prepara la directory immagini con permessi per l'utente non root; il collaudo con ricreazione del container resta distinto da questa verifica dei file.

Per un riavvio ordinato, senza rimuovere container o dati:

```powershell
docker compose -f docker/docker-compose.yml stop backend postgres
docker compose -f docker/docker-compose.yml up -d --wait
```

Dopo che `/api/health` risponde, accedere di nuovo e confrontare lista, archivio e dettaglio delle issue con quelli prima dell'arresto.

## Qualita' backend

E' predisposta l'analisi con **SonarQube Server locale**, senza SonarCloud o workflow pubblicati. Coverlet, gia' nel progetto test, genera OpenCover. Per la coverage completa, con il PostgreSQL di test disponibile:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File quality/analyze-backend.ps1 -Mode Coverage
```

`Bypass` vale solo per quel processo, non cambia la policy globale; usare soltanto dopo aver controllato lo script. Per server, scanner, token nella sessione e modalita' Analysis vedere [qualita_backend.md](Documentazione/Documentazione%20finale/qualita_backend.md). Il comando non crea account o progetti. I report sono in una nuova cartella temporanea per ogni esecuzione, non nel repository. Migrazioni e file generati sono esclusi dalla coverage, non il codice applicativo.

Verifica del 19 ottobre 2026 su `6aa175a` con correzioni locali non committate:
**166 test backend superati**, PostgreSQL dedicato, analisi SonarQube realmente
elaborata e coverage importata. Coverage Sonar 93,7% complessiva, 95,7% linee,
88,2% rami; gate OK, 0 bug e 0 vulnerabilita' segnalati, 4 code smell documentati.
Risultati e limiti nel [report di audit](Documentazione/Documentazione%20finale/verifica-backend/audit-20261019.md).
I report del 9 e 16 ottobre sono storici, non l'esito corrente.

## Limiti attuali

Il PATCH status ora rifiuta `Duplicate` (400) e le issue gia' duplicate (409); il percorso dedicato azzera `ResolvedAt`. Gli assegnatari indicati devono esistere, essere attivi e non READONLY. Le annotazioni precedenti sul merge di queste correzioni non sono piu' attuali.

- Persistenza dati/immagini dopo restart e ricreazione backend verificata il 19 ottobre, senza eliminare volumi; evidenze nella cartella di verifica backend.
- RF-17 deciso dal gruppo e verificato: READONLY consulta le proprie notifiche ma PATCH read restituisce 403 senza scritture; la UI non mostra il comando.
- `Microsoft.OpenApi` e' fissato a 2.7.5; il precedente avviso su 2.0.0 e' storico.
- I valori predefiniti di Compose sono per la demo locale, non per un servizio pubblico. Nessuna certificazione completa di sicurezza o prestazioni e' dichiarata.
- Restano sessioni di usabilita' con persone reali, conformita' CASE e prova cronometrata della presentazione.

Requisiti, design, sorgenti dei diagrammi e verifiche sono in [Documentazione](Documentazione), non in una cartella `docs`.
