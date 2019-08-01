import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { ServerManagerService } from 'src/app/core/server-manager.service';
import { Server } from 'src/app/core/models/server';
import { ServerDataService } from 'src/app/core/server-data.service';
import { LocalStorageService } from 'ngx-webstorage';
import { ShortcutEventOutput, AllowIn } from 'ng-keyboard-shortcuts';
import { Router } from '@angular/router';

@Component({
  selector: 'side-nav',
  templateUrl: './side-nav.component.html',
  styleUrls: ['./side-nav.component.scss']
})
export class SideNavComponent implements OnInit {
  shortcuts: any;
  webAPIs: any;
  models: any;
  search: string;
  expantion: { [key: string]: boolean } = {};
  @ViewChild('searchInput', {static: true}) searchElem: ElementRef;
  constructor(
    public sd: ServerDataService,
    private storage: LocalStorageService,
    private router: Router,
  ) { }

  ngOnInit() {
    this.webAPIs = this.sd.data.WebApiDetailsInfo;
    this.models = this.sd.data.ProjectDomainDetailsInfo;

    let localExpantion = this.storage.retrieve(`expantion-${this.sd.server.id}`);
    if (localExpantion) {
      this.expantion = localExpantion;
      for (const api of this.webAPIs.HttpControllers) {
        api.IsExpanded = this.expantion[api.Id];
      }
      for (const model of this.models.Models) {
        model.IsExpanded = this.expantion[model.Id];
      }
    }

    this.shortcuts = [
      {
        key: 'ctrl + shift + f',
        allowIn: [AllowIn.Textarea, AllowIn.Input, AllowIn.Select], 
        command: (output: ShortcutEventOutput) => {
          this.searchElem.nativeElement.focus();
        },
      }
    ]
    
  }

  toggleExpand(item, name: string) {
    item.IsExpanded = !item.IsExpanded;
    this.expantion[item.Id] = item.IsExpanded;
    this.storage.store(`expantion-${this.sd.server.id}`, this.expantion);
    // this.serverManager.save();
  }

  autoCompleteSelect(data: any) {
    this.search = undefined;
    console.log(data);
    if (data.MethodName) {
      this.router.navigate(['/server', this.sd.server.id, 'method', data.Id]);
    } else {
      this.router.navigate(['/server', this.sd.server.id, 'model', data.Id]);
    }
  }

}
