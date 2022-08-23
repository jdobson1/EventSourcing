import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ProductsComponent } from './products/products.component';
import { ProductComponent } from './products/product.component';

import { OKTA_CONFIG, OktaAuthModule } from '@okta/okta-angular';
import { OktaAuth } from '@okta/okta-auth-js';

const oktaAuth = new OktaAuth({
  issuer: 'https://dev-91453357.okta.com/oauth2/default',
  clientId: '0oa643azsrkWD2XUZ5d7',
  redirectUri: window.location.origin + '/login/callback',
  scopes: ['openid', 'profile', 'email']
});

@NgModule({
  declarations: [
    AppComponent,
    ProductsComponent,
    ProductComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    OktaAuthModule,
    HttpClientModule,
    FormsModule
  ],
  providers: [
    { provide: OKTA_CONFIG, useValue: {oktaAuth}}
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
