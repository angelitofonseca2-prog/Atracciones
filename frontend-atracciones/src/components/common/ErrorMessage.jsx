function ErrorMessage({ mensaje }) {
  if (!mensaje) return null
  return (
    <div className="error-message" role="alert">
      <span>⚠</span>
      <span>{mensaje}</span>
    </div>
  )
}

export default ErrorMessage
