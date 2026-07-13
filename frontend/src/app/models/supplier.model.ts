export interface SupplierResponseDto {
  id: number;
  name: string;
  address?: string;
  address2?: string;
  city?: string;
  state?: string;
  country?: string;
  zip?: string;
  phone?: string;
  fax?: string;
  email?: string;
  url?: string;
  contact?: string;
  image?: string;
  notes?: string;
  assetsCount: number;
  licensesCount: number;
  accessoriesCount: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateSupplierDto {
  name: string;
  address?: string;
  address2?: string;
  city?: string;
  state?: string;
  country?: string;
  zip?: string;
  phone?: string;
  fax?: string;
  email?: string;
  url?: string;
  contact?: string;
  notes?: string;
}

export interface UpdateSupplierDto extends Partial<CreateSupplierDto> {}

export interface PagedSuppliers {
  data: SupplierResponseDto[];
  skip: number;
  take: number;
  total: number;
}
