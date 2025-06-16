import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClientService } from '../../../core/services/http-client.service';
import { UserService } from '../../../core/services/auth.service';
import { UserInfo } from '../../../models/AuthModels';
import { CommonModule } from '@angular/common';
import { Chat, FriendInfo, User } from '../../../models/UserModels';
import { FormsModule } from '@angular/forms';
import { NotificationService } from '../../../core/services/notification.service';
import { Subject, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-chat',
  templateUrl: './chat-thread.component.html',
  styleUrls: ['./chat-thread.component.scss'],
  imports: [CommonModule, FormsModule],
})
export class ChatThreadComponent implements OnInit, OnDestroy {
  friendId!: string;
  currentUserId!: string;
  currentUserInfo!: UserInfo;
  friendInfo!: FriendInfo;
  messages: Chat[] = [];
  newMessage = '';
  ngUnsubscribe = new Subject<void>();
  userNameMap: {[key: string]: string} = {}
  
  constructor(
    private route: ActivatedRoute,
    private httpservice: HttpClientService,
    private userService: UserService,
    private notificationService: NotificationService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.friendId = this.route.snapshot.paramMap.get('friendId')!;
    this.loadMessages();
    this.notificationService.messageReceived$
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((chat: Chat) => {
        this.messages.push(chat);
        //test purpose
        this.toastr.show("Message received");
      });

    this.currentUserInfo = this.userService.getUserInfo();
    this.currentUserId = this.currentUserInfo.UserId;
  }

  loadMessages(): void {
    this.httpservice
      .getChatHistory(this.friendId)
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((data) => {
        this.messages = data?.chats;
        this.friendInfo = data?.friendInfo;
      });
  }

  sendMessage(): void {
    if (!this.newMessage.trim()) return;
    const messagePayload: Chat = {
      receiverId: this.friendId,
      content: this.newMessage,
      isSeen: false,
      senderId: this.currentUserId,
      sentAt: new Date(),
    };

    this.httpservice.sendMessage(messagePayload).subscribe(() => {
      this.newMessage = '';
      this.messages.push(messagePayload);
      // this.loadMessages(); // or push the message directly
    });
  }

  mapUserNames(){
    this.userNameMap = {
      [this.currentUserId] : this.currentUserInfo.UserName,
      [this.friendId]: this.friendInfo.username
    }
  }
  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }
}
