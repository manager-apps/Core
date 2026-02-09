import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';

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
      <Toolbar sx={{ display: 'flex', justifyContent: 'space-between' }}/>
    </AppBar>
  );
}
