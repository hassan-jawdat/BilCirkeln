// The API sends enum values in English ("Pending"). Here we translate them for the UI.

export const statusLabels = {
  Pending: 'Inskickad',
  OfferMade: 'Bud lämnat',
  Accepted: 'Accepterad',
  Declined: 'Bud nekat',
  Rejected: 'Avböjd av oss',
}

export const offerStatusLabels = {
  Open: 'Väntar på svar',
  Accepted: 'Accepterat',
  Declined: 'Nekat',
}

export const fuelLabels = {
  Petrol: 'Bensin',
  Diesel: 'Diesel',
  Hybrid: 'Hybrid',
  Electric: 'El',
}

export const gearboxLabels = {
  Manual: 'Manuell',
  Automatic: 'Automat',
}

export const yesNoLabels = {
  true: 'Ja',
  false: 'Nej',
}

export const keyCountLabels = {
  One: '1',
  TwoOrMore: '2+',
}

export const serviceHistoryLabels = {
  All: 'Alla',
  Some: 'Några',
  None: 'Inga',
}

export const inspectionLabels = {
  Recent: 'Ja',
  NotRecent: 'Nej',
  Expired: 'Har redan gått ut',
}

export const sellerTypeLabels = {
  Private: 'Privat eller företagsbil utan moms',
  CompanyWithVat: 'Företagsbil med moms',
}

export const interiorColorLabels = {
  Black: 'Svart',
  Grey: 'Grå',
  Beige: 'Beige',
  Blue: 'Blå',
  White: 'Vit',
  Other: 'Annan',
}

// The dot color shown on the button for each interior color.
export const interiorColorDots = {
  Black: '#111',
  Grey: '#a3a3a3',
  Beige: '#dcc39b',
  Blue: '#2f4fb3',
  White: '#fff',
}

export const interiorMaterialLabels = {
  Fabric: 'Tyg',
  Leather: 'Läder',
  PartLeather: 'Delläder',
  Alcantara: 'Alcantara',
  Plastic: 'Plast',
  Other: 'Annan',
}

// Turns { Black: 'Svart', ... } into [{ value: 'Black', label: 'Svart' }, ...] for ChoiceButtons.
export function toOptions(labels) {
  return Object.entries(labels).map(([value, label]) => ({ value, label }))
}

// Shows the label for a value, or "–" if the submission has no answer (older submissions).
export function labelOf(labels, value) {
  return value === null || value === undefined ? '–' : labels[value]
}

export function formatPrice(amount) {
  return amount.toLocaleString('sv-SE') + ' kr'
}

// The API stores km. In Sweden we talk about "mil" (1 mil = 10 km).
export function formatMileage(km) {
  return Math.round(km / 10).toLocaleString('sv-SE') + ' mil'
}

export function formatDate(iso) {
  return new Date(iso).toLocaleDateString('sv-SE')
}
