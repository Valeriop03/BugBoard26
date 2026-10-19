# Review sistematica dell'interfaccia - 19 ottobre 2026

## Metodo E Perimetro

Review tecnica assistita degli otto criteri gia' definiti in
[usabilita.md](../../usabilita.md), con ispezione dei sorgenti, osservazione
degli screenshot e interazioni browser. La matrice dei risultati e'
[review_tecnica.csv](../../evidenze/review_tecnica.csv).
Non viene attribuita questa review a un componente del gruppo o a un
partecipante che non l'ha svolta. Le tre sessioni con amici riferite dal
moderatore restano evidenze distinte, non nuove sessioni di questa review.

Riferimento: HEAD `06ae1b090637ad5ee2f8debea3c5d8cb55cdd036` con modifiche
locali, inclusi fix CSV e colori. Metadata e SHA256 identificano il codice.
Nessun commit, branch, push, tag o invio eseguito. Nessun nuovo test
permanente o dipendenza aggiunta; backend e regole applicative invariati.

Chrome headless 155, Angular reale su 4200 e API Docker reale su 5257,
PostgreSQL dedicato `bugboard26_test_review_9572cbe9aaac` e volume immagini
separato. Nel browser soltanto, l'URL API configurato per 5256 e' inoltrato
all'API di review. Le normali risposte HTTP provengono dal backend reale;
ritardo e interruzione della rete sono iniettati e dichiarati nei casi
dedicati. Il backend di sviluppo 5256 non e' stato fermato e i suoi dati
non sono stati modificati. Il container review e' stato arrestato al termine,
senza eliminare volumi. I file env e le credenziali restano fuori repository.

## Risultati

Tutti gli otto criteri hanno un esito registrato nel perimetro provato.
L'esito conforme non significa copertura di ogni browser o condizione.
Il giro finale ha **28 controlli browser PASS**, nessuna eccezione JavaScript
non gestita. Le misure di contrasto sono nei JSON, non dedotte dal colore
apparente delle immagini. Il primo giro aveva evidenziato quattro campioni
insufficienti: il suo completamento funzionale non equivaleva a conformita'
del contrasto. Il problema e' corretto e il giro finale e' ripetuto.

| Criterio | Riscontro |
| --- | --- |
| Ruoli e comandi | Login e menu per ADMIN/USER/READONLY; guard e rifiuti API coerenti. |
| Login e form | Errore credenziali, obbligatorieta' e formato allegato; messaggi e aria-invalid presenti. |
| Feedback | Caricamento, lista vuota e errore di rete distinguibili; ripristino tramite Pulisci. |
| Ricerca ed export | Keyword, filtro BUG e CSV coerenti con i risultati; combinazioni complete coperte dalla suite API separata. |
| Stato e archivio | Informazioni nel dettaglio, riferimento duplicato e consultazione archivio verificati. |
| Notifiche | Lettura USER/ADMIN e consultazione READONLY; 403 e nessuna scrittura impropria nei casi provati. |
| Tastiera e leggibilita' | Tab/Shift+Tab/Enter, focus visibile; cinque campioni di contrasto e quattro viste a 320/390 pixel. |
| Recupero upload | Firma PNG falsa rifiutata dall'API; issue gia' salvata riconoscibile e upload ripetuto sullo stesso ID, con una sola creazione. |

La fixture della notifica READONLY e' inserita con SQL nel solo database di
test, non creata da una risoluzione simulata: non esiste un endpoint per
inserire notifiche arbitrarie. Le altre operazioni usano API/UI reali.
Il falso PNG supera il controllo preliminare estensione/MIME ma viene
rifiutato dalla verifica della firma del backend: e' un errore HTTP reale,
non una risposta 400 inventata dal browser.

## Correzioni Puntuali

Corretto il contrasto del testo piccolo senza cambiare layout, ruoli o
funzionalita'. Nella lista e nell'archivio sono scuriti intestazioni e badge;
scurite anche note del form/dettaglio, nota export e testo errore login.

| Campione lista | Prima | Dopo | Soglia applicata |
| --- | --- | --- | --- |
| Intestazioni | 4,478 | 4,973 | 4,5 |
| Priorita' | 3,030 | 5,245 | 4,5 |
| Stato | 3,438 | 5,630 | 4,5 |
| Nota export | 2,814 | 4,973 | 4,5 |
| Nuova Issue | 4,702 | 4,702 | 4,5 |

Il confronto usa i valori non arrotondati di luminanza sRGB estratti dai
colori computed del browser; in tabella sono abbreviati. La soglia del
testo normale segue [W3C, contrasto minimo](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html).
Come riferimento supplementare sono state lette le
[Web Interface Guidelines](https://github.com/vercel-labs/web-interface-guidelines/blob/main/command.md),
non introdotte come nuovi requisiti dell'esame.

File modificati: CSS di issues-page, archived-issues-page, issue-form-page,
issue-detail-page e login-page. Non aggiunti nuovi test permanenti.
**40/40 test frontend esistenti e build produzione riusciti**; bundle
iniziale 380,34 kB. I 166 test backend restano l'esito dell'ultima suite
gia' eseguita: non e' dichiarata una nuova esecuzione backend per questi CSS.

## Limiti E Miglioramenti Non Bloccanti

Il controllo e' circoscritto ai percorsi e ai cinque campioni elencati:
non e' una certificazione WCAG, una prova con lettore schermo o una verifica
di tutti i temi, browser e dispositivi. La rete e' perturbata nel browser,
non mediante guasti reali al database. Sul mobile le righe sono impilate;
etichette per ogni riga renderebbero la lettura piu' immediata. Gli stati
restano in inglese come nell'interfaccia esistente.

Dal sorgente restano opportunita' di miglioramento: focus sul primo campo
errato dopo invio non valido, annunci live delle variazioni asincrone e
conferma esplicita delle azioni admin. I controlli sono raggiungibili da
tastiera e i messaggi visuali sono presenti, ma questi aspetti non vengono
dichiarati risolti o testati con tecnologie assistive. Non si aggiungono
nuove funzioni per chiudere artificialmente ogni raccomandazione esterna.

Due tentativi intermedi sono conservati: uno script presumeva che il
suggerimento non scegliesse il nuovo USER, ma la scelta era legittima con
i carichi modificati dalle prove. Usata poi una fixture con assegnatario
ADMIN esplicito. Un altro controllo contava gli errori prima del rendering:
aggiunta l'attesa del messaggio. Non sono bug applicativi mascherati.
Le conclusioni si riferiscono all'ultima esecuzione completa.

## Ripetizione E Evidenze

Avviare il progetto come nel README. Provare login errato e corretto,
campi vuoti, ricerca senza risultati, filtri/export, permessi per ruolo e
archivio. Percorrere login, ricerca e creazione con Tab/Shift+Tab/Enter;
controllare il focus. Con dati dimostrativi, provare un file .png con firma
errata, poi un PNG valido usando Riprova caricamento: l'ID deve restare lo
stesso. Non modificare issue preesistenti. Per guasti di rete, lavorare in
un ambiente di prova e registrare esplicitamente la simulazione.

Da frontend, con Chrome disponibile:

```powershell
$env:CHROME_BIN = 'C:\Program Files\Google\Chrome\Application\chrome.exe'
node node_modules/@angular/cli/bin/ng.js test --watch=false --browsers=ChromeHeadless
npm run build
```

Risultati in `frontend-e2e-results.json`, `contrasti.json`, `mobile.json`,
`tastiera.json`, log e screenshot adiacenti. `prima/` conserva il baseline.
Gli script temporanei di review non costituiscono un nuovo framework E2E
versionato. Il manifest SHA256 identifica le evidenze archiviate.
