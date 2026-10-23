import React, { useState } from 'react';
import type { CarResponseDto } from '../types/car';

interface DeleteModalProps {
  car: CarResponseDto | null;
  onClose: () => void;
  onConfirm: (id: number, adminKey: string) => Promise<void>;
}

export const DeleteModal: React.FC<DeleteModalProps> = ({
  car,
  onClose,
  onConfirm,
}) => {
  const [adminKey, setAdminKey] = useState('');
  const [isDeleting, setIsDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!car) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsDeleting(true);

    try {
      await onConfirm(car.id, adminKey.trim());
      setAdminKey('');
      onClose();
    } catch (err: any) {
      setError(err.message || 'Failed to delete vehicle.');
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div
      className="modal-backdrop open"
      role="dialog"
      aria-modal="true"
      aria-labelledby="deleteModalTitle"
    >
      <div className="modal-dialog">
        <div className="modal-header">
          <h2 id="deleteModalTitle" className="modal-title">
            Confirm Deletion
          </h2>
          <button
            type="button"
            className="btn-close"
            onClick={onClose}
            aria-label="Close modal"
          >
            &times;
          </button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="modal-body">
            <p style={{ marginBottom: '16px', color: 'var(--text-muted)' }}>
              Are you sure you want to delete{' '}
              <strong style={{ color: 'var(--text-main)' }}>
                {car.carModel} ({car.licensePlate})
              </strong>{' '}
              from workshop records? This operation is permanent.
            </p>

            {error ? (
              <div
                style={{
                  padding: '8px 12px',
                  backgroundColor: 'var(--status-danger-bg)',
                  color: 'var(--status-danger)',
                  borderRadius: '4px',
                  marginBottom: '16px',
                  fontSize: '0.85rem',
                }}
                role="alert"
              >
                {error}
              </div>
            ) : null}

            <div className="form-group">
              <label htmlFor="adminKeyInput" className="form-label">
                Admin API Key (X-Admin-Key) *
              </label>
              <input
                type="password"
                id="adminKeyInput"
                className="form-input"
                required
                placeholder="Enter workshop admin key..."
                value={adminKey}
                onChange={(e) => setAdminKey(e.target.value)}
              />
              <div className="form-hint">
                Default development key: <code>garage-admin-secret-2026</code>
              </div>
            </div>
          </div>

          <div className="modal-footer">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={onClose}
              disabled={isDeleting}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="btn btn-danger"
              disabled={isDeleting}
            >
              {isDeleting ? 'Deleting...' : 'Delete Vehicle'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
