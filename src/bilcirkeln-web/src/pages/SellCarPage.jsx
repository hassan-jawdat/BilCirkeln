import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { createSubmission } from '../api/submissionsApi'
import { getErrorMessage } from '../api/client'
import {
  fuelLabels, gearboxLabels, keyCountLabels, serviceHistoryLabels, inspectionLabels,
  sellerTypeLabels, interiorColorLabels, interiorColorDots, interiorMaterialLabels, toOptions,
} from '../labels'
import LicensePlateInput from '../components/LicensePlateInput'
import ProgressGauge from '../components/ProgressGauge'
import ChoiceButtons from '../components/ChoiceButtons'

const yesNo = [
  { value: true, label: 'Ja' },
  { value: false, label: 'Nej' },
]

// The questions that are answered with buttons. Each one becomes its own step.
const choiceQuestions = [
  { name: 'keyCount', question: 'Hur många bilnycklar har du?', options: toOptions(keyCountLabels) },
  { name: 'hasExtraTires', question: 'Andra uppsättning däck tillgänglig?', options: yesNo },
  { name: 'hasServiceBook', question: 'Har du en servicebok (eller digital servicehistorik)?', options: yesNo },
  { name: 'serviceHistory', question: 'Hur många servicetillfällen har utförts och dokumenterats?', options: toOptions(serviceHistoryLabels) },
  { name: 'inspectionStatus', question: 'Har bilen besiktats under de senaste 6 månaderna?', options: toOptions(inspectionLabels) },
  { name: 'sellerType', question: 'Typ av säljare?', options: toOptions(sellerTypeLabels) },
  { name: 'isImported', question: 'Importerad bil?', options: yesNo },
  { name: 'isFinanced', question: 'Är din bil för närvarande finansierad eller leasad?', options: yesNo },
  {
    name: 'interiorColor',
    question: 'Färg bilklädsel',
    options: toOptions(interiorColorLabels).map((o) => ({ ...o, color: interiorColorDots[o.value] })),
  },
  { name: 'interiorMaterial', question: 'Vilket material har bilklädseln?', options: toOptions(interiorMaterialLabels) },
]

const emptyForm = {
  registrationNumber: '',
  mileageMil: '',
  make: '',
  model: '',
  year: '',
  fuelType: '',
  gearbox: '',
  askingPrice: '',
  description: '',
  // '' = not answered yet. Gives { keyCount: '', hasExtraTires: '', ... }
  ...Object.fromEntries(choiceQuestions.map((q) => [q.name, ''])),
}

const currentYear = new Date().getFullYear()
const regNumberPattern = /^[A-Z]{3}\d{2}[A-Z0-9]$/

// One function per step. A step is complete when its function returns true.
const steps = [
  (f) => regNumberPattern.test(f.registrationNumber),
  (f) => f.mileageMil !== '' && Number(f.mileageMil) >= 0,
  (f) => f.make.trim() !== '' && f.model.trim() !== '',
  (f) => Number(f.year) >= 1950 && Number(f.year) <= currentYear + 1,
  (f) => f.fuelType !== '' && f.gearbox !== '',
  ...choiceQuestions.map((q) => (f) => f[q.name] !== ''),
  (f) => Number(f.askingPrice) > 0,
]

// The step number of the first button question and of the price.
const firstChoiceStep = 5
const priceStep = firstChoiceStep + choiceQuestions.length

// How many steps in a row (from the start) are complete. Decides which steps are shown.
function countStepsInOrder(form) {
  let count = 0
  for (const isComplete of steps) {
    if (!isComplete(form)) break
    count++
  }
  return count
}

// Shows a step with the gauge to the right of it (only on the latest shown step).
function StepRow({ showGauge, percent, children }) {
  return (
    <div className="step-row">
      <div className="step-field">{children}</div>
      <div className="step-side">{showGauge && <ProgressGauge percent={percent} />}</div>
    </div>
  )
}

export default function SellCarPage() {
  const navigate = useNavigate()
  const [form, setForm] = useState(emptyForm)
  // The highest number of completed steps so far. Steps that have been shown stay visible.
  const [reached, setReached] = useState(0)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  function handleChange(e) {
    const newForm = { ...form, [e.target.name]: e.target.value }
    setForm(newForm)
    setReached(Math.max(reached, countStepsInOrder(newForm)))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    setSaving(true)
    try {
      const { mileageMil, ...rest } = form
      const created = await createSubmission({
        ...rest,
        year: Number(form.year),
        // The customer writes Swedish "mil" (1 mil = 10 km), the API stores km.
        mileageKm: Number(mileageMil) * 10,
        askingPrice: Number(form.askingPrice),
      })
      navigate(`/forfragan/${created.id}`)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setSaving(false)
    }
  }

  // Progress = how many steps are complete right now.
  const completed = steps.filter((isComplete) => isComplete(form)).length
  const percent = Math.round((completed / steps.length) * 100)
  const allDone = reached === steps.length

  // The gauge is shown next to the last visible step.
  const lastVisible = allDone ? steps.length : reached
  const rowProps = (index) => ({ showGauge: index === lastVisible, percent })

  return (
    <section className="sell-page">
      <h1 className="sell-title">Sälj din bil – <strong>få ett bud</strong></h1>

      <form className="sell-form" onSubmit={handleSubmit}>
        <StepRow {...rowProps(0)}>
          <LicensePlateInput name="registrationNumber" value={form.registrationNumber} onChange={handleChange} />
        </StepRow>

        {reached >= 1 && (
          <StepRow {...rowProps(1)}>
            <label className="step-label">
              Hur många mil har din bil gått?
              <div className="input-suffix">
                <input name="mileageMil" type="number" min={0} max={200000} value={form.mileageMil}
                  onChange={handleChange} placeholder="Fyll i miltal" required />
                <span>mil</span>
              </div>
            </label>
          </StepRow>
        )}

        {reached >= 2 && (
          <StepRow {...rowProps(2)}>
            <div className="step-label">
              Vilket märke och vilken modell?
              <div className="row">
                <input name="make" value={form.make} onChange={handleChange}
                  placeholder="Märke, t.ex. Volvo" aria-label="Märke" required maxLength={50} />
                <input name="model" value={form.model} onChange={handleChange}
                  placeholder="Modell, t.ex. V70" aria-label="Modell" required maxLength={50} />
              </div>
            </div>
          </StepRow>
        )}

        {reached >= 3 && (
          <StepRow {...rowProps(3)}>
            <label className="step-label">
              Vilken årsmodell är bilen?
              <input name="year" type="number" min={1950} max={currentYear + 1} value={form.year}
                onChange={handleChange} placeholder={`t.ex. ${currentYear - 8}`} required />
            </label>
          </StepRow>
        )}

        {reached >= 4 && (
          <StepRow {...rowProps(4)}>
            <div className="step-label">
              Drivmedel och växellåda?
              <div className="row">
                <select name="fuelType" value={form.fuelType} onChange={handleChange} aria-label="Drivmedel" required>
                  <option value="" disabled>Välj drivmedel</option>
                  {Object.entries(fuelLabels).map(([value, label]) => (
                    <option key={value} value={value}>{label}</option>
                  ))}
                </select>
                <select name="gearbox" value={form.gearbox} onChange={handleChange} aria-label="Växellåda" required>
                  <option value="" disabled>Välj växellåda</option>
                  {Object.entries(gearboxLabels).map(([value, label]) => (
                    <option key={value} value={value}>{label}</option>
                  ))}
                </select>
              </div>
            </div>
          </StepRow>
        )}

        {/* The button questions. Question number i is step number firstChoiceStep + i. */}
        {choiceQuestions.map((q, i) => {
          const stepNumber = firstChoiceStep + i
          if (reached < stepNumber) return null
          return (
            <StepRow key={q.name} {...rowProps(stepNumber)}>
              <div className="step-label">
                {q.question}
                <ChoiceButtons name={q.name} value={form[q.name]} options={q.options} onChange={handleChange} />
              </div>
            </StepRow>
          )
        })}

        {reached >= priceStep && (
          <StepRow {...rowProps(priceStep)}>
            <label className="step-label">
              Vad vill du ha för bilen?
              <div className="input-suffix">
                <input name="askingPrice" type="number" min={1} value={form.askingPrice}
                  onChange={handleChange} placeholder="Önskat pris" required />
                <span>kr</span>
              </div>
            </label>
          </StepRow>
        )}

        {allDone && (
          <StepRow {...rowProps(steps.length)}>
            <label className="step-label">
              Något mer vi bör veta? <span className="muted">(valfritt)</span>
              <textarea name="description" rows={3} maxLength={1000} value={form.description}
                onChange={handleChange} placeholder="Skick, utrustning, skador..." />
            </label>
          </StepRow>
        )}

        {allDone && (
          <div className="step-row">
            <div className="step-field submit-field">
              {error && <p className="error">{error}</p>}
              <button type="submit" className="button big-button" disabled={saving}>
                {saving ? 'Skickar...' : 'Skicka in och få bud ›'}
              </button>
            </div>
            <div className="step-side" />
          </div>
        )}
      </form>
    </section>
  )
}
