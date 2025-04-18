import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class ScreenshotService {
  constructor(private http: HttpClient) {}

  captureScreenshot(queryParams: any): Promise<Blob> {
    const queryString = new URLSearchParams(queryParams).toString();
    const fullUrl = `https://localhost:7156/api/screen-shot?${queryString}`;

    return this.http
      .get(fullUrl, { responseType: 'blob' })
      .toPromise()
      .then((blob) => {
        if (!blob) {
          throw new Error('No Blob returned from API');
        }
        return blob;
      })
      .catch((error) => {
        console.error('Error capturing screenshot:', error);
        throw error;
      });
  }
}
