import { Box, Card, CardContent, Chip, Grid, Typography, Switch, FormControlLabel } from "@mui/material";
import { KeyValueTable } from "../../../components/table/KeyValueTable";
import type { AgentDetailResponse, AgentState } from "../../../types/agent";
import { AgentState as AgentStateEnum } from "../../../types/agent";
import ComputerIcon from "@mui/icons-material/Computer";
import MemoryIcon from "@mui/icons-material/Memory";
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

const getStateChip = (state: number) => {
    switch (state) {
        case AgentStateEnum.Active:
            return <Chip label="Active" color="success" size="small" />;
        case AgentStateEnum.Inactive:
            return <Chip label="Inactive" color="default" size="small" />;
        default:
            return <Chip label="Unknown" color="warning" size="small" />;
    }
};

export const AgentOverviewTab: React.FC<AgentOverviewTabProps> = ({ agent, onStateChange }) => {
    const [loading, setLoading] = useState(false);

    const handleStateToggle = async () => {
        setLoading(true);
        try {
            const newState = agent.state === AgentStateEnum.Active
                ? AgentStateEnum.Inactive
                : AgentStateEnum.Active;
            await onStateChange(newState);
        } finally {
            setLoading(false);
        }
    };

    const stateControl = (
        <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
            {getStateChip(agent.state)}
            <FormControlLabel
                control={
                    <Switch
                        checked={agent.state === AgentStateEnum.Active}
                        onChange={handleStateToggle}
                        disabled={loading}
                        size="small"
                    />
                }
                label=""
                sx={{ ml: 1, mr: 0 }}
            />
        </Box>
    );

    const agentInfoRows = [
        { key: "ID", value: agent.id },
        { key: "Name", value: agent.name },
        { key: "State", value: stateControl },
        { key: "Version", value: agent.version },
        { key: "Current Tag", value: agent.currentTag || "-" },
        { key: "Source Tag", value: agent.sourceTag || "-" },
        { key: "Created At", value: formatDate(agent.createdAt) },
        { key: "Last Updated", value: formatDate(agent.lastUpdatedAt) },
    ];

    const hardwareRows = [
        { key: "Machine Name", value: agent.hardware.machineName || "-" },
        { key: "OS Version", value: agent.hardware.osVersion || "-" },
        { key: "Processor Count", value: agent.hardware.processorCount },
        { key: "Total Memory", value: formatBytes(agent.hardware.totalMemoryBytes) },
        { key: "Last Updated", value: agent.hardware.updatedAt ? formatDate(agent.hardware.updatedAt) : "-" },
    ];

    return (
        <Box sx={{ p: 2 }}>
            <Grid container spacing={3}>
                {/* Agent Info Card */}
                <Grid size={{ xs: 12, md: 6 }}>
                    <Card elevation={2}>
                        <CardContent>
                            <Box sx={{ display: "flex", alignItems: "center", mb: 2 }}>
                                <ComputerIcon sx={{ mr: 1, color: "primary.main" }} />
                                <Typography variant="h6">Agent Information</Typography>
                            </Box>
                            <KeyValueTable rows={agentInfoRows} />
                        </CardContent>
                    </Card>
                </Grid>

                {/* Hardware Info Card */}
                <Grid size={{ xs: 12, md: 6 }}>
                    <Card elevation={2}>
                        <CardContent>
                            <Box sx={{ display: "flex", alignItems: "center", mb: 2 }}>
                                <MemoryIcon sx={{ mr: 1, color: "secondary.main" }} />
                                <Typography variant="h6">Hardware Information</Typography>
                            </Box>
                            <KeyValueTable rows={hardwareRows} />
                        </CardContent>
                    </Card>
                </Grid>
            </Grid>
        </Box>
    );
};
