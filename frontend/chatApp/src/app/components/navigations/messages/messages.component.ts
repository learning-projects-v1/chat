import { CommonModule } from "@angular/common";
import { Component, Input, OnDestroy, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { HttpClientService } from "../../../core/services/http-client.service";
import { Subject, takeUntil } from "rxjs";
import { NotificationService } from "../../../core/services/notification.service";
import { Chat, ChatThreadPreview, UserInfoDto } from "../../../models/Dtos";
import { FriendInfoService } from "../../../core/global/friend-info.service";
import { UserService } from "../../../core/services/auth.service";

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
  // messagePreviews: ChatThreadPreview[] = [];
  chats: Chat[] = [];
  ngUnsubscribe: Subject<void> = new Subject<void>();
  friendInfosMap: Map<string, UserInfoDto> = new Map();
  currentUser?: UserInfoDto;

  constructor(
    private router: Router,
    private httpService: HttpClientService,
    private notificationService: NotificationService,
    private friendInfoService: FriendInfoService,
    private userInfoService: UserService
  ) {}

  ngOnInit(): void {
    this.httpService
      .getLatestMessages()
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((res) => {
        this.chats = res;
      });

    this.notificationService.messageReceived$
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((res) => {
        // this.messagePreviews.push(res);
      });

      this.friendInfosMap = this.friendInfoService.getFriendInfosMap();   
  }

  getUserName(id: string){
    let val =  this.friendInfosMap.get(id);
    return val?.username;
  }

  goToChat(threadId: string) {
    this.router.navigate(["/chat", threadId]);
  }

  
  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }
}
