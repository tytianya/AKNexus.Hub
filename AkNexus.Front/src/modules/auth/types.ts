/**
 * 认证模块类型定义
 * 预留登录设计体系
 */

/** 用户信息 */
export interface UserInfo {
  /** 用户ID */
  id: string
  /** 用户名 */
  username: string
  /** 昵称 */
  nickname?: string
  /** 邮箱 */
  email?: string
  /** 手机号 */
  phone?: string
  /** 头像 */
  avatar?: string
  /** 角色列表 */
  roles: string[]
  /** 权限列表 */
  permissions: string[]
}

/** 登录参数 */
export interface LoginParams {
  /** 用户名 */
  username: string
  /** 密码 */
  password: string
  /** 验证码（可选） */
  captcha?: string
  /** 记住我 */
  rememberMe?: boolean
}

/** 登录响应 */
export interface LoginResult {
  /** 访问令牌 */
  accessToken: string
  /** 刷新令牌 */
  refreshToken?: string
  /** 过期时间（秒） */
  expiresIn?: number
  /** 用户信息 */
  user: UserInfo
}

/** 认证状态 */
export interface AuthState {
  /** 是否已登录 */
  isLoggedIn: boolean
  /** 用户信息 */
  user: UserInfo | null
  /** 访问令牌 */
  accessToken: string | null
  /** 刷新令牌 */
  refreshToken: string | null
}

/** 超级管理员角色标识 */
export const SUPER_ADMIN_ROLE = 'superadmin'

/** 认证配置 */
export interface AuthConfig {
  /** 登录页面路径 */
  loginPath: string
  /** 登录成功后跳转路径 */
  homePath: string
  /** 令牌存储键名 */
  tokenKey: string
  /** 刷新令牌存储键名 */
  refreshTokenKey: string
  /** 令牌过期时间（秒） */
  tokenExpiresIn: number
  /** 是否自动刷新令牌 */
  autoRefreshToken: boolean
  /** 令牌刷新提前时间（秒） */
  refreshBeforeExpires: number
}