import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import * as api from '../api/submissionsApi'
import { getErrorMessage } from '../api/client'
import { useAuth } from '../context/AuthContext'
import StatusBadge from '../components/StatusBadge'
import {
  formatDate, formatMileage, formatPrice, fuelLabels, gearboxLabels, offerStatusLabels, labelOf,
  yesNoLabels, keyCountLabels, serviceHistoryLabels, inspectionLabels, sellerTypeLabels,
  interiorColorLabels, interiorMaterialLabels,
} from '../labels'

// Shared page for both roles. What you can DO depends on role and status.
export default function SubmissionDetailsPage() {
  const { id } = useParams() // The :id from the URL /forfragan/:id
  const { isStaff, isCustomer } = useAuth()
  const [submission, setSubmission] = useState(null)
  const [error, setError] = useState('')
  const [offerAmount, setOfferAmount] = useState('')
  const [offerMessage, setOfferMessage] = useState('')

  useEffect(() => {
    api.getSubmission(id)
      .then(setSubmission)
      .catch((err) => setError(getErrorMessage(err)))
  }, [id])

  // Runs an API action and shows the updated submission (or the error).
  async function runAction(action) {
    setError('')
    try {
      setSubmission(await action())
    } catch (err) {
      setError(getErrorMessage(err))
    }
  }

  async function handleOffer(e) {
    e.preventDefault()
    await runAction(() => api.makeOffer(id, Number(offerAmount), offerMessage))
    setOfferAmount('')
    setOfferMessage('')
  }

  if (!submission) return error ? <p className="error">{error}</p> : <p>Laddar...</p>

  const s = submission
  const canMakeOffer = isStaff && (s.status === 'Pending' || s.status === 'Declined')
  const canRespond = isCustomer && s.status === 'OfferMade'

  return (
    <section>
      <div className="title-row">
        <h2>{s.make} {s.model} ({s.year})</h2>
        <StatusBadge status={s.status} />
      </div>
      {error && <p className="error">{error}</p>}

      <div className="card">
        <dl className="details">
          <dt>Registreringsnummer</dt><dd>{s.registrationNumber}</dd>
          <dt>Mätarställning</dt><dd>{formatMileage(s.mileageKm)}</dd>
          <dt>Drivmedel</dt><dd>{fuelLabels[s.fuelType]}</dd>
          <dt>Växellåda</dt><dd>{gearboxLabels[s.gearbox]}</dd>
          <dt>Önskat pris</dt><dd>{formatPrice(s.askingPrice)}</dd>
          <dt>Inskickad</dt><dd>{formatDate(s.createdAt)}</dd>
          {isStaff && (<><dt>Kund</dt><dd>{s.customerName} ({s.customerEmail})</dd></>)}
        </dl>
        {s.description && <p className="description">{s.description}</p>}
      </div>

      <h3>Mer om bilen</h3>
      <div className="card">
        <dl className="details">
          <dt>Bilnycklar</dt><dd>{labelOf(keyCountLabels, s.keyCount)}</dd>
          <dt>Extra däck</dt><dd>{labelOf(yesNoLabels, s.hasExtraTires)}</dd>
          <dt>Servicebok</dt><dd>{labelOf(yesNoLabels, s.hasServiceBook)}</dd>
          <dt>Dokumenterad service</dt><dd>{labelOf(serviceHistoryLabels, s.serviceHistory)}</dd>
          <dt>Besiktad senaste 6 mån</dt><dd>{labelOf(inspectionLabels, s.inspectionStatus)}</dd>
          <dt>Säljare</dt><dd>{labelOf(sellerTypeLabels, s.sellerType)}</dd>
          <dt>Importerad</dt><dd>{labelOf(yesNoLabels, s.isImported)}</dd>
          <dt>Finansierad/leasad</dt><dd>{labelOf(yesNoLabels, s.isFinanced)}</dd>
          <dt>Klädsel</dt>
          <dd>{labelOf(interiorColorLabels, s.interiorColor)}, {labelOf(interiorMaterialLabels, s.interiorMaterial)}</dd>
        </dl>
      </div>

      <h3>Bud</h3>
      {s.offers.length === 0 && <p className="muted">Inga bud ännu.</p>}
      {s.offers.map((o) => (
        <div key={o.id} className="card offer">
          <div>
            <strong>{formatPrice(o.amount)}</strong> – {offerStatusLabels[o.status]}
            <span className="muted"> · {formatDate(o.createdAt)}</span>
          </div>
          {o.message && <p>{o.message}</p>}
        </div>
      ))}

      {canRespond && (
        <div className="actions">
          <button className="button" onClick={() => runAction(() => api.respondToOffer(id, true))}>
            Acceptera budet
          </button>
          <button className="button secondary" onClick={() => runAction(() => api.respondToOffer(id, false))}>
            Neka budet
          </button>
        </div>
      )}

      {canMakeOffer && (
        <form className="card form" onSubmit={handleOffer}>
          <h3>Lämna bud</h3>
          <label>
            Belopp (kr)
            <input type="number" min={1} value={offerAmount} onChange={(e) => setOfferAmount(e.target.value)} required />
          </label>
          <label>
            Meddelande till kunden
            <textarea rows={3} maxLength={1000} value={offerMessage} onChange={(e) => setOfferMessage(e.target.value)} />
          </label>
          <div className="actions">
            <button type="submit" className="button">Skicka bud</button>
            <button type="button" className="button danger" onClick={() => runAction(() => api.rejectSubmission(id))}>
              Inte intresserad
            </button>
          </div>
        </form>
      )}
    </section>
  )
}
