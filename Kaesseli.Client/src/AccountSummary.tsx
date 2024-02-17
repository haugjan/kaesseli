import { useState, useEffect } from 'react';
import BalanceSheetAccount from './BalanceSheetAccount';
import IncomeStatementAccount from './IncomeStatementAccount';
import { IAccountSummary } from "./IAccountSummary";
import { Container, Row, Col } from 'react-bootstrap';

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
            .then(data => setAccounts(data))
            .catch(error => setError(error.message));
    }, []);

    if (error) {
        return <div>Fehler beim Laden der Daten: {error}</div>;
    }

    return (
        <Container className="mt-5">
            <h1>Konten</h1>
            <Row>
                <Col lg={5} sm={12}>
                    <h2>Aufwand</h2>
                    <IncomeStatementAccount accounts={accounts.filter(account => account.typeId === 3)} />
                </Col>
                <Col lg={5} sm={12}>
                    <h2>Ertrag</h2>
                    <IncomeStatementAccount accounts={accounts.filter(account => account.typeId === 4)} />
                </Col>
                <Col lg={3} sm={12}>
                    <h2>Aktive</h2>
                    <BalanceSheetAccount accounts={accounts.filter(account => account.typeId === 1)} />
                </Col>
                <Col lg={{ span: 3, offset: 2 }} sm={12}>
                    <h2>Passive</h2>
                    <BalanceSheetAccount accounts={accounts.filter(account => account.typeId === 2)} />
                </Col>
            </Row>
        </Container>
    );
}

export default AccountSummary;