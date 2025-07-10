import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import { TextField, Button } from '@mui/material';

const loginValidationSchema = Yup.object({
    email: Yup.string().email('Geçerli bir email giriniz').required('Email zorunludur'),
    password: Yup.string().required('Şifre zorunludur')
});

const LoginForm = ({ onSubmit }) => {
    return (
        <Formik
            initialValues={{ email: '', password: '' }}
            validationSchema={loginValidationSchema}
            onSubmit={onSubmit}
        >
            {({ errors, touched, handleChange }) => (
                <Form>
                    <TextField
                        fullWidth
                        name="email"
                        label="Email"
                        onChange={handleChange}
                        error={touched.email && Boolean(errors.email)}
                        helperText={touched.email && errors.email}
                        margin="normal"
                    />
                    <TextField
                        fullWidth
                        name="password"
                        type="password"
                        label="Şifre"
                        onChange={handleChange}
                        error={touched.password && Boolean(errors.password)}
                        helperText={touched.password && errors.password}
                        margin="normal"
                    />
                    <Button type="submit" variant="contained" color="primary">
                        Giriş Yap
                    </Button>
                </Form>
            )}
        </Formik>
    );
};