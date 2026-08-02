import { useState } from 'react';
import { login } from '../../api/auth';
import FormField from '@aquascale/shared/ui/FormField';

// onLoginSuccess receives the LoginResponse payload:
// { id, fullName, email, roleName, mustChangePassword }
function LoginForm({ onLoginSuccess }) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState(null);

    const isFormIncomplete = !email.trim() || !password;
    
  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);

    if (!email.trim() || !password) {
      setError('Email and password are required.');
      return;
    }

    setIsSubmitting(true);
    try {
      // NOTE: rememberMe isn't sent to the API yet — the backend cookie is always
      // persistent/7-day (see AuthController). Wire this through once there's a
      // session-vs-persistent distinction server-side.
      const profile = await login(email.trim(), password);
      onLoginSuccess?.(profile);
    } catch (err) {
      setError(err.message || 'Something went wrong. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="login-form">
      <h2 className="login-form__title">Login</h2>
      <p className="login-form__subtitle">Sign in to manage subdivisions.</p>

      <form onSubmit={handleSubmit} noValidate>
        <FormField
          id="email"
          label="Email"
          type="email"
          icon="fa-regular fa-envelope"
          placeholder="employee@gmail.com"
          autoComplete="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          disabled={isSubmitting}
        />

        <FormField
          id="password"
          label="Password"
          type={showPassword ? 'text' : 'password'}
          icon="fa-solid fa-lock"
          placeholder="Enter Password"
          autoComplete="current-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          disabled={isSubmitting}
          trailingAction={
            <button
              type="button"
              className="form-field__toggle"
              onClick={() => setShowPassword((v) => !v)}
              disabled={isSubmitting}
              aria-label={showPassword ? 'Hide password' : 'Show password'}
            >
              <i className={showPassword ? 'fa-solid fa-eye-slash' : 'fa-solid fa-eye'} />
            </button>
          }
        />

        <div className="login-form__row">
          <label className="login-form__remember">
            <input
              type="checkbox"
              checked={rememberMe}
              onChange={(e) => setRememberMe(e.target.checked)}
              disabled={isSubmitting}
            />
            Remember me
          </label>
          <a href="#" className="login-form__forgot">
            Forgot Password?
          </a>
        </div>

        {error && (
          <p className="login-form__error" role="alert">
            {error}
          </p>
        )}

        <button 
          type="submit" 
          className="login-form__submit"
          disabled={isSubmitting || isFormIncomplete} 
        >
          <i className="fa-solid fa-right-to-bracket" />
          {isSubmitting ? 'Logging in…' : 'Log in'}
        </button>
      </form>
    </div>
  );
}

export default LoginForm;