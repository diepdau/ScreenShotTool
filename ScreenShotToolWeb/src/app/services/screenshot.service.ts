import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class ScreenshotService {
  constructor(private http: HttpClient) {}
  fetchUrlChooseFile(): Observable<any> {
    return this.http.get<any>('/api/select');
  }
  captureScreenshot(
    payload: any
  ): Observable<{ path: string | null; message: string }> {
    const url = `/api/screen-shot`;

    return this.http.post<any>(url, payload, { responseType: 'json' }).pipe(
      map((res: any) => {
        if (!res || !res.path || !res.message) {
          throw new Error('API does not return valid data');
        }

        if (res.message && res.message.includes('Timeout')) {
          return {
            message: res.message,
            path: null,
          };
        }
        console.log('ấdf', res);
        return {
          path: res.path,
          message: res.message,
        };
      }),
      catchError((error) => {
        console.error('Error during screenshot capture:', error);
        throw new Error('An error occurred while calling the API.');
      })
    );
  }
}
