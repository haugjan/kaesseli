import { Link } from 'react-router-dom';

import { Nav, Navbar, Container } from 'react-bootstrap';

function navigation() {
    return (
        <Navbar bg="light" expand="sm">
            <Container>
                <Link to="/">
                    <Navbar.Brand>Kässeli</Navbar.Brand>
                </Link>
                <Navbar.Toggle aria-controls="basic-navbar-nav" />
                <Navbar.Collapse id="basic-navbar-nav">
                    <Nav className="me-auto">
                        <Link to="/">
                            <Nav.Link>Startseite</Nav.Link>
                        </Link>
                        <Link to="/accounts">
                            <Nav.Link>Konten</Nav.Link>
                        </Link>
                    </Nav>
                </Navbar.Collapse>
            </Container>
        </Navbar>
    );
}

export default navigation;