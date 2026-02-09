import { useParams } from "react-router-dom";
import { createShellInstructionForAgent, createGpoInstructionForAgent, fetchAgentById, fetchInstructionsForAgent, updateAgentConfig} from "../../api/agent";
import { InstructionType } from "../../types/instruction";
import { useEffect, useState } from "react";

import type { AgentDetailResponse } from "../../types/agent";
import type { InstructionResponse, CreateShellCommandRequest, CreateGpoSetRequest } from "../../types/instruction";
import type { ConfigUpdateRequest } from "../../types/config";

import FetchContentWrapper from "../../components/wrappers/FetchContentWrapper";
import { Box, Chip, Tab, Tabs } from "@mui/material";
import { AgentOverviewTab } from "./components/AgentOverviewTab";
import { AgentConfigTab } from "./components/AgentConfigTab";
import { AgentInstructionsTab } from "./components/AgentInstructionsTab";

interface TabPanelProps {
    children?: React.ReactNode;
    index: number;
    value: number;
}

function TabPanel(props: TabPanelProps) {
    const { children, value, index, ...other } = props;
    return (
        <div
            role="tabpanel"
            hidden={value !== index}
            id={`agent-tabpanel-${index}`}
            aria-labelledby={`agent-tab-${index}`}
            {...other}
        >
            {value === index && <Box>{children}</Box>}
        </div>
    );
}

export function AgentPage() {
    const { id } = useParams<{ id: string }>();
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);
    const [tabValue, setTabValue] = useState(0);

    const [agent, setAgent] = useState<AgentDetailResponse | null>(null);
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
            console.log("Fetched agent details:", agentData);

            const instructionsData = await fetchInstructionsForAgent(agentData.id);
            setInstructions(instructionsData);
            console.log("Fetched instructions for agent:", instructionsData);
        } catch (e) {
            console.error(e);
            setError("Failed to load agent data");
        } finally {
            setLoading(false);
        }
    };

    const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
        setTabValue(newValue);
    };

    const handleConfigSave = async (config: ConfigUpdateRequest) => {
        if (!agent) return;
        await updateAgentConfig(agent.id, config);
        await loadAgentAndInstructions(agent.id);
    };

    const handleCreateInstruction = async (type: number, payload: CreateShellCommandRequest | CreateGpoSetRequest) => {
        if (!agent) return;
        if (type === InstructionType.Shell) {
            await createShellInstructionForAgent(agent.id, payload as CreateShellCommandRequest);
        } else if (type === InstructionType.Gpo) {
            await createGpoInstructionForAgent(agent.id, payload as CreateGpoSetRequest);
        }
    };

    const handleRefreshInstructions = () => {
        if (agent) {
            loadAgentAndInstructions(agent.id);
        }
    };

    return (
        <FetchContentWrapper loading={loading} error={error}>
            {agent && (
                <Box>
                    <Box sx={{ borderBottom: 1, borderColor: "divider" }}>
                        <Tabs value={tabValue} onChange={handleTabChange}>
                            <Tab label="Overview" id="agent-tab-0" aria-controls="agent-tabpanel-0" />
                            <Tab label="Configuration" id="agent-tab-1" aria-controls="agent-tabpanel-1" />
                            <Tab
                                label={
                                    <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                                        Instructions
                                        <Chip label={instructions.length} size="small" />
                                    </Box>
                                }
                                id="agent-tab-2"
                                aria-controls="agent-tabpanel-2"
                            />
                        </Tabs>
                    </Box>
                    <TabPanel value={tabValue} index={0}>
                      {agent.hardware && agent.config ? (
                        <AgentOverviewTab
                            agentId={agent.id}
                            agentVersion={agent.version}
                            agentName={agent.name}
                            agentCreatedAt={agent.createdAt}
                            agentCurrentTag={agent.currentTag}
                            agentSourceTag={agent.sourceTag}
                            agentLastUpdatedAt={agent.updatedAt}
                            hardware={agent.hardware} />
                      ) : (
                        <Box sx={{ p: 2 }}>No details available for this agent.</Box>
                      )}
                    </TabPanel>
                    <TabPanel value={tabValue} index={1}>
                      { agent.config ? (
                        <AgentConfigTab
                            agentId={agent.id}
                            agentName={agent.name}
                            config={agent.config}
                            onSave={handleConfigSave} />
                      ) : (
                        <Box sx={{ p: 2 }}>No configuration available for this agent.</Box>
                      )}
                    </TabPanel>
                    <TabPanel value={tabValue} index={2}>
                        <AgentInstructionsTab
                            agentId={agent.id}
                            instructions={instructions}
                            onCreateInstruction={handleCreateInstruction}
                            onRefresh={handleRefreshInstructions}
                        />
                    </TabPanel>
                </Box>
            )}
        </FetchContentWrapper>
    );
}

