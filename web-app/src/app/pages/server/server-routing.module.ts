import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ServerComponent } from './server.component';
import { MethodInfoComponent } from './method-info/method-info.component';
import { ModelInfoComponent } from './model-info/model-info.component';

const routes: Routes = [
  {
    path: ':id', component: ServerComponent,
    children: [
      {
        path: 'model/:id',
        component: ModelInfoComponent,
      },
      {
        path: 'method/:id',
        component: MethodInfoComponent,
      },
      {
        path: 'method/:id/:callId',
        component: MethodInfoComponent,
      }
    ]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ServerRoutingModule { }
