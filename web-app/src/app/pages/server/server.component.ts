import { Component, OnInit } from '@angular/core';
import { ServerManagerService } from 'src/app/core/server-manager.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Server } from 'src/app/core/models/server';
import { ServerDataService } from 'src/app/core/server-data.service';

@Component({
  selector: 'app-server',
  templateUrl: './server.component.html',
  styleUrls: ['./server.component.scss']
})
export class ServerComponent implements OnInit {
  constructor(
    public serverManager: ServerManagerService,
    private route: ActivatedRoute,
    private router: Router,
    public sd: ServerDataService,
  ) { }

  ngOnInit() {
    this.route.params.subscribe((params) => {
      try {
        this.sd.load(this.serverManager.servers.find(x => x.id == Number(params.id)));
      } catch (error) {
        this.router.navigate(['/']);
      }
    })
  }

}
