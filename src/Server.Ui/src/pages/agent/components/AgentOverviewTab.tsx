
import ComputerIcon from "@mui/icons-material/Computer";
import MemoryIcon from "@mui/icons-material/Memory";
import StorageIcon from "@mui/icons-material/Storage";
import AccessTimeIcon from "@mui/icons-material/AccessTime";
import UpdateIcon from "@mui/icons-material/Update";
import LabelIcon from "@mui/icons-material/Label";
import React from "react";
import type {HardwareResponse} from "../../../types/hardware.ts";
import {
    Box,
    Card,
    CardContent,
    Chip,
    Grid,
    Typography,
    Avatar,
    Paper,
    Divider,
} from "@mui/material";

interface AgentOverviewTabProps {
    agentId: number;
    agentVersion: string;
    agentName: string;
    agentCreatedAt: string;
    agentSourceTag: string;
    agentCurrentTag: string;
    agentLastUpdatedAt: string | undefined;
    hardware: HardwareResponse;
}

const formatBytes = (bytes: number): string => {
    const gb = bytes / (1024 * 1024 * 1024);
    return `${gb.toFixed(2)} GB`;
};

const formatDate = (dateString: string | undefined): string => {
    if (!dateString) 
        return "-";
    
    return new Date(dateString).toLocaleString();
};

const formatRelativeTime = (dateString: string | undefined): string => {
    if (!dateString) 
        return "-";
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

export const AgentOverviewTab: React.FC<AgentOverviewTabProps> = ({
    agentId,
    agentVersion,
    agentName,
    agentLastUpdatedAt,
    agentCreatedAt,
    agentCurrentTag,
    agentSourceTag,
    hardware,
}) => {
    return (
        <Box sx={{ p: 2 }}>
            <Grid container spacing={2} sx={{ mb: 3 }}>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <StatCard
                        icon={<ComputerIcon />}
                        title="Machine"
                        value={hardware.machineName || "Unknown"}
                        color="primary.main"
                    />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <StatCard
                        icon={<MemoryIcon />}
                        title="Processors"
                        value={hardware.processorCount}
                        subtitle="CPU Cores"
                        color="secondary.main"
                    />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <StatCard
                        icon={<StorageIcon />}
                        title="Memory"
                        value={formatBytes(hardware.totalMemoryBytes)}
                        subtitle="Total RAM"
                        color="info.main"
                    />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <StatCard
                        icon={<AccessTimeIcon />}
                        title="Created / Updated"
                        value={formatRelativeTime(agentCreatedAt) + " / " + formatRelativeTime(agentLastUpdatedAt)}
                        subtitle={formatDate(agentCreatedAt) + " / " + formatDate(agentLastUpdatedAt)}
                        color="warning.main"
                    />
                </Grid>
            </Grid>

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
                                        #{agentId}
                                    </Typography>
                                </Box>
                                <Divider />

                                <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                                    <Typography variant="body2" color="text.secondary">
                                        Name
                                    </Typography>
                                    <Typography variant="body2" fontWeight={500}>
                                        {agentName}
                                    </Typography>
                                </Box>
                                <Divider />
                                <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                                    <Typography variant="body2" color="text.secondary">
                                        Current Tag
                                    </Typography>
                                    {agentCurrentTag ? (
                                        <Chip icon={<LabelIcon />} label={agentCurrentTag} size="small" variant="outlined" />
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
                                    {agentSourceTag ? (
                                        <Chip icon={<LabelIcon />} label={agentSourceTag} size="small" variant="outlined" />
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
                                    <Chip icon={<UpdateIcon />} label={`v${agentVersion}`} size="small" color="primary" variant="outlined" />
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
                                        {hardware.machineName || "-"}
                                    </Typography>
                                </Box>
                                <Divider />
                                <Box  sx={{ display: "flex", justifyContent: "space-between" }}>
                                    <Typography variant="body2" color="text.secondary">
                                        Operating System
                                    </Typography>
                                    <Typography variant="body2" fontWeight={500}>
                                        {hardware.osVersion || "-"}
                                    </Typography>
                                </Box>
                                <Divider />
                                <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                                    <Typography variant="body2" color="text.secondary">
                                        Hardware Updated
                                    </Typography>
                                    <Typography variant="body2" fontWeight={500}>
                                        {hardware.updatedAt ? formatRelativeTime(hardware.updatedAt) : "-"}
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
