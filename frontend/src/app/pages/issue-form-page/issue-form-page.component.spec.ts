import { TestBed } from '@angular/core/testing';
import { NgForm } from '@angular/forms';
import { Router, provideRouter } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { of, Subject } from 'rxjs';
import { Issue } from '../../models/issue.model';
import { IssuesService } from '../../services/issues.service';
import { IssueFormPageComponent } from './issue-form-page.component';

describe('IssueFormPageComponent validazione', () => {
  let service: jasmine.SpyObj<IssuesService>;
  let router: Router;
  let form: NgForm;

  beforeEach(() => {
    service = jasmine.createSpyObj('IssuesService', ['suggestAssignee', 'createIssue']);
    service.suggestAssignee.and.returnValue(of({ userId: 2, email: 'user@example.local', openAssignedIssues: 0 }));
    form = { invalid: false, control: { markAllAsTouched: jasmine.createSpy('markAllAsTouched') } } as unknown as NgForm;
    TestBed.configureTestingModule({ providers: [
      { provide: IssuesService, useValue: service },
      provideRouter([])
    ] });
    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.returnValue(Promise.resolve(true));
  });

  for (const [title, description] of [
    ['', ''], ['   ', 'Descrizione'], ['Titolo', '   '],
    ['ab', 'Descrizione'], ['Titolo', 'ab'],
    ['x'.repeat(201), 'Descrizione'], ['Titolo', 'x'.repeat(4001)]
  ]) {
    it(`rifiuta lunghezze titolo=${title.length}, descrizione=${description.length}`, () => {
      const fixture = TestBed.createComponent(IssueFormPageComponent);
      const page = fixture.componentInstance;
      page.title = title;
      page.description = description;
      page.submit(form);
      expect(service.createIssue).not.toHaveBeenCalled();
      expect(form.control.markAllAsTouched).toHaveBeenCalled();
      expect(page.isSubmitting).toBeFalse();
      fixture.destroy();
    });
  }

  it('invia dati validi senza spazi esterni e impedisce doppi invii', () => {
    const fixture = TestBed.createComponent(IssueFormPageComponent);
    const page = fixture.componentInstance;
    const response = new Subject<Issue>();
    service.createIssue.and.returnValue(response);
    page.title = '  Errore login  ';
    page.description = '  Descrizione errore  ';
    page.submit(form);
    page.submit(form);
    expect(service.createIssue).toHaveBeenCalledOnceWith({
      title: 'Errore login', description: 'Descrizione errore', type: 'BUG', priority: null, assignedToId: 2
    });
    response.next({ id: 5 } as Issue);
    response.complete();
    expect(router.navigate).toHaveBeenCalledWith(['/issues', 5]);
    expect(page.isSubmitting).toBeFalse();
    fixture.destroy();
  });

  it('mantiene il messaggio backend e permette un nuovo invio', () => {
    const fixture = TestBed.createComponent(IssueFormPageComponent);
    const page = fixture.componentInstance;
    const response = new Subject<Issue>();
    service.createIssue.and.returnValue(response);
    page.title = 'Titolo';
    page.description = 'Descrizione';
    page.submit(form);
    response.error(new HttpErrorResponse({ status: 400, error: { errors: { Title: ['Titolo non valido.'] } } }));
    expect(page.errorMessage).toBe('Titolo non valido.');
    expect(page.isSubmitting).toBeFalse();
    service.createIssue.and.returnValue(of({ id: 6 } as Issue));
    page.submit(form);
    expect(service.createIssue).toHaveBeenCalledTimes(2);
    fixture.destroy();
  });
});
