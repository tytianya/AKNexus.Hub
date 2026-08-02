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

// 创建应用实例
const app = createApp(App)

// 注册 Element Plus
app.use(ElementPlus)

// 注册 Element Plus 图标
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

// 注册路由
app.use(router)

// 注册状态管理
app.use(pinia)

// 创建模块注册器
const moduleRegistrar = createModuleRegistrar({
  autoLoad: true,
  strictDependencies: true
})

// 加载所有模块（预留，后续通过模块自注册）
// await moduleRegistrar.loadAll(app, router)

// 挂载应用
app.mount('#app')