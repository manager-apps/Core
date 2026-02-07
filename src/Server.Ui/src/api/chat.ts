import axios from "./axios";
import type { ChatRequest, ChatResponse } from "../types/chat";

export const sendChatMessage = async (request: ChatRequest): Promise<ChatResponse> => {
    const response = await axios.post<ChatResponse>("/api/chat", request);
    return response.data;
};

export const streamChatMessage = async (
    request: ChatRequest,
    onChunk: (chunk: string) => void,
    signal?: AbortSignal
): Promise<void> => {
    const response = await fetch("/api/chat/stream", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(request),
        signal,
    });

    if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
    }

    const reader = response.body?.getReader();
    if (!reader) {
        throw new Error("No response body");
    }

    const decoder = new TextDecoder();

    while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value, { stream: true });
        onChunk(chunk);
    }
};
