# Documentazione finale

## Revisione del 19 ottobre 2026

La relazione unificata e' alla versione 1.7: revisione editoriale con
promemoria, coordinamento e istruzioni di manutenzione separati in
[note operative interne](note_operative_gruppo.md), da escludere dallo ZIP.
Nel PDF restano descrizione del sistema, risultati e limiti documentati.
La revisione del testo non implica una nuova esecuzione dei test o la
chiusura delle attivita' di presentazione, release e invio.

Aggiornamento diagrammi sul commit `b95d008` con modifiche documentali
locali: il modello dei casi d'uso distingue consultazione delle notifiche
per tutti i ruoli e lettura riservata ai proprietari USER/ADMIN. La sequenza
di cambio stato esplicita anche il rifiuto READONLY del PATCH read.
Rigenerati i PNG interessati e i tre PDF nella revisione 1.6, precedente
alla pulizia editoriale. Nessuna modifica applicativa o nuova esecuzione dei test.
Gli otto VPD restano storici: l'allineamento nell'editor originale non e'
verificato e non viene dichiarato concluso dall'aggiornamento dei PNG.

L'audit tecnico riguarda `6aa175a357dbdf1c35702686b985d46bff58c43e` con
correzioni locali identificate nei report. L'integrazione successiva del
survey e del fix CSV parte da `06ae1b0`, ancora con modifiche locali.
Sono registrate tre sessioni USER riferite dal moderatore e completata la
review sistematica assistita. La revisione editoriale e' completata;
allineamento dei VPD storici nell'editor originale e prova cronometrata
non sono dichiarati eseguiti. I VPD non fanno parte dello ZIP concordato.

- [Documento unico](documentazione_finale.pdf) e [sorgente](documentazione_finale.tex).
- [Presentazione aggiornata](presentazione/presentazione_tecnica_20261019.pptx):
  9 slide e 540 secondi pianificati nell'audit tecnico. Da allineare alla
  successiva raccolta di usabilita'; non e' una prova cronometrata.
- [Audit corrente](verifica-backend/audit-20261019.md): 166 test backend, Sonar
  realmente elaborato, 0 bug e 0 vulnerabilita', gate OK e limiti residui.
- [Collaudo integrato del 19 ottobre](verifica-backend/finalizzazione-20261019.md)
  e [report frontend storico](verifica-frontend/report.md), mantenuti distinti.
- [Test plan](esiti_test_plan.csv): ogni esito rimanda a test HTTP e, dove
  richiesto, al browser reale; non e' una prova con persone.
- [Registro](evidenze.md), [qualita' backend](qualita_backend.md),
  [checklist di consegna](consegna.md) e [usabilita'](usabilita.md).
- [Review sistematica completata](verifica-frontend/review-20261019/report.md):
  otto criteri, evidenze, correzioni di contrasto e limiti espliciti.
- [Scheda per utenti reali](verifica-backend/sessione-utenti-pronta.md) e
  [sessioni e risposte ricevute](evidenze/risposte-parziali-usabilita.md):
  tre sessioni USER del 19/10 e 15 risposte numeriche; risposte aperte non
  fornite, senza inventare difficolta' o assenza di problemi.

RF-17 e' implementato e verificato: READONLY legge le proprie notifiche ma
non le segna lette, anche se gia' lette. La decisione e' del gruppo, non dei
docenti. I report si trovano sotto `Documentazione/Documentazione finale/`,
non in una cartella `verifica-backend/` alla radice del repository.

Per rigenerare sul PC corrente, dalla radice del repository:

```powershell
& '.\Documentazione\Documentazione finale\rigenera.ps1' -SoloDocumento `
  -TectonicExecutable "$env:LOCALAPPDATA\BugBoard26\document-tools\tectonic-0.17.0\tectonic.exe"
```

Senza `-SoloDocumento` vengono generate anche slide in un nuovo file, senza
sovrascrivere il PowerPoint preesistente. I percorsi dei runtime sono
parametrizzabili; lo script rileva la skill Presentations installata anziche'
vincolarsi a una vecchia versione. Controllare sempre PDF e slide generati.

## Integrazione del 16 ottobre 2026 - storico

Le indicazioni successive su verifiche mancanti descrivono il checkout di
allora: per lo stato corrente usare il riepilogo del 19 ottobre e i suoi report.

La base è il **documento unico già creato da Valerio**, acquisito nuovamente prima dell'integrazione. Il suo riepilogo del collaudo frontend reale è conservato integralmente, con attribuzione della provenienza. Non è stato creato un secondo documento finale. Le note del 2 e 9 ottobre sotto restano storiche.

- [Documento unico: sorgente](documentazione_finale.tex) e [PDF](documentazione_finale.pdf): requisiti, design del sistema/software e testing in capitoli distinti.
- [Presentazione tecnica](presentazione/presentazione_tecnica.pptx): 9 slide, budget previsto di 9 minuti, prova cronometrata ancora da svolgere.
- [Registro delle evidenze](evidenze.md): frontend reale riportato da Valerio, risultati backend/persistenza/Sonar attesi da amico 1, attività e chiarimenti aperti.
- [Test plan: esiti da compilare](esiti_test_plan.csv): mantenuti TP-01--TP-10 e aggiunti TP-11--TP-14. I risultati attesi non sono esiti superati.
- [Usabilità](usabilita.md): review tecnica, attività con utenti reali/potenziali e questionario, con modelli vuoti in `evidenze/`.
- [Consegna e demo](consegna.md): coordinamento da confermare con Valerio, checklist release GitHub/invio individuale Teams e comandi Git soltanto proposti.
- [Manifest dei diagrammi](diagrammi_manifest.json): corrispondenza sorgenti/PNG; VPD invariati e storici. Il nuovo deployment locale è distinto da quello precedente. Accettazione CASE ancora aperta.
- [Verifica documentale](verifica_documentale.md): controlli effettivamente svolti e limiti. Non equivale a collaudo applicativo.

Amico 1 depositerà suite backend completa, coverage, persistenza dopo ricreazione dei container e analisi SonarQube effettiva in `verifica-backend/`, alla radice. Valerio preparerà il report permanente delle prove frontend già svolte. I nuovi report non sono ancora disponibili al controllo. Il gruppo deve svolgere review e sessioni con utenti, raccogliere risposte reali e chiudere la conformità CASE. Nessun messaggio o consegna è stato inviato.

**RF-17, decisione del gruppo del 16 ottobre 2026:** READONLY può consultare le proprie notifiche ma non segnarle come lette. Non è un chiarimento dei docenti. Backend e test sono affidati ad amico 1; frontend da allineare e verificare. Test plan e slide distinguono questo contratto dagli esiti tecnici ancora aperti. Il collaudo precedente non verifica il nuovo allineamento. La gestione del documento unico e della presentazione rimane a questa attività documentale.

### Rigenerazione

Prerequisiti: Python con Pillow, Tectonic 0.17.0 e, per i diagrammi, Java 21 con PlantUML 1.2026.8. Per le slide serve il runtime Node di Codex con `@oai/artifact-tool` e il finalizzatore della skill Presentations. Tutti i percorsi sono parametrizzabili in `rigenera.ps1`; il PowerPoint finale rimane modificabile anche senza questo runtime.

Dalla radice della repository, se Tectonic è nel PATH:

```powershell
& '.\Documentazione\Documentazione finale\rigenera.ps1'
# Per il solo documento:
& '.\Documentazione\Documentazione finale\rigenera.ps1' -SoloDocumento
```

Sul PC di questa revisione è stato verificato anche il percorso portatile seguente:

```powershell
& '.\Documentazione\Documentazione finale\rigenera.ps1' `
  -TectonicExecutable 'C:\Users\emanu\AppData\Local\Temp\bugboard26-document-tools-ea4e0bb253ba48668e3e70c648b514bf\tectonic\tectonic.exe'
```

La cartella temporanea non è una dipendenza da consegnare: se rimossa, indicare una propria installazione. Per rigenerare anche le immagini usare `-RigeneraDiagrammi -PlantUmlJar 'C:\percorso\plantuml.jar'`. Senza questo switch lo script verifica i PNG esistenti e interrompe la procedura se il sorgente incorporato non coincide. Non aggiorna VPD o codice applicativo. I frammenti `strategie_dominio.tex`, `test_plan.tex` e `vincoli_case.tex` devono restare accanto al documento unico; lo script non riscrive quest'ultimo.

Gli output di lavoro sono sotto `_build/` e non vanno aggiunti alla consegna. Se esiste già un PPTX finale, una rigenerazione produce un nuovo nome con suffisso per preservare quello precedente: scegliere il file stampato dal comando dopo averlo controllato. La validazione automatica non sostituisce il controllo visivo di tutte le pagine e slide dopo modifiche. Nessuna build o suite applicativa viene eseguita da `rigenera.ps1`.

---

## Materiale e verifiche precedenti (storico)

Le osservazioni seguenti su RF-17 descrivono codice e dubbi del 9 ottobre; la decisione del gruppo riportata sopra le supera come criterio di accettazione. Non attestano che backend, test o frontend siano già allineati. I PDF dei homework restano storici; la legenda del diagramma dei casi d’uso richiamato dal documento unico è aggiornata alla decisione del gruppo.

Stato del codice, verifiche e punti aperti. Istruzioni di installazione nel [README principale](../../README.md); perimetro del gruppo nella specifica requisiti, confrontato con la [traccia ufficiale](../../Progetto-SWE-2025-2026%20-%20canale%202%20-%20v1.1.pdf), paragrafi 3 e 8.

## Allineamento documentazione - 9 ottobre 2026

Revisione sul commit `15ca2e2`: casi d'uso, architettura, schema dati, funzionalita' assegnate e avvio locale. I requisiti originali restano invariati; comportamento attuale e verifiche sono distinti. Gli altri colleghi stanno verificando frontend e test: i risultati storici non certificano la consegna finale.

- [Specifica requisiti, UC-02 e matrice di verifica](../Primo%20homework/specifica_requisiti.tex).
- [Design: ruoli, notifiche e confronto del diagramma](../Secondo%20homework/documento_design.tex).
- I diagrammi aggiornati usano i sorgenti PlantUML e PNG indicati sotto. I `.vpd` non sono stati modificati: restano storici e non vanno usati per riesportare le immagini aggiornate.

### Funzionalita' nel checkout

La tabella descrive **codice presente**, non prove end-to-end eseguite. Numeri della traccia come riportati nell'assegnazione della specifica.

| N. | Comportamento attuale | Riferimento backend |
| --- | --- | --- |
| 1 | Login email/password, PBKDF2, JWT; seed ADMIN se assente. Solo ADMIN crea/elenca utenti; hash non esposto. | `AuthController`, `UsersController`, `DatabaseSeeder` |
| 2 | ADMIN/USER creano una issue Todo; assegnatario opzionale validato. Immagine PNG/JPEG entro 5 MiB, upload separato per ADMIN o USER segnalatore; lettura autenticata. | `IssuesController.Create/UploadImage/GetImage`, `IssueImageStorage` |
| 3, 11 | Lista senza archiviati; filtri tipo/stato/priorita'/segnalatore/assegnatario, ricerca titolo/descrizione, ordinamento data/priorita'/stato/titolo. Nessuna paginazione. | `IssueListQuery`, `IssuesController.ApplyIssueQuery/ApplySorting` |
| 6 | ADMIN o USER assegnatario cambia stato; risoluzione notifica il segnalatore. Ogni account consulta/segna lette solo notifiche proprie. | `IssueDomainService.CanChangeIssueStatus`, `NotificationsController` |
| 8 | CSV UTF-8 con stessa query e ordine della lista, incluse esclusione archiviati e filtri; accessibile anche a READONLY. Non esporta immagini o notifiche. | `IssuesController.Export` |
| 13 | Solo ADMIN archivia; archivio separato e dettaglio conservato. Nessuna API di ripristino. | `IssuesController.Archive/GetArchived/GetById` |
| 14 | Suggerimento consultabile da tutti i ruoli: candidati attivi ADMIN/USER, minimo carico Todo/InProgress non archiviato, parita' per ID. Non assegna automaticamente; 404 senza candidati. | `IssuesController.SuggestAssignee`, `IssueDomainService.SuggestAssignee` |
| 15, 16 | READONLY consulta ma non modifica issue. Solo ADMIN marca Duplicate, collega l'originale e azzera ResolvedAt. | `IssuesController`, controlli descritti sotto |

L'upload immagini **non e' assente**: codice, frontend e test sono gia' nel checkout. Restano da confermare collaudo completo e persistenza, non da dichiarare implementata una nuova funzionalita'. Commenti, dashboard, tag, cronologia, report mensili, scadenze e gestione separata delle assegnazioni con notifica non fanno parte dell'assegnazione qui documentata e non sono dichiarati presenti.

### Comportamento attuale e punti aperti

1. `IssuesController.UpdateStatus` ora rifiuta `Duplicate` con 400, anche per ADMIN, e il cambio di issue gia' duplicate con 409 (stato o riferimento all'originale valorizzato). `MarkAsDuplicate`, solo ADMIN, richiede un originale esistente diverso dalla issue e azzera `ResolvedAt`, senza notifica nuova. Controlli letti nel codice e coperti dai test dedicati presenti, **non eseguiti in questa revisione**. Non si inventa una matrice generale di transizioni.
2. READONLY può segnare come letta una propria notifica, comportamento previsto anche dai test presenti. La portata di RF-17 ("senza modificare dati") va chiarita prima di considerarlo un'eccezione ammessa. Riferimento: `backend/BugBoard26.Api/Controllers/NotificationsController.cs`, `MarkAsRead`.
3. `IssuesController.Create` accetta `AssignedToId` nullo; se indicato deve esistere ed essere attivo, ADMIN o USER. Inesistente/inattivo/READONLY: 400 senza creazione. `IssueAssigneeValidationIntegrationTests` verifica questi casi e la compatibilita' del suggerimento.
4. Il DTO di creazione non include l'immagine: l'upload e' separato e un errore non annulla la issue gia' salvata. Dockerfile e Compose gia' configurano directory scrivibile non root e volume `bugboard26-issue-images`; nessun file Docker e' stato modificato. Con `dotnet run` i file sono invece in `uploads/issue-images` sotto la radice API: passare tra i due avvii non trasferisce i file. Validazione per estensione e marcatori PNG/JPEG, non decodifica completa o antivirus. RF-05 resta invariato.

Le annotazioni obsolete sui permessi frontend sono state rimosse: `canChangeStatus` mostra il pannello solo ad ADMIN o USER assegnatario; `canCreateIssue` e `issueCreationGuard` escludono READONLY dalla creazione e `adminGuard` protegge la pagina utenti. Riferimenti: componente dettaglio issue, componente lista, `frontend/src/app/services/auth.guard.ts` e `app.routes.ts`.

### Verifiche precedenti - 2 ottobre 2026

- `dotnet test backend/BugBoard26.slnx`: **56 superati, 0 falliti, 0 ignorati**, con PostgreSQL di test separato sulla porta 55432. Non sono stati modificati test o configurazioni. Rimane l'avviso NU1903 su `Microsoft.OpenApi` 2.0.0; aggiornare dipendenze non rientra in questa attività.
- `IssueStatusIntegrationTests` copre permessi ADMIN/USER assegnatario, rifiuto USER non assegnatario e READONLY, risoluzione con date e notifica persistite, stesso stato senza nuove scritture, 401 e 404. `NotificationsIntegrationTests` copre elenco personale ordinato, lettura delle proprie notifiche, rifiuto delle altrui anche per ADMIN, 401, 404 ed elenco vuoto. La riapertura, seconda risoluzione e ripetizione del PATCH read restano senza casi dedicati in questi file.
- `docker compose config`, build e avvio del backend .NET 10 verificati. `/api/health`, login admin, lista, archivio e dettaglio issue rispondono correttamente; lista senza token: 401. Provati anche login e lista dal frontend locale.
- Riavvio ordinato con `stop backend postgres` e `up -d --wait`: stesso volume `docker_bugboard26-postgres-data`, stessi conteggi e impronte delle righe prima/dopo (**2 utenti, 3 issue, 0 notifiche, 1 migrazione**). Ripetuti login e letture API. Nessun dato di sviluppo creato, cancellato o modificato per la prova; i database temporanei dei test sono distinti.

### Verifiche di questa revisione - 9 ottobre 2026

- Prova locale preliminare sul commit `15ca2e2`: build Release e test di dominio superati con coverage OpenCover **parziale**; report, formato, assembly ed esclusioni controllati. Dettagli storici in [qualita_backend.md](qualita_backend.md). Non e' un conteggio definitivo: non sono stati rilanciati i test durante la verifica dei colleghi.
- Suite PostgreSQL non eseguita: Docker Desktop non si avvia per il socket `dockerInference`. Non sono state ripetute le prove di avvio/persistenza del 2 ottobre, ne' eseguito il flusso immagini frontend/Docker.
- `IssueDuplicateValidationIntegrationTests` copre il rifiuto 400/409 e date coerenti nel percorso dedicato; `IssueAssigneeValidationIntegrationTests` copre account validi, null e assegnatari non validi. Sono descritti sulla lettura dei test, non dichiarati superati oggi.
- Analisi SonarQube non eseguita: manca server locale/scanner/token. Configurazione coverage verificata; integrazione scanner e Quality Gate ancora da provare.

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
- [ ] **Duplicati:** PATCH status `{"status":"Duplicate"}` da ADMIN o USER assegnatario: atteso 400. Come ADMIN usare il percorso duplicate con originale valido: 200, `resolvedAt: null`, nessuna nuova notifica. Autoreferenza/originale assente: 400; issue assente: 404; cambio stato successivo su duplicata: 409. USER e READONLY non possono usare il percorso duplicate (403).
- [ ] **Assegnatari:** creare con ID inesistente, inattivo o READONLY: 400, nessuna issue aggiunta. Con ADMIN/USER attivo: 201; con `assignedToId: null`: 201 e nessuna assegnazione.
- [ ] **RF-17, criterio aggiornato il 16 ottobre:** predisporre notifiche proprie per R; GET 200 con soli dati propri, PATCH read 403 senza scritture, anche su notifica già letta; UI senza comando e senza PATCH automatici. Registrare l’esito reale dopo l’allineamento backend/test e frontend; senza dati indicare "non verificato".
- [ ] **Immagini:** come S caricare una PNG/JPEG valida dal dettaglio, ricaricare e leggerla anche come R. U e R non possono caricare (403); file non valido o oltre 5 MiB: rifiuto senza sostituzione. Controllare persistenza dopo riavvio e ricreazione del solo backend Docker, senza eliminare volumi; il test di nuovo host non sostituisce questa prova Docker.
- [ ] **Export e suggerimento:** come R confrontare gli ID di lista e CSV con gli stessi filtri/ordinamento, senza archiviati. Verificare che il suggerito sia attivo e non READONLY, con carico Todo/InProgress non archiviato minimo e parita' risolta per ID. La richiesta non deve modificare dati.

### Strategia dei test presenti

`IssueDomainServiceTests` verifica tre metodi non banali a due parametri: `SearchIssues(issues, keyword)`, `CanChangeIssueStatus(user, issue)` e `SuggestAssignee(users, issues)`. Le Theory distinguono ricerca nel titolo/descrizione o assenza di risultati, ruolo/assegnazione/attivazione e carichi diversi; il Fact copre l'assenza di candidati. Sono classi di equivalenza, non una dimostrazione di copertura strutturale completa. I test HTTP con PostgreSQL verificano anche autorizzazioni, persistenza e assenza di scritture nei casi rifiutati; immagini includono sostituzione, concorrenza e nuovo host, CSV escaping/filtri/ordine, suggerimento parita' ed esclusioni. Test plan, casi aggiuntivi ed esiti definitivi vanno concordati con il collega che sta verificando i test.

### Verifica documentale

Confrontati requisiti, controller, `ApplicationDbContext`, `IssueDomainService`, modelli, migrazione e test con la documentazione. Gli esiti del 2 ottobre restano storici; la revisione attuale non li estende ai commit successivi. La checklist manuale completa **non è stata eseguita**. La compilazione LaTeX verifica i documenti, non l'applicazione.

Compilati entrambi i sorgenti con Tectonic 0.17.0 e aggiornati i PDF di consegna: **14 pagine per la specifica e 21 per il design**, copertina inclusa. Renderizzate tutte le pagine e controllate in dettaglio le viste modificate. Il diagramma dei casi d'uso e' stato rigenerato e controllato insieme al nuovo sorgente: export/suggerimento comuni, notifica legata alla risoluzione e nessun comando separato di chiusura duplicato. Gli **8 PNG strutturali e di sequenza** sono gia' stati rigenerati nella revisione precedente; sorgenti riconfrontati con il codice e viste incorporate nei PDF ricontrollate. Il cambio stato non e' stato modificato di nuovo. Restano avvisi Underfull e avvisi non bloccanti del compilatore portatile su fontconfig e ancora della copertina, senza Overfull nel risultato finale. Questi controlli non equivalgono alle prove applicative.

Per ricompilare, con Tectonic disponibile nel PATH, eseguire ciascun comando dalla cartella che contiene il relativo sorgente:

```powershell
tectonic --untrusted specifica_requisiti.tex
tectonic --untrusted documento_design.tex
```

I PDF di consegna mantengono i nomi `Primo homework/Specifica Requisiti.pdf` e `Secondo homework/Documento_Design.pdf`; dopo una ricompilazione sostituirli con gli output `specifica_requisiti.pdf` e `documento_design.pdf`. Controllare indice, UC-02, architettura, ruoli, notifiche e diagrammi. Per rigenerare le immagini, dalla cartella `Secondo homework`, con PlantUML 1.2026.8 e Java 21:

```powershell
java '-Djava.awt.headless=true' -jar "C:\percorso\plantuml.jar" -failfast2 -tpng diagrammi_architetturali/component_diagram.puml diagrammi_dettaglio/class_diagram.puml diagrammi_dettaglio/database_schema.puml diagrammi_comportamentali/sequence_creazione_issue.puml diagrammi_comportamentali/sequence_cambio_stato.puml diagrammi_comportamentali/sequence_issue_duplicata.puml
```

Per il diagramma dei casi d'uso, dalla cartella `Primo homework`:

```powershell
java '-Djava.awt.headless=true' -jar "C:\percorso\plantuml.jar" -failfast2 -tpng UCD/use_case_diagram.puml
```

Le viste strutturali usano Smetana integrato nel JAR: non richiedono Graphviz. Sostituzioni dei sorgenti storici:

| Sorgente PlantUML attuale | VPD storico sostituito | PNG generati |
| --- | --- | --- |
| `Primo homework/UCD/use_case_diagram.puml` | `use_case_diagram.vpd` | `use_case_diagram.png` |
| `diagrammi_architetturali/component_diagram.puml` | `component_diagram.vpd` | `component_diagram.png` |
| `diagrammi_dettaglio/class_diagram.puml` | `class_diagram.vpd` | `class_diagram.png`, `class_diagram_parte1.png`, `class_diagram_parte2.png` |
| `diagrammi_dettaglio/database_schema.puml` | `database_schema.vpd` | `database_schema.png` |
| `diagrammi_comportamentali/sequence_creazione_issue.puml` | `sequence_creazione_issue.vpd` | `sequence_creazione_issue.png` |
| `diagrammi_comportamentali/sequence_issue_duplicata.puml` | `sequence_issue_duplicata.vpd` | `sequence_issue_duplicata.png` |
| `diagrammi_comportamentali/sequence_cambio_stato.puml` | `sequence_cambio_stato.vpd` | `sequence_cambio_stato.png` |

Il cambio stato conserva il flusso precedente, aggiungendo soltanto le guardie 400/409 e correggendo la nota sul merge. Il deployment originale non e' stato rigenerato: resta la vista logica gia' annotata nel design. Nessun `.vpd` viene dichiarato aggiornato.

### Punti aperti per la consegna

- [ ] Attuare e verificare la decisione del gruppo RF-17: backend e test ad amico 1; frontend da allineare e verificare. Il criterio è deciso, gli esiti tecnici mancano.
- [ ] Acquisire il collaudo frontend/test dei colleghi su un checkout stabile, ripetere la suite con coverage completa e registrare commit/data degli esiti. Collaudare immagini e persistenza dei due volumi senza cancellare dati.
- [ ] Produrre il report backend SonarQube effettivo: server locale, progetto esistente, scanner e token non sono disponibili. Configurazione pronta, analisi/importazione coverage/Quality Gate ancora da verificare.
- [x] Documento unico predisposto da Valerio e integrato il 16 ottobre, con capitoli distinti, test plan e strategia. Restano da consolidare gli esiti necessari: la documentazione non è dichiarata conclusa.
- [ ] Confermare l'uso dei sorgenti PlantUML rispetto al requisito CASE del paragrafo 3.2; se richiesto, sincronizzare le viste aggiornate in Visual Paradigm. I VPD storici non sono stati aggiornati e non rappresentano il modello attuale.

Questi sono passaggi da completare, non attivita' dichiarate svolte da questa revisione.
