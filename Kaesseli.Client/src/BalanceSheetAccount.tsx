import { IAccountSummary } from "./IAccountSummary";
import { Link } from 'react-router-dom';
import { Table } from 'react-bootstrap';

function balanceSheetAccount({ accounts }: { accounts: IAccountSummary[] }) {
    return (
        <Table >
            <thead>
            <tr>
                <th>Name</th>
                    <th style={{ textAlign: 'right' }}>Kontostand</th>
                </tr>
            </thead>
            <tbody>
            {accounts.map(account => (
                <tr key={account.id}>
                    <td>
                        <Link to={`/account/${account.id}`}>{account.name}</Link>
                    </td>
                    <td style={{ fontWeight: 'bold', textAlign: 'right' }}>{account.accountBalance.toFixed(2)}</td>
                </tr>
            ))}
            </tbody>
        </Table>
    );
}

export default balanceSheetAccount;