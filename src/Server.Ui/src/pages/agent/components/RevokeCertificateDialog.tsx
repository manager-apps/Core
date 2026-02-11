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
    Typography,
    Divider,
    Paper,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import { revokeCertificate } from "../../../api/certificate";

interface RevokeCertificateDialogProps {
    open: boolean;
    onClose: () => void;
    agentId: number;
    agentName: string;
}

export function RevokeCertificateDialog({ open, onClose, agentId, agentName }: RevokeCertificateDialogProps) {
    const [reason, setReason] = useState("");
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState(false);

    const handleRevoke = async () => {
        if (!reason.trim()) {
            setError("Reason is required");
            return;
        }

        try {
            setLoading(true);
            setError(null);

            await revokeCertificate(agentId, { reason: reason.trim() });
            setSuccess(true);

            setTimeout(() => {
                handleClose();
            }, 2000);
        } catch (err: any) {
            console.error("Failed to revoke certificate", err);
            setError(err.response?.data?.detail || "Failed to revoke certificate");
        } finally {
            setLoading(false);
        }
    };

    const handleClose = () => {
        setReason("");
        setError(null);
        setSuccess(false);
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
                                Revoke Certificate
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                                Revoke certificates for {agentName}
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
                    {success && (
                        <Alert severity="success" sx={{ borderRadius: 1 }}>
                            Certificate(s) revoked successfully!
                        </Alert>
                    )}
                    {!success && (
                        <>
                            <Alert severity="warning" sx={{ borderRadius: 1 }}>
                                This will revoke all active certificates for agent <strong>{agentName}</strong>.
                                The agent will need to re-enroll.
                            </Alert>
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
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 1 }}>
                                        Reason
                                    </Typography>
                                    <TextField
                                        autoFocus
                                        fullWidth
                                        multiline
                                        rows={3}
                                        size="small"
                                        value={reason}
                                        onChange={(e) => setReason(e.target.value)}
                                        disabled={loading}
                                        placeholder="Enter reason for revocation..."
                                    />
                                    <Typography variant="caption" color="text.secondary">
                                        Provide a reason for revoking the certificate
                                    </Typography>
                                </Box>
                            </Paper>
                        </>
                    )}
                </Box>
            </DialogContent>
            <Divider />
            <DialogActions sx={{ p: 2, gap: 1 }}>
                {!success && (
                    <>
                        <Button onClick={handleClose} variant="outlined" disabled={loading}>
                            Cancel
                        </Button>
                        <Button onClick={handleRevoke} variant="contained" color="error" disabled={loading}>
                            {loading ? "Revoking..." : "Revoke"}
                        </Button>
                    </>
                )}
            </DialogActions>
        </Dialog>
    );
}
