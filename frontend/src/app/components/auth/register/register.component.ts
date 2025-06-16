// register.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { HttpClientService } from '../../../core/services/http-client.service';
import { catchError, take, throwError } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { RegisterRequest } from '../../../models/AuthModels';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  registerForm: FormGroup;
  testMessage: string = "TEST";
  constructor(private fb: FormBuilder, private httpService: HttpClientService, private toastr: ToastrService) {
    this.registerForm = this.fb.group({
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
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
          this.toastr.error(err?.error?.message ?? "some error occured");
          return throwError(()=> new Error("errorrr"));
        })
      ).subscribe({
        next: (res: any) => {
          this.toastr.show(res?.message ?? "Front-end: registered");
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
}
