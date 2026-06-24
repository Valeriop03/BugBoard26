# BugBoard26

BugBoard26 e' una web app per la gestione collaborativa di issue, sviluppata per il progetto di Ingegneria del Software 2025/2026.

Questo repository e' organizzato con frontend e backend separati:

- `backend`: ASP.NET Core Web API.
- `frontend`: Angular.
- `docker`: servizi locali di supporto, per ora PostgreSQL.
- `docs`: documentazione di progetto da completare in modo incrementale.

## Requisiti assegnati

Il gruppo implementa solo le funzionalita' assegnate: autenticazione con ruoli, creazione e consultazione issue, filtri e ricerca, cambio stato per assegnatario/admin, notifiche di risoluzione, export CSV, archiviazione, suggerimento assegnatario, modalita' readonly e gestione duplicati.

## Avvio locale

Database:

```powershell
docker compose -f docker/docker-compose.yml up -d
```

Backend:

```powershell
dotnet run --project backend/BugBoard26.Api/BugBoard26.Api.csproj
```

Frontend:

```powershell
cd frontend
npm start
```

## Database locale

Per lo sviluppo locale viene usato PostgreSQL in Docker.

- Host: `localhost`
- Porta: `5432`
- Database: `bugboard26`
- Utente: `bugboard26`
- Password: `bugboard26`

La stringa di connessione verra' collegata al backend quando sara' introdotta la persistenza.
