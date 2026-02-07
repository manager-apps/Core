export interface ChatMessage {
    id: string;
    role: "user" | "assistant" | "system";
    content: string;
    timestamp: Date;
    isLoading?: boolean;
    error?: string;
}

export interface ChatRequest {
    message: string;
    conversationId?: string;
}

export interface ChatResponse {
    message: string;
    conversationId: string;
}

export interface ChatConversation {
    id: string;
    title: string;
    createdAt: Date;
    updatedAt: Date;
    messages: ChatMessage[];
}
