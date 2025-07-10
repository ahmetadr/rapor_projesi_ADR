import { Typography, Box } from '@mui/material';
import { useSelector } from 'react-redux';

export default function Home() {
    const { user } = useSelector((state) => state.auth);

    return (
        <Box sx={{ textAlign: 'center', mt: 4 }}>
            <Typography variant="h3" gutterBottom>
                Hoş Geldiniz, {user?.username}
            </Typography>
            <Typography variant="body1">
                Sol menüden raporlarınıza ve veritabanı bağlantılarınıza erişebilirsiniz.
            </Typography>
        </Box>
    );
}