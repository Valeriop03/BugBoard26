# Verifica documentale

## Review Sistematica - Versione 1.5

Integrata la review assistita degli otto criteri, con matrice compilata in
`evidenze/review_tecnica.csv` e report/evidenze in
`verifica-frontend/review-20261019/`. Distinta dalle tre sessioni riferite dal
moderatore: nessun nuovo partecipante, risposta o secondo revisore inventato.
Correzioni limitate ai colori di cinque pagine; 28 controlli browser finali,
40 test frontend e build riusciti. Nessun nuovo test permanente e nessuna
nuova esecuzione della suite backend dichiarata per queste modifiche CSS.

PDF versione 1.5 ricompilato con Tectonic: 33 pagine, nessun Overfull o
riferimento indefinito e un Underfull non bloccante. Rendering di tutte le
pagine a 105 DPI e controllo visuale, con dettaglio della sezione review;
nessun carattere fuori pagina. Il messaggio Fontconfig resta non bloccante.
Slide non rigenerate; pulizia editoriale generale e verifica dei modelli
CASE restano distinte da questa review.

## Integrazione Survey - Versione 1.4

Tre sessioni del 19/10/2026 registrate secondo il resoconto del moderatore:
P01/U2, P02/U4, P03/U2, tutti USER sul suo PC in condivisione, con
completamento autonomo dopo spiegazione iniziale. Riportati i quindici
punteggi Q1-Q5; non inventate risposte aperte, tempi o osservazioni.
CSV, riepilogo, registro e documento unico sono aggiornati. Le limitazioni
del campione e i campi mancanti restano espliciti. Il checkout effettivo
delle sessioni non e' desunto automaticamente dall'HEAD di registrazione.

PDF ricompilato con Tectonic 0.17.0, 33 pagine fisiche: zero Overfull,
zero riferimenti indefiniti e un Underfull non bloccante. Tutte le pagine
renderizzate a 105 DPI e controllate; nessun carattere fuori pagina.
Letta a grandezza maggiore la nuova tabella con i risultati e i limiti.
La compilazione emette lo stesso messaggio Fontconfig gia' documentato,
senza impedire l'export o il rendering. Non rigenerate le slide, da
allineare alla raccolta prima della prova finale richiesta dal gruppo.

Il documento riporta anche i risultati successivi al fix CSV: 40 test
frontend, 166 backend e apertura CSV in Excel confermata dall'utente.
Non sono nuove esecuzioni durante questa sola integrazione documentale.
Le verifiche dell'audit 1.3 riportate sotto restano riferite ai suoi artefatti.

## Revisione Effettiva Del 19 Ottobre 2026

Documento unico versione 1.3: HEAD `6aa175a` con correzioni locali non
committate, non solo la revisione Git. [Audit corrente](verifica-backend/audit-20261019.md)
con TRX, coverage, Sonar e collaudo browser conservati accanto al documento.
I risultati storici del collega sono preservati e attribuiti; nessun esito
di test o di utenti e dedotto dalla sola presenza dei sorgenti.

- Tectonic 0.17.0 con `--untrusted --keep-logs`: 32 pagine fisiche,
  0 Overfull, 0 riferimenti indefiniti, un Underfull non bloccante.
  Il messaggio Fontconfig non ha impedito l'export; font e rendering controllati.
- Tutte le pagine renderizzate con Poppler a 105 DPI e controllate,
  con dettaglio delle tabelle e sezioni aggiornate. Nessun carattere
  fuori pagina rilevato dal controllo geometrico.
- Presentazione corrente `presentazione/presentazione_tecnica_20261019.pptx`:
  9 slide, integrita e geometria senza rilievi, Arial e due tabelle native.
  Reimportazione ed esame di tutte le nove slide renderizzate riusciti.
  Nessuna verifica in Microsoft PowerPoint dichiarata.
- `rigenera.ps1 -SoloDocumento` eseguito con successo; presentazione
  generata separatamente tramite il suo sorgente e validatore. Il runtime
  installato viene ora individuato senza un percorso di versione obsoleto.
- `prepara_documentazione.py`: 12 viste coerenti con metadati PNG e SHA256.
  VPD non modificati; conformita CASE ancora da risolvere.
- Controllati 63 collegamenti locali nei README, Markdown finali e report
  backend: nessuna destinazione mancante. Non e una verifica di URL remoti.
- Test plan: 14 righe verificate contro nomi Passed del TRX di 166 test.
  Frontend 36 test e build passati; browser reale 20 controlli. Report e
  documento distinguono unita, integrazione, browser e prove umane.
- Budget slide 540 secondi pianificati; nessuna prova cronometrata eseguita.

Evidenze tecniche e impronte in `verifica-backend/evidenze/audit-20261019/`.
Restano review umana completa, sessioni/questionari reali, decisione CASE,
rehearsal e pubblicazione/consegna manuali. A differenza della revisione
storica sottostante, questo audit include fix applicativi e relativi test.
Nessun commit, branch, push, tag, release o invio effettuato.

## Storico Del 16 Ottobre 2026

I paragrafi seguenti descrivono la revisione 1.2 di allora: non sono
affermazioni sullo stato corrente o verifiche ancora tecnicamente mancanti.

Questa verifica riguarda documenti e slide. Non certifica l'esecuzione dell'applicazione o la conclusione della consegna.

Revisione corrente: **1.2, decisione RF-17 del gruppo**. READONLY può consultare le proprie notifiche ma non segnarle lette; backend e test sono affidati ad amico 1, frontend da allineare e verificare. La decisione non viene attribuita ai docenti. Sono aggiornati test plan, registro, checklist, legenda UML e slide esistenti; non sono aggiunti altri documenti di consegna.

## Base e integrazione

È stata riletta la versione del documento unico di Valerio prima di modificarla. Impronta della base acquisita: `af49cf68987aad35f93dcf8ef2378aec10d26cb89a77e4d112dd46e3262390c0` (SHA-256). Il riepilogo originale della sezione «Collaudo frontend reale -- 16 ottobre 2026» è stato confrontato e conservato integralmente; è stata aggiunta l'attribuzione della fonte. Nessun secondo documento finale viene consegnato.

Per l'aggiornamento RF-17 è stata acquisita nuovamente la versione locale, SHA-256 `35a8f22c128ca57800f9398828c49dc4d2a064bc4fde383928c655860d62f7f5`. Confrontate le impronte dei file di destinazione prima dell'integrazione, per evitare di sovrascrivere modifiche concorrenti. Il riepilogo frontend è rimasto integralmente invariato anche in questa revisione; non verifica l'allineamento alla nuova decisione.

Restano i casi TP-01--TP-10 e i capitoli già predisposti. Le integrazioni aggiungono limiti delle classi testate, ambiente/criteri del test plan, casi TP-11--TP-14, provenienza delle evidenze, protocollo di usabilità e domanda CASE. È stata corretta la descrizione del test `API`: la fixture contiene esattamente le stesse maiuscole, quindi quel caso non prova da solo il confronto case-insensitive.

## Compilazione e leggibilità

- Documento compilato con **Tectonic 0.17.0**, opzione `--untrusted`: **32 pagine fisiche**, copertina e indice inclusi. Zero avvisi Overfull e zero riferimenti indefiniti nel log finale. Rimangono avvisi Underfull di spaziatura, senza errori bloccanti.
- Tutte le pagine finali renderizzate con Poppler a 105 DPI e controllate per impaginazione, con lettura di dettaglio delle viste e delle tabelle modificate. Nessun testo fuori pagina rilevato dal controllo dei bounding box. Il deployment locale è stato disposto su una pagina separata per migliorarne la leggibilità.
- Presentazione: **9 slide**, budget pianificato **540 secondi**. Il budget è una previsione, non una prova cronometrata.
- PPTX verificato per integrità del pacchetto, dimensioni 16:9, geometria, font dichiarati Arial e presenza delle due tabelle native. Reimportazione nel runtime Artifact Tool riuscita. Nessuna anomalia riportata dai validatori finali di layout.
- Tutte le slide renderizzate dal PPTX reimportato; confronto delle slide 5 e 8 modificate con le originali e scansione del mazzo finale. Le altre viste mantengono contenuti e disposizione precedenti. Non è stata dichiarata una verifica in Microsoft PowerPoint: non è stato aperto lì.
- Lo script `rigenera.ps1` è stato eseguito con successo per documento e presentazione, usando il compilatore portatile disponibile sul PC. La procedura non esegue build o test applicativi.

## Diagrammi e sorgenti

`prepara_documentazione.py` ha verificato **12 viste** confrontando ciascun blocco `@startuml` con il sorgente incorporato nel relativo PNG. Il manifest registra percorsi e impronte. Le due immagini incorporate nel PPTX coincidono con i PNG finali delle viste sintetiche. Queste ultime omettono dettagli e molteplicità, rinviando al modello completo del documento.

I VPD e il deployment storico non sono modificati. Non è stato verificato un modello Visual Paradigm aggiornato. L'accettazione di PlantUML rispetto al vincolo CASE è ancora aperta e la domanda predisposta non è stata inviata.

Aggiornata solo la legenda RF-17 del sorgente dei casi d'uso e rigenerato il suo PNG, richiamato dal documento unico. I PDF precedenti dei homework restano storici. Il manifest delle 12 viste è stato rigenerato e controllato; la coerenza degli export non chiude il requisito CASE.

## Perimetro e risultati ancora necessari

Il confronto delle impronte dei **125 file applicativi/configurativi esaminati** non mostra modifiche rispetto all'inizio del lavoro. Sono controllati i sorgenti e le configurazioni di frontend, backend, Docker, quality e radice; sono esclusi dipendenze installate, bin/obj e altri output generati. Le modifiche di progetto di questa attività riguardano soltanto `Documentazione`.

Non sono stati eseguiti nuovi test applicativi. Il risultato frontend è quello effettivo riportato da Valerio e confermato dal gruppo; Valerio preparerà il report permanente. Da amico 1 si attendono in `verifica-backend/`, alla radice, suite backend completa, coverage, verifica dei volumi dopo ricreazione e analisi SonarQube: cartella e risultati non ancora disponibili al controllo. Review sistematica, prove con utenti, questionari, conformità CASE e prova congiunta della demo restano aperti. RF-17 è deciso dal gruppo; restano aperti attuazione e verifica in backend, test e frontend. I 14 casi del test plan conservano stato `DA_ESEGUIRE` e risultati effettivi vuoti; nessun esito viene dedotto dal codice.

Non sono stati effettuati commit, creazione di branch, push, tag, release, invio di messaggi o consegne. La documentazione **non è dichiarata conclusa**.
