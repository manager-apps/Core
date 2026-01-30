export const InstructionType = {
  GpoSet: 1,
  ShellCommand: 2,
  ConfigUpdate: 3,
} as const;
export type InstructionType = (typeof InstructionType)[keyof typeof InstructionType];

export const InstructionState = {
  Pending: 1,
  Completed: 2,
  Failed: 3,
} as const;
export type InstructionState = (typeof InstructionState)[keyof typeof InstructionState];

export interface CreateInstructionRequest {
  agentId: number;
  type: InstructionType;
  payloadJson: string;
}

export interface InstructionResponse {
  id: number;
  agentId: number;
  type: InstructionType;
  payloadJson: string;
  state: InstructionState;
  output?: string;
  error?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface ShellCommandPayload {
  command: string;
  description?: string;
  timeout?: number;
}

export interface GpoSetPayload {
  path: string;
  name: string;
  value: string;
  type: string;
  description?: string;
}

export type InstructionPayload = ShellCommandPayload | GpoSetPayload;
