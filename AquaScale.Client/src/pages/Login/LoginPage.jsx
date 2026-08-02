import BrandPanel from './BrandPanel';
import LoginForm from './LoginForm';
import './Login.css';

function LoginPage({ onLoginSuccess }) {
  return (
    <div className="login-page">
      <BrandPanel />
      <div className="login-page__panel">
        <LoginForm onLoginSuccess={onLoginSuccess} />
      </div>
    </div>
  );
}

export default LoginPage;