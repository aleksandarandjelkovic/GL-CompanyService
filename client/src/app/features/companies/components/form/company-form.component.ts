import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NgIf, CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CompanyApiService } from '../../../../core/services/company-api.service';
import { Company } from '../../models/company';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-company-form',
  templateUrl: './company-form.component.html',
  styleUrls: ['./company-form.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatProgressSpinnerModule
  ]
})
export class CompanyFormComponent implements OnInit {
  companyForm: FormGroup;
  isEditMode = false;
  companyId: string | null = null;
  isLoading = false;
  isSaving = false;
  
  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private companyService: CompanyApiService,
    private snackBar: MatSnackBar
  ) {
    this.companyForm = this.buildForm();
  }

  ngOnInit(): void {
    this.determineFormMode();
  }

  private determineFormMode(): void {
    // Check if we're in edit mode (from route)
    const id = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!id;
    
    if (this.isEditMode && id) {
      this.companyId = id;
      this.loadCompany(id);
    }
  }

  buildForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required]],
      ticker: ['', [Validators.required]],
      exchange: ['', [Validators.required]],
      isin: ['', [
        Validators.required, 
        Validators.minLength(12),
        Validators.maxLength(12),
        Validators.pattern(/^[A-Z]{2}[A-Z0-9]{10}$/)
      ]],
      website: ['', [
        Validators.pattern(/^(https?:\/\/)(www\.)?[a-zA-Z0-9\-.]+\.[a-zA-Z]{2,}(\/\S*)?$/)
      ]]
    });
  }

  loadCompany(id: string): void {
    this.isLoading = true;
    this.companyService.getById(id).subscribe({
      next: (company: Company) => {
        this.companyForm.patchValue(company);
        this.isLoading = false;
      },
      error: (error: unknown) => {
        this.snackBar.open('Error loading company', 'Close', { duration: 3000 });
        this.isLoading = false;
        this.router.navigate(['/companies']);
      }
    });
  }

  onSubmit(): void {
    if (this.companyForm.invalid) {
      this.markFormGroupTouched(this.companyForm);
      return;
    }

    this.isSaving = true;
    const formValues = this.companyForm.value;
    
    // Convert empty website string to null
    const company: Company = {
      ...formValues,
      website: formValues.website?.trim() ? formValues.website : null
    };

    if (this.isEditMode && this.companyId) {
      this.updateCompany(this.companyId, company);
    } else {
      this.createCompany(company);
    }
  }

  createCompany(company: Company): void {
    this.companyService.create(company).subscribe({
      next: (createdCompany: Company) => {
        this.snackBar.open('Company created successfully', 'Close', { duration: 3000 });
        this.isSaving = false;
        this.router.navigate(['/companies', createdCompany.id]);
      },
      error: (error: HttpErrorResponse) => {
        this.handleApiError(error);
        this.isSaving = false;
      }
    });
  }

  updateCompany(id: string, company: Company): void {
    this.companyService.update(id, company).subscribe({
      next: (updatedCompany: Company) => {
        this.snackBar.open('Company updated successfully', 'Close', { duration: 3000 });
        this.isSaving = false;
        this.router.navigate(['/companies', updatedCompany.id]);
      },
      error: (error: HttpErrorResponse) => {
        this.handleApiError(error);
        this.isSaving = false;
      }
    });
  }

  handleApiError(error: HttpErrorResponse): void {
    if (error.status === 400) {
      if (error.error?.errors) {
        // Handle standard validation errors format
        const validationErrors = error.error.errors;
        
        // Apply validation errors to form controls
        Object.keys(validationErrors).forEach(key => {
          const formControlName = key.charAt(0).toLowerCase() + key.slice(1); // Convert first letter to lowercase to match form control names
          const control = this.companyForm.get(formControlName);
          
          if (control) {
            // Set error on the form control
            control.setErrors({ serverError: validationErrors[key].join(', ') });
            control.markAsTouched();
          } else {
            // If no matching form control, show in snackbar
            this.snackBar.open(`${key}: ${validationErrors[key].join(', ')}`, 'Close', { duration: 5000 });
          }
        });
      } else if (error.error?.message) {
        // Handle business rule error format
        const errorMessage = error.error.message;
        
        // Check if the error is related to ISIN uniqueness
        if (errorMessage.includes('ISIN') && errorMessage.includes('unique')) {
          const isinControl = this.companyForm.get('isin');
          if (isinControl) {
            isinControl.setErrors({ serverError: errorMessage });
            isinControl.markAsTouched();
          } else {
            this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
          }
        } else {
          // For other business rule errors, show in snackbar
          this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
        }
      } else {
        // Handle other types of errors
        const errorMessage = error.error?.error || error.error?.title || 'An error occurred';
        this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
      }
    } else {
      // Handle non-400 errors
      const errorMessage = error.error?.message || error.error?.error || error.error?.title || 'An error occurred';
      this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
    }
  }

  // Helper method to mark all form controls as touched
  markFormGroupTouched(formGroup: FormGroup): void {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  cancel(): void {
    if (this.isEditMode && this.companyId) {
      this.router.navigate(['/companies', this.companyId]);
    } else {
      this.router.navigate(['/companies']);
    }
  }
} 