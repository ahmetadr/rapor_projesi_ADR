import { useState, useEffect } from 'react';
import {
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Paper,
    Button,
    CircularProgress,
    Alert,
    Typography,
    IconButton
} from '@mui/material';
import { Edit, Delete, PlayArrow } from '@mui/icons-material';
import { reportApi } from '../../api/reportApi';
import { format } from 'date-fns';
import { tr } from 'date-fns/locale';
import { useNavigate } from 'react-router-dom';

export default function Reports() {
    const [reports, setReports] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        loadReports();
    }, []);

    const loadReports = async () => {
        try {
            setLoading(true);
            const data = await reportApi.fetchReports();
            setReports(data);
            setError(null);
        } catch (err) {
            setError('Raporlar yüklenirken bir hata oluştu');
            console.error('Error loading reports:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (id) => {
        if (!window.confirm('Bu raporu silmek istediğinizden emin misiniz?')) {
            return;
        }

        try {
            await reportApi.deleteReport(id);
            await loadReports();
        } catch (err) {
            setError('Rapor silinirken bir hata oluştu');
            console.error('Error deleting report:', err);
        }
    };

    const handleExecute = async (id) => {
        navigate(`/reports/${id}/execute`);
    };

    if (loading) {
        return <CircularProgress />;
    }

    if (error) {
        return <Alert severity="error">{error}</Alert>;
    }

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1rem' }}>
                <Typography variant="h5">Raporlar</Typography>
                <Button
                    variant="contained"
                    color="primary"
                    onClick={() => navigate('/reports/create')}
                >
                    Yeni Rapor
                </Button>
            </div>

            <TableContainer component={Paper}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Rapor Adı</TableCell>
                            <TableCell>Açıklama</TableCell>
                            <TableCell>Oluşturulma Tarihi</TableCell>
                            <TableCell>Son Güncelleme</TableCell>
                            <TableCell>İşlemler</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {reports.map((report) => (
                            <TableRow key={report.reportID}>
                                <TableCell>{report.reportName}</TableCell>
                                <TableCell>{report.description}</TableCell>
                                <TableCell>
                                    {format(new Date(report.createdAt), 'dd MMMM yyyy HH:mm', { locale: tr })}
                                </TableCell>
                                <TableCell>
                                    {report.updatedAt &&
                                        format(new Date(report.updatedAt), 'dd MMMM yyyy HH:mm', { locale: tr })}
                                </TableCell>
                                <TableCell>
                                    <IconButton
                                        onClick={() => navigate(`/reports/${report.reportID}/edit`)}
                                        color="primary"
                                    >
                                        <Edit />
                                    </IconButton>
                                    <IconButton
                                        onClick={() => handleExecute(report.reportID)}
                                        color="success"
                                    >
                                        <PlayArrow />
                                    </IconButton>
                                    <IconButton
                                        onClick={() => handleDelete(report.reportID)}
                                        color="error"
                                    >
                                        <Delete />
                                    </IconButton>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    );
}