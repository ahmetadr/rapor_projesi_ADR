import { createContext, useContext, useState, useEffect, useMemo } from 'react';
import { useSelector } from 'react-redux';
import api from '../api/api';

const ReportContext = createContext();

export function ReportProvider({ children }) {
    const { user } = useSelector((state) => state.auth);
    const [reports, setReports] = useState([]);
    const [selectedReport, setSelectedReport] = useState(null);
    const [reportResults, setReportResults] = useState(null);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState(null);

    const loadReports = async () => {
        setIsLoading(true);
        try {
            const response = await api.get('/reports');
            setReports(response.data);
            setError(null);
        } catch (err) {
            setError(err.response?.data?.message || 'Raporlar yüklenirken hata oluştu');
        } finally {
            setIsLoading(false);
        }
    };

    const runReport = async (reportId, parameters) => {
        setIsLoading(true);
        try {
            const response = await api.post(`/reports/${reportId}/execute`, parameters);
            setReportResults(response.data);
            setError(null);
            return response.data;
        } catch (err) {
            setError(err.response?.data?.message || 'Rapor çalıştırılırken hata oluştu');
            throw err;
        } finally {
            setIsLoading(false);
        }
    };

    const exportReport = async (reportId, format = 'excel') => {
        try {
            const response = await api.get(
                `/reports/${reportId}/export?format=${format}`,
                { responseType: 'blob' }
            );
            return response.data;
        } catch (err) {
            setError(err.response?.data?.message || 'Rapor dışa aktarılırken hata oluştu');
            throw err;
        }
    };

    useEffect(() => {
        if (user) {
            loadReports();
        } else {
            setReports([]);
            setSelectedReport(null);
            setReportResults(null);
        }
    }, [user]);

    const value = useMemo(() => ({
        reports,
        selectedReport,
        setSelectedReport,
        reportResults,
        isLoading,
        error,
        loadReports,
        runReport,
        exportReport
    }), [reports, selectedReport, reportResults, isLoading, error]);

    return (
        <ReportContext.Provider value={value}>
            {children}
        </ReportContext.Provider>
    );
}

export const useReports = () => {
    const context = useContext(ReportContext);
    if (!context) {
        throw new Error('useReports hook must be used within a ReportProvider');
    }
    return context;
};