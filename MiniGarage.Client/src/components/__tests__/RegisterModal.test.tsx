import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { RegisterModal } from '../RegisterModal';

describe('RegisterModal Component', () => {
  it('does not render when isOpen is false', () => {
    const { container } = render(
      <RegisterModal isOpen={false} onClose={vi.fn()} onSubmit={vi.fn()} />
    );
    expect(container.firstChild).toBeNull();
  });

  it('renders inputs and submits typed data via accessible labels', async () => {
    const user = userEvent.setup();
    const handleSubmit = vi.fn().mockResolvedValue(undefined);
    const handleClose = vi.fn();

    render(
      <RegisterModal isOpen={true} onClose={handleClose} onSubmit={handleSubmit} />
    );

    expect(screen.getByRole('heading', { name: /register new vehicle/i })).toBeInTheDocument();

    await user.type(screen.getByLabelText(/owner full name/i), 'Nasser Al-Subaie');
    await user.type(screen.getByLabelText(/car model & make/i), 'Ford F-150');
    await user.type(screen.getByLabelText(/license plate number/i), 'F-150-TRUCK');
    await user.type(screen.getByLabelText(/problem description/i), 'Transmission slip under heavy load');
    await user.selectOptions(screen.getByLabelText(/initial workshop status/i), '1');

    await user.click(screen.getByRole('button', { name: /save & register/i }));

    expect(handleSubmit).toHaveBeenCalledWith({
      ownerName: 'Nasser Al-Subaie',
      carModel: 'Ford F-150',
      licensePlate: 'F-150-TRUCK',
      problemDescription: 'Transmission slip under heavy load',
      repairStatus: 1,
    });
    expect(handleClose).toHaveBeenCalled();
  });
});
