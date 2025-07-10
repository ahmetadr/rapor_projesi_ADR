import api from './api';
import { prepareHeaders } from '../utils/apiHelpers';

export const reportApi = {
    fetchReports: async () => {
        const response = await api.get('/api/reports', {
            headers: prepareHeaders()
        });
        return response.data;
    },

    getReportById: async (id) => {
        const response = await api.get(`/api/reports/${id}`, {
            headers: prepareHeaders()
        });
        return response.data;
    },

    createReport: async (reportData) => {
        const response = await api.post('/api/reports', reportData, {
            headers: prepareHeaders()
        });
        return response.data;
    },

    updateReport: async (id, reportData) => {
        const response = await api.put(`/api/reports/${id}`, reportData, {
            headers: prepareHeaders()
        });
        return response.data;
    },

    deleteReport: async (id) => {
        await api.delete(`/api/reports/${id}`, {
            headers: prepareHeaders()
        });
    },

    executeReport: async (id, parameters) => {
        const response = await api.post(`/api/reports/${id}/execute`, { parameters }, {
            headers: prepareHeaders()
        });
        return response.data;
    },

    saveReport: async (userId, reportData, definition) => {
        const response = await api.post('/api/reports/save', {
            userId,
            model: reportData,
            definition
        }, {
            headers: prepareHeaders()
        });
        return response.data;
    }
};