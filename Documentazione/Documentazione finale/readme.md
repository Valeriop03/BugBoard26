La consegna finale include tutta la documentazione prodotta nelle prime fasi (eventualmente aggiornata e rivista) nonchè tutti i documenti sulla progettazione e implementazione non ancora realizzata nelle fasi precedenti.

Eventuali materiali necessari alla presentazione e discussione del progetto possono essere anch'essi caricati in questa cartella.

Le informazioni riguardanti l'installazione e utilizzo del progetto vanno invece scritte nel readme della cartella principale, coerentemente con la modalità di fruizione dei normali progetti open source.

## Verifica implementazione e avvio locale - 2 ottobre 2026

Revisione di UC-02, RF-10/RF-11/RF-17, RNF-02/RNF-05, architettura implementata, cambio stato, notifiche e avvio locale. I requisiti originali restano invariati; comportamento attuale e differenze sono distinti.

- [Specifica requisiti, UC-02 e matrice di verifica](../Primo%20homework/specifica_requisiti.tex).
- [Design: ruoli, notifiche e confronto del diagramma](../Secondo%20homework/documento_design.tex).
- Il diagramma aggiornato usa [sequence_cambio_stato.puml](../Secondo%20homework/diagrammi_comportamentali/sequence_cambio_stato.puml) e il relativo PNG, generato con PlantUML 1.2026.8. Il vecchio `.vpd` non è stato modificato: resta storico e non va usato per riesportare il PNG aggiornato.

### Differenze da discutere

1. Il PATCH status accetta ancora `Duplicate` anche per un USER assegnatario, senza originale; il frontend disabilita invece il cambio di una issue duplicata. `MarkAsDuplicate` conserva l'eventuale `ResolvedAt`. Riferimento: `backend/BugBoard26.Api/Controllers/IssuesController.cs`, `UpdateStatus` e `MarkAsDuplicate`; confronto con UC-03. Sono punti da ricontrollare dopo il merge delle correzioni backend del collega, non correzioni completate in questa revisione. Non si inventano nuove regole sulle transizioni.
2. READONLY può segnare come letta una propria notifica, comportamento verificato anche dai test. La portata di RF-17 ("senza modificare dati") va chiarita prima di considerarlo un'eccezione ammessa. Riferimento: `backend/BugBoard26.Api/Controllers/NotificationsController.cs`, `MarkAsRead`.
3. Upload immagini non implementato: `Issue.ImagePath` esiste, ma mancano endpoint di upload e campo nel DTO di creazione; RF-05 non viene modificato per giustificare questa assenza.
4. I diagrammi iniziali di componenti, classi, schema dati, creazione e duplicati restano da allineare. In `component_diagram.vpd`/PNG eliminare Repository e collegare i controller a `ApplicationDbContext` e `IssuesController` a `IssueDomainService`; in `class_diagram.vpd` e immagini sostituire `AppDbContext` e servizi/repository assenti con le classi reali. In `class_diagram` e `database_schema.vpd`/PNG sostituire GUID con identificativi interi e allineare colonne, lunghezze e nullabilità alla migrazione `InitialCreate`, come specificato nel design. Nei sorgenti e PNG `sequence_creazione_issue` e `sequence_issue_duplicata` rappresentare query e salvataggio del controller sul contesto, senza servizi/repository fittizi. Il design indica le rispettive cartelle. Solo il diagramma cambio stato è stato rigenerato.

Le annotazioni obsolete sui permessi frontend sono state rimosse: `canChangeStatus` mostra il pannello solo ad ADMIN o USER assegnatario; `canCreateIssue` e `issueCreationGuard` escludono READONLY dalla creazione e `adminGuard` protegge la pagina utenti. Riferimenti: componente dettaglio issue, componente lista, `frontend/src/app/services/auth.guard.ts` e `app.routes.ts`.

### Test e avvio realmente verificati

- `dotnet test backend/BugBoard26.slnx`: **56 superati, 0 falliti, 0 ignorati**, con PostgreSQL di test separato sulla porta 55432. Non sono stati modificati test o configurazioni. Rimane l'avviso NU1903 su `Microsoft.OpenApi` 2.0.0; aggiornare dipendenze non rientra in questa attività.
- `IssueStatusIntegrationTests` copre permessi ADMIN/USER assegnatario, rifiuto USER non assegnatario e READONLY, risoluzione con date e notifica persistite, stesso stato senza nuove scritture, 401 e 404. `NotificationsIntegrationTests` copre elenco personale ordinato, lettura delle proprie notifiche, rifiuto delle altrui anche per ADMIN, 401, 404 ed elenco vuoto. La riapertura, seconda risoluzione e ripetizione del PATCH read restano senza casi dedicati in questi file.
- `docker compose config`, build e avvio del backend .NET 10 verificati. `/api/health`, login admin, lista, archivio e dettaglio issue rispondono correttamente; lista senza token: 401. Provati anche login e lista dal frontend locale.
- Riavvio ordinato con `stop backend postgres` e `up -d --wait`: stesso volume `docker_bugboard26-postgres-data`, stessi conteggi e impronte delle righe prima/dopo (**2 utenti, 3 issue, 0 notifiche, 1 migrazione**). Ripetuti login e letture API. Nessun dato di sviluppo creato, cancellato o modificato per la prova; i database temporanei dei test sono distinti.

### Checklist manuale - non eseguita

Preparare account attivi distinti: segnalatore S (USER), assegnatario A (USER), altro utente U (USER), R (READONLY) e un ADMIN. Annotare ID issue, ID notifica, date e risposte HTTP prima/dopo. Usare il token dell'account indicato nelle richieste API; gli stati API sono `Todo`, `InProgress`, `Resolved`, `Closed`, `Duplicate`.

- [ ] **Creazione:** come S creare una issue assegnata ad A. Il form usa l'assegnatario suggerito: verificarne l'identità; per una prova deterministica usare `POST /api/issues` con `assignedToId` di A, titolo/descrizione validi e `type: "Bug"`. Atteso: 201, stato `Todo`, `resolvedAt: null`, `createdById` di S.
- [ ] **Risoluzione:** come A scegliere RESOLVED nel dettaglio, oppure `PATCH /api/issues/{id}/status` con `{"status":"Resolved"}`. Atteso: 200, stato risolto, `updatedAt` aggiornato e `resolvedAt` valorizzato in UTC.
- [ ] **Notifica:** come S aprire o ricaricare `/notifications`. Atteso: notifica per la issue, messaggio di risoluzione e `isRead: false`; `GET /api/notifications` contiene solo notifiche con `userId` di S. Se A è distinto da S, non riceve questa notifica di risoluzione.
- [ ] **Lettura:** come S premere "Segna come letta". Atteso: PATCH `/api/notifications/{id}/read` 200, `isRead: true` e persistenza dopo ricaricamento. Ripetere via API: resta letta, senza nuova notifica.
- [ ] **Stesso stato e riapertura:** come A ripetere `Resolved`; atteso 200 con date e numero di notifiche invariati. Cambiare poi in `InProgress`: `resolvedAt` diventa null, `updatedAt` cambia e la vecchia notifica conserva lo stato di lettura. Tornare in `Resolved`: nuova data e nuova notifica per S. Questa prova descrive il codice, non prescrive una sequenza obbligatoria.
- [ ] **ADMIN non assegnatario:** su una issue di prova, cambio stato consentito con 200 anche se assegnata ad A.
- [ ] **USER non assegnatario:** come U il pannello cambio stato non compare; tentare il PATCH diretto. Atteso 403 e nessun cambiamento di stato, date o notifiche.
- [ ] **READONLY:** come R il pannello cambio stato non compare; il PATCH diretto è rifiutato con 403 anche se l'account risultasse assegnatario. Nessun cambiamento di stato, date o notifiche.
- [ ] **Notifica altrui:** con U, A, R e ADMIN controllare che GET non restituisca la notifica di S; usare il suo ID nel PATCH read deve dare 403 senza cambiarla. Un ID inesistente dà 404; senza autenticazione le API notifiche danno 401. Non è previsto un GET di dettaglio per ID.
- [ ] **Differenze aperte:** con USER assegnatario provare su una issue separata il PATCH status `{"status":"Duplicate"}` senza originale: il codice attuale lo accetta, da confrontare con UC-03. Se esiste una notifica di proprietà di R, il suo PATCH read attualmente dà 200: registrare l'esito senza assumere risolta l'ambiguità RF-17; se manca il dato indicare "non verificato".

### Verifica documentale

Sono stati confrontati requisiti, controller, `ApplicationDbContext`, `IssueDomainService`, modelli, migrazioni e pagine frontend con la documentazione. I test e le prove di avvio riportati sopra sono stati eseguiti; la checklist manuale completa cambio stato/notifiche **non è stata eseguita**. La compilazione LaTeX verifica i documenti, non il funzionamento dell'applicazione.

Entrambi i sorgenti sono stati compilati con Tectonic 0.17.0 e i PDF di consegna aggiornati: **14 pagine per la specifica e 18 per il design**, copertina inclusa. Controllati i render e le pagine modificate; nessun testo fuori pagina rilevato. Restano avvisi non bloccanti Underfull nella specifica, sull'ancora della copertina e sulla configurazione dei font del compilatore portatile; i PDF sono generati correttamente.

Per ricompilare, con Tectonic disponibile nel PATH, eseguire ciascun comando dalla cartella che contiene il relativo sorgente:

```powershell
tectonic --untrusted specifica_requisiti.tex
tectonic --untrusted documento_design.tex
```

I PDF di consegna mantengono i nomi `Primo homework/Specifica Requisiti.pdf` e `Secondo homework/Documento_Design.pdf`; dopo una ricompilazione sostituirli con gli output `specifica_requisiti.pdf` e `documento_design.pdf`. Controllare indice, UC-02, architettura, ruoli, notifiche e diagramma. Per rigenerare il solo cambio stato, dalla cartella `Secondo homework`, con il JAR ufficiale PlantUML:

```powershell
java '-Djava.awt.headless=true' -jar "C:\percorso\plantuml.jar" -tpng diagrammi_comportamentali/sequence_cambio_stato.puml
```
