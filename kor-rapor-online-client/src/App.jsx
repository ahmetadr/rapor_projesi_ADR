import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Provider } from 'react-redux';
import { store } from './store/store';
import { ThemeProvider } from '@mui/material/styles';
import { SnackbarProvider } from 'notistack';
import CssBaseline from '@mui/material/CssBaseline';
import theme from './theme';

// Components
import Layout from './components/Layout';
import ProtectedRoute from './components/ProtectedRoute';

// Pages
import Home from './pages/Home';
import Login from './pages/Auth/Login';
import Register from './pages/Auth/Register';
import ReportDesigner from './pages/Reports/ReportDesigner';
import ReportViewer from './pages/Reports/ReportViewer';
import ReportList from './pages/Reports/ReportList';
import Connections from './pages/Connections';
import NotFound from './pages/NotFound';
import Unauthorized from './pages/Unauthorized';

// Contexts
import { ReportProvider } from './context/ReportContext';

function App() {
    return (
        <Provider store={store}>
            <ThemeProvider theme={theme}>
                <SnackbarProvider maxSnack={3}>
                    <CssBaseline />
                    <Router>
                        <ReportProvider>
                            <Routes>
                                <Route path="/login" element={<Login />} />
                                <Route path="/register" element={<Register />} />

                                <Route element={<ProtectedRoute />}>
                                    <Route element={<Layout />}>
                                        <Route path="/" element={<Home />} />

                                        {/* Report Routes */}
                                        <Route path="/reports" element={<ReportList />} />
                                        <Route path="/reports/new" element={<ReportDesigner />} />
                                        <Route path="/reports/:id" element={<ReportViewer />} />
                                        <Route path="/reports/:id/edit" element={<ReportDesigner />} />

                                        {/* Database Connection Routes */}
                                        <Route path="/connections" element={<Connections />} />

                                        {/* Error Routes */}
                                        <Route path="/unauthorized" element={<Unauthorized />} />
                                        <Route path="*" element={<NotFound />} />
                                    </Route>
                                </Route>
                            </Routes>
                        </ReportProvider>
                    </Router>
                </SnackbarProvider>
            </ThemeProvider>
        </Provider>
    );
}

export default App;