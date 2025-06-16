import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { CompanyListComponent } from './components/list/company-list.component';
import { CompanyFormComponent } from './components/form/company-form.component';
import { CompanyDetailsComponent } from './components/details/company-details.component';

const routes: Routes = [
  { path: '', component: CompanyListComponent },
  { path: 'create', component: CompanyFormComponent },
  { path: 'edit/:id', component: CompanyFormComponent },
  { path: ':id', component: CompanyDetailsComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CompaniesRoutingModule { } 