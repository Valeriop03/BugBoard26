# Consegna e presentazione tecnica

Stato: materiale documentale predisposto, **consegna non conclusa**. Non creare release né inviare nulla finché le evidenze necessarie sono aperte.

## Scaletta della presentazione

Budget previsto: **9 minuti**, da confermare con una prova cronometrata reale. La demo breve segue la presentazione, come prevede il §10; non è compresa nei nove minuti. Il gruppo deve confermare chi espone e il passaggio al collega frontend. Tutti devono conoscere l'intero progetto.

| Slide | Argomento tecnico | Budget |
| --- | --- | --- |
| 1 | Contesto e perimetro assegnato | 20 s |
| 2 | Separazione SPA/API e persistenza | 75 s |
| 3 | Modello dati e vincoli | 75 s |
| 4 | Astrazioni OO e dipendenze reali | 60 s |
| 5 | Permessi e controlli sul backend | 75 s |
| 6 | Risoluzione, notifica e idempotenza | 60 s |
| 7 | Strategia dei tre metodi e test plan | 80 s |
| 8 | Evidenze qualità, usabilità e punti aperti | 60 s |
| 9 | Passaggio alla demo | 35 s |

Nelle note delle slide sono riportati riferimenti ai sorgenti e argomenti da spiegare. Evitare numeri di test o qualità non consolidati. Le immagini UML conservano i sorgenti modificabili nei file `.puml` accanto alla documentazione; il PowerPoint non sostituisce il modello CASE.

## Demo breve da concordare con il collega frontend

Proposta di circa 2-3 minuti, non ancora provata insieme. Preparare prima un database dimostrativo, account S e A distinti e READONLY, una issue assegnata e i filtri necessari. Confermare host, browser, credenziali dimostrative e chi controlla il mouse. Non mostrare token, password o dati reali.

1. Collega frontend: mostra come READONLY lista, filtro e disponibilità dell'export, con assenza dei comandi di modifica.
2. Collega frontend: passa all'account A e risolve una issue già assegnata. Relatore: spiega controllo server, date e persistenza della notifica.
3. Collega frontend: passa a S, ricarica le notifiche e segna letta quella ricevuta. Relatore: collega il comportamento a UC-02 e al test plan.

L'immagine opzionale si mostra solo se il collaudo reale del collega è disponibile e il tempo lo permette. Non fingere che la pagina riceva notifiche realtime. Se il sistema non parte, dichiarare il problema e usare i diagrammi per spiegare il flusso: un video o una schermata storica non va presentata come esecuzione live. Stabilire chi risponde su modello, permessi, API e test, senza limitare la conoscenza alle parti assegnate.

## Coordinamento delle evidenze

Al 19 ottobre i risultati effettivi sono raccolti in `verifica-backend/` accanto a questo documento: suite completa 166/166, SonarQube con Quality Gate OK e persistenza Docker verificata. L'audit e i report precedenti distinguono le rispettive esecuzioni e le modifiche locali dal commit iniziale. I test frontend correnti sono 36/36. RF-17 e verificato: READONLY consulta le proprie notifiche, ma PATCH read restituisce 403 senza scritture; il frontend non offre quel comando. Nessun chiarimento del docente viene attribuito alla decisione del gruppo. Vedere [audit corrente](verifica-backend/audit-20261019.md) e [registro evidenze](evidenze.md).

## Checklist finale di consegna

- [ ] Consolidare i report test completi e il test plan sullo stesso riferimento stabile del codice.
- [x] Integrare report effettivo di qualita backend e limiti: analisi eseguita, non solo predisposta.
- [x] Registrare tre sessioni USER del 19/10 e quindici risposte numeriche, secondo il resoconto del moderatore e con limiti dichiarati.
- [x] Completare la review sistematica assistita sugli otto criteri; registrati esiti e limiti in verifica-frontend/review-20261019/report.md. I commenti qualitativi del survey non forniti restano tali.
- [ ] Risolvere la conformità CASE o conservare una risposta esplicita dei docenti; non presentare i VPD storici come aggiornati.
- [x] Registrare la decisione del gruppo RF-17: consultazione delle proprie notifiche consentita a READONLY, modifica del flag di lettura vietata.
- [x] Verificare RF-17 in backend e frontend: TP-14, rifiuto 403 senza scritture e assenza di comando UI.
- [x] Verificare documento unico, indice, diagrammi, sorgenti/immagini e slide dopo la rigenerazione del 19 ottobre; rifare i controlli dopo eventuali aggiornamenti.
- [ ] Confermare l'assegnazione del gruppo e il materiale sorgente/build/eseguibile richiesto dalla traccia. Conservare Dockerfile e istruzioni esistenti.
- [ ] Fare la prova tecnica cronometrata entro 10 minuti e concordare la demo con il collega frontend.
- [ ] Solo quando pronto, pubblicare sul repository il materiale definitivo e creare una release GitHub riferita alla revisione consegnata. Questa attività non lo fa.
- [ ] Preparare link repository, **link diretto alla release** e link ad eventuale altro materiale. Per materiale molto pesante, la traccia richiede Unina Gigamail.
- [ ] **Ogni componente invia individualmente** la consegna tramite l'attività Teams dell'insegnamento, anche se i link sono identici.
- [ ] Verificare scadenza e prenotazione della seduta sui canali ufficiali. La traccia tradizionale indica il 31 marzo 2027 come termine generale; valgono anche le scadenze dell'appello comunicate dai docenti.
- [ ] Alla consegna definitiva valutare la notifica ai docenti e la prenotazione della presentazione previste dal §9.2. Nessun messaggio viene inviato qui.

## Comandi Git proposti, da non eseguire automaticamente

Esempi da valutare soltanto dopo review del gruppo. Nessun commit, branch, push, tag o release viene creato da questa attività.

```powershell
git status --short
git diff --stat
# Dopo review, includere anche le correzioni applicative e i test approvati.
git add -- README.md backend frontend Documentazione
git diff --cached --stat
git diff --cached --name-only
# Controllare che lo staging contenga soltanto il materiale approvato.
git commit -m "correzioni finali e aggiornamento verifiche BugBoard26"
# Soltanto dopo approvazione e sul ramo concordato:
git push
```

Per la release usare successivamente l'interfaccia GitHub, scegliendo il commit concordato e un tag deciso dal gruppo. Non usare un URL di release inventato nella consegna. Nessun comando di branch è necessario per questa revisione documentale.
