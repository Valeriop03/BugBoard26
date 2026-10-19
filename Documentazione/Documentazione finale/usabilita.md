# Usabilità: protocollo e raccolta reale

Stato: review sistematica assistita completata sugli otto criteri sotto, con [report ed evidenze](verifica-frontend/review-20261019/report.md) e matrice CSV. Ispezionati interfaccia e screenshot, tastiera, cinque campioni di contrasto, condizioni di errore e recupero upload; 28 controlli browser finali superati. Corretti i colori insufficienti; nessuna certificazione WCAG o revisione umana indipendente dichiarata. Le tre sessioni riferite dall'utente del 19/10/2026 restano distinte: P01/P03 U2 e P02 U4, tutti USER su PC condiviso, esecuzione autonoma dopo introduzione e 15 voti Q1-Q5. Dettagli in [sessioni e risposte](evidenze/risposte-parziali-usabilita.md), senza attribuire attivita' o risposte aperte non comunicate. La [review precedente](verifica-backend/review-tecnica-20261019.csv) resta storica: i suoi rilievi e le voci non verificate non sono lo stato dell'ultima review.

## Review tecnica con checklist

La review corrente confronta ogni punto col prodotto avviato, i sorgenti e le schermate osservate. La matrice registra `conforme`, `non conforme` o `non verificato`, evidenza e conseguenza; l'esito e' circoscritto ai percorsi provati. Non sono inventati un secondo revisore o partecipanti aggiuntivi. La priorita' dipende dall'impedimento concreto, non da un punteggio arbitrario.

1. Ruolo corrente e operazioni consentite sono riconoscibili; i comandi riservati non inducono in errore.
2. Login e form spiegano errori e come correggerli; campi obbligatori e limiti sono leggibili.
3. Caricamento, successo, lista vuota ed errore di rete hanno feedback distinguibili.
4. Ricerca, filtri e ordinamento sono comprensibili e permettono di ritrovare la issue.
5. Stato, assegnatario e significato di duplicato/archivio risultano chiari.
6. La notifica rende riconoscibile la risoluzione e lo stato letto/non letto. Secondo la decisione del gruppo RF-17, READONLY consulta solo le proprie notifiche e non dispone del comando per segnarle lette. Allineamento e verifica tecnica acquisiti il 19 ottobre; resta da osservare la comprensione da parte delle persone.
7. Contrasto, dimensione del testo, etichette e percorso da tastiera permettono le attività previste.
8. Un errore di upload non fa credere che la issue già salvata sia stata annullata.

Usare [review_tecnica.csv](evidenze/review_tecnica.csv). La checklist non certifica conformità WCAG né prestazioni misurate.

## Preparazione delle sessioni

Il gruppo deve coinvolgere persone volontarie che possano usare un issue tracker: studenti con esperienza di progetto software, sviluppatori o stakeholder potenziali. Registrare solo codice partecipante e profilo generico, esperienza indicativa e ruolo provato. Non raccogliere nomi, e-mail, matricole, credenziali o token. Il gruppo prepara account dimostrativi e dati sintetici; non impersona i partecipanti.

Annotare data, versione/checkout, browser, dimensione indicativa dello schermo e ambiente. Richiedere il consenso alla partecipazione e all'annotazione anonima. Non servono registrazioni audio/video. Spiegare che si valuta l'interfaccia e che il partecipante può interrompere.

Il moderatore legge l'obiettivo, osserva e annota senza indicare subito pulsanti o percorsi. Se deve aiutare, registra l'aiuto. Non contare un'attività svolta dal moderatore come completata in autonomia dal partecipante. Non creare utenti o dati in produzione.

## Attività realistiche

| ID | Profilo e situazione | Consegna da leggere al partecipante | Verifica dell'osservatore |
| --- | --- | --- | --- |
| U1 | Tutti, già autenticati | «Devi capire se il problema di accesso è già stato segnalato. Trova la segnalazione pertinente e dimmi stato e assegnatario.» | Issue predisposta rintracciata, interpretazione corretta, ricerca/filtri e dubbi osservati. |
| U2 | USER | «Il login rifiuta una password valida. Inserisci una segnalazione che permetta al team di riprodurre il problema. Puoi aggiungere questa immagine di prova.» | Campi validi e riscontro finale. Il suggerimento sceglie l'assegnatario nel form attuale: non chiedere una selezione manuale assente. Annotare upload/errore eventuale. |
| U3 | USER assegnatario o ADMIN | «Hai verificato la correzione della issue che ti è stata affidata. Aggiorna la segnalazione in modo che il collega che l'ha aperta sappia che è risolta.» | Ruolo/account predisposto corretto, aggiornamento a Resolved. Nessuna istruzione sul pulsante da premere. |
| U4 | USER segnalatore | «Un collega dice di aver risolto il problema che avevi segnalato. Trova il riscontro nel sistema e indica che lo hai letto.» | Il moderatore predispone la risoluzione con un account distinto prima dell'attività; osservare ricerca della notifica e comprensione del feedback. |
| U5 | READONLY | «Sei uno stakeholder. Trova le issue ad alta priorità e prepara un elenco condivisibile senza modificarle.» | Consultazione, filtro ed export CSV. Verificare che il partecipante comprenda le limitazioni del ruolo. |

Proporre soltanto le attività coerenti col profilo. Per U3/U4 usare account distinti e descrivere chi ha eseguito il prerequisito. Non servono tutti i ruoli per ogni partecipante. Registrare tempi solo se realmente rilevati con lo stesso criterio, dall'inizio della consegna al completamento/rinuncia. Distinguere `completata autonomamente`, `con aiuto`, `non completata` e `non proposta`. Una mancata esecuzione non vale zero secondi.

## Questionario breve dopo le attività

Scala per Q1-Q5: **1 per nulla, 2 poco, 3 moderatamente, 4 abbastanza, 5 molto**; consentire `non applicabile` e non trattarlo come un numero. Le dimensioni mantengono quelle del documento unificato di Valerio. Questionario locale breve, **non SUS** e non strumento validato.

1. Quanto era chiara la navigazione fra le schermate?
2. Quanto erano comprensibili i campi e le informazioni richieste?
3. Quanto è stato facile trovare la issue cercata?
4. Quanto era chiaro l'esito delle operazioni e degli eventuali errori?
5. Quanto erano chiare le operazioni consentite al tuo ruolo?
6. Quale passaggio è stato meno chiaro? Descrivi ciò che ti aspettavi.
7. Facoltativo: quale modifica aiuterebbe maggiormente a svolgere queste attività?

Usare [questionario.csv](evidenze/questionario.csv) e [sessioni_utenti.csv](evidenze/sessioni_utenti.csv). Contengono i punteggi e i tre esiti riferiti dall'utente. I campi non forniti restano vuoti; non aggiungere righe dimostrative che possano sembrare risultati e non sostituire le risposte mancanti con valori inventati.

## Integrazione Delle Evidenze

Review e sessioni hanno fonti differenti: la prima e' assistita da controlli browser e ispezione; le seconde sono riferite dal moderatore. Sono conservati codici, attivita', esiti e punteggi realmente comunicati, senza dati personali non necessari. Nessun messaggio esterno o consegna e' stato effettuato.

Il resoconto distingue osservazione, interpretazione e proposta. Il campione e' limitato a tre persone e non viene generalizzato con percentuali. I fix della review riguardano soltanto i colori: nessuna nuova funzione o regola backend. I campi non comunicati restano vuoti; le slide saranno allineate prima della prova finale.
