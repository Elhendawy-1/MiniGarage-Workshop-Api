import { useState, useEffect, useMemo, useCallback } from 'react';
import type { CarResponseDto, CreateCarDto, UpdateCarDto } from './types/car';
import { carApi } from './services/api';
import { Header } from './components/Header';
import { KpiStrip } from './components/KpiStrip';
import { Toolbar } from './components/Toolbar';
import { CarCard } from './components/CarCard';
import { RegisterModal } from './components/RegisterModal';
import { EditModal } from './components/EditModal';
import { DeleteModal } from './components/DeleteModal';
import { ToastContainer } from './components/Toast';
import type { ToastMessage } from './components/Toast';

export function App() {
  const [cars, setCars] = useState<CarResponseDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [searchQuery, setSearchQuery] = useState('');
  const [currentFilter, setCurrentFilter] = useState('all');

  const [isRegisterOpen, setIsRegisterOpen] = useState(false);
  const [editingCar, setEditingCar] = useState<CarResponseDto | null>(null);
  const [deletingCar, setDeletingCar] = useState<CarResponseDto | null>(null);

  const [toasts, setToasts] = useState<ToastMessage[]>([]);

  const addToast = useCallback((message: string, type: 'success' | 'error' | 'info' = 'info') => {
    const id = `${Date.now()}-${Math.random()}`;
    setToasts((prev) => [...prev, { id, message, type }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 4000);
  }, []);

  // Initial Fetch (Vercel Best Practice: async-parallel)
  useEffect(() => {
    let isMounted = true;
    async function fetchInitialData() {
      setIsLoading(true);
      setError(null);
      try {
        const data = await carApi.getAll();
        if (isMounted) {
          setCars(data);
        }
      } catch (err: any) {
        if (isMounted) {
          setError(err.message || 'Failed to connect to API');
          addToast('Failed to load cars from server', 'error');
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    fetchInitialData();
    return () => {
      isMounted = false;
    };
  }, [addToast]);

  // Derived Filtered List (Vercel Best Practice: rerender-derived-state-no-effect)
  const filteredCars = useMemo(() => {
    const q = searchQuery.toLowerCase().trim();
    return cars.filter((car) => {
      if (currentFilter !== 'all' && car.repairStatus !== parseInt(currentFilter, 10)) {
        return false;
      }
      if (q) {
        return (
          car.licensePlate.toLowerCase().includes(q) ||
          car.ownerName.toLowerCase().includes(q) ||
          car.carModel.toLowerCase().includes(q)
        );
      }
      return true;
    });
  }, [cars, searchQuery, currentFilter]);

  // Handlers with functional state updates (Vercel Best Practice: rerender-functional-setstate)
  const handleCreateCar = async (dto: CreateCarDto) => {
    const created = await carApi.create(dto);
    setCars((prev) => [created, ...prev]);
    addToast(`Vehicle ${created.licensePlate} registered successfully!`, 'success');
  };

  const handleUpdateCar = async (id: number, dto: UpdateCarDto) => {
    const updated = await carApi.update(id, dto);
    setCars((prev) => prev.map((c) => (c.id === id ? updated : c)));
    addToast(`Vehicle ${updated.licensePlate} updated!`, 'success');
  };

  const handleDeleteCar = async (id: number, adminKey: string) => {
    await carApi.delete(id, adminKey);
    setCars((prev) => prev.filter((c) => c.id !== id));
    addToast('Vehicle deleted from workshop records', 'success');
  };

  return (
    <>
      <Header onOpenRegister={() => setIsRegisterOpen(true)} />

      <main className="container">
        <KpiStrip cars={cars} />

        <Toolbar
          searchQuery={searchQuery}
          onSearchChange={setSearchQuery}
          currentFilter={currentFilter}
          onFilterChange={setCurrentFilter}
        />

        {/* Rendering Conditionals with Ternary (Vercel Best Practice: rendering-conditional-render) */}
        {isLoading ? (
          <div className="empty-state" role="status">
            <div className="empty-state-icon">⏳</div>
            <h2>Loading workshop records...</h2>
            <p>Fetching vehicle status from ASP.NET Core API.</p>
          </div>
        ) : error ? (
          <div className="empty-state" role="alert">
            <div className="empty-state-icon" style={{ color: 'var(--status-danger)' }}>
              ⚠️
            </div>
            <h2>Unable to connect to API</h2>
            <p>{error}</p>
            <button
              type="button"
              className="btn btn-secondary btn-sm"
              onClick={() => window.location.reload()}
            >
              Retry Connection
            </button>
          </div>
        ) : filteredCars.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">🚗</div>
            <h2>No vehicles found</h2>
            <p>
              {cars.length === 0
                ? 'No cars registered in the garage yet.'
                : 'No vehicles match the selected filter criteria.'}
            </p>
            <button
              type="button"
              className="btn btn-primary btn-sm"
              onClick={() => setIsRegisterOpen(true)}
            >
              + Register First Vehicle
            </button>
          </div>
        ) : (
          <section className="cars-grid" aria-label="Vehicles List">
            <h2 className="sr-only">Registered Workshop Vehicles</h2>
            {filteredCars.map((car) => (
              <CarCard
                key={car.id}
                car={car}
                onEdit={setEditingCar}
                onDelete={setDeletingCar}
              />
            ))}
          </section>
        )}
      </main>

      <footer className="app-footer">
        <div className="container footer-content">
          <div>Mini Garage — React 19 Client • ASP.NET Core 9 Web API</div>
          <div>Protected by Rate Limiting & Admin Key Authorization</div>
        </div>
      </footer>

      {/* Modals */}
      <RegisterModal
        isOpen={isRegisterOpen}
        onClose={() => setIsRegisterOpen(false)}
        onSubmit={handleCreateCar}
      />

      <EditModal
        car={editingCar}
        onClose={() => setEditingCar(null)}
        onSubmit={handleUpdateCar}
      />

      <DeleteModal
        car={deletingCar}
        onClose={() => setDeletingCar(null)}
        onConfirm={handleDeleteCar}
      />

      <ToastContainer toasts={toasts} />
    </>
  );
}

export default App;
