import type { ConfigResponse } from "./config";
import type { HardwareResponse } from "./hardware";

export interface AgentResponse {
    id: number;
    name: string;
    currentTag: string;
    sourceTag: string;
    version: string;
    createdAt: string; // ISO date string
    lastUpdatedAt: string; // ISO date string
    updatedAt?: string; // ISO date string, optional
}

export interface AgentDetailResponse extends AgentResponse {
    config?: ConfigResponse;
    hardware?: HardwareResponse;
}
