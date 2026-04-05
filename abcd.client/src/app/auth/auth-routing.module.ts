import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { VerifyPinComponent } from './verify-pin/verify-pin.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'verify-pin', component: VerifyPinComponent },
  { path: '', redirectTo: 'login', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule { }
