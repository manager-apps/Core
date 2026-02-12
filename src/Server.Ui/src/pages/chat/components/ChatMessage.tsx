import { Box, Avatar, Typography, Paper, CircularProgress } from "@mui/material";
import SmartToyIcon from "@mui/icons-material/SmartToy";
import PersonIcon from "@mui/icons-material/Person";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import rehypeHighlight from "rehype-highlight";
import "highlight.js/styles/github-dark.css";
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
                    ) : isAssistant ? (
                        <Box
                            className="markdown-content"
                            sx={{
                                "& p": {
                                    margin: "0 0 12px 0",
                                    lineHeight: 1.6,
                                    "&:last-child": {
                                        marginBottom: 0,
                                    },
                                },
                                "& h1, & h2, & h3, & h4, & h5, & h6": {
                                    margin: "16px 0 8px 0",
                                    fontWeight: "bold",
                                    "&:first-of-type": {
                                        marginTop: 0,
                                    },
                                },
                                "& ul, & ol": {
                                    margin: "8px 0",
                                    paddingLeft: "24px",
                                },
                                "& li": {
                                    margin: "4px 0",
                                },
                                "& code": {
                                    bgcolor: "action.hover",
                                    px: 0.75,
                                    py: 0.25,
                                    borderRadius: 1,
                                    fontFamily: "monospace",
                                    fontSize: "0.875em",
                                },
                                "& pre": {
                                    bgcolor: "#0d1117",
                                    p: 2,
                                    borderRadius: 1,
                                    overflow: "auto",
                                    margin: "12px 0",
                                    "& code": {
                                        bgcolor: "transparent",
                                        px: 0,
                                        py: 0,
                                        fontSize: "0.875rem",
                                        color: "#e6edf3",
                                    },
                                },
                                "& blockquote": {
                                    borderLeft: "4px solid",
                                    borderColor: "divider",
                                    pl: 2,
                                    my: 1,
                                    color: "text.secondary",
                                },
                                "& a": {
                                    color: "primary.main",
                                    textDecoration: "none",
                                    "&:hover": {
                                        textDecoration: "underline",
                                    },
                                },
                                "& table": {
                                    borderCollapse: "collapse",
                                    width: "100%",
                                    margin: "12px 0",
                                },
                                "& th, & td": {
                                    border: "1px solid",
                                    borderColor: "divider",
                                    padding: "8px 12px",
                                    textAlign: "left",
                                },
                                "& th": {
                                    bgcolor: "action.hover",
                                    fontWeight: "bold",
                                },
                            }}
                        >
                            <ReactMarkdown
                                remarkPlugins={[remarkGfm]}
                                rehypePlugins={[rehypeHighlight]}
                            >
                                {message.content}
                            </ReactMarkdown>
                        </Box>
                    ) : (
                        <Typography
                            variant="body2"
                            sx={{
                                whiteSpace: "pre-wrap",
                                wordBreak: "break-word",
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
