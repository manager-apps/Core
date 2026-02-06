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
import { Icon, SvgIcon } from '@mui/material';
import TryIcon from '@mui/icons-material/Try';

const GrafanaIcon = () => (
  <SvgIcon viewBox="0 0 24 24">
    <path d="M22.687 12.154c-.043-.47-.086-.853-.173-1.322a10.397 10.397 0 0 0-.41-1.538 10.606 10.606 0 0 0-.86-1.837 10.397 10.397 0 0 0-1.15-1.58 9.816 9.816 0 0 0-2.67-2.154 10.826 10.826 0 0 0-1.708-.812 10.65 10.65 0 0 0-1.837-.512c-.43-.086-.86-.129-1.322-.172-.215-.043-.43-.043-.688-.043A10.604 10.604 0 0 0 6.48 4.37a9.816 9.816 0 0 0-2.154 2.67 10.826 10.826 0 0 0-.812 1.708 10.65 10.65 0 0 0-.512 1.837c-.086.43-.129.86-.172 1.322 0 .473-.043.86 0 1.376v.129c.043.516.086.946.172 1.419.129.602.301 1.161.516 1.72.215.516.473 1.032.774 1.505.602.946 1.333 1.794 2.197 2.483.43.344.903.645 1.376.903.989.516 2.068.86 3.19 1.018.387.043.774.086 1.161.086.43 0 .86-.043 1.29-.086a10.482 10.482 0 0 0 3.189-1.018c.947-.516 1.794-1.204 2.526-2.025.731-.82 1.29-1.794 1.677-2.827.129-.344.215-.688.301-1.032.086-.387.172-.774.215-1.161.043-.387.043-.774.043-1.161v-.387c0-.215-.043-.43-.043-.688z" />
  </SvgIcon>
);

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
    externalUrl: 'http://localhost:3000',
    icon: <GrafanaIcon />,
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
    disabled: true
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
    </Drawer>
  );
}
