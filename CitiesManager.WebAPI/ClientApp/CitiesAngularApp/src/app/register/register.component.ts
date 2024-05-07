import { Component } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../services/account.service';
import { Router } from '@angular/router';
import { compareValidatorFactory } from '../validators/compare-validator-factory';
import { existingEmailValidatorFactory } from '../validators/existing-email-validator-factory';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {

  registerForm: FormGroup;

  constructor(private accountService: AccountService, private router: Router, private http: HttpClient) {

  }
  ngOnInit() {
    this.registerForm = new FormGroup({
      'personName': new FormControl(null, [Validators.required]),
      'email': new FormControl(null, [Validators.required, Validators.email], [existingEmailValidatorFactory(this.http)]),
      'phoneNumber': new FormControl(null, [Validators.required]),
      'password': new FormControl(null, [Validators.required]),
      'confirmPassword': new FormControl(null, [Validators.required])
    }, [compareValidatorFactory('password', 'confirmPassword')]);
  }

  onSubmit() {
    console.log(this.registerForm);
    this.accountService.postRegister(this.registerForm.value).subscribe({
      next: () => {
        this.router.navigate(['/cities']);
        this.registerForm.reset();

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
