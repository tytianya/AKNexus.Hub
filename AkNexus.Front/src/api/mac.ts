import http from './http'

export interface MacRangeDto {
  id: number
  rangeName: string
  startMac: string
  endMac: string
  nextMac: string
  totalCount: number
  allocatedCount: number
  remainingCount: number
}

export interface MacRangeCreateDto {
  rangeName: string
  startMac: string
  endMac: string
}

export interface MacAllocateRequest {
  rangeName: string
  allocatedTo: string
  count?: number
}

export interface MacAllocationDto {
  id: number
  macAddress: string
  rangeName: string
  allocatedTo: string
  allocatedAt: string
}

export const getMacRanges = () => http.get<MacRangeDto[]>('/MacRange')

export const createMacRange = (data: MacRangeCreateDto) => http.post<MacRangeDto>('/MacRange', data)

export const deleteMacRange = (id: number) => http.delete<void>(`/MacRange/${id}`)

export const allocateMac = (data: MacAllocateRequest) =>
  http.post<MacAllocationDto[]>('/Mac/allocate', data)

export const getMacAllocations = () => http.get<MacAllocationDto[]>('/Mac/allocations')
