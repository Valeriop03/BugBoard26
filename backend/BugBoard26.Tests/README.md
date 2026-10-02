# Test backend

Prerequisiti: .NET SDK 10 e Docker con container Linux (oppure PostgreSQL 16
dedicato ai test). Non occorre avviare separatamente l'API.

Eseguire dalla radice del repository:

```powershell
docker compose -p bugboard26-tests -f backend/BugBoard26.Tests/docker-compose.tests.yml up -d --wait
dotnet test backend/BugBoard26.slnx
docker compose -p bugboard26-tests -f backend/BugBoard26.Tests/docker-compose.tests.yml down
```

Il container usa la porta locale `55432`, il database `bugboard26_tests` e
utente/password `bugboard26_tests`. Non usa il container, la porta o il volume
di sviluppo. I dati del container di test sono temporanei.

Per un PostgreSQL di test gia' disponibile, creare un database vuoto chiamato
esattamente `bugboard26_tests` e usare un utente con permesso `CREATEDB` e
possibilita' di eliminare i database da lui creati:

```powershell
$env:BUGBOARD26_TEST_CONNECTION_STRING = 'Host=127.0.0.1;Port=55432;Database=bugboard26_tests;Username=bugboard26_tests;Password=bugboard26_tests'
dotnet test backend/BugBoard26.slnx
Remove-Item Env:BUGBOARD26_TEST_CONNECTION_STRING
```

Non impostare questa variabile con credenziali o database di sviluppo.
La configurazione rifiuta qualsiasi database di partenza con nome diverso da
`bugboard26_tests`, senza usare come fallback la connessione dell'API.

## Isolamento e funzionamento

Ogni caso di test crea un database `bugboard26_test_<guid>` distinto, avvia
`WebApplicationFactory<Program>` e lascia che l'API applichi le migrazioni reali.
Il contesto EF viene sostituito prima del seeder iniziale, mantenendo Npgsql.
Alla fine viene eliminato soltanto il database creato da quel caso; non vengono
svuotate tabelle o eliminati altri database. Se il processo viene interrotto
forzatamente, possono restare database temporanei nell'istanza di test.

Gli utenti ADMIN, USER, READONLY e inattivo hanno password generate con
l'hasher reale. I JWT vengono ottenuti tramite HTTP da `/api/auth/login`:
non vengono simulati autenticazione, ruoli o persistenza. Le richieste HTTP
attraversano TestServer e tutta la pipeline ASP.NET Core, senza una porta API
reale. PostgreSQL e' invece un processo reale, non EF InMemory o SQLite.

I test verificano login, accesso autenticato, creazione utenti senza esposizione
della password/hash, archiviazione e duplicazione con relativi permessi.
Coprono anche creazione issue e ricerca case-insensitive tramite `ILike`.
Nei casi rifiutati confrontano uno snapshot di utenti, issue e notifiche prima
e dopo la richiesta, includendo hash, date e riferimenti.

Ogni test ha dati e client propri, quindi non dipende dall'ordine di esecuzione
e puo' girare in parallelo agli altri. PostgreSQL mancante produce un errore
esplicito: i test non vengono saltati automaticamente e non usano database finti.

Per eseguire soltanto una categoria:

```powershell
dotnet test backend/BugBoard26.slnx --filter 'Category=Integration'
dotnet test backend/BugBoard26.slnx --filter 'Category!=Integration'
```

I test di dominio preesistenti sono mantenuti senza modifiche.
