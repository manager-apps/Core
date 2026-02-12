import axios, { getApiBaseUrl } from "./axios";
import type { ChatRequest, ChatResponse } from "../types/chat";

export const streamChatMessage = async (
    request: ChatRequest,
    onChunk: (chunk: string) => void,
    signal?: AbortSignal
): Promise<void> => {
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
