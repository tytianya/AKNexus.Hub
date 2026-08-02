/**
 * 模块注册器核心实现
 * 支持模块自注册、依赖检查、生命周期管理
 */

import type { App, Plugin } from 'vue'
import type { Router, RouteRecordRaw } from 'vue-router'
import type {
  Module,
  ModuleRegistrar,
  ModuleContext,
  ModuleMenuItem,
  ModuleRouteConfig,
  ModuleConfig
} from './types'

/** 全局模块存储 */
const moduleRegistry: Map<string, Module> = new Map()

/** 全局配置存储 */
const globalConfig: Map<string, any> = new Map()

/** 全局菜单存储 */
const globalMenus: ModuleMenuItem[] = []

/** 模块注册器实例 */
let registrarInstance: ModuleRegistrar | null = null

/**
 * 创建模块上下文
 */
function createModuleContext(app: App, router: Router, meta: Module['meta']): ModuleContext {
  return {
    app,
    router,
    meta,
    registerRoutes: (config: ModuleRouteConfig) => {
      if (config.routes && config.routes.length > 0) {
        config.routes.forEach(route => {
          router.addRoute(route)
        })
      }
    },
    registerMenu: (menu: ModuleMenuItem) => {
      globalMenus.push(menu)
    },
    registerComponent: (name: string, component: any) => {
      app.component(name, component)
    },
    registerPlugin: (plugin: Plugin, options?: any) => {
      app.use(plugin, options)
    },
    getModule: (id: string) => moduleRegistry.get(id),
    getConfig: <T = any>(key: string, defaultValue?: T): T => {
      return globalConfig.has(key) ? globalConfig.get(key) : defaultValue
    },
    setConfig: (key: string, value: any) => {
      globalConfig.set(key, value)
    }
  }
}

/**
 * 检查模块依赖
 */
function checkDependencies(module: Module, strict: boolean): boolean {
  const deps = module.meta.dependencies
  if (!deps || deps.length === 0) return true

  for (const depId of deps) {
    if (!moduleRegistry.has(depId)) {
      if (strict) {
        throw new Error(`Module "${module.meta.id}" depends on "${depId}" which is not registered`)
      }
      console.warn(`Module "${module.meta.id}" depends on "${depId}" which is not registered`)
      return false
    }
  }
  return true
}

/**
 * 按依赖关系排序模块
 */
function sortModulesByDependencies(modules: Module[]): Module[] {
  const sorted: Module[] = []
  const visited = new Set<string>()
  const visiting = new Set<string>()

  function visit(module: Module) {
    const id = module.meta.id
    if (visited.has(id)) return
    if (visiting.has(id)) {
      throw new Error(`Circular dependency detected: ${id}`)
    }

    visiting.add(id)

    const deps = module.meta.dependencies || []
    for (const depId of deps) {
      const dep = moduleRegistry.get(depId)
      if (dep) visit(dep)
    }

    visiting.delete(id)
    visited.add(id)
    sorted.push(module)
  }

  modules.forEach(module => visit(module))
  return sorted
}

/**
 * 创建模块注册器
 */
export function createModuleRegistrar(config?: ModuleConfig): ModuleRegistrar {
  const _config: ModuleConfig = {
    autoLoad: true,
    strictDependencies: true,
    loadTimeout: 10000,
    ...config
  }

  const registrar: ModuleRegistrar = {
    register(module: Module) {
      if (moduleRegistry.has(module.meta.id)) {
        console.warn(`Module "${module.meta.id}" is already registered. Skipping.`)
        return
      }

      // 检查是否启用（默认启用）
      if (module.meta.enabled === false) {
        console.log(`Module "${module.meta.id}" is disabled. Skipping.`)
        return
      }

      moduleRegistry.set(module.meta.id, module)
      console.log(`Module "${module.meta.id}" registered.`)
    },

    unregister(moduleId: string) {
      const module = moduleRegistry.get(moduleId)
      if (!module) return

      moduleRegistry.delete(moduleId)
      console.log(`Module "${moduleId}" unregistered.`)
    },

    getModule(id: string) {
      return moduleRegistry.get(id)
    },

    getAllModules() {
      return Array.from(moduleRegistry.values())
    },

    async loadAll(app: App, router: Router) {
      const modules = Array.from(moduleRegistry.values())

      // 检查依赖
      modules.forEach(module => checkDependencies(module, _config.strictDependencies!))

      // 按依赖排序
      const sortedModules = sortModulesByDependencies(modules)

      // 加载模块
      for (const module of sortedModules) {
        const context = createModuleContext(app, router, module.meta)

        try {
          // 执行生命周期钩子
          if (module.lifecycle?.beforeLoad) {
            await module.lifecycle.beforeLoad(context)
          }

          // 注册路由
          if (module.routes) {
            context.registerRoutes(module.routes)
          }

          // 注册菜单
          if (module.menus) {
            module.menus.forEach(menu => context.registerMenu(menu))
          }

          // 注册插件
          if (module.plugin) {
            app.use(module.plugin)
          }

          // 执行安装函数
          if (module.install) {
            await module.install(context)
          }

          // 执行生命周期钩子
          if (module.lifecycle?.afterLoad) {
            await module.lifecycle.afterLoad(context)
          }

          console.log(`Module "${module.meta.id}" loaded.`)
        } catch (error) {
          console.error(`Failed to load module "${module.meta.id}":`, error)
          throw error
        }
      }
    },

    async unloadAll() {
      const modules = Array.from(moduleRegistry.values()).reverse()

      for (const module of modules) {
        const context = createModuleContext({} as App, {} as Router, module.meta)

        try {
          if (module.lifecycle?.beforeUnload) {
            await module.lifecycle.beforeUnload(context)
          }

          if (module.uninstall) {
            await module.uninstall(context)
          }

          if (module.lifecycle?.afterUnload) {
            await module.lifecycle.afterUnload(context)
          }

          console.log(`Module "${module.meta.id}" unloaded.`)
        } catch (error) {
          console.error(`Failed to unload module "${module.meta.id}":`, error)
        }
      }

      moduleRegistry.clear()
      globalMenus.length = 0
    }
  }

  registrarInstance = registrar
  return registrar
}

/**
 * 获取模块注册器实例
 */
export function getModuleRegistrar(): ModuleRegistrar {
  if (!registrarInstance) {
    throw new Error('Module registrar not initialized. Call createModuleRegistrar first.')
  }
  return registrarInstance
}

/**
 * 注册模块（便捷方法）
 */
export function registerModule(module: Module): void {
  getModuleRegistrar().register(module)
}

/**
 * 获取全局菜单
 */
export function getGlobalMenus(): ModuleMenuItem[] {
  return [...globalMenus]
}

/**
 * 设置全局配置
 */
export function setGlobalConfig(key: string, value: any): void {
  globalConfig.set(key, value)
}

/**
 * 获取全局配置
 */
export function getGlobalConfig<T = any>(key: string, defaultValue?: T): T {
  return globalConfig.has(key) ? globalConfig.get(key) : defaultValue
}

// 导出类型
export * from './types'