import { CommonModule } from "@angular/common";
import { Component, Input, OnDestroy, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { HttpClientService } from "../../../core/services/http-client.service";
import { Subject, takeUntil } from "rxjs";
import { NotificationService } from "../../../core/services/notification.service";
import { ChatThreadPreview } from "../../../models/UserModels";

export interface ConnectedUser {
  userId: string;
  username: string;
  lastMessage: string;
  lastMessageTime: string; // ISO format
}

@Component({
  selector: "app-messages",
  templateUrl: "./messages.component.html",
  styleUrls: ["./messages.component.scss"],
  imports: [CommonModule],
})
export class MessagesComponent implements OnInit, OnDestroy {
  messagePreviews: ChatThreadPreview[] = [];
  ngUnsubscribe: Subject<void> = new Subject<void>();

  constructor(
    private router: Router,
    private httpService: HttpClientService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.httpService
      .getLatestMessages()
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((res) => {
        this.messagePreviews = res;
      });

    this.notificationService.messageReceived$
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((res) => {
        this.messagePreviews.push(res);
      });
  }

  goToChat(userId: string) {
    this.router.navigate(["/chat", userId]);
  }

  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }
}
