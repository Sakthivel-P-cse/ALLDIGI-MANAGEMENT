/**
 * Generic paginated response envelope returned by list endpoints.
 * Mirrors the backend PagedResult<T> / PaginatedResponse<T> shape
 * ({ data, skip, take, total }); page/totalPages are optional because
 * older endpoints only emit data/total.
 */
export interface PagedResult<T> {
  data: T[];
  total: number;
  skip?: number;
  take?: number;
  page?: number;
  totalPages?: number;
}

/**
 * Standard query parameters for list endpoints. Matches the backend PageQuery.
 */
export interface PageQuery {
  skip?: number;
  take?: number;
  search?: string;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
}
