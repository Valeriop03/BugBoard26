# Passaggio al collega della documentazione

Data della verifica: 16 ottobre 2026. Fonte principale: [report.md](report.md).
Questo file e' un promemoria: non modifica i documenti condivisi.

## Dati utilizzabili nei documenti finali

- Commit ricevuto dopo la pull: `e13995140dff382dc3d548208c6c1e60be91fe5d`.
- Risultato finale su quel commit piu' le tre modifiche locali elencate nel diff:
  **143/143 superati, 0 falliti, 0 ignorati**, di cui 11 dominio e 132 integrazione.
- Comando: `powershell -NoProfile -ExecutionPolicy Bypass -File quality/analyze-backend.ps1 -Mode Coverage`.
- Esecuzione finale: 16/10/2026, 09:20:09-09:21:58 UTC,
  cioe' 11:20:09-11:21:58 Europe/Rome. Metadati originali nelle evidenze.
- Build Release finale: 0 errori, 0 avvisi. PostgreSQL 16.15 reale dedicato ai
  test sulla porta 55432, non database di sviluppo. Server temporaneo arrestato.
- Coverage locale finale OpenCover: **782/824 sequence point (94,90%)** e
  **247/285 branch point (86,66%)**, assembly API, 29 sorgenti. Non sono valori
  Sonar. Stesse esclusioni del file quality ricevuto, nessuna esclusione aggiunta.
- La coverage preliminare dei soli 11 test di dominio riportata nei vecchi
  materiali resta storica e parziale, non e' il risultato della suite completa.

## Modifiche backend da riportare

1. Pin mirato di `Microsoft.OpenApi` a 2.7.5, per la dipendenza con advisory
   GHSA-v5pm-xwqc-g5wc. Audit finale NuGet senza pacchetti segnalati; non equivale
   a un'attestazione di assenza di vulnerabilita' del sistema.
2. Decisione READONLY confermata dal gruppo prima della modifica: nessuna
   scrittura, neppure per segnare come letta una propria notifica. GET delle
   proprie notifiche resta consentito; PATCH read restituisce 403 senza modifiche
   del database. ADMIN e USER restano invariati sulle proprie notifiche.
3. Aggiornamento del test precedente: sostituito il caso che consentiva la
   scrittura READONLY, verificando anche GET successivo e snapshot completo.
   Nessun aumento del totale dei test.

I file sono `BugBoard26.Api.csproj`, `NotificationsController.cs` e
`NotificationsIntegrationTests.cs`; il diff completo e' allegato.
Non descrivere come approvata un'eccezione a RF-17: il gruppo ha confermato il
divieto. Far verificare al collega frontend il comportamento del pulsante e
la gestione del 403 per READONLY; l'interfaccia non e' stata modificata qui.

## Cosa non dichiarare verificato

- Nessuna prova Docker su questo PC: mancano CLI/runtime disponibili.
- Nessun confronto di dati/immagini dopo restart o ricreazione container.
  Il test WebApplicationFactory non sostituisce questa prova.
- Nessuna analisi SonarQube, importazione coverage o Quality Gate eseguiti.
  Il preflight rifiutato per prerequisiti mancanti non e' un'analisi.
- Nessuno smoke HTTP completato della generazione OpenAPI in Development dopo
  il pin della dipendenza; includerlo nel collaudo del collega.
- Nessuna verifica frontend, modifica di documenti condivisi o operazione Git
  di commit, branch o push.

## Evidenze da aggiungere dal PC del collega

Per Docker: commit effettivo dell'immagine, data e comandi, ID issue di prova
riconoscibile, snapshot e SHA-256 prima/dopo restart e ricreazione del solo
backend, stessi mount e container PostgreSQL, utente non root e directory
immagini scrivibile. Non eliminare volumi o dati esistenti.

Per Sonar: versioni server/scanner, progetto esistente, commit e diff locale,
suite completa eseguita nella run di analisi, importazione OpenCover verificata,
task server completato, analysis ID, problemi valutati e stato/condizioni del
Quality Gate dell'analisi corrente. Non salvare token o directory scanner grezze.

Le istruzioni sono in [comandi-ripetizione.md](comandi-ripetizione.md).
La presenza di una procedura pronta non ne attesta l'esecuzione.
