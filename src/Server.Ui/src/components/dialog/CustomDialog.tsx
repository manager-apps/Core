import React from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    Box,
    IconButton,
    Typography,
    Divider,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";

interface CustomDialogProps {
    open: boolean;
    title: string;
    onClose: () => void;
    onSubmit?: () => void;
    submitLabel?: string;
    cancelLabel?: string;
    children?: React.ReactNode;
    icon?: React.ReactNode;
    maxWidth?: "xs" | "sm" | "md" | "lg" | "xl";
    loading?: boolean;
}

const CustomDialog: React.FC<CustomDialogProps> = ({
    open,
    title,
    onClose,
    onSubmit,
    submitLabel = "Submit",
    cancelLabel = "Cancel",
    children,
    icon,
    maxWidth = "sm",
    loading = false,
}) => {
    return (
        <Dialog
            open={open}
            onClose={onClose}
            fullWidth
            maxWidth={maxWidth}
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
                        {icon}
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
            <DialogContent sx={{ p: 2.5 }}>{children}</DialogContent>
            <Divider />
            <DialogActions sx={{ p: 2, gap: 1 }}>
                <Button onClick={onClose} variant="outlined" disabled={loading}>
                    {cancelLabel}
                </Button>
                {onSubmit && (
                    <Button onClick={onSubmit} variant="contained" disabled={loading}>
                        {loading ? "Loading..." : submitLabel}
                    </Button>
                )}
            </DialogActions>
        </Dialog>
    );
};

export default CustomDialog;
