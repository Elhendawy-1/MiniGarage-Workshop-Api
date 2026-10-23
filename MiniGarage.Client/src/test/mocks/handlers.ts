import { http, HttpResponse } from 'msw';
import type { CarResponseDto } from '../../types/car';

export const mockCars: CarResponseDto[] = [
  {
    id: 1,
    ownerName: 'Ahmed Al-Rashidi',
    carModel: 'Toyota Corolla',
    licensePlate: 'ABC-1234',
    problemDescription: 'Engine overheating',
    repairStatus: 1,
    repairStatusLabel: 'InProgress',
    createdAt: '2026-01-10T08:00:00Z',
  },
  {
    id: 2,
    ownerName: 'Sara Al-Mutairi',
    carModel: 'Honda Civic',
    licensePlate: 'XYZ-5678',
    problemDescription: 'Brake pads worn out',
    repairStatus: 0,
    repairStatusLabel: 'Pending',
    createdAt: '2026-01-11T09:30:00Z',
  },
];

export const handlers = [
  // GET /api/cars
  http.get('/api/cars', () => {
    return HttpResponse.json(mockCars);
  }),

  // GET /api/cars/:id
  http.get('/api/cars/:id', ({ params }) => {
    const id = Number(params.id);
    const car = mockCars.find((c) => c.id === id);
    if (!car) {
      return HttpResponse.json({ message: `Car with ID ${id} was not found.` }, { status: 404 });
    }
    return HttpResponse.json(car);
  }),

  // POST /api/cars
  http.post('/api/cars', async ({ request }) => {
    const body = (await request.json()) as any;
    if (!body.ownerName || !body.carModel || !body.licensePlate || !body.problemDescription) {
      return HttpResponse.json({ title: 'Validation error', status: 400 }, { status: 400 });
    }

    const newCar: CarResponseDto = {
      id: 99,
      ownerName: body.ownerName,
      carModel: body.carModel,
      licensePlate: body.licensePlate,
      problemDescription: body.problemDescription,
      repairStatus: body.repairStatus ?? 0,
      repairStatusLabel: body.repairStatus === 1 ? 'InProgress' : body.repairStatus === 2 ? 'Completed' : 'Pending',
      createdAt: new Date().toISOString(),
    };

    return HttpResponse.json(newCar, {
      status: 201,
      headers: { Location: `/api/cars/${newCar.id}` },
    });
  }),

  // PUT /api/cars/:id
  http.put('/api/cars/:id', async ({ params, request }) => {
    const id = Number(params.id);
    const body = (await request.json()) as any;
    const existing = mockCars.find((c) => c.id === id);

    if (!existing) {
      return HttpResponse.json({ message: `Car with ID ${id} was not found.` }, { status: 404 });
    }

    const updated: CarResponseDto = {
      ...existing,
      ...body,
      repairStatusLabel: body.repairStatus === 1 ? 'InProgress' : body.repairStatus === 2 ? 'Completed' : 'Pending',
    };

    return HttpResponse.json(updated);
  }),

  // DELETE /api/cars/:id (Requires X-Admin-Key)
  http.delete('/api/cars/:id', ({ params, request }) => {
    const adminKey = request.headers.get('X-Admin-Key');
    if (adminKey !== 'garage-admin-secret-2026') {
      return HttpResponse.json({ message: 'Unauthorized: A valid X-Admin-Key header is required.' }, { status: 401 });
    }

    const id = Number(params.id);
    const existing = mockCars.find((c) => c.id === id);
    if (!existing) {
      return HttpResponse.json({ message: `Car with ID ${id} was not found.` }, { status: 404 });
    }

    return new HttpResponse(null, { status: 204 });
  }),
];
