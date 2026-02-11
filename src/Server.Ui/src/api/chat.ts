import axios, { getApiBaseUrl } from "./axios";
import type { ChatRequest, ChatResponse } from "../types/chat";

export const sendChatMessage = async (request: ChatRequest): Promise<ChatResponse> => {
    const response = await axios.post<ChatResponse>("/chat", request);
    return response.data;
};

export const streamChatMessage = async (
    request: ChatRequest,
    onChunk: (chunk: string) => void,
    signal?: AbortSignal
): Promise<void> => {
    // Get the base URL and construct the full URL
    const baseUrl = getApiBaseUrl();
    const url = `${baseUrl}/chat/stream`;

    const response = await fetch(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(request),
        signal,
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Chat API error (${response.status}): ${errorText || response.statusText}`);
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
