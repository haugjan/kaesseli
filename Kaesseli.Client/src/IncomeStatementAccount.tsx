import { IAccountSummary } from "./IAccountSummary";

const getComparisonColor = (value: number) => {
    if (value > 0) {
        return 'fw-bold text-end text-success';
    } else if (value < 0) {
        return 'fw-bold text-end text-danger';
    } else {
        return 'fw-bold text-end';
    }
};


// ReSharper disable once InconsistentNaming
function IncomeStatementAccount({ accounts }: { accounts: IAccountSummary[] }) {
    return (
        <table className="table">
            <thead>
                <tr>
                    <th>Name</th>
                    <th className="text-end">Kontostand</th>
                    <th className="text-end">Budget</th>
                    <th className="text-end">Vergleich</th>
                </tr>
            </thead>
            <tbody>
                {accounts.map(account => (
                    <tr key={account.id}>
                        <td>{account.name}</td>
                        <td className="text-end">{account.accountBalance.toFixed(2)}</td>
                        <td className="text-end">{account.budget.toFixed(2)}</td>
                        <td className={getComparisonColor(account.budgetBalance)}>{account.budgetBalance.toFixed(2)}</td>
                    </tr>
                ))}
            </tbody>
        </table>
    );
}

export default IncomeStatementAccount;