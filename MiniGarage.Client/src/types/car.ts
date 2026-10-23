export type RepairStatus = 0 | 1 | 2;

export interface CarResponseDto {
  id: number;
  ownerName: string;
  carModel: string;
  licensePlate: string;
  problemDescription: string;
  repairStatus: RepairStatus;
  repairStatusLabel: string;
  createdAt: string;
}

export interface CreateCarDto {
  ownerName: string;
  carModel: string;
  licensePlate: string;
  problemDescription: string;
  repairStatus: RepairStatus;
}

export interface UpdateCarDto {
  ownerName: string;
  carModel: string;
  licensePlate: string;
  problemDescription: string;
  repairStatus: RepairStatus;
}
