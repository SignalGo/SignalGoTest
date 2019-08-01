import { Component, OnInit } from '@angular/core';
import { ServerHistoryService } from 'src/app/core/server-history.service';
import { ServerDataService } from 'src/app/core/server-data.service';
import { Call } from 'src/app/core/models/call';
import { trigger, transition, query, style, stagger, animate } from '@angular/animations';
import { ServerManagerService } from 'src/app/core/server-manager.service';

@Component({
  selector: 'history-nav',
  templateUrl: './history-nav.component.html',
  styleUrls: ['./history-nav.component.scss'],
  animations: [
    trigger('filterAnimation', [
      transition(':enter, * => 0, * => -1', []),
      transition(':increment', [
        query(':enter', [
          style({ opacity: 0, width: '0px' }),
          stagger(50, [
            animate('300ms ease-out', style({ opacity: 1, width: '*' })),
          ]),
        ], { optional: true })
      ]),
      transition(':decrement', [
        query(':leave', [
          stagger(50, [
            animate('300ms ease-out', style({ opacity: 0, width: '0px' })),
          ]),
        ])
      ]),
    ]),
  ]
})
export class HistoryNavComponent implements OnInit {
  stringify = JSON.stringify;
  favs = false;
  constructor(
    public historyService: ServerHistoryService,
    public sd: ServerDataService,
    private serverManager: ServerManagerService,
  ) { }

  ngOnInit() {
    
  }

  call(call: Call) {
    let object: Call = {
      name: call.name,
      url: call.url,
      body: call.body,
      serverId: call.serverId,
      methodId: call.methodId,
      date: new Date(),
    }
    this.sd.call(object).subscribe();
  }

  toggleFav(call: Call, event: Event) {
    event.stopPropagation();
    event.preventDefault()

    this.historyService.toggleFav(call);
  }


}
