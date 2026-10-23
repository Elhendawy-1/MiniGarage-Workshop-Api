import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect } from 'vitest';
import App from '../App';

describe('App Full Integration (with MSW)', () => {
  it('loads and displays cars from the API', async () => {
    render(<App />);

    // Initially shows loading state
    expect(screen.getByText(/loading workshop records/i)).toBeInTheDocument();

    // MSW returns mock cars (Toyota Corolla & Honda Civic)
    expect(await screen.findByRole('heading', { name: 'Toyota Corolla' })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'Honda Civic' })).toBeInTheDocument();

    // Verify KPI counters updated
    expect(screen.getByText('Total in Workshop').previousElementSibling).toHaveTextContent('2');
  });

  it('filters vehicles by search query', async () => {
    const user = userEvent.setup();
    render(<App />);

    // Wait for data to load
    await screen.findByRole('heading', { name: 'Toyota Corolla' });

    // Type "Civic" in search input
    const searchInput = screen.getByLabelText(/search vehicles/i);
    await user.type(searchInput, 'Civic');

    // Honda Civic should be visible, Toyota Corolla should be filtered out
    expect(screen.getByRole('heading', { name: 'Honda Civic' })).toBeInTheDocument();
    expect(screen.queryByRole('heading', { name: 'Toyota Corolla' })).not.toBeInTheDocument();
  });

  it('filters vehicles by status tabs', async () => {
    const user = userEvent.setup();
    render(<App />);

    await screen.findByRole('heading', { name: 'Toyota Corolla' });

    // Click "Pending" tab
    const pendingTab = screen.getByRole('tab', { name: /pending/i });
    await user.click(pendingTab);

    // Honda Civic is Pending (status 0), Toyota Corolla is InProgress (status 1)
    expect(screen.getByRole('heading', { name: 'Honda Civic' })).toBeInTheDocument();
    expect(screen.queryByRole('heading', { name: 'Toyota Corolla' })).not.toBeInTheDocument();
  });

  it('registers a new vehicle via modal and adds it to the grid', async () => {
    const user = userEvent.setup();
    render(<App />);

    await screen.findByRole('heading', { name: 'Toyota Corolla' });

    // Open register modal
    await user.click(screen.getByRole('button', { name: /\+ register vehicle/i }));

    // Fill form
    await user.type(screen.getByLabelText(/owner full name/i), 'New Customer');
    await user.type(screen.getByLabelText(/car model & make/i), 'Mercedes G63');
    await user.type(screen.getByLabelText(/license plate number/i), 'G63-AMG');
    await user.type(screen.getByLabelText(/problem description/i), 'Exhaust valve squeaking');

    // Submit
    await user.click(screen.getByRole('button', { name: /save & register/i }));

    // Verify newly registered car is displayed in the grid
    expect(await screen.findByRole('heading', { name: 'Mercedes G63' })).toBeInTheDocument();
    expect(screen.getByText('G63-AMG')).toBeInTheDocument();
  });
});
