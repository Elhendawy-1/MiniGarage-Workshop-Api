import React from 'react';
import type { CarResponseDto } from '../types/car';

interface KpiStripProps {
  cars: CarResponseDto[];
}

export const KpiStrip: React.FC<KpiStripProps> = ({ cars }) => {
  // Single-pass computation during render (Vercel Best Practice: rerender-derived-state-no-effect)
  const counts = React.useMemo(() => {
    let pending = 0;
    let inProgress = 0;
    let completed = 0;

    for (let i = 0; i < cars.length; i++) {
      const status = cars[i].repairStatus;
      if (status === 0) pending++;
      else if (status === 1) inProgress++;
      else if (status === 2) completed++;
    }

    return { total: cars.length, pending, inProgress, completed };
  }, [cars]);

  return (
    <section className="kpi-strip" aria-label="Workshop Overview Metrics">
      <div className="kpi-card kpi-total">
        <div className="kpi-icon" aria-hidden="true">🚗</div>
        <div className="kpi-details">
          <div className="kpi-value">{counts.total}</div>
          <div className="kpi-label">Total in Workshop</div>
        </div>
      </div>
      <div className="kpi-card kpi-pending">
        <div className="kpi-icon" aria-hidden="true">⏳</div>
        <div className="kpi-details">
          <div className="kpi-value">{counts.pending}</div>
          <div className="kpi-label">Pending Inspection</div>
        </div>
      </div>
      <div className="kpi-card kpi-progress">
        <div className="kpi-icon" aria-hidden="true">⚙️</div>
        <div className="kpi-details">
          <div className="kpi-value">{counts.inProgress}</div>
          <div className="kpi-label">In Active Repair</div>
        </div>
      </div>
      <div className="kpi-card kpi-completed">
        <div className="kpi-icon" aria-hidden="true">✅</div>
        <div className="kpi-details">
          <div className="kpi-value">{counts.completed}</div>
          <div className="kpi-label">Ready for Pickup</div>
        </div>
      </div>
    </section>
  );
};
