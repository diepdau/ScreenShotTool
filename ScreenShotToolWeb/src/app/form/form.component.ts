import { Component } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-form',
  standalone: false,
  templateUrl: './form.component.html',
  styleUrl: './form.component.css',
})
export class FormComponent {
  form!: FormGroup;
  fetchedUrls: string[] = [];
  resultList: { url: string; message: string; path: string }[] = [];
  isLoading = false;
  statusMessage = '';
  logs: { type: 'info' | 'error'; message: string }[] = [];
  languageList: { languageName: string; languageId: string }[] = [];
  constructor(private fb: FormBuilder, private http: HttpClient) {
    const urlRegex =
      /^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$/;
    this.form = this.fb.group(
      {
        api: ['', Validators.pattern(urlRegex)],
        urls: [''],
        customCss: [''],
        delay: ['', Validators.min(0)],
        width: ['', Validators.min(300)],
        folderPath: ['', Validators.required],
        projectSlug: [''],
        languageId: [''],
      },
      {
        validators: this.atLeastOneFieldValidator,
      }
    );
    this.http
      .get<any>('https://67ff715358f18d7209f1348e.mockapi.io/api/v1/urls/url')
      .subscribe({
        next: (data) => {
          this.languageList = data;
          this.addLog('Fetched language successfull');
        },
        error: (err) => {
          this.addLog('Error fetching language' + err.message, 'error');
        },
      });
  }
  atLeastOneFieldValidator(group: FormGroup): ValidationErrors | null {
    // const api = group.get('api')?.value?.trim();
    // const urls = group.get('urls')?.value?.trim();
    // if (!api && !urls) {
    //   return { requireOne: true };
    // }
    // if (api && !urls) {
    //   return { requireOne: true };
    // }
    // return null;

    const projectSlug = group.get('projectSlug')?.value?.trim();
    const languageId = group.get('languageId')?.value?.trim();
    const urls = group.get('urls')?.value?.trim();
    if (projectSlug && languageId && !urls) {
      return { requireOne: true };
    }
    if (!projectSlug && !languageId && !urls) {
      return { requireOne: true };
    }
    return null;
  }
  fetchUrlsFromApi() {
    this.isLoading = true;
    this.statusMessage = 'Fetching URLs...';

    this.http.get<any>(this.form.value.api).subscribe({
      next: (data) => {
        this.fetchedUrls = data;
        this.form.patchValue({ urls: data.join(', ') });
        this.statusMessage = ' URLs loaded successfully.';
        this.addLog('Fetched ' + data.length + ' URLs');
      },
      error: (err) => {
        this.statusMessage = 'Failed to fetch URLs';
        this.addLog('Error fetching URLs: ' + err.message, 'error');
      },
      complete: () => (this.isLoading = false),
    });
  }

  // url projectSlug and languageId
  fetchUrlsProjectLanguageIdFromApi() {
    this.isLoading = true;
    this.statusMessage = 'Fetching URLs...';

    this.http
      .get<any>(
        `https://tcma-api-public.dev.usms.impartner.io/showcase/urls?ProjectSlug=${this.form.value.projectSlug}&LanguageId= ${this.form.value.languageId}&AccountId=147347`
      )
      .subscribe({
        next: (data) => {
          this.fetchedUrls = data;
          this.form.patchValue({ urls: data.join(', ') });
          this.statusMessage = ' URLs loaded successfully.';
          this.addLog('Fetched ' + data.length + ' URLs');
        },
        error: (err) => {
          this.statusMessage = 'Failed to fetch URLs';
          this.addLog('Error fetching URLs: ' + err.message, 'error');
        },
        complete: () => (this.isLoading = false),
      });
  }

  async onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.isLoading = true;
    this.statusMessage = 'Processing...';
    this.resultList = [];
    const formValue = this.form.value;
    const urlsArray = formValue.urls
      ? formValue.urls
          .split(',')
          .map((url: string) => url.trim())
          .filter(Boolean)
      : [];
    let screenshotCount = 1;
    function chunkArray(array: any[], chunkSize: number) {
      const chunks = [];
      for (let i = 0; i < array.length; i += chunkSize) {
        chunks.push(array.slice(i, i + chunkSize));
      }
      return chunks;
    }

    const chunks = chunkArray(urlsArray, 10);

    for (const chunk of chunks) {
      const promises = chunk.map((url, index) => {
        const queryString = new URLSearchParams({
          url: url,
          width: formValue.width || '',
          customCss: formValue.customCss || '',
          folderPath: formValue.folderPath || '',
          delay: formValue.delay || 0,
        }).toString();

        const fullUrl = `https://localhost:7156/api/screen-shot?${queryString}`;

        return this.http
          .get(fullUrl)
          .toPromise()
          .then((res: any) => {
            this.addLog(`${screenshotCount++} Screenshot saved: ${res?.path}`);
          })
          .catch((err) => {
            this.addLog(`Error on ${url}: ${JSON.stringify(err)}`);
          });
      });
      await Promise.all(promises);
    }
    this.statusMessage = 'Processed successfully';

    setTimeout(() => {
      this.isLoading = false;
      this.statusMessage = '';
    }, 2000);
  }

  onReset() {
    this.form.reset();
    this.statusMessage = '';
    this.isLoading = false;
    this.logs = [];
  }
  chooseFolder(event: any) {
    const files = event.target.files;
    if (files && files.length > 0) {
      const firstFilePath = files[0].webkitRelativePath;
      const lastSlashIndex = firstFilePath.lastIndexOf('/');
      const folderPath = firstFilePath.substring(0, lastSlashIndex);
      this.form.patchValue({ folderPath });
      console.log('Selected folder:', folderPath);
    }
  }

  addLog(message: string, type: 'info' | 'error' = 'info') {
    const now = new Date();
    const timeStr = now.toLocaleTimeString();
    this.logs.push({ type, message: `[${timeStr}] ${message}` });
  }
}
