import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgIf, CommonModule } from '@angular/common';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CompanyApiService } from '../../../../core/services/company-api.service';
import { Company } from '../../models/company';

@Component({
  selector: 'app-company-details',
  templateUrl: './company-details.component.html',
  styleUrls: ['./company-details.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ]
})
export class CompanyDetailsComponent implements OnInit {
  company: Company | null = null;
  isLoading = true;
  
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private companyService: CompanyApiService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadCompany();
  }

  loadCompany(): void {
    const id = this.route.snapshot.paramMap.get('id');
    
    if (!id) {
      this.snackBar.open('Invalid company ID', 'Close', { duration: 3000 });
      this.router.navigate(['/companies']);
      return;
    }

    this.isLoading = true;
    this.companyService.getById(id).subscribe({
      next: (company: Company) => {
        this.company = company;
        this.isLoading = false;
      },
      error: (error: unknown) => {
        this.snackBar.open('Error loading company details', 'Close', { duration: 3000 });
        this.isLoading = false;
        this.router.navigate(['/companies']);
      }
    });
  }

  editCompany(): void {
    if (this.company?.id) {
      this.router.navigate(['/companies/edit', this.company.id]);
    }
  }

  goBack(): void {
    this.router.navigate(['/companies']);
  }
} 