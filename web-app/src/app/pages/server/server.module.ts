import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ServerRoutingModule } from './server-routing.module';
import { ServerComponent } from './server.component';
import { MatButtonModule } from '@angular/material/button';
import { MatRippleModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { SideNavComponent } from './side-nav/side-nav.component';
import { MethodInfoComponent } from './method-info/method-info.component';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { HistoryNavComponent } from './history-nav/history-nav.component';
import { MatTabsModule } from '@angular/material/tabs';
import { SearchPipe } from 'src/app/core/search.pipe';
import { KeyboardShortcutsModule } from 'ng-keyboard-shortcuts'; 
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ModelInfoComponent } from './model-info/model-info.component';
import { MatAutocompleteModule } from '@angular/material/autocomplete';


@NgModule({
  declarations: [ServerComponent, SideNavComponent, MethodInfoComponent, HistoryNavComponent, SearchPipe, ModelInfoComponent],
  imports: [
    CommonModule,
    ServerRoutingModule,
    FormsModule,
    KeyboardShortcutsModule.forRoot(),

    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule,
    MatRippleModule,
    MatFormFieldModule,
    MatInputModule,
    MatMenuModule,
    MatTabsModule,
    MatSlideToggleModule,
    MatAutocompleteModule,
  ]
})
export class ServerModule { }
