import {
    Button,
    Checkbox,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormControlLabel,
    Box,
    IconButton,
    Typography,
    Divider,
    Paper,
    Avatar,
} from "@mui/material";
import React, { useState, useRef } from "react";
import CloseIcon from "@mui/icons-material/Close";
import ChecklistIcon from "@mui/icons-material/Checklist";

interface MultiSelectDialogProps {
    open: boolean;
    title: string;
    options: string[];
    selected: string[];
    onClose: () => void;
    onSave: (selected: string[]) => void;
}

export const MultiSelectDialog: React.FC<MultiSelectDialogProps> = ({
    open,
    title,
    options,
    selected,
    onClose,
    onSave,
}) => {
    const [currentSelected, setCurrentSelected] = useState<string[]>([]);
    const prevOpenRef = useRef(open);

    // Sync state when dialog opens
    if (open && !prevOpenRef.current) {
        setCurrentSelected(selected);
    }
    prevOpenRef.current = open;

    const handleToggle = (option: string) => {
        setCurrentSelected((prev) =>
            prev.includes(option) ? prev.filter((item) => item !== option) : [...prev, option]
        );
    };

    const handleSave = () => {
        onSave(currentSelected);
        onClose();
    };

    return (
        <Dialog
            open={open}
            onClose={onClose}
            maxWidth="xs"
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
                        <Avatar sx={{ width: 36, height: 36, bgcolor: "primary.main" }}>
                            <ChecklistIcon fontSize="small" />
                        </Avatar>
                        <Typography variant="h6" fontWeight={600}>
                            {title}
                        </Typography>
                    </Box>
                    <IconButton size="small" onClick={onClose} sx={{ color: "text.secondary" }}>
                        <CloseIcon fontSize="small" />
                    </IconButton>
                </Box>
            </DialogTitle>
            <Divider />
            <DialogContent sx={{ p: 2.5 }}>
                <Paper
                    variant="outlined"
                    sx={{
                        p: 1,
                        borderRadius: 1,
                        border: "1px solid",
                        borderColor: "divider",
                    }}
                >
                    {options.map((option) => (
                        <FormControlLabel
                            key={option}
                            sx={{
                                display: "flex",
                                m: 0,
                                px: 1,
                                py: 0.5,
                                borderRadius: 1,
                                "&:hover": { bgcolor: "action.hover" },
                            }}
                            control={
                                <Checkbox
                                    checked={currentSelected.includes(option)}
                                    onChange={() => handleToggle(option)}
                                    size="small"
                                />
                            }
                            label={<Typography variant="body2">{option}</Typography>}
                        />
                    ))}
                </Paper>
                <Typography variant="caption" color="text.secondary" sx={{ display: "block", mt: 1.5 }}>
                    {currentSelected.length} of {options.length} selected
                </Typography>
            </DialogContent>
            <Divider />
            <DialogActions sx={{ p: 2, gap: 1 }}>
                <Button onClick={onClose} variant="outlined">
                    Cancel
                </Button>
                <Button onClick={handleSave} variant="contained">
                    Save
                </Button>
            </DialogActions>
        </Dialog>
    );
};
