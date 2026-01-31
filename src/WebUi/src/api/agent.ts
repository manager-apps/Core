import type { AgentResponse } from "../types/agent";
import type { InstructionResponse } from "../types/instruction";
import api from "./axios";

export const fetchAgents = async (): Promise<AgentResponse[]> => {
  const response = await api.get<AgentResponse[]>('agent');
  return response.data;
};

export const fetchAgentById = async (id: number): Promise<AgentResponse> => {
  const response = await api.get<AgentResponse>(`agent/${id}`);
  return response.data;
}

export const fetchInstructionsForAgent = async (agentId: number): Promise<InstructionResponse[]> => {
  const response = await api.get<InstructionResponse[]>(`agent/${agentId}/instructions`);
  return response.data;
}