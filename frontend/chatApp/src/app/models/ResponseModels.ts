import { User } from "./UserModels";

export interface FriendRequestReceivedResponse{
    sender: User,
    message: string
}
