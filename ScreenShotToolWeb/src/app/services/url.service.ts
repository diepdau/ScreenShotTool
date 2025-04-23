import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class UrlService {
  constructor(private http: HttpClient) {}

  fetchUrlsProjectLanguageIdFromApi(
    projectSlug: string,
    languageId: string,
    accountId: string
  ): Observable<any> {
    const url = `https://tcma-api-public.dev.usms.impartner.io/showcase/urls?ProjectSlug=${projectSlug}&LanguageId=${languageId}&AccountId=${accountId}`;
    return this.http.get<any>(url);
  }
}
