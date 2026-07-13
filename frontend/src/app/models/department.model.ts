export interface DepartmentResponseDto {
  id: number;
  name: string;
  companyId?: number;
  companyName?: string;
  locationId?: number;
  locationName?: string;
  managerId?: number;
  managerName?: string;
  phone?: string;
  fax?: string;
  image?: string;
  notes?: string;
  usersCount: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateDepartmentDto {
  name: string;
  companyId?: number;
  locationId?: number;
  managerId?: number;
  phone?: string;
  fax?: string;
  notes?: string;
}

export interface UpdateDepartmentDto extends Partial<CreateDepartmentDto> {}

export interface PagedDepartments {
  data: DepartmentResponseDto[];
  skip: number;
  take: number;
  total: number;
}
