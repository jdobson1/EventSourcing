//import { Injectable } from '@angular/core';
//import * as signalR from "@microsoft/signalr";

//@Injectable({
//    providedIn: 'root'
//})
//export class SignalRInitializerService {
//    constructor() { }

//    public InitSignalR(viewName: string, onConnectionHandler: any) {
//        const connection = new signalR.HubConnectionBuilder()
//            .withUrl('http://localhost:7073/api')
//            .configureLogging(signalR.LogLevel.Information)
//            .build();

//        connection.on(viewName, (view) => onConnectionHandler);

//        connection.start()
//            .catch(console.error);
//    }
//}
