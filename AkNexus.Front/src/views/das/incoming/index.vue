<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Delete, Refresh } from '@element-plus/icons-vue'
import {
  getIncomingMaterials,
  createIncomingMaterial,
  updateIncomingMaterial,
  deleteIncomingMaterial,
  getBarcodes,
  type IncomingMaterialDto,
  type MaterialBarcodeDto
} from '/@/api/incomingMaterial'
import { getFlows, type FlowDto } from '/@/api/stationFlow'

defineOptions({ name: 'DasIncoming' })

// 列表
const loading = ref(false)
const tableData = ref<IncomingMaterialDto[]>([])

// 流程
const flows = ref<FlowDto[]>([])

// 新增弹窗
const createVisible = ref(false)
const createSubmitting = ref(false)
const createForm = reactive({
  materialCode: '',
  productModel: '',
  quantity: 10,
  flowId: null as number | null,
  supplier: '',
  lotNo: '',
  remark: ''
})

// 条码查看弹窗
const barcodeVisible = ref(false)
const barcodeLoading = ref(false)
const barcodeData = ref<MaterialBarcodeDto[]>([])
const barcodeMaterial = ref<IncomingMaterialDto | null>(null)

// ---- 条码预览 ----
const padSeq = (n: number) => String(n).padStart(4, '0')

const formatStamp = (d: Date) => {
  const yy = String(d.getFullYear()).slice(-2)
  const MM = String(d.getMonth() + 1).padStart(2, '0')
  const dd = String(d.getDate()).padStart(2, '0')
  const HH = String(d.getHours()).padStart(2, '0')
  const mm = String(d.getMinutes()).padStart(2, '0')
  return `${yy}${MM}${dd}${HH}${mm}`
}

const previewItems = computed(() => {
  const model = createForm.productModel.trim()
  const qty = Number(createForm.quantity)
  if (!model || !qty || qty < 1) return [] as string[]
  const stamp = formatStamp(new Date())
  const show = Math.min(qty, 3)
  const arr: string[] = []
  for (let i = 1; i <= show; i++) {
    arr.push(`${model}${stamp}${padSeq(i)}`)
  }
  return arr
})

const previewTotal = computed(() => {
  const qty = Number(createForm.quantity)
  return qty > 0 ? qty : 0
})

// ---- 数据加载 ----
const loadData = async () => {
  loading.value = true
  try {
    tableData.value = await getIncomingMaterials()
  } catch (e: any) {
    ElMessage.error(e.message || '加载来料列表失败')
  } finally {
    loading.value = false
  }
}

const loadFlows = async () => {
  try {
    flows.value = await getFlows()
  } catch (e: any) {
    ElMessage.error(e.message || '加载流程列表失败')
  }
}

const resetCreate = () => {
  createForm.materialCode = ''
  createForm.productModel = ''
  createForm.quantity = 10
  createForm.flowId = null
  createForm.supplier = ''
  createForm.lotNo = ''
  createForm.remark = ''
}

const openCreate = () => {
  resetCreate()
  createVisible.value = true
}

const handleCreate = async () => {
  if (!createForm.materialCode.trim()) {
    ElMessage.warning('请输入物料编码')
    return
  }
  if (!createForm.productModel.trim()) {
    ElMessage.warning('请输入产品型号')
    return
  }
  const qty = Number(createForm.quantity)
  if (!qty || qty < 1) {
    ElMessage.warning('请输入有效的来料数量')
    return
  }

  createSubmitting.value = true
  try {
    const created: IncomingMaterialDto = await createIncomingMaterial({
      materialCode: createForm.materialCode.trim(),
      productModel: createForm.productModel.trim(),
      quantity: qty,
      flowId: createForm.flowId ?? undefined,
      supplier: createForm.supplier.trim() || undefined,
      lotNo: createForm.lotNo.trim() || undefined,
      remark: createForm.remark.trim() || undefined
    })

    // 获取首末条码用于提示
    let firstBarcode = '-'
    let lastBarcode = '-'
    try {
      const barcodes = await getBarcodes(created.id)
      if (barcodes.length > 0) {
        firstBarcode = barcodes[0].barcode
        lastBarcode = barcodes[barcodes.length - 1].barcode
      }
    } catch {
      // 条码拉取失败不影响主流程
    }

    const count = created.barcodeCount ?? qty
    ElMessageBox.alert(
      `<div style="line-height:1.9">
        <div><b>条码生成成功</b></div>
        <div>共生成 <b>${count}</b> 个条码</div>
        <div>首个条码：<span style="font-family:Consolas,monospace">${firstBarcode}</span></div>
        <div>末个条码：<span style="font-family:Consolas,monospace">${lastBarcode}</span></div>
      </div>`,
      '提示',
      { dangerouslyUseHTMLString: true, confirmButtonText: '知道了' }
    ).catch(() => {})

    createVisible.value = false
    loadData()
  } catch (e: any) {
    ElMessage.error(e.message || '创建失败')
  } finally {
    createSubmitting.value = false
  }
}

const handleInspect = (row: IncomingMaterialDto, result: 'Pass' | 'Fail') => {
  const label = result === 'Pass' ? '通过' : '不合格'
  ElMessageBox.prompt(`请输入检测人员姓名（${label}）`, '检测确认', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    inputPlaceholder: '检测人员',
    inputValidator: (val: string) => {
      if (!val || !val.trim()) return '请输入检测人员姓名'
      return true
    }
  })
    .then(async ({ value }) => {
      try {
        await updateIncomingMaterial(row.id, {
          inspectResult: result,
          inspectedBy: value.trim()
        })
        ElMessage.success('检测结果已更新')
        loadData()
      } catch (e: any) {
        ElMessage.error(e.message || '更新失败')
      }
    })
    .catch(() => {})
}

const handleDelete = (row: IncomingMaterialDto) => {
  ElMessageBox.confirm(`确认删除来料「${row.materialCode}」及其条码吗？`, '提示', {
    type: 'warning',
    confirmButtonText: '确定',
    cancelButtonText: '取消'
  })
    .then(async () => {
      try {
        await deleteIncomingMaterial(row.id)
        ElMessage.success('删除成功')
        loadData()
      } catch (e: any) {
        ElMessage.error(e.message || '删除失败')
      }
    })
    .catch(() => {})
}

const openBarcodes = async (row: IncomingMaterialDto) => {
  barcodeMaterial.value = row
  barcodeData.value = []
  barcodeVisible.value = true
  barcodeLoading.value = true
  try {
    barcodeData.value = await getBarcodes(row.id)
  } catch (e: any) {
    ElMessage.error(e.message || '加载条码失败')
  } finally {
    barcodeLoading.value = false
  }
}

const inspectTagType = (r: string | null) => {
  if (r === 'Pass') return 'success'
  if (r === 'Fail') return 'danger'
  return 'warning'
}

const inspectTagText = (r: string | null) => {
  if (r === 'Pass') return '通过'
  if (r === 'Fail') return '不合格'
  return '待检测'
}

const formatTime = (t?: string | null) => {
  if (!t) return '-'
  const d = new Date(t)
  if (isNaN(d.getTime())) return t
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

onMounted(() => {
  loadData()
  loadFlows()
})
</script>

<template>
  <div class="das-page">
    <!-- 来料检测 -->
    <el-card shadow="never" class="panel-card">
      <template #header>
        <div class="card-header">
          <span class="card-title">来料检测</span>
          <div class="header-actions">
            <el-button :icon="Refresh" @click="loadData">刷新</el-button>
            <el-button type="primary" :icon="Plus" @click="openCreate">新增来料</el-button>
          </div>
        </div>
      </template>

      <el-table v-loading="loading" :data="tableData" border stripe style="width: 100%">
        <el-table-column label="物料编码" prop="materialCode" min-width="140" show-overflow-tooltip />
        <el-table-column label="产品型号" prop="productModel" min-width="140" show-overflow-tooltip />
        <el-table-column label="数量" prop="quantity" width="80" align="center" />
        <el-table-column label="条码数" width="100" align="center">
          <template #default="{ row }">
            <el-link type="primary" :underline="false" @click="openBarcodes(row as IncomingMaterialDto)">
              {{ row.barcodeCount }}
            </el-link>
          </template>
        </el-table-column>
        <el-table-column label="供应商" min-width="130">
          <template #default="{ row }">
            <span>{{ row.supplier || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="批次" min-width="120">
          <template #default="{ row }">
            <span>{{ row.lotNo || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="检测结果" width="110" align="center">
          <template #default="{ row }">
            <el-tag :type="inspectTagType(row.inspectResult)" effect="light">
              {{ inspectTagText(row.inspectResult) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="240" fixed="right">
          <template #default="{ row }">
            <template v-if="!row.inspectResult || row.inspectResult === 'Pending'">
              <el-button type="success" link size="small" @click="handleInspect(row as IncomingMaterialDto, 'Pass')">通过</el-button>
              <el-button type="danger" link size="small" @click="handleInspect(row as IncomingMaterialDto, 'Fail')">不合格</el-button>
            </template>
            <template v-else>
              <div class="inspect-info">
                <span>{{ row.inspectedBy || '-' }}</span>
                <span class="text-muted">{{ formatTime(row.inspectedAt) }}</span>
              </div>
            </template>
            <el-button type="danger" link size="small" :icon="Delete" @click="handleDelete(row as IncomingMaterialDto)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新增来料弹窗 -->
    <el-dialog
      v-model="createVisible"
      title="新增来料"
      width="560px"
      :close-on-click-modal="false"
      destroy-on-close
    >
      <el-form :model="createForm" label-width="100px">
        <el-form-item label="物料编码" required>
          <el-input v-model="createForm.materialCode" placeholder="请输入物料编码" />
        </el-form-item>
        <el-form-item label="产品型号" required>
          <el-input v-model="createForm.productModel" placeholder="条码前缀，如 PROD-A001" />
        </el-form-item>
        <el-form-item label="来料数量" required>
          <el-input-number
            v-model="createForm.quantity"
            :min="1"
            :max="9999"
            :step="1"
            controls-position="right"
          />
        </el-form-item>
        <el-form-item label="关联流程">
          <el-select
            v-model="createForm.flowId"
            placeholder="选填"
            clearable
            filterable
            style="width: 100%"
          >
            <el-option
              v-for="f in flows"
              :key="f.id"
              :label="`${f.flowCode} - ${f.flowName}`"
              :value="f.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="供应商">
          <el-input v-model="createForm.supplier" placeholder="选填" />
        </el-form-item>
        <el-form-item label="批次号">
          <el-input v-model="createForm.lotNo" placeholder="选填" />
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="createForm.remark" placeholder="选填" />
        </el-form-item>

        <el-divider content-position="left">条码预览</el-divider>
        <div class="barcode-preview">
          <div class="preview-tip">条码格式：产品型号 + yyMMddHHmm + 四位数序号</div>
          <template v-if="previewItems.length">
            <div v-for="(b, i) in previewItems" :key="i" class="preview-line">
              第 {{ i + 1 }} 个：<span class="mac-mono">{{ b }}</span>
            </div>
            <div v-if="previewTotal > previewItems.length" class="preview-more">
              ... 共 {{ previewTotal }} 个
            </div>
          </template>
          <div v-else class="preview-empty">请输入产品型号与数量后预览条码</div>
        </div>
      </el-form>

      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" :loading="createSubmitting" @click="handleCreate">生成条码</el-button>
      </template>
    </el-dialog>

    <!-- 条码查看弹窗 -->
    <el-dialog
      v-model="barcodeVisible"
      :title="`条码明细 - ${barcodeMaterial?.materialCode || ''}`"
      width="800px"
      destroy-on-close
    >
      <el-table v-loading="barcodeLoading" :data="barcodeData" border stripe max-height="480">
        <el-table-column label="序号" prop="sequenceNo" width="80" align="center" />
        <el-table-column label="条码" min-width="260">
          <template #default="{ row }">
            <span class="mac-mono">{{ row.barcode }}</span>
          </template>
        </el-table-column>
        <el-table-column label="当前站点" min-width="140">
          <template #default="{ row }">
            <span>{{ row.stationName || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isUsed ? 'success' : 'info'" effect="light">
              {{ row.isUsed ? '已使用' : '未使用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="使用时间" min-width="160">
          <template #default="{ row }">
            <span>{{ formatTime(row.usedAt) }}</span>
          </template>
        </el-table-column>
      </el-table>

      <template #footer>
        <el-button @click="barcodeVisible = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.das-page {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.panel-card {
  background: var(--ds-bg-panel, #ffffff);
  border: 1px solid var(--ds-border-light, #ebeef5);
  border-radius: var(--ds-card-radius, 8px);
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;

  .header-actions {
    display: flex;
    align-items: center;
    gap: 8px;
  }
}

.card-title {
  font-weight: 600;
  color: var(--ds-text-title, #303133);
}

.text-muted {
  color: var(--ds-text-tertiary, #909399);
  font-size: 12px;
}

.mac-mono {
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  letter-spacing: 0.5px;
}

.inspect-info {
  display: inline-flex;
  flex-direction: column;
  line-height: 1.4;
  margin-right: 8px;
}

.barcode-preview {
  padding: 12px 16px;
  background: var(--ds-bg-header, #f5f7fa);
  border: 1px dashed var(--ds-border-light, #dcdfe6);
  border-radius: 6px;

  .preview-tip {
    color: var(--ds-text-tertiary, #909399);
    font-size: 12px;
    margin-bottom: 8px;
  }

  .preview-line {
    line-height: 1.8;
    color: var(--ds-text-title, #303133);
  }

  .preview-more {
    color: var(--ds-text-tertiary, #909399);
    margin-top: 4px;
  }

  .preview-empty {
    color: var(--ds-text-tertiary, #909399);
  }
}
</style>
