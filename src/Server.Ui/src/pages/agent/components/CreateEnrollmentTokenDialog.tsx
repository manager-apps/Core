import { useState } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    Box,
    Alert,
    IconButton,
    InputAdornment,
    Typography,
    Divider,
    Paper,
} from "@mui/material";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import CloseIcon from "@mui/icons-material/Close";
import { createEnrollmentToken } from "../../../api/certificate";
import type { EnrollmentTokenResponse } from "../../../types/certificate";

interface CreateEnrollmentTokenDialogProps {
    open: boolean;
    onClose: () => void;
}

export function CreateEnrollmentTokenDialog({ open, onClose }: CreateEnrollmentTokenDialogProps) {
    const [agentName, setAgentName] = useState("");
    const [validityHours, setValidityHours] = useState(24);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [tokenResponse, setTokenResponse] = useState<EnrollmentTokenResponse | null>(null);
    const [copied, setCopied] = useState(false);

    const handleCreate = async () => {
        if (!agentName.trim()) {
            setError("Agent name is required");
            return;
        }

        if (validityHours <= 0) {
            setError("Validity hours must be greater than 0");
            return;
        }

        try {
            setLoading(true);
            setError(null);

            const response = await createEnrollmentToken({
                agentName: agentName.trim(),
                validityHours,
            });

            setTokenResponse(response);
        } catch (err: any) {
            console.error("Failed to create enrollment token", err);
            setError(err.response?.data?.detail || "Failed to create enrollment token");
        } finally {
            setLoading(false);
        }
    };

    const handleCopyToken = () => {
        if (tokenResponse) {
            navigator.clipboard.writeText(tokenResponse.token);
            setCopied(true);
            setTimeout(() => setCopied(false), 2000);
        }
    };

    const handleClose = () => {
        setAgentName("");
        setValidityHours(24);
        setError(null);
        setTokenResponse(null);
        setCopied(false);
        onClose();
    };

    return (
        <Dialog
            open={open}
            onClose={handleClose}
            maxWidth="sm"
            fullWidth
            slotProps={{
                paper: {
                    elevation: 0,
                    sx: {
                        border: "1px solid",
                        borderColor: "divider",
                        borderRadius: 2,
                        bgcolor: "background.paper",
                    },
                }
            }}
        >
            <DialogTitle sx={{ p: 2.5, pb: 2 }}>
                <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
                    <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
                        <Box>
                            <Typography variant="h6" fontWeight={600}>
                                Create Enrollment Token
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                                Generate a token for agent enrollment
                            </Typography>
                        </Box>
                    </Box>
                    <IconButton size="small" onClick={handleClose} sx={{ color: "text.secondary" }}>
                        <CloseIcon fontSize="small" />
                    </IconButton>
                </Box>
            </DialogTitle>
            <Divider />
            <DialogContent sx={{ p: 2.5 }}>
                <Box sx={{ display: "flex", flexDirection: "column", gap: 2.5 }}>
                    {error && (
                        <Alert severity="error" sx={{ borderRadius: 1 }}>
                            {error}
                        </Alert>
                    )}

                    {!tokenResponse ? (
                        <Paper
                            variant="outlined"
                            sx={{
                                p: 2,
                                borderRadius: 1,
                                border: "1px solid",
                                borderColor: "divider",
                                bgcolor: "background.default",
                            }}
                        >
                            <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 1 }}>
                                        Agent Name
                                    </Typography>
                                    <TextField
                                        autoFocus
                                        fullWidth
                                        size="small"
                                        value={agentName}
                                        onChange={(e) => setAgentName(e.target.value)}
                                        disabled={loading}
                                        placeholder="Enter agent name..."
                                    />
                                    <Typography variant="caption" color="text.secondary">
                                        Name for the agent that will use this token
                                    </Typography>
                                </Box>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 1 }}>
                                        Validity (Hours)
                                    </Typography>
                                    <TextField
                                        type="number"
                                        fullWidth
                                        size="small"
                                        value={validityHours}
                                        onChange={(e) => setValidityHours(Number(e.target.value))}
                                        disabled={loading}
                                        slotProps={{ htmlInput: { min: 1 } }}
                                    />
                                    <Typography variant="caption" color="text.secondary">
                                        How long the token will be valid
                                    </Typography>
                                </Box>
                            </Box>
                        </Paper>
                    ) : (
                        <Paper
                            variant="outlined"
                            sx={{
                                p: 2,
                                borderRadius: 1,
                                border: "1px solid",
                                borderColor: "success.main",
                                bgcolor: "background.default",
                            }}
                        >
                            <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                                <Alert severity="success" sx={{ borderRadius: 1 }} icon={null}>
                                    Enrollment token created successfully!
                                </Alert>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 1 }}>
                                        Agent Name
                                    </Typography>
                                    <Typography variant="body1" sx={{ fontFamily: "monospace" }}>
                                        {tokenResponse.agentName}
                                    </Typography>
                                </Box>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 1 }}>
                                        Expires
                                    </Typography>
                                    <Typography variant="body1">
                                        {new Date(tokenResponse.expiresAt).toLocaleString()}
                                    </Typography>
                                </Box>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 1 }}>
                                        Enrollment Token
                                    </Typography>
                                    <TextField
                                        fullWidth
                                        size="small"
                                        value={tokenResponse.token}
                                        slotProps={{
                                            input: {
                                                readOnly: true,
                                                endAdornment: (
                                                    <InputAdornment position="end">
                                                        <IconButton onClick={handleCopyToken} edge="end" size="small">
                                                            <ContentCopyIcon fontSize="small" />
                                                        </IconButton>
                                                    </InputAdornment>
                                                ),
                                            }
                                        }}
                                        sx={{
                                            "& .MuiInputBase-input": {
                                                fontFamily: "monospace",
                                                fontSize: "0.875rem",
                                            },
                                        }}
                                    />
                                    <Typography variant="caption" color="text.secondary">
                                        {copied ? "✓ Copied to clipboard!" : "Click icon to copy token"}
                                    </Typography>
                                </Box>
                                <Alert severity="warning" sx={{ borderRadius: 1 }}>
                                    Save this token securely. It won't be shown again.
                                </Alert>
                            </Box>
                        </Paper>
                    )}
                </Box>
            </DialogContent>
            <Divider />
            <DialogActions sx={{ p: 2, gap: 1 }}>
                {!tokenResponse ? (
                    <>
                        <Button onClick={handleClose} variant="outlined" disabled={loading}>
                            Cancel
                        </Button>
                        <Button onClick={handleCreate} variant="contained" disabled={loading}>
                            {loading ? "Creating..." : "Create"}
                        </Button>
                    </>
                ) : (
                    <Button onClick={handleClose} variant="contained">
                        Close
                    </Button>
                )}
            </DialogActions>
        </Dialog>
    );
}
