import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../../api/api';
import { Box, Typography, Button, Paper, Grid, FormControlLabel, Checkbox, TextField, Select, MenuItem, InputLabel, FormControl } from '@mui/material';

const ReportDesigner = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [connections, setConnections] = useState([]);
  const [tables, setTables] = useState([]);
  const [columns, setColumns] = useState([]);
  const [report, setReport] = useState({
    ReportID: 0,
    ConnectionID: '',
    ReportName: '',
    Description: '',
    IsPublic: false
  });
  const [definition, setDefinition] = useState({
    BaseObject: '',
    ObjectType: 'Table',
    SelectedFields: [],
    Filters: [],
    Sorting: [],
    Grouping: [],
    Parameters: []
  });

  useEffect(() => {
    const fetchConnections = async () => {
      const response = await api.get('/api/database/connections');
      setConnections(response.data);
    };

    fetchConnections();

    if (id) {
      const fetchReport = async () => {
        const response = await api.get(`/api/reports/${id}`);
        setReport(response.data);
        const def = JSON.parse(response.data.QueryDefinition);
        setDefinition(def);
        
        // Tablo ve sütunları yükle
        if (def.BaseObject) {
          const tablesResponse = await api.get(`/api/database/${response.data.ConnectionID}/tables`);
          setTables(tablesResponse.data);
          
          const columnsResponse = await api.get(`/api/database/${response.data.ConnectionID}/tables/${def.BaseObject}/columns`);
          setColumns(columnsResponse.data);
        }
      };

      fetchReport();
    }
  }, [id]);

  const handleConnectionChange = async (e) => {
    const connectionId = e.target.value;
    setReport({ ...report, ConnectionID: connectionId });
    
    if (connectionId) {
      const response = await api.get(`/api/database/${connectionId}/tables`);
      setTables(response.data);
    } else {
      setTables([]);
      setColumns([]);
    }
  };

  const handleTableChange = async (e) => {
    const tableName = e.target.value;
    setDefinition({ ...definition, BaseObject: tableName });
    
    if (tableName && report.ConnectionID) {
      const response = await api.get(`/api/database/${report.ConnectionID}/tables/${tableName}/columns`);
      setColumns(response.data);
    } else {
      setColumns([]);
    }
  };

  const toggleFieldSelection = (column) => {
    setDefinition(prev => {
      const existingIndex = prev.SelectedFields.findIndex(f => f.Name === column.ColumnName);
      
      if (existingIndex >= 0) {
        const updatedFields = [...prev.SelectedFields];
        updatedFields.splice(existingIndex, 1);
        return { ...prev, SelectedFields: updatedFields };
      } else {
        return {
          ...prev,
          SelectedFields: [
            ...prev.SelectedFields,
            {
              Name: column.ColumnName,
              DisplayName: column.ColumnName,
              Visible: true,
              Aggregation: ''
            }
          ]
        };
      }
    });
  };

  const saveReport = async () => {
    const reportData = {
      ReportModel: report,
      ReportDefinition: definition
    };

    if (report.ReportID === 0) {
      await api.post('/api/reports', reportData);
    } else {
      await api.put(`/api/reports/${report.ReportID}`, reportData);
    }
    
    navigate('/reports');
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        {id ? 'Raporu Düzenle' : 'Yeni Rapor Oluştur'}
      </Typography>
      
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" gutterBottom>
          Rapor Bilgileri
        </Typography>
        <Grid container spacing={3}>
          <Grid item xs={12} sm={6}>
            <TextField
              label="Rapor Adı"
              fullWidth
              value={report.ReportName}
              onChange={(e) => setReport({ ...report, ReportName: e.target.value })}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              label="Açıklama"
              fullWidth
              value={report.Description}
              onChange={(e) => setReport({ ...report, Description: e.target.value })}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <FormControl fullWidth>
              <InputLabel>Veritabanı Bağlantısı</InputLabel>
              <Select
                value={report.ConnectionID}
                onChange={handleConnectionChange}
                label="Veritabanı Bağlantısı"
              >
                {connections.map(conn => (
                  <MenuItem key={conn.ConnectionID} value={conn.ConnectionID}>
                    {conn.ConnectionName} ({conn.ServerName})
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} sm={6}>
            <FormControlLabel
              control={
                <Checkbox 
                  checked={report.IsPublic}
                  onChange={(e) => setReport({ ...report, IsPublic: e.target.checked })}
                />
              }
              label="Herkese Açık"
            />
          </Grid>
        </Grid>
      </Paper>
      
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" gutterBottom>
          Veri Kaynağı
        </Typography>
        <Grid container spacing={3}>
          <Grid item xs={12} sm={6}>
            <FormControl fullWidth>
              <InputLabel>Tablo/View</InputLabel>
              <Select
                value={definition.BaseObject}
                onChange={handleTableChange}
                label="Tablo/View"
                disabled={!report.ConnectionID}
              >
                {tables.map(table => (
                  <MenuItem key={table.TableName} value={table.TableName}>
                    {table.TableName} ({table.TableType})
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} sm={6}>
            <FormControl fullWidth>
              <InputLabel>Tür</InputLabel>
              <Select
                value={definition.ObjectType}
                onChange={(e) => setDefinition({ ...definition, ObjectType: e.target.value })}
                label="Tür"
              >
                <MenuItem value="Table">Tablo</MenuItem>
                <MenuItem value="View">View</MenuItem>
              </Select>
            </FormControl>
          </Grid>
        </Grid>
      </Paper>
      
      <Grid container spacing={3}>
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 3, height: '100%' }}>
            <Typography variant="h6" gutterBottom>
              Sütunlar
            </Typography>
            <Box sx={{ maxHeight: 400, overflow: 'auto' }}>
              {columns.map(column => (
                <FormControlLabel
                  key={column.ColumnName}
                  control={
                    <Checkbox
                      checked={definition.SelectedFields.some(f => f.Name === column.ColumnName)}
                      onChange={() => toggleFieldSelection(column)}
                    />
                  }
                  label={`${column.ColumnName} (${column.DataType})`}
                />
              ))}
            </Box>
          </Paper>
        </Grid>
        
        <Grid item xs={12} md={8}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Rapor Önizleme
            </Typography>
            <Box sx={{ mb: 3 }}>
              <Typography variant="subtitle1">Seçili Alanlar:</Typography>
              <ul>
                {definition.SelectedFields.map(field => (
                  <li key={field.Name}>
                    {field.DisplayName} {field.Aggregation && `(${field.Aggregation})`}
                  </li>
                ))}
              </ul>
            </Box>
            
            <Button 
              variant="contained" 
              color="primary" 
              onClick={saveReport}
              disabled={!report.ReportName || !definition.BaseObject || definition.SelectedFields.length === 0}
            >
              Raporu Kaydet
            </Button>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default ReportDesigner;