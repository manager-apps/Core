import {
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    IconButton,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Paper,
    Typography,
    Tooltip,
    Collapse,
    Avatar,
    Grid,
} from "@mui/material";
import { useState } from "react";
import type { InstructionResponse, CreateShellCommandRequest, CreateGpoSetRequest } from "../../../types/instruction";
import { InstructionType, InstructionState } from "../../../types/instruction";
import PlaylistAddIcon from "@mui/icons-material/PlaylistAdd";
import AddIcon from "@mui/icons-material/Add";
import RefreshIcon from "@mui/icons-material/Refresh";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import ExpandLessIcon from "@mui/icons-material/ExpandLess";
import TerminalIcon from "@mui/icons-material/Terminal";
import PolicyIcon from "@mui/icons-material/Policy";
import SettingsIcon from "@mui/icons-material/Settings";
import PendingIcon from "@mui/icons-material/Pending";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import ErrorIcon from "@mui/icons-material/Error";
import SendIcon from "@mui/icons-material/Send";
import { CreateInstructionDialog } from "./CreateInstructionDialog";

interface AgentInstructionsTabProps {
    agentId: number;
    instructions: InstructionResponse[];
    onCreateInstruction: (type: number, payload: CreateShellCommandRequest | CreateGpoSetRequest) => Promise<void>;
    onRefresh: () => void;
}

const getInstructionTypeInfo = (type: number) => {
    switch (type) {
        case InstructionType.Shell:
            return { label: "Shell", color: "primary" as const, icon: <TerminalIcon fontSize="small" /> };
        case InstructionType.Gpo:
            return { label: "GPO", color: "secondary" as const, icon: <PolicyIcon fontSize="small" /> };
        case InstructionType.Config:
            return { label: "Config", color: "info" as const, icon: <SettingsIcon fontSize="small" /> };
        default:
            return { label: "Unknown", color: "default" as const, icon: undefined };
    }
};

const getStateChip = (state: number) => {
    switch (state) {
        case InstructionState.Pending:
            return <Chip label="Pending" color="warning" size="small" variant="outlined" />;
        case InstructionState.Dispatched:
            return <Chip label="Dispatched" color="info" size="small" variant="outlined" />;
        case InstructionState.Completed:
            return <Chip label="Completed" color="success" size="small" variant="outlined" />;
        case InstructionState.Failed:
            return <Chip label="Failed" color="error" size="small" variant="outlined" />;
        default:
            return <Chip label="Unknown" color="default" size="small" variant="outlined" />;
    }
};

const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleString();
};

const parsePayload = (payloadJson: string, type: number): string => {
    try {
        const payload = JSON.parse(payloadJson);
        if (type === InstructionType.Shell) {
            return `Command: ${payload.command}`;
        } else if (type === InstructionType.Gpo) {
            return `${payload.name}: ${payload.value}`;
        }
        return payloadJson;
    } catch {
        return payloadJson;
    }
};

interface InstructionRowProps {
    instruction: InstructionResponse;
}

const InstructionRow: React.FC<InstructionRowProps> = ({ instruction }) => {
    const [expanded, setExpanded] = useState(false);
    const typeInfo = getInstructionTypeInfo(instruction.type);

    return (
        <>
            <TableRow
                hover
                sx={{
                    cursor: "pointer",
                    bgcolor: "background.paper",
                    "&:nth-of-type(odd)": { bgcolor: "action.hover" },
                }}
                onClick={() => setExpanded(!expanded)}
            >
                <TableCell>
                    <IconButton size="small">
                        {expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                    </IconButton>
                </TableCell>
                <TableCell>
                    <Typography variant="body2" fontWeight={500}>
                        #{instruction.id}
                    </Typography>
                </TableCell>
                <TableCell>
                    <Chip
                        icon={typeInfo.icon}
                        label={typeInfo.label}
                        color={typeInfo.color}
                        size="small"
                        variant="outlined"
                    />
                </TableCell>
                <TableCell>{getStateChip(instruction.state)}</TableCell>
                <TableCell>
                    <Tooltip title={parsePayload(instruction.payloadJson, instruction.type)}>
                        <Typography
                            variant="body2"
                            sx={{
                                maxWidth: 200,
                                overflow: "hidden",
                                textOverflow: "ellipsis",
                                whiteSpace: "nowrap",
                                fontFamily: "monospace",
                                fontSize: "0.8rem",
                            }}
                        >
                            {parsePayload(instruction.payloadJson, instruction.type)}
                        </Typography>
                    </Tooltip>
                </TableCell>
                <TableCell>
                    <Typography variant="caption" color="text.secondary">
                        {formatDate(instruction.createdAt)}
                    </Typography>
                </TableCell>
            </TableRow>
            <TableRow>
                <TableCell sx={{ py: 0, borderBottom: expanded ? 1 : 0, borderColor: "divider" }} colSpan={6}>
                    <Collapse in={expanded} timeout="auto" unmountOnExit>
                        <Box sx={{ p: 2.5, bgcolor: "background.default", borderRadius: 1, my: 1 }}>
                            <Typography variant="subtitle2" fontWeight={600} gutterBottom sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                <SettingsIcon fontSize="small" color="primary" />
                                Payload
                            </Typography>
                            <Paper
                                variant="outlined"
                                sx={{
                                    p: 2,
                                    mb: 2,
                                    bgcolor: "background.paper",
                                    borderRadius: 1,
                                    border: "1px solid",
                                    borderColor: "divider",
                                }}
                            >
                                <Typography
                                    variant="body2"
                                    component="pre"
                                    sx={{ fontFamily: "monospace", whiteSpace: "pre-wrap", m: 0, color: "text.primary", fontSize: "0.85rem" }}
                                >
                                    {JSON.stringify(JSON.parse(instruction.payloadJson), null, 2)}
                                </Typography>
                            </Paper>

                            {instruction.output && (
                                <>
                                    <Typography variant="subtitle2" fontWeight={600} gutterBottom sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                        <CheckCircleIcon fontSize="small" color="success" />
                                        Output
                                    </Typography>
                                    <Paper
                                        variant="outlined"
                                        sx={{
                                            p: 2,
                                            mb: 2,
                                            bgcolor: "background.paper",
                                            borderRadius: 1,
                                            border: "1px solid",
                                            borderColor: "success.main",
                                        }}
                                    >
                                        <Typography
                                            variant="body2"
                                            component="pre"
                                            sx={{ fontFamily: "monospace", whiteSpace: "pre-wrap", m: 0, color: "success.light", fontSize: "0.85rem" }}
                                        >
                                            {instruction.output}
                                        </Typography>
                                    </Paper>
                                </>
                            )}

                            {instruction.error && (
                                <>
                                    <Typography variant="subtitle2" fontWeight={600} gutterBottom sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                        <ErrorIcon fontSize="small" color="error" />
                                        Error
                                    </Typography>
                                    <Paper
                                        variant="outlined"
                                        sx={{
                                            p: 2,
                                            bgcolor: "background.paper",
                                            borderRadius: 1,
                                            border: "1px solid",
                                            borderColor: "error.main",
                                        }}
                                    >
                                        <Typography
                                            variant="body2"
                                            component="pre"
                                            sx={{ fontFamily: "monospace", whiteSpace: "pre-wrap", m: 0, color: "error.light", fontSize: "0.85rem" }}
                                        >
                                            {instruction.error}
                                        </Typography>
                                    </Paper>
                                </>
                            )}
                        </Box>
                    </Collapse>
                </TableCell>
            </TableRow>
        </>
    );
};

interface StatCardProps {
    title: string;
    value: number;
    icon: React.ReactNode;
    color: string;
}

const StatCard: React.FC<StatCardProps> = ({ title, value, icon, color }) => (
    <Card elevation={0} sx={{ border: "1px solid", borderColor: "divider", borderRadius: 2 }}>
        <CardContent sx={{ p: 2, "&:last-child": { pb: 2 } }}>
            <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
                <Box>
                    <Typography variant="caption" color="text.secondary" textTransform="uppercase">
                        {title}
                    </Typography>
                    <Typography variant="h4" fontWeight={600}>
                        {value}
                    </Typography>
                </Box>
                <Avatar sx={{ bgcolor: color, width: 48, height: 48 }}>{icon}</Avatar>
            </Box>
        </CardContent>
    </Card>
);

export const AgentInstructionsTab: React.FC<AgentInstructionsTabProps> = ({
    agentId,
    instructions,
    onCreateInstruction,
    onRefresh,
}) => {
    const [dialogOpen, setDialogOpen] = useState(false);

    const handleCreate = async (type: number, payload: CreateShellCommandRequest | CreateGpoSetRequest) => {
        await onCreateInstruction(type, payload);
        setDialogOpen(false);
        onRefresh();
    };

    const stats = {
        total: instructions.length,
        pending: instructions.filter((i) => i.state === InstructionState.Pending).length,
        completed: instructions.filter((i) => i.state === InstructionState.Completed).length,
        failed: instructions.filter((i) => i.state === InstructionState.Failed).length,
    };

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
                    <Box
                        sx={{ display: "flex", alignItems: "center", justifyContent: "space-between", flexWrap: "wrap", gap: 2 }}
                    >
                        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
                            <Avatar sx={{ width: 56, height: 56, bgcolor: "primary.main" }}>
                                <PlaylistAddIcon fontSize="large" />
                            </Avatar>
                            <Box>
                                <Typography variant="h5" fontWeight={600}>
                                    Instructions
                                </Typography>
                                <Typography variant="body2" color="text.secondary">
                                    Manage and monitor agent instructions
                                </Typography>
                            </Box>
                        </Box>
                        <Box sx={{ display: "flex", gap: 1 }}>
                            <Tooltip title="Refresh">
                                <IconButton onClick={onRefresh} sx={{ border: "1px solid", borderColor: "divider" }}>
                                    <RefreshIcon />
                                </IconButton>
                            </Tooltip>
                            <Button variant="contained" startIcon={<AddIcon />} onClick={() => setDialogOpen(true)}>
                                Create Instruction
                            </Button>
                        </Box>
                    </Box>
                </CardContent>
            </Card>

            {/* Stats Cards */}
            <Grid container spacing={2} sx={{ mb: 3 }}>
                <Grid size={{ xs: 6, sm: 3 }}>
                    <StatCard title="Total" value={stats.total} icon={<PlaylistAddIcon />} color="primary.main" />
                </Grid>
                <Grid size={{ xs: 6, sm: 3 }}>
                    <StatCard title="Pending" value={stats.pending} icon={<PendingIcon />} color="warning.main" />
                </Grid>
                <Grid size={{ xs: 6, sm: 3 }}>
                    <StatCard title="Completed" value={stats.completed} icon={<CheckCircleIcon />} color="success.main" />
                </Grid>
                <Grid size={{ xs: 6, sm: 3 }}>
                    <StatCard title="Failed" value={stats.failed} icon={<ErrorIcon />} color="error.main" />
                </Grid>
            </Grid>

            {/* Instructions Table */}
            <Card elevation={0} sx={{ border: "1px solid", borderColor: "divider", borderRadius: 2 }}>
                <CardContent sx={{ p: 0 }}>
                    {instructions.length === 0 ? (
                        <Box sx={{ textAlign: "center", py: 6 }}>
                            <Avatar sx={{ width: 64, height: 64, bgcolor: "action.hover", mx: "auto", mb: 2 }}>
                                <SendIcon sx={{ fontSize: 32, color: "text.secondary" }} />
                            </Avatar>
                            <Typography variant="h6" color="text.secondary" gutterBottom>
                                No Instructions Yet
                            </Typography>
                            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                                Create your first instruction to get started
                            </Typography>
                            <Button variant="outlined" startIcon={<AddIcon />} onClick={() => setDialogOpen(true)}>
                                Create Instruction
                            </Button>
                        </Box>
                    ) : (
                        <TableContainer>
                            <Table size="small" sx={{ bgcolor: "background.default" }}>
                                <TableHead>
                                    <TableRow sx={{ bgcolor: "action.selected" }}>
                                        <TableCell width={50} />
                                        <TableCell sx={{ fontWeight: 600 }}>ID</TableCell>
                                        <TableCell sx={{ fontWeight: 600 }}>Type</TableCell>
                                        <TableCell sx={{ fontWeight: 600 }}>State</TableCell>
                                        <TableCell sx={{ fontWeight: 600 }}>Payload</TableCell>
                                        <TableCell sx={{ fontWeight: 600 }}>Created At</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {instructions.map((instruction) => (
                                        <InstructionRow key={instruction.id} instruction={instruction} />
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    )}
                </CardContent>
            </Card>

            <CreateInstructionDialog
                open={dialogOpen}
                onClose={() => setDialogOpen(false)}
                onCreate={handleCreate}
                agentId={agentId}
            />
        </Box>
    );
};
