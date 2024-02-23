import { LinkContainer } from 'react-router-bootstrap';
import { Nav, Navbar, Container } from 'react-bootstrap';

function Navigation() {
    return (
        <Navbar bg="light" expand="sm">
            <Container>
                <LinkContainer to="/">
                    <Navbar.Brand>Kässeli</Navbar.Brand>
                </LinkContainer>
                <Navbar.Toggle aria-controls="basic-navbar-nav" />
                <Navbar.Collapse id="basic-navbar-nav">
                    <Nav className="me-auto">
                        <LinkContainer to="/">
                            <Nav.Link>Startseite</Nav.Link>
                        </LinkContainer>
                        <LinkContainer to="/accounts">
                            <Nav.Link>Konten</Nav.Link>
                        </LinkContainer>
                    </Nav>
                </Navbar.Collapse>
            </Container>
        </Navbar>
    );
}

export default Navigation;