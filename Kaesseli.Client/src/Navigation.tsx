import { Link } from 'react-router-dom';

function Navigation() {

    return <nav className="navbar navbar-expand-sm navbar-light bg-light">
               <Link className="navbar-brand" to="/">Kässeli</Link>
               <button className="navbar-toggler" type="button" data-toggle="collapse" data-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
                   <span className="navbar-toggler-icon"></span>
               </button>
               <div className="collapse navbar-collapse" id="navbarNav">
                   <ul className="navbar-nav">
                       <li className="nav-item">
                           <Link className="nav-link" to="/">Startseite</Link>
                       </li>
                       <li className="nav-item">
                           <Link className="nav-link" to="/example">Beispiel-Seite</Link>
                       </li>
                   </ul>
               </div>
           </nav>;

}

export default Navigation;