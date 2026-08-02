import { useState } from 'react'
import LoginPage from './pages/Login/LoginPage'

function App() {
  // Temporary placeholder for real routing/auth-context, which don't exist yet.
  // Just enough to prove the login screen works end-to-end.
  const [currentUser, setCurrentUser] = useState(null)

  if (!currentUser) {
    return <LoginPage onLoginSuccess={setCurrentUser} />
  }

  return (
    <div>
      <h1>Logged in as {currentUser.fullName}</h1>
      <p>Role: {currentUser.roleName}</p>
      {currentUser.mustChangePassword && <p>You must change your password.</p>}
    </div>
  )
}

export default App