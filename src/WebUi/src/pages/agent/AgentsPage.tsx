import { useEffect, useState } from "react";
import { AgentState, type AgentResponse } from "../../types/agent";
import { fetchAgents } from "../../api/agent";
import FetchContentWrapper from "../../components/wrappers/FetchContentWrapper";
import { CustomTable } from "../../components/table/CustomTable";
import { useNavigate } from "react-router-dom";

const columns = [
  { id: 'id', label: 'ID', minWidth: 100 },
  { id: 'name', label: 'Name', minWidth: 150 },
  { id: 'state', label: 'State', minWidth: 100,
     render: (row: AgentResponse) => {
        switch (row.state) {
            case AgentState.Active: return <span style={{ color: 'green' }}>Active</span>;
            case AgentState.Inactive: return <span style={{ color: 'red' }}>Inactive</span>;
            default: return <span style={{ color: 'gray' }}>Unknown</span>;
        }
    }
  },
];

export function AgentsPage() {
    const navigate = useNavigate();
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [rows, setRows] = useState<AgentResponse[]>([]);
    const [page, setPage] = useState(0);
    const [rowsPerPage, setRowsPerPage] = useState(10);
    
    useEffect(() => {
        loadAgents();
    }, []);

    const loadAgents = async () => {
        try {
            setLoading(true);
            setError(null);
            const agents = await fetchAgents();
            setRows(agents);
            console.log("Fetched agents:", agents);
        } catch (err) {
            console.error("Failed to fetch agents", err);
            setError("Failed to load agents. Please try again.");
        } finally {
            setLoading(false);
        }
    };

    const handleChangePage = (event: unknown, newPage: number) => {
        setPage(newPage);
    };

    const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
        setRowsPerPage(parseInt(event.target.value, 10));
        setPage(0);
    };

    return (
        <FetchContentWrapper loading={loading} error={error} onRetry={loadAgents}>
            <CustomTable
                columns={columns}
                rows={rows}
                page={page}
                rowsPerPage={rowsPerPage}
                handleChangePage={handleChangePage}
                handleChangeRowsPerPage={handleChangeRowsPerPage}
                onRowClick={(row) => navigate(`/agents/${row.id}`)}
            />
        </FetchContentWrapper>
    )
}
