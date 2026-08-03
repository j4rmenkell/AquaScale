import './TopBar.css';

// tabs: [{ id, label }] — the sub-tabs for whichever sidebar section is
// active. Right now Dashboard only has one tab ("Dashboard" itself); other
// sections will pass their own list once they exist.
//
// rightSlot: whatever the current page wants shown on the right side of the
// bar — a date pill for Dashboard, maybe a search box or filter for other
// pages later, or nothing at all. TopBar stays generic and doesn't assume.
function TopBar({ tabs = [], activeTab, onNavigateTab, rightSlot }) {
  return (
    <header className="top-bar">
      <nav className="top-bar__tabs">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            type="button"
            className={`top-bar__tab${activeTab === tab.id ? ' top-bar__tab--active' : ''}`}
            onClick={() => onNavigateTab?.(tab.id)}
          >
            {tab.label}
          </button>
        ))}
      </nav>
      {rightSlot && <div className="top-bar__right">{rightSlot}</div>}
    </header>
  );
}

export default TopBar;