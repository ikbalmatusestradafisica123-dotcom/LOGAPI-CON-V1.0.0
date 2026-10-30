export interface UserResponse {
  id: string;
  username: string;
  role: string;
  isActive: boolean;
  createdAtUtc: string;
}

export interface CreateUserRequest {
  username: string;
  password: string;
  role: string;
}

export interface UpdateUserRoleRequest{
  role: string;
}

export interface UpdateUserStatusRequest {
  isActive: boolean;
}
