<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Upload, UploadFilled, Delete, Refresh } from '@element-plus/icons-vue'
import type { UploadUserFile } from 'element-plus'
import {
  getSoftware,
  uploadSoftware,
  deleteSoftware,
  bindSoftware,
  getStationSoftware,
  getDownloadUrl,
  type TestSoftwareDto
} from '/@/api/testSoftware'
import { getStations, type Station } from '/@/api/station'

defineOptions({ name: 'DasSoftware' })

// 软件列表
const loading = ref(false)
const softwareList = ref<TestSoftwareDto[]>([])

// 站点
const stations = ref<Station[]>([])

// 上传弹窗
const uploadVisible = ref(false)
const uploadSubmitting = ref(false)
const uploadProgress = ref(0)
const uploadForm = reactive({
  softwareCode: '',
  softwareName: '',
  version: '',
  description: ''
})
const uploadRef = ref()
const fileList = ref<UploadUserFile[]>([])

// 站点绑定
const selectedStationId = ref<number | null>(null)
const selectedSoftwareId = ref<number | null>(null)
const bindSubmitting = ref(false)
const boundLoading = ref(false)
const boundList = ref<TestSoftwareDto[]>([])

const loadSoftware = async () => {
  loading.value = true
  try {
    softwareList.value = await getSoftware()
  } catch (e: any) {
    ElMessage.error(e.message || '加载软件列表失败')
  } finally {
    loading.value = false
  }
}

const loadStations = async () => {
  try {
    stations.value = await getStations()
  } catch (e: any) {
    ElMessage.error(e.message || '加载站点列表失败')
  }
}

const loadBound = async () => {
  if (selectedStationId.value == null) {
    boundList.value = []
    return
  }
  boundLoading.value = true
  try {
    boundList.value = await getStationSoftware(selectedStationId.value)
  } catch (e: any) {
    ElMessage.error(e.message || '加载站点绑定软件失败')
  } finally {
    boundLoading.value = false
  }
}

const onStationChange = () => {
  loadBound()
}

const resetUpload = () => {
  uploadForm.softwareCode = ''
  uploadForm.softwareName = ''
  uploadForm.version = ''
  uploadForm.description = ''
  fileList.value = []
  uploadProgress.value = 0
}

const openUpload = () => {
  resetUpload()
  uploadVisible.value = true
}

const handleExceed = (files: File[]) => {
  uploadRef.value?.clearFiles()
  const file = files[0]
  if (file) uploadRef.value?.handleStart(file)
}

const handleUpload = async () => {
  if (!uploadForm.softwareCode.trim()) {
    ElMessage.warning('请输入软件编码')
    return
  }
  if (!uploadForm.softwareName.trim()) {
    ElMessage.warning('请输入软件名称')
    return
  }
  if (!uploadForm.version.trim()) {
    ElMessage.warning('请输入版本号')
    return
  }
  if (fileList.value.length === 0) {
    ElMessage.warning('请选择要上传的文件')
    return
  }

  const file = fileList.value[0]?.raw
  if (!file) {
    ElMessage.warning('文件无效')
    return
  }

  const fd = new FormData()
  fd.append('file', file)
  fd.append('softwareCode', uploadForm.softwareCode.trim())
  fd.append('softwareName', uploadForm.softwareName.trim())
  fd.append('version', uploadForm.version.trim())
  fd.append('description', uploadForm.description.trim())

  uploadSubmitting.value = true
  uploadProgress.value = 0
  try {
    await uploadSoftware(fd, (percent: number) => {
      uploadProgress.value = percent
    })
    ElMessage.success('上传成功')
    uploadVisible.value = false
    loadSoftware()
    if (selectedStationId.value != null) loadBound()
  } catch (e: any) {
    ElMessage.error(e.message || '上传失败')
  } finally {
    uploadSubmitting.value = false
  }
}

const handleDelete = (row: TestSoftwareDto) => {
  ElMessageBox.confirm(`确认删除软件「${row.softwareName}」吗？`, '提示', {
    type: 'warning',
    confirmButtonText: '确定',
    cancelButtonText: '取消'
  })
    .then(async () => {
      try {
        await deleteSoftware(row.id)
        ElMessage.success('删除成功')
        loadSoftware()
        if (selectedStationId.value != null) loadBound()
      } catch (e: any) {
        ElMessage.error(e.message || '删除失败')
      }
    })
    .catch(() => {})
}

const handleBind = async () => {
  if (selectedStationId.value == null) {
    ElMessage.warning('请选择站点')
    return
  }
  if (selectedSoftwareId.value == null) {
    ElMessage.warning('请选择软件')
    return
  }
  bindSubmitting.value = true
  try {
    await bindSoftware({
      stationId: selectedStationId.value,
      softwareId: selectedSoftwareId.value
    })
    ElMessage.success('绑定成功')
    selectedSoftwareId.value = null
    loadBound()
  } catch (e: any) {
    ElMessage.error(e.message || '绑定失败')
  } finally {
    bindSubmitting.value = false
  }
}

const formatSize = (size?: number) => {
  if (size == null) return '-'
  return (size / 1024).toFixed(1) + ' KB'
}

const formatTime = (t?: string | null) => {
  if (!t) return '-'
  const d = new Date(t)
  if (isNaN(d.getTime())) return t
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`
}

onMounted(() => {
  loadSoftware()
  loadStations()
})
</script>

<template>
  <div class="das-page">
    <!-- 测试软件管理 -->
    <el-card shadow="never" class="panel-card">
      <template #header>
        <div class="card-header">
          <span class="card-title">测试软件管理</span>
          <div class="header-actions">
            <el-button :icon="Refresh" @click="loadSoftware">刷新</el-button>
            <el-button type="primary" :icon="Upload" @click="openUpload">上传软件</el-button>
          </div>
        </div>
      </template>

      <el-table v-loading="loading" :data="softwareList" border stripe style="width: 100%">
        <el-table-column label="编码" prop="softwareCode" min-width="120" show-overflow-tooltip />
        <el-table-column label="名称" prop="softwareName" min-width="160" show-overflow-tooltip />
        <el-table-column label="版本" prop="version" width="110" align="center">
          <template #default="{ row }">
            <el-tag type="info" effect="light">v{{ row.version }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="文件名" prop="fileName" min-width="180" show-overflow-tooltip />
        <el-table-column label="大小(KB)" width="120" align="center">
          <template #default="{ row }">
            <span>{{ formatSize(row.fileSize) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="上传时间" min-width="170">
          <template #default="{ row }">
            <span>{{ formatTime(row.uploadedAt) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="150" align="center" fixed="right">
          <template #default="{ row }">
            <el-link
              type="primary"
              :underline="false"
              :href="getDownloadUrl(row.id)"
              target="_blank"
            >下载</el-link>
            <el-button type="danger" link size="small" :icon="Delete" @click="handleDelete(row as TestSoftwareDto)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 站点软件绑定 -->
    <el-card shadow="never" class="panel-card">
      <template #header>
        <div class="card-header">
          <span class="card-title">站点软件绑定</span>
        </div>
      </template>

      <el-form :inline="true" class="bind-form">
        <el-form-item label="站点">
          <el-select
            v-model="selectedStationId"
            placeholder="请选择站点"
            filterable
            clearable
            style="width: 240px"
            @change="onStationChange"
          >
            <el-option
              v-for="s in stations"
              :key="s.id"
              :label="`${s.stationCode} - ${s.stationName}`"
              :value="s.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="软件">
          <el-select
            v-model="selectedSoftwareId"
            placeholder="请选择软件"
            filterable
            clearable
            style="width: 280px"
          >
            <el-option
              v-for="sw in softwareList"
              :key="sw.id"
              :label="`${sw.softwareCode} - ${sw.softwareName} v${sw.version}`"
              :value="sw.id"
            />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :loading="bindSubmitting" @click="handleBind">绑定</el-button>
        </el-form-item>
      </el-form>

      <el-table v-loading="boundLoading" :data="boundList" border stripe style="width: 100%">
        <el-table-column label="编码" prop="softwareCode" min-width="120" show-overflow-tooltip />
        <el-table-column label="名称" prop="softwareName" min-width="160" show-overflow-tooltip />
        <el-table-column label="版本" prop="version" width="110" align="center">
          <template #default="{ row }">
            <el-tag type="info" effect="light">v{{ row.version }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="文件名" prop="fileName" min-width="180" show-overflow-tooltip />
        <el-table-column label="大小(KB)" width="120" align="center">
          <template #default="{ row }">
            <span>{{ formatSize(row.fileSize) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="110" align="center" fixed="right">
          <template #default="{ row }">
            <el-link
              type="primary"
              :underline="false"
              :href="getDownloadUrl(row.id)"
              target="_blank"
            >下载</el-link>
          </template>
        </el-table-column>
      </el-table>
      <el-empty
        v-if="!boundLoading && selectedStationId == null"
        description="请先选择站点查看已绑定软件"
        :image-size="60"
      />
    </el-card>

    <!-- 上传弹窗 -->
    <el-dialog
      v-model="uploadVisible"
      title="上传软件"
      width="520px"
      :close-on-click-modal="false"
      destroy-on-close
    >
      <el-form :model="uploadForm" label-width="90px">
        <el-form-item label="软件编码" required>
          <el-input v-model="uploadForm.softwareCode" placeholder="请输入软件编码" />
        </el-form-item>
        <el-form-item label="软件名称" required>
          <el-input v-model="uploadForm.softwareName" placeholder="请输入软件名称" />
        </el-form-item>
        <el-form-item label="版本号" required>
          <el-input v-model="uploadForm.version" placeholder="请输入版本号，如 1.0.0" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="uploadForm.description" type="textarea" :rows="2" placeholder="选填" />
        </el-form-item>
        <el-form-item label="文件" required>
          <el-upload
            ref="uploadRef"
            v-model:file-list="fileList"
            drag
            :auto-upload="false"
            :limit="1"
            :on-exceed="handleExceed"
          >
            <el-icon class="el-icon--upload"><upload-filled /></el-icon>
            <div class="el-upload__text">将文件拖到此处，或<em>点击选择</em></div>
            <template #tip>
              <div class="upload-tip">只能上传一个文件，如需更换请先移除</div>
            </template>
          </el-upload>
        </el-form-item>

        <el-progress
          v-if="uploadSubmitting"
          :percentage="uploadProgress"
          :stroke-width="8"
          :status="uploadProgress >= 100 ? 'success' : undefined"
        />
      </el-form>

      <template #footer>
        <el-button @click="uploadVisible = false">取消</el-button>
        <el-button type="primary" :loading="uploadSubmitting" @click="handleUpload">上传</el-button>
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

.bind-form {
  margin-bottom: 12px;
}

.upload-tip {
  font-size: 12px;
  color: var(--ds-text-tertiary, #909399);
  margin-top: 4px;
}
</style>
