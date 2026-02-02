import { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Button,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
} from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import ClearIcon from '@mui/icons-material/Clear';
import { JsonEditor } from './JsonEditor';
import type {
  InstructionType,
  CreateInstructionRequest,
  ShellCommandPayload,
  GpoSetPayload,
  ConfigUpdatePayload,
} from '../types/instruction';
import { InstructionType as InstructionTypeConst } from '../types/instruction';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role='tabpanel'
      hidden={value !== index}
      id={`payload-tabpanel-${index}`}
      aria-labelledby={`payload-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
    </div>
  );
}

interface InstructionFormProps {
  onSuccess?: (response: any) => void;
  onError?: (error: string) => void;
  apiBaseUrl?: string;
}

export const InstructionForm = ({
  onSuccess,
  onError,
  apiBaseUrl = 'http://localhost:5140',
}: InstructionFormProps) => {
  const [agentId, setAgentId] = useState<string>('');
  const [instructionType, setInstructionType] = useState<InstructionType | ''>('');
  const [payloadJson, setPayloadJson] = useState<string>('{}');
  const [tabValue, setTabValue] = useState(0);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const getTemplateForType = (type: InstructionType): string => {
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
      case InstructionTypeConst.ConfigUpdate: {
        const payload: ConfigUpdatePayload = {
          configKey: 'app.setting',
          configValue: 'value',
          description: 'Example config update',
        };
        return JSON.stringify(payload, null, 2);
      }
      default:
        return '{}';
    }
  };

  const handleTypeChange = (type: InstructionType) => {
    setInstructionType(type);
    setPayloadJson(getTemplateForType(type));
  };

  const validateForm = (): boolean => {
    if (!agentId.trim()) {
      setMessage({ type: 'error', text: 'Agent ID is required' });
      return false;
    }

    if (!instructionType) {
      setMessage({ type: 'error', text: 'Instruction type is required' });
      return false;
    }

    try {
      JSON.parse(payloadJson);
    } catch {
      setMessage({ type: 'error', text: 'Invalid JSON in payload' });
      return false;
    }

    return true;
  };

  const handleSubmit = async () => {
    if (!validateForm()) {
      return;
    }

    setLoading(true);
    setMessage(null);

    try {
      const request: CreateInstructionRequest = {
        agentId: parseInt(agentId, 10),
        type: instructionType as InstructionType,
        payloadJson,
      };

      const response = await fetch(`${apiBaseUrl}/api/v1/instructions`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || `Server error: ${response.status}`);
      }

      const data = await response.json();
      setMessage({ type: 'success', text: `Instruction created successfully (ID: ${data.id})` });
      onSuccess?.(data);
      handleReset();
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error occurred';
      setMessage({ type: 'error', text: errorMessage });
      onError?.(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleReset = () => {
    setAgentId('');
    setInstructionType('');
    setPayloadJson('{}');
    setTabValue(0);
  };

  return (
    <Card sx={{ width: '100%', maxWidth: 900 }}>
      <CardHeader
        title='Create Instruction'
        subheader='Create a new instruction with JSON payload'
      />
      <CardContent>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          {message && (
            <Alert severity={message.type} onClose={() => setMessage(null)}>
              {message.text}
            </Alert>
          )}

          <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
            <TextField
              label='Agent ID'
              type='number'
              value={agentId}
              onChange={(e) => setAgentId(e.target.value)}
              placeholder='Enter agent ID'
              fullWidth
              inputProps={{ min: 1 }}
            />

            <FormControl fullWidth>
              <InputLabel>Instruction Type</InputLabel>
              <Select
                value={instructionType}
                onChange={(e) => handleTypeChange(e.target.value as InstructionType)}
                label='Instruction Type'
              >
                <MenuItem value={InstructionTypeConst.ShellCommand}>Shell Command</MenuItem>
                <MenuItem value={InstructionTypeConst.GpoSet}>GPO Set</MenuItem>
                <MenuItem value={InstructionTypeConst.ConfigUpdate}>Config Update</MenuItem>
              </Select>
            </FormControl>
          </Box>

          <Box>
            <InputLabel sx={{ mb: 1, fontWeight: 600 }}>Payload JSON</InputLabel>
            <Tabs
              value={tabValue}
              onChange={(_, newValue) => setTabValue(newValue)}
              aria-label='payload tabs'
              sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}
            >
              <Tab label='Editor' id='payload-tab-0' aria-controls='payload-tabpanel-0' />
              <Tab label='Templates' id='payload-tab-1' aria-controls='payload-tabpanel-1' />
            </Tabs>

            <TabPanel value={tabValue} index={0}>
              <JsonEditor
                value={payloadJson}
                onChange={setPayloadJson}
              />
            </TabPanel>

            <TabPanel value={tabValue} index={1}>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                <Button
                  variant='outlined'
                  fullWidth
                  onClick={() => handleTypeChange(InstructionTypeConst.ShellCommand)}
                >
                  Shell Command Template
                </Button>
                <Button
                  variant='outlined'
                  fullWidth
                  onClick={() => handleTypeChange(InstructionTypeConst.GpoSet)}
                >
                  GPO Set Template
                </Button>
                <Button
                  variant='outlined'
                  fullWidth
                  onClick={() => handleTypeChange(InstructionTypeConst.ConfigUpdate)}
                >
                  Config Update Template
                </Button>
              </Box>
            </TabPanel>
          </Box>

          <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
            <Button
              variant='outlined'
              startIcon={<ClearIcon />}
              onClick={handleReset}
              disabled={loading}
            >
              Reset
            </Button>
            <Button
              variant='contained'
              startIcon={loading ? <CircularProgress size={20} /> : <SaveIcon />}
              onClick={handleSubmit}
              disabled={loading}
            >
              {loading ? 'Creating...' : 'Create Instruction'}
            </Button>
          </Box>
        </Box>
      </CardContent>
    </Card>
  );
};
