import { HttpParams } from '@angular/common/http';
import { PageQuery } from '../models/paged-result.model';

/**
 * Builds HttpParams from a PageQuery, omitting empty values so list endpoints
 * receive only the parameters the caller actually set. Shared by every module
 * service so query-string construction is written once.
 */
export function buildPageParams(query: PageQuery = {}): HttpParams {
  let params = new HttpParams();

  if (query.skip != null) {
    params = params.set('skip', String(query.skip));
  }
  if (query.take != null) {
    params = params.set('take', String(query.take));
  }
  if (query.search) {
    params = params.set('search', query.search);
  }
  if (query.sortBy) {
    params = params.set('sortBy', query.sortBy);
  }
  if (query.sortDir) {
    params = params.set('sortDir', query.sortDir);
  }

  return params;
}

/**
 * Resolves the API base URL from the environment, normalizing a trailing slash.
 * Centralizes the `environment.apiUrl?.replace(...) || '/api/v1'` idiom that was
 * duplicated across the existing services.
 */
export function apiBase(apiUrl: string | undefined): string {
  return apiUrl?.replace(/\/$/, '') || '/api/v1';
}
