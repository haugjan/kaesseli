import { useState, useEffect} from 'react';
import  fetch  from 'node-fetch';
interface IAccountDetail {
    name: string;
    accountBalance: number;
    budget: number;
    budgetBalance: number;
    entries: IJournalEntry[];
}

interface IJournalEntry {
    valueDate: string;
    amountType: number;
    otherAccount: string;
    amount: number;
    description: string;
}

// ReSharper disable once InconsistentNaming
function Account({ accountId }: { accountId: number }) {
    /*const { accountId } = useParams();*/
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

    return (

        <div className="container mt-5">
            {account && (
                <div className="row">
                    <div className="col-lg-6 col-sm-12">
                        <h1>Konto {account.name}</h1>
                        <div className="lead">
                            <em>Kontostand: </em>{account.accountBalance}&nbsp;
                            <em>Budget: </em>{account.budget}&nbsp;
                            <em>Vergleich: </em>{account.budgetBalance}&nbsp;
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
                                    <tr >
                                        <td>{entry.valueDate}</td>
                                        <td className={entry.amountType === 1 ? 'fw-bold' : ''}>{(entry.amountType === 1 ? 'Budget ' : '') + entry.description}</td>
                                        <td>{entry.otherAccount}</td>
                                        <td className="text-end">{entry.amountType !== 3 ? entry.amount : ''}</td>
                                        <td className="text-end">{entry.amountType === 3 ? entry.amount : ''}</td>
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