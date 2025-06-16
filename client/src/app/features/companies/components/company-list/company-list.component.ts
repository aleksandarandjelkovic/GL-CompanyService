import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CompanyApiService } from '../../../../core/services/company-api.service';
import { Company } from '../../models/company';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-company-list',
  templateUrl: './company-list.component.html',
  styleUrls: ['./company-list.component.scss']
})
export class CompanyListComponent implements OnInit {
  companies: Company[] = [];
  loading = true;
  error: string | null = null;
  displayedColumns: string[] = ['name', 'symbol', 'isin', 'price', 'actions'];

  constructor(
    private companyService: CompanyApiService,
    private http: HttpClient
  ) {
    console.log('COMPANY LIST COMPONENT - DIRECT CONSOLE TEST');
  }

  ngOnInit(): void {
    // Skip the normal loading and use manual fetch instead
    this.loadCompaniesManually();
  }

  loadCompaniesManually(): void {
    this.loading = true;
    const token = localStorage.getItem('access_token');
    
    if (!token) {
      console.error('No token found in localStorage');
      this.error = 'You must log in first';
      this.loading = false;
      return;
    }
    
    console.log('Manual API call - Token exists, first 20 chars:', token.substring(0, 20) + '...');
    
    // Create headers with explicit token format
    const headers = new HttpHeaders()
      .set('Authorization', `Bearer ${token}`)
      .set('Content-Type', 'application/json');
    
    console.log('Manual API call - Using URL:', `${environment.apiUrl}/companies`);
    console.log('Manual API call - Headers:', 
      Array.from(headers.keys()).map(key => `${key}: ${headers.get(key)}`).join(', '));
    
    // Make the API call with explicit headers
    this.http.get<Company[]>(`${environment.apiUrl}/companies`, { headers: headers })
      .subscribe({
        next: (data) => {
          console.log('Manual API call - Success! Companies:', data);
          this.companies = data;
          this.error = null;
          this.loading = false;
        },
        error: (err) => {
          console.error('Manual API call - Error:', err.status, err.statusText);
          console.error('Manual API call - Full error:', err);
          this.error = `API Error: ${err.status} ${err.statusText}`;
          this.loading = false;
        }
      });
  }
  
  loadCompanies(): void {
    this.loading = true;
    console.log('COMPANY LIST - Loading companies via service');
    
    this.companyService.getAll().subscribe({
      next: (data) => {
        console.log('COMPANY LIST - Companies loaded successfully:', data.length);
        this.companies = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('COMPANY LIST - Error loading companies:', error);
        this.error = 'Failed to load companies. Please try again.';
        this.loading = false;
        
        // Try direct HTTP call if the service method fails
        this.testDirectApiCall();
      }
    });
  }
  
  // Test direct API call with token
  testDirectApiCall(): void {
    console.log('COMPANY LIST - Testing direct API call');
    const token = localStorage.getItem('access_token');
    console.log('COMPANY LIST - Token exists:', !!token);
    
    if (token) {
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`
      });
      
      console.log('COMPANY LIST - Making direct API call with headers');
      console.log('COMPANY LIST - API URL:', `${environment.apiUrl}/companies`);
      console.log('COMPANY LIST - Headers:', headers.get('Authorization')?.substring(0, 20) + '...');
      
      this.http.get<Company[]>(`${environment.apiUrl}/companies`, { headers })
        .subscribe({
          next: (data) => {
            console.log('COMPANY LIST - Direct API call successful:', data.length);
          },
          error: (err) => {
            console.error('COMPANY LIST - Direct API call failed:', err.status, err.statusText);
            console.error('COMPANY LIST - Error details:', err);
          }
        });
    } else {
      console.error('COMPANY LIST - No token found for direct API call');
    }
  }
} 