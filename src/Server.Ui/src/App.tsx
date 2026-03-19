import { ThemeProvider, createTheme } from '@mui/material';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { type ReactNode } from 'react';
import Layout from './components/layout/Layout';
import { AgentsPage } from './pages/agent/AgentsPage';
import { AgentPage } from './pages/agent/AgentPage';
import { MetricsPage } from './pages/metrics/MetricsPage';
import { ChatPage } from './pages/chat/ChatPage';
import { LoginPage } from './pages/auth/LoginPage';
import { AuthProvider, useAuth } from './contexts/AuthContext';

const demoTheme = createTheme({
  cssVariables: {
    colorSchemeSelector: 'data-toolpad-color-scheme',
  },
  typography: {
    // htmlFontSize: 18.4,
  },
  colorSchemes: { light: true, dark: true },
  breakpoints: {
    values: {
      xs: 0,
      sm: 600,
      md: 600,
      lg: 1200,
      xl: 1536,
    },
  },
});

function ProtectedRoute({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
}

const googleClientId =
  (window as any).__ENV__?.GOOGLE_CLIENT_ID ||
  import.meta.env.VITE_GOOGLE_CLIENT_ID ||
  '';

function App() {
  return (
    <GoogleOAuthProvider clientId={googleClientId}>
      <AuthProvider>
        <ThemeProvider theme={demoTheme}>
          <BrowserRouter>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/" element={<ProtectedRoute><Layout /></ProtectedRoute>}>
                <Route index element={<Navigate to="/agents" replace />} />
                <Route path="dashboard" element={<div>test</div>} />
                <Route path="metrics" element={<MetricsPage />} />
                <Route path="agents" element={<AgentsPage />} />
                <Route path="agents/:id" element={<AgentPage />} />
                <Route path="chat" element={<ChatPage />} />
              </Route>
            </Routes>
          </BrowserRouter>
        </ThemeProvider>
      </AuthProvider>
    </GoogleOAuthProvider>
  )
}

export default App
