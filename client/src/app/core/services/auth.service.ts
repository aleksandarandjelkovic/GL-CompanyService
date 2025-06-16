import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

interface AuthResponse {
  access_token: string;
  expires_in: number;
  token_type: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private authUrl = environment.authUrl;
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());
  
  isAuthenticated$ = this.isAuthenticatedSubject.asObservable();
  
  constructor(private http: HttpClient) { }
  
  login(username: string, password: string): Observable<AuthResponse> {
    const body = new HttpParams()
      .set('grant_type', 'client_credentials')
      .set('client_id', 'swagger')
      .set('client_secret', 'secret')
      .set('scope', 'companyapi');
      
    const headers = new HttpHeaders({
      'Content-Type': 'application/x-www-form-urlencoded'
    });
    
    // Use the appropriate auth URL based on environment
    const tokenUrl = `${this.authUrl}/connect/token`;
    
    return this.http.post<AuthResponse>(tokenUrl, body.toString(), { headers }).pipe(
      tap(response => {
        localStorage.setItem('access_token', response.access_token);
        this.isAuthenticatedSubject.next(true);
      })
    );
  }
  
  logout(): void {
    localStorage.removeItem('access_token');
    this.isAuthenticatedSubject.next(false);
  }
  
  private hasToken(): boolean {
    const token = localStorage.getItem('access_token');
    return !!token;
  }
} 