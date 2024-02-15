import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';
import { IAccountSummary } from "./IAccountSummary";

// ReSharper disable once InconsistentNaming
function BalanceSheetAccount({ accounts }: { accounts: IAccountSummary[] }) {
    return (
        <table className="table">
            <thead>
                <tr>
                    <th>Name</th>
                    <th className="text-end">Kontostand</th>
                </tr>
            </thead>
            <tbody>
                {accounts.map(account => (
                    <tr key={account.id}>
                        <td>{account.name}</td>
                        <td className="fw-bold text-end">{account.accountBalance.toFixed(2)}</td>
                    </tr>
                ))}
            </tbody>
        </table>
    );
}

export default BalanceSheetAccount;