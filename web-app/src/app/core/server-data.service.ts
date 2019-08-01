import { Injectable } from '@angular/core';
import { Server } from './models/server';
import { HttpClient } from '@angular/common/http';
import { SignalgoService } from './signalgo.service';
import { ServerManagerService } from './server-manager.service';
import { Observable } from 'rxjs';
import { Call } from './models/call';
import { ServerHistoryService } from './server-history.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class ServerDataService {
  server: Server;
  data: any;
  lastCall: Call;
  constructor(
    private http: HttpClient,
    private signalGo: SignalgoService,
    private serverManager: ServerManagerService,
    private history: ServerHistoryService,
    private snack: MatSnackBar,
    private router: Router,
  ) {
    // this.serverManager.db.
  }

  load(server: Server) {
    this.server = server;
    this.http.post(server.url + '/any', { a: 1 }, {
      headers: {
        'signalgo-servicedetail': 'full',
      },
      withCredentials: true,
    }).subscribe((response) => {
      let res = this.signalGo.deserializeReferences(response)
      console.info('server', res);
      this.data = res;
    }, (error) => {
      // obs.error(error);
    })
  }

  call(object: Call, skipSave?: boolean): Observable<any> {
    this.snack.open(`${object.name} pending...`);
    return new Observable((obs) => {
      this.http.post(object.url, object.body, {
        withCredentials: true,
      }).subscribe((res) => {
        object.response = res;
        this.snack.open(`${object.name} success`, 'view response', {
          duration: 3000,
        }).onAction().subscribe(() => {
          this.router.navigate(['/server', object.serverId, 'method', object.methodId, object.id]);
        });
        obs.next(res);
        obs.complete();
        try {
          if (!skipSave) {
            this.history.saveCall(object);
          }
        } catch (error) { }
        this.lastCall = object;
      }, (error) => {
        error.headers = undefined;
        object.error = error;
        this.snack.open(`${object.name} error`, 'view error', {
          duration: 3000,
        }).onAction().subscribe(() => {
          this.router.navigate(['/server', object.serverId, 'method', object.methodId, object.id]);
        });
        obs.error(error);
        try {
          if (!skipSave) {
            this.history.saveCall(object);
          }
        } catch (error) { }
        this.lastCall = object;
      });
    });
  }
}
