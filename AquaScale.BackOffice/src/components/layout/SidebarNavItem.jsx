function SidebarNavItem({ icon, label, active = false, expandable = false, onClick }) {
  return (
    <button
      type="button"
      className={`sidebar-nav-item${active ? ' sidebar-nav-item--active' : ''}`}
      onClick={onClick}
    >
      <i className={`sidebar-nav-item__icon ${icon}`} aria-hidden="true" />
      <span className="sidebar-nav-item__label">{label}</span>
      {expandable && (
        <i className="fa-solid fa-chevron-down sidebar-nav-item__chevron" aria-hidden="true" />
      )}
    </button>
  );
}

export default SidebarNavItem;