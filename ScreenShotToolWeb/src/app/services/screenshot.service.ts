import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
@Injectable({
  providedIn: 'root',
})
export class ScreenshotService {
  constructor(private http: HttpClient) {}
  baseUrl = 'https://localhost:7156/api';
  fetchUrlChooseFile(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/select`);
  }

  captureScreenshot(payload: any): Promise<{ path: string; message: string }> {
    const url = `${this.baseUrl}/screen-shot`;
    return this.http
      .post(url, payload, {
        responseType: 'json',
      })
      .toPromise()
      .then((res: any) => {
        if (!res || !res.path || !res.message) {
          throw new Error('API không trả về dữ liệu hợp lệ');
        }

        return {
          path: res.path,
          message: res.message,
        };
      });
  }
}
