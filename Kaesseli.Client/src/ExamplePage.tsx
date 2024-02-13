import { useState, useEffect } from 'react';

function ExamplePage() {
    const [accounts, setAccounts] = useState([]);
    const [error, setError] = useState(null);

    useEffect(() => {
        // Die URL des Endpunkts, von dem die Accounts abgerufen werden sollen
        const apiUrl = 'https://localhost:7123/account';

        fetch(apiUrl)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Netzwerkantwort war nicht ok');
                }
                return response.json();
            })
            .then(data => setAccounts(data))
            .catch(error => setError(error.message));
    }, []);

    // Fehlerbehandlung
    if (error) {
        return <div>Fehler beim Laden der Daten: {error}</div>;
    }

    return (
        <div className="container mt-5">
            <h2>Accounts</h2>
            <table className="table">
                <thead>
                <tr>
                    <th>Name</th>
                    <th>Typ</th>
                </tr>
                </thead>
                <tbody>
                {accounts.map(account => (
                    <tr key={account.id}>
                        <td>{account.name}</td>
                        <td>{account.type}</td>
                    </tr>
                ))}
                </tbody>
            </table>
        </div>
    );
}

export default ExamplePage;