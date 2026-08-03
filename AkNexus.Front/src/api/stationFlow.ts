import http from './http'

export interface FlowStationDto {
  stationId: number
  stationCode: string
  order: number
  required: boolean
}

export interface FlowDto {
  id: number
  flowCode: string
  flowName: string
  description: string
  isActive: boolean
  stations: FlowStationDto[]
}

export interface FlowCreateDto {
  flowCode: string
  flowName: string
  description?: string
  isActive?: boolean
  stations: FlowStationDto[]
}

export const getFlows = () => http.get<FlowDto[]>('/StationFlow')

export const getFlowById = (id: number) => http.get<FlowDto>(`/StationFlow/${id}`)

export const createFlow = (data: FlowCreateDto) => http.post<FlowDto>('/StationFlow', data)

export const updateFlow = (id: number, data: FlowCreateDto) =>
  http.put<void>(`/StationFlow/${id}`, data)

export const deleteFlow = (id: number) => http.delete<void>(`/StationFlow/${id}`)
