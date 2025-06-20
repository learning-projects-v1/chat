// register.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
  FormsModule,
} from '@angular/forms';
import { HttpClientService } from '../../../core/services/http-client.service';
import { catchError, take, throwError } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { LoginRequest } from '../../../models/AuthModels';
import { UserService } from '../../../core/services/auth.service';
import { Route, Router } from '@angular/router';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
})
export class LoginComponent {
  loginForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private httpService: HttpClientService,
    private toastr: ToastrService,
    private userService: UserService,
    private router: Router,
    private notificationService: NotificationService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      // password: ['', [Validators.required]]
      password: [''],
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      const value = this.loginForm.value;
      const request: LoginRequest = {
        Email: value.email,
        Password: value.password,
      };
      this.httpService
        .login(request)
        .pipe(
          take(1),
          catchError((err) => {
            this.toastr.error(err?.message ?? 'front-end: Login failed');
            return throwError(() => new Error('Login error'));
          })
        )
        .subscribe({
          next: (res: any) => {
            this.toastr.success(res?.message ?? 'Login successful');
            this.httpService.setAccessToken(res?.accessToken ?? '');
            this.userService.setUserInfo({
              Email: res?.email,
              Username: res?.username,
              UserId: res?.userId,
            });
            this.notificationService.connect(res?.accessToken, res?.userId);
            this.router.navigate(['/connections']);
          },
          error: (err) => {
            console.error('Login error:', err.message);
          },
        });
    }
  }
}
