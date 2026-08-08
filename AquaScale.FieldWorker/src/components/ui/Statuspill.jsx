import { LOT_STATUS } from "../../data/mockLots";
import "./StatusPill.css";

const STATUS_LABEL = {
  [LOT_STATUS.PENDING]: "Pending",
  [LOT_STATUS.RECAPTURE]: "Recapture",
  [LOT_STATUS.CAPTURED]: "Captured",
};

/** @param {"pending"|"recapture"|"captured"} status */
export default function StatusPill({ status }) {
  return (
    <span className={`status-pill status-pill--${status}`}>
      {STATUS_LABEL[status] ?? status}
    </span>
  );
}