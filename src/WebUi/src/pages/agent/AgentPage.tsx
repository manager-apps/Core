import { useParams } from "react-router-dom";
import { fetchAgentById, fetchInstructionsForAgent } from "../../api/agent";
import { useEffect, useState } from "react";
import type { AgentResponse } from "../../types/agent";
import type { GpoSetPayload, InstructionResponse, InstructionType, ShellCommandPayload } from "../../types/instruction";
import FetchContentWrapper from "../../components/wrappers/FetchContentWrapper";
import { InstructionType as InstructionTypeConst } from '../../types/instruction';
import { Box, Button, TextField, FormControl, InputLabel, Select, MenuItem } from "@mui/material";
import { CustomTable } from "../../components/table/CustomTable";


function InstructionsBlock({ instructions, handleCreate }: { 
    instructions: InstructionResponse[],
    handleCreate?: () => void,
}) {
    const [value, setValue] = useState<string>("");
    const [selectedInstruction, setSelectedInstruction] = useState<InstructionResponse | null>(null);
    const [output, setOutput] = useState<string>("");
    const [error, setError] = useState<string>("");
    const [page, setPage] = useState(0);
    const [rowsPerPage, setRowsPerPage] = useState(10);
    const [templateType, setTemplateType] = useState<string>("");

    const getTemplateForType = (type: string): string => {
        switch (type) {
            case InstructionTypeConst.ShellCommand: {
                const payload: ShellCommandPayload = {
                    command: 'echo Hello',
                    description: 'Example command',
                    timeout: 30,
                };
                return JSON.stringify(payload, null, 2);
            }
            case InstructionTypeConst.GpoSet: {
                const payload: GpoSetPayload = {
                    path: 'HKLM\\Software\\Policies\\Microsoft',
                    name: 'PolicyName',
                    value: 'PolicyValue',
                    type: 'String',
                    description: 'Example GPO policy',
                };
                return JSON.stringify(payload, null, 2);
            }
            default:
                return '{}';
        }
    };

    const handleTemplateSelect = (type: string) => {
        setTemplateType(type);
        setValue(getTemplateForType(type));
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setValue(e.target.value);
    }

    const handleInstructionClick = (instruction: InstructionResponse) => {
        setSelectedInstruction(instruction);
        setValue(instruction.payloadJson ? JSON.stringify(JSON.parse(instruction.payloadJson), null, 2) : '');
        setOutput("");
        setError("");
    };

    const handleFormat = () => {
        try {
            const parsed = JSON.parse(value);
            setValue(JSON.stringify(parsed, null, 2));
            setError("");
        } catch (e) {
            setError(`Invalid JSON: ${e instanceof Error ? e.message : 'Unknown error'}`);
        }
    };

    const handleMinify = () => {
        try {
            const parsed = JSON.parse(value);
            setValue(JSON.stringify(parsed));
            setError("");
        } catch (e) {
            setError(`Invalid JSON: ${e instanceof Error ? e.message : 'Unknown error'}`);
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
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'stretch' }}>
            <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column', gap: 2 }}>
                <TextField
                    fullWidth
                    multiline
                    minRows={15}
                    maxRows={25}
                    value={value}
                    onChange={handleChange}
                    placeholder='Enter JSON here...'
                    variant='outlined'
                    sx={{
                        fontFamily: 'monospace',
                        fontSize: '12px',
                        flex: 1,
                        '& .MuiOutlinedInput-root': {
                            fontFamily: 'monospace',
                            height: '100%',
                        }
                    }}
                />
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', alignItems: 'center' }}>
                    <Button
                        variant='contained'
                        size='small'
                        onClick={handleFormat}
                    >
                        Format
                    </Button>
                    <Button
                        variant='contained'
                        size='small'
                        onClick={handleMinify}
                    >
                        Minify
                    </Button>
                    <Button
                        variant='contained'
                        size='small'
                    >
                        Create
                    </Button>
                    <FormControl sx={{ minWidth: 200 }}>
                        <InputLabel>Select Template</InputLabel>
                        <Select
                            value={templateType}
                            onChange={(e) => handleTemplateSelect(e.target.value)}
                            label="Select Template"
                            size="small"
                        >
                            <MenuItem value="">None</MenuItem>
                            <MenuItem value={InstructionTypeConst.ShellCommand}>Shell Command</MenuItem>
                            <MenuItem value={InstructionTypeConst.GpoSet}>GPO Set</MenuItem>
                        </Select>
                    </FormControl>
                </Box>
            </Box>

            <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
                <CustomTable
                    height="100%"
                    width="100%"
                    columns={[
                        { id: 'id', label: 'ID', minWidth: 80 },
                        { id: 'type', label: 'Type', minWidth: 100 },
                    ]}
                    rows={instructions}
                    page={page}
                    rowsPerPage={rowsPerPage}
                    handleChangePage={handleChangePage}
                    handleChangeRowsPerPage={handleChangeRowsPerPage}
                    onRowClick={handleInstructionClick}
                />
            </Box>
        </Box>
    );
}

export function AgentPage() {
    const { id } = useParams<{ id: string }>();
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const [agent, setAgent] = useState<AgentResponse | null>(null);
    const [instructions, setInstructions] = useState<InstructionResponse[]>([]);

    useEffect(() => {
        const agentId = Number(id);
        if (!id || Number.isNaN(agentId)) {
            setError("Invalid agent id");
            return;
        }

        loadAgentAndInstructions(agentId);
    }, [id]);

    const loadAgentAndInstructions = async (agentId: number) => {
        setLoading(true);
        setError(null);

        try {
            const agentData = await fetchAgentById(agentId);
            setAgent(agentData);

            const instructionsData = await fetchInstructionsForAgent(agentData.id);
            setInstructions(instructionsData);
        } catch (e) {
            console.error(e);
            setError("Failed to load agent data");
        } finally {
            setLoading(false);
        }
    };


    return (
        <FetchContentWrapper loading={loading} error={error}>
            {/* {agent && (
                <div>
                    <h2>Agent Details</h2>
                    <p><strong>ID:</strong> {agent.id}</p>
                    <p><strong>Name:</strong> {agent.name}</p>
                    <p><strong>State:</strong> {agent.state}</p>
                    <h3>Instructions</h3>
                    {instructions.length === 0 ? (
                        <p>No instructions found for this agent.</p>
                    ) : (
                        instructions.map(instr => (
                            <InstructionBlock key={instr.id} instruction={instr} />
                        ))
                    )}
                </div>
            )} */}
            <div>test</div>
            
            <InstructionsBlock 
                instructions={instructions}
                agentId={agent?.id ?? null}
            />
        </FetchContentWrapper>
    );
}

