import { useEffect, useState } from 'react'

let toastId = 0

export function emitirToast(message, type = 'info') {
  window.dispatchEvent(
    new CustomEvent('app:toast', { detail: { id: ++toastId, message, type } }),
  )
}

function Toast() {
  const [toasts, setToasts] = useState([])

  useEffect(() => {
    const handler = (event) => {
      const toast = event.detail
      setToasts((prev) => [...prev.slice(-2), toast])
      setTimeout(() => {
        setToasts((prev) => prev.filter((t) => t.id !== toast.id))
      }, 4000)
    }
    window.addEventListener('app:toast', handler)
    return () => window.removeEventListener('app:toast', handler)
  }, [])

  if (toasts.length === 0) return null

  return (
    <div className="toast-container">
      {toasts.map((t) => (
        <div key={t.id} className={`toast toast-${t.type}`}>
          {t.message}
          <button
            onClick={() => setToasts((prev) => prev.filter((x) => x.id !== t.id))}
            className="toast-close"
          >
            ×
          </button>
        </div>
      ))}
    </div>
  )
}

export default Toast
