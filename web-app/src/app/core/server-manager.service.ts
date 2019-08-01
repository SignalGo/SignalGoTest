import { Injectable } from '@angular/core';
import { LocalStorageService } from 'ngx-webstorage';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { Server } from './models/server';
import { NgxIndexedDB } from 'ngx-indexed-db';

@Injectable({
  providedIn: 'root'
})
export class ServerManagerService {
  servers: Server[];
  serversSubject = new Subject<Server[]>();
  selectedServer: Server;

  public db = new NgxIndexedDB('myDb', 1);

  constructor(
    private http: HttpClient,
  ) {
    this.db.openDatabase(1, evt => {
      let serverStore = evt.currentTarget.result.createObjectStore('server', { keyPath: 'id', autoIncrement: true });
      serverStore.createIndex('name', 'name', { unique: true });

      
      let callStore = evt.currentTarget.result.createObjectStore('call', { keyPath: 'id', autoIncrement: true });
      callStore.createIndex('serverId', 'serverId', { unique: false });
      callStore.createIndex('methodId', 'methodId', { unique: false });
      callStore.createIndex('date', 'date', { unique: false });
      
    }).then(() => {
      this.db.getAll('server').then((servers) => {
        this.servers = servers;
        this.serversSubject.next(this.servers);
        this.serversSubject.complete();
      });
    });
  }

  getAllServer(): Observable<Server[]> {
    return new Observable((obs) => {
      if (this.servers) {
        obs.next(this.servers);
        obs.complete();
      } else {
        this.serversSubject.subscribe((servers) => {
          obs.next(servers);
        })
      }
    });
  }

  register(name: string, url: string): Observable<number> {
    if (url[url.length-1] == '/') url = url.substr(0, url.length-1);
    return new Observable((obs) => {
      this.http.get(url + '/any?a=1', {
        headers: {
          'signalgo-servicedetail': 'full'
        }
      }).subscribe((response) => {
        let server = new Server();
        server.name = name;
        server.url = url;
        this.db.add('server', server).then(() => {
          let id = this.servers && this.servers.length > 0 ? this.servers[this.servers.length - 1].id + 1 : 1;
          server.id = id;
          this.servers.push(server);
          obs.next(server.id);
          obs.complete();
        }).catch((error) => {
          obs.error(error);
        });
      }, (error) => {
        obs.error(error);
      })
    })
  }
}