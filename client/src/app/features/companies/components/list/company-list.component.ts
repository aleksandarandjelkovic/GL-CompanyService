import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { NgIf, NgForOf, AsyncPipe, CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CompanyApiService } from '../../../../core/services/company-api.service';
import { Company } from '../../models/company';

@Component({
  selector: 'app-company-list',
  templateUrl: './company-list.component.html',
  styleUrls: ['./company-list.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    RouterLinkActive,
    AsyncPipe,
    MatTableModule,
    MatPaginatorModule, 
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule
  ]
})
export class CompanyListComponent implements OnInit {
  displayedColumns: string[] = ['name', 'ticker', 'exchange', 'isin', 'website', 'actions'];
  dataSource = new MatTableDataSource<Company>([]);
  isLoading = true;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  
  constructor(
    private companyService: CompanyApiService,
    private router: Router,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadCompanies();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  loadCompanies(): void {
    this.isLoading = true;
    this.companyService.getAll().subscribe({
      next: (companies: Company[]) => {
        this.dataSource.data = companies;
        this.isLoading = false;
      },
      error: (error: unknown) => {
        this.snackBar.open('Error loading companies', 'Close', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  viewCompany(id: string): void {
    if (id) {
      this.router.navigate(['/companies', id]);
    }
  }

  editCompany(id: string): void {
    if (id) {
      this.router.navigate(['/companies/edit', id]);
    }
  }
} 