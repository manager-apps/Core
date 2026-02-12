export interface ConfigResponse {
    id: number;
    agentId: number;
    iterationDelaySeconds: number;
    authenticationExitIntervalSeconds: number;
    runningExitIntervalSeconds: number;
    executionExitIntervalSeconds: number;
    instructionsExecutionLimit: number;
    instructionResultsSendLimit: number;
    metricsSendLimit: number;
    allowedCollectors: string[];
    allowedInstructions: string[];
}

export interface ConfigUpdateRequest {
    authenticationExitIntervalSeconds?: number;
    runningExitIntervalSeconds?: number;
    executionExitIntervalSeconds?: number;
    instructionsExecutionLimit?: number;
    instructionResultsSendLimit?: number;
    metricsSendLimit?: number;
    allowedCollectors?: string[];
    allowedInstructions?: string[];
}