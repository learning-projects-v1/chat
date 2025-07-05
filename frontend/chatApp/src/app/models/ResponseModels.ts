import { User } from "./Dtos";

export interface FriendRequestReceivedResponse{
    sender: User,
    message: string
}
