export interface HardwareResponse {
    id: number;
    agentId: number;
    osVersion?: string;
    machineName?: string;
    processorCount: number;
    totalMemoryBytes: number;
    createdAt: string; // ISO date string
    updatedAt?: string; // ISO date string, optional
}