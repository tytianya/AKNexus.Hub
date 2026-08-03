/**
 * 系统管理模块
 * 提供菜单配置等系统级功能，仅超级管理员可见
 */
import type { RouteRecordRaw } from 'vue-router'
import type { Module } from '../types'

export const systemModule: Module = {
  meta: {
    id: 'system',
    name: '系统管理',
    version: '1.0.0',
    description: '系统管理模块（菜单配置等）',
    order: 90
  },
  install: (context) => {
    // 注册路由为 Layout 子路由
    const routes: RouteRecordRaw[] = [
      {
        path: '/system/menu',
        name: 'SystemMenu',
        component: () => import('/@/views/system/menu/index.vue'),
        meta: { title: '菜单配置', icon: 'Menu' }
      }
    ]
    routes.forEach((route) => context.router.addRoute('Layout', route))

    // 注册一级菜单
    context.registerMenu({
      id: 'system-menu',
      title: '菜单配置',
      path: '/system/menu',
      icon: 'Menu',
      order: 100
    })
  }
}

export default systemModule
