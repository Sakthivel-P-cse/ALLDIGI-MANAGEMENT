export interface ModelResponseDto {
  id: number;
  name: string;
  modelNumber?: string;
  manufacturerId?: number;
  manufacturerName?: string;
  categoryId?: number;
  categoryName?: string;
  depreciationId?: number;
  fieldsetId?: number;
  eol?: number;
  requireSerial: boolean;
  requestable: number;
  image?: string;
  notes?: string;
  assetsCount: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateModelDto {
  name: string;
  modelNumber?: string;
  manufacturerId?: number;
  categoryId?: number;
  depreciationId?: number;
  fieldsetId?: number;
  eol?: number;
  requireSerial?: boolean;
  requestable?: number;
  image?: string;
  notes?: string;
}

export interface UpdateModelDto extends Partial<CreateModelDto> {}

export interface PagedModels {
  data: ModelResponseDto[];
  skip: number;
  take: number;
  total: number;
}
