// Temporary mock data for the Pending / Captured Lots screen.
// Swap fetchLots() for a real API call once the endpoint is ready —
// keep the same shape so the components don't need to change.

export const UTILITY = {
  WATER: "water",
  ELECTRICITY: "electricity",
};

export const LOT_STATUS = {
  PENDING: "pending",
  RECAPTURE: "recapture",
  CAPTURED: "captured",
};

let idCounter = 1;
const lot = (block, lotNumber, utilities, status, capturedAt = null) => ({
  id: idCounter++,
  block,
  lotNumber,
  utilities,
  status,
  capturedAt, // e.g. "07-09-2026 09:15" — set only for captured lots
});

export const mockLots = [
  lot(1, 43, [UTILITY.WATER], LOT_STATUS.PENDING),
  lot(1, 44, [UTILITY.WATER], LOT_STATUS.PENDING),
  lot(1, 45, [UTILITY.WATER, UTILITY.ELECTRICITY], LOT_STATUS.PENDING),
  lot(1, 46, [UTILITY.WATER], LOT_STATUS.PENDING),
  lot(1, 47, [UTILITY.WATER, UTILITY.ELECTRICITY], LOT_STATUS.RECAPTURE),
  lot(1, 48, [UTILITY.WATER], LOT_STATUS.PENDING),
  lot(2, 1, [UTILITY.WATER, UTILITY.ELECTRICITY], LOT_STATUS.PENDING),
  lot(2, 2, [UTILITY.WATER, UTILITY.ELECTRICITY], LOT_STATUS.PENDING),
  lot(2, 3, [UTILITY.WATER], LOT_STATUS.PENDING),
  lot(2, 3, [UTILITY.WATER], LOT_STATUS.RECAPTURE),
  lot(2, 4, [UTILITY.WATER], LOT_STATUS.PENDING),
  lot(2, 5, [UTILITY.WATER], LOT_STATUS.PENDING),
  lot(2, 6, [UTILITY.WATER], LOT_STATUS.PENDING),
  lot(2, 7, [UTILITY.WATER], LOT_STATUS.PENDING),
  lot(1, 1, [UTILITY.WATER], LOT_STATUS.CAPTURED, "07-09-2026 09:15"),
  lot(1, 2, [UTILITY.WATER], LOT_STATUS.CAPTURED, "07-09-2026 09:15"),
  lot(1, 3, [UTILITY.WATER, UTILITY.ELECTRICITY], LOT_STATUS.CAPTURED, "07-09-2026 09:20"),
  lot(1, 4, [UTILITY.WATER], LOT_STATUS.CAPTURED, "07-09-2026 09:27"),
  lot(1, 5, [UTILITY.WATER], LOT_STATUS.CAPTURED, "07-09-2026 09:31"),
];

// Simulates an async fetch so swapping in a real API later is a one-line change.
export function fetchLots() {
  return Promise.resolve(mockLots);
}