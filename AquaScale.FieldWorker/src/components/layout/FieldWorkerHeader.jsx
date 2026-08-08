import "./FieldWorkerHeader.css";

/**
 * Top bar for the Field Worker app: assigned subdivision + geofence status.
 *
 * @param {string} subdivision - e.g. "Tanza A"
 * @param {boolean} inBounds - whether the device is inside the assigned geofence
 */
export default function FieldWorkerHeader({ subdivision, inBounds }) {
  return (
    <header className="fw-header">
      <div className="fw-header__subdivision">
        <span className="fw-header__label">Assigned Subdivision</span>
        <span className="fw-header__value">{subdivision}</span>
      </div>

      <div
        className="fw-header__status"
        role="status"
        aria-live="polite"
      >
        <span
          className={`fw-header__dot ${inBounds ? "is-in" : "is-out"}`}
          aria-hidden="true"
        />
        <span className={inBounds ? "is-in" : "is-out"}>
          {inBounds ? "In Bounds" : "Out of Bounds"}
        </span>
      </div>
    </header>
  );
}