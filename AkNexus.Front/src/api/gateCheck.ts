import http from './http'

export interface GateCheckRequest {
  productCode: string
  stationCode: string
  operator?: string
  remark?: string
}

export interface GateCheckMatchedRule {
  ruleType: string
  ruleName: string
}

export interface GateCheckResult {
  passed: boolean
  result: string
  message: string
  productCode: string
  stationCode: string
  stationName: string
  barcode: string
  nextStationCode: string | null
  nextStationName: string | null
  matchedRules: GateCheckMatchedRule[]
}

export const check = (data: GateCheckRequest) =>
  http.post<GateCheckResult>('/GateCheck/check', data)
