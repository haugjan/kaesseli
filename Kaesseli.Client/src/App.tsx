//import React from 'react';
import Navigation from './Navigation';
import AccountSummary from './AccountSummary';
import Account from './Account';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';

// ReSharper disable once InconsistentNaming
function App() {
    return (
        <Router>
            <Navigation  />
            <Routes>
                <Route path="/" element={<div>Startseite</div>} />
                <Route path="/accounts" element={<AccountSummary />} />
                <Route path="/account/:accountId" element={<Account />} />
            </Routes>
        </Router>
    );
}


export default App;