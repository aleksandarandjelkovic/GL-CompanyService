import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <mat-toolbar color="primary" class="app-toolbar">
      <span routerLink="/" class="app-name">Company Management</span>
      <span class="spacer"></span>
      <ng-container *ngIf="isAuthenticated">
        <button mat-button routerLink="/companies">
          <mat-icon>business</mat-icon>
          Companies
        </button>
        <button mat-button (click)="logout()">
          <mat-icon>logout</mat-icon>
          Logout
        </button>
      </ng-container>
      <button mat-button *ngIf="!isAuthenticated && !isLoginPage" (click)="loginDirectly()">
        <mat-icon>login</mat-icon>
        Login
      </button>
    </mat-toolbar>
    
    <div class="app-content">
      <router-outlet></router-outlet>
    </div>
  `,
  styles: [`
    .app-toolbar {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      z-index: 100;
    }
    
    .app-name {
      cursor: pointer;
      font-weight: 500;
    }
    
    .spacer {
      flex: 1 1 auto;
    }
    
    .app-content {
      margin-top: 64px; /* Height of toolbar */
      min-height: calc(100vh - 64px);
      padding: 0;
      box-sizing: border-box;
    }
    
    @media (max-width: 600px) {
      .app-content {
        margin-top: 56px; /* Height of toolbar on small screens */
        min-height: calc(100vh - 56px);
      }
    }
  `]
})
export class AppComponent implements OnInit {
  title = 'Company Management';
  isAuthenticated = false;
  isLoginPage = false;
  
  constructor(private authService: AuthService, private router: Router) {}
  
  ngOnInit(): void {
    // Add a global error handler to catch any issues
    window.onerror = (message, source, lineno, colno, error) => {
      return false;
    };
    
    this.authService.isAuthenticated$.subscribe(isAuthenticated => {
      this.isAuthenticated = isAuthenticated;
    });
    
    // Subscribe to router events to track current URL
    this.router.events.subscribe(() => {
      this.isLoginPage = this.router.url === '/login';
    });
  }
  
  loginDirectly(): void {
    this.authService.login('', '').subscribe({
      next: () => this.isAuthenticated = true,
      error: () => this.isAuthenticated = false
    });
  }
  
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
} 