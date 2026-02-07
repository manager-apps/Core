import { useState, type KeyboardEvent } from "react";
import { Box, TextField, IconButton, Paper, Tooltip } from "@mui/material";
import SendIcon from "@mui/icons-material/Send";
import StopIcon from "@mui/icons-material/Stop";

interface ChatInputProps {
    onSend: (message: string) => void;
    onStop?: () => void;
    disabled?: boolean;
    isStreaming?: boolean;
    placeholder?: string;
}

export const ChatInput: React.FC<ChatInputProps> = ({
    onSend,
    onStop,
    disabled = false,
    isStreaming = false,
    placeholder = "Type your message...",
}) => {
    const [message, setMessage] = useState("");

    const handleSend = () => {
        if (message.trim() && !disabled) {
            onSend(message.trim());
            setMessage("");
        }
    };

    const handleKeyDown = (e: KeyboardEvent<HTMLDivElement>) => {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            handleSend();
        }
    };

    return (
        <Paper
            elevation={0}
            sx={{
                p: 2,
                border: "1px solid",
                borderColor: "divider",
                borderRadius: 2,
                bgcolor: "background.paper",
            }}
        >
            <Box sx={{ display: "flex", gap: 1, alignItems: "flex-end" }}>
                <TextField
                    fullWidth
                    multiline
                    maxRows={4}
                    size="small"
                    value={message}
                    onChange={(e) => setMessage(e.target.value)}
                    onKeyDown={handleKeyDown}
                    placeholder={placeholder}
                    disabled={disabled}
                    sx={{
                        "& .MuiOutlinedInput-root": {
                            borderRadius: 1.5,
                            bgcolor: "background.default",
                        },
                    }}
                />
                {isStreaming ? (
                    <Tooltip title="Stop generating">
                        <IconButton
                            color="error"
                            onClick={onStop}
                            sx={{
                                bgcolor: "error.main",
                                color: "error.contrastText",
                                "&:hover": { bgcolor: "error.dark" },
                            }}
                        >
                            <StopIcon />
                        </IconButton>
                    </Tooltip>
                ) : (
                    <Tooltip title="Send message (Enter)">
                        <span>
                            <IconButton
                                color="primary"
                                onClick={handleSend}
                                disabled={!message.trim() || disabled}
                                sx={{
                                    bgcolor: "primary.main",
                                    color: "primary.contrastText",
                                    "&:hover": { bgcolor: "primary.dark" },
                                    "&.Mui-disabled": {
                                        bgcolor: "action.disabledBackground",
                                        color: "action.disabled",
                                    },
                                }}
                            >
                                <SendIcon />
                            </IconButton>
                        </span>
                    </Tooltip>
                )}
            </Box>
            <Box sx={{ mt: 1, display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                <Box sx={{ display: "flex", gap: 1 }}>
                    {/* Future: Add attachment button, emoji button, etc. */}
                </Box>
                <Box sx={{ display: "flex", alignItems: "center", gap: 0.5 }}>
                    <kbd
                        style={{
                            padding: "2px 6px",
                            fontSize: "0.7rem",
                            borderRadius: 4,
                            backgroundColor: "rgba(255,255,255,0.1)",
                            border: "1px solid rgba(255,255,255,0.2)",
                        }}
                    >
                        Enter
                    </kbd>
                    <span style={{ fontSize: "0.7rem", color: "rgba(255,255,255,0.5)" }}>to send</span>
                </Box>
            </Box>
        </Paper>
    );
};
