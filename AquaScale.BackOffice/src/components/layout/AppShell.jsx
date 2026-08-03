import Sidebar from './Sidebar';
import TopBar from './TopBar';
import './AppShell.css';

// activeSection: which sidebar item is highlighted (e.g. "dashboard")
// tabs/activeTab: the top bar's sub-navigation for that section
function AppShell({
  user,
  activeSection,
  onNavigateSection,
  tabs,
  activeTab,
  onNavigateTab,
  onLoggedOut,
  children,
}) {
  return (
    <div className="app-shell">
      <Sidebar
        user={user}
        activeItem={activeSection}
        onNavigate={onNavigateSection}
        onLoggedOut={onLoggedOut}
      />
      <div className="app-shell__main">
        <TopBar tabs={tabs} activeTab={activeTab} onNavigateTab={onNavigateTab} />
        <div className="app-shell__content">{children}</div>
      </div>
    </div>
  );
}

export default AppShell;