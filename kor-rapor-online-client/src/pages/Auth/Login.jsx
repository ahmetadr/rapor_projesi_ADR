import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, TextField, Button, Typography, Container, Paper } from '@mui/material';
import { loginStart, loginSuccess, loginFailure } from '../../store/authSlice';
import api from '../../api/api';

const Login = () => {
    const [credentials, setCredentials] = useState({ username: '', password: '' });
    const { error } = useSelector((state) => state.auth);
    const dispatch = useDispatch();
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            dispatch(loginStart());
            const response = await api.post('/Auth/login', credentials);
            dispatch(loginSuccess(response.data));
            navigate('/');
        } catch (err) {
            dispatch(loginFailure(err.response?.data?.message || 'Kullanıcı adı veya şifre hatalı'));
        }
    };

    return (
        <Container maxWidth="sm">
            <Paper elevation={3} sx={{ p: 4, mt: 8 }}>
                <Typography variant="h4" gutterBottom align="center">
                    Giriş Yap
                </Typography>
                {error && (
                    <Typography color="error" align="center" sx={{ mb: 2 }}>
                        {error}
                    </Typography>
                )}
                <Box component="form" onSubmit={handleSubmit}>
                    <TextField
                        label="Kullanıcı Adı"
                        fullWidth
                        margin="normal"
                        value={credentials.username}
                        onChange={(e) => setCredentials({ ...credentials, username: e.target.value })}
                        required
                    />
                    <TextField
                        label="Şifre"
                        type="password"
                        fullWidth
                        margin="normal"
                        value={credentials.password}
                        onChange={(e) => setCredentials({ ...credentials, password: e.target.value })}
                        required
                    />
                    <Button
                        type="submit"
                        variant="contained"
                        fullWidth
                        size="large"
                        sx={{ mt: 3 }}
                    >
                        Giriş Yap
                    </Button>
                </Box>
            </Paper>
        </Container>
    );
};

export default Login;