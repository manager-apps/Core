import type { ConfigResponse } from "./config";
import type { HardwareResponse } from "./hardware";

export const AgentState = {
    Inactive: 1,
    Active: 2
} as const;
export type AgentState = (typeof AgentState)[keyof typeof AgentState];

export interface AgentResponse {
    id: number;
    name: string;
    currentTag: string;
    sourceTag: string;
    version: string;
    state: AgentState;
    createdAt: string; // ISO date string
    lastUpdatedAt: string; // ISO date string
    updatedAt?: string; // ISO date string, optional
}

export interface AgentUpdateStateRequest {
    newState: AgentState;
}

export interface AgentDetailResponse extends AgentResponse {
    config?: ConfigResponse;
    hardware?: HardwareResponse;
}
