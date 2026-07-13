export interface ManufacturerResponseDto {
  id: number;
  name: string;
  url?: string;
  supportUrl?: string;
  supportPhone?: string;
  supportEmail?: string;
  warrantyLookupUrl?: string;
  image?: string;
  notes?: string;
  assetsCount: number;
  licensesCount: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateManufacturerDto {
  name: string;
  url?: string;
  supportUrl?: string;
  supportPhone?: string;
  supportEmail?: string;
  warrantyLookupUrl?: string;
  image?: string;
  notes?: string;
}

export interface UpdateManufacturerDto extends Partial<CreateManufacturerDto> {}

export interface PagedManufacturers {
  data: ManufacturerResponseDto[];
  skip: number;
  take: number;
  total: number;
}
