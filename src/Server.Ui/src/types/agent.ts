export const AgentState = {
    Inactive: 1,
    Active: 2
} as const;
export type AgentState = (typeof AgentState)[keyof typeof AgentState];

export interface AgentResponse {
    id: number;
    name: string;
    state: AgentState;
    createdAt: string;
    lastUpdatedAt: string;
    updatedAt?: string;
}