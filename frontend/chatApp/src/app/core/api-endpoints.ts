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
    Thread: (threadId: string) => `${environment.baseUrl}/api/threads/${threadId}/messages`,
    SendMessage: (threadId: string) => `${environment.baseUrl}/api/threads/${threadId}/messages`,
    
    AddReaction: (threadId: string, messageId: string) => `${environment.baseUrl}/api/threads/${threadId}/messages/${messageId}/reactions`,
    UpdateReaction: (threadId: string, messageId: string) => `${environment.baseUrl}/api/threads/${threadId}/messages/${messageId}/reactions/update`,
    DeleteReaction: (reactId: string, threadId: string, messageId: string) => `${environment.baseUrl}/api/threads/${threadId}/messages/${messageId}/reactions/${reactId}`,

    UpdateSeenStatus: (threadId: string) => `${environment.baseUrl}/api/threads/${threadId}/messages/seen-status`
}
