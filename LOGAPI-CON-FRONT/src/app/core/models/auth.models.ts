export interface LoginRequest { username: string; password: string; }
export interface LoginResponse {
  accessToken: string;
  tokenType: string;
  expiresAtUtc: string;
  username: string;
  role: string;
}
