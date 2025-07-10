import { useEffect, useState } from 'react';
import api from '../../api/api';
import { Table, Button } from '@mui/material';


export default function Connections() {
    const [connections, setConnections] = useState([]);

    useEffect(() => {
        const loadConnections = async () => {
            const res = await api.get('/api/database/connections');
            setConnections(res.data);
        };
        loadConnections();
    }, []);

    return (
        <Table>
            {/* Bağlantı listesi tablosu */}
        </Table>
    );
}