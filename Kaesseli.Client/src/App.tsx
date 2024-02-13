//import React from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';
import Navigation from './Navigation';
import ExamplePage from './ExamplePage';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';


function App() {
    return (
        <Router>
            <Navigation />
            <Routes>
                <Route path="/" element={<div>Startseite</div>} />
                <Route path="/example" element={<ExamplePage />} />
            </Routes>
        </Router>
    );
}

export default App;