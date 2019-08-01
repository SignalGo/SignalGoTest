import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ServerResolverGuard } from './core/server-resolver.guard';

const routes: Routes = [
  { path: 'register', loadChildren: () => import('./auth/auth.module').then(m => m.AuthModule)},
  { path: 'server', loadChildren: () => import('./pages/pages.module').then(m => m.PagesModule), canActivate: [ServerResolverGuard] },
  {
    path: '',
    redirectTo: '/register',
    pathMatch: 'full'
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {useHash: true})],
  exports: [RouterModule]
})
export class AppRoutingModule { }
