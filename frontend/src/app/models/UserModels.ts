export interface User{
    id: string,
    userName: string,
    fullName: string
}

export interface Message {
  id: string;
  content: string;
  sentAt: string; // ISO string, will be parsed to Date
  isSeen: boolean;
  username: string;
  replyToMessageId?: string;
  replyToContent?: string;
}

export interface LatestMessage {
  friendId: string,
  friendUsername: string,
  content: string,
  sentAt: Date,
  messageSenderId: string
}

export interface Chat {
  id?: string,
  content: string,
  senderId: string,
  receiverId: string,
  isSeen: boolean,
  replyToMessageId?: string,
  sentAt: Date,
}

export interface FriendInfo {
  id: string,
  username: string,
  avatarUrl?: string
}

export interface ChatThread{
  friendInfo: FriendInfo,
  chats: Chat[]
}