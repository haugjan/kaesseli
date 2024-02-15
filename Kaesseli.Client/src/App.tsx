//import React from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';
import Navigation from './Navigation';
import AccountSummary from './AccountSummary';
import Account from './Account';
import { BrowserRouter as Router, Route, Routes} from 'react-router-dom';


// ReSharper disable once InconsistentNaming
function App() {
    return (
        <Router>
            <Navigation />
            <Routes>
                <Route path="/" element={<div>Startseite</div>} />
                <Route path="/accounts" element={<AccountSummary />} />
                <Route path="/account/:accountId" element={<Account accountId={accountId}/>} />
            </Routes>
        </Router>
    );
}


export default App;