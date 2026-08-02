/**
 * 模块系统类型定义
 * 组件式核心管理，支持模块自注册
 */

import type { RouteRecordRaw, Router } from 'vue-router'
import type { App, Plugin } from 'vue'

/** 模块元数据 */
export interface ModuleMeta {
  /** 模块ID（唯一标识） */
  id: string
  /** 模块名称 */
  name: string
  /** 模块版本 */
  version?: string
  /** 模块描述 */
  description?: string
  /** 模块作者 */
  author?: string
  /** 模块图标 */
  icon?: string
  /** 模块排序（数字越小越靠前） */
  order?: number
  /** 是否启用 */
  enabled?: boolean
  /** 依赖的其他模块ID */
  dependencies?: string[]
}

/** 模块路由配置 */
export interface ModuleRouteConfig {
  /** 路由记录 */
  routes: RouteRecordRaw[]
  /** 是否在菜单中隐藏 */
  hideInMenu?: boolean
  /** 父级菜单路径 */
  parentPath?: string
}

/** 模块菜单项 */
export interface ModuleMenuItem {
  /** 菜单ID */
  id: string
  /** 菜单标题 */
  title: string
  /** 路由路径 */
  path: string
  /** 图标 */
  icon?: string
  /** 排序 */
  order?: number
  /** 是否在菜单中隐藏 */
  hideInMenu?: boolean
  /** 子菜单 */
  children?: ModuleMenuItem[]
}

/** 模块上下文 - 提供给模块的运行环境 */
export interface ModuleContext {
  /** Vue 应用实例 */
  app: App
  /** 路由实例 */
  router: Router
  /** 模块元数据 */
  meta: ModuleMeta
  /** 注册路由 */
  registerRoutes: (config: ModuleRouteConfig) => void
  /** 注册菜单 */
  registerMenu: (menu: ModuleMenuItem) => void
  /** 注册组件 */
  registerComponent: (name: string, component: any) => void
  /** 注册插件 */
  registerPlugin: (plugin: Plugin, options?: any) => void
  /** 获取其他模块 */
  getModule: (id: string) => Module | undefined
  /** 获取配置 */
  getConfig: <T = any>(key: string, defaultValue?: T) => T
  /** 设置配置 */
  setConfig: (key: string, value: any) => void
}

/** 模块生命周期钩子 */
export interface ModuleLifecycleHooks {
  /** 模块加载前 */
  beforeLoad?: (context: ModuleContext) => void | Promise<void>
  /** 模块加载后 */
  afterLoad?: (context: ModuleContext) => void | Promise<void>
  /** 模块卸载前 */
  beforeUnload?: (context: ModuleContext) => void | Promise<void>
  /** 模块卸载后 */
  afterUnload?: (context: ModuleContext) => void | Promise<void>
}

/** 模块接口 */
export interface Module {
  /** 模块元数据 */
  meta: ModuleMeta
  /** 路由配置 */
  routes?: ModuleRouteConfig
  /** 菜单配置 */
  menus?: ModuleMenuItem[]
  /** Vue 插件（可选） */
  plugin?: Plugin
  /** 生命周期钩子 */
  lifecycle?: ModuleLifecycleHooks
  /** 安装函数 */
  install?: (context: ModuleContext) => void | Promise<void>
  /** 卸载函数 */
  uninstall?: (context: ModuleContext) => void | Promise<void>
}

/** 模块注册器接口 */
export interface ModuleRegistrar {
  /** 注册模块 */
  register(module: Module): void
  /** 注销模块 */
  unregister(moduleId: string): void
  /** 获取模块 */
  getModule(id: string): Module | undefined
  /** 获取所有模块 */
  getAllModules(): Module[]
  /** 加载所有模块 */
  loadAll(app: App, router: Router): Promise<void>
  /** 卸载所有模块 */
  unloadAll(): Promise<void>
}

/** 模块配置 */
export interface ModuleConfig {
  /** 是否自动加载 */
  autoLoad?: boolean
  /** 是否严格检查依赖 */
  strictDependencies?: boolean
  /** 模块加载超时时间（毫秒） */
  loadTimeout?: number
}