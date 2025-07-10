import { useState, useCallback } from 'react';
import api from '../api/api';
import { handleApiError } from '../utils/apiHelpers';

export default function useApi() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const sendRequest = useCallback(async (config) => {
        setLoading(true);
        setError(null);

        try {
            const response = await api({
                method: config.method || 'GET',
                url: config.url,
                data: config.body,
                headers: config.headers
            });
            return response.data;
        } catch (err) {
            const formattedError = handleApiError(err);
            setError(formattedError);
            throw formattedError;
        } finally {
            setLoading(false);
        }
    }, []);

    return { sendRequest, loading, error };
}