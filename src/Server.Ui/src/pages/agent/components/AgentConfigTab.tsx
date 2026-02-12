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
    Avatar,
    Paper,
} from "@mui/material";
import { useState } from "react";
import type { ConfigResponse, ConfigUpdateRequest } from "../../../types/config";
import SettingsIcon from "@mui/icons-material/Settings";
import SaveIcon from "@mui/icons-material/Save";
import RestoreIcon from "@mui/icons-material/Restore";
import EditIcon from "@mui/icons-material/Edit";
import SpeedIcon from "@mui/icons-material/Speed";
import BlockIcon from "@mui/icons-material/Block";
import { MultiSelectDialog } from "../../../components/dialog/MultiSelectDialog";
import { RevokeCertificateDialog } from "./RevokeCertificateDialog";

const AVAILABLE_COLLECTORS = ["cpu_usage", "disk_usage", "memory_usage"];
const AVAILABLE_INSTRUCTIONS = ["Gpo", "Shell"];

interface AgentConfigTabProps {
    agentId: number;
    agentName: string;
    config: ConfigResponse;
    onSave: (config: ConfigUpdateRequest) => Promise<void>;
}

interface ConfigFieldProps {
    label: string;
    description: string;
    value: number;
    onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
    suffix?: string;
}

const ConfigField: React.FC<ConfigFieldProps> = ({ label, description, value, onChange, suffix }) => (
    <Box>
        <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", mb: 1 }}>
            <Box sx={{ flex: 1, pr: 2 }}>
                <Typography variant="body2" fontWeight={500}>
                    {label}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                    {description}
                </Typography>
            </Box>
            <TextField
                type="number"
                value={value}
                onChange={onChange}
                size="small"
                sx={{ width: 120 }}
                slotProps={{
                    input: suffix
                        ? {
                              endAdornment: (
                                  <Typography variant="caption" color="text.secondary">
                                      {suffix}
                                  </Typography>
                              ),
                          }
                        : undefined,
                }}
            />
        </Box>
    </Box>
);

export const AgentConfigTab: React.FC<AgentConfigTabProps> = ({ agentId, agentName, config, onSave }) => {
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
    const [revokeDialogOpen, setRevokeDialogOpen] = useState(false);

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
            {/* Header Card */}
            <Card
                elevation={0}
                sx={{
                    mb: 3,
                    border: "1px solid",
                    borderColor: "divider",
                    borderRadius: 2,
                }}
            >
                <CardContent sx={{ p: 3 }}>
                    <Box
                        sx={{ display: "flex", alignItems: "center", justifyContent: "space-between", flexWrap: "wrap", gap: 2 }}
                    >
                        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
                            <Avatar sx={{ width: 56, height: 56, bgcolor: "secondary.main" }}>
                                <SettingsIcon fontSize="large" />
                            </Avatar>
                            <Box>
                                <Typography variant="h5" fontWeight={600}>
                                    Agent Configuration
                                </Typography>
                                <Typography variant="body2" color="text.secondary">
                                    Manage timing, limits, and permissions for this agent
                                </Typography>
                            </Box>
                        </Box>
                        <Box sx={{ display: "flex", gap: 1 }}>
                            <Button variant="outlined" startIcon={<RestoreIcon />} onClick={handleReset} disabled={saving}>
                                Reset
                            </Button>
                            <Button variant="contained" startIcon={<SaveIcon />} onClick={handleSave} disabled={saving}>
                                {saving ? "Saving..." : "Save Changes"}
                            </Button>
                        </Box>
                    </Box>
                </CardContent>
            </Card>

            <Grid container spacing={3}>
                <Grid size={{ xs: 12, md: 6 }}>
                    <Card elevation={0} sx={{ border: "1px solid", borderColor: "divider", borderRadius: 2, height: "100%" }}>
                        <CardContent>
                            <Box sx={{ display: "flex", alignItems: "center", gap: 1.5, mb: 3 }}>
                                <Box>
                                    <Typography variant="subtitle1" fontWeight={600}>
                                        Timing Settings
                                    </Typography>
                                </Box>
                            </Box>

                            <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                                <ConfigField
                                    label="Authentication Retry"
                                    description="Wait time before retrying failed authentication"
                                    value={formData.authenticationExitIntervalSeconds ?? 0}
                                    onChange={handleChange("authenticationExitIntervalSeconds")}
                                    suffix="sec"
                                />
                                <Divider />
                                <ConfigField
                                    label="Running Loop Interval"
                                    description="Delay between running state iterations"
                                    value={formData.runningExitIntervalSeconds ?? 0}
                                    onChange={handleChange("runningExitIntervalSeconds")}
                                    suffix="sec"
                                />
                                <Divider />
                                <ConfigField
                                    label="Execution Cooldown"
                                    description="Wait time after executing instructions"
                                    value={formData.executionExitIntervalSeconds ?? 0}
                                    onChange={handleChange("executionExitIntervalSeconds")}
                                    suffix="sec"
                                />
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>

                <Grid size={{ xs: 12, md: 6 }}>
                    <Card elevation={0} sx={{ border: "1px solid", borderColor: "divider", borderRadius: 2, height: "100%" }}>
                        <CardContent>
                            <Box sx={{ display: "flex", alignItems: "center", gap: 1.5, mb: 3 }}>
                                <Box>
                                    <Typography variant="subtitle1" fontWeight={600}>
                                        Batch Limits
                                    </Typography>
                                </Box>
                            </Box>

                            <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                                <ConfigField
                                    label="Instructions per Batch"
                                    description="Max instructions to execute per iteration"
                                    value={formData.instructionsExecutionLimit ?? 0}
                                    onChange={handleChange("instructionsExecutionLimit")}
                                />
                                <Divider />
                                <ConfigField
                                    label="Results per Send"
                                    description="Max instruction results per server batch"
                                    value={formData.instructionResultsSendLimit ?? 0}
                                    onChange={handleChange("instructionResultsSendLimit")}
                                />
                                <Divider />
                                <ConfigField
                                    label="Metrics per Send"
                                    description="Max metrics to send per batch"
                                    value={formData.metricsSendLimit ?? 0}
                                    onChange={handleChange("metricsSendLimit")}
                                />
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>

                {/* Collectors Card */}
                <Grid size={{ xs: 12, md: 6 }}>
                    <Card elevation={0} sx={{ border: "1px solid", borderColor: "divider", borderRadius: 2 }}>
                        <CardContent>
                            <Box
                                sx={{
                                    display: "flex",
                                    alignItems: "center",
                                    justifyContent: "space-between",
                                    mb: 2,
                                }}
                            >
                                <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
                                    <Box>
                                        <Typography variant="subtitle1" fontWeight={600}>
                                            Metric Collectors
                                        </Typography>
                                        <Typography variant="caption" color="text.secondary">
                                            {formData.allowedCollectors?.length || 0} of {AVAILABLE_COLLECTORS.length} enabled
                                        </Typography>
                                    </Box>
                                </Box>
                                <Tooltip title="Edit collectors">
                                    <IconButton onClick={() => setCollectorsDialogOpen(true)} color="primary">
                                        <EditIcon />
                                    </IconButton>
                                </Tooltip>
                            </Box>
                            <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1 }}>
                                {formData.allowedCollectors && formData.allowedCollectors.length > 0 ? (
                                    formData.allowedCollectors.map((collector, index) => (
                                        <Chip
                                            key={index}
                                            icon={<SpeedIcon />}
                                            label={collector}
                                            color="success"
                                            variant="outlined"
                                            size="small"
                                        />
                                    ))
                                ) : (
                                    <Paper
                                        variant="outlined"
                                        sx={{
                                            p: 2,
                                            width: "100%",
                                            textAlign: "center",
                                            borderStyle: "dashed",
                                        }}
                                    >
                                        <Typography variant="body2" color="text.secondary">
                                            No collectors enabled. Click edit to add.
                                        </Typography>
                                    </Paper>
                                )}
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>

                <Grid size={{ xs: 12, md: 6 }}>
                    <Card elevation={0} sx={{ border: "1px solid", borderColor: "divider", borderRadius: 2 }}>
                        <CardContent>
                            <Box
                                sx={{
                                    display: "flex",
                                    alignItems: "center",
                                    justifyContent: "space-between",
                                    mb: 2,
                                }}
                            >
                                <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
                                    <Box>
                                        <Typography variant="subtitle1" fontWeight={600}>
                                            Allowed Instructions
                                        </Typography>
                                        <Typography variant="caption" color="text.secondary">
                                            {formData.allowedInstructions?.length || 0} of {AVAILABLE_INSTRUCTIONS.length} enabled
                                        </Typography>
                                    </Box>
                                </Box>
                                <Tooltip title="Edit instructions">
                                    <IconButton onClick={() => setInstructionsDialogOpen(true)} color="primary">
                                        <EditIcon />
                                    </IconButton>
                                </Tooltip>
                            </Box>
                            <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1 }}>
                                <Chip label="Config" color="default" variant="filled" size="small" sx={{ opacity: 0.7 }} />
                                {formData.allowedInstructions && formData.allowedInstructions.length > 0 ? (
                                    formData.allowedInstructions.map((instruction, index) => (
                                        <Chip key={index} label={instruction} color="warning" variant="outlined" size="small" />
                                    ))
                                ) : null}
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
            </Grid>

            <Card elevation={0} sx={{ mt: 3, border: "1px solid", borderColor: "divider", borderRadius: 2 }}>
                <CardContent>
                    <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>
                        Security Actions
                    </Typography>
                    <Box sx={{ display: "flex", gap: 2 }}>
                        <Button
                            variant="outlined"
                            color="error"
                            startIcon={<BlockIcon />}
                            onClick={() => setRevokeDialogOpen(true)}
                        >
                            Revoke Certificate
                        </Button>
                    </Box>
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

            <RevokeCertificateDialog
                open={revokeDialogOpen}
                onClose={() => setRevokeDialogOpen(false)}
                agentId={agentId}
                agentName={agentName}
            />
        </Box>
    );
};
