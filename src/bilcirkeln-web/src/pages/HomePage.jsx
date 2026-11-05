import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function HomePage() {
  const { user, isStaff } = useAuth()

  return (
    <section className="hero">
      <h1>Vi köper din bil</h1>
      <p>
        Slipp krånglet med annonser och provkörningar. Skicka in uppgifter om din bil,
        så lämnar vi ett bud. Du bestämmer själv om du vill acceptera.
      </p>

      <ol className="steps">
        <li><strong>1.</strong> Skapa ett konto och fyll i uppgifter om bilen.</li>
        <li><strong>2.</strong> Vi går igenom bilen och lämnar ett bud.</li>
        <li><strong>3.</strong> Du accepterar eller nekar budet.</li>
      </ol>

      {!user && (
        <div className="actions">
          <Link to="/registrera" className="button">Kom igång</Link>
          <Link to="/login" className="button secondary">Logga in</Link>
        </div>
      )}
      {user && !isStaff && <Link to="/salj" className="button">Sälj din bil</Link>}
      {isStaff && <Link to="/personal" className="button">Gå till förfrågningar</Link>}
    </section>
  )
}
