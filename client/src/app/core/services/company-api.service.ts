import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Company } from '../../features/companies/models/company';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CompanyApiService {
  private apiUrl: string;

  constructor(private http: HttpClient) {
    this.apiUrl = `${environment.apiUrl}/companies`;
  }

  getAll(): Observable<Company[]> {
    return this.http.get<Company[]>(this.apiUrl);
  }

  getById(id: string): Observable<Company> {
    return this.http.get<Company>(`${this.apiUrl}/${id}`);
  }

  getByIsin(isin: string): Observable<Company> {
    return this.http.get<Company>(`${this.apiUrl}/isin/${isin}`);
  }

  create(company: Company): Observable<Company> {
    return this.http.post<Company>(this.apiUrl, company);
  }

  update(id: string, company: Company): Observable<Company> {
    const updatedCompany = { ...company, id };
    return this.http.put<Company>(`${this.apiUrl}/${id}`, updatedCompany);
  }
} 