export interface LocationResponseDto {
  id: number;
  name?: string;
  address?: string;
  address2?: string;
  city?: string;
  state?: string;
  country?: string;
  zip?: string;
  phone?: string;
  fax?: string;
  currency?: string;
  parentId?: number;
  parentName?: string;
  managerId?: number;
  managerName?: string;
  companyId?: number;
  image?: string;
  notes?: string;
  assetsCount: number;
  usersCount: number;
  childrenCount: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateLocationDto {
  name: string;
  address?: string;
  address2?: string;
  city?: string;
  state?: string;
  country?: string;
  zip?: string;
  phone?: string;
  fax?: string;
  currency?: string;
  parentId?: number;
  managerId?: number;
  companyId?: number;
  notes?: string;
}

export interface UpdateLocationDto extends Partial<CreateLocationDto> {}

export interface PagedLocations {
  data: LocationResponseDto[];
  skip: number;
  take: number;
  total: number;
}
