import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { RegisterUser } from '../models/register-user';
import { Observable, tap } from 'rxjs';
import { LoginUser } from '../models/login-user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  public isSignIn: boolean = false;
  constructor(private http: HttpClient) { }

  public postRegister(registerUser: RegisterUser) {
    return this.http.post<any>("https://localhost:7001/api/account/1/register", registerUser).pipe(tap(
      (authResponse) => {
        localStorage.setItem('auth-token', authResponse.token);
        localStorage.setItem('refresh-token', authResponse.refreshToken);
        this.isSignIn = true;
      })
    );
  }

  public postLogin(loginUser: LoginUser) {
    return this.http.post<any>("https://localhost:7001/api/account/1/login", loginUser).pipe(tap(
      (authResponse) => {
        localStorage.setItem('auth-token', authResponse.token);
        localStorage.setItem('refresh-token', authResponse.refreshToken);
        this.isSignIn = true;
      })
    );
  }

  public getLogout() {
    return this.http.get("https://localhost:7001/api/account/1/logout").pipe(tap(
      () => {
        localStorage.removeItem('auth-token');
        this.isSignIn = false;
      })
    );
  }

  public postGenerateNewToken() {
    var token = localStorage.getItem("auth-token");
    var refreshToken = localStorage.getItem("refresh-token");
    return this.http.post<any>("https://localhost:7001/api/account/1/generate-new-jwt-token", { token: token, refreshToken: refreshToken }).pipe(tap(
      (authResponse) => {
        localStorage.setItem('auth-token', authResponse.token);
        localStorage.setItem('refresh-token', authResponse.refreshToken);
        this.isSignIn = true;
      })
    );
  }
}
