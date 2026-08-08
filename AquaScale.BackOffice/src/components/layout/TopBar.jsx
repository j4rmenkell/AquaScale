import './TopBar.css';

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
