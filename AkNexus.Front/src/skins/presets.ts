/**
 * 皮肤预设定义
 * 参考工业软件设计规范，提供浅色和深色两种主题
 */

import type { SkinPreset, DesignTokens } from './types'

/** 浅色主题设计变量 */
const lightTokens: DesignTokens = {
  mode: 'light',
  color: {
    primary: '#3B82F6',
    primaryHover: '#2563EB',
    primaryBg: '#EFF6FF',
    primaryBorder: '#BFDBFE',
    success: {
      text: '#059669',
      bg: '#ECFDF5',
      border: '#A7F3D0'
    },
    warning: {
      text: '#D97706',
      bg: '#FFFBEB',
      border: '#FDE68A'
    },
    danger: {
      text: '#DC2626',
      bg: '#FEF2F2',
      border: '#FECACA'
    },
    info: {
      text: '#4B5563',
      bg: '#F9FAFB',
      border: '#E5E7EB'
    }
  },
  text: {
    primary: '#1F2937',
    secondary: '#4B5563',
    tertiary: '#9CA3AF',
    placeholder: '#9CA3AF',
    disabled: '#D1D5DB',
    title: '#111827',
    inverse: '#FFFFFF'
  },
  bg: {
    base: '#F3F4F6',
    content: '#FFFFFF',
    panel: '#FFFFFF',
    header: '#F9FAFB',
    hover: '#F3F4F6',
    overlay: 'rgba(0, 0, 0, 0.5)',
    dialog: '#FFFFFF'
  },
  border: {
    light: '#E5E7EB',
    medium: '#D1D5DB'
  },
  shadow: {
    card: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)',
    dropdown: '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
    dialog: '0 25px 50px -12px rgba(0, 0, 0, 0.25)'
  },
  size: {
    tableFontSize: '14px',
    tableHeaderFontSize: '13px',
    inputHeight: '32px',
    selectHeight: '32px',
    formItemGap: '18px',
    cardPadding: '16px',
    cardHeaderPadding: '12px 16px',
    cardRadius: '4px',
    dialogHeaderPadding: '12px 16px',
    dialogHeaderHeight: '44px',
    dialogBodyPadding: '16px',
    dialogFooterPadding: '12px 16px',
    buttonPadding: '7px 15px'
  },
  menuBar: {
    bg: '#FFFFFF',
    text: '#4B5563',
    hoverBg: '#F3F4F6',
    activeBg: '#EFF6FF',
    activeText: '#3B82F6'
  },
  toolbar: {
    bg: '#FFFFFF',
    text: '#4B5563',
    hoverBg: '#F3F4F6',
    borderColor: '#E5E7EB'
  }
}

/** 深色主题设计变量 */
const darkTokens: DesignTokens = {
  mode: 'dark',
  color: {
    primary: '#3B82F6',
    primaryHover: '#60A5FA',
    primaryBg: '#1E3A5F',
    primaryBorder: '#2563EB',
    success: {
      text: '#34D399',
      bg: '#064E3B',
      border: '#059669'
    },
    warning: {
      text: '#FBBF24',
      bg: '#78350F',
      border: '#D97706'
    },
    danger: {
      text: '#F87171',
      bg: '#7F1D1D',
      border: '#DC2626'
    },
    info: {
      text: '#9CA3AF',
      bg: '#374151',
      border: '#4B5563'
    }
  },
  text: {
    primary: '#E5E7EB',
    secondary: '#9CA3AF',
    tertiary: '#6B7280',
    placeholder: '#6B7280',
    disabled: '#4B5563',
    title: '#F3F4F6',
    inverse: '#111827'
  },
  bg: {
    base: '#0F172A',
    content: '#1E293B',
    panel: '#1E293B',
    header: '#1E293B',
    hover: '#334155',
    overlay: 'rgba(0, 0, 0, 0.7)',
    dialog: '#1E293B'
  },
  border: {
    light: '#334155',
    medium: '#475569'
  },
  shadow: {
    card: '0 4px 6px -1px rgba(0, 0, 0, 0.3), 0 2px 4px -1px rgba(0, 0, 0, 0.2)',
    dropdown: '0 10px 15px -3px rgba(0, 0, 0, 0.4), 0 4px 6px -2px rgba(0, 0, 0, 0.3)',
    dialog: '0 25px 50px -12px rgba(0, 0, 0, 0.5)'
  },
  size: {
    tableFontSize: '14px',
    tableHeaderFontSize: '13px',
    inputHeight: '32px',
    selectHeight: '32px',
    formItemGap: '18px',
    cardPadding: '16px',
    cardHeaderPadding: '12px 16px',
    cardRadius: '4px',
    dialogHeaderPadding: '12px 16px',
    dialogHeaderHeight: '44px',
    dialogBodyPadding: '16px',
    dialogFooterPadding: '12px 16px',
    buttonPadding: '7px 15px'
  },
  menuBar: {
    bg: '#111827',
    text: '#9CA3AF',
    hoverBg: '#1F2937',
    activeBg: '#1E3A5F',
    activeText: '#60A5FA'
  },
  toolbar: {
    bg: '#1E293B',
    text: '#9CA3AF',
    hoverBg: '#334155',
    borderColor: '#334155'
  }
}

/** 预设皮肤列表 */
export const skinPresets: SkinPreset[] = [
  {
    id: 'light',
    name: '浅色主题',
    description: '经典的浅色工业软件界面',
    isDark: false,
    tokens: lightTokens
  },
  {
    id: 'dark',
    name: '深色主题',
    description: '深色界面，适合长时间工作',
    isDark: true,
    tokens: darkTokens
  }
]

/** 默认皮肤ID */
export const DEFAULT_SKIN_ID = 'light'

/** 获取皮肤预设 */
export function getSkinPreset(id: string): SkinPreset | undefined {
  return skinPresets.find(skin => skin.id === id)
}

/** 获取所有皮肤预设 */
export function getAllSkinPresets(): SkinPreset[] {
  return [...skinPresets]
}

/** 根据深色模式获取皮肤ID */
export function getSkinIdByDarkMode(isDark: boolean): string {
  return isDark ? 'dark' : 'light'
}