# Note operative interne del gruppo

Questo file raccoglie i promemoria separati dalla relazione finale durante
la revisione editoriale del 19/10/2026. Non e' parte della relazione accademica
e va escluso dallo ZIP di consegna. La pulizia del testo non certifica la
chiusura delle attivita' organizzative elencate sotto.

## Chiusura del materiale

- Relazione versione 1.7: contenuti descrittivi, risultati e limiti effettivi;
  nessuna checklist di lavoro, istruzione ai colleghi o procedura Teams.
- Prima di creare la release, controllare che il riferimento del codice,
  le evidenze e i file inclusi corrispondano al materiale approvato dal gruppo.
  Le esecuzioni storiche restano associate ai rispettivi commit e SHA256.
- Lo ZIP includera' sorgenti e immagini dei diagrammi correnti, non i VPD
  storici, secondo la decisione del gruppo. Conservarli nel repository:
  non rinominarli in VPP e non presentarli come sincronizzati. L'apertura
  nell'editor originale non e' stata verificata. La traccia richiede l'uso
  di strumenti CASE, non una specifica estensione VPD/VPP.
- Escludere dipendenze installate, bin/obj, directory temporanee, credenziali
  personali e token. Non eliminare volumi Docker o dati esistenti.
- Le operazioni Git, la creazione di release e l'invio sono esclusivamente
  manuali e a carico dell'utente. Nessuna di queste e' eseguita dalla review.

## Presentazione e demo

La scaletta dettagliata e la checklist amministrativa restano in
[consegna.md](consegna.md). Le slide devono ancora essere allineate ai
40 test frontend, alle tre sessioni e alla review con 28 controlli browser.
La scaletta da nove minuti e' un budget, non una prova cronometrata.

Prima della discussione:

1. Provare la presentazione entro dieci minuti e poi la breve demo separata.
2. Concordare chi espone, chi controlla il mouse e come passare fra gli account.
3. Preparare dati sintetici riconoscibili, account S/A distinti e READONLY.
4. Mostrare consultazione READONLY, risoluzione come assegnatario e notifica
   al segnalatore; aggiungere creazione con immagine, CSV, duplicato e archivio
   soltanto se il tempo della demo lo permette.
5. Non presentare screenshot storici come prova live o notifiche come realtime.
6. Preparare i link effettivi a repository e release; ogni componente segue
   la procedura individuale Teams e verifica le scadenze ufficiali.

La precedente istruzione di chiedere obbligatoriamente un chiarimento ai
docenti sui VPD non e' un requisito esplicito della traccia. Eventuali dubbi
sull'interpretazione del tool CASE restano distinti dalla scelta dello ZIP.

## Manutenzione del documento

Compilare dalla directory `Documentazione/Documentazione finale`, mantenendo
le cartelle di mockup e diagrammi nei percorsi relativi usati dal sorgente.
Il PDF puo' essere consultato autonomamente; il sorgente TeX richiede i file
inclusi `strategie_dominio.tex`, `test_plan.tex` e `vincoli_case.tex`.

```powershell
& '.\rigenera.ps1' -SoloDocumento `
  -TectonicExecutable "$env:LOCALAPPDATA\BugBoard26\document-tools\tectonic-0.17.0\tectonic.exe"
```

Le alternative e la rigenerazione dei PNG sono documentate nel
[readme](readme.md). Dopo modifiche a sorgenti o immagini, ricompilare e
controllare indice, riferimenti, tabelle e tutte le pagine renderizzate.
La compilazione LaTeX non esegue test applicativi. La generazione delle
slide e' separata dal comando sopra.

## Evidenze e usabilita'

I registri [evidenze.md](evidenze.md) e [verifica_documentale.md](verifica_documentale.md)
conservano le diverse esecuzioni. Non sommare checkpoint browser, test
Jasmine e casi xUnit come un unico conteggio. Eventuali errori degli script
temporanei sono descritti nei report tecnici, non eliminati dalle evidenze.

Il protocollo di preparazione e moderazione di ulteriori sessioni resta in
[usabilita.md](usabilita.md) e il questionario distribuibile in
[questionario-da-inviare.md](evidenze/questionario-da-inviare.md).
Le tre sessioni e i quindici voti gia' ricevuti restano registrati; tempi,
risposte aperte e osservazioni non comunicate non vanno completati per
deduzione. La decisione RF-17 proviene dal gruppo, non da un'approvazione
attribuita ai docenti.
