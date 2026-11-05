// An input that looks like a Swedish license plate (blue EU band with "S").
export default function LicensePlateInput({ value, onChange, name }) {
  function handleChange(e) {
    // Always show capital letters and remove spaces: "abc 123" -> "ABC123".
    const cleaned = e.target.value.toUpperCase().replace(/\s/g, '')
    onChange({ target: { name, value: cleaned } })
  }

  return (
    <div className="plate">
      <div className="plate-eu">
        <span className="plate-stars">★</span>
        <span>S</span>
      </div>
      <input
        className="plate-input"
        name={name}
        value={value}
        onChange={handleChange}
        placeholder="ABC123"
        maxLength={6}
        required
        pattern="[A-Z]{3}[0-9]{2}[A-Z0-9]"
        title="Tre bokstäver, två siffror och en bokstav eller siffra, t.ex. ABC123"
        aria-label="Registreringsnummer"
        autoComplete="off"
      />
    </div>
  )
}
