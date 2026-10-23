import React, { useState, useEffect } from 'react';
import type { CarResponseDto, UpdateCarDto, RepairStatus } from '../types/car';

interface EditModalProps {
  car: CarResponseDto | null;
  onClose: () => void;
  onSubmit: (id: number, dto: UpdateCarDto) => Promise<void>;
}

export const EditModal: React.FC<EditModalProps> = ({
  car,
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

  useEffect(() => {
    if (car) {
      setOwnerName(car.ownerName);
      setCarModel(car.carModel);
      setLicensePlate(car.licensePlate);
      setProblemDescription(car.problemDescription);
      setRepairStatus(car.repairStatus);
      setError(null);
    }
  }, [car]);

  if (!car) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await onSubmit(car.id, {
        ownerName: ownerName.trim(),
        carModel: carModel.trim(),
        licensePlate: licensePlate.trim().toUpperCase(),
        problemDescription: problemDescription.trim(),
        repairStatus,
      });
      onClose();
    } catch (err: any) {
      setError(err.message || 'Failed to update vehicle.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div
      className="modal-backdrop open"
      role="dialog"
      aria-modal="true"
      aria-labelledby="editModalTitle"
    >
      <div className="modal-dialog">
        <div className="modal-header">
          <h2 id="editModalTitle" className="modal-title">
            Update Vehicle Status
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
              <label htmlFor="editOwner" className="form-label">
                Owner Full Name *
              </label>
              <input
                type="text"
                id="editOwner"
                className="form-input"
                required
                minLength={2}
                maxLength={100}
                value={ownerName}
                onChange={(e) => setOwnerName(e.target.value)}
              />
            </div>

            <div className="form-group">
              <label htmlFor="editModel" className="form-label">
                Car Model & Make *
              </label>
              <input
                type="text"
                id="editModel"
                className="form-input"
                required
                minLength={2}
                maxLength={100}
                value={carModel}
                onChange={(e) => setCarModel(e.target.value)}
              />
            </div>

            <div className="form-group">
              <label htmlFor="editPlate" className="form-label">
                License Plate Number *
              </label>
              <input
                type="text"
                id="editPlate"
                className="form-input"
                required
                pattern="^[a-zA-Z0-9\s\-]+$"
                maxLength={20}
                value={licensePlate}
                onChange={(e) => setLicensePlate(e.target.value)}
              />
            </div>

            <div className="form-group">
              <label htmlFor="editProblem" className="form-label">
                Work & Problem Description *
              </label>
              <textarea
                id="editProblem"
                className="form-textarea"
                rows={3}
                required
                minLength={5}
                maxLength={500}
                value={problemDescription}
                onChange={(e) => setProblemDescription(e.target.value)}
              />
            </div>

            <div className="form-group">
              <label htmlFor="editStatus" className="form-label">
                Current Repair Status *
              </label>
              <select
                id="editStatus"
                className="form-select"
                value={repairStatus}
                onChange={(e) => setRepairStatus(Number(e.target.value) as RepairStatus)}
              >
                <option value={0}>Pending</option>
                <option value={1}>In Progress</option>
                <option value={2}>Completed</option>
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
              {isSubmitting ? 'Updating...' : 'Update Record'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
