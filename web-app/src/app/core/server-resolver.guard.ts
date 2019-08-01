import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ServerManagerService } from './server-manager.service';

@Injectable({
  providedIn: 'root'
})
export class ServerResolverGuard implements CanActivate {
  constructor(
    private serverManager: ServerManagerService,
    private router: Router,
  ) {}
  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | UrlTree | Observable<boolean | UrlTree> | Promise<boolean | UrlTree> {
    return new Observable((obs) => {
      this.serverManager.getAllServer().subscribe((servers) => {
        if (this.serverManager.servers.length == 0) {
          obs.next(false);
          obs.complete();
          this.router.navigateByUrl('/register');
        } else {
          obs.next(true);
          obs.complete();
        }
      })
    })
  }
  
}
