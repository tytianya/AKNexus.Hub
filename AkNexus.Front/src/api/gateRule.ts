import http from './http'

export interface GateRuleDto {
  id: number
  stationId: number
  stationCode: string
  stationName: string
  ruleName: string
  ruleType: string
  condition: string
  isActive: boolean
  priority: number
}

export interface GateRuleCreateDto {
  stationId: number
  ruleName: string
  ruleType: string
  condition: string
  isActive?: boolean
  priority?: number
}

export const getRules = () => http.get<GateRuleDto[]>('/GateRule')

export const createRule = (data: GateRuleCreateDto) => http.post<GateRuleDto>('/GateRule', data)

export const updateRule = (id: number, data: GateRuleCreateDto) =>
  http.put<void>(`/GateRule/${id}`, data)

export const deleteRule = (id: number) => http.delete<void>(`/GateRule/${id}`)
