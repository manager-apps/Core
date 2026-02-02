// components/loader/FetchContentWrapper.tsx
import React from "react";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import LoadingOverlay from "../loader/LoadingOverlay";

interface FetchContentWrapperProps {
    loading: boolean;
    error: string | null;
    onRetry?: () => void;
    children: React.ReactNode;
}

const FetchContentWrapper: React.FC<FetchContentWrapperProps> = ({
    loading,
    error,
    onRetry,
    children,
}) => {
    return (
        <>
            <LoadingOverlay open={loading} />

            {error && (
                <Alert
                    severity="error"
                    sx={{ mb: 2 }}
                    action={
                        onRetry && (
                            <Button color="inherit" size="small" onClick={onRetry}>
                                Retry
                            </Button>
                        )
                    }
                >
                    {error}
                </Alert>
            )}

            {!loading && !error && children}
        </>
    );
};

export default FetchContentWrapper;