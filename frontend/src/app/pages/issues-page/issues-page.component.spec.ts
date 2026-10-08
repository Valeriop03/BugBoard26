import { fakeAsync, TestBed, tick } from '@angular/core/testing';
import { Subject } from 'rxjs';
import { Issue } from '../../models/issue.model';
import { AuthService } from '../../services/auth.service';
import { IssuesService } from '../../services/issues.service';
import { IssuesPageComponent } from './issues-page.component';

describe('IssuesPageComponent ricerca live', () => {
  let requests: Subject<Issue[]>[];
  let service: jasmine.SpyObj<IssuesService>;

  beforeEach(() => {
    requests = [];
    service = jasmine.createSpyObj('IssuesService', ['getIssues', 'exportIssues']);
    service.getIssues.and.callFake(() => {
      const response = new Subject<Issue[]>();
      requests.push(response);
      return response;
    });
    TestBed.configureTestingModule({
      providers: [
        { provide: IssuesService, useValue: service },
        { provide: AuthService, useValue: { getCurrentUser: () => null } }
      ]
    });
  });

  it('annulla la richiesta precedente appena cambia la ricerca', fakeAsync(() => {
    const fixture = TestBed.createComponent(IssuesPageComponent);
    const page = fixture.componentInstance;
    tick(0);
    page.keyword = 'login';
    page.onSearchChange();
    expect(requests[0].observed).toBeFalse();
    expect(page.isLoading).toBeTrue();
    tick(200);
    page.keyword = 'export';
    page.onSearchChange();
    tick(349);
    expect(service.getIssues).toHaveBeenCalledTimes(1);
    tick(1);
    expect(service.getIssues.calls.mostRecent().args[0]?.keyword).toBe('export');
    const latest = [{ id: 2, title: 'Export CSV' } as Issue];
    requests[1].next(latest);
    requests[1].complete();
    requests[0].next([{ id: 1 } as Issue]);
    expect(page.issues).toEqual(latest);
    expect(page.isLoading).toBeFalse();
    fixture.destroy();
  }));

  it('continua a cercare dopo un errore e ripristina i filtri', fakeAsync(() => {
    const fixture = TestBed.createComponent(IssuesPageComponent);
    const page = fixture.componentInstance;
    tick(0);
    requests[0].error(new Error('API non disponibile'));
    expect(page.errorMessage).not.toBe('');
    expect(page.isLoading).toBeFalse();
    page.type = 'BUG';
    page.loadIssues();
    tick(0);
    expect(page.errorMessage).toBe('');
    requests[1].next([{ id: 3 } as Issue]);
    requests[1].complete();
    expect(page.issues.length).toBe(1);
    page.keyword = 'login';
    page.onSearchChange();
    page.resetFilters();
    tick(0);
    expect(service.getIssues.calls.mostRecent().args[0]).toEqual({
      keyword: '', type: '', status: '', priority: '', sortBy: 'Date', sortDirection: 'Desc'
    });
    tick(350);
    expect(service.getIssues).toHaveBeenCalledTimes(3);
    fixture.destroy();
  }));

  it('interrompe richieste e attesa alla distruzione', fakeAsync(() => {
    const fixture = TestBed.createComponent(IssuesPageComponent);
    tick(0);
    fixture.componentInstance.onSearchChange();
    fixture.destroy();
    tick(350);
    expect(requests[0].observed).toBeFalse();
    expect(service.getIssues).toHaveBeenCalledTimes(1);
  }));

  it('esporta i parametri correnti anche durante il debounce', fakeAsync(() => {
    const fixture = TestBed.createComponent(IssuesPageComponent);
    const page = fixture.componentInstance;
    service.exportIssues.and.returnValue(new Subject<Blob>());
    page.keyword = 'login';
    page.type = 'BUG';
    page.onSearchChange();
    page.exportCsv();
    expect(service.exportIssues.calls.mostRecent().args[0]).toEqual({
      keyword: 'login', type: 'BUG', status: '', priority: '', sortBy: 'Date', sortDirection: 'Desc'
    });
    fixture.destroy();
    tick(350);
  }));
});
