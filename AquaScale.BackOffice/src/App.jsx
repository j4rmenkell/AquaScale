import { useState } from 'react'
import LoginPage from './pages/Login/LoginPage'
import DashboardPage from './pages/Dashboard/DashboardPage'

function App() {
  const [currentUser, setCurrentUser] = useState(null)

  if (!currentUser) {
    return <LoginPage onLoginSuccess={setCurrentUser} />
  }

  return <DashboardPage user={currentUser} onLoggedOut={() => setCurrentUser(null)} />
}

export default App
