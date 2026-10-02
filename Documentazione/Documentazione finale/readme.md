La consegna finale include tutta la documentazione prodotta nelle prime fasi (eventualmente aggiornata e rivista) nonchè tutti i documenti sulla progettazione e implementazione non ancora realizzata nelle fasi precedenti.

Eventuali materiali necessari alla presentazione e discussione del progetto possono essere anch'essi caricati in questa cartella.

Le informazioni riguardanti l'installazione e utilizzo del progetto vanno invece scritte nel readme della cartella principale, coerentemente con la modalità di fruizione dei normali progetti open source.

## Verifica cambio stato e notifiche - 1 ottobre 2026

Revisione limitata a UC-02, RF-10/RF-11/RF-17, RNF-02/RNF-05 e alle sezioni di design del cambio stato e delle notifiche. I requisiti originali restano invariati; i dettagli del codice e le differenze sono etichettati separatamente.

- [Specifica requisiti, UC-02 e matrice di verifica](../Primo%20homework/specifica_requisiti.tex).
- [Design: ruoli, notifiche e confronto del diagramma](../Secondo%20homework/documento_design.tex).
- Il diagramma `sequence_cambio_stato.vpd` e il PNG restano da correggere in Visual Paradigm. La sezione "Cambio stato con notifica" elenca lifeline, condizioni e salvataggi da aggiornare.

### Differenze da discutere

1. Il frontend mostra il cambio stato anche ai USER non assegnatari; l'API lo rifiuta. Riferimento: `frontend/src/app/pages/issue-detail-page/issue-detail-page.component.html`, blocco `!isReadonly`; RNF-05/UC-02.
2. Il PATCH status può impostare `Duplicate` anche per un USER assegnatario, senza originale, e non applica il blocco mostrato dal frontend su una issue duplicata. È diverso dal design amministrativo e da UC-03. Inoltre `MarkAsDuplicate` conserva l'eventuale `ResolvedAt`. Riferimento: `backend/BugBoard26.Api/Controllers/IssuesController.cs`, metodi `UpdateStatus` e `MarkAsDuplicate`. Non si deducono da questo nuove regole sulle transizioni.
3. READONLY può segnare come letta una propria notifica. La portata di RF-17 ("senza modificare dati") va chiarita prima di considerare questa scrittura un'eccezione ammessa. Riferimenti: `backend/BugBoard26.Api/Controllers/NotificationsController.cs`, `MarkAsRead`, e pagina frontend notifiche.
4. Il diagramma usa servizi/repository previsti ma assenti dal flusso implementato e due salvataggi; il controller usa direttamente `ApplicationDbContext`, il controllo `IssueDomainService` e un solo `SaveChangesAsync`.

### Checklist manuale - non eseguita

Preparare account attivi distinti: segnalatore S (USER), assegnatario A (USER), altro utente U (USER), R (READONLY) e un ADMIN. Annotare ID issue, ID notifica, date e risposte HTTP prima/dopo. Usare il token dell'account indicato nelle richieste API; gli stati API sono `Todo`, `InProgress`, `Resolved`, `Closed`, `Duplicate`.

- [ ] **Creazione:** come S creare una issue assegnata ad A. Il form usa l'assegnatario suggerito: verificarne l'identità; per una prova deterministica usare `POST /api/issues` con `assignedToId` di A, titolo/descrizione validi e `type: "Bug"`. Atteso: 201, stato `Todo`, `resolvedAt: null`, `createdById` di S.
- [ ] **Risoluzione:** come A scegliere RESOLVED nel dettaglio, oppure `PATCH /api/issues/{id}/status` con `{"status":"Resolved"}`. Atteso: 200, stato risolto, `updatedAt` aggiornato e `resolvedAt` valorizzato in UTC.
- [ ] **Notifica:** come S aprire o ricaricare `/notifications`. Atteso: notifica per la issue, messaggio di risoluzione e `isRead: false`; `GET /api/notifications` contiene solo notifiche con `userId` di S. Se A è distinto da S, non riceve questa notifica di risoluzione.
- [ ] **Lettura:** come S premere "Segna come letta". Atteso: PATCH `/api/notifications/{id}/read` 200, `isRead: true` e persistenza dopo ricaricamento. Ripetere via API: resta letta, senza nuova notifica.
- [ ] **Stesso stato e riapertura:** come A ripetere `Resolved`; atteso 200 con date e numero di notifiche invariati. Cambiare poi in `InProgress`: `resolvedAt` diventa null, `updatedAt` cambia e la vecchia notifica conserva lo stato di lettura. Tornare in `Resolved`: nuova data e nuova notifica per S. Questa prova descrive il codice, non prescrive una sequenza obbligatoria.
- [ ] **ADMIN non assegnatario:** su una issue di prova, cambio stato consentito con 200 anche se assegnata ad A.
- [ ] **USER non assegnatario:** come U tentare il cambio stato anche via API. Atteso 403 e nessun cambiamento di stato, date o notifiche. Il pulsante attualmente visibile è la differenza UI annotata, non un'autorizzazione.
- [ ] **READONLY:** come R il pannello cambio stato non compare; il PATCH diretto è rifiutato con 403 anche se l'account risultasse assegnatario. Nessun cambiamento di stato, date o notifiche.
- [ ] **Notifica altrui:** con U, A, R e ADMIN controllare che GET non restituisca la notifica di S; usare il suo ID nel PATCH read deve dare 403 senza cambiarla. Un ID inesistente dà 404; senza autenticazione le API notifiche danno 401. Non è previsto un GET di dettaglio per ID.
- [ ] **Differenze aperte:** con USER assegnatario provare su una issue separata il PATCH status `{"status":"Duplicate"}` senza originale: il codice attuale lo accetta, da confrontare con UC-03. Se esiste una notifica di proprietà di R, il suo PATCH read attualmente dà 200: registrare l'esito senza assumere risolta l'ambiguità RF-17; se manca il dato indicare "non verificato".

### Verifica documentale

Sono state lette le fonti indicate e confrontati UC-02 e l'immagine del diagramma con il codice. Le prove della checklist e i test backend/frontend **non sono stati eseguiti**. La compilazione LaTeX verifica i documenti, non il funzionamento dell'applicazione.

Entrambi i sorgenti sono stati compilati con successo con Tectonic 0.17.0; i PDF di consegna sono stati aggiornati (14 pagine per la specifica, 18 per il design). Sono stati controllati i render dei documenti e delle pagine modificate. Restano avvisi di impaginazione Underfull nella specifica e sull'ancora della copertina; non sono emersi errori di compilazione o testo oltre i margini nelle pagine controllate.

Per ricompilare, con Tectonic disponibile nel PATH, eseguire ciascun comando dalla cartella che contiene il relativo sorgente:

```powershell
tectonic --untrusted specifica_requisiti.tex
tectonic --untrusted documento_design.tex
```

I PDF di consegna mantengono i nomi `Primo homework/Specifica Requisiti.pdf` e `Secondo homework/Documento_Design.pdf`; dopo una ricompilazione sostituirli con gli output `specifica_requisiti.pdf` e `documento_design.pdf`. Controllare indice, UC-02, ruoli, notifiche e annotazioni al diagramma. Nessun diagramma è dichiarato aggiornato finché non vengono corretti sorgente e immagine.
