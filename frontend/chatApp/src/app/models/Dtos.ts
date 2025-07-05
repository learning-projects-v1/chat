export interface User{
    id: string,
    userName: string,
    fullName?: string,
    avatarUrl?: string
}

export interface Chat {
  id: string;
  content: string;
  senderId: string;
  chatThreadId: string;
  isSeen: boolean;
  username: string;
  replyToMessageId?: string;
  sentAt: string; // ISO string, will be parsed to Date
}

export interface ChatOverview {
  id?: string,
  content: string,
  senderId: string,
  threadId: string,
  replyToMessageId?: string,
  sentAt: Date,
}

export interface UserInfoDto {
  id: string,
  username: string,
  avatarUrl?: string,
  fullName?: string
}

export interface ChatThread{
  memberInfoList: UserInfoDto[],
  chats: Chat[]
}

export interface ChatThreadPreview{
  senderInfo: {[key: string]: User};
  chat: ChatOverview
}

export interface IncomingMessageNotification{
  friendInfo: UserInfoDto,
  chat: Chat
}
