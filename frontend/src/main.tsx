import '@fontsource/inter/400.css'
import '@fontsource/inter/500.css'
import '@fontsource/inter/600.css'
import '@fontsource/inter/700.css'
import '@fontsource/source-serif-4/400.css'
import '@fontsource/source-serif-4/400-italic.css'
import '@fontsource/source-serif-4/600.css'
import '@fontsource/source-serif-4/600-italic.css'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import './index.css'
import { refresh } from './api/auth.ts'
import App from './App.tsx'
import { CartProvider } from './context/CartContext.tsx'
import { ToastProvider } from './context/ToastContext.tsx'
import { setAccessToken } from './lib/auth.ts'

// Restore the session from the HttpOnly refresh cookie before the first render,
// so components checking getAccessToken() on mount (e.g. Header, CartProvider) see it.
try {
  const tokens = await refresh()
  setAccessToken(tokens.accessToken)
} catch {
  // no valid refresh cookie - user is not logged in
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <ToastProvider>
        <CartProvider>
          <App />
        </CartProvider>
      </ToastProvider>
    </BrowserRouter>
  </StrictMode>,
)
