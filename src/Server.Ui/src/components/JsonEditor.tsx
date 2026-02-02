import { useState } from 'react';
import {
  Box,
  Button,
  TextField,
  Alert,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Typography,
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ErrorIcon from '@mui/icons-material/Error';

interface JsonEditorProps {
  value: string;
  onChange: (value: string) => void;
  onFormat?: () => void;
  onMinify?: () => void;
}

export const JsonEditor = ({ value, onChange, onFormat, onMinify }: JsonEditorProps) => {
  const [error, setError] = useState<string | null>(null);

  const validateJson = (json: string): boolean => {
    try {
      JSON.parse(json);
      setError(null);
      return true;
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Invalid JSON');
      return false;
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    onChange(newValue);
    validateJson(newValue);
  };

  const handleFormat = () => {
    try {
      const parsed = JSON.parse(value);
      const formatted = JSON.stringify(parsed, null, 2);
      onChange(formatted);
      setError(null);
      onFormat?.();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Invalid JSON');
    }
  };

  const handleMinify = () => {
    try {
      const parsed = JSON.parse(value);
      const minified = JSON.stringify(parsed);
      onChange(minified);
      setError(null);
      onMinify?.();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Invalid JSON');
    }
  };

  return (
    <Box sx={{ width: '100%', display: 'flex', flexDirection: 'column', gap: 2 }}>
      <TextField
        fullWidth
        multiline
        minRows={10}
        maxRows={20}
        value={value}
        onChange={handleChange}
        placeholder='Enter JSON here...'
        variant='outlined'
        sx={{
          fontFamily: 'monospace',
          fontSize: '12px',
          '& .MuiOutlinedInput-root': {
            fontFamily: 'monospace',
          },
        }}
      />

      <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
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
      </Box>

      {error && (
        <Alert severity='error' icon={<ErrorIcon />}>
          {error}
        </Alert>
      )}

      {!error && value && (
        <Accordion defaultExpanded>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Typography variant='subtitle2'>JSON Preview</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <Box
              component='pre'
              sx={{
                width: '100%',
                padding: 1,
                backgroundColor: '#f5f5f5',
                borderRadius: 1,
                overflow: 'auto',
                maxHeight: '200px',
                fontFamily: 'monospace',
                fontSize: '12px',
              }}
            >
              {JSON.stringify(JSON.parse(value), null, 2)}
            </Box>
          </AccordionDetails>
        </Accordion>
      )}
    </Box>
  );
};
