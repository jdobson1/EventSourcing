import { Injectable } from "@angular/core";
import { Product } from "./product";
import { HttpClient, HttpHeaders, HttpErrorResponse } from "@angular/common/http";
import { Observable, throwError } from 'rxjs';
import { map, catchError, retry } from 'rxjs/operators';

@Injectable({
    providedIn: "root"
})
export class BasketService {
    private products: Product[] = [];
    private httpOptions: {};

    constructor(private http: HttpClient) {
        this.httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': "application/json"
            })
        };
    }

    getBasketItems(): Product[] {
        return this.products;
    }

    addToBasket(product: Product) {
        let addItemToCart: AddItemToCart = { ProductId: product.id, ClientId: "53c1cfdc-399b-48b2-81ee-5970063f26bb", CartId: "83c80db4-246d-4085-98d8-7881d1401eb0", Quantity: 1};
        console.log(addItemToCart);
        this.products.push(product);
        this.http.post("http://localhost:7072/api/AddItemToCart", addItemToCart, this.httpOptions).subscribe({
            next: data => {
                
            },
            error: error => {
                this.handleError(error);
                console.error('There was an error!', error);
            }
        })
        console.log("Item added to cart");
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

export class AddItemToCart {
    public ProductId: string | undefined;
    public ClientId!: string;
    public CartId!: string;
    public Quantity: number = 0;
}