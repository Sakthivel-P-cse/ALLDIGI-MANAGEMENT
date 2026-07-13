export interface CategoryResponseDto {
  id: number;
  name: string;
  categoryType?: string;
  useDefaultEula: boolean;
  requireAcceptance: boolean;
  checkinEmail: boolean;
  alertOnResponse: boolean;
  tagColor?: string;
  image?: string;
  notes?: string;
  assetsCount: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateCategoryDto {
  name: string;
  categoryType?: string;
  useDefaultEula?: boolean;
  requireAcceptance?: boolean;
  checkinEmail?: boolean;
  alertOnResponse?: boolean;
  tagColor?: string;
  notes?: string;
}

export interface UpdateCategoryDto extends Partial<CreateCategoryDto> {}

export interface PagedCategories {
  data: CategoryResponseDto[];
  skip: number;
  take: number;
  total: number;
}
