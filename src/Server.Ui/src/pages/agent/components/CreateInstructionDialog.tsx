import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    Box,
    Alert,
    IconButton,
    Typography,
    Divider,
    Avatar,
    Paper,
} from "@mui/material";
import { useState } from "react";
import type { CreateShellCommandRequest, CreateGpoSetRequest } from "../../../types/instruction";
import { InstructionType } from "../../../types/instruction";
import CloseIcon from "@mui/icons-material/Close";
import AddCircleOutlineIcon from "@mui/icons-material/AddCircleOutline";
import TerminalIcon from "@mui/icons-material/Terminal";
import PolicyIcon from "@mui/icons-material/Policy";

interface CreateInstructionDialogProps {
    open: boolean;
    onClose: () => void;
    onCreate: (type: number, payload: CreateShellCommandRequest | CreateGpoSetRequest) => Promise<void>;
    agentId: number;
}

export const CreateInstructionDialog: React.FC<CreateInstructionDialogProps> = ({
    open,
    onClose,
    onCreate,
}) => {
    const [type, setType] = useState<number>(InstructionType.Shell);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Shell command fields
    const [command, setCommand] = useState("");
    const [timeout, setTimeout] = useState(30000);

    // GPO fields
    const [gpoName, setGpoName] = useState("");
    const [gpoValue, setGpoValue] = useState("");

    const handleClose = () => {
        setType(InstructionType.Shell);
        setCommand("");
        setTimeout(30000);
        setGpoName("");
        setGpoValue("");
        setError(null);
        onClose();
    };

    const handleCreate = async () => {
        setError(null);
        setLoading(true);

        try {
            if (type === InstructionType.Shell) {
                if (!command.trim()) {
                    setError("Command is required");
                    setLoading(false);
                    return;
                }
                const payload: CreateShellCommandRequest = {
                    command: command.trim(),
                    timeout,
                };
                await onCreate(type, payload);
            } else if (type === InstructionType.Gpo) {
                if (!gpoName.trim() || !gpoValue.trim()) {
                    setError("GPO name and value are required");
                    setLoading(false);
                    return;
                }
                const payload: CreateGpoSetRequest = {
                    name: gpoName.trim(),
                    value: gpoValue.trim(),
                };
                await onCreate(type, payload);
            }
            handleClose();
        } catch {
            setError("Failed to create instruction");
        } finally {
            setLoading(false);
        }
    };

    return (
        <Dialog
            open={open}
            onClose={handleClose}
            maxWidth="sm"
            fullWidth
            PaperProps={{
                elevation: 0,
                sx: {
                    border: "1px solid",
                    borderColor: "divider",
                    borderRadius: 2,
                    bgcolor: "background.paper",
                },
            }}
        >
            <DialogTitle sx={{ p: 2.5, pb: 2 }}>
                <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
                    <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
                        <Avatar sx={{ width: 40, height: 40, bgcolor: "primary.main" }}>
                            <AddCircleOutlineIcon />
                        </Avatar>
                        <Box>
                            <Typography variant="h6" fontWeight={600}>
                                Create New Instruction
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                                Send a command to the agent
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

                    <FormControl fullWidth size="small">
                        <InputLabel>Instruction Type</InputLabel>
                        <Select
                            value={type}
                            label="Instruction Type"
                            onChange={(e) => setType(Number(e.target.value))}
                        >
                            <MenuItem value={InstructionType.Shell}>
                                <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                    <TerminalIcon fontSize="small" color="primary" />
                                    Shell Command
                                </Box>
                            </MenuItem>
                            <MenuItem value={InstructionType.Gpo}>
                                <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                    <PolicyIcon fontSize="small" color="secondary" />
                                    GPO Setting
                                </Box>
                            </MenuItem>
                        </Select>
                    </FormControl>

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
                        {type === InstructionType.Shell && (
                            <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 1 }}>
                                        Command
                                    </Typography>
                                    <TextField
                                        fullWidth
                                        multiline
                                        rows={3}
                                        size="small"
                                        value={command}
                                        onChange={(e) => setCommand(e.target.value)}
                                        placeholder="Enter shell command..."
                                        sx={{
                                            "& .MuiInputBase-input": {
                                                fontFamily: "monospace",
                                                fontSize: "0.875rem",
                                            },
                                        }}
                                    />
                                    <Typography variant="caption" color="text.secondary">
                                        The command to execute on the agent
                                    </Typography>
                                </Box>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 1 }}>
                                        Timeout (ms)
                                    </Typography>
                                    <TextField
                                        type="number"
                                        fullWidth
                                        size="small"
                                        value={timeout}
                                        onChange={(e) => setTimeout(Number(e.target.value))}
                                    />
                                    <Typography variant="caption" color="text.secondary">
                                        Maximum execution time in milliseconds
                                    </Typography>
                                </Box>
                            </Box>
                        )}

                        {type === InstructionType.Gpo && (
                            <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 1 }}>
                                        GPO Name
                                    </Typography>
                                    <TextField
                                        fullWidth
                                        size="small"
                                        value={gpoName}
                                        onChange={(e) => setGpoName(e.target.value)}
                                        placeholder="Enter GPO setting name..."
                                    />
                                </Box>
                                <Box>
                                    <Typography variant="body2" fontWeight={500} sx={{ mb: 1 }}>
                                        GPO Value
                                    </Typography>
                                    <TextField
                                        fullWidth
                                        size="small"
                                        value={gpoValue}
                                        onChange={(e) => setGpoValue(e.target.value)}
                                        placeholder="Enter GPO setting value..."
                                    />
                                </Box>
                            </Box>
                        )}
                    </Paper>
                </Box>
            </DialogContent>
            <Divider />
            <DialogActions sx={{ p: 2, gap: 1 }}>
                <Button onClick={handleClose} variant="outlined" disabled={loading}>
                    Cancel
                </Button>
                <Button onClick={handleCreate} variant="contained" disabled={loading}>
                    {loading ? "Creating..." : "Create"}
                </Button>
            </DialogActions>
        </Dialog>
    );
};
