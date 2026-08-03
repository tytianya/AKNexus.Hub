/**
 * 应用入口
 */
import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import pinia from './store'

// Element Plus
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'

// 全局样式
import './styles/index.scss'

// 模块注册器
import { createModuleRegistrar } from './modules'

// DAS 模块
import { dasModule } from './modules/das'

// 系统管理模块
import { systemModule } from './modules/system'

/**
 * 应用启动
 * 模块加载完成后才挂载应用，确保路由与菜单已注册
 */
async function bootstrap() {
  // 创建应用实例
  const app = createApp(App)

  // 注册 Element Plus
  app.use(ElementPlus)

  // 注册 Element Plus 图标
  for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
    app.component(key, component)
  }

  // 注册状态管理
  app.use(pinia)

  // 创建模块注册器
  const moduleRegistrar = createModuleRegistrar({
    autoLoad: true,
    strictDependencies: true
  })

  // 注册 DAS 模块
  moduleRegistrar.register(dasModule)

  // 注册系统管理模块（超级管理员菜单配置）
  moduleRegistrar.register(systemModule)

  // 加载所有模块（注册路由与菜单），必须在 app.use(router) 之前完成，
  // 否则动态添加的路由在初始导航时还未注册，F5 刷新会命中 404
  await moduleRegistrar.loadAll(app, router)

  // 注册路由（此时所有模块路由已注册，初始导航能正确匹配）
  app.use(router)

  // 挂载应用
  app.mount('#app')
}

bootstrap()
