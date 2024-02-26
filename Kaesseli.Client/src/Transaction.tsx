import { useState, useEffect } from "react";
import { Container } from "react-bootstrap";
import ListGroup from "react-bootstrap/ListGroup";

interface ITransactionSummary {
    valueDateFrom: string;
    valueDateTo: string;
}

function transaction() {
    const [summaries, setSummaries] = useState<ITransactionSummary[]>([]);
    const [error, setError] = useState(null);

    useEffect(() => {
            const apiUrl = "https://localhost:7123/transactionSummary";

            // ReSharper disable once TsResolvedFromInaccessibleModule
            fetch(apiUrl)
                .then(response => {
                    if (!response.ok) {
                        throw new Error("Network error");
                    }
                    return response.json();
                })
                .then(data => setSummaries(data))
                .catch(error => setError(error.message));
        },
        []);

    if (error) {
        return <div>Fehler beim Laden der Daten: {error} </div>;
    }

    return <Container className="mt-5">
               <ListGroup>
                   {summaries.map(summary => <ListGroup.Item>
                                                 {summary.valueDateFrom}
                                             </ListGroup.Item>)}
               </ListGroup>
           </Container>;
}

export default transaction();