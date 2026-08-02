// Generic labeled input: icon + input + optional trailing button (e.g. the
// password show/hide toggle). Kept dumb/controlled — all state lives in the parent.
function FormField({
  id,
  label,
  type = 'text',
  icon,
  value,
  onChange,
  placeholder,
  autoComplete,
  disabled,
  trailingAction,
}) {
  return (
    <div className="form-field">
      <label htmlFor={id} className="form-field__label">
        {label}
      </label>
      <div className="form-field__control">
        {icon && <i className={`form-field__icon ${icon}`} aria-hidden="true" />}
        <input
          id={id}
          name={id}
          type={type}
          value={value}
          onChange={onChange}
          placeholder={placeholder}
          autoComplete={autoComplete}
          disabled={disabled}
          required
          className="form-field__input"
        />
        {trailingAction}
      </div>
    </div>
  );
}

export default FormField;