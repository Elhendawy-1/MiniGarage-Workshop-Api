import React from 'react';
import type { CarResponseDto } from '../types/car';

interface CarCardProps {
  car: CarResponseDto;
  onEdit: (car: CarResponseDto) => void;
  onDelete: (car: CarResponseDto) => void;
}

function getStatusClass(status: number): string {
  switch (status) {
    case 0:
      return 'status-pending';
    case 1:
      return 'status-inprogress';
    case 2:
      return 'status-completed';
    default:
      return 'status-pending';
  }
}

// Memoized to prevent unnecessary re-renders of unaffected cards (Vercel Best Practice: rerender-memo)
export const CarCard: React.FC<CarCardProps> = React.memo(({ car, onEdit, onDelete }) => {
  const statusClass = getStatusClass(car.repairStatus);
  const formattedDate = new Date(car.createdAt).toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });

  return (
    <article className="car-card" aria-labelledby={`car-title-${car.id}`}>
      <div>
        <div className="card-top">
          <div className="license-plate">
            {car.licensePlate}
          </div>
          <span className={`status-badge ${statusClass}`}>
            ● {car.repairStatusLabel}
          </span>
        </div>

        <h3 id={`car-title-${car.id}`} className="vehicle-model">
          {car.carModel}
        </h3>
        <div className="owner-name">👤 {car.ownerName}</div>
      </div>

      <div className="issue-box">
        <strong>Work Required:</strong>
        <br />
        {car.problemDescription}
      </div>

      <div className="card-footer">
        <span className="created-time" title="Registered date">
          Reg: {formattedDate}
        </span>
        <div className="card-actions">
          <button
            type="button"
            className="btn btn-secondary btn-sm"
            onClick={() => onEdit(car)}
            aria-label={`Edit ${car.carModel}`}
          >
            Edit
          </button>
          <button
            type="button"
            className="btn btn-danger btn-sm"
            onClick={() => onDelete(car)}
            aria-label={`Delete ${car.carModel}`}
          >
            Delete
          </button>
        </div>
      </div>
    </article>
  );
});

CarCard.displayName = 'CarCard';
