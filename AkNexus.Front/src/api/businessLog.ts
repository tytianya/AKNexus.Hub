import http from './http'

/** 业务类型 */
export enum BusinessType {
  BarcodeTest = 1,
  StationTest = 2
}

/** 业务状态 */
export enum BusinessStatus {
  Success = 1,
  Failed = 2
}

export const businessTypeOptions = [
  { value: BusinessType.BarcodeTest, label: '条码测试' },
  { value: BusinessType.StationTest, label: '站点测试' }
]

export const businessStatusOptions = [
  { value: BusinessStatus.Success, label: '成功' },
  { value: BusinessStatus.Failed, label: '失败' }
]

export const businessTypeName = (t: BusinessType) =>
  businessTypeOptions.find(o => o.value === t)?.label ?? '未知'

export const businessStatusName = (s: BusinessStatus) =>
  businessStatusOptions.find(o => o.value === s)?.label ?? '未知'

export interface BusinessLogDto {
  id: number
  type: BusinessType
  typeName: string
  code: string
  content: string
  remark: string | null
  status: BusinessStatus
  createdAt: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface BusinessLogQuery {
  type?: BusinessType
  status?: BusinessStatus
  keyword?: string
  pageNumber: number
  pageSize: number
}

export interface BusinessLogPushInput {
  type: BusinessType
  content: unknown
  remark?: string
  status: BusinessStatus
}

export const getBusinessLogs = (params: BusinessLogQuery) =>
  http.get<PagedResult<BusinessLogDto>>('/BusinessLog', { params })

export const pushBusinessLog = (data: BusinessLogPushInput) =>
  http.post<void>('/BusinessLog/push', data)
