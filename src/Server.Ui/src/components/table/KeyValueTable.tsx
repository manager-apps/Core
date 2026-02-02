import { Table, TableBody, TableCell, TableContainer, TableRow, Paper } from "@mui/material";
import React from "react";

interface KeyValueTableProps {
  rows: { key: string; value: React.ReactNode }[];
  maxHeight?: number;
}

export const KeyValueTable: React.FC<KeyValueTableProps> =
({
 rows,
 maxHeight = 440
}) => {
  return (
    <TableContainer component={Paper} sx={{ maxHeight }}>
      <Table size="small" stickyHeader>
        <TableBody>
          {rows.map((row, index) => (
            <TableRow key={index}>
              <TableCell><strong>{row.key}</strong></TableCell>
              <TableCell>{row.value}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};
