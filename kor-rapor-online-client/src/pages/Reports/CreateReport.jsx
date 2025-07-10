import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    Paper,
    TextField,
    Button,
    Typography,
    Box,
    Alert,
    CircularProgress
} from '@mui/material';
import { reportApi } from '../../api/reportApi';

export default function CreateReport() {
    const navigate = useNavigate();
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [formData, setFormData] = useState({
        title: '',
        description: '',
        queryText: ''
    });

    const handleSubmit = async (e) => {
        e.preventDefault();
        
        try {
            setLoading(true);
            setError(null);
            
            const createdReport = await reportApi.createReport(formData);
            navigate(`/reports/${createdReport.reportID}`);
        } catch (err) {
            setError('Rapor oluşturulurken bir hata oluştu');
            console.error('Error creating report:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
    };

    return (
        <Paper sx={{ p: 3 }}>
            <Typography variant="h5" gutterBottom>
                Yeni Rapor Oluştur
            </Typography>

            {error && (
                <Alert severity="error" sx={{ mb: 2 }}>
                    {error}
                </Alert>
            )}

            <form onSubmit={handleSubmit}>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                    <TextField
                        name="title"
                        label="Rapor Adı"
                        value={formData.title}
                        onChange={handleChange}
                        required
                    />

                    <TextField
                        name="description"
                        label="Açıklama"
                        value={formData.description}
                        onChange={handleChange}
                        multiline
                        rows={3}
                    />

                    <TextField
                        name="queryText"
                        label="SQL Sorgusu"
                        value={formData.queryText}
                        onChange={handleChange}
                        multiline
                        rows={5}
                        required
                    />

                    <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                        <Button
                            type="button"
                            onClick={() => navigate('/reports')}
                        >
                            İptal
                        </Button>
                        <Button
                            type="submit"
                            variant="contained"
                            disabled={loading}
                        >
                            {loading ? <CircularProgress size={24} /> : 'Kaydet'}
                        </Button>
                    </Box>
                </Box>
            </form>
        </Paper>
    );
}