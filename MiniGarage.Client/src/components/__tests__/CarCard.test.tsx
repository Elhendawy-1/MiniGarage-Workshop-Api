import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { CarCard } from '../CarCard';
import type { CarResponseDto } from '../../types/car';

describe('CarCard Component', () => {
  const mockCar: CarResponseDto = {
    id: 10,
    ownerName: 'Tariq Al-Mansoor',
    carModel: 'Porsche Cayenne',
    licensePlate: 'KSA-777',
    problemDescription: 'Air suspension calibration needed',
    repairStatus: 1,
    repairStatusLabel: 'InProgress',
    createdAt: '2026-03-01T10:00:00Z',
  };

  it('renders car details, embossed license plate, and status', () => {
    render(<CarCard car={mockCar} onEdit={vi.fn()} onDelete={vi.fn()} />);

    expect(screen.getByText('KSA-777')).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Porsche Cayenne' })).toBeInTheDocument();
    expect(screen.getByText(/Tariq Al-Mansoor/)).toBeInTheDocument();
    expect(screen.getByText(/Air suspension calibration needed/)).toBeInTheDocument();
    expect(screen.getByText(/● InProgress/)).toBeInTheDocument();
  });

  it('calls onEdit when Edit button is clicked', async () => {
    const user = userEvent.setup();
    const handleEdit = vi.fn();

    render(<CarCard car={mockCar} onEdit={handleEdit} onDelete={vi.fn()} />);

    await user.click(screen.getByRole('button', { name: /edit porsche cayenne/i }));
    expect(handleEdit).toHaveBeenCalledWith(mockCar);
  });

  it('calls onDelete when Delete button is clicked', async () => {
    const user = userEvent.setup();
    const handleDelete = vi.fn();

    render(<CarCard car={mockCar} onEdit={vi.fn()} onDelete={handleDelete} />);

    await user.click(screen.getByRole('button', { name: /delete porsche cayenne/i }));
    expect(handleDelete).toHaveBeenCalledWith(mockCar);
  });
});
