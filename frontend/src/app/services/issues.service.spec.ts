import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../api.config';
import { IssuesService } from './issues.service';

describe('IssuesService immagini', () => {
  let service: IssuesService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(IssuesService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('invia multipart con campo image senza forzare Content-Type', () => {
    const file = new File(['png'], 'screen.png', { type: 'image/png' });
    service.uploadImage(5, file).subscribe(issue => expect(issue.hasImage).toBeTrue());
    const request = http.expectOne(`${API_BASE_URL}/issues/5/image`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body instanceof FormData).toBeTrue();
    expect(request.request.body.get('image')).toBe(file);
    expect(request.request.headers.has('Content-Type')).toBeFalse();
    request.flush({ id: 5, type: 'Bug', status: 'Todo', hasImage: true });
  });

  it('scarica tramite HttpClient come Blob', () => {
    const blob = new Blob(['png'], { type: 'image/png' });
    service.getImage(5).subscribe(result => expect(result).toBe(blob));
    const request = http.expectOne(`${API_BASE_URL}/issues/5/image`);
    expect(request.request.method).toBe('GET');
    expect(request.request.responseType).toBe('blob');
    request.flush(blob);
  });
});
