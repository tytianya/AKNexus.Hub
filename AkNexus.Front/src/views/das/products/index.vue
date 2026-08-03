<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Edit, Delete, Connection } from '@element-plus/icons-vue'
import {
  getProducts,
  createProduct,
  updateProduct,
  deleteProduct,
  bindFlow,
  type Product
} from '/@/api/product'
import { getFlows, type FlowDto } from '/@/api/stationFlow'

defineOptions({ name: 'DasProducts' })

// 状态选项
const statusOptions = [
  { value: 'Active', label: '启用' },
  { value: 'Inactive', label: '停用' },
  { value: 'Scrapped', label: '报废' }
]

const statusMap: Record<string, { label: string; type: 'success' | 'info' | 'danger' | 'warning' }> = {
  Active: { label: '启用', type: 'success' },
  Inactive: { label: '停用', type: 'info' },
  Scrapped: { label: '报废', type: 'danger' }
}

// 列表数据
const loading = ref(false)
const tableData = ref<Product[]>([])
const flows = ref<FlowDto[]>([])

// 启用的流程（用于绑定）
const activeFlows = computed(() => flows.value.filter(f => f.isActive))

// 流程名称查找
const flowNameOf = (code: string | null) =>
  code ? flows.value.find(f => f.flowCode === code)?.flowName || code : ''

// 产品编辑弹窗
const editDialogVisible = ref(false)
const editDialogTitle = ref('')
const isEdit = ref(false)
const submitting = ref(false)
const currentId = ref<number | null>(null)

const form = reactive({
  productCode: '',
  productName: '',
  spec: '',
  batchNo: '',
  status: 'Active'
})

// 流程绑定弹窗
const bindDialogVisible = ref(false)
const bindSubmitting = ref(false)
const bindProduct = ref<Product | null>(null)
const bindFlowId = ref<number | null>(null)

// 加载产品列表
const loadData = async () => {
  loading.value = true
  try {
    tableData.value = await getProducts()
  } catch (e: any) {
    ElMessage.error(e.message || '加载产品列表失败')
  } finally {
    loading.value = false
  }
}

// 加载流程列表
const loadFlows = async () => {
  try {
    flows.value = await getFlows()
  } catch (e: any) {
    ElMessage.error(e.message || '加载流程列表失败')
  }
}

// 重置表单
const resetForm = () => {
  form.productCode = ''
  form.productName = ''
  form.spec = ''
  form.batchNo = ''
  form.status = 'Active'
  currentId.value = null
}

// 打开新建
const openCreate = () => {
  resetForm()
  isEdit.value = false
  editDialogTitle.value = '新建产品'
  editDialogVisible.value = true
}

// 打开编辑
const openEdit = (row: Product) => {
  resetForm()
  isEdit.value = true
  editDialogTitle.value = '编辑产品'
  currentId.value = row.id
  form.productCode = row.productCode
  form.productName = row.productName
  form.spec = row.spec || ''
  form.batchNo = row.batchNo || ''
  form.status = row.status || 'Active'
  editDialogVisible.value = true
}

// 提交
const submit = async () => {
  if (!form.productCode.trim()) {
    ElMessage.warning('请输入产品编码')
    return
  }
  if (!form.productName.trim()) {
    ElMessage.warning('请输入产品名称')
    return
  }
  submitting.value = true
  const payload = {
    productCode: form.productCode.trim(),
    productName: form.productName.trim(),
    spec: form.spec.trim(),
    batchNo: form.batchNo.trim(),
    status: form.status
  }
  try {
    if (isEdit.value && currentId.value != null) {
      await updateProduct(currentId.value, payload)
      ElMessage.success('更新成功')
    } else {
      await createProduct(payload)
      ElMessage.success('创建成功')
    }
    editDialogVisible.value = false
    loadData()
  } catch (e: any) {
    ElMessage.error(e.message || '保存失败')
  } finally {
    submitting.value = false
  }
}

// 删除
const handleDelete = (row: Product) => {
  ElMessageBox.confirm(`确认删除产品「${row.productName}」吗？`, '提示', {
    type: 'warning',
    confirmButtonText: '确定',
    cancelButtonText: '取消'
  })
    .then(async () => {
      try {
        await deleteProduct(row.id)
        ElMessage.success('删除成功')
        loadData()
      } catch (e: any) {
        ElMessage.error(e.message || '删除失败')
      }
    })
    .catch(() => {})
}

// 打开流程绑定
const openBind = (row: Product) => {
  bindProduct.value = row
  // 预选当前已绑定的流程
  const matched = flows.value.find(f => f.flowCode === row.boundFlowCode)
  bindFlowId.value = matched ? matched.id : null
  bindDialogVisible.value = true
}

// 确认绑定
const confirmBind = async () => {
  if (bindFlowId.value == null) {
    ElMessage.warning('请选择流程')
    return
  }
  if (!bindProduct.value) return
  bindSubmitting.value = true
  try {
    await bindFlow(bindProduct.value.id, bindFlowId.value)
    ElMessage.success('绑定成功')
    bindDialogVisible.value = false
    loadData()
  } catch (e: any) {
    ElMessage.error(e.message || '绑定失败')
  } finally {
    bindSubmitting.value = false
  }
}

onMounted(() => {
  loadData()
  loadFlows()
})
</script>

<template>
  <div class="das-page">
    <el-card class="product-card" v-loading="loading">
      <template #header>
        <div class="card-header">
          <span class="card-title">产品管理</span>
          <el-button type="primary" :icon="Plus" @click="openCreate">新建产品</el-button>
        </div>
      </template>

      <el-table :data="tableData" stripe style="width: 100%">
        <el-table-column label="产品编码" prop="productCode" min-width="120" />
        <el-table-column label="产品名称" prop="productName" min-width="150" />
        <el-table-column label="规格" prop="spec" min-width="120">
          <template #default="{ row }">
            <span>{{ row.spec || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="批次号" prop="batchNo" min-width="120">
          <template #default="{ row }">
            <span>{{ row.batchNo || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="已绑定流程" min-width="160">
          <template #default="{ row }">
            <el-tag v-if="row.boundFlowCode" type="primary" effect="light">
              {{ flowNameOf(row.boundFlowCode) }}
            </el-tag>
            <span v-else class="text-muted">未绑定</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="(statusMap[row.status]?.type) || 'info'" effect="light">
              {{ statusMap[row.status]?.label || row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="180">
          <template #default="{ row }">
            <span>{{ row.createdAt || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="230" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" :icon="Connection" @click="openBind(row as Product)">
              绑定流程
            </el-button>
            <el-button link type="primary" :icon="Edit" @click="openEdit(row as Product)">编辑</el-button>
            <el-button link type="danger" :icon="Delete" @click="handleDelete(row as Product)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新建/编辑产品弹窗 -->
    <el-dialog
      v-model="editDialogVisible"
      :title="editDialogTitle"
      width="560px"
      :close-on-click-modal="false"
      destroy-on-close
    >
      <el-form :model="form" label-width="90px" label-position="right">
        <el-form-item label="产品编码">
          <el-input v-model="form.productCode" placeholder="请输入产品编码" :disabled="isEdit" />
        </el-form-item>
        <el-form-item label="产品名称">
          <el-input v-model="form.productName" placeholder="请输入产品名称" />
        </el-form-item>
        <el-form-item label="规格">
          <el-input v-model="form.spec" placeholder="请输入规格" />
        </el-form-item>
        <el-form-item label="批次号">
          <el-input v-model="form.batchNo" placeholder="请输入批次号" />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="form.status" placeholder="请选择状态" style="width: 100%">
            <el-option
              v-for="opt in statusOptions"
              :key="opt.value"
              :label="opt.label"
              :value="opt.value"
            />
          </el-select>
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="editDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="submit">确定</el-button>
      </template>
    </el-dialog>

    <!-- 流程绑定弹窗 -->
    <el-dialog
      v-model="bindDialogVisible"
      title="绑定流程"
      width="500px"
      :close-on-click-modal="false"
      destroy-on-close
    >
      <div class="bind-product">
        <span class="bind-label">产品：</span>
        <span class="bind-value">
          {{ bindProduct?.productName }}（{{ bindProduct?.productCode }}）
        </span>
      </div>

      <el-form label-width="90px" class="bind-form">
        <el-form-item label="当前绑定">
          <el-tag v-if="bindProduct?.boundFlowCode" type="primary" effect="light">
            {{ flowNameOf(bindProduct.boundFlowCode) }}
          </el-tag>
          <span v-else class="text-muted">未绑定</span>
        </el-form-item>
        <el-form-item label="选择流程">
          <el-select
            v-model="bindFlowId"
            placeholder="请选择启用的流程"
            filterable
            style="width: 100%"
          >
            <el-option
              v-for="f in activeFlows"
              :key="f.id"
              :label="`${f.flowCode} - ${f.flowName}`"
              :value="f.id"
            />
          </el-select>
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="bindDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="bindSubmitting" @click="confirmBind">
          确认绑定
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.das-page {
  padding: 16px;
}

.product-card {
  background: var(--ds-bg-panel);
  border: 1px solid var(--ds-border-light);
  border-radius: var(--ds-card-radius);
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.card-title {
  font-weight: 600;
  color: var(--ds-text-title);
}

.text-muted {
  color: var(--ds-text-tertiary);
}

.bind-product {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 12px 16px;
  margin-bottom: 8px;
  background: var(--ds-bg-header);
  border: 1px solid var(--ds-border-light);
  border-radius: var(--ds-card-radius);
}

.bind-label {
  color: var(--ds-text-secondary);
  font-size: 13px;
}

.bind-value {
  color: var(--ds-text-title);
  font-weight: 600;
}

.bind-form {
  margin-top: 8px;
}
</style>
