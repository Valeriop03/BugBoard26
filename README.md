# BugBoard26

BugBoard26 e' una web app per la gestione collaborativa di issue, sviluppata per il progetto di Ingegneria del Software 2025/2026.

Questo repository e' organizzato con frontend e backend separati:

- `backend`: ASP.NET Core Web API e test.
- `frontend`: Angular.
- `docker`: Compose per PostgreSQL e backend.
- `Documentazione`: requisiti, design, diagrammi e documentazione finale.

## Requisiti assegnati

Il gruppo implementa solo le funzionalita' assegnate: autenticazione con ruoli, creazione e consultazione issue, filtri e ricerca, cambio stato per assegnatario/admin, notifiche di risoluzione, export CSV, archiviazione, suggerimento assegnatario, modalita' readonly e gestione duplicati.

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

Il profilo `http` ascolta su `http://localhost:5256`. Non avviare contemporaneamente il backend Docker e quello sul PC sulla stessa porta.

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

Il volume nominato `bugboard26-postgres-data` del Compose esistente e' mantenuto. L'arresto senza `-v` conserva i dati; non usare `down -v` o eliminare il volume se si vogliono mantenere utenti e issue. Usare lo stesso file Compose e nome di progetto per riutilizzare il volume esistente.

Per un riavvio ordinato, senza rimuovere container o dati:

```powershell
docker compose -f docker/docker-compose.yml stop backend postgres
docker compose -f docker/docker-compose.yml up -d --wait
```

Dopo che `/api/health` risponde, accedere di nuovo e confrontare lista, archivio e dettaglio delle issue con quelli prima dell'arresto.

## Limiti attuali

- Upload immagini non implementato, anche se previsto dai requisiti e dal modello.
- Gestione dei duplicati tramite PATCH status e coerenza di `ResolvedAt` nel percorso duplicate da ricontrollare dopo il merge delle correzioni backend.
- Portata di READONLY sulla lettura delle proprie notifiche da chiarire rispetto a RF-17; il codice attuale la consente.

Requisiti, design, sorgenti dei diagrammi e verifiche sono in [Documentazione](Documentazione), non in una cartella `docs`.
