import AppShell from '../../components/layout/AppShell';
import './Dashboard.css';

const DASHBOARD_TABS = [{ id: 'dashboard', label: 'Dashboard' }];

function DashboardPage({ user, onLoggedOut }) {
  const firstName = user?.fullName?.split(' ')[0] ?? '';

  return (
    <AppShell
      user={user}
      activeSection="dashboard"
      tabs={DASHBOARD_TABS}
      activeTab="dashboard"
      onLoggedOut={onLoggedOut}
    >
      <h1>Good Morning, {firstName}</h1>
      <p>Dashboard content coming soon.</p>
    </AppShell>
  );
}

export default DashboardPage;
