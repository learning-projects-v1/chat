import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClientService } from '../../../core/services/http-client.service';
import { UserService } from '../../../core/services/auth.service';
import { UserInfo } from '../../../models/AuthModels';
import { CommonModule } from '@angular/common';
import { Chat, ChatThread, ChatThreadPreview, FriendInfo, IncomingMessageNotification, User } from '../../../models/UserModels';
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
export class ChatThreadComponent implements OnInit, AfterViewInit,OnDestroy {
  @ViewChild('scrollAnchor') private scrollAnchor!: ElementRef;
  friendId!: string;
  currentUserId!: string;
  currentUserInfo!: UserInfo;
  friendInfo!: FriendInfo;
  chats: Chat[] = [];
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
      .subscribe((incomingMessage: IncomingMessageNotification) => {
        this.chats.push(incomingMessage.chat);
        setTimeout(() => this.scrollToBottom(), 50);
        //test purpose
        this.toastr.show("Message received");
      });

    this.currentUserInfo = this.userService.getUserInfo();
    this.currentUserId = this.currentUserInfo.UserId;
  } 

  ngAfterViewInit(): void {
    setTimeout(() => this.scrollToBottom(), 50);
  }

  loadMessages(): void {
    this.httpservice
      .getChatHistory(this.friendId)
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((data) => {
        this.chats = data?.chats;
        this.friendInfo = data?.friendInfo;
        this.mapUserNames();
      });
  }

  //html
  onSendReplyMessage(){
    this.sendMessage();
  }

  //html
  onSendMessage(){
    this.sendMessage();
  }

  replyTo(event: any){

  }

  reactTo(event: any){

  }

  deleteMsg(event: any){

  }

  sendMessage(replyToMessageId = ""): void {
    if (!this.newMessage.trim()) return;
    const messagePayload: Chat = {
      receiverId: this.friendId,
      content: this.newMessage,
      isSeen: false,
      senderId: this.currentUserId,
      sentAt: new Date(),
    };
    if(replyToMessageId){
      messagePayload.replyToMessageId = replyToMessageId;
    }
    
    this.httpservice.sendMessage(messagePayload).subscribe(() => {
      this.newMessage = '';
      this.chats.push(messagePayload);
      setTimeout(() => this.scrollToBottom(), 50);
    });
  }

  mapUserNames(){
    this.userNameMap = {
      [this.currentUserId] : this.currentUserInfo.Username,
      [this.friendId]: this.friendInfo.username
    }
  }

  private scrollToBottom(): void {
    if (this.scrollAnchor) {
      this.scrollAnchor.nativeElement.scrollIntoView({ behavior: 'smooth' });
    }
  }

  isSenderMe(id: string){
    return this.currentUserId == id;
  }
  
  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }
}
