import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ServerDataService } from 'src/app/core/server-data.service';
import { Call } from 'src/app/core/models/call';
import { ServerHistoryService } from 'src/app/core/server-history.service';

@Component({
  selector: 'method-info',
  templateUrl: './method-info.component.html',
  styleUrls: ['./method-info.component.scss']
})
export class MethodInfoComponent implements OnInit {
  method: any;
  parameters: any[];
  calling: boolean;
  response: any;
  error: any;

  callParams: any = {};
  constructor(
    private route: ActivatedRoute,
    private sd: ServerDataService,
    private history: ServerHistoryService,
  ) { }

  ngOnInit() {
    this.route.params.subscribe((params) => {
      let id = Number(params.id);
      let callId: number;
      if (params.callId) {
        callId = Number(params.callId);
      }
      this.method = undefined;
      this.response = undefined;
      this.error = undefined;
      this.callParams = {};
      this.load(id, callId);
    });
  }

  load(id: number, callId?: number) {
    for (const controller of this.sd.data.WebApiDetailsInfo.HttpControllers) {
      if (this.method) break;
      for (const method of controller.Methods) {
        if (method.Id == id) {
          this.method = method;
          this.parameters = this.method.Requests[0].Parameters;
          this.setParamTypes();
          break;
        }
      }
    }

    if (callId) {
      this.setCallData(callId);
    }
  }

  private setParamTypes() {
    for (const p of this.parameters) {
      let type = this.getBaseType(p.Type);
      if (type) {
        p.type = type;
      } else {
        let model = this.sd.data.ProjectDomainDetailsInfo.Models.find(x => x.Name == p.Type);
        if (model) {
          let params = JSON.parse(model.JsonTemplate);
          let isArray = Array.isArray(params);
          if (isArray) {
            p.type = 'number';
          } else {
            p.hidden = true;
            console.log(params);
            for (const key in params) {
              let type = this.getBaseType(params[key]);
              if (type) {
                this.parameters.push({ Name: p.Name + '.' + key, type: type });
              }
            }
          }
        } else {
          p.type = "text";
        }
      }
    }
  }
  
  getBaseType(type: string) {
    switch (type) {
      case 'Int32':
      case 'UInt32':
      case 'bool':
      case 'boolean':
      case 'double':
      case 'int':
      case 'Nullable<Int32>':
      case 'Nullable<UInt32>':
      case 'Nullable<bool>':
      case 'Nullable<boolean>':
      case 'Nullable<double>':
      case 'Nullable<int>':
        return 'number';
      case 'String':
      case 'string':
      case 'Nullable<String>':
      case 'Nullable<string>':
        return 'text';
      case 'DateTime':
      case 'date':
      case 'Nullable<DateTime>':
      case 'Nullable<date>':
        return 'date';
    }
  }

  private setCallData(id: number) {
    this.history.isLoaded().subscribe(() => {
      let call = this.history.history.find(x => x.id == id);
      if (call && call.serverId == this.sd.server.id && call.methodId == this.method.Id) {
        this.callParams = call.body;
        this.response = call.response;
        this.error = call.error;
      }
    })
  }

  call() {
    this.response = undefined;
    this.error = undefined;
    let object = this.method.TestExample.split('?');
    let call: Call = {
      name: this.method.MethodName,
      url: object[0].replace(':-1', this.sd.server.url),
      date: new Date(),
      body: this.getRealParams(),
      methodId: this.method.Id,
      serverId: this.sd.server.id,
    }
    this.calling = true;
    this.sd.call(call).subscribe((res) => {
      this.calling = false;
      this.response = res;
    }, (error) => {
      this.calling = false;
      this.error = error;
    })
  }

  getRealParams(): any {
    let res = {};
    for (const key in this.callParams) {
      if (this.callParams.hasOwnProperty(key)) {
        let subs = key.split('.');
        if (subs.length > 1) {
          let parent = res[subs[0]];
          if (!parent) {
            res[subs[0]] = {};
            parent = res[subs[0]];
          }
          parent[subs[1]] = this.callParams[key];
        } 
        else {
          res[key] = this.callParams[key];
        }
        
      }
    }
    return res;
  }

}
