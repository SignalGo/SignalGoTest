import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { ServerManagerService } from 'src/app/core/server-manager.service';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  form: FormGroup;
  loading = false;
  constructor(
    public serverManager: ServerManagerService,
    private router: Router,
    private snack: MatSnackBar,
  ) { }

  ngOnInit() {
    this.form = new FormGroup({
      name: new FormControl('', [
        Validators.pattern('[a-zA-Z ]*'),
        Validators.minLength(3),
        Validators.required
      ]),
      url: new FormControl('', Validators.required)
    });
  }

  connect() {
    this.loading = true;
    let name = this.form.controls['name'].value;
    let url = this.form.controls['url'].value;
    this.serverManager.register(name, url).subscribe((id) => {
      this.router.navigate(['/server', id]);
      this.loading = false;
    }, (error) => {
      this.loading = false;
      this.snack.open('error on connecting', '', {
        duration: 3000
      });
    })
  }
  
  
}
