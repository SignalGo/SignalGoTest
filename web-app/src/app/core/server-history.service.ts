import { Injectable } from '@angular/core';
import { ServerManagerService } from './server-manager.service';
import { Server } from './models/server';
import { Call } from './models/call';
import { Subject, Observable, of } from 'rxjs';
import { ServerDataService } from './server-data.service';

@Injectable({
  providedIn: 'root'
})
export class ServerHistoryService {
  history: Call[];
  favs: Call[] = [];
  loading = new Subject<void>();
  constructor(
    private serverManager: ServerManagerService,
    private sd: ServerDataService,
  ) {
    this.serverManager.db.getAll('call').then((calls: Call[]) => {
      this.history = calls.filter(x => x.serverId == this.sd.server.id);
      this.history.sort((a, b) => b.id - a.id);

      for (const h of this.history) {
        if (h.isFav && h.favDate) this.favs.push(h);
      }
      this.favs.sort((a, b) => b.favDate.valueOf() - a.favDate.valueOf());
      this.loading.next();
      this.loading.complete();
    });
    
  }

  saveCall(call: Call) {
    this.serverManager.db.add('call', call).then((val : any) => {
      if (this.history.length > 0) {
        call.id = this.history[0].id + 1;
      } else {
        call.id = 1;
      }
      this.history.unshift(call);
    })
  }

  toggleFav(call: Call) {
    call.isFav = !call.isFav;
    if (call.isFav) {
      call.favDate = new Date();
    } else {
      call.favDate = undefined;
    }
    this.serverManager.db.update('call', call).then(() => {
      if (call.isFav) {
        this.favs.unshift(call);
      } else {
        this.favs.splice(this.favs.indexOf(call), 1);
      }
    })
    
  }

  isLoaded(): Observable<void> {
    return new Observable((obs) => {
      if (this.history) {
        obs.next();
        obs.complete();
      } else {
        this.loading.subscribe(() => {
          obs.next();
          obs.complete();
        })
      }
    })
  }
}
