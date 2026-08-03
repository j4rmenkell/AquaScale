import heroBackground from '../../assets/login/hero-background.png';
import logoIcon from '../../assets/login/logo-icon.png';

function BrandPanel() {
  return (
    <div
      className="brand-panel"
      style={{ backgroundImage: `url(${heroBackground})` }}
    >
      <div className="brand-panel__overlay" />

      <div className="brand-panel__content">
        <div className="brand-panel__badge">
          <div className="brand-panel__logo-mark">
            <img src={logoIcon} alt="" />
          </div>
          <div>
            <p className="brand-panel__name">AquaScale</p>
            <p className="brand-panel__tagline">Utility Management System</p>
          </div>
        </div>

        <div className="brand-panel__copy">
          <h1 className="brand-panel__headline">
            Water and electricity management, built for every subdivision.
          </h1>
          <p className="brand-panel__description">
            GPS-geotagged meter reading, OCR-assisted verification, and payment
            tracking, all in one place for Charles Builder's subdivisions.
          </p>
        </div>
      </div>
    </div>
  );
}

export default BrandPanel;