import {
    Button,
    Checkbox,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormControlLabel,
    FormGroup,
} from "@mui/material";
import { useState, useEffect } from "react";

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
    const [currentSelected, setCurrentSelected] = useState<string[]>(selected);

    useEffect(() => {
        setCurrentSelected(selected);
    }, [selected, open]);

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
        <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
            <DialogTitle>{title}</DialogTitle>
            <DialogContent>
                <FormGroup>
                    {options.map((option) => (
                        <FormControlLabel
                            key={option}
                            control={
                                <Checkbox
                                    checked={currentSelected.includes(option)}
                                    onChange={() => handleToggle(option)}
                                />
                            }
                            label={option}
                        />
                    ))}
                </FormGroup>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose}>Cancel</Button>
                <Button onClick={handleSave} variant="contained">
                    Save
                </Button>
            </DialogActions>
        </Dialog>
    );
};
