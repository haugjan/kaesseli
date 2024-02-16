import { useState, useEffect } from 'react';
import BalanceSheetAccount from './BalanceSheetAccount';
import IncomeStatementAccount from './IncomeStatementAccount';
import { IAccountSummary } from "./IAccountSummary";

// ReSharper disable once InconsistentNaming
function AccountSummary() {
    const [accounts, setAccounts] = useState<IAccountSummary[]>([]);
    const [error, setError] = useState(null);

    useEffect(() => {
        const apiUrl = 'https://localhost:7123/accountSummary';

        fetch(apiUrl)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network error');
                }
                return response.json();
            })
            // ReSharper disable once TS2345
            .then(data => setAccounts(data))
            .catch(error => setError(error.message));
    }, []);

    if (error) {
        return <div>Fehler beim Laden der Daten: {error}</div>;
    }

    return (
        <div className="container mt-5">
            <h1>Konten</h1>
            <div className="row">
                <div className="col-lg-5 col-sm-12">
                    <h2>Aufwand</h2>
                    <IncomeStatementAccount accounts={accounts.filter(account => account.typeId === 3)} />
                </div>
                <div className="col-lg-5 col-sm-12">
                    <h2>Ertrag</h2>
                    <IncomeStatementAccount accounts={accounts.filter(account => account.typeId === 4)} />
                </div>
                <div className="col-lg-3 col-sm-12">
                    <h2>Aktive</h2>
                    <BalanceSheetAccount accounts={accounts.filter(account => account.typeId === 1)} />
                </div>
                <div className="offset-lg-2 col-lg-3 col-sm-12">
                    <h2>Passive</h2>
                    <BalanceSheetAccount accounts={accounts.filter(account => account.typeId === 2)} />
                </div>
            </div>
        </div>
    );
}

export default AccountSummary;