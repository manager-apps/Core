import Box from '@mui/material/Box';
import CssBaseline from '@mui/material/CssBaseline';
import Toolbar from '@mui/material/Toolbar';
import { Outlet } from 'react-router-dom';

import AppBarHeader from './AppBarHeader';
import DrawerMenu from './DrawerMenu';

const drawerWidth = 240;

export default function Layout() {
  return (
    <Box sx={{ display: 'flex' }}>
      <CssBaseline />
      <AppBarHeader drawerWidth={drawerWidth} />
      <DrawerMenu drawerWidth={drawerWidth} />
      <Box
        component="main"
        sx={{ flexGrow: 1, bgcolor: 'background.default', p: 3 }}
      >
        <Toolbar />
        <Outlet />
      </Box>
    </Box>
  );
}