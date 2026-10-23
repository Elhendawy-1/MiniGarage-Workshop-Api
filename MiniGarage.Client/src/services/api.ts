import type { CarResponseDto, CreateCarDto, UpdateCarDto } from '../types/car';

async function handleResponse<T>(res: Response): Promise<T> {
  if (!res.ok) {
    let errorDetail = `Request failed: ${res.status} ${res.statusText}`;
    try {
      const data = await res.json();
      if (data.message) {
        errorDetail = data.message;
      } else if (data.errors) {
        const firstField = Object.keys(data.errors)[0];
        if (firstField && data.errors[firstField]?.length > 0) {
          errorDetail = data.errors[firstField][0];
        }
      } else if (data.title) {
        errorDetail = data.title;
      }
    } catch {
      // Body not JSON
    }
    throw new Error(errorDetail);
  }

  if (res.status === 204) {
    return null as T;
  }

  return res.json() as Promise<T>;
}

export const carApi = {
  async getAll(): Promise<CarResponseDto[]> {
    const res = await fetch('/api/cars');
    return handleResponse<CarResponseDto[]>(res);
  },

  async getById(id: number): Promise<CarResponseDto> {
    const res = await fetch(`/api/cars/${id}`);
    return handleResponse<CarResponseDto>(res);
  },

  async create(dto: CreateCarDto): Promise<CarResponseDto> {
    const res = await fetch('/api/cars', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dto),
    });
    return handleResponse<CarResponseDto>(res);
  },

  async update(id: number, dto: UpdateCarDto): Promise<CarResponseDto> {
    const res = await fetch(`/api/cars/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dto),
    });
    return handleResponse<CarResponseDto>(res);
  },

  async delete(id: number, adminKey: string): Promise<void> {
    const res = await fetch(`/api/cars/${id}`, {
      method: 'DELETE',
      headers: {
        'X-Admin-Key': adminKey,
      },
    });
    return handleResponse<void>(res);
  },
};
