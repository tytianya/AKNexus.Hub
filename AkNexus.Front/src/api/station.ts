import http from './http'

export interface Station {
  id: number
  stationCode: string
  stationName: string
  stationType: string
  location: string
  sortOrder: number
}

export interface StationCreateDto {
  stationCode: string
  stationName: string
  stationType: string
  location: string
  sortOrder: number
}

export const getStations = () => http.get<Station[]>('/Station')

export const getStationById = (id: number) => http.get<Station>(`/Station/${id}`)

export const createStation = (data: StationCreateDto) => http.post<Station>('/Station', data)

export const updateStation = (id: number, data: StationCreateDto) =>
  http.put<void>(`/Station/${id}`, data)

export const deleteStation = (id: number) => http.delete<void>(`/Station/${id}`)
