import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { useNavigate } from 'react-router-dom';
import { Link } from 'react-router-dom';
interface IAccountDetail {
    id: string;
    name: string;
    accountBalance: number;
    budget: number;
    budgetBalance: number;
    entries: IJournalEntry[];
}

interface IJournalEntry {
    id: string;
    valueDate: string;
    amountType: number;
    otherAccount: string;
    otherAccountId: string;
    amount: number;
    description: string;
}

// ReSharper disable once InconsistentNaming
function Account({accountId} : {accountId: string}) {
    //const { accountId } = useParams();
    const [account, setAccounts] = useState<IAccountDetail>();
    const [error, setError] = useState(null);

    useEffect(() => {
        const apiUrl = `https://localhost:7123/account/${accountId}`;

        fetch(apiUrl)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network Error');
                }
                return response.json();
            })
            .then(data => setAccounts(data))
            .catch(error => setError(error.message));
    }, []);

    if (error) {
        return <div>Fehler beim Laden der Daten: {error}</div>;
    }
    let navigate = useNavigate();

    return (

        <div className="container mt-5">
            {account && (
                <div className="row" key={account.id}>
                    <div><button type="button" className="btn btn-primary" onClick={() => navigate(-1)}>Zurück</button></div>
                    <div className="col-lg-6 col-sm-12">
                        <h1>Konto {account.name}</h1>
                        <div className="lead">
                            <em>Kontostand: </em>{account.accountBalance.toFixed(2)}&nbsp;
                            <em>Budget: </em>{account.budget.toFixed(2)}&nbsp;
                            <em>Vergleich: </em>{account.budgetBalance.toFixed(2)}&nbsp;
                        </div>
                        <table>
                            <thead>
                                <tr>
                                    <th>Datum</th>
                                    <th>Text</th>
                                    <th >Konto</th>
                                    <th className="text-end">Soll</th>
                                    <th className="text-end">Haben</th>
                                </tr>
                            </thead>
                            <tbody>
                                {account.entries.map(entry => (
                                    <tr key={entry.id}>
                                        <td>{entry.valueDate}</td>
                                        <td className={entry.amountType === 1 ? 'fw-bold' : ''}>{(entry.amountType === 1 ? 'Budget ' : '') + entry.description}</td>
                                        <td><Link to={`/account/${entry.otherAccountId}`}> {entry.otherAccount}</Link></td>
                                        <td className="text-end">{entry.amountType !== 3 ? entry.amount.toFixed(2) : ''}</td>
                                        <td className="text-end">{entry.amountType === 3 ? entry.amount.toFixed(2) : ''}</td>
                                    </tr>
                                ))}

                            </tbody>
                        </table>
                    </div>
                </div>
            )}
        </div>
    );
}



export default Account;