import { useState } from 'react';
import { logout as apiLogout } from '../../api/auth';
import logoIcon from '../../assets/logo-icon.png';
import SidebarNavItem from './SidebarNavItem';
import './Sidebar.css';

const MANAGEMENT_ITEMS = [
  { id: 'dashboard', label: 'Dashboard', icon: 'fa-solid fa-table-columns' },
  { id: 'customers', label: 'Customers', icon: 'fa-solid fa-user' },
  { id: 'accounting', label: 'Accounting', icon: 'fa-solid fa-file-invoice-dollar' },
  { id: 'generate-qr', label: 'Generate QR', icon: 'fa-solid fa-qrcode' },
  { id: 'subdivisions', label: 'Subdivisions', icon: 'fa-solid fa-city', expandable: true },
];

const ADMINISTRATION_ITEMS = [
  { id: 'employees', label: 'Employees', icon: 'fa-solid fa-id-badge' },
  { id: 'roles-permissions', label: 'Roles and Permissions', icon: 'fa-solid fa-crown' },
  { id: 'audit-logs', label: 'Audit Logs', icon: 'fa-solid fa-clipboard-list' },
  { id: 'settings', label: 'Settings', icon: 'fa-solid fa-gear' },
];

function Sidebar({ user, activeItem = 'dashboard', onNavigate, onLoggedOut }) {
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  async function handleLogout() {
    setIsLoggingOut(true);
    try {
      await apiLogout();
    } catch {
      // Log out locally regardless of whether the server call succeeded.
    } finally {
      setIsLoggingOut(false);
      onLoggedOut?.();
    }
  }

  return (
    <aside className="sidebar">
      <div className="sidebar__brand">
        <div className="sidebar__logo-mark">
          <img src={logoIcon} alt="" />
        </div>
        <div>
          <p className="sidebar__brand-name">AquaScale</p>
          <p className="sidebar__brand-tagline">Utility Management</p>
        </div>
      </div>

      <nav className="sidebar__nav">
        <div className="sidebar__section">
          <p className="sidebar__section-label">Management</p>
          {MANAGEMENT_ITEMS.map((item) => (
            <SidebarNavItem
              key={item.id}
              icon={item.icon}
              label={item.label}
              expandable={item.expandable}
              active={activeItem === item.id}
              onClick={() => onNavigate?.(item.id)}
            />
          ))}
        </div>

        <div className="sidebar__divider" />

        <div className="sidebar__section">
          <p className="sidebar__section-label">Administration</p>
          {ADMINISTRATION_ITEMS.map((item) => (
            <SidebarNavItem
              key={item.id}
              icon={item.icon}
              label={item.label}
              active={activeItem === item.id}
              onClick={() => onNavigate?.(item.id)}
            />
          ))}
        </div>
      </nav>

      <div className="sidebar__user">
        <span className="sidebar__user-info">
          <span className="sidebar__user-name">{user?.fullName}</span>
          <span className="sidebar__user-role">{user?.roleName}</span>
        </span>
        <button
          type="button"
          className="sidebar__logout-btn"
          onClick={handleLogout}
          disabled={isLoggingOut}
          aria-label="Log out"
        >
          <i
            className={
              isLoggingOut ? 'fa-solid fa-spinner fa-spin' : 'fa-solid fa-right-from-bracket'
            }
            aria-hidden="true"
          />
        </button>
      </div>
    </aside>
  );
}

export default Sidebar;
