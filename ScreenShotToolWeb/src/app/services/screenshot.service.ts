import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { ScreenshotResponse } from '../models/screentShot.models';

@Injectable({
  providedIn: 'root',
})
export class ScreenshotService {
  constructor(private http: HttpClient) {}

  // captureScreenshot(payload: any): Promise<Blob> {
  //   const fullUrl = `https://localhost:7156/api/screen-shot`;

  //   return this.http
  //     .post(fullUrl, payload, { responseType: 'blob' })
  //     .toPromise()
  //     .then((blob) => {
  //       if (!blob) {
  //         throw new Error('No Blob returned from API');
  //       }
  //       return blob;
  //     })
  //     .catch((error) => {
  //       console.error('Error capturing screenshot:', error);
  //       throw error;
  //     });
  // }
  captureScreenshot(payload: any): Promise<ScreenshotResponse> {
    const fullUrl = `https://localhost:7156/api/screen-shot123`;

    return this.http
      .post<ScreenshotResponse>(fullUrl, payload)
      .toPromise()
      .then((res) => {
        if (!res || !res.base64Image) {
          throw new Error('Invalid screenshot response');
        }
        return res;
      })
      .catch((error) => {
        console.error('Error capturing screenshot:', error);
        throw error;
      });
  }
}
