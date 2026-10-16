# Verifica frontend

## Collaudo reale del 16 ottobre 2026

Codice verificato: `e13995140dff382dc3d548208c6c1e60be91fe5d`.
Checkout pulito prima e dopo; nessuna correzione applicativa necessaria.
Questo report registra la prova gia' eseguita, non una nuova esecuzione
completa sul codice successivo.

Ambiente: Windows, Chrome headless tramite Playwright, Angular sulla porta
4200, ASP.NET Core e PostgreSQL 16 avviati con il Compose del progetto.
Il login e le operazioni hanno usato API e database reali, senza mock HTTP.
Docker inizialmente non era disponibile; dopo l'avvio di Docker Desktop,
build e avvio Compose sono riusciti. Health, login ADMIN e lista hanno
risposto correttamente.

### Dati e isolamento

Sono stati creati tre account con prefisso `collaudo-20261016084339`
(due USER, un READONLY) e le issue 4-7. Le modifiche di stato,
duplicazione e archiviazione hanno riguardato solo questi nuovi dati.
Non sono stati cancellati dati preesistenti o volumi.

Le issue 4-6 sono state create dal form. La 7 e' stata predisposta tramite
API con assegnatario deterministico per verificare il cambio stato USER.
La 4 e' infine duplicata della 5 e archiviata; la 7 e' risolta.
Questi ID descrivono il database locale della prova, non dati iniziali
garantiti su ogni installazione. Non sono riportati token nel report.

### Esiti osservati

| Prova | Esito |
| --- | --- |
| Accesso anonimo a `/issues/new` | Reindirizzamento a `/login`; navbar assente nella login. |
| Login ADMIN corretto / password errata | Accesso riuscito / HTTP 401 con messaggio UI. |
| Ricerca live e digitazione rapida | Risultato dell'ultima parola cercata visualizzato. |
| Filtri tipo, stato, priorita' e ordine titolo | Risposte reali riuscite e lista coerente. |
| Lista vuota e Pulisci | Messaggio corretto; keyword, tre filtri e ordinamento ripristinati. |
| Export CSV | Download riuscito; stessi ID e ordine della lista filtrata. |
| Creazione senza immagine | Issue creata, dettaglio senza immagine. |
| PNG e JPEG validi | Upload riuscito e immagine realmente visualizzata tramite download autenticato. |
| Upload fallito e recupero | JPEG con contenuto invalido rifiutato dal backend; ID conservato; retry con JPEG valido senza seconda POST di creazione. |
| Validazione form e file | Soli spazi: messaggi su entrambi i campi, nessuna POST; formato non ammesso e oltre 5 MiB rifiutati dal frontend. |
| USER assegnatario | Cambio a RESOLVED riuscito e stato verificato nel backend. |
| USER non assegnatario / READONLY | Cambio stato non visibile; PATCH diretto rifiutato con 403. |
| READONLY creazione e immagini | Accesso diretto a creazione reindirizzato; immagine leggibile, azioni di modifica assenti nel dettaglio. |
| Duplicato ADMIN | Stato DUPLICATE e riferimento all'originale aggiornati nel dettaglio e nell'API. |
| Archiviazione ADMIN | Issue esclusa dalla lista principale, presente in archivio e dettaglio apribile dal link. |
| Dettaglio inesistente | Messaggio "Issue non trovata", senza crash. |
| Layout | Lista, form e dettaglio controllati a 1366x900 e 390x844; screenshot ispezionati, nessun overflow orizzontale. |
| Notifica di risoluzione | Presenza della notifica della issue 7 verificata via API per il segnalatore ADMIN. Non e' un collaudo completo della pagina notifiche. |

Build frontend riuscita; 27 test Jasmine/Karma superati senza watch con
ChromeHeadless. I test unitari usano servizi simulati, distinti dalle prove
browser sopra. Alcune asserzioni del test browser temporaneo sono state
corrette per attendere il rendering o usare il testo effettivo della UI,
poi ripetute con successo; non erano errori applicativi.

Gli screenshot selezionati sono nella sottocartella `immagini/`:
lista desktop, lista mobile, form mobile e dettaglio mobile. I mockup
progettuali restano separati da queste evidenze del prodotto eseguito.

### Limiti

Non verificati in questa prova: persistenza dopo ricreazione dei container,
suite backend completa, coverage/Sonar, carico elevato, rete rallentata,
altri browser, sessioni con utenti e questionari. La presenza di una
notifica via API non dimostra tutti i flussi della pagina notifiche.

## Allineamento notifiche READONLY

Decisione del gruppo: READONLY consulta le proprie notifiche, ma non deve
segnarle come lette. La modifica frontend e' successiva al collaudo sopra.
Base del lavoro: `cc2446a62c06bf09232b9cccd2d152dc6f6095f6`; modifiche locali
non committate. Non attribuire i nuovi test al precedente commit e139951.

Il componente usa il ruolo reale di AuthService per nascondere il pulsante
e impedire l'invio anche se markAsRead viene chiamato direttamente.
ADMIN e USER mantengono la stessa operazione sulle proprie notifiche.
Un 403 produce un messaggio chiaro, senza aggiornare localmente la notifica.
Il backend resta responsabile della sicurezza.

Sei test mirati verificano con servizi simulati: consultazione READONLY
senza pulsante, invocazione diretta bloccata, comportamento ADMIN e USER,
errore 403 e identita' assente.

Verifica eseguita il 16 ottobre 2026 sul checkout cc2446a con queste
modifiche locali: `npm run build` riuscito; suite completa frontend
senza watch, ChromeHeadless 155: **33 test superati, 0 falliti**
(27 preesistenti e 6 nuovi). `git diff --check` senza errori.
Non e' stata ripetuta l'intera demo con API reali dopo questa modifica;
i sei nuovi test sono prove isolate del componente, non del backend.

La correzione backend READONLY non e' ancora presente nel checkout letto
all'inizio di questa attivita'. Dopo l'integrazione del collega occorre
verificare con una propria notifica READONLY il PATCH diretto: atteso 403
e IsRead invariato. Non e' dichiarata completata questa verifica integrata.

## Comandi e ripetizione

Dalla radice, con Docker Desktop operativo:

```powershell
docker compose -f docker/docker-compose.yml up -d --build
Invoke-RestMethod http://localhost:5256/api/health
cd frontend
npm run build
node node_modules/@angular/cli/bin/ng.js test --watch=false --browsers=ChromeHeadless
npm start
```

Per ripetere il collaudo creare nuovi account/issue con un prefisso distinto,
eseguire i passi della tabella e confrontare risposte, dati e interfaccia.
Non riutilizzare le issue gia' duplicate/archiviate per provare il percorso
di creazione o risoluzione. Registrare il nuovo commit e i nuovi esiti;
il precedente successo non si estende automaticamente alle revisioni future.

Sequenza demo: login, creazione con immagine, ricerca/filtri/export,
risoluzione come assegnatario, notifica al segnalatore, duplicato/archivio
come ADMIN, consultazione READONLY. Il gruppo deve ancora provare insieme
presentazione e demo sulla revisione finale.
