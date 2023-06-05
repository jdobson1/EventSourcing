import { Injectable } from '@angular/core';
import { Product } from './product';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError, retry } from 'rxjs/operators';
import { HttpHeaders } from '@angular/common/http';
//import * as uuid from 'uuid';

@Injectable({
  providedIn: 'root'
})
export class ProductsService {
    private httpOptions: {};

    constructor(private http: HttpClient) {
        this.httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json'
            })
        };
    }

    public getProducts(): Observable<Product[]> {
        let getProducts: GetProducts = { ClientId: "53c1cfdc-399b-48b2-81ee-5970063f26bb" };
        return this.http.post<Product[]>('http://localhost:7073/api/GetProducts', getProducts, this.httpOptions);
    }

    public createProduct(product: Product) {
        

        let createProduct: CreateProduct = { Id: product.id, Name: product.name, ClientId: "53c1cfdc-399b-48b2-81ee-5970063f26bb" };
        //console.log(createProduct);
        return this.http.post('http://localhost:7179/api/CreateProduct', createProduct, this.httpOptions);
    }

    private handleError(error: HttpErrorResponse) {
        if (error.status === 0) {
            // A client-side or network error occurred. Handle it accordingly.
            console.error('An error occurred:', error.error);
        } else {
            // The backend returned an unsuccessful response code.
            // The response body may contain clues as to what went wrong.
            console.error(
                `Backend returned code ${error.status}, body was: `, error.error);
        }
        // Return an observable with a user-facing error message.
        return throwError(() => new Error('Something bad happened; please try again later.'));
    }
}

export class CreateProduct {
    public Id: string | undefined;
    public Name!: string;
    public ClientId!: string;
}

export class GetProducts {
    public ClientId!: string;
}
