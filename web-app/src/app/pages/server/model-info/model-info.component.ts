import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ServerDataService } from 'src/app/core/server-data.service';

@Component({
  selector: 'model-info',
  templateUrl: './model-info.component.html',
  styleUrls: ['./model-info.component.scss']
})
export class ModelInfoComponent implements OnInit {
  model: any;
  parameters: any[] = [];
  constructor(
    private route: ActivatedRoute,
    private sd: ServerDataService,
  ) { }

  ngOnInit() {
    this.route.params.subscribe((params) => {
      let id = Number(params.id);
      this.parameters = [];
      this.model = this.sd.data.ProjectDomainDetailsInfo.Models.find(x => x.Id == id);
      if (this.model && this.model.JsonTemplate) {
        let params = JSON.parse(this.model.JsonTemplate);
        let isArray = Array.isArray(params);
        for (const key in params) {
          if (isArray) {
            let v = params[key].split('=');
            this.parameters.push({ Name: v[0].trim(), Type: v[1].trim() });
          } else {
            this.parameters.push({ Name: key, Type: params[key] });
          }
        }
      }
    });
  }

}
