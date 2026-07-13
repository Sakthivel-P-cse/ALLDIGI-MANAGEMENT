export interface CompanyResponseDto {
  id: number;
  name: string;
  email?: string;
  phone?: string;
  fax?: string;
  image?: string;
  notes?: string;
  assetsCount: number;
  usersCount: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateCompanyDto {
  name: string;
  email?: string;
  phone?: string;
  fax?: string;
  notes?: string;
}

export interface UpdateCompanyDto extends Partial<CreateCompanyDto> {}

export interface PagedCompanies {
  data: CompanyResponseDto[];
  skip: number;
  take: number;
  total: number;
}
