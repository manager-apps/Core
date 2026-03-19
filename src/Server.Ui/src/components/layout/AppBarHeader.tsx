import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Avatar from '@mui/material/Avatar';
import IconButton from '@mui/material/IconButton';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import Typography from '@mui/material/Typography';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';

interface AppBarHeaderProps {
  drawerWidth: number;
}

export default function AppBarHeader(props: AppBarHeaderProps) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const handleLogout = () => {
    setAnchorEl(null);
    logout();
    navigate('/login');
  };

  return (
    <AppBar
      position="fixed"
      sx={{
        width: `calc(100% - ${props.drawerWidth}px)`,
        ml: `${props.drawerWidth}px`,
      }}
    >
      <Toolbar sx={{ display: 'flex', justifyContent: 'space-between' }}>
        <span />
        <IconButton onClick={(e) => setAnchorEl(e.currentTarget)} sx={{ p: 0 }}>
          <Avatar src={user?.avatarUrl ?? undefined} sx={{ width: 36, height: 36 }}>
            {user?.name?.charAt(0).toUpperCase()}
          </Avatar>
        </IconButton>
        <Menu anchorEl={anchorEl} open={!!anchorEl} onClose={() => setAnchorEl(null)}>
          <MenuItem disabled>
            <Typography variant="body2">{user?.email}</Typography>
          </MenuItem>
          <MenuItem onClick={handleLogout}>Sign out</MenuItem>
        </Menu>
      </Toolbar>
    </AppBar>
  );
}
