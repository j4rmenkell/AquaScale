import "./ScreenFooter.css";

/**
 * Sticky bottom bar for the Field Worker screen.
 * @param {string} message
 */
export default function ScreenFooter({ message }) {
  return (
    <footer className="screen-footer">
      <p>{message}</p>
    </footer>
  );
}