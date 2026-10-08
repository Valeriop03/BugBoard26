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
    service = jasmine.createSpyObj('IssuesService', ['suggestAssignee', 'createIssue', 'uploadImage']);
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
  for (const file of [
    new File(['text'], 'note.txt', { type: 'text/plain' }),
    new File(['text'], 'fake.png', { type: 'text/plain' }),
    new File([], 'empty.png', { type: 'image/png' }),
    new File([new Uint8Array(5242881)], 'large.jpg', { type: 'image/jpeg' })
  ]) {
    it(`rifiuta immagine ${file.name}`, () => {
      const fixture = TestBed.createComponent(IssueFormPageComponent);
      const page = fixture.componentInstance;
      page.title = 'Titolo';
      page.description = 'Descrizione';
      page.selectImage({ target: { files: [file], value: file.name } } as unknown as Event);
      page.submit(form);
      expect(page.imageError).not.toBe('');
      expect(page.selectedImage).toBeNull();
      expect(service.createIssue).not.toHaveBeenCalled();
      fixture.destroy();
    });
  }

  it('carica dopo la creazione e mantiene il blocco durante upload', () => {
    const fixture = TestBed.createComponent(IssueFormPageComponent);
    const page = fixture.componentInstance;
    const file = new File(['png'], 'screen.png', { type: 'image/png' });
    const upload = new Subject<Issue>();
    service.createIssue.and.returnValue(of({ id: 8 } as Issue));
    service.uploadImage.and.returnValue(upload);
    page.title = 'Titolo';
    page.description = 'Descrizione';
    page.selectImage({ target: { files: [file] } } as unknown as Event);
    page.submit(form);
    page.submit(form);
    page.retryImageUpload();
    expect(page.isSubmitting).toBeTrue();
    expect(service.createIssue).toHaveBeenCalledTimes(1);
    expect(service.uploadImage).toHaveBeenCalledOnceWith(8, file);
    expect(router.navigate).not.toHaveBeenCalled();
    upload.next({ id: 8, hasImage: true } as Issue);
    upload.complete();
    expect(router.navigate).toHaveBeenCalledWith(['/issues', 8]);
    expect(page.isSubmitting).toBeFalse();
    fixture.destroy();
  });

  it('riprova solo upload dopo un errore senza creare una seconda issue', () => {
    const fixture = TestBed.createComponent(IssueFormPageComponent);
    const page = fixture.componentInstance;
    const file = new File(['jpeg'], 'screen.jpg', { type: 'image/jpeg' });
    const upload = new Subject<Issue>();
    service.createIssue.and.returnValue(of({ id: 9 } as Issue));
    service.uploadImage.and.returnValue(upload);
    page.title = 'Titolo';
    page.description = 'Descrizione';
    page.selectedImage = file;
    page.submit(form);
    upload.error(new HttpErrorResponse({ status: 400, error: { message: 'File non valido.' } }));
    expect(page.createdIssueId).toBe(9);
    expect(page.errorMessage).toBe('File non valido.');
    expect(page.isSubmitting).toBeFalse();
    page.submit(form);
    service.uploadImage.and.returnValue(of({ id: 9, hasImage: true } as Issue));
    page.retryImageUpload();
    expect(service.createIssue).toHaveBeenCalledTimes(1);
    expect(service.uploadImage).toHaveBeenCalledTimes(2);
    expect(router.navigate).toHaveBeenCalledWith(['/issues', 9]);
    fixture.destroy();
  });

  it('rimuove la selezione e permette la creazione senza immagine', () => {
    const fixture = TestBed.createComponent(IssueFormPageComponent);
    const page = fixture.componentInstance;
    const input = { value: 'screen.png' } as HTMLInputElement;
    page.selectedImage = new File(['png'], 'screen.png', { type: 'image/png' });
    page.removeImage(input);
    page.title = 'Titolo';
    page.description = 'Descrizione';
    service.createIssue.and.returnValue(of({ id: 10 } as Issue));
    page.submit(form);
    expect(input.value).toBe('');
    expect(service.uploadImage).not.toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/issues', 10]);
    fixture.destroy();
  });
});
