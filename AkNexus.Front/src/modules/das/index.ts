/**
 * DAS 模块 - MES 产品卡关管理系统
 * 注册 9 个 MES 业务页面路由与一级菜单
 *
 * 菜单采用 admin.net 风格的一级目录布局：
 * 顶层菜单项可直接点击打开页面（无折叠子菜单）
 */
import type { RouteRecordRaw } from 'vue-router'
import type { Module } from '../types'

/** DAS 业务页面配置项 */
interface DasPageConfig {
  /** 路由路径（绝对路径，作为 Layout 子路由） */
  path: string
  /** 路由名称 */
  name: string
  /** 页面组件懒加载 */
  component: () => Promise<any>
  /** 页面标题 */
  title: string
  /** 菜单图标（Element Plus 图标名） */
  icon: string
  /** 菜单 ID */
  menuId: string
  /** 排序（数字越小越靠前） */
  order: number
}

/** DAS 页面清单（每项对应一个一级菜单与路由） */
const dasPages: DasPageConfig[] = [
  { path: '/das/flow', name: 'DasFlow', component: () => import('/@/views/das/flow/index.vue'), title: '流程配置', icon: 'Share', menuId: 'das-flow', order: 10 },
  { path: '/das/check', name: 'DasCheck', component: () => import('/@/views/das/check/index.vue'), title: '过站检查', icon: 'CircleCheck', menuId: 'das-check', order: 20 },
  { path: '/das/products', name: 'DasProducts', component: () => import('/@/views/das/products/index.vue'), title: '产品管理', icon: 'Box', menuId: 'das-products', order: 30 },
  { path: '/das/rules', name: 'DasRules', component: () => import('/@/views/das/rules/index.vue'), title: '卡关规则', icon: 'Shield', menuId: 'das-rules', order: 40 },
  { path: '/das/query', name: 'DasQuery', component: () => import('/@/views/das/query/index.vue'), title: '记录查询', icon: 'Search', menuId: 'das-query', order: 50 },
  { path: '/das/stations', name: 'DasStations', component: () => import('/@/views/das/stations/index.vue'), title: '站点管理', icon: 'OfficeBuilding', menuId: 'das-stations', order: 60 },
  { path: '/das/mac', name: 'DasMac', component: () => import('/@/views/das/mac/index.vue'), title: 'MAC管理', icon: 'Connection', menuId: 'das-mac', order: 70 },
  { path: '/das/software', name: 'DasSoftware', component: () => import('/@/views/das/software/index.vue'), title: '测试软件', icon: 'Files', menuId: 'das-software', order: 80 },
  { path: '/das/incoming', name: 'DasIncoming', component: () => import('/@/views/das/incoming/index.vue'), title: '来料检测', icon: 'Goods', menuId: 'das-incoming', order: 90 },
  { path: '/das/business-log', name: 'DasBusinessLog', component: () => import('/@/views/das/businessLog/index.vue'), title: '业务日志', icon: 'Document', menuId: 'das-business-log', order: 100 }
]

/** DAS 模块定义 */
export const dasModule: Module = {
  meta: {
    id: 'das',
    name: 'DAS MES系统',
    version: '1.0.0',
    description: 'MES 产品卡关管理系统',
    order: 10
  },
  install: (context) => {
    // 将路由注册为 Layout 的子路由，保持侧边栏布局一致
    const routes: RouteRecordRaw[] = dasPages.map((page) => ({
      path: page.path,
      name: page.name,
      component: page.component,
      meta: {
        title: page.title,
        icon: page.icon
      }
    }))
    routes.forEach((route) => context.router.addRoute('Layout', route))

    // 注册一级菜单（直接可点击，无折叠子菜单）
    dasPages.forEach((page) => {
      context.registerMenu({
        id: page.menuId,
        title: page.title,
        path: page.path,
        icon: page.icon,
        order: page.order
      })
    })
  }
}

export default dasModule
