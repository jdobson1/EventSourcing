import { Component, OnInit } from '@angular/core';
import { BasketService, Product, ProductsService } from '@shared';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { UUID } from 'angular2-uuid';

@Component({
  selector: 'app-products',
  templateUrl: './products.component.html',
  styles: [
  ]
})
export class ProductsComponent implements OnInit {
    public products: Product[] = [];

  constructor(private productsService: ProductsService, private basketService: BasketService) { }

  public ngOnInit(): void {
      this.productsService.getProducts().subscribe(products => {
          this.products = products
      });
      this.InitSignalR();
  }

  public addToBasket(productId: string): void {
    const product = this.products.find(p => p.id === productId) as Product;
    this.basketService.addToBasket(product);
    }

    public createProduct(form: any) {
        let product: Product = {
            id: UUID.UUID(),
            name: form.productName,
            link: '',
            description: ''
        };
        this.productsService.createProduct(product).subscribe((response: any) => {
            //console.log(response);
        });
    }

  private InitSignalR() {

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(environment.productQueryUrl)
        .configureLogging(signalR.LogLevel.Information)
          .build();
      
    connection.on('ProductsView', (productsView: ProductsView) => {
          this.products = [];
          productsView.Products.forEach( (dto: ProductDto) => {
              this.products.push({ id: dto.Id, name: dto.Name, link: '', description: '' });
          })
    });
    connection.start()
        .catch(console.error);
  }  
}

export interface ProductsView {
    Products: ProductDto[];
}

export interface ProductDto {
    Id: string,
    Name: string
    
}
