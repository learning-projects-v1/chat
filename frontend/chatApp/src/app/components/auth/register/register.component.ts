// register.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { HttpClientService } from '../../../core/services/http-client.service';
import { catchError, take, throwError } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { RegisterRequest } from '../../../models/AuthModels';
import { Router } from '@angular/router';
import { ErrorMessageService } from '../../../core/services/error-message.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  private readonly passwordPattern = /^(?=.*[A-Z])(?=.*\d).+$/;
  registerForm: FormGroup;
  testMessage: string = "TEST";
  constructor(
    private fb: FormBuilder,
    private httpService: HttpClientService,
    private toastr: ToastrService,
    private router: Router,
    private errorMessageService: ErrorMessageService
  ) {
    this.registerForm = this.fb.group({
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6), Validators.pattern(this.passwordPattern)]]
    });
  }

  get passwordControl() {
    return this.registerForm.get('password');
  }

  onSubmit() {
    if (this.registerForm.valid) {
      const value = this.registerForm.value;
      const request : RegisterRequest = {
        Email: value.email,
        Password: value.password,
        UserName: value.username,
      };
      console.log('Register Data:', value);
      this.httpService.register(request).pipe(
        take(1),
        catchError((err) => {
          if (this.errorMessageService.isDuplicateUserError(err)) {
            this.toastr.error('User already exists. Please login instead.');
          } else {
            this.toastr.error(
              this.errorMessageService.getFriendlyMessage(
                err,
                'Registration failed. Please try again.'
              )
            );
          }
          return throwError(() => err);
        })
      ).subscribe({
        next: (res: any) => {
          this.toastr.success(res?.message ?? "Registered successfully.");
          this.router.navigate(['/login']);
        },
        error: (err: any) => {
          console.error(`error: ${err.message}`);
        }
      })
    }
  }
  test(){
    this.httpService.test(this.testMessage).subscribe((res: any) => {
      console.log(res);
      this.toastr.show(res?.message);
    })
  }

goToLogin(): void {
  this.router.navigate(['/login']);
}
}
