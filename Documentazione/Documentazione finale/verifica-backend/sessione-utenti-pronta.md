# Chiudere la prova con utenti reali

## Stato accertato

Al controllo del 19 ottobre 2026 `evidenze/sessioni_utenti.csv` e
`evidenze/questionario.csv`, nella cartella Documentazione finale, contengono
solo intestazioni. Il documento finale dichiara le prove non eseguite.
Il protocollo completo e' gia' in `../usabilita.md`: non serve inventare una
seconda metodologia o cambiare il questionario.

Il collaudo browser automatico e la review tecnica assistita sono evidenze
tecniche, **non partecipanti** e non risposte a un questionario. Non compilare
i due CSV con i dati dello script o con persone simulate.

## Sessione breve proposta, non gia' svolta

Una proposta pratica e' coinvolgere due volontari rappresentativi: uno che
possa usare il ruolo USER e uno il ruolo READONLY. Due non e' un minimo imposto
dalla traccia, ne' un campione statisticamente rappresentativo. Registrare
soltanto il numero effettivo di persone che partecipa.

1. Avviare Docker del progetto e Angular. Confermare il checkout e annotare
   le eventuali modifiche locali; usare solo account e issue dimostrativi.
2. Il moderatore ADMIN crea gli account di prova dalla pagina Utenti. Conserva
   le credenziali solo localmente e prepara una issue riconoscibile con titolo
   e descrizione sul problema login, senza modificare segnalazioni altrui.
3. Spiegare al volontario che si valuta il prodotto, non la sua capacita', che
   puo' interrompere e che verranno raccolte osservazioni anonime. Ottenere il
   consenso e assegnare un codice P01/P02, senza raccogliere nomi o email reali.
4. Per USER proporre U1 (trovare la issue), U2 (crearene una con allegato) e U4
   (trovare la notifica e leggerla). Prima di U4 il moderatore risolve come ADMIN
   la nuova issue: il partecipante USER deve essere il creatore. Non attribuire
   al partecipante le operazioni preparatorie svolte dal moderatore.
5. Per READONLY proporre U1 e U5: cercare, filtrare per priorita' e scaricare il
   CSV, senza modificare dati. Non chiedere a questo ruolo di segnare lette
   le notifiche, perche' sarebbe contrario al requisito concordato.
6. Leggere gli obiettivi di `usabilita.md`, senza indicare subito pulsanti o
   percorsi. Annotare completamento autonomo/con aiuto/non completato, dubbi,
   aiuti ed errori osservati. Rilevare tempi solo se misurati davvero.
7. Far rispondere personalmente alle sette domande gia' definite. Per Q1-Q5
   usare 1-5 o non applicabile. Non e' un questionario SUS validato.
8. Salvare le righe reali nei CSV esistenti e controllarle con il gruppo. Solo
   dopo aggiornare documento e slide con numero effettivo, osservazioni e limiti.

Se U3 non viene proposta per mancanza di un assegnatario predisposto, indicare
non proposta: non confonderla con completata. Per provarla, predisporre una issue
assegnata al partecipante tramite l'API esistente; il form non ha selezione
manuale dell'assegnatario e non va aggiunta una funzionalita' per questo test.

## Cosa consegnare per chiudere il punto

- CSV delle sessioni con righe corrispondenti ad attivita' realmente osservate.
- CSV del questionario con sole risposte date dai volontari, anche negative.
- Sintesi breve: persone effettive, attivita' proposte, principali problemi,
  interventi eventualmente applicati e limiti del campione.
- Rilettura umana della review tecnica, soprattutto navigazione, tastiera,
  errori e contrasto: il controllo assistito non certifica WCAG.

Non sono necessarie registrazioni audio/video o servizi esterni. Non salvare
password, token, dati personali o statistiche inventate nella documentazione.
