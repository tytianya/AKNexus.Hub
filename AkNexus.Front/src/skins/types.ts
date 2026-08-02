/**
 * 皮肤系统类型定义
 * 基于 CSS 变量的设计系统
 */

/** 设计变量前缀 */
export type DesignVarPrefix = '--ds-'

/** 颜色层级类型 */
export interface DesignColorTokens {
  /** 主色 */
  primary: string
  primaryHover: string
  primaryBg: string
  primaryBorder: string

  /** 成功色 */
  success: {
    text: string
    bg: string
    border: string
  }

  /** 警告色 */
  warning: {
    text: string
    bg: string
    border: string
  }

  /** 危险色 */
  danger: {
    text: string
    bg: string
    border: string
  }

  /** 信息色 */
  info: {
    text: string
    bg: string
    border: string
  }
}

/** 文字颜色层级 */
export interface DesignTextTokens {
  /** 主要文字 */
  primary: string
  /** 次要文字 */
  secondary: string
  /** 三级文字 */
  tertiary: string
  /** 占位符文字 */
  placeholder: string
  /** 禁用文字 */
  disabled: string
  /** 标题文字 */
  title: string
  /** 反色文字 */
  inverse: string
}

/** 背景颜色层级 */
export interface DesignBgTokens {
  /** 基础背景（最深） */
  base: string
  /** 内容背景 */
  content: string
  /** 面板背景 */
  panel: string
  /** 头部背景 */
  header: string
  /** 悬浮背景 */
  hover: string
  /** 遮罩背景 */
  overlay: string
  /** 对话框背景 */
  dialog: string
}

/** 边框颜色层级 */
export interface DesignBorderTokens {
  /** 浅边框 */
  light: string
  /** 中等边框 */
  medium: string
}

/** 阴影层级 */
export interface DesignShadowTokens {
  /** 卡片阴影 */
  card: string
  /** 下拉框阴影 */
  dropdown: string
  /** 对话框阴影 */
  dialog: string
}

/** 尺寸相关变量 */
export interface DesignSizeTokens {
  /** 表格字号 */
  tableFontSize: string
  /** 表格头部字号 */
  tableHeaderFontSize: string
  /** 输入框高度 */
  inputHeight: string
  /** 选择器高度 */
  selectHeight: string
  /** 表单项间距 */
  formItemGap: string
  /** 卡片内边距 */
  cardPadding: string
  /** 卡片头部内边距 */
  cardHeaderPadding: string
  /** 卡片圆角 */
  cardRadius: string
  /** 对话框头部内边距 */
  dialogHeaderPadding: string
  /** 对话框头部高度 */
  dialogHeaderHeight: string
  /** 对话框主体内边距 */
  dialogBodyPadding: string
  /** 对话框底部内边距 */
  dialogFooterPadding: string
  /** 按钮内边距 */
  buttonPadding: string
}

/** 菜单栏变量 */
export interface DesignMenuBarTokens {
  /** 菜单栏背景 */
  bg: string
  /** 菜单栏文字 */
  text: string
  /** 悬浮背景 */
  hoverBg: string
  /** 激活背景 */
  activeBg: string
  /** 激活文字 */
  activeText: string
}

/** 工具栏变量 */
export interface DesignToolbarTokens {
  /** 工具栏背景 */
  bg: string
  /** 工具栏文字 */
  text: string
  /** 悬浮背景 */
  hoverBg: string
  /** 边框色 */
  borderColor: string
}

/** 完整设计变量集合 */
export interface DesignTokens {
  mode: 'light' | 'dark'
  color: DesignColorTokens
  text: DesignTextTokens
  bg: DesignBgTokens
  border: DesignBorderTokens
  shadow: DesignShadowTokens
  size: DesignSizeTokens
  menuBar: DesignMenuBarTokens
  toolbar: DesignToolbarTokens
}

/** 皮肤预设 */
export interface SkinPreset {
  /** 预设ID */
  id: string
  /** 预设名称 */
  name: string
  /** 预设描述 */
  description?: string
  /** 是否为深色模式 */
  isDark: boolean
  /** 设计变量 */
  tokens: DesignTokens
}

/** 皮肤配置 */
export interface SkinConfig {
  /** 当前激活的皮肤ID */
  activeId: string
  /** 是否跟随系统 */
  followSystem: boolean
  /** 是否持久化到 localStorage */
  persistent: boolean
}

/** 皮肤变更事件 */
export interface SkinChangeEvent {
  /** 旧皮肤 */
  oldSkin: SkinPreset | null
  /** 新皮肤 */
  newSkin: SkinPreset
  /** 变更来源 */
  source: 'user' | 'system' | 'init'
}