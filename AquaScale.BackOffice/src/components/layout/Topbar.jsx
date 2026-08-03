import './TopBar.css';

// tabs: [{ id, label }] — the sub-tabs for whichever sidebar section is
// active. Right now Dashboard only has one tab ("Dashboard" itself); other
// sections will pass their own list once they exist.
function TopBar({ tabs = [], activeTab, onNavigateTab }) {
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
    </header>
  );
}

export default TopBar;