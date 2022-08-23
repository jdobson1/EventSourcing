import { Injectable } from '@angular/core';
import { Product } from './product';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError, retry } from 'rxjs/operators';
import { HttpHeaders } from '@angular/common/http';

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

    public getProducts() : Observable<Product[]> {
        return this.http.post<Product[]>('http://localhost:7073/api/GetProducts', this.httpOptions);
    }

    public createProduct(product: Product) {
        

        let createProduct: CreateProduct = { Id: product.id, Name: product.name };
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
}

export class GetProducts {
}
