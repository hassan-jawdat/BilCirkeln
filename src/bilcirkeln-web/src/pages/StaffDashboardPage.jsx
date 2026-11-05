import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getAllSubmissions } from '../api/submissionsApi'
import { getErrorMessage } from '../api/client'
import StatusBadge from '../components/StatusBadge'
import { formatDate, formatMileage, formatPrice, statusLabels } from '../labels'

export default function StaffDashboardPage() {
  const [submissions, setSubmissions] = useState([])
  const [status, setStatus] = useState('Pending')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  // Runs again every time "status" changes, so the list follows the filter.
  useEffect(() => {
    setLoading(true)
    setError('')
    getAllSubmissions(status)
      .then(setSubmissions)
      .catch((err) => setError(getErrorMessage(err)))
      .finally(() => setLoading(false))
  }, [status])

  return (
    <section>
      <h2>Förfrågningar</h2>
      <label className="filter">
        Visa:
        <select value={status} onChange={(e) => setStatus(e.target.value)}>
          <option value="">Alla</option>
          {Object.entries(statusLabels).map(([value, label]) => (
            <option key={value} value={value}>{label}</option>
          ))}
        </select>
      </label>

      {error && <p className="error">{error}</p>}
      {loading && <p>Laddar...</p>}
      {!loading && submissions.length === 0 && <p>Inga förfrågningar.</p>}

      <div className="list">
        {submissions.map((s) => (
          <Link key={s.id} to={`/forfragan/${s.id}`} className="card list-item">
            <div>
              <strong>{s.make} {s.model}</strong> ({s.year}) – {s.registrationNumber}
              <div className="muted">
                {s.customerName} · {formatMileage(s.mileageKm)} · Önskat {formatPrice(s.askingPrice)} · {formatDate(s.createdAt)}
              </div>
            </div>
            <StatusBadge status={s.status} />
          </Link>
        ))}
      </div>
    </section>
  )
}
