function Spinner({ message = 'Cargando...' }) {
  return (
    <div className="spinner-wrap" role="status" aria-live="polite">
      <span className="spinner" />
      <p>{message}</p>
    </div>
  )
}

export default Spinner
