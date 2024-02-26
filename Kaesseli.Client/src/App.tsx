//import React from 'react';
import Navigation from './Navigation';
import AccountSummary from './AccountSummary';
import Account from "./IAccountDetail";
import CamtUpload from "./CamtUpload";
import Transaction from "./Transaction";

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
                <Route path="/camtUpload" element={<CamtUpload />} />
                <Route path="/transactionSummary" element={<Transaction />} />
            </Routes>
        </Router>
    );
}


export default App;