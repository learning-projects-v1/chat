 import * as signalR from '@microsoft/signalr';
import { Injectable } from '@angular/core';
import { apiEndpoints } from '../api-endpoints';
import { BehaviorSubject, Subject } from 'rxjs';
import { ChatOverview, ChatThread, IncomingMessageNotification, User } from '../../models/Dtos';
import { FriendRequestReceivedResponse } from '../../models/ResponseModels';
import { GlobalConstants } from '../constants';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private hubConnection!: signalR.HubConnection;
  
  private friendRequestReceivedSource = new Subject<FriendRequestReceivedResponse>();
  friendRequestReceived$ = this.friendRequestReceivedSource.asObservable();

  private messageReceivedSource = new Subject<IncomingMessageNotification>();
  messageReceived$ = this.messageReceivedSource.asObservable();

  connect(token: string, userId: string) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(apiEndpoints.Notification, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(()=>{
        console.log("SignalR connected");
        this.register(userId);
      })
      .catch(console.error);

    this.hubConnection.on(GlobalConstants.FriendRequestReceived, (payload: FriendRequestReceivedResponse) => {
      return this.friendRequestReceivedSource.next(payload);
    });

    this.hubConnection.on(GlobalConstants.MessageReceived, (payload: IncomingMessageNotification) => {
      return this.messageReceivedSource.next(payload);
    });
  }

  private register(userId: string) {
    this.hubConnection.invoke('register', userId)
      .catch(err => console.error('Join group error:', err));
  }

  disconnect() {
    if (this.hubConnection) this.hubConnection.stop();
  }
}
