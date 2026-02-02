import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Box from '@mui/material/Box';
import WifiIcon from '@mui/icons-material/Wifi';

interface AppBarHeaderProps {
  drawerWidth: number;
}

export default function AppBarHeader(props: AppBarHeaderProps) {
  return (
    <AppBar
      position="fixed"
      sx={{
        width: `calc(100% - ${props.drawerWidth}px)`,
        ml: `${props.drawerWidth}px`,
      }}
    >
      <Toolbar sx={{ display: 'flex', justifyContent: 'space-between' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <WifiIcon fontSize="small" />
        </Box>
      </Toolbar>
    </AppBar>
  );
}
