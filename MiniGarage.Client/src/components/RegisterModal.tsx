import React, { useState } from 'react';
import type { CreateCarDto, RepairStatus } from '../types/car';

interface RegisterModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (dto: CreateCarDto) => Promise<void>;
}

export const RegisterModal: React.FC<RegisterModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
}) => {
  const [ownerName, setOwnerName] = useState('');
  const [carModel, setCarModel] = useState('');
  const [licensePlate, setLicensePlate] = useState('');
  const [problemDescription, setProblemDescription] = useState('');
  const [repairStatus, setRepairStatus] = useState<RepairStatus>(0);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await onSubmit({
        ownerName: ownerName.trim(),
        carModel: carModel.trim(),
        licensePlate: licensePlate.trim().toUpperCase(),
        problemDescription: problemDescription.trim(),
        repairStatus,
      });

      // Reset form
      setOwnerName('');
      setCarModel('');
      setLicensePlate('');
      setProblemDescription('');
      setRepairStatus(0);
      onClose();
    } catch (err: any) {
      setError(err.message || 'Failed to register vehicle.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div
      className="modal-backdrop open"
      role="dialog"
      aria-modal="true"
      aria-labelledby="registerModalTitle"
    >
      <div className="modal-dialog">
        <div className="modal-header">
          <h2 id="registerModalTitle" className="modal-title">
            Register New Vehicle
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
              <label htmlFor="regOwner" className="form-label">
                Owner Full Name *
              </label>
              <input
                type="text"
                id="regOwner"
                className="form-input"
                required
                minLength={2}
                maxLength={100}
                placeholder="e.g. Faisal Al-Ghamdi"
                value={ownerName}
                onChange={(e) => setOwnerName(e.target.value)}
              />
            </div>

            <div className="form-group">
              <label htmlFor="regModel" className="form-label">
                Car Model & Make *
              </label>
              <input
                type="text"
                id="regModel"
                className="form-input"
                required
                minLength={2}
                maxLength={100}
                placeholder="e.g. Toyota Camry 2023"
                value={carModel}
                onChange={(e) => setCarModel(e.target.value)}
              />
            </div>

            <div className="form-group">
              <label htmlFor="regPlate" className="form-label">
                License Plate Number *
              </label>
              <input
                type="text"
                id="regPlate"
                className="form-input"
                required
                pattern="^[a-zA-Z0-9\s\-]+$"
                maxLength={20}
                placeholder="e.g. KSA-1020"
                value={licensePlate}
                onChange={(e) => setLicensePlate(e.target.value)}
              />
              <div className="form-hint">Alphanumeric, spaces, and hyphens only.</div>
            </div>

            <div className="form-group">
              <label htmlFor="regProblem" className="form-label">
                Problem Description *
              </label>
              <textarea
                id="regProblem"
                className="form-textarea"
                rows={3}
                required
                minLength={5}
                maxLength={500}
                placeholder="Describe vehicle symptoms or required maintenance..."
                value={problemDescription}
                onChange={(e) => setProblemDescription(e.target.value)}
              />
            </div>

            <div className="form-group">
              <label htmlFor="regStatus" className="form-label">
                Initial Workshop Status
              </label>
              <select
                id="regStatus"
                className="form-select"
                value={repairStatus}
                onChange={(e) => setRepairStatus(Number(e.target.value) as RepairStatus)}
              >
                <option value={0}>Pending (Waiting for Inspection)</option>
                <option value={1}>In Progress (Currently on Lift)</option>
                <option value={2}>Completed (Ready for Pickup)</option>
              </select>
            </div>
          </div>

          <div className="modal-footer">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={onClose}
              disabled={isSubmitting}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="btn btn-primary"
              disabled={isSubmitting}
            >
              {isSubmitting ? 'Registering...' : 'Save & Register'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
