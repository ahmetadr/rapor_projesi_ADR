import { ThemeProvider } from '@mui/material/styles';
import theme from './assets/styles/theme';
import './assets/styles/global.css';

ReactDOM.render(
    <ThemeProvider theme={theme}>
        <App />
    </ThemeProvider>,
    document.getElementById('root')
);