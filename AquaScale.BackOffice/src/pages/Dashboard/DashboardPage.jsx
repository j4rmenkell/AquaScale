import AppShell from '../../components/layout/AppShell';
import './Dashboard.css';

// Only one tab exists so far — more get added here once other sections
// (Customers, Accounting, etc.) have sub-tabs of their own.
const DASHBOARD_TABS = [{ id: 'dashboard', label: 'Dashboard' }];

function formatToday() {
  return new Date().toLocaleDateString('en-US', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

function DashboardPage({ user, onLoggedOut }) {
  const firstName = user?.fullName?.split(' ')[0] ?? '';

  return (
    <AppShell
      user={user}
      activeSection="dashboard"
      tabs={DASHBOARD_TABS}
      activeTab="dashboard"
      topBarRight={<div className="dashboard-date-pill">{formatToday()}</div>}
      onLoggedOut={onLoggedOut}
    >
      <h1>Good Morning, {firstName}</h1>
      <p>Dashboard content coming soon.</p>
    </AppShell>
  );
}

export default DashboardPage;