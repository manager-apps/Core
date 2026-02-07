import { Box, Avatar, Typography, Paper, CircularProgress } from "@mui/material";
import SmartToyIcon from "@mui/icons-material/SmartToy";
import PersonIcon from "@mui/icons-material/Person";
import type { ChatMessage as ChatMessageType } from "../../../types/chat";

interface ChatMessageProps {
    message: ChatMessageType;
}

const formatTime = (date: Date): string => {
    return new Date(date).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
};

export const ChatMessage: React.FC<ChatMessageProps> = ({ message }) => {
    const isUser = message.role === "user";
    const isAssistant = message.role === "assistant";

    return (
        <Box
            sx={{
                display: "flex",
                gap: 2,
                mb: 2,
                flexDirection: isUser ? "row-reverse" : "row",
            }}
        >
            <Avatar
                sx={{
                    bgcolor: isUser ? "primary.main" : isAssistant ? "secondary.main" : "grey.500",
                    width: 36,
                    height: 36,
                }}
            >
                {isUser ? <PersonIcon fontSize="small" /> : <SmartToyIcon fontSize="small" />}
            </Avatar>

            <Box sx={{ maxWidth: "75%", minWidth: 0 }}>
                <Paper
                    elevation={0}
                    sx={{
                        p: 2,
                        borderRadius: 2,
                        bgcolor: isUser ? "primary.main" : "background.paper",
                        color: isUser ? "primary.contrastText" : "text.primary",
                        border: isUser ? "none" : "1px solid",
                        borderColor: "divider",
                    }}
                >
                    {message.isLoading ? (
                        <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                            <CircularProgress size={16} color="inherit" />
                            <Typography variant="body2">Thinking...</Typography>
                        </Box>
                    ) : message.error ? (
                        <Typography variant="body2" color="error.main">
                            {message.error}
                        </Typography>
                    ) : (
                        <Typography
                            variant="body2"
                            sx={{
                                whiteSpace: "pre-wrap",
                                wordBreak: "break-word",
                                "& code": {
                                    bgcolor: isUser ? "rgba(255,255,255,0.1)" : "action.hover",
                                    px: 0.5,
                                    py: 0.25,
                                    borderRadius: 0.5,
                                    fontFamily: "monospace",
                                    fontSize: "0.85em",
                                },
                                "& pre": {
                                    bgcolor: isUser ? "rgba(255,255,255,0.1)" : "background.default",
                                    p: 1.5,
                                    borderRadius: 1,
                                    overflow: "auto",
                                    "& code": {
                                        bgcolor: "transparent",
                                        p: 0,
                                    },
                                },
                            }}
                        >
                            {message.content}
                        </Typography>
                    )}
                </Paper>
                <Typography
                    variant="caption"
                    color="text.secondary"
                    sx={{
                        display: "block",
                        mt: 0.5,
                        textAlign: isUser ? "right" : "left",
                    }}
                >
                    {formatTime(message.timestamp)}
                </Typography>
            </Box>
        </Box>
    );
};
