import { Injectable } from '@angular/core';
import { ElectronService } from 'ngx-electron';

@Injectable({
  providedIn: 'root',
})
export class FileDialogService {
  constructor(private electronService: ElectronService) {}
  //cd
  selectFolder(): Promise<string | undefined> {
    return new Promise<string>((resolve, reject) => {
      this.electronService.remote.dialog
        .showOpenDialog({
          properties: ['openDirectory'],
        })
        .then((result: Electron.OpenDialogReturnValue) => {
          if (!result.canceled) {
            resolve(result.filePaths[0]);
          } else {
            reject('No folder selected');
          }
        })
        .catch((err: any) => reject(err));
    });
  }
}
