import { Box, Card, CardContent, Typography } from '@mui/material';
import { GoogleLogin } from '@react-oauth/google';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import api from '../../api/axios';

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSuccess = async (credentialResponse: { credential?: string }) => {
    if (!credentialResponse.credential) return;
    const { data } = await api.post('/auth/login/google', {
      credential: credentialResponse.credential
    });
    login(data.token, { name: data.name, email: data.email, avatarUrl: data.avatarUrl });
    navigate('/agents', { replace: true });
  };

  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
      <Card elevation={3} sx={{ minWidth: 340, p: 2 }}>
        <CardContent sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 3 }}>
          <Typography variant="h5" fontWeight={600}>Sign in to Manager</Typography>
          <GoogleLogin
            onSuccess={handleSuccess}
            onError={() => console.error('Google login failed')}
          />
        </CardContent>
      </Card>
    </Box>
  );
}
