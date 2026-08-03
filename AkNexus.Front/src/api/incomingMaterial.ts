import http from './http'

export interface IncomingMaterialDto {
  id: number
  materialCode: string
  productModel: string
  quantity: number
  barcodeCount: number
  supplier: string
  lotNo: string
  inspectResult: string | null
  inspectedBy: string | null
  inspectedAt: string | null
}

export interface IncomingMaterialCreateDto {
  materialCode: string
  productModel: string
  quantity: number
  flowId?: number
  supplier?: string
  lotNo?: string
  remark?: string
}

export interface IncomingMaterialUpdateDto {
  materialCode?: string
  productModel?: string
  quantity?: number
  flowId?: number
  supplier?: string
  lotNo?: string
  remark?: string
  inspectResult?: string
  inspectedBy?: string
}

export interface MaterialBarcodeDto {
  id: number
  sequenceNo: number
  barcode: string
  currentStationId: number | null
  stationName: string | null
  isUsed: boolean
  usedAt: string | null
}

export const getIncomingMaterials = () =>
  http.get<IncomingMaterialDto[]>('/IncomingMaterial')

export const createIncomingMaterial = (data: IncomingMaterialCreateDto) =>
  http.post<IncomingMaterialDto>('/IncomingMaterial', data)

export const updateIncomingMaterial = (id: number, data: IncomingMaterialUpdateDto) =>
  http.put<void>(`/IncomingMaterial/${id}`, data)

export const deleteIncomingMaterial = (id: number) =>
  http.delete<void>(`/IncomingMaterial/${id}`)

export const getBarcodes = (id: number) =>
  http.get<MaterialBarcodeDto[]>(`/IncomingMaterial/${id}/barcodes`)
