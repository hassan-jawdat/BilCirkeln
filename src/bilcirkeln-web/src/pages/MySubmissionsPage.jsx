import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getMySubmissions } from '../api/submissionsApi'
import { getErrorMessage } from '../api/client'
import StatusBadge from '../components/StatusBadge'
import { formatDate, formatPrice } from '../labels'

export default function MySubmissionsPage() {
  const [submissions, setSubmissions] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  // useEffect with [] runs once when the page is shown - here we fetch the data.
  useEffect(() => {
    getMySubmissions()
      .then(setSubmissions)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <p>Laddar...</p>

  return (
    <section>
      <h2>Mina bilar</h2>
      {error && <p className="error">{error}</p>}
      {submissions.length === 0 && !error && (
        <p>Du har inte skickat in någon bil än. <Link to="/salj">Sälj din bil</Link></p>
      )}
      <div className="list">
        {submissions.map((s) => (
          <Link key={s.id} to={`/forfragan/${s.id}`} className="card list-item">
            <div>
              <strong>{s.make} {s.model}</strong> ({s.year}) – {s.registrationNumber}
              <div className="muted">Inskickad {formatDate(s.createdAt)} · Önskat pris {formatPrice(s.askingPrice)}</div>
            </div>
            <StatusBadge status={s.status} />
          </Link>
        ))}
      </div>
    </section>
  )
}
