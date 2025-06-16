import {
  HttpEvent,
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
  HttpResponse,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, map, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { inject } from '@angular/core';
import { Router } from '@angular/router';

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  const router = inject(Router);
  
  // Get the token from localStorage
  const token = localStorage.getItem('access_token');
  
  // Check if this is an API request (not an auth request)
  const isAuthRequest = req.url.includes(`${environment.authUrl}/connect/token`);
  const isApiRequest = req.url.includes(environment.apiUrl);
  
  // For Docker environment, also check the Docker URLs
  const isDockerAuthRequest = environment.dockerApiUrl && req.url.includes(`${environment.dockerAuthUrl}/connect/token`);
  const isDockerApiRequest = environment.dockerApiUrl && req.url.includes(environment.dockerApiUrl);
  
  // Only add token to API requests and not to auth requests
  if (token && (isApiRequest || isDockerApiRequest) && !(isAuthRequest || isDockerAuthRequest)) {
    const clonedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    
    return next(clonedRequest).pipe(
      map((event: HttpEvent<unknown>) => {
        if (event instanceof HttpResponse) {
          // Success response handling if needed
        }
        return event;
      }),
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Navigate to login page on authentication error
          router.navigate(['/login']);
        }
        return throwError(() => error);
      })
    );
  }
  
  // For auth requests or when no token exists, send the original request
  return next(req).pipe(
    map((event: HttpEvent<unknown>) => {
      if (event instanceof HttpResponse) {
        // Success response handling if needed
      }
      return event;
    })
  );
}; 