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
  /** 后端地址（不含 /api）：留空走同域名 nginx 代理，填 http://host:port 直连后端 */
  readonly VITE_API_BASE_URL: string
  /** 部署子路径：开发 / ，生产 /mes/ */
  readonly VITE_BASE_URL: string
  /** 是否为生产环境 */
  readonly VITE_PROD: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}