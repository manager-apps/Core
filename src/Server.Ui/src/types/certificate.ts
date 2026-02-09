export interface CreateEnrollmentTokenRequest {
  agentName: string;
  validityHours: number;
}

export interface EnrollmentTokenResponse {
  token: string;
  agentName: string;
  expiresAt: string;
}

export interface RevokeRequest {
  reason: string;
}
