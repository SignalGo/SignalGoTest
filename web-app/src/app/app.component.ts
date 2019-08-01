import { Component } from '@angular/core';
import { ServerManagerService } from './core/server-manager.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  constructor(private server: ServerManagerService) {}
}
