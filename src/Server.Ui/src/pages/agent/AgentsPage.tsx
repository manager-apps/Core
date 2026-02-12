import React, { useEffect, useState } from "react";
import {Box, Typography, Avatar, Card, CardContent, Button, Chip} from "@mui/material";
import { type AgentResponse } from "../../types/agent";
import { fetchAgents } from "../../api/agent";
import FetchContentWrapper from "../../components/wrappers/FetchContentWrapper";
import { StyledTable, type StyledTableColumn } from "../../components/table/StyledTable";
import { useNavigate } from "react-router-dom";
import ComputerIcon from "@mui/icons-material/Computer";
import AddIcon from "@mui/icons-material/Add";
import { CreateEnrollmentTokenDialog } from "./components/CreateEnrollmentTokenDialog";
import UpdateIcon from "@mui/icons-material/Update";
import LabelIcon from "@mui/icons-material/Label";

const columns: StyledTableColumn<AgentResponse>[] =
[
  { id: "id", label: "ID", minWidth: 80 },
  { id: "name", label: "Name", minWidth: 150 },
  {
    id: "version",
    label: "Version",
    minWidth: 150,
    render: (value) => (
      <Chip icon={<UpdateIcon />} label={`v${value.version}`} size="small" color="primary" variant="outlined" />
    )
  },
  {
    id: "sourceTag",
    label: "Source Tag",
    minWidth: 150,
    render: (value) => (
      <Chip icon={<LabelIcon />} label={value.sourceTag} size="small" variant="outlined" />
    )
  },
  {
    id: "currentTag",
    label: "Current Tag",
    minWidth: 150,
    render: (value) => (
      <Chip icon={<LabelIcon />} label={value.currentTag} size="small" variant="outlined" />
    )
  },
];

export function AgentsPage() {
    const navigate = useNavigate();
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [rows, setRows] = useState<AgentResponse[]>([]);
    const [tokenDialogOpen, setTokenDialogOpen] = useState(false);

    useEffect(() => {
        loadAgents();
    }, []);

    const loadAgents = async () => {
        try {
            setLoading(true);
            setError(null);

            const agents = await fetchAgents();
            setRows(agents);

            console.log("Fetched agents:", agents);
        } catch (err) {
            console.error("Failed to fetch agents", err);
            setError("Failed to load agents. Please try again.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <FetchContentWrapper loading={loading} error={error} onRetry={loadAgents}>
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
                        <Box sx={{ display: "flex", alignItems: "center", gap: 2, justifyContent: "space-between" }}>
                            <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
                                <Avatar sx={{ width: 56, height: 56, bgcolor: "primary.main" }}>
                                    <ComputerIcon fontSize="large" />
                                </Avatar>
                                <Box>
                                    <Typography variant="h5" fontWeight={600}>
                                        Agents
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        {rows.length} registered agent{rows.length !== 1 ? "s" : ""}
                                    </Typography>
                                </Box>
                            </Box>
                            <Button
                                variant="contained"
                                startIcon={<AddIcon />}
                                onClick={() => setTokenDialogOpen(true)}
                            >
                                Create Enrollment Token
                            </Button>
                        </Box>
                    </CardContent>
                </Card>

                <StyledTable
                    columns={columns}
                    rows={rows}
                    getRowId={(row) => row.id}
                    onRowClick={(row) => navigate(`/agents/${row.id}`)}
                    emptyMessage="No Agents Registered"
                    emptyIcon={<ComputerIcon sx={{ fontSize: 32, color: "text.secondary" }} />}
                />

                <CreateEnrollmentTokenDialog
                    open={tokenDialogOpen}
                    onClose={() => setTokenDialogOpen(false)}
                />
            </Box>
        </FetchContentWrapper>
    );
}
