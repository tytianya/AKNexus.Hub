/// <reference types="vite/client" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<{}, {}, any>
  export default component
}

/** 环境变量类型 */
interface ImportMetaEnv {
  /** 应用标题 */
  readonly VITE_APP_TITLE: string
  /** API 基础路径 */
  readonly VITE_API_BASE_URL: string
  /** 是否为生产环境 */
  readonly VITE_PROD: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}