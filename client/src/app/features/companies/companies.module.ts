import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { CompaniesRoutingModule } from './companies-routing.module';

// Components
import { CompanyListComponent } from './components/list/company-list.component';
import { CompanyFormComponent } from './components/form/company-form.component';
import { CompanyDetailsComponent } from './components/details/company-details.component';

@NgModule({
  declarations: [
    CompanyListComponent,
    CompanyFormComponent,
    CompanyDetailsComponent
  ],
  imports: [
    SharedModule,
    CompaniesRoutingModule
  ]
})
export class CompaniesModule { } 