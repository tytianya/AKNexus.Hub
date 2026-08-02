/**
 * 应用状态
 */
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAppStore = defineStore('app', () => {
  /** 侧边栏是否折叠 */
  const sidebarCollapsed = ref(false)

  /** 是否显示设置面板 */
  const showSettings = ref(false)

  /** 设备类型 */
  const device = ref<'desktop' | 'mobile'>('desktop')

  /** 切换侧边栏 */
  const toggleSidebar = () => {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }

  /** 设置设备类型 */
  const setDevice = (value: 'desktop' | 'mobile') => {
    device.value = value
  }

  /** 显示设置面板 */
  const openSettings = () => {
    showSettings.value = true
  }

  /** 隐藏设置面板 */
  const closeSettings = () => {
    showSettings.value = false
  }

  return {
    sidebarCollapsed,
    showSettings,
    device,
    toggleSidebar,
    setDevice,
    openSettings,
    closeSettings
  }
})