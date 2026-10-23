import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { KpiStrip } from '../KpiStrip';
import type { CarResponseDto } from '../../types/car';

describe('KpiStrip Component', () => {
  const sampleCars: CarResponseDto[] = [
    {
      id: 1,
      ownerName: 'Owner 1',
      carModel: 'Model A',
      licensePlate: 'PLATE-1',
      problemDescription: 'Issue 1',
      repairStatus: 0, // Pending
      repairStatusLabel: 'Pending',
      createdAt: '2026-01-01T00:00:00Z',
    },
    {
      id: 2,
      ownerName: 'Owner 2',
      carModel: 'Model B',
      licensePlate: 'PLATE-2',
      problemDescription: 'Issue 2',
      repairStatus: 1, // InProgress
      repairStatusLabel: 'InProgress',
      createdAt: '2026-01-02T00:00:00Z',
    },
    {
      id: 3,
      ownerName: 'Owner 3',
      carModel: 'Model C',
      licensePlate: 'PLATE-3',
      problemDescription: 'Issue 3',
      repairStatus: 2, // Completed
      repairStatusLabel: 'Completed',
      createdAt: '2026-01-03T00:00:00Z',
    },
  ];

  it('calculates and renders correct counts for all statuses', () => {
    render(<KpiStrip cars={sampleCars} />);

    expect(screen.getByText('Total in Workshop').previousElementSibling).toHaveTextContent('3');
    expect(screen.getByText('Pending Inspection').previousElementSibling).toHaveTextContent('1');
    expect(screen.getByText('In Active Repair').previousElementSibling).toHaveTextContent('1');
    expect(screen.getByText('Ready for Pickup').previousElementSibling).toHaveTextContent('1');
  });

  it('renders zeroes when cars array is empty', () => {
    render(<KpiStrip cars={[]} />);

    expect(screen.getByText('Total in Workshop').previousElementSibling).toHaveTextContent('0');
    expect(screen.getByText('Pending Inspection').previousElementSibling).toHaveTextContent('0');
  });
});
