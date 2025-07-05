import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  ElementRef,
  HostListener,
  OnDestroy,
  OnInit,
  ViewChild,
} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { HttpClientService } from "../../../core/services/http-client.service";
import { UserService } from "../../../core/services/auth.service";
import { UserInfo } from "../../../models/AuthModels";
import { CommonModule } from "@angular/common";
import {
  ChatOverview,
  ChatThread,
  ChatThreadPreview,
  UserInfoDto,
  IncomingMessageNotification,
  User,
  Chat,
} from "../../../models/Dtos";
import { FormsModule } from "@angular/forms";
import { NotificationService } from "../../../core/services/notification.service";
import { Subject, takeUntil } from "rxjs";
import { ToastrService } from "ngx-toastr";
import { RouteConstants } from "../../../core/constants";
import { FriendInfoService } from "../../../core/global/friend-info.service";

interface reactionModal {
  class: string;
  title: string;
  style: string;
}

@Component({
  selector: "app-chat",
  templateUrl: "./chat-thread.component.html",
  styleUrls: ["./chat-thread.component.scss"],
  imports: [CommonModule, FormsModule],
})
export class ChatThreadComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild("scrollAnchor") private scrollAnchor!: ElementRef;
  @ViewChild("messageInput") messageInputRef!: ElementRef<HTMLInputElement>;

  threadId: string = "";
  friendId!: string;
  currentUserId!: string;
  currentUserInfo!: UserInfo;
  friendInfoList!: UserInfoDto[];
  chats: Chat[] = [];
  newMessage = "";
  ngUnsubscribe = new Subject<void>();
  userNameMap: { [key: string]: string } = {};
  isReplying: boolean = false;
  replyToMessage: Chat | null = null;
  reactToMessageId: string | null = "";
  reactionPopupPosition = { top: 0, left: 0 };
  reactionModals: reactionModal[] = [];
  friendInfosMap: UserInfoDto[] = [];

  constructor(
    private route: ActivatedRoute,
    private httpservice: HttpClientService,
    private userService: UserService,
    private notificationService: NotificationService,
    private toastr: ToastrService,
    private cdRef: ChangeDetectorRef,
    private friendInfoService: FriendInfoService
  ) {
    this.reactionModals = [
      { class: "fas fa-thumbs-up", title: "Like", style: "color:#3b82f6;" },
      { class: "fas fa-heart", title: "Love", style: "color:#ef4444;" },
      { class: "fas fa-face-laugh", title: "Laugh", style: "color:#facc15;" },
      {
        class: "fas fa-face-surprise",
        title: "Surprised",
        style: "color:#8b5cf6;",
      },
      { class: "fas fa-face-sad-tear", title: "Sad", style: "color:#64748b;" },
    ];
  }

  ngOnInit(): void {
    this.threadId = this.route.snapshot.paramMap.get(RouteConstants.chatThreadParam)!;
    this.loadMessages(this.threadId);

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
    this.focusInput();
    document.addEventListener("click", this.handleDocumentClick);
  }

  loadMessages(threadId: string): void {
    this.httpservice
      .getThreadContents(threadId)
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((data : ChatThread) => {
        this.chats = data?.chats;
        this.friendInfoList = data?.memberInfoList;
        this.mapUserNames();
      });
  }

  //html
  onSendReplyMessage() {
    this.sendMessage();
  }

  //html
  onSendMessage() {
    this.sendMessage();
  }

  replyTo(msg: Chat) {
    this.replyToMessage = msg;

    setTimeout(() => {
      this.messageInputRef?.nativeElement.focus();
    }, 0.5);
  }

  focusInput() {
    // Timeout ensures focus after view updates
    setTimeout(() => {
      this.messageInputRef?.nativeElement?.focus();
    });
  }

  @HostListener("document:keydown", ["$event"])
  onGlobalKeydown(event: KeyboardEvent) {
    const input = this.messageInputRef?.nativeElement;
    const charCode = event.key.charCodeAt(0);
    const isTextChar =
      (event.key.length === 1 && charCode >= 65 && charCode <= 90) ||
      (charCode >= 97 && charCode <= 122);

    if (isTextChar && document.activeElement !== input) {
      input?.focus();
    }
  }

  cancelReply() {
    this.replyToMessage = null;
  }

  reactTo(event: any) {
    // this.reactToMessageId = "";
  }

  deleteMsg(event: any) {}

  sendMessage(): void {
    // if (!this.newMessage.trim()) return;
    // const messagePayload: ChatOverview = {
    //   // receiverId: this.friendId,
    //   content: this.newMessage,
    //   // isSeen: false,
    //   senderId: this.currentUserId,
    //   sentAt: new Date(),
    // };

    // if (this.replyToMessage) {
    //   messagePayload.replyToMessageId = this.replyToMessage.id;
    //   this.replyToMessage = null;
    // }

    // this.httpservice
    //   .sendMessage(messagePayload)
    //   .subscribe((sentMessage: ChatOverview) => {
    //     this.newMessage = "";
    //     this.chats.push(sentMessage);
    //     setTimeout(() => this.scrollToBottom(), 50);
    //   });
  }

  mapUserNames() {
    for(let friendInfo of this.friendInfoList){
      this.userNameMap[friendInfo.id] = friendInfo.username;
    }
    
    this.userNameMap[this.currentUserId] = "You";
  }

  private scrollToBottom(): void {
    if (this.scrollAnchor) {
      this.scrollAnchor.nativeElement.scrollIntoView({ behavior: "smooth" });
    }
  }

  getMessageContent(messageId: string) {
    let chat = this.chats.find((c) => c.id == messageId); ///todo: need optimizations for old messages
    return chat?.content ?? "Message not found";
  }

  getMessageSenderName(messageId: string) {
    let chat = this.chats.find((c) => c.id == messageId);
    if (chat && chat.senderId) return this.userNameMap[chat.senderId];
    else return "User not found";
  }

  isSenderMe(id: string) {
    return this.currentUserId == id;
  }

  showReactionButtons(msg: Chat, event: MouseEvent) {
    setTimeout(() => {
      const modalHeight = 34;
      const modalWidth = 140;
      const target = (event.target as HTMLElement).closest("button");
      const rect = target?.getBoundingClientRect();
      this.reactionPopupPosition = {
        top: rect!.top - modalHeight - 5,
        left: rect!.left - modalWidth / 2,
      };
      this.reactToMessageId = msg.id ?? "";
    });
    // this.reactToMessageId = msg.id;
  }

  handleDocumentClick = (event: MouseEvent) => {
    const target = event.target as HTMLElement;
    const clickedInside = target.closest(".reaction-popup");
    if (!clickedInside) {
      this.reactToMessageId = null;
      this.cdRef.detectChanges();
    }
  };

  ngOnDestroy(): void {
    document.removeEventListener("click", this.handleDocumentClick);
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }
}
