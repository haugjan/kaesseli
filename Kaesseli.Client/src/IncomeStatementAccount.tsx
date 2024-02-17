import { IAccountSummary } from "./IAccountSummary";
import { Link } from 'react-router-dom';
import { Table } from 'react-bootstrap';

const getComparisonStyle = (value: number) => {
    if (value > 0) {
        return "fw-bold text-end text-success"; 
    } else if (value < 0) {
        return "fw-bold text-end text-danger";
    } else {
        return "fw-bold text-end";
    }
};

function IncomeStatementAccount({ accounts }: { accounts: IAccountSummary[] }) {
    return (
        <Table>
            <thead>
                <tr>
                    <th>Name</th>
                    <th style={{ textAlign: 'right' }}>Kontostand</th>
                    <th style={{ textAlign: 'right' }}>Budget</th>
                    <th style={{ textAlign: 'right' }}>Vergleich</th>
                </tr>
            </thead>
            <tbody>
                {accounts.map(account => (
                    <tr key={account.id}>
                        <td><Link to={`/account/${account.id}`}>{account.name}</Link></td>
                        <td style={{ textAlign: 'right' }}>{account.accountBalance.toFixed(2)}</td>
                        <td style={{ textAlign: 'right' }}>{account.budget.toFixed(2)}</td>
                        <td className={getComparisonStyle(account.budgetBalance)}>{account.budgetBalance.toFixed(2)}</td>
                    </tr>
                ))}
            </tbody>
        </Table>
    );
}

export default IncomeStatementAccount;