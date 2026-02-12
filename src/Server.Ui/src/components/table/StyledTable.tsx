import React, { useState } from "react";
import {
    Box,
    Card,
    CardContent,
    Collapse,
    IconButton,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TablePagination,
    TableRow,
    Typography,
    Avatar,
} from "@mui/material";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import ExpandLessIcon from "@mui/icons-material/ExpandLess";
import TableRowsIcon from "@mui/icons-material/TableRows";

export interface StyledTableColumn<T = unknown> {
    id: string;
    label: string;
    minWidth?: number;
    align?: "right" | "left" | "center";
    render?: (row: T) => React.ReactNode;
}

interface StyledTableProps<T> {
    columns: StyledTableColumn<T>[];
    rows: T[];
    getRowId: (row: T) => string | number;
    onRowClick?: (row: T) => void;
    renderExpandedContent?: (row: T) => React.ReactNode;
    pagination?: boolean;
    emptyMessage?: string;
    emptyIcon?: React.ReactNode;
}

interface ExpandableRowProps<T> {
    row: T;
    columns: StyledTableColumn<T>[];
    onRowClick?: (row: T) => void;
    renderExpandedContent?: (row: T) => React.ReactNode;
    isOdd: boolean;
}

function ExpandableRow<T>({ row, columns, onRowClick, renderExpandedContent, isOdd }: ExpandableRowProps<T>) {
    const [expanded, setExpanded] = useState(false);
    const isExpandable = !!renderExpandedContent;

    const handleClick = () => {
        if (isExpandable) {
            setExpanded(!expanded);
        } else if (onRowClick) {
            onRowClick(row);
        }
    };

    return (
        <>
            <TableRow
                hover
                sx={{
                    cursor: onRowClick || isExpandable ? "pointer" : "default",
                    bgcolor: isOdd ? "action.hover" : "background.paper",
                }}
                onClick={handleClick}
            >
                {isExpandable && (
                    <TableCell sx={{ width: 50, py: 1 }}>
                        <IconButton size="small">
                            {expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                        </IconButton>
                    </TableCell>
                )}
                {columns.map((column) => (
                    <TableCell key={column.id} align={column.align} sx={{ py: 1.5 }}>
                        {column.render ? column.render(row) : (row as Record<string, unknown>)[column.id] as React.ReactNode}
                    </TableCell>
                ))}
            </TableRow>
            {isExpandable && (
                <TableRow>
                    <TableCell
                        sx={{ py: 0, borderBottom: expanded ? 1 : 0, borderColor: "divider" }}
                        colSpan={columns.length + 1}
                    >
                        <Collapse in={expanded} timeout="auto" unmountOnExit>
                            <Box sx={{ p: 2.5, bgcolor: "background.default", borderRadius: 1, my: 1 }}>
                                {renderExpandedContent(row)}
                            </Box>
                        </Collapse>
                    </TableCell>
                </TableRow>
            )}
        </>
    );
}

export function StyledTable<T>({
    columns,
    rows,
    getRowId,
    onRowClick,
    renderExpandedContent,
    pagination = true,
    emptyMessage = "No data available",
    emptyIcon,
}: StyledTableProps<T>) {
    const [page, setPage] = useState(0);
    const [rowsPerPage, setRowsPerPage] = useState(10);

    const handleChangePage = (_: unknown, newPage: number) => {
        setPage(newPage);
    };

    const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
        setRowsPerPage(parseInt(event.target.value, 10));
        setPage(0);
    };

    const displayedRows = pagination ? rows.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage) : rows;
    const isExpandable = !!renderExpandedContent;

    if (rows.length === 0) {
        return (
            <Card elevation={0} sx={{ border: "1px solid", borderColor: "divider", borderRadius: 2 }}>
                <CardContent>
                    <Box sx={{ textAlign: "center", py: 6 }}>
                        <Avatar sx={{ width: 64, height: 64, bgcolor: "action.hover", mx: "auto", mb: 2 }}>
                            {emptyIcon || <TableRowsIcon sx={{ fontSize: 32, color: "text.secondary" }} />}
                        </Avatar>
                        <Typography variant="h6" color="text.secondary" gutterBottom>
                            {emptyMessage}
                        </Typography>
                    </Box>
                </CardContent>
            </Card>
        );
    }

    return (
        <Card elevation={0} sx={{ border: "1px solid", borderColor: "divider", borderRadius: 2 }}>
            <TableContainer>
                <Table size="small" sx={{ bgcolor: "background.default" }}>
                    <TableHead>
                        <TableRow sx={{ bgcolor: "action.selected" }}>
                            {isExpandable && <TableCell sx={{ width: 50 }} />}
                            {columns.map((column) => (
                                <TableCell
                                    key={column.id}
                                    align={column.align}
                                    sx={{ fontWeight: 600, minWidth: column.minWidth }}
                                >
                                    {column.label}
                                </TableCell>
                            ))}
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {displayedRows.map((row, index) => (
                            <ExpandableRow
                                key={getRowId(row)}
                                row={row}
                                columns={columns}
                                onRowClick={onRowClick}
                                renderExpandedContent={renderExpandedContent}
                                isOdd={index % 2 === 1}
                            />
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
            {pagination && (
                <TablePagination
                    rowsPerPageOptions={[10, 25, 50]}
                    component="div"
                    count={rows.length}
                    rowsPerPage={rowsPerPage}
                    page={page}
                    onPageChange={handleChangePage}
                    onRowsPerPageChange={handleChangeRowsPerPage}
                    sx={{ borderTop: "1px solid", borderColor: "divider" }}
                />
            )}
        </Card>
    );
}
