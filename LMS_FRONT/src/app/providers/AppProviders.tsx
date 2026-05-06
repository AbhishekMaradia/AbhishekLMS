import React from 'react';
import { Provider } from 'react-redux';
import { PersistGate } from 'redux-persist/integration/react';
import { BrowserRouter as Router } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import { store, persistor } from '../../store/index';
import 'react-toastify/dist/ReactToastify.css';
import { ThemeProvider } from './ThemeProvider';
import { ModalProvider } from './ModalProvider';

export const AppProviders: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    return (
        <Provider store={store}>
            <PersistGate loading={null} persistor={persistor}>
                <ModalProvider>
                    <ThemeProvider>
                        <Router>
                            <ToastContainer 
                                theme="colored" 
                                position="bottom-right" 
                                autoClose={3000} 
                                hideProgressBar={false}
                                newestOnTop={false}
                                closeOnClick
                                rtl={false}
                                pauseOnFocusLoss
                                draggable
                                pauseOnHover
                            />
                            {children}
                        </Router>
                    </ThemeProvider>
                </ModalProvider>
            </PersistGate>
        </Provider>
    );
};
