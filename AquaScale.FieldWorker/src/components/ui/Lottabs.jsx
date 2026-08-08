import "./LotTabs.css";

/**
 * Segmented control switching between Pending and Captured lots.
 *
 * @param {"pending"|"captured"} activeTab
 * @param {(tab: "pending"|"captured") => void} onChange
 * @param {number} pendingCount
 * @param {number} capturedCount
 */
export default function LotTabs({ activeTab, onChange, pendingCount, capturedCount }) {
  return (
    <div className="lot-tabs" role="tablist" aria-label="Lot lists">
      <button
        type="button"
        role="tab"
        aria-selected={activeTab === "pending"}
        className={`lot-tabs__tab ${activeTab === "pending" ? "is-active" : ""}`}
        onClick={() => onChange("pending")}
      >
        Pending Lots ({pendingCount})
      </button>
      <button
        type="button"
        role="tab"
        aria-selected={activeTab === "captured"}
        className={`lot-tabs__tab ${activeTab === "captured" ? "is-active" : ""}`}
        onClick={() => onChange("captured")}
      >
        Captured Lots ({capturedCount})
      </button>
    </div>
  );
}