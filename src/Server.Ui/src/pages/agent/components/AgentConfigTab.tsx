import {
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    Grid,
    TextField,
    Typography,
    Divider,
    Alert,
    Snackbar,
} from "@mui/material";
import { useState } from "react";
import type { ConfigResponse, ConfigUpdateRequest } from "../../../types/config";
import SettingsIcon from "@mui/icons-material/Settings";
import SaveIcon from "@mui/icons-material/Save";
import RestoreIcon from "@mui/icons-material/Restore";

interface AgentConfigTabProps {
    config: ConfigResponse;
    onSave: (config: ConfigUpdateRequest) => Promise<void>;
}

export const AgentConfigTab: React.FC<AgentConfigTabProps> = ({ config, onSave }) => {
    const [formData, setFormData] = useState<ConfigUpdateRequest>({
        authenticationExitIntervalSeconds: config.authenticationExitIntervalSeconds,
        runningExitIntervalSeconds: config.runningExitIntervalSeconds,
        executionExitIntervalSeconds: config.executionExitIntervalSeconds,
        instructionsExecutionLimit: config.instructionsExecutionLimit,
        instructionResultsSendLimit: config.instructionResultsSendLimit,
        metricsSendLimit: config.metricsSendLimit,
        allowedCollectors: [...config.allowedCollectors],
        allowedInstructions: [...config.allowedInstructions],
    });

    const [saving, setSaving] = useState(false);
    const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: "success" | "error" }>({
        open: false,
        message: "",
        severity: "success",
    });

    const handleChange = (field: keyof ConfigUpdateRequest) => (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.type === "number" ? Number(e.target.value) : e.target.value;
        setFormData((prev) => ({ ...prev, [field]: value }));
    };

    const handleReset = () => {
        setFormData({
            authenticationExitIntervalSeconds: config.authenticationExitIntervalSeconds,
            runningExitIntervalSeconds: config.runningExitIntervalSeconds,
            executionExitIntervalSeconds: config.executionExitIntervalSeconds,
            instructionsExecutionLimit: config.instructionsExecutionLimit,
            instructionResultsSendLimit: config.instructionResultsSendLimit,
            metricsSendLimit: config.metricsSendLimit,
            allowedCollectors: [...config.allowedCollectors],
            allowedInstructions: [...config.allowedInstructions],
        });
    };

    const handleSave = async () => {
        setSaving(true);
        try {
            await onSave(formData);
            setSnackbar({ open: true, message: "Configuration saved successfully!", severity: "success" });
        } catch {
            setSnackbar({ open: true, message: "Failed to save configuration", severity: "error" });
        } finally {
            setSaving(false);
        }
    };

    const handleCloseSnackbar = () => {
        setSnackbar((prev) => ({ ...prev, open: false }));
    };

    return (
        <Box sx={{ p: 2 }}>
            <Card elevation={2}>
                <CardContent>
                    <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between", mb: 2 }}>
                        <Box sx={{ display: "flex", alignItems: "center" }}>
                            <SettingsIcon sx={{ mr: 1, color: "primary.main" }} />
                            <Typography variant="h6">Agent Configuration</Typography>
                        </Box>
                        <Box sx={{ display: "flex", gap: 1 }}>
                            <Button
                                variant="outlined"
                                startIcon={<RestoreIcon />}
                                onClick={handleReset}
                                disabled={saving}
                            >
                                Reset
                            </Button>
                            <Button
                                variant="contained"
                                startIcon={<SaveIcon />}
                                onClick={handleSave}
                                disabled={saving}
                            >
                                {saving ? "Saving..." : "Save"}
                            </Button>
                        </Box>
                    </Box>

                    <Divider sx={{ mb: 3 }} />

                    {/* Read-only info */}
                    <Alert severity="info" sx={{ mb: 3 }}>
                        <strong>Iteration Delay:</strong> {config.iterationDelaySeconds} seconds (read-only)
                    </Alert>

                    <Grid container spacing={3}>
                        {/* Timing Settings */}
                        <Grid size={12}>
                            <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
                                Timing Settings
                            </Typography>
                            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                                Configure intervals for agent state transitions
                            </Typography>
                        </Grid>

                        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                            <TextField
                                fullWidth
                                label="Authentication Exit Interval"
                                type="number"
                                value={formData.authenticationExitIntervalSeconds}
                                onChange={handleChange("authenticationExitIntervalSeconds")}
                                size="small"
                                helperText="Seconds to wait before retrying authentication"
                                slotProps={{ input: { endAdornment: <Typography variant="caption" color="text.secondary">sec</Typography> } }}
                            />
                        </Grid>

                        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                            <TextField
                                fullWidth
                                label="Running Exit Interval"
                                type="number"
                                value={formData.runningExitIntervalSeconds}
                                onChange={handleChange("runningExitIntervalSeconds")}
                                size="small"
                                helperText="Delay between running state iterations"
                                slotProps={{ input: { endAdornment: <Typography variant="caption" color="text.secondary">sec</Typography> } }}
                            />
                        </Grid>

                        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                            <TextField
                                fullWidth
                                label="Execution Exit Interval"
                                type="number"
                                value={formData.executionExitIntervalSeconds}
                                onChange={handleChange("executionExitIntervalSeconds")}
                                size="small"
                                helperText="Delay after executing instructions"
                                slotProps={{ input: { endAdornment: <Typography variant="caption" color="text.secondary">sec</Typography> } }}
                            />
                        </Grid>

                        {/* Limits Settings */}
                        <Grid size={12}>
                            <Divider sx={{ my: 2 }} />
                            <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
                                Limits
                            </Typography>
                            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                                Control batch sizes for instruction execution and data transmission
                            </Typography>
                        </Grid>

                        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                            <TextField
                                fullWidth
                                label="Instructions Execution Limit"
                                type="number"
                                value={formData.instructionsExecutionLimit}
                                onChange={handleChange("instructionsExecutionLimit")}
                                size="small"
                                helperText="Max instructions to execute per iteration"
                            />
                        </Grid>

                        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                            <TextField
                                fullWidth
                                label="Instruction Results Send Limit"
                                type="number"
                                value={formData.instructionResultsSendLimit}
                                onChange={handleChange("instructionResultsSendLimit")}
                                size="small"
                                helperText="Max instruction results to send per batch"
                            />
                        </Grid>

                        <Grid size={{ xs: 12, sm: 6, md: 4 }}>
                            <TextField
                                fullWidth
                                label="Metrics Send Limit"
                                type="number"
                                value={formData.metricsSendLimit}
                                onChange={handleChange("metricsSendLimit")}
                                size="small"
                                helperText="Max metrics to send per batch"
                            />
                        </Grid>

                        {/* Allowed Lists */}
                        <Grid size={12}>
                            <Divider sx={{ my: 2 }} />
                            <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
                                Allowed Collectors
                            </Typography>
                            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                                Metric collectors enabled for this agent
                            </Typography>
                            <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1, mt: 1 }}>
                                {config.allowedCollectors.length > 0 ? (
                                    config.allowedCollectors.map((collector, index) => (
                                        <Chip key={index} label={collector} color="primary" variant="outlined" />
                                    ))
                                ) : (
                                    <Typography variant="body2" color="text.secondary">
                                        No collectors configured
                                    </Typography>
                                )}
                            </Box>
                        </Grid>

                        <Grid size={12}>
                            <Divider sx={{ my: 2 }} />
                            <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
                                Allowed Instructions
                            </Typography>
                            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                                Instruction types this agent can execute
                            </Typography>
                            <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1, mt: 1 }}>
                                {config.allowedInstructions.length > 0 ? (
                                    config.allowedInstructions.map((instruction, index) => (
                                        <Chip key={index} label={instruction} color="secondary" variant="outlined" />
                                    ))
                                ) : (
                                    <Typography variant="body2" color="text.secondary">
                                        No instructions configured
                                    </Typography>
                                )}
                            </Box>
                        </Grid>
                    </Grid>
                </CardContent>
            </Card>

            <Snackbar
                open={snackbar.open}
                autoHideDuration={4000}
                onClose={handleCloseSnackbar}
                anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
            >
                <Alert onClose={handleCloseSnackbar} severity={snackbar.severity} sx={{ width: "100%" }}>
                    {snackbar.message}
                </Alert>
            </Snackbar>
        </Box>
    );
};
