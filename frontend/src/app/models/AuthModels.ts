export interface RegisterRequest{
    Email: string,
    Password: string,
    UserName?: string | null,
    // Fullname?: string | null
}


export interface LoginRequest{
    Email: string,
    Password: string
}

export interface UserInfo{
    FullName?: string,
    Email: string,
    UserName: string,
    UserId: string
}