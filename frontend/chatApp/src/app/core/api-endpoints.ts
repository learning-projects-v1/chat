import { environment } from "../environments/environment";


export const apiEndpoints = {
    Test: `${environment.baseUrl}/api/auth/Test`,
    Register: `${environment.baseUrl}/api/Auth/register`,
    Login: `${environment.baseUrl}/api/auth/login`,
    Notification: `${environment.baseUrl}/hubs/notifications`,
    FriendSuggestions: `${environment.baseUrl}/api/users/suggestions`,
    
    SendFriendRequest: (id: string) => `${environment.baseUrl}/api/friends/requests/${id}`,
    AcceptFriendRequest: (id: string) => `${environment.baseUrl}/api/friends/requests/${id}/accept`,
    RejectFriendRequest: (id: string) => `${environment.baseUrl}/api/friends/requests/${id}/reject`,
    Friends: `${environment.baseUrl}/api/friends/`,
    FriendRequests: `${environment.baseUrl}/api/friends/requests`,

    LatestMessages: `${environment.baseUrl}/api/chatoverview`,
    ChatHistory: (friendId: string) => `${environment.baseUrl}/api/messages/thread/${friendId}`,
    SendMessage: `${environment.baseUrl}/api/messages/send`
}