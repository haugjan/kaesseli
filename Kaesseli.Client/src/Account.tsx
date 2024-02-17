import { useState, useEffect, Fragment } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { Container, Button, Row, Col, Table } from 'react-bootstrap';
import 'bootstrap/dist/css/bootstrap.min.css';

interface IAccountDetail {
    id: string;
    name: string;
    type: string;
    typeId: number;
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
function Account() {
    const { accountId } = useParams();
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
    }, [accountId]);

    if (error) {
        return <div>Fehler beim Laden der Daten: {error}</div>;
    }

    return (
        <Container>
            {account && (
                <Fragment>
                    <Row>
                        <Col lg={6} sm={12}>
                            <h1>Konto {account.name} ({account.type})</h1>
                            <div className="lead">
                                <em>Kontostand: </em>{account.accountBalance.toFixed(2)}&nbsp;
                                {account.typeId === 3 || account.typeId === 4 ? (
                                    <Fragment>
                                        <em>Budget: </em>{account.budget.toFixed(2)}&nbsp;
                                        <em>Vergleich: </em>{account.budgetBalance.toFixed(2)}&nbsp;
                                    </Fragment>
                                ) : null}
                            </div>
                            <Table variant="dark">
                                <thead>
                                    <tr>
                                        <th>Datum</th>
                                        <th>Text</th>
                                        <th>Konto</th>
                                        <th className="text-end">Soll</th>
                                        <th className="text-end">Haben</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {account.entries.map(entry => (
                                        <tr key={entry.id}>
                                            <td>{new Date(entry.valueDate).toLocaleDateString('de-CH', { day: '2-digit', month: '2-digit', year: 'numeric' })}</td>
                                            <td className={entry.amountType === 1 ? 'fw-bold' : ''}>{(entry.amountType === 1 ? 'Budget ' : '') + entry.description}</td>
                                            <td><Link to={`/account/${entry.otherAccountId}`}>{entry.otherAccount}</Link></td>
                                            <td className="text-end">{entry.amountType !== 3 ? entry.amount.toFixed(2) : ''}</td>
                                            <td className="text-end">{entry.amountType === 3 ? entry.amount.toFixed(2) : ''}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </Col>
                    </Row>
                </Fragment>
            )}
        </Container>
    );
}

export default Account;
