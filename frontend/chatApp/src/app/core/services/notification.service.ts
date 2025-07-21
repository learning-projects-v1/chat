import * as signalR from "@microsoft/signalr";
import { Injectable } from "@angular/core";
import { apiEndpoints } from "../api-endpoints";
import { BehaviorSubject, filter, Observable, Subject } from "rxjs";
import {
  ChatOverview,
  ChatThread,
  IncomingMessageNotification,
  IncomingReactionNotification,
  User,
} from "../../models/Dtos";
import { FriendRequestReceivedResponse } from "../../models/ResponseModels";
import { GlobalConstants } from "../constants";

@Injectable({ providedIn: "root" })
export class NotificationService {
  private hubConnection!: signalR.HubConnection;

  private friendRequestReceivedSource =
    new Subject<FriendRequestReceivedResponse>();
  private messageReceivedSource = new Subject<IncomingMessageNotification>();
  private reactionReceivedSource = new Subject<IncomingReactionNotification>();

  friendRequestReceived$ = this.friendRequestReceivedSource.asObservable();
  messageReceived$ = this.messageReceivedSource.asObservable();
  reactionReceived$ = this.reactionReceivedSource.asObservable();

  connect(token: string, userId: string) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(apiEndpoints.Notification, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log("SignalR connected");
        this.register(userId);
      })
      .catch(console.error);

    this.hubConnection.on(
      GlobalConstants.FriendRequestReceived,
      (payload: FriendRequestReceivedResponse) => {
        return this.friendRequestReceivedSource.next(payload);
      }
    );

    this.hubConnection.on(
      GlobalConstants.MessageAllNotification,
      (payload: IncomingMessageNotification) => {
        return this.messageReceivedSource.next(payload);
      }
    );

    this.hubConnection.on(
      GlobalConstants.ReactionNotification,
      (payload: IncomingReactionNotification) => {
        return this.reactionReceivedSource.next(payload);
      }
    );
  }

  reactionReceived(threadid: string): Observable<IncomingReactionNotification>{
    return this.reactionReceived$.pipe(filter((x) => x.threadId == threadid));
  }

  private register(userId: string) {
    this.hubConnection
      .invoke("register", userId)
      .catch((err) => console.error("Join group error:", err));
  }

  disconnect() {
    if (this.hubConnection) this.hubConnection.stop();
  }
}
