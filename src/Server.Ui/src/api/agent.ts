import type { AgentDetailResponse, AgentResponse, AgentUpdateStateRequest } from "../types/agent";
import type { ConfigUpdateRequest } from "../types/config";
import type { CreateGpoSetRequest, CreateShellCommandRequest, InstructionResponse } from "../types/instruction";
import api from "./axios";

export const fetchAgents = async (): Promise<AgentResponse[]> => {
  const response = await api.get<AgentResponse[]>('agents');
  return response.data;
};

export const fetchAgentById = async (id: number): Promise<AgentDetailResponse> => {
  const response = await api.get<AgentDetailResponse>(`agents/${id}`);
  return response.data;
}

export const fetchInstructionsForAgent = async (agentId: number): Promise<InstructionResponse[]> => {
  const response = await api.get<InstructionResponse[]>(`agents/${agentId}/instructions`);
  return response.data;
}

export const updateAgentConfig = async (agentId: number, config: ConfigUpdateRequest): Promise<void> => {
  await api.patch(`agents/${agentId}/config`, config);
}

export const createShellInstruction = async (
  agentId: number,
  payload: CreateShellCommandRequest
): Promise<InstructionResponse> => {
  const response = await api.post<InstructionResponse>(`agents/${agentId}/instructions/shell`, payload);
  return response.data;
}

export const createGpoInstruction = async (
  agentId: number,
  payload: CreateGpoSetRequest
): Promise<InstructionResponse> => {
  const response = await api.post<InstructionResponse>(`agents/${agentId}/instructions/gpo`, payload);
  return response.data;
}

export const updateAgentState = async (agentId: number, request: AgentUpdateStateRequest): Promise<AgentResponse> => {
  const response = await api.put<AgentResponse>(`agents/${agentId}/state`, request);
  return response.data;
}
