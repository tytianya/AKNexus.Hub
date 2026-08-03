/**
 * 路由配置
 */
import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import Layout from '/@/layouts/index.vue'

/** 基础路由 */
export const baseRoutes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'Layout',
    component: Layout,
    redirect: '/dashboard',
    children: [
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('/@/views/dashboard/index.vue'),
        meta: {
          title: '仪表盘',
          icon: 'Odometer'
        }
      }
    ]
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: () => import('/@/views/error/404.vue'),
    meta: {
      title: '页面不存在'
    }
  }
]

/** 创建路由实例 */
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: baseRoutes,
  scrollBehavior: () => ({ left: 0, top: 0 })
})

export default router