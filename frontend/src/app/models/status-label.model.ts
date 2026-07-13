export type StatusLabelType = 'deployable' | 'pending' | 'archived' | 'undeployable';

export interface StatusLabelResponseDto {
  id: number;
  name?: string;
  statusType: StatusLabelType;
  deployable: boolean;
  pending: boolean;
  archived: boolean;
  color?: string;
  showInNav?: boolean;
  defaultLabel?: boolean;
  notes?: string;
  assetsCount: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateStatusLabelDto {
  name: string;
  statusType: StatusLabelType;
  color?: string;
  showInNav?: boolean;
  defaultLabel?: boolean;
  notes?: string;
}

export interface UpdateStatusLabelDto extends Partial<CreateStatusLabelDto> {}

export interface PagedStatusLabels {
  data: StatusLabelResponseDto[];
  skip: number;
  take: number;
  total: number;
}
