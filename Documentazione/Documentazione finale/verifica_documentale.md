# Verifica documentale - 16 ottobre 2026

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
