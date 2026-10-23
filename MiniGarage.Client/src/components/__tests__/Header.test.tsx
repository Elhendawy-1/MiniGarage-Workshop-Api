import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { Header } from '../Header';

describe('Header Component', () => {
  it('renders brand title and protocol indicator', () => {
    render(<Header onOpenRegister={vi.fn()} />);

    expect(screen.getByRole('heading', { name: /mini garage/i })).toBeInTheDocument();
    expect(screen.getByText(/http\/3 ready/i)).toBeInTheDocument();
  });

  it('triggers onOpenRegister when clicking + Register Vehicle button', async () => {
    const user = userEvent.setup();
    const handleOpen = vi.fn();

    render(<Header onOpenRegister={handleOpen} />);

    const button = screen.getByRole('button', { name: /\+ register vehicle/i });
    await user.click(button);

    expect(handleOpen).toHaveBeenCalledTimes(1);
  });
});
