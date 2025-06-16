import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'companies',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./core/components/login/login.component')
      .then(c => c.LoginComponent)
  },
  {
    path: 'companies',
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./features/companies/components/list/company-list.component')
          .then(c => c.CompanyListComponent)
      },
      {
        path: 'create',
        loadComponent: () => import('./features/companies/components/form/company-form.component')
          .then(c => c.CompanyFormComponent)
      },
      {
        path: 'edit/:id',
        loadComponent: () => import('./features/companies/components/form/company-form.component')
          .then(c => c.CompanyFormComponent)
      },
      {
        path: ':id',
        loadComponent: () => import('./features/companies/components/details/company-details.component')
          .then(c => c.CompanyDetailsComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'companies'
  }
]; 