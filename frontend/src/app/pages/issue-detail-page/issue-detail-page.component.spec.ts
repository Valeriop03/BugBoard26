import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { of, Subject } from 'rxjs';
import { Issue } from '../../models/issue.model';
import { AuthService } from '../../services/auth.service';
import { IssuesService } from '../../services/issues.service';
import { IssueDetailPageComponent } from './issue-detail-page.component';

describe('IssueDetailPageComponent immagine', () => {
  let service: jasmine.SpyObj<IssuesService>;
  let image: Subject<Blob>;

  beforeEach(() => {
    service = jasmine.createSpyObj('IssuesService', ['getIssueById', 'getImage']);
    service.getIssueById.and.returnValue(of({ id: 5, title: 'Errore', hasImage: true } as Issue));
    image = new Subject<Blob>();
    service.getImage.and.returnValue(image);
    TestBed.configureTestingModule({ providers: [
      provideRouter([]),
      { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => '5' } } } },
      { provide: IssuesService, useValue: service },
      { provide: AuthService, useValue: { getCurrentUser: () => ({ id: 3, role: 'READONLY' }) } }
    ] });
    spyOn(URL, 'createObjectURL').and.returnValue('blob:issue-image');
    spyOn(URL, 'revokeObjectURL');
  });

  it('carica anche per READONLY e revoca object URL alla distruzione', () => {
    const fixture = TestBed.createComponent(IssueDetailPageComponent);
    const page = fixture.componentInstance;
    expect(service.getImage).toHaveBeenCalledWith(5);
    expect(page.isLoadingImage).toBeTrue();
    const blob = new Blob(['png'], { type: 'image/png' });
    image.next(blob);
    image.complete();
    expect(URL.createObjectURL).toHaveBeenCalledWith(blob);
    expect(page.imageUrl).toBe('blob:issue-image');
    expect(page.isLoadingImage).toBeFalse();
    expect(page.canChangeStatus).toBeFalse();
    fixture.destroy();
    expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:issue-image');
  });

  it('non richiede un file quando hasImage e false', () => {
    service.getIssueById.and.returnValue(of({ id: 5, hasImage: false } as Issue));
    const fixture = TestBed.createComponent(IssueDetailPageComponent);
    expect(service.getImage).not.toHaveBeenCalled();
    expect(fixture.componentInstance.imageUrl).toBeNull();
    fixture.destroy();
  });

  it('gestisce errore HTTP senza nascondere il dettaglio', () => {
    const fixture = TestBed.createComponent(IssueDetailPageComponent);
    image.error(new Error('File non disponibile'));
    expect(fixture.componentInstance.imageError).not.toBe('');
    expect(fixture.componentInstance.issue?.id).toBe(5);
    expect(fixture.componentInstance.isLoadingImage).toBeFalse();
    fixture.destroy();
  });

  it('annulla un download pendente quando si lascia la pagina', () => {
    const fixture = TestBed.createComponent(IssueDetailPageComponent);
    fixture.destroy();
    expect(image.observed).toBeFalse();
    expect(URL.createObjectURL).not.toHaveBeenCalled();
  });

  it('libera il file se il browser non riesce a visualizzarlo', () => {
    const fixture = TestBed.createComponent(IssueDetailPageComponent);
    image.next(new Blob(['invalid']));
    image.complete();
    fixture.componentInstance.imageLoadFailed();
    expect(fixture.componentInstance.imageUrl).toBeNull();
    expect(fixture.componentInstance.imageError).not.toBe('');
    expect(URL.revokeObjectURL).toHaveBeenCalledOnceWith('blob:issue-image');
    fixture.destroy();
    expect(URL.revokeObjectURL).toHaveBeenCalledTimes(1);
  });
});
