import React from 'react';

interface HeaderProps {
  onOpenRegister: () => void;
}

export const Header: React.FC<HeaderProps> = ({ onOpenRegister }) => {
  return (
    <header className="app-header">
      <div className="container header-content">
        <a href="#" className="brand">
          <div className="brand-icon" aria-hidden="true">🔧</div>
          <div className="brand-info">
            <h1>Mini Garage <span>Workshop</span></h1>
            <p>ASP.NET Core 9 Web API — React Client</p>
          </div>
        </a>

        <div className="header-actions">
          <div className="badge-protocol" role="status" aria-label="Connection Status">
            <span className="pulse-dot" aria-hidden="true"></span>
            <span>HTTP/3 Ready</span>
          </div>
          <a
            href="/swagger"
            target="_blank"
            rel="noreferrer"
            className="btn btn-secondary btn-sm"
            title="OpenAPI Specification"
          >
            Swagger Docs ↗
          </a>
          <button
            type="button"
            className="btn btn-primary"
            onClick={onOpenRegister}
          >
            + Register Vehicle
          </button>
        </div>
      </div>
    </header>
  );
};
