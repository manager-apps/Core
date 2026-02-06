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
} from "@mui/material";
import { useState } from "react";
import type { CreateShellCommandRequest, CreateGpoSetRequest } from "../../../types/instruction";
import { InstructionType } from "../../../types/instruction";

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
        <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
            <DialogTitle>Create New Instruction</DialogTitle>
            <DialogContent>
                <Box sx={{ display: "flex", flexDirection: "column", gap: 2, mt: 1 }}>
                    {error && <Alert severity="error">{error}</Alert>}

                    <FormControl fullWidth>
                        <InputLabel>Instruction Type</InputLabel>
                        <Select
                            value={type}
                            label="Instruction Type"
                            onChange={(e) => setType(Number(e.target.value))}
                        >
                            <MenuItem value={InstructionType.Shell}>Shell Command</MenuItem>
                            <MenuItem value={InstructionType.Gpo}>GPO Setting</MenuItem>
                        </Select>
                    </FormControl>

                    {type === InstructionType.Shell && (
                        <>
                            <TextField
                                label="Command"
                                fullWidth
                                multiline
                                rows={3}
                                value={command}
                                onChange={(e) => setCommand(e.target.value)}
                                placeholder="Enter shell command..."
                                helperText="The command to execute on the agent"
                            />
                            <TextField
                                label="Timeout (ms)"
                                type="number"
                                fullWidth
                                value={timeout}
                                onChange={(e) => setTimeout(Number(e.target.value))}
                                helperText="Maximum execution time in milliseconds"
                            />
                        </>
                    )}

                    {type === InstructionType.Gpo && (
                        <>
                            <TextField
                                label="GPO Name"
                                fullWidth
                                value={gpoName}
                                onChange={(e) => setGpoName(e.target.value)}
                                placeholder="Enter GPO setting name..."
                            />
                            <TextField
                                label="GPO Value"
                                fullWidth
                                value={gpoValue}
                                onChange={(e) => setGpoValue(e.target.value)}
                                placeholder="Enter GPO setting value..."
                            />
                        </>
                    )}
                </Box>
            </DialogContent>
            <DialogActions>
                <Button onClick={handleClose} disabled={loading}>
                    Cancel
                </Button>
                <Button
                    onClick={handleCreate}
                    variant="contained"
                    disabled={loading}
                >
                    {loading ? "Creating..." : "Create"}
                </Button>
            </DialogActions>
        </Dialog>
    );
};
