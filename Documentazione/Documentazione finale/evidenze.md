# Registro delle evidenze e attività aperte

Revisione documentale 16 ottobre 2026. Riferimento locale letto da HEAD: `e13995140dff382dc3d548208c6c1e60be91fe5d`. Integrata la versione aggiornata del documento unico di Valerio, senza sostituirla con un altro documento. Nessuna nuova esecuzione dei test applicativi in questa attività. Un test presente nel sorgente non è automaticamente un test superato.

| ID | Materiale / provenienza | Stato e limite | Azione necessaria |
| --- | --- | --- | --- |
| E01 | Documento unico già preparato da Valerio, due homework e diagrammi esistenti | Il documento di Valerio è la base conservata; mock-up storici, non screenshot attuali. | Rigenerare e controllare dopo ogni modifica. |
| E02 | Nota del 2 ottobre nel readme storico | Risultati riferiti a una revisione precedente, non certificati nuovamente. | Non estendere i conteggi al checkout finale. |
| E03 | `qualita_backend.md`, prova preliminare 9 ottobre su `15ca2e2` | Dichiarati build Release e test di dominio con coverage parziale; nessun Sonar/Quality Gate. Evidenza documentale storica, non nuovo report di esecuzione. | Ottenere TRX/OpenCover completi sul checkout stabile. |
| E04 | `IssueDomainServiceTests.cs` e test di integrazione letti | Strategia e oracoli descritti staticamente. | Collega test: esecuzione, comandi, ambiente, report e anomalie. |
| E05 | Test plan TP-UC02 | Preparato, tutti gli esiti iniziali `DA_ESEGUIRE`. | Compilare `esiti_test_plan.csv` con riscontri reali. |
| E06 | Collaudo frontend reale di Valerio del 16 ottobre, riepilogo nel capitolo 3 del documento unico | Risultato effettivo confermato dal gruppo: conservato e attribuito a Valerio. Include build e test frontend, UI/API reali, immagini, export e ruoli; non è una nuova esecuzione dell'autore documentale. Report e screenshot permanenti non ancora archiviati. | Valerio: salvare evidenze condivisibili. Amico 1: persistenza dopo ricreazione dei container, non provata da questo collaudo. |
| E07 | Qualità backend | Esiti attesi da amico 1 in `verifica-backend/` alla radice; cartella e risultati non ancora presenti al controllo. | Acquisire suite backend completa, coverage, persistenza e report SonarQube effettivo con configurazione, problemi, import e gate se applicabile. Leggere risultati e limiti prima di integrarli. |
| E08 | Review sistematica di usabilità | Checklist pronta, review non eseguita. | Gruppo: registrare osservazioni in `evidenze/review_tecnica.csv`. |
| E09 | Prove con utenti e survey | Protocollo pronto, nessuna sessione o risposta ricevuta. | Gruppo: persone reali/potenziali, osservazioni anonime e risposte reali. |
| E10 | Diagrammi CASE | Sorgenti PlantUML e PNG verificabili; VPD storici invariati, non sincronizzati. | Chiarimento docenti o aggiornamento nel CASE richiesto. |
| E11 | RF-17: decisione del gruppo, 16 ottobre 2026 | Decisione presa: READONLY consulta le proprie notifiche, ma non le segna lette. Non è un chiarimento dei docenti. Attuazione e verifica aperte. | Amico 1: allineare backend e test; frontend da allineare e verificare. TP-14: GET proprie 200, PATCH read 403 senza scritture e nessun comando UI. Acquisire esiti reali dopo l’allineamento; il precedente collaudo non li sostituisce. |
| E12 | Presentazione e demo | Scaletta con budget inferiore a 10 minuti; demo successiva separata. | Gruppo: prova cronometrata e coordinamento reale con collega frontend. |

## Contributi da fornire

Amico 1 depositerà i risultati in `verifica-backend/` nella radice del repository. Valerio preparerà il report permanente frontend; percorso non ancora comunicato. Non creare duplicati di questi report: richiamarli dal documento dopo averli controllati. La gestione del documento unico e della presentazione rimane a questa attività documentale. Per i dati di usabilità usare i modelli già presenti in `evidenze/`. Conservare solo materiale condivisibile, senza token o dati personali non necessari. Dati minimi: ID evidenza, data, checkout e modifiche locali rilevanti, ambiente/versioni, comando o procedura, risultato effettivo, percorso del report e limiti. Per risultati negativi conservare anche il problema: non rinominare un fallimento come attività conclusa.

Gli output dell'analisi qualità già predisposta restano nelle directory temporanee indicate dal relativo script. Il gruppo seleziona i report da archiviare; questa attività non esegue lo script e non pubblica analisi.

Quando arriva materiale: verificare il riferimento del codice, leggere l'evidenza, registrarla qui, aggiornare il capitolo testing di `documentazione_finale.tex`, i suoi frammenti e le slide, rigenerare e ricontrollare. Non chiudere E05-E11 sulla sola presenza di un file vuoto o di una descrizione del risultato atteso.

## Domande predisposte, non inviate

1. **CASE, §3.2:** «Sono accettati sorgenti UML PlantUML versionati con export coerenti e tracciabili, oppure occorre un modello nativo sincronizzato in un tool CASE come Visual Paradigm? In quest'ultimo caso quali formati di progetto/export devono essere consegnati?»
**RF-17 non è più una domanda ai docenti:** il gruppo ha deciso il comportamento sopra. Rimangono aperte l’attuazione e la verifica tecnica, da documentare con esiti effettivi.

La documentazione rimane **aperta** finché mancano risultati necessari alla consegna.
