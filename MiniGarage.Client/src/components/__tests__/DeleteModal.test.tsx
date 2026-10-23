import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { DeleteModal } from '../DeleteModal';
import type { CarResponseDto } from '../../types/car';

describe('DeleteModal Component', () => {
  const mockCar: CarResponseDto = {
    id: 5,
    ownerName: 'Test Owner',
    carModel: 'Audi RS6',
    licensePlate: 'AUD-888',
    problemDescription: 'Brake fluid service',
    repairStatus: 0,
    repairStatusLabel: 'Pending',
    createdAt: '2026-01-01T00:00:00Z',
  };

  it('renders confirmation text with car details', () => {
    render(<DeleteModal car={mockCar} onClose={vi.fn()} onConfirm={vi.fn()} />);

    expect(screen.getByRole('heading', { name: /confirm deletion/i })).toBeInTheDocument();
    expect(screen.getByText(/Audi RS6 \(AUD-888\)/)).toBeInTheDocument();
  });

  it('submits car ID and admin key upon confirmation', async () => {
    const user = userEvent.setup();
    const handleConfirm = vi.fn().mockResolvedValue(undefined);
    const handleClose = vi.fn();

    render(<DeleteModal car={mockCar} onClose={handleClose} onConfirm={handleConfirm} />);

    await user.type(screen.getByLabelText(/admin api key/i), 'garage-admin-secret-2026');
    await user.click(screen.getByRole('button', { name: /delete vehicle/i }));

    expect(handleConfirm).toHaveBeenCalledWith(5, 'garage-admin-secret-2026');
    expect(handleClose).toHaveBeenCalled();
  });
});
