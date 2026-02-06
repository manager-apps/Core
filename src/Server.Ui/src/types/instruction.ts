export const InstructionType = {
  Gpo: 1,
  Shell: 2,
  Config: 3,
} as const;
export type InstructionType = (typeof InstructionType)[keyof typeof InstructionType];

export const InstructionState = {
  Pending: 1,
  Dispatched: 2,
  Completed: 3,
  Failed: 4,
} as const;
export type InstructionState = (typeof InstructionState)[keyof typeof InstructionState];

export interface CreateShellCommandRequest {
  command: string;
  timeout: number; // in milliseconds
}
export interface CreateGpoSetRequest {
  name: string;
  value: string;
}

export interface InstructionResponse {
  id: number;
  agentId: number;
  type: InstructionType;
  state: InstructionState;
  payloadJson: string; // can be CreateShellCommandRequest or CreateGpoSetRequest serialized as JSON
  output?: string;
  error?: string;
  createdAt: string; // ISO date string
  updatedAt?: string; // ISO date string
}

