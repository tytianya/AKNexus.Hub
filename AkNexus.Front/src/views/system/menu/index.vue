<script setup lang="ts">
/**
 * 菜单配置页面
 * 超级管理员可查看和配置系统菜单
 * 当前为预留实现，展示已注册菜单列表
 */
import { ref, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getGlobalMenus } from '/@/modules'
import type { ModuleMenuItem } from '/@/modules'
import { useAuthStore } from '/@/modules/auth/store'

defineOptions({ name: 'SystemMenu' })

const authStore = useAuthStore()

// 菜单列表
const menus = ref<ModuleMenuItem[]>([])

// 加载菜单
const loadMenus = () => {
  menus.value = getGlobalMenus()
}

// 排序后的菜单
const sortedMenus = computed(() => {
  return [...menus.value].sort((a, b) => (a.order ?? 999) - (b.order ?? 999))
})

// 编辑对话框
const dialogVisible = ref(false)
const editingMenu = ref<ModuleMenuItem | null>(null)

const openEdit = (menu: ModuleMenuItem) => {
  editingMenu.value = { ...menu }
  dialogVisible.value = true
}

const saveMenu = () => {
  // 预留：后续对接后端 API 持久化菜单配置
  ElMessage.success('菜单配置保存成功（预留功能，当前仅本地展示）')
  dialogVisible.value = false
}

onMounted(() => loadMenus())
</script>

<template>
  <div class="das-page">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>菜单配置</span>
          <el-tag v-if="authStore.isSuperAdmin" type="danger" size="small">超级管理员</el-tag>
        </div>
      </template>

      <el-alert
        type="info"
        :closable="false"
        show-icon
        style="margin-bottom: 16px"
      >
        当前展示系统已注册的菜单项。超级管理员可配置菜单的显示、排序和路径。
        后续对接后端后，菜单配置将持久化到数据库。
      </el-alert>

      <el-table :data="sortedMenus" border style="width: 100%">
        <el-table-column prop="title" label="菜单名称" min-width="120" />
        <el-table-column prop="path" label="路由路径" min-width="150">
          <template #default="{ row }">
            <el-link :href="row.path" type="primary" :underline="false" style="font-family: monospace">
              {{ row.path }}
            </el-link>
          </template>
        </el-table-column>
        <el-table-column prop="icon" label="图标" width="100">
          <template #default="{ row }">
            <el-icon v-if="row.icon" :size="16">
              <component :is="row.icon" />
            </el-icon>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column prop="order" label="排序" width="80" align="center" />
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.hideInMenu ? 'info' : 'success'" size="small">
              {{ row.hideInMenu ? '已隐藏' : '显示中' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" align="center" fixed="right">
          <template #default="{ row }">
            <el-button type="primary" link size="small" @click="openEdit(row as ModuleMenuItem)">编辑</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 编辑对话框 -->
    <el-dialog v-model="dialogVisible" title="编辑菜单" width="480px">
      <el-form v-if="editingMenu" label-width="80px" :model="editingMenu">
        <el-form-item label="菜单名称">
          <el-input v-model="editingMenu.title" />
        </el-form-item>
        <el-form-item label="路由路径">
          <el-input v-model="editingMenu.path" disabled />
        </el-form-item>
        <el-form-item label="图标">
          <el-input v-model="editingMenu.icon" placeholder="Element Plus 图标名" />
        </el-form-item>
        <el-form-item label="排序">
          <el-input-number v-model="editingMenu.order" :min="0" :max="999" />
        </el-form-item>
        <el-form-item label="是否隐藏">
          <el-switch v-model="editingMenu.hideInMenu" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveMenu">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.das-page {
  padding: 16px;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 12px;
}
</style>
