import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { createRequire } from 'node:module';

const root = path.dirname(fileURLToPath(import.meta.url));
const build = path.join(root, '_build', 'presentazione');
await fs.mkdir(build, {recursive:true});
const runtimeModules = process.env.ARTIFACT_NODE_MODULES;
if (!runtimeModules) throw new Error('Impostare ARTIFACT_NODE_MODULES: directory con @oai/artifact-tool.');
process.env.RUNTIME_NODE_MODULES=runtimeModules;
const requireRuntime = createRequire(path.join(runtimeModules, '__resolve_artifact__.cjs'));
const {Presentation, PresentationFile, FileBlob} = await import(pathToFileURL(requireRuntime.resolve('@oai/artifact-tool')).href);
const skill = process.env.PRESENTATIONS_SKILL_DIR;
const python = process.env.ARTIFACT_PYTHON;
if (!skill || !python) throw new Error('Impostare PRESENTATIONS_SKILL_DIR e ARTIFACT_PYTHON per la validazione.');
const {finalizePresentation} = await import(pathToFileURL(path.join(skill,'container_tools/artifact_tool_utils.mjs')).href);

const p = Presentation.create({slideSize:{width:1280,height:720}});
const font = 'Arial';
const ink = '#183B5C', body = '#263746';
const specs = [];
function text(s, value, x, y, w, h, size=30, bold=false, color=body) {
  const sh=s.shapes.add({geometry:'textbox',name:value.slice(0,70),
    position:{left:x,top:y,width:w,height:h},fill:'none',
    line:{style:'solid',fill:'none',width:0}});
  sh.text=value;
  sh.text.style={typeface:font,fontSize:size,bold,color};
  return sh;
}
function slide(title, seconds, notes) {
  const s=p.slides.add(); s.background.fill='#FFFFFF';
  text(s,title,64,40,1152,78,44,true,ink);
  const n=p.slides.items.length;
  text(s,`${n}`,1180,674,40,24,18,false,'#5D6B77');
  s.speakerNotes.text=`Budget: ${seconds} secondi.\n${notes}`;
  specs.push({slide:n,title,seconds});
  return s;
}
async function diagram(s,name,area) {
  const b=await fs.readFile(path.join(root,'diagrammi',name+'.png'));
  s.images.add({blob:b.buffer.slice(b.byteOffset,b.byteOffset+b.byteLength),
    contentType:'image/png',alt:name,fit:'contain',position:area});
}
function table(s,values,widths,top,height,size=25) {
  const tb=s.tables.add({rows:values.length,columns:values[0].length,
    left:64,top,width:1152,height,columnWidths:widths,values});
  tb.styleOptions={headerRow:false,bandedRows:false};
  tb.borders.assign({style:'solid',fill:'#D5DFE7',width:1});
  for(let r=0;r<values.length;r++)for(let c=0;c<values[0].length;c++){
    const cell=tb.getCell(r,c);
    cell.fill=r===0?'#183B5C':'#FFFFFF';
    cell.text.style={typeface:font,fontSize:size,color:r===0?'#FFFFFF':body,bold:r===0};
  }
  return tb;
}

let s=slide('BugBoard26',20,
  'Progetto SWE 2025/2026, modalità tradizionale. Presentare perimetro assegnato e scelte tecniche. Specificare che il materiale è una revisione aperta: risultati finali ancora da consolidare. Fonte: traccia §§3.2, 8, 10 e specifica requisiti. Repository: https://github.com/Valeriop03/BugBoard26.');
text(s,'Architettura, modello e verifica',64,236,1152,84,48,true,ink);
text(s,'Progetto di Ingegneria del Software 2025/2026',64,350,1152,60,30);
text(s,'Revisione documentale del 16 ottobre 2026',64,440,1152,50,26);
text(s,'Risultati finali ancora da consolidare',64,520,1152,62,32,true);

s=slide('Architettura e separazione dei componenti',75,
  'Spiegare perché SPA e API sono eseguibili separatamente. Il browser chiama REST con JWT, il backend centralizza dati e regole. EF Core/Npgsql accede a PostgreSQL; i file immagine risiedono nello storage backend. Compose esegue API e database, Angular resta sul PC. Motivare indipendenza dalla UI e limite del setup HTTP locale. Fonti: backend/BugBoard26.Api/Program.cs, docker/docker-compose.yml, README.md, component_diagram.puml. Il disegno è una vista sintetica del modello esistente.');
await diagram(s,'architettura_presentazione',{left:64,top:154,width:1152,height:360});
text(s,'Backend autonomo. Stato persistente centralizzato.',64,552,1152,55,30,true);
text(s,'Angular sul PC, API e PostgreSQL separati in Compose',64,614,1152,42,25);

s=slide('Modello dati e vincoli',75,
  'Descrivere User, Issue e Notification. Issue ha segnalatore obbligatorio, assegnatario e originale duplicato opzionali. Unique email e FK configurate in EF. Distinguere validazione degli assegnatari dal vincolo SQL. ImagePath è un riferimento al file; non un BLOB. Le regole DeleteBehavior sono nel diagramma dati completo. La vista sintetica omette alcune associazioni: AssignedToId e UserId sono FK verso User, DuplicateOfIssueId è una FK verso Issue. Consultare il modello completo per tutte le molteplicità e cancellazioni. Fonti: Models/*.cs, Data/ApplicationDbContext.cs, Migrations/20260629102322_InitialCreate.cs, database_schema.puml.');
await diagram(s,'modello_presentazione',{left:64,top:146,width:1152,height:380});
text(s,'Email univoca e chiavi esterne nel mapping EF',64,556,1152,48,29,true);
text(s,'Vista sintetica. Tutte le relazioni e i vincoli nel documento.',64,614,1152,44,25);

s=slide('Astrazioni object-oriented e dipendenze',60,
  'Motivare le astrazioni presenti, senza attribuire pattern assenti. PasswordHasher e JwtTokenService implementano interfacce sostituibili tramite DI. IssueDomainService è una classe concreta con regole pure. DTO separano HTTP ed entità, DbContext è ricevuto direttamente dai controller. Le entità non sono un dominio ricco: orchestrazione e salvataggi sono nei controller. Limite: dipendenza da EF da verificare con test HTTP/PostgreSQL. Fonti: Program.cs, Services/IPasswordHasher.cs, IJwtTokenService.cs, IssueDomainService.cs, Controllers, class_diagram.puml.');
table(s,[['Scelta','Responsabilità'],
  ['Contratti e DI','Hash password e generazione JWT sostituibili'],
  ['Regole pure','IssueDomainService senza accesso al database'],
  ['DTO ed entità','Contratto HTTP distinto dalla persistenza']], [340,812],176,334,27);
text(s,'I controller orchestrano e usano direttamente DbContext',64,550,1152,52,29,true);
text(s,'Il codice attuale non introduce repository applicativi',64,614,1152,44,26);

s=slide('Permessi e controlli sul backend',75,
  'La UI nasconde comandi ma il server resta il punto di controllo. Mostrare la distinzione tra creazione, cambio stato, archiviazione/duplicazione e gestione utenti. CanChangeIssueStatus verifica account attivo e assegnazione. Non attribuire un lookup account attivo a ogni endpoint: Archive e MarkAsDuplicate usano il ruolo JWT. NotificationsController controlla claim e account nel metodo, senza attributo Authorize. RF-17, decisione del gruppo del 16 ottobre 2026, non chiarimento docente: READONLY consulta le proprie notifiche ma non le segna lette. Backend e test affidati ad amico 1; frontend da allineare e verificare. Oracolo TP-14: GET proprie 200, PATCH read su notifica propria esistente 403 senza scritture, nessun comando UI. Non presentare questi esiti come già acquisiti. Fonti: IssuesController, UsersController, NotificationsController, IssueDomainService, auth.guard.ts.');
table(s,[['Operazione','ADMIN','USER','READONLY'],
  ['Consultare ed esportare','Sì','Sì','Sì'],
  ['Creare issue','Sì','Sì','No'],
  ['Cambiare stato','Sì','Assegnatario','No'],
  ['Archiviare / duplicato','Sì','No','No'],
  ['Gestire utenti','Sì','No','No']], [510,180,282,180],164,366,25);
text(s,'RF-17, decisione del gruppo: READONLY consulta le proprie notifiche',64,554,1152,42,26,true);
text(s,'Segna letta vietato. Allineamento e verifica ancora aperti.',64,602,1152,42,26);

s=slide('Risoluzione e notifica: persistenza e ripetizione',60,
  'Seguire la sequenza cambio stato esistente. Ingresso in Resolved aggiorna UpdatedAt e ResolvedAt e aggiunge Notification per CreatedById, non per AssignedToId. Una sola SaveChangesAsync persiste le modifiche. Stesso stato valido su issue non duplicata non scrive date o notifiche. Uscire da Resolved azzera ResolvedAt, senza cancellare notifiche. Duplicate è rifiutato nel percorso status con 400, una issue già duplicata dà 409. Non introdurre una matrice generale di transizioni assente nel codice. Fonti: IssuesController.UpdateStatus, sequence_cambio_stato.puml, IssueStatusIntegrationTests.cs.');
text(s,'1. Account attivo e permesso di modifica',64,180,1152,60,34,true);
text(s,'2. Stato e date UTC aggiornati nel controller',64,270,1152,60,34);
text(s,'3. Notifica non letta al segnalatore',64,360,1152,60,34);
text(s,'4. Un salvataggio EF per issue e notifica',64,450,1152,60,34);
text(s,'Stesso stato: date e notifiche restano invariate',64,580,1152,58,31,true);

s=slide('Strategia di test e piano UC-02',80,
  'Tre metodi non banali a due parametri già testati in xUnit. SearchIssues: titolo, descrizione, nessuna corrispondenza; assert sul conteggio, non sugli ID. CanChangeIssueStatus: ruoli, attività, assegnazione. SuggestAssignee: carichi e assenza candidati. Esporre anche i limiti: null/trim e case della ricerca, parità e archiviati non hanno casi unitari dedicati. Test HTTP reali con PostgreSQL separato verificano autorizzazione, query e persistenza. TP-UC02 copre risoluzione, date, notifica, rifiuti e ripetizione. In questa attività i test non sono rilanciati e non si dichiara coverage completa. Fonti: IssueDomainServiceTests.cs, Integration/*, documentazione_finale.tex, strategie_dominio.tex, test_plan.tex.');
text(s,'SearchIssues: titolo, descrizione, nessun risultato',64,170,1152,66,31,true);
text(s,'CanChangeIssueStatus: ruolo, attività e assegnazione',64,258,1152,66,31);
text(s,'SuggestAssignee: carico e assenza di candidati',64,346,1152,66,31);
text(s,'TP-UC02: permessi, date, notifiche e ripetizione',64,448,1152,66,31,true);
text(s,'xUnit in memoria e test HTTP con PostgreSQL isolato',64,550,1152,48,27);
text(s,'Strategie documentate. Esiti finali da acquisire.',64,616,1152,44,26,true);

s=slide('Qualità, usabilità ed evidenze aperte',60,
  'Il collaudo frontend reale di Valerio del 16 ottobre è riportato nel documento unico ed è confermato dal gruppo. Comprende build e 27 test frontend e scenari UI/API con backend Docker e PostgreSQL reali; non è una nuova esecuzione dell’autore documentale e non equivale alla suite backend o alle prove con utenti. Report e screenshot permanenti da acquisire. Amico 1 depositerà suite/coverage backend, persistenza dopo ricreazione e report SonarQube in verifica-backend/ alla radice del repository. Report ancora da acquisire e leggere, non verificati dalla presenza del codice. La nota del 9 ottobre resta storica e parziale. Il §2.4 richiede review sistematica, utenti reali/potenziali e survey: attività ancora da svolgere. Conformità CASE ancora aperta. RF-17 deciso dal gruppo: READONLY consulta le proprie notifiche e non le segna lette. Backend e test ad amico 1, frontend da allineare e verificare; esiti ancora aperti. Fonti: documentazione_finale.tex, sezione Collaudo frontend reale, evidenze.md, qualita_backend.md, usabilita.md, traccia §§2.4 e 3.2.');
text(s,'Collaudo frontend',64,164,1152,53,34,true);
text(s,'Riepilogo reale di Valerio. Report permanente atteso.',64,221,1152,55,29);
text(s,'Qualità backend',64,316,1152,53,34,true);
text(s,'Amico 1 depositerà gli esiti in verifica-backend/.',64,373,1152,55,29);
text(s,'Usabilità e conformità CASE',64,468,1152,53,34,true);
text(s,'Prove con utenti e accettazione del tool ancora aperte.',64,525,1152,55,29);
text(s,'Documentazione aperta fino alle evidenze necessarie',64,620,1152,43,26,true);

s=slide('Demo breve coordinata con il frontend',35,
  'Passare al collega frontend dopo la presentazione tecnica. Proposta di demo successiva di 2-3 minuti, da concordare e provare realmente. Preparare account distinti S e A, READONLY e una issue già assegnata. Legare ogni passaggio a un controllo tecnico. Non dichiarare collaudata la demo prima della prova. Tutti i membri devono conoscere ogni parte del sistema. Fonte: consegna.md e traccia §10. Budget complessivo delle slide: 540 secondi, previsione non prova cronometrata.');
text(s,'READONLY consulta, filtra ed esporta',64,204,1152,68,36,true);
text(s,'L’assegnatario risolve una issue predisposta',64,316,1152,68,36);
text(s,'Il segnalatore ritrova e legge la notifica',64,428,1152,68,36);
text(s,'Passaggio al collega e prova congiunta ancora da confermare',64,582,1152,65,28,true);

if(specs.reduce((n,s)=>n+s.seconds,0)!==540)throw new Error('Budget slide inatteso');
const candidate=path.join(build,'candidate.pptx');
await(await PresentationFile.exportPptx(p)).save(candidate);
let final=path.join(root,'presentazione','presentazione_tecnica.pptx');
await fs.mkdir(path.dirname(final),{recursive:true});
try { await fs.access(final); final=path.join(root,'presentazione',`presentazione_tecnica_${Date.now()}.pptx`); } catch {}
await finalizePresentation({workspaceDir:root,candidatePath:candidate,finalPath:final,
  pythonExecutable:python,
  integrityValidatorPath:path.join(skill,'container_tools/inspect_presentation_package_integrity.py'),
  layoutValidatorPath:path.join(skill,'container_tools/inspect_presentation_layout_geometry.py'),
  explicitTotalSlideCount:9,requiredNativeTableOwnerSlides:[4,5],
  layoutArgs:['--expected-slide-size-emu','12192000,6858000','--validate-bullet-geometry','--validate-heading-fit',
    '--require-native-table-slide','4','--require-native-table-slide','5'],
  fontPolicy:{basis:'design',families:[font]},verifyArtifactToolImport:true,
  receiptPath:path.join(build,`validation-${Date.now()}.json`)});
const checked=await PresentationFile.importPptx(await FileBlob.load(final));
const previews=path.join(build,'slides');await fs.mkdir(previews,{recursive:true});
for(const [i,sl] of checked.slides.items.entries()){
  const png=await checked.export({slide:sl,format:'png',scale:1});
  await fs.writeFile(path.join(previews,`slide-${String(i+1).padStart(2,'0')}.png`),new Uint8Array(await png.arrayBuffer()));
}
await fs.writeFile(path.join(build,'scaletta.json'),JSON.stringify({total_seconds:540,rehearsal:'NON ESEGUITA',slides:specs},null,2));
console.log(JSON.stringify({file:final,slides:checked.slides.items.length,planned_seconds:540,rehearsal:'NON ESEGUITA'}));
