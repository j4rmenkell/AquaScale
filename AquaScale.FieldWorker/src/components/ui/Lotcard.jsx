import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDroplet, faBolt } from "@fortawesome/free-solid-svg-icons";
import { UTILITY, LOT_STATUS } from "../../data/mockLots";
import StatusPill from "./StatusPill";
import "./LotCard.css";

const UTILITY_ICON = {
  [UTILITY.WATER]: { icon: faDroplet, className: "lot-card__badge--water", label: "Water" },
  [UTILITY.ELECTRICITY]: { icon: faBolt, className: "lot-card__badge--electricity", label: "Electricity" },
};

function UtilityBadges({ utilities }) {
  return (
    <>
      {utilities.map((utility) => {
        const { icon, className, label } = UTILITY_ICON[utility];
        return (
          <span
            key={utility}
            className={`lot-card__badge ${className}`}
            aria-label={label}
            title={label}
          >
            <FontAwesomeIcon icon={icon} style={{ fontSize: 12 }} aria-hidden="true" />
          </span>
        );
      })}
    </>
  );
}

/**
 * @param {{ block: number, lotNumber: number, utilities: string[], status: string, capturedAt?: string }} lot
 * @param {() => void} onPress - called when a pending/recapture row is tapped, to open the meter-type picker
 */
export default function LotCard({ lot, onPress }) {
  const isCaptured = lot.status === LOT_STATUS.CAPTURED;

  // Captured lots are a record, not an action — render as a static row
  // with the capture timestamp instead of a tappable button.
  if (isCaptured) {
    return (
      <div className="lot-card lot-card--static">
        <span className="lot-card__label">
          Blk {lot.block}, Lot {lot.lotNumber}
        </span>
        <span className="lot-card__meta">
          <UtilityBadges utilities={lot.utilities} />
          <span className="lot-card__timestamp">{lot.capturedAt}</span>
        </span>
      </div>
    );
  }

  return (
    <button type="button" className="lot-card" onClick={onPress}>
      <span className="lot-card__label">
        Blk {lot.block}, Lot {lot.lotNumber}
      </span>
      <span className="lot-card__meta">
        <UtilityBadges utilities={lot.utilities} />
        <StatusPill status={lot.status} />
      </span>
    </button>
  );
}