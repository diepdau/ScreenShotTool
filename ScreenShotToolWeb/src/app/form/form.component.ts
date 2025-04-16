import { Component } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
const httpOptions = {
  headers: new HttpHeaders({ 'Content-Type': 'Application/json' }),
};

@Component({
  selector: 'app-form',
  standalone: false,
  templateUrl: './form.component.html',
  styleUrl: './form.component.css',
})
export class FormComponent {
  form!: FormGroup;
  fetchedUrls: string[] = [];
  constructor(
    private fb: FormBuilder,
    private httpClient: HttpClient,
    private http: HttpClient
  ) {
    this.form = this.fb.group({
      api: ['', [Validators.required, this.urlValidator]],
      urls: ['', Validators.required],
      customCss: [''],
      delay: ['', Validators.min(0)],
      width: ['', Validators.min(300)],
      folderPath: ['', Validators.required],
    });
  }
  urlValidator(control: AbstractControl): ValidationErrors | null {
    const url = control.value;
    try {
      new URL(url);
      return null;
    } catch {
      return { invalidUrl: true };
    }
  }

  fetchUrlsFromApi() {
    const apiUrl = this.form.get('api');
    if (!apiUrl) return;

    apiUrl.markAsTouched();

    if (apiUrl.invalid) {
      alert('Please enter a valid url.');
      return;
    }

    const api = apiUrl.value;

    this.http.get<any>(api).subscribe(
      (res) => {
        const urls = res[0]?.url || [];
        this.fetchedUrls = urls;
        this.form.patchValue({ urls: urls.join(', ') });
        alert('Get URLs API success');
      },
      (err) => {
        alert('Error fetch URLs from API.');
        console.error('Error:', err);
      }
    );
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const formValue = this.form.value;

    const urlsArray = formValue.urls
      ? formValue.urls
          .split(',')
          .map((url: string) => url.trim())
          .filter(Boolean)
      : [];

    for (const url of urlsArray) {
      const queryString = new URLSearchParams({
        url: url,
        width: formValue.width || '',
        customCss: formValue.customCss || '',
        folderPath: formValue.folderPath || '',
      }).toString();

      const fullUrl = `https://localhost:7156/api/screen-shot?${queryString}`;

      this.http.get(fullUrl).subscribe(
        (res) => {
          alert(
            ` Screenshot done for: ${url}\n\nResponse:\n${JSON.stringify(
              res,
              null,
              2
            )}`
          );
        },
        (err) => {
          alert(
            `Error with: ${url}\n\nDetails:\n${JSON.stringify(err, null, 2)}`
          );
        }
      );
    }
  }
  onReset() {
    this.form.reset();
  }
  chooseFolder(event: any) {
    const files = event.target.files;
    if (files && files.length > 0) {
      const firstFilePath = files[0].webkitRelativePath;
      const folderPath = firstFilePath.split('/')[0];
      this.form.patchValue({ folderPath });
      console.log('selected folder:', folderPath);
    }
  }
}
