import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="login-container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>Login</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Username</mat-label>
              <input matInput formControlName="username" type="text" autocomplete="username">
              <mat-error *ngIf="loginForm.get('username')?.hasError('required')">
                Username is required
              </mat-error>
            </mat-form-field>
            
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Password</mat-label>
              <input matInput formControlName="password" type="password" autocomplete="current-password">
              <mat-error *ngIf="loginForm.get('password')?.hasError('required')">
                Password is required
              </mat-error>
            </mat-form-field>
            
            <div class="form-actions">
              <button mat-raised-button color="primary" type="submit" [disabled]="loginForm.invalid || isLoading">
                <span *ngIf="!isLoading">Login</span>
                <mat-spinner *ngIf="isLoading" diameter="24"></mat-spinner>
              </button>
            </div>
            
            <div *ngIf="errorMessage" class="error-message">
              {{ errorMessage }}
            </div>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      justify-content: center;
      align-items: center;
      height: calc(100vh - 64px);
    }
    
    mat-card {
      width: 100%;
      max-width: 400px;
      padding: 20px;
    }
    
    .full-width {
      width: 100%;
      margin-bottom: 15px;
    }
    
    .form-actions {
      display: flex;
      justify-content: flex-end;
    }
    
    .error-message {
      color: #f44336;
      margin-top: 15px;
      text-align: center;
    }
  `]
})
export class LoginComponent {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  
  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }
  
  onSubmit(): void {
    if (this.loginForm.invalid) {
      return;
    }
    
    const { username, password } = this.loginForm.value;
    this.isLoading = true;
    this.errorMessage = '';
    
    this.authService.login(username, password).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/companies']);
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.error_description || 'Login failed. Please check your credentials.';
      }
    });
  }
} 