import {
    Box,
    Card,
    CardContent,
    Chip,
    Grid,
    Typography,
    Switch,
    FormControlLabel,
    Avatar,
    Paper,
    Divider,
} from "@mui/material";
import type { AgentDetailResponse, AgentState } from "../../../types/agent";
import { AgentState as AgentStateEnum } from "../../../types/agent";
import ComputerIcon from "@mui/icons-material/Computer";
import MemoryIcon from "@mui/icons-material/Memory";
import StorageIcon from "@mui/icons-material/Storage";
import AccessTimeIcon from "@mui/icons-material/AccessTime";
import UpdateIcon from "@mui/icons-material/Update";
import LabelIcon from "@mui/icons-material/Label";
import DnsIcon from "@mui/icons-material/Dns";
import { useState } from "react";

interface AgentOverviewTabProps {
    agent: AgentDetailResponse;
    onStateChange: (newState: AgentState) => Promise<void>;
}

const formatBytes = (bytes: number): string => {
    const gb = bytes / (1024 * 1024 * 1024);
    return `${gb.toFixed(2)} GB`;
};

const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleString();
};

const formatRelativeTime = (dateString: string): string => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffMins < 1) return "Just now";
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    return `${diffDays}d ago`;
};

interface StatCardProps {
    icon: React.ReactNode;
    title: string;
    value: string | number;
    subtitle?: string;
    color?: string;
}

const StatCard: React.FC<StatCardProps> = ({ icon, title, value, subtitle, color = "primary.main" }) => (
    <Paper
        elevation={0}
        sx={{
            p: 2,
            border: "1px solid",
            borderColor: "divider",
            borderRadius: 2,
            height: "100%",
        }}
    >
        <Box sx={{ display: "flex", alignItems: "flex-start", gap: 2 }}>
            <Avatar sx={{ bgcolor: color, width: 48, height: 48 }}>{icon}</Avatar>
            <Box sx={{ flex: 1, minWidth: 0 }}>
                <Typography variant="caption" color="text.secondary" sx={{ textTransform: "uppercase", letterSpacing: 0.5 }}>
                    {title}
                </Typography>
                <Typography variant="h6" fontWeight={600} sx={{ mt: 0.5, overflow: "hidden", textOverflow: "ellipsis" }}>
                    {value}
                </Typography>
                {subtitle && (
                    <Typography variant="caption" color="text.secondary">
                        {subtitle}
                    </Typography>
                )}
            </Box>
        </Box>
    </Paper>
);

export const AgentOverviewTab: React.FC<AgentOverviewTabProps> = ({ agent, onStateChange }) => {
    const [loading, setLoading] = useState(false);

    const handleStateToggle = async () => {
        setLoading(true);
        try {
            const newState = agent.state === AgentStateEnum.Active ? AgentStateEnum.Inactive : AgentStateEnum.Active;
            await onStateChange(newState);
        } finally {
            setLoading(false);
        }
    };

    const isActive = agent.state === AgentStateEnum.Active;

    return (
        <Box sx={{ p: 2 }}>
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
                    <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between", flexWrap: "wrap", gap: 2 }}>
                        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
                            <Avatar
                                sx={{
                                    width: 64,
                                    height: 64,
                                    bgcolor: isActive ? "success.main" : "grey.400",
                                    fontSize: 28,
                                }}
                            >
                                <DnsIcon fontSize="large" />
                            </Avatar>
                            <Box>
                                <Typography variant="h5" fontWeight={600}>
                                    {agent.name}
                                </Typography>
                                <Box sx={{ display: "flex", alignItems: "center", gap: 1, mt: 0.5 }}>
                                    <Chip
                                        label={isActive ? "Active" : "Inactive"}
                                        color={isActive ? "success" : "default"}
                                        size="small"
                                        sx={{ fontWeight: 500 }}
                                    />
                                    <Typography variant="body2" color="text.secondary">
                                        v{agent.version}
                                    </Typography>
                                </Box>
                            </Box>
                        </Box>
                        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
                            <Box sx={{ textAlign: "right" }}>
                                <Typography variant="caption" color="text.secondary">
                                    Last updated
                                </Typography>
                                <Typography variant="body2" fontWeight={500}>
                                    {formatRelativeTime(agent.lastUpdatedAt)}
                                </Typography>
                            </Box>
                            <FormControlLabel
                                control={
                                    <Switch checked={isActive} onChange={handleStateToggle} disabled={loading} color="success" />
                                }
                                label=""
                                sx={{ m: 0 }}
                            />
                        </Box>
                    </Box>
                </CardContent>
            </Card>

            {/* Stats Grid */}
            <Grid container spacing={2} sx={{ mb: 3 }}>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <StatCard
                        icon={<ComputerIcon />}
                        title="Machine"
                        value={agent.hardware.machineName || "Unknown"}
                        color="primary.main"
                    />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <StatCard
                        icon={<MemoryIcon />}
                        title="Processors"
                        value={agent.hardware.processorCount}
                        subtitle="CPU Cores"
                        color="secondary.main"
                    />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <StatCard
                        icon={<StorageIcon />}
                        title="Memory"
                        value={formatBytes(agent.hardware.totalMemoryBytes)}
                        subtitle="Total RAM"
                        color="info.main"
                    />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <StatCard
                        icon={<AccessTimeIcon />}
                        title="Created"
                        value={formatRelativeTime(agent.createdAt)}
                        subtitle={formatDate(agent.createdAt)}
                        color="warning.main"
                    />
                </Grid>
            </Grid>

            {/* Details Cards */}
            <Grid container spacing={3}>
                <Grid size={{ xs: 12, md: 6 }}>
                    <Card elevation={0} sx={{ border: "1px solid", borderColor: "divider", borderRadius: 2, height: "100%" }}>
                        <CardContent>
                            <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>
                                Agent Details
                            </Typography>
                            <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                                <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                                    <Typography variant="body2" color="text.secondary">
                                        Agent ID
                                    </Typography>
                                    <Typography variant="body2" fontWeight={500}>
                                        #{agent.id}
                                    </Typography>
                                </Box>
                                <Divider />
                                <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                                    <Typography variant="body2" color="text.secondary">
                                        Current Tag
                                    </Typography>
                                    {agent.currentTag ? (
                                        <Chip icon={<LabelIcon />} label={agent.currentTag} size="small" variant="outlined" />
                                    ) : (
                                        <Typography variant="body2" color="text.disabled">
                                            Not set
                                        </Typography>
                                    )}
                                </Box>
                                <Divider />
                                <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                                    <Typography variant="body2" color="text.secondary">
                                        Source Tag
                                    </Typography>
                                    {agent.sourceTag ? (
                                        <Chip icon={<LabelIcon />} label={agent.sourceTag} size="small" variant="outlined" />
                                    ) : (
                                        <Typography variant="body2" color="text.disabled">
                                            Not set
                                        </Typography>
                                    )}
                                </Box>
                                <Divider />
                                <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                                    <Typography variant="body2" color="text.secondary">
                                        Version
                                    </Typography>
                                    <Chip icon={<UpdateIcon />} label={`v${agent.version}`} size="small" color="primary" variant="outlined" />
                                </Box>
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>

                <Grid size={{ xs: 12, md: 6 }}>
                    <Card elevation={0} sx={{ border: "1px solid", borderColor: "divider", borderRadius: 2, height: "100%" }}>
                        <CardContent>
                            <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>
                                System Information
                            </Typography>
                            <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                                <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                                    <Typography variant="body2" color="text.secondary">
                                        Machine Name
                                    </Typography>
                                    <Typography variant="body2" fontWeight={500}>
                                        {agent.hardware.machineName || "-"}
                                    </Typography>
                                </Box>
                                <Divider />
                                <Box>
                                    <Box sx={{ display: "flex", justifyContent: "space-between", mb: 1 }}>
                                        <Typography variant="body2" color="text.secondary">
                                            Operating System
                                        </Typography>
                                    </Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ wordBreak: "break-word" }}>
                                        {agent.hardware.osVersion || "-"}
                                    </Typography>
                                </Box>
                                <Divider />
                                <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                                    <Typography variant="body2" color="text.secondary">
                                        Hardware Updated
                                    </Typography>
                                    <Typography variant="body2" fontWeight={500}>
                                        {agent.hardware.updatedAt ? formatRelativeTime(agent.hardware.updatedAt) : "-"}
                                    </Typography>
                                </Box>
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
            </Grid>
        </Box>
    );
};
