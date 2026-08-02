/**
 * 认证状态管理
 * 预留登录设计体系
 */
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { UserInfo, AuthState, LoginParams, LoginResult } from './types'

/** 默认认证配置 */
export const defaultAuthConfig = {
  loginPath: '/login',
  homePath: '/dashboard',
  tokenKey: 'aknexus_access_token',
  refreshTokenKey: 'aknexus_refresh_token',
  tokenExpiresIn: 7200,
  autoRefreshToken: true,
  refreshBeforeExpires: 300
}

export const useAuthStore = defineStore('auth', () => {
  /** 用户信息 */
  const user = ref<UserInfo | null>(null)

  /** 访问令牌 */
  const accessToken = ref<string | null>(null)

  /** 刷新令牌 */
  const refreshToken = ref<string | null>(null)

  /** 是否已登录 */
  const isLoggedIn = computed(() => !!accessToken.value && !!user.value)

  /**
   * 登录
   * @param params 登录参数
   * @returns 登录结果
   */
  const login = async (params: LoginParams): Promise<LoginResult> => {
    // TODO: 调用登录 API
    // const result = await authApi.login(params)

    // 模拟登录结果
    const result: LoginResult = {
      accessToken: 'mock_access_token',
      refreshToken: 'mock_refresh_token',
      expiresIn: 7200,
      user: {
        id: '1',
        username: params.username,
        nickname: '管理员',
        roles: ['admin'],
        permissions: ['*']
      }
    }

    // 保存状态
    user.value = result.user
    accessToken.value = result.accessToken
    refreshToken.value = result.refreshToken || null

    // 持久化到 localStorage
    localStorage.setItem(defaultAuthConfig.tokenKey, result.accessToken)
    if (result.refreshToken) {
      localStorage.setItem(defaultAuthConfig.refreshTokenKey, result.refreshToken)
    }

    return result
  }

  /**
   * 登出
   */
  const logout = async () => {
    // TODO: 调用登出 API
    // await authApi.logout()

    // 清除状态
    user.value = null
    accessToken.value = null
    refreshToken.value = null

    // 清除 localStorage
    localStorage.removeItem(defaultAuthConfig.tokenKey)
    localStorage.removeItem(defaultAuthConfig.refreshTokenKey)
  }

  /**
   * 刷新令牌
   */
  const refreshAccessToken = async (): Promise<void> => {
    if (!refreshToken.value) return

    // TODO: 调用刷新令牌 API
    // const result = await authApi.refreshToken(refreshToken.value)
    // accessToken.value = result.accessToken
  }

  /**
   * 从 localStorage 恢复登录状态
   */
  const restoreAuth = () => {
    const token = localStorage.getItem(defaultAuthConfig.tokenKey)
    const refresh = localStorage.getItem(defaultAuthConfig.refreshTokenKey)

    if (token) {
      accessToken.value = token
      refreshToken.value = refresh
      // TODO: 获取用户信息
      // await getUserInfo()
    }
  }

  /**
   * 检查权限
   * @param permission 权限标识
   */
  const hasPermission = (permission: string): boolean => {
    if (!user.value) return false
    if (user.value.permissions.includes('*')) return true
    return user.value.permissions.includes(permission)
  }

  /**
   * 检查角色
   * @param role 角色标识
   */
  const hasRole = (role: string): boolean => {
    if (!user.value) return false
    return user.value.roles.includes(role)
  }

  return {
    user,
    accessToken,
    refreshToken,
    isLoggedIn,
    login,
    logout,
    refreshAccessToken,
    restoreAuth,
    hasPermission,
    hasRole
  }
})