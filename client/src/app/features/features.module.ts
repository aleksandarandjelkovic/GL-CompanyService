import { NgModule } from '@angular/core';
import { CompaniesModule } from './companies/companies.module';

@NgModule({
  imports: [
    CompaniesModule
  ],
  exports: [
    CompaniesModule
  ]
})
export class FeaturesModule { } 