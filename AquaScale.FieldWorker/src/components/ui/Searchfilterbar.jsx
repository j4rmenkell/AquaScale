import { useEffect, useRef, useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faMagnifyingGlass,
  faLayerGroup,
  faChevronDown,
  faCheck,
} from "@fortawesome/free-solid-svg-icons";
import "./SearchFilterBar.css";

const FILTER_OPTIONS = [
  { value: "all", label: "All" },
  { value: "water", label: "Water" },
  { value: "electricity", label: "Electricity" },
  { value: "water+electricity", label: "Water & Electricity" },
];

/**
 * @param {string} searchValue
 * @param {(value: string) => void} onSearchChange
 * @param {string} filterValue - one of FILTER_OPTIONS values
 * @param {(value: string) => void} onFilterChange
 */
export default function SearchFilterBar({
  searchValue,
  onSearchChange,
  filterValue,
  onFilterChange,
}) {
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef(null);

  useEffect(() => {
    function handleClickOutside(event) {
      if (containerRef.current && !containerRef.current.contains(event.target)) {
        setIsOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const activeLabel =
    FILTER_OPTIONS.find((option) => option.value === filterValue)?.label ?? "All";

  return (
    <div className="search-filter-bar">
      <label className="search-filter-bar__search">
        <FontAwesomeIcon
          icon={faMagnifyingGlass}
          className="search-filter-bar__search-icon"
          style={{ fontSize: 15 }}
          aria-hidden="true"
        />
        <input
          type="text"
          inputMode="search"
          placeholder="Search Block..."
          value={searchValue}
          onChange={(event) => onSearchChange(event.target.value)}
          aria-label="Search by block"
        />
      </label>

      <div className="search-filter-bar__filter" ref={containerRef}>
        <button
          type="button"
          className="search-filter-bar__filter-trigger"
          onClick={() => setIsOpen((open) => !open)}
          aria-haspopup="listbox"
          aria-expanded={isOpen}
        >
          <FontAwesomeIcon icon={faLayerGroup} style={{ fontSize: 13 }} aria-hidden="true" />
          <span>{activeLabel}</span>
          <FontAwesomeIcon icon={faChevronDown} style={{ fontSize: 13 }} aria-hidden="true" />
        </button>

        {isOpen && (
          <ul className="search-filter-bar__menu" role="listbox">
            {FILTER_OPTIONS.map((option) => (
              <li key={option.value}>
                <button
                  type="button"
                  role="option"
                  aria-selected={filterValue === option.value}
                  className="search-filter-bar__menu-item"
                  onClick={() => {
                    onFilterChange(option.value);
                    setIsOpen(false);
                  }}
                >
                  <span>{option.label}</span>
                  {filterValue === option.value && (
                    <FontAwesomeIcon icon={faCheck} style={{ fontSize: 13 }} aria-hidden="true" />
                  )}
                </button>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}