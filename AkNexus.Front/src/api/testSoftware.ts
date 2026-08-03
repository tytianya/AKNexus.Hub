import http from './http'

export interface TestSoftwareDto {
  id: number
  softwareCode: string
  softwareName: string
  version: string
  description: string
  fileName: string
  fileSize: number
  uploadedAt: string
}

export interface TestSoftwareBindDto {
  stationId: number
  softwareId: number
}

export const getSoftware = () => http.get<TestSoftwareDto[]>('/TestSoftware')

export const uploadSoftware = (
  formData: FormData,
  onProgress?: (percent: number) => void
) =>
  http.post<TestSoftwareDto>('/TestSoftware/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
    onUploadProgress: onProgress
      ? (e) => onProgress(e.total ? Math.round((e.loaded * 100) / e.total) : 0)
      : undefined
  })

export const deleteSoftware = (id: number) => http.delete<void>(`/TestSoftware/${id}`)

export const getDownloadUrl = (id: number) =>
  `${import.meta.env.VITE_API_BASE_URL || ''}/api/TestSoftware/download/${id}`

export const bindSoftware = (data: TestSoftwareBindDto) =>
  http.post<void>('/TestSoftware/bind', data)

export const getStationSoftware = (stationId: number) =>
  http.get<TestSoftwareDto[]>(`/TestSoftware/station/${stationId}`)
