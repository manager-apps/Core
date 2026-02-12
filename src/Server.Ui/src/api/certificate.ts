import type { CreateEnrollmentTokenRequest, EnrollmentTokenResponse, RevokeRequest } from "../types/certificate";
import api from "./axios";

export const createEnrollmentToken = async (
  payload: CreateEnrollmentTokenRequest
): Promise<EnrollmentTokenResponse> => {
  const response = await api.post<EnrollmentTokenResponse>(
    'certs/tokens', payload);
  return response.data;
}

export const revokeCertificate = async (
  agentId: number,
  payload: RevokeRequest
): Promise<boolean> => {
  const response = await api.post<boolean>(
    `agents/${agentId}/certs/revoke`, payload);
  return response.data;
}
