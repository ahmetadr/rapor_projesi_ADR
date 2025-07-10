import { Outlet } from 'react-router-dom';
import { Container, AppBar, Toolbar, Typography, Button } from '@mui/material';
import { useDispatch, useSelector } from 'react-redux';
import { logout } from '../../store/authSlice';

export default function Layout() {
    const { user } = useSelector((state) => state.auth);
    const dispatch = useDispatch();

    const handleLogout = () => {
        dispatch(logout());
    };

    return (
        <>
            <AppBar position="static">
                <Toolbar>
                    <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
                        Kor Rapor Sistemi
                    </Typography>
                    {user && (
                        <Button color="inherit" onClick={handleLogout}>
                            Çıkış Yap
                        </Button>
                    )}
                </Toolbar>
            </AppBar>
            <Container maxWidth="lg" sx={{ mt: 4 }}>
                <Outlet />
            </Container>
        </>
    );
}