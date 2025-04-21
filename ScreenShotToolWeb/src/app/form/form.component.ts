import { Component } from '@angular/core';
import { NotificationService } from '../services/notification.service';
import {
  FormBuilder,
  FormGroup,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { UrlService } from '../services/url.service';
import { ScreenshotService } from '../services/screenshot.service';
import { ScreenshotResult } from '../models/screentShot.models';
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
  resultList1: string[] = [];
  screenshotBlobs: Blob[] = [];
  resultList: ScreenshotResult[] = [];

  languageList: { languageName: string; languageId: string }[] = [];
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
        folderPath: [''],
        projectSlug: [''],
        languageId: [''],
      },
      {
        validators: this.atLeastOneFieldValidator,
      }
    );
    this.setLanguageList();
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

  private setLanguageList(): void {
    this.languageList = [
      { languageName: 'German', languageId: '3' },
      { languageName: 'Hungarian', languageId: '33' },
      { languageName: 'German', languageId: '2' },
      { languageName: 'Latvian', languageId: '34' },
      { languageName: 'English Global', languageId: '1' },
      { languageName: 'Slovakian', languageId: '38' },
      { languageName: 'Canada-French', languageId: '52' },
      { languageName: 'Dutch', languageId: '11' },
    ];
  }
  fetchUrlsFromApi() {
    this.urlService.fetchUrlsFromApi(this.form.value.api).subscribe({
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

  fetchUrlsProjectLanguageIdFromApi() {
    this.urlService
      .fetchUrlsProjectLanguageIdFromApi(
        this.form.value.projectSlug,
        this.form.value.languageId
      )
      .subscribe({
        next: (data) => {
          this.fetchedUrls = data;
          this.form.patchValue({ urls: data.join(', ') });
          this.notificationService.success('URLs loaded successfully.');
          this.addLog('Fetched ' + data.length + ' URLs');
        },
        error: (err) => {
          this.notificationService.error('Failed to fetch URLs');
          this.statusMessage = '';
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
  //   this.resultList = [];
  //   this.screenshotBlobs = [];

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
  //         width: this.form.value.width,
  //         customCss: this.form.value.customCss,
  //         folderPath: this.form.value.folderPath,
  //         delay: this.form.value.delay,
  //       };

  //       try {
  //         const blob = await this.screenshotService.captureScreenshot(payload);
  //         if (blob) {
  //           this.screenshotBlobs.push(blob);
  //           this.addLog(`${screenshotCount++} Screenshot captured for ${url}`);
  //           this.resultList.push(url);
  //         } else {
  //           this.addLog(`Screenshot not captured for ${url}`, 'error');
  //         }
  //       } catch (error) {
  //         this.addLog(
  //           `${screenshotCount++} Error capturing ${url}: ${JSON.stringify(
  //             error
  //           )}`,
  //           'error'
  //         );
  //       }
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
    this.resultList = [];
    this.screenshotBlobs = [];

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
          url,
          width: this.form.value.width,
          customCss: this.form.value.customCss,
          folderPath: this.form.value.folderPath,
          delay: this.form.value.delay,
        };

        try {
          const res = await this.screenshotService.captureScreenshot(payload);
          if (res && res.base64Image) {
            this.resultList.push({
              url,
              image: res.base64Image,
              isSame: res.isSameAsPrevious,
              diff: res.diffPath,
            });
            this.addLog(`${screenshotCount++} Screenshot captured for ${url}`);
            this.resultList1.push(url);
          } else {
            this.addLog(`Screenshot not captured for ${url}`, 'error');
          }
        } catch (error) {
          this.addLog(
            `${screenshotCount++} Error capturing ${url}: ${JSON.stringify(
              error
            )}`,
            'error'
          );
        }
      });

      await Promise.all(promises);
    }

    this.isLoading = false;
    this.statusMessage = '';
    this.notificationService.success('Photo processing complete');
  }

  async saveToLocalFolder() {
    this.isLoading = true;
    this.statusMessage = 'Processing...';
    try {
      if (!this.screenshotBlobs.length || !this.resultList?.length) {
        this.addLog('Cannot find image!');
        return;
      }
      const dirHandle = await (window as any).showDirectoryPicker();
      for (let i = 0; i < this.screenshotBlobs.length; i++) {
        const blob = this.screenshotBlobs[i];
        const originalUrl = this.resultList1[i];
        let titleOfPage = 'untitled';
        try {
          const segments = new URL(originalUrl).pathname.split('/');
          const pageIndex = segments.findIndex((s) => s === 'page');
          if (pageIndex !== -1 && pageIndex + 1 < segments.length) {
            titleOfPage = segments[pageIndex + 1];
          }
        } catch (e) {
          this.notificationService.error('URL parse error');
          console.warn('URL parse error:', e);
        }

        const now = new Date();
        const timeStr = `${now.getHours()}_${now.getMinutes()}_${now.getSeconds()}`;
        const fileName = `${titleOfPage}_${timeStr}.png`;

        const fileHandle = await dirHandle.getFileHandle(fileName, {
          create: true,
        });
        const writable = await fileHandle.createWritable();
        await writable.write(blob);
        await writable.close();
      }
      this.notificationService.success(
        ` ${this.screenshotBlobs.length} images saved to the folder of your choice`
      );
      this.statusMessage = '';
      this.addLog(`Image saved to folder successfully!`);
      this.isLoading = false;
    } catch (error) {
      this.notificationService.error('Error save capture');
      this.addLog(`Error save capture ${error}`, 'error');
    }
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

  get canSave(): boolean {
    return this.screenshotBlobs.length > 0 && this.resultList.length > 0;
  }
}
