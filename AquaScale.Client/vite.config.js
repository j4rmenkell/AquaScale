import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Backend's dev HTTPS profile (see AquaScale.Api/Properties/launchSettings.json).
      // Proxying means the browser sees same-origin requests, so the aquascale_session
      // cookie works in dev without needing CORS configured on the API yet.
      '/api': {
        target: 'https://localhost:7140',
        changeOrigin: true,
        secure: false, // dev cert is self-signed
      },
    },
  },
})