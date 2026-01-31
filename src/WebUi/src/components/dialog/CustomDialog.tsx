import React from "react";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";

interface CustomDialogProps {
  open: boolean;
  title: string;
  onClose: () => void;
  onSubmit?: () => void;
  submitLabel?: string;
  children?: React.ReactNode;
}

const CustomDialog: React.FC<CustomDialogProps> = ({
   open,
   title,
   onClose,
   onSubmit,
   submitLabel = "Submit",
   children
}) => {
  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        {children}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        {onSubmit && (
          <Button onClick={onSubmit} variant="contained">
            {submitLabel}
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
};

export default CustomDialog;
