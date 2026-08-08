import { useEffect } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDroplet, faBolt } from "@fortawesome/free-solid-svg-icons";
import { UTILITY } from "../../data/mockLots";
import "./MeterTypeModal.css";

const METER_OPTION = {
  [UTILITY.WATER]: { icon: faDroplet, label: "Water", className: "meter-type-modal__option--water" },
  [UTILITY.ELECTRICITY]: {
    icon: faBolt,
    label: "Electricity",
    className: "meter-type-modal__option--electricity",
  },
};

/**
 * @param {object|null} lot - the lot being captured; modal is hidden when null
 * @param {(lot: object, utility: string) => void} onSelectMeterType
 * @param {() => void} onClose
 */
export default function MeterTypeModal({ lot, onSelectMeterType, onClose }) {
  useEffect(() => {
    if (!lot) return;
    function handleKeyDown(event) {
      if (event.key === "Escape") onClose();
    }
    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [lot, onClose]);

  if (!lot) return null;

  return (
    <div
      className="meter-type-modal__overlay"
      onClick={onClose}
      role="presentation"
    >
      <div
        className="meter-type-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="meter-type-modal-title"
        onClick={(event) => event.stopPropagation()}
      >
        <h2 id="meter-type-modal-title" className="meter-type-modal__title">
          Select Meter Type
        </h2>

        <div className="meter-type-modal__options">
          {lot.utilities.map((utility) => {
            const { icon, label, className } = METER_OPTION[utility];
            return (
              <button
                key={utility}
                type="button"
                className={`meter-type-modal__option ${className}`}
                onClick={() => onSelectMeterType(lot, utility)}
              >
                <FontAwesomeIcon icon={icon} style={{ fontSize: 40 }} aria-hidden="true" />
                <span>{label}</span>
              </button>
            );
          })}
        </div>

        <button type="button" className="meter-type-modal__close" onClick={onClose}>
          Close
        </button>
      </div>
    </div>
  );
}