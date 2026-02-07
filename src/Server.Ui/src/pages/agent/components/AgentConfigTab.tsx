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
    IconButton,
    Tooltip,
} from "@mui/material";
import { useState } from "react";
import type { ConfigResponse, ConfigUpdateRequest } from "../../../types/config";
import SettingsIcon from "@mui/icons-material/Settings";
import SaveIcon from "@mui/icons-material/Save";
import RestoreIcon from "@mui/icons-material/Restore";
import EditIcon from "@mui/icons-material/Edit";
import { MultiSelectDialog } from "../../../components/dialog/MultiSelectDialog";

const AVAILABLE_COLLECTORS = ["cpu_usage", "disk_usage", "memory_usage"];
const AVAILABLE_INSTRUCTIONS = ["Gpo", "Shell"];

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
    const [collectorsDialogOpen, setCollectorsDialogOpen] = useState(false);
    const [instructionsDialogOpen, setInstructionsDialogOpen] = useState(false);

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

                    <Grid container spacing={3}>
                        {/* Left column - Timing Settings */}
                        <Grid size={{ xs: 12, md: 6 }}>
                            <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
                                Timing Settings
                            </Typography>
                            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                                Configure intervals for agent state transitions
                            </Typography>
                            <Box sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 0.5 }}>
                                        Authentication Exit Interval
                                    </Typography>
                                    <Typography variant="caption" color="text.secondary" sx={{ display: "block", mb: 1 }}>
                                        Time the agent waits before retrying authentication after a failed attempt
                                    </Typography>
                                    <TextField
                                        fullWidth
                                        type="number"
                                        value={formData.authenticationExitIntervalSeconds}
                                        onChange={handleChange("authenticationExitIntervalSeconds")}
                                        size="small"
                                        slotProps={{ input: { endAdornment: <Typography variant="caption" color="text.secondary">sec</Typography> } }}
                                    />
                                </Box>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 0.5 }}>
                                        Running Exit Interval
                                    </Typography>
                                    <Typography variant="caption" color="text.secondary" sx={{ display: "block", mb: 1 }}>
                                        Delay between each iteration of the running state loop (collecting metrics, checking instructions)
                                    </Typography>
                                    <TextField
                                        fullWidth
                                        type="number"
                                        value={formData.runningExitIntervalSeconds}
                                        onChange={handleChange("runningExitIntervalSeconds")}
                                        size="small"
                                        slotProps={{ input: { endAdornment: <Typography variant="caption" color="text.secondary">sec</Typography> } }}
                                    />
                                </Box>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 0.5 }}>
                                        Execution Exit Interval
                                    </Typography>
                                    <Typography variant="caption" color="text.secondary" sx={{ display: "block", mb: 1 }}>
                                        Time to wait after executing a batch of instructions before continuing
                                    </Typography>
                                    <TextField
                                        fullWidth
                                        type="number"
                                        value={formData.executionExitIntervalSeconds}
                                        onChange={handleChange("executionExitIntervalSeconds")}
                                        size="small"
                                        slotProps={{ input: { endAdornment: <Typography variant="caption" color="text.secondary">sec</Typography> } }}
                                    />
                                </Box>
                            </Box>
                        </Grid>

                        <Grid size={{ xs: 12, md: 6 }}>
                            <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
                                Limits
                            </Typography>
                            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                                Control batch sizes for instruction execution and data transmission
                            </Typography>
                            <Box sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 0.5 }}>
                                        Instructions Execution Limit
                                    </Typography>
                                    <Typography variant="caption" color="text.secondary" sx={{ display: "block", mb: 1 }}>
                                        Maximum number of instructions the agent will execute in a single iteration
                                    </Typography>
                                    <TextField
                                        fullWidth
                                        type="number"
                                        value={formData.instructionsExecutionLimit}
                                        onChange={handleChange("instructionsExecutionLimit")}
                                        size="small"
                                    />
                                </Box>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 0.5 }}>
                                        Instruction Results Send Limit
                                    </Typography>
                                    <Typography variant="caption" color="text.secondary" sx={{ display: "block", mb: 1 }}>
                                        Maximum number of instruction results to send to the server per batch
                                    </Typography>
                                    <TextField
                                        fullWidth
                                        type="number"
                                        value={formData.instructionResultsSendLimit}
                                        onChange={handleChange("instructionResultsSendLimit")}
                                        size="small"
                                    />
                                </Box>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 0.5 }}>
                                        Metrics Send Limit
                                    </Typography>
                                    <Typography variant="caption" color="text.secondary" sx={{ display: "block", mb: 1 }}>
                                        Maximum number of collected metrics to send to the server per batch
                                    </Typography>
                                    <TextField
                                        fullWidth
                                        type="number"
                                        value={formData.metricsSendLimit}
                                        onChange={handleChange("metricsSendLimit")}
                                        size="small"
                                    />
                                </Box>
                            </Box>
                        </Grid>

                        <Grid size={12}>
                            <Divider sx={{ my: 2 }} />
                            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                <Typography variant="subtitle1" fontWeight="bold">
                                    Allowed Collectors
                                </Typography>
                                <Tooltip title="Edit collectors">
                                    <IconButton size="small" onClick={() => setCollectorsDialogOpen(true)}>
                                        <EditIcon fontSize="small" />
                                    </IconButton>
                                </Tooltip>
                            </Box>
                            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                                Metric collectors enabled for this agent
                            </Typography>
                            <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1, mt: 1 }}>
                                {formData.allowedCollectors && formData.allowedCollectors.length > 0 ? (
                                    formData.allowedCollectors.map((collector, index) => (
                                        <Chip key={index} label={collector} color="primary" variant="outlined" />
                                    ))
                                ) : (
                                    <Typography variant="body2" color="text.secondary">
                                        No collectors configured
                                    </Typography>
                                )}
                            </Box>
                        </Grid>
                        <Alert severity="info" >
                            Available collectors are based on version of the agent.
                        </Alert>

                        <Grid size={12}>
                            <Divider sx={{ my: 2 }} />
                            <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                <Typography variant="subtitle1" fontWeight="bold">
                                    Allowed Instructions
                                </Typography>
                                <Tooltip title="Edit instructions">
                                    <IconButton size="small" onClick={() => setInstructionsDialogOpen(true)}>
                                        <EditIcon fontSize="small" />
                                    </IconButton>
                                </Tooltip>
                            </Box>
                            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                                Instruction types this agent can execute
                            </Typography>
                            <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1, mt: 1 }}>
                                {formData.allowedInstructions && formData.allowedInstructions.length > 0 ? (
                                    formData.allowedInstructions.map((instruction, index) => (
                                        <Chip key={index} label={instruction} color="secondary" variant="outlined" />
                                    ))
                                ) : (
                                    <Typography variant="body2" color="text.secondary">
                                        No instructions configured
                                    </Typography>
                                )}
                            </Box>
                        </Grid>
                        <Alert severity="info">
                            Instruction type <strong>Config</strong> is always allowed
                        </Alert>
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

            <MultiSelectDialog
                open={collectorsDialogOpen}
                title="Select Allowed Collectors"
                options={AVAILABLE_COLLECTORS}
                selected={formData.allowedCollectors || []}
                onClose={() => setCollectorsDialogOpen(false)}
                onSave={(selected) => setFormData((prev) => ({ ...prev, allowedCollectors: selected }))}
            />

            <MultiSelectDialog
                open={instructionsDialogOpen}
                title="Select Allowed Instructions"
                options={AVAILABLE_INSTRUCTIONS}
                selected={formData.allowedInstructions || []}
                onClose={() => setInstructionsDialogOpen(false)}
                onSave={(selected) => setFormData((prev) => ({ ...prev, allowedInstructions: selected }))}
            />
        </Box>
    );
};
