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
            return <Chip label="Pending" color="warning" size="small" />;
        case InstructionState.Dispatched:
            return <Chip label="Dispatched" color="info" size="small" />;
        case InstructionState.Completed:
            return <Chip label="Completed" color="success" size="small" />;
        case InstructionState.Failed:
            return <Chip label="Failed" color="error" size="small" />;
        default:
            return <Chip label="Unknown" color="default" size="small" />;
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
                sx={{ cursor: "pointer" }}
                onClick={() => setExpanded(!expanded)}
            >
                <TableCell>
                    <IconButton size="small">
                        {expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                    </IconButton>
                </TableCell>
                <TableCell>{instruction.id}</TableCell>
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
                            }}
                        >
                            {parsePayload(instruction.payloadJson, instruction.type)}
                        </Typography>
                    </Tooltip>
                </TableCell>
                <TableCell>{formatDate(instruction.createdAt)}</TableCell>
            </TableRow>
            <TableRow>
                <TableCell sx={{ py: 0 }} colSpan={6}>
                    <Collapse in={expanded} timeout="auto" unmountOnExit>
                        <Box sx={{ p: 2, bgcolor: "background.default" }}>
                            <Typography variant="subtitle2" gutterBottom>
                                Payload:
                            </Typography>
                            <Paper variant="outlined" sx={{ p: 1, mb: 2, bgcolor: "background.paper" }}>
                                <Typography
                                    variant="body2"
                                    component="pre"
                                    sx={{ fontFamily: "monospace", whiteSpace: "pre-wrap", m: 0, color: "text.primary" }}
                                >
                                    {JSON.stringify(JSON.parse(instruction.payloadJson), null, 2)}
                                </Typography>
                            </Paper>

                            {instruction.output && (
                                <>
                                    <Typography variant="subtitle2" gutterBottom>
                                        Output:
                                    </Typography>
                                    <Paper variant="outlined" sx={{ p: 1, mb: 2, bgcolor: "background.paper" }}>
                                        <Typography
                                            variant="body2"
                                            component="pre"
                                            sx={{ fontFamily: "monospace", whiteSpace: "pre-wrap", m: 0, color: "success.main" }}
                                        >
                                            {instruction.output}
                                        </Typography>
                                    </Paper>
                                </>
                            )}

                            {instruction.error && (
                                <>
                                    <Typography variant="subtitle2" gutterBottom color="error">
                                        Error:
                                    </Typography>
                                    <Paper variant="outlined" sx={{ p: 1, bgcolor: "background.paper" }}>
                                        <Typography
                                            variant="body2"
                                            component="pre"
                                            sx={{ fontFamily: "monospace", whiteSpace: "pre-wrap", m: 0, color: "error.main" }}
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

    return (
        <Box sx={{ p: 2 }}>
            <Card elevation={2}>
                <CardContent>
                    <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between", mb: 2 }}>
                        <Box sx={{ display: "flex", alignItems: "center" }}>
                            <PlaylistAddIcon sx={{ mr: 1, color: "primary.main" }} />
                            <Typography variant="h6">Instructions</Typography>
                            <Chip
                                label={instructions.length}
                                size="small"
                                sx={{ ml: 1 }}
                            />
                        </Box>
                        <Box sx={{ display: "flex", gap: 1 }}>
                            <Tooltip title="Refresh">
                                <IconButton onClick={onRefresh}>
                                    <RefreshIcon />
                                </IconButton>
                            </Tooltip>
                            <Button
                                variant="contained"
                                startIcon={<AddIcon />}
                                onClick={() => setDialogOpen(true)}
                            >
                                Create Instruction
                            </Button>
                        </Box>
                    </Box>

                    {instructions.length === 0 ? (
                        <Box sx={{ textAlign: "center", py: 4 }}>
                            <Typography variant="body1" color="text.secondary">
                                No instructions yet. Create your first instruction!
                            </Typography>
                        </Box>
                    ) : (
                        <TableContainer component={Paper} variant="outlined">
                            <Table size="small">
                                <TableHead>
                                    <TableRow>
                                        <TableCell width={50} />
                                        <TableCell>ID</TableCell>
                                        <TableCell>Type</TableCell>
                                        <TableCell>State</TableCell>
                                        <TableCell>Payload</TableCell>
                                        <TableCell>Created At</TableCell>
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
