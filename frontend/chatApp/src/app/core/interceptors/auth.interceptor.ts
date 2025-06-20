import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { catchError, Observable, throwError } from "rxjs";
import { HttpClientService } from "../services/http-client.service";
import { Router } from "@angular/router";



@Injectable({providedIn: "root"})
export class AuthInterceptor implements HttpInterceptor{
    
    constructor(private httpService: HttpClientService, private router: Router) {
        
    }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const token = this.httpService.getAccessToken();
        let clonedRequest = req;
        if(token){
            clonedRequest = req.clone({
                setHeaders : {
                    Authorization: `Bearer ${token}`
                }
            });
        }

        return next.handle(clonedRequest).pipe(
            catchError((err: HttpErrorResponse) => {
                if(err.status == 401){
                    // this.httpService.setAccessToken("");
                    this.router.navigate(["/login"]);
                }
                return throwError(()=>err);
            })
        )
    }
}