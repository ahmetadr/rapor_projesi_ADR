import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import { TextField, Button, Container, Typography, Box } from '@mui/material';
import { loginSuccess } from '../../store/authSlice';
import api from '../../api/api';

export default function Register() {
    const [formData, setFormData] = useState({
        username: '',
        password: '',
        email: '',
        fullName: ''
    });
    const [error, setError] = useState('');
    const dispatch = useDispatch();
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const response = await api.post('/Auth/register', formData);
            if (response.data.success) {
                dispatch(loginSuccess(response.data));
                navigate('/');
            } else {
                setError('Kayıt işlemi başarısız oldu.');
            }
        } catch (error) {
            setError(error.response?.data?.message || 'Kayıt sırasında bir hata oluştu');
            console.error('Kayıt hatası:', error);
        }
    };

    return (
        <Container maxWidth="sm">
            <Box component="form" onSubmit={handleSubmit} sx={{ mt: 4 }}>
                <Typography variant="h4" gutterBottom>Kayıt Ol</Typography>
                {error && (
                    <Typography color="error" align="center" sx={{ mb: 2 }}>
                        {error}
                    </Typography>
                )}
                <TextField
                    label="Kullanıcı Adı"
                    fullWidth
                    margin="normal"
                    value={formData.username}
                    onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                    required
                />
                <TextField
                    label="Şifre"
                    type="password"
                    fullWidth
                    margin="normal"
                    value={formData.password}
                    onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                    required
                />
                <TextField
                    label="Email"
                    type="email"
                    fullWidth
                    margin="normal"
                    value={formData.email}
                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    required
                />
                <TextField
                    label="Tam Ad"
                    fullWidth
                    margin="normal"
                    value={formData.fullName}
                    onChange={(e) => setFormData({ ...formData, fullName: e.target.value })}
                />
                <Button type="submit" variant="contained" sx={{ mt: 3 }}>
                    Kayıt Ol
                </Button>
            </Box>
        </Container>
    );
}