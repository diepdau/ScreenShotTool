import { Component } from '@angular/core';
import { NotificationService } from '../services/notification.service';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { UrlService } from '../services/url.service';
import { ScreenshotService } from '../services/screenshot.service';
@Component({
  selector: 'app-form',
  standalone: false,
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.css'],
})
export class FormComponent {
  form: FormGroup;
  isLoading = false;
  statusMessage = '';
  logs: { type: 'info' | 'error'; message: string }[] = [];
  fetchedUrls: string[] = [];

  languageList: { languageName: string; languageId: string }[] = [];
  accountList: { accountName: string; accountId: string }[] = [];
  urlRegex =
    /^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$/;

  constructor(
    private fb: FormBuilder,
    private urlService: UrlService,
    private screenshotService: ScreenshotService,
    private notificationService: NotificationService
  ) {
    this.form = this.fb.group(
      {
        api: ['', Validators.pattern(this.urlRegex)],
        urls: [''],
        customCss: [''],
        delay: ['', Validators.min(0)],
        width: ['', Validators.min(300)],
        folderPath: ['', [Validators.required, this.folderPathValidator()]],
        projectSlug: ['rv-portal-template'],
        languageId: ['1'],
        accountId: ['147347'],
      },
      {
        validators: this.atLeastOneFieldValidator,
      }
    );

    this.form.get('urls')?.valueChanges.subscribe((value: string) => {
      if (!value) return;

      const lines = value
        .split('\n')
        .map((line) => line.trim())
        .filter((line) => line !== '');

      const formatted = lines
        .map((line) => {
          if (!line.startsWith('http://') && !line.startsWith('https://')) {
            return 'https://' + line;
          }
          return line;
        })
        .join('\n');

      if (formatted !== value.trim()) {
        this.form.get('urls')?.patchValue(formatted, { emitEvent: false });
      }
    });
  }
  atLeastOneFieldValidator(group: FormGroup): ValidationErrors | null {
    const api = group.get('api')?.value?.trim();
    const urls = group.get('urls')?.value?.trim();
    if (!api && !urls) {
      return { requireOne: true };
    }
    if (api && !urls) {
      return { requireOne: true };
    }
    return null;
  }
  folderPathValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const path = control.value;
      const isValid = /^[a-zA-Z]:\\(?:[^\\/:*?"<>|\r\n]+\\?)*$/.test(path);
      return isValid ? null : { invalidFolderPath: true };
    };
  }

  fetchUrlsFromApi() {
    const apiUrl = this.form.value.api;

    if (!apiUrl || apiUrl.trim() === '') {
      this.notificationService.error('API URL is required.');
      this.addLog('Attempted to fetch URLs without API URL');
      return;
    }
    this.parseUrlAndSetFormValues(this.form.value.api);
    this.urlService.fetchUrlsFromApi(apiUrl).subscribe({
      next: (data) => {
        this.fetchedUrls = data;
        this.form.patchValue({ urls: data.join(', ') });
        this.notificationService.success('URLs loaded successfully.');
        this.addLog('Fetched ' + data.length + ' URLs');
      },
      error: (err) => {
        this.notificationService.error('Failed to fetch URLs');
        this.addLog('Error fetching URLs: ' + err.message, 'error');
      },
    });
  }
  // async onSubmit() {
  //   if (this.form.invalid) {
  //     this.form.markAllAsTouched();
  //     return;
  //   }

  //   this.isLoading = true;
  //   this.statusMessage = 'Processing screenshot...';

  //   const urlsArray = this.form.value.urls
  //     .split(',')
  //     .map((url: string) => url.trim())
  //     .filter(Boolean);

  //   const invalidUrls = urlsArray.filter(
  //     (url: any) => !this.urlRegex.test(url)
  //   );
  //   if (invalidUrls.length > 0) {
  //     this.isLoading = false;
  //     this.notificationService.error(
  //       `The following URLs are invalid: \n\n${invalidUrls.join('\n')}`
  //     );
  //     this.statusMessage = 'Please check the urls again...';
  //     this.addLog(`Invalid URLs: ${invalidUrls.join('\n')}`, 'error');
  //     return;
  //   }

  //   function chunkArray(array: any[], chunkSize: number) {
  //     const chunks = [];
  //     for (let i = 0; i < array.length; i += chunkSize) {
  //       chunks.push(array.slice(i, i + chunkSize));
  //     }
  //     return chunks;
  //   }

  //   let screenshotCount = 1;
  //   const chunks = chunkArray(urlsArray, 10);
  //   for (const chunk of chunks) {
  //     const promises = chunk.map(async (url) => {
  //       const payload = {
  //         url: url,
  //         width: this.form.value.width || null,
  //         customCss: this.form.value.customCss || null,
  //         folderPath: this.form.value.folderPath || null,
  //         delay: this.form.value.delay || null,
  //         languageId: this.form.value.languageId || '1',
  //         accountId: this.form.value.accountId || '147347',
  //         projectSlugs: this.form.value.projectSlug || 'rv-portal-template',
  //       };

  //       // try {
  //       //   const result = await this.screenshotService.captureScreenshot(
  //       //     payload
  //       //   );

  //       //   if (result.message.includes('giống')) {
  //       //     this.addLog(`${screenshotCount++} Duplicate for ${url}`, 'info');
  //       //     this.notificationService.success('Duplicate image');
  //       //     return;
  //       //   }
  //       //   if (result.message.includes('Timeout')) {
  //       //     this.addLog(`${screenshotCount++} Timeout ${url}`, 'info');
  //       //     this.notificationService.success(
  //       //       'Timeout image - Cannot be captured '
  //       //     );
  //       //     return;
  //       //   }
  //       //   this.addLog(`${screenshotCount++} Captured for ${url}`);
  //       //   // this.notificationService.success(result.message);
  //       // } catch (error: any) {
  //       //   this.addLog(
  //       //     `${screenshotCount++} Error capturing ${url}: ${JSON.stringify(
  //       //       error
  //       //     )}`,
  //       //     'error'
  //       //   );
  //       //   this.notificationService.error('Error when taking photos');
  //       // }

  //     });
  //     await Promise.all(promises);
  //   }

  //   this.isLoading = false;
  //   this.statusMessage = '';
  //   this.notificationService.success('Photo processing complete');
  // }
  async onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.statusMessage = 'Processing screenshot...';

    const urlsArray = this.form.value.urls
      .split(',')
      .map((url: string) => url.trim())
      .filter(Boolean);

    const invalidUrls = urlsArray.filter(
      (url: any) => !this.urlRegex.test(url)
    );

    if (invalidUrls.length > 0) {
      this.isLoading = false;
      this.notificationService.error(
        `The following URLs are invalid: \n\n${invalidUrls.join('\n')}`
      );
      this.statusMessage = 'Please check the URLs again...';
      this.addLog(`Invalid URLs: ${invalidUrls.join('\n')}`, 'error');
      return;
    }

    function chunkArray(array: any[], chunkSize: number) {
      const chunks = [];
      for (let i = 0; i < array.length; i += chunkSize) {
        chunks.push(array.slice(i, i + chunkSize));
      }
      return chunks;
    }

    let screenshotCount = 1;
    const chunks = chunkArray(urlsArray, 10);
    for (const chunk of chunks) {
      const promises = chunk.map(async (url) => {
        const payload = {
          url: url,
          width: this.form.value.width || null,
          customCss: this.form.value.customCss || null,
          folderPath: this.form.value.folderPath || null,
          delay: this.form.value.delay || null,
          languageId: this.form.value.languageId || '1',
          accountId: this.form.value.accountId || '147347',
          projectSlugs: this.form.value.projectSlug || 'rv-portal-template',
        };

        try {
          const result = await this.screenshotService
            .captureScreenshot(payload)
            .toPromise();

          if (!result || !result.message) {
            this.addLog(
              `${screenshotCount++} Error capturing for ${url}: Result is undefined or invalid`,
              'error'
            );
            this.notificationService.error(
              'Error: Invalid response from the server'
            );
            return;
          }
          if (result.message.includes('same')) {
            this.addLog(`${screenshotCount++} Duplicate for ${url}`, 'info');
            this.notificationService.success('Duplicate image');
            return;
          }

          if (result.message.includes('Timeout')) {
            this.addLog(`${screenshotCount++} Timeout for ${url}`, 'info');
            this.notificationService.success(
              'Timeout image - Cannot be captured'
            );
            return;
          }

          this.addLog(`${screenshotCount++} Captured for ${url}`);
          this.notificationService.success(result.message);
        } catch (error: any) {
          this.addLog(
            `${screenshotCount++} Error capturing for ${url}: ${JSON.stringify(
              error
            )}`,
            'error'
          );
          this.notificationService.error('Error when taking photos');
        }
      });

      await Promise.all(promises);
    }
    this.isLoading = false;
    this.statusMessage = '';
    this.notificationService.success('Photo processing complete');
  }

  onReset() {
    this.form.reset();
    this.statusMessage = '';
    this.isLoading = false;
    this.logs = [];
  }
  addLog(message: string, type: 'info' | 'error' = 'info') {
    const now = new Date();
    const timeStr = now.toLocaleTimeString();
    this.logs.push({ type, message: `[${timeStr}] ${message}` });
  }
  parseUrlAndSetFormValues(fullUrl: string) {
    const url = new URL(fullUrl);
    const projectSlug = url.searchParams.get('ProjectSlug') || '';
    const languageId = url.searchParams.get('LanguageId') || '';
    const accountId = url.searchParams.get('AccountId') || '';

    this.form.patchValue({
      projectSlug,
      languageId,
      accountId,
    });
    this.form.get('projectSlug')?.setValue(projectSlug);
    this.form.get('LanguageId')?.setValue(languageId);
    this.form.get('AccountId')?.setValue(accountId);
  }
  ChooseFolder() {
    this.screenshotService.fetchUrlChooseFile().subscribe({
      next: (data) => {
        this.form.get('folderPath')?.setValue(data.folderPath);
      },
      error: (err) => {
        console.log(err);
      },
    });
  }
}
