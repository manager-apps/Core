import { Link as RouterLink } from 'react-router-dom';
import Drawer from '@mui/material/Drawer';
import Toolbar from '@mui/material/Toolbar';
import Divider from '@mui/material/Divider';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import SupportAgentIcon from '@mui/icons-material/SupportAgent';
import TryIcon from '@mui/icons-material/Try';
import LegendToggleIcon from '@mui/icons-material/LegendToggle';
import { Box, Icon, Typography } from '@mui/material';
import CircleIcon from '@mui/icons-material/Circle';
import { getApiBaseUrl, getGrafanaBaseUrl } from '../../api/axios';


interface DrawerMenuProps {
  drawerWidth: number;
}

interface MenuItem {
  text: string;
  path?: string;
  externalUrl?: string;
  icon: React.ReactNode;
  disabled: boolean;
}

const firstList: MenuItem[] = [
  {
    text: 'Metrics',
    icon: <Icon component={LegendToggleIcon} />,
    path: '/metrics',
    disabled: false
  },
  {
    text: 'Agents',
    path: '/agents',
    icon: <Icon component={SupportAgentIcon} />,
    disabled: false
  },
  {
    text: 'Chat',
    path: '/chat',
    icon: <Icon component={TryIcon} />,
    disabled: false
  },
];

export default function DrawerMenu({ drawerWidth }: DrawerMenuProps) {
  return (
    <Drawer
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: drawerWidth,
          boxSizing: 'border-box',
        },
      }}
      variant="permanent"
      anchor="left"
    >
      <Toolbar />
      <Divider />
      <List>
        {firstList.map(({ text, path, externalUrl, icon, disabled }, index) => (
          <ListItem key={index} disablePadding>
            {externalUrl ? (
              <ListItemButton
                component="a"
                href={externalUrl}
                target="_blank"
                rel="noopener noreferrer"
                disabled={disabled}
              >
                <ListItemIcon>{icon}</ListItemIcon>
                <ListItemText primary={text} />
              </ListItemButton>
            ) : (
              <ListItemButton component={RouterLink} to={path || '/'} disabled={disabled}>
                <ListItemIcon>{icon}</ListItemIcon>
                <ListItemText primary={text} />
              </ListItemButton>
            )}
          </ListItem>
        ))}
      </List>

      <Box sx={{ mt: 'auto', p: 2, borderTop: 1, borderColor: 'divider' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
          <CircleIcon sx={{ fontSize: 8, color: 'success.main' }} />
          <Typography variant="caption" color="text.secondary" noWrap>
            Grafana: {getGrafanaBaseUrl()}
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <CircleIcon sx={{ fontSize: 8, color: 'primary.main' }} />
          <Typography variant="caption" color="text.secondary" noWrap>
            API: {getApiBaseUrl()}
          </Typography>
        </Box>
      </Box>
    </Drawer>
  );
}
