import React from 'react';

interface ToolbarProps {
  searchQuery: string;
  onSearchChange: (q: string) => void;
  currentFilter: string;
  onFilterChange: (filter: string) => void;
}

export const Toolbar: React.FC<ToolbarProps> = ({
  searchQuery,
  onSearchChange,
  currentFilter,
  onFilterChange,
}) => {
  return (
    <section className="toolbar" aria-label="Vehicle Filters">
      <div className="search-box">
        <span className="search-icon" aria-hidden="true">🔍</span>
        <input
          type="text"
          className="search-input"
          placeholder="Search by license plate, owner, or car model..."
          value={searchQuery}
          onChange={(e) => onSearchChange(e.target.value)}
          aria-label="Search vehicles"
        />
      </div>

      <div className="filter-tabs" role="tablist" aria-label="Filter by repair status">
        <button
          type="button"
          className={`tab-btn ${currentFilter === 'all' ? 'active' : ''}`}
          onClick={() => onFilterChange('all')}
          role="tab"
          aria-selected={currentFilter === 'all'}
        >
          All Vehicles
        </button>
        <button
          type="button"
          className={`tab-btn ${currentFilter === '0' ? 'active' : ''}`}
          onClick={() => onFilterChange('0')}
          role="tab"
          aria-selected={currentFilter === '0'}
        >
          Pending
        </button>
        <button
          type="button"
          className={`tab-btn ${currentFilter === '1' ? 'active' : ''}`}
          onClick={() => onFilterChange('1')}
          role="tab"
          aria-selected={currentFilter === '1'}
        >
          In Progress
        </button>
        <button
          type="button"
          className={`tab-btn ${currentFilter === '2' ? 'active' : ''}`}
          onClick={() => onFilterChange('2')}
          role="tab"
          aria-selected={currentFilter === '2'}
        >
          Completed
        </button>
      </div>
    </section>
  );
};
