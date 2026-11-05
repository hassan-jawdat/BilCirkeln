import { statusLabels } from '../labels'

export default function StatusBadge({ status }) {
  return <span className={`badge badge-${status}`}>{statusLabels[status] ?? status}</span>
}
