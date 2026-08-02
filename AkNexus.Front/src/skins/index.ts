/**
 * 皮肤系统核心逻辑
 * 负责将设计变量应用到 CSS 变量，管理皮肤状态
 */

import { ref, watch, computed, type Ref, type ComputedRef } from 'vue'
import type { SkinPreset, SkinConfig, DesignTokens, SkinChangeEvent } from './types'
import { skinPresets, DEFAULT_SKIN_ID, getSkinPreset } from './presets'

/** CSS 变量前缀 */
const CSS_VAR_PREFIX = '--ds-'

/** localStorage 存储键 */
const STORAGE_KEY = 'aknexus-skin-config'

/** 当前激活的皮肤 */
const activeSkin: Ref<SkinPreset | null> = ref(null)

/** 皮肤配置 */
const skinConfig: Ref<SkinConfig> = ref({
  activeId: DEFAULT_SKIN_ID,
  followSystem: false,
  persistent: true
})

/** 皮肤变更监听器 */
const skinChangeListeners: Set<(event: SkinChangeEvent) => void> = new Set()

/**
 * 将设计变量扁平化为 CSS 变量映射
 */
function flattenTokens(tokens: DesignTokens): Record<string, string> {
  const result: Record<string, string> = {}

  // 主色系
  result['primary'] = tokens.color.primary
  result['primary-hover'] = tokens.color.primaryHover
  result['primary-bg'] = tokens.color.primaryBg
  result['primary-border'] = tokens.color.primaryBorder

  // 状态色
  result['success-text'] = tokens.color.success.text
  result['success-bg'] = tokens.color.success.bg
  result['success-border'] = tokens.color.success.border
  result['warning-text'] = tokens.color.warning.text
  result['warning-bg'] = tokens.color.warning.bg
  result['warning-border'] = tokens.color.warning.border
  result['danger-text'] = tokens.color.danger.text
  result['danger-bg'] = tokens.color.danger.bg
  result['danger-border'] = tokens.color.danger.border
  result['info-text'] = tokens.color.info.text
  result['info-bg'] = tokens.color.info.bg
  result['info-border'] = tokens.color.info.border

  // 文字
  result['text-primary'] = tokens.text.primary
  result['text-secondary'] = tokens.text.secondary
  result['text-tertiary'] = tokens.text.tertiary
  result['text-placeholder'] = tokens.text.placeholder
  result['text-disabled'] = tokens.text.disabled
  result['text-title'] = tokens.text.title
  result['text-inverse'] = tokens.text.inverse

  // 背景
  result['bg-base'] = tokens.bg.base
  result['bg-content'] = tokens.bg.content
  result['bg-panel'] = tokens.bg.panel
  result['bg-header'] = tokens.bg.header
  result['bg-hover'] = tokens.bg.hover
  result['bg-overlay'] = tokens.bg.overlay
  result['bg-dialog'] = tokens.bg.dialog

  // 边框
  result['border-light'] = tokens.border.light
  result['border-medium'] = tokens.border.medium

  // 阴影
  result['shadow-card'] = tokens.shadow.card
  result['shadow-dropdown'] = tokens.shadow.dropdown
  result['shadow-dialog'] = tokens.shadow.dialog

  // 尺寸
  result['table-font-size'] = tokens.size.tableFontSize
  result['table-header-font-size'] = tokens.size.tableHeaderFontSize
  result['input-height'] = tokens.size.inputHeight
  result['select-height'] = tokens.size.selectHeight
  result['form-item-gap'] = tokens.size.formItemGap
  result['card-padding'] = tokens.size.cardPadding
  result['card-header-padding'] = tokens.size.cardHeaderPadding
  result['card-radius'] = tokens.size.cardRadius
  result['dialog-header-padding'] = tokens.size.dialogHeaderPadding
  result['dialog-header-height'] = tokens.size.dialogHeaderHeight
  result['dialog-body-padding'] = tokens.size.dialogBodyPadding
  result['dialog-footer-padding'] = tokens.size.dialogFooterPadding
  result['button-padding'] = tokens.size.buttonPadding

  // 菜单栏
  result['menu-bar-bg'] = tokens.menuBar.bg
  result['menu-bar-text'] = tokens.menuBar.text
  result['menu-bar-hover-bg'] = tokens.menuBar.hoverBg
  result['menu-bar-active-bg'] = tokens.menuBar.activeBg
  result['menu-bar-active-text'] = tokens.menuBar.activeText

  // 工具栏
  result['toolbar-bg'] = tokens.toolbar.bg
  result['toolbar-text'] = tokens.toolbar.text
  result['toolbar-hover-bg'] = tokens.toolbar.hoverBg
  result['toolbar-border-color'] = tokens.toolbar.borderColor

  return result
}

/**
 * 应用皮肤到 CSS 变量
 */
function applySkinToDom(skin: SkinPreset): void {
  const root = document.documentElement
  const flatTokens = flattenTokens(skin.tokens)

  // 设置 CSS 变量
  for (const [key, value] of Object.entries(flatTokens)) {
    root.style.setProperty(`${CSS_VAR_PREFIX}${key}`, value)
  }

  // 设置 html 的 class（用于 Tailwind 或其他 CSS 框架的深色模式）
  if (skin.isDark) {
    root.classList.add('dark')
    root.setAttribute('data-theme', 'dark')
  } else {
    root.classList.remove('dark')
    root.setAttribute('data-theme', 'light')
  }
}

/**
 * 保存配置到 localStorage
 */
function saveConfig(): void {
  if (skinConfig.value.persistent) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(skinConfig.value))
  }
}

/**
 * 从 localStorage 加载配置
 */
function loadConfig(): SkinConfig | null {
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored) {
      return JSON.parse(stored) as SkinConfig
    }
  } catch {
    // 忽略解析错误
  }
  return null
}

/**
 * 监听系统深色模式变化
 */
function setupSystemDarkModeListener(): void {
  const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')

  const handleChange = (e: MediaQueryListEvent | MediaQueryList) => {
    if (skinConfig.value.followSystem) {
      const skinId = e.matches ? 'dark' : 'light'
      setSkin(skinId, 'system')
    }
  }

  // 初始检查
  if (skinConfig.value.followSystem) {
    handleChange(mediaQuery)
  }

  // 监听变化
  mediaQuery.addEventListener('change', handleChange)
}

/**
 * 初始化皮肤系统
 */
export function initSkinSystem(): void {
  // 从 localStorage 加载配置
  const savedConfig = loadConfig()
  if (savedConfig) {
    skinConfig.value = savedConfig
  }

  // 设置初始皮肤
  const initialId = skinConfig.value.followSystem
    ? (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light')
    : skinConfig.value.activeId

  const initialSkin = getSkinPreset(initialId)
  if (initialSkin) {
    activeSkin.value = initialSkin
    applySkinToDom(initialSkin)
  }

  // 设置系统深色模式监听
  setupSystemDarkModeListener()
}

/**
 * 设置当前皮肤
 */
export function setSkin(skinId: string, source: 'user' | 'system' | 'init' = 'user'): void {
  const skin = getSkinPreset(skinId)
  if (!skin) {
    console.warn(`Skin preset "${skinId}" not found`)
    return
  }

  const oldSkin = activeSkin.value
  activeSkin.value = skin
  skinConfig.value.activeId = skinId

  // 应用到 DOM
  applySkinToDom(skin)

  // 保存配置
  saveConfig()

  // 触发变更事件
  const event: SkinChangeEvent = { oldSkin, newSkin: skin, source }
  skinChangeListeners.forEach(listener => listener(event))
}

/**
 * 切换深色/浅色模式
 */
export function toggleDarkMode(): void {
  const newId = activeSkin.value?.isDark ? 'light' : 'dark'
  setSkin(newId, 'user')
}

/**
 * 设置是否跟随系统
 */
export function setFollowSystem(follow: boolean): void {
  skinConfig.value.followSystem = follow
  saveConfig()

  if (follow) {
    const isDark = window.matchMedia('(prefers-color-scheme: dark)').matches
    setSkin(isDark ? 'dark' : 'light', 'system')
  }
}

/**
 * 获取当前皮肤
 */
export function getActiveSkin(): ComputedRef<SkinPreset | null> {
  return computed(() => activeSkin.value)
}

/**
 * 获取当前皮肤配置
 */
export function getSkinConfig(): ComputedRef<SkinConfig> {
  return computed(() => skinConfig.value)
}

/**
 * 监听皮肤变更
 */
export function onSkinChange(listener: (event: SkinChangeEvent) => void): () => void {
  skinChangeListeners.add(listener)
  return () => skinChangeListeners.delete(listener)
}

/**
 * 判断当前是否为深色模式
 */
export function isDarkMode(): ComputedRef<boolean> {
  return computed(() => activeSkin.value?.isDark ?? false)
}

// 导出类型和预设
export * from './types'
export { skinPresets, DEFAULT_SKIN_ID, getSkinPreset, getAllSkinPresets } from './presets'