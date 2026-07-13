export interface AssetResponseDto {
  id: number;
  name?: string | null;
  assetTag?: string | null;
  modelId?: number | null;
  serial?: string | null;
  purchaseDate?: string | null; // ISO date string
  assetEolDate?: string | null;
  eolExplicit?: boolean | null;
  purchaseCost?: number | null;
  orderNumber?: string | null;
  assignedTo?: number | null;
  notes?: string | null;
  statusId?: number | null;
  warrantyMonths?: number | null;
  depreciate?: boolean | null;
  supplierId?: number | null;
  requestable?: boolean | null;
  rtdLocationId?: number | null;
  locationId?: number | null;
  companyId?: number | null;
  customFields?: Record<string, string> | null;
  accepted?: string | null;
  lastCheckout?: string | null;
  lastCheckin?: string | null;
  expectedCheckin?: string | null;
  assignedType?: string | null;
  lastAuditDate?: string | null;
  nextAuditDate?: string | null;
  checkinCounter?: number;
  createdAt?: string | null;
  updatedAt?: string | null;
  deletedAt?: string | null;
  archived?: boolean | null;

  // Display fields populated by API
  modelName?: string | null;
  locationName?: string | null;
  assignedToName?: string | null;
  supplierName?: string | null;
}

export interface CreateAssetDto {
  name: string;
  assetTag: string;
  modelId: number;
  serial?: string | null;
  purchaseDate?: string | null;
  assetEolDate?: string | null;
  eolExplicit?: boolean;
  purchaseCost?: number | null;
  orderNumber?: string | null;
  assignedTo?: number | null;
  notes?: string | null;
  statusId?: number | null;
  warrantyMonths?: number | null;
  depreciate?: boolean | null;
  supplierId?: number | null;
  requestable?: boolean;
  rtdLocationId?: number | null;
  locationId?: number | null;
  companyId?: number | null;
  customFields?: Record<string, string> | null;
}

export interface UpdateAssetDto {
  id: number;
  name?: string | null;
  assetTag?: string | null;
  modelId?: number | null;
  serial?: string | null;
  purchaseDate?: string | null;
  assetEolDate?: string | null;
  eolExplicit?: boolean | null;
  purchaseCost?: number | null;
  orderNumber?: string | null;
  assignedTo?: number | null;
  notes?: string | null;
  statusId?: number | null;
  warrantyMonths?: number | null;
  depreciate?: boolean | null;
  supplierId?: number | null;
  requestable?: boolean | null;
  rtdLocationId?: number | null;
  locationId?: number | null;
  companyId?: number | null;
  customFields?: Record<string, string> | null;
}

export interface PagedAssets {
  data: AssetResponseDto[];
  total: number;
}
