import { Component } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../services/account.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  loginForm: FormGroup;

  constructor(private accountService: AccountService, private router: Router) {

  }
  ngOnInit() {
    this.loginForm = new FormGroup({
      'email': new FormControl(null, [Validators.required, Validators.email]),
      'password': new FormControl(null, [Validators.required]),
    } );
  }

  onSubmit() {
    this.accountService.postLogin(this.loginForm.value).subscribe({
      next: () => {
        this.router.navigate(['/cities']);
        this.loginForm.reset();

      },
      error: (errResponse) => {
        if (errResponse.error.detail) {
          console.log('Error: ' + errResponse.error.detail);
        }
        else {
          console.log(errResponse);
        }
      }
    })
  }
}
