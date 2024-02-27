import React, { useState, useEffect, ChangeEvent, FormEvent } from 'react';
import { Form, Button, Dropdown, DropdownButton } from 'react-bootstrap';

interface IAccount {
    id: string; // oder number, je nachdem, wie Ihre Account-IDs strukturiert sind
    name: string;
}

const CamtUpload: React.FC = () => {
    const [accounts, setAccounts] = useState<IAccount[]>([]);
    const [selectedAccount, setSelectedAccount] = useState<string>('');
    const [file, setFile] = useState<File | null>(null);

    useEffect(() => {
        const fetchAccounts = async () => {
            try {
                const response = await fetch('https://localhost:7123/account');
                if (!response.ok) {
                    throw new Error('Netzwerkantwort war nicht ok');
                }
                const data: IAccount[] = await response.json();
                setAccounts(data);
            } catch (error) {
                console.error('Fehler beim Laden der Accounts:', error);
            }
        };

        fetchAccounts();
    }, []);

    const handleAccountSelect = (accountId: string) => {
        setSelectedAccount(accountId);
    };

    const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
        const files = event.target.files;
        if (files) {
            setFile(files[0]);
        }
    };

    const handleSubmit = (event: FormEvent) => {
        event.preventDefault();
        // Implementieren Sie hier die Logik zum Senden der Daten an Ihren Server
        console.log("Ausgewählter Account:", selectedAccount);
        console.log("Hochgeladene Datei:", file);
    };

    return (
        <Form onSubmit={handleSubmit}>
            <DropdownButton id="dropdown-basic-button" title="Account auswählen">
                {accounts.map((account) => (
                    <Dropdown.Item key={account.id} onClick={() => handleAccountSelect(account.id)}>
                        {account.name}
                    </Dropdown.Item>
                ))}
            </DropdownButton>

            <Form.Group controlId="formFile" className="mb-3">
                <Form.Label>Camt053 Datei hochladen</Form.Label>
                <Form.Control type="file" onChange={handleFileChange} accept=".xml" />
            </Form.Group>

            <Button variant="primary" type="submit">
                Einreichen
            </Button>
        </Form>
    );
};

export default CamtUpload;
