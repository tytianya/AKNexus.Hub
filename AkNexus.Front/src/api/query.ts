import http from './http'

export interface QueryRequest {
  productCode?: string
  stationCode?: string
  passResult?: string
  startTime?: string
  endTime?: string
  page: number
  pageSize: number
}

export interface ProductStationLogDto {
  id: number
  productCode: string
  productName: string
  stationCode: string
  stationName: string
  flowCode: string
  passResult: string
  operator: string
  remark: string
  passTime: string
}

export interface QueryResult {
  items: ProductStationLogDto[]
  total: number
  page: number
  pageSize: number
}

export const queryLogs = (data: QueryRequest) => http.post<QueryResult>('/Query/logs', data)
