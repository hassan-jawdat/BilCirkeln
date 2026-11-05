// A group of buttons where you pick one answer, e.g. "Ja" / "Nej".
// options: [{ value, label, color? }]. color shows a small colored dot.
export default function ChoiceButtons({ name, value, options, onChange }) {
  return (
    <div className={`choices ${options.length === 2 ? 'choices-2' : 'choices-3'}`}>
      {options.map((option) => (
        <button
          key={String(option.value)}
          // type="button" so clicking does not submit the form.
          type="button"
          className={`choice ${value === option.value ? 'selected' : ''}`}
          aria-pressed={value === option.value}
          // Same shape as an input event, so the page can use its normal handleChange.
          onClick={() => onChange({ target: { name, value: option.value } })}
        >
          {option.color && <span className="color-dot" style={{ background: option.color }} />}
          {option.label}
        </button>
      ))}
    </div>
  )
}
