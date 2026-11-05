// A half circle that fills up from 0 to 100 percent.
export default function ProgressGauge({ percent }) {
  return (
    <div className="gauge">
      <svg viewBox="0 0 100 56" width="110" aria-hidden="true">
        {/* Grey background arc */}
        <path d="M 10 50 A 40 40 0 0 1 90 50" className="gauge-track" pathLength="100" />
        {/* Green arc. pathLength="100" lets us draw exactly "percent" of the arc. */}
        <path
          d="M 10 50 A 40 40 0 0 1 90 50"
          className="gauge-fill"
          pathLength="100"
          strokeDasharray={`${percent} 100`}
        />
      </svg>
      <div className="gauge-text">
        <strong>{percent}%</strong>
        <span>slutförd</span>
      </div>
    </div>
  )
}
