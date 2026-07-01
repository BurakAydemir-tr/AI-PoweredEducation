import axios from 'axios'

type ApiErrorBody = {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

export function getApiErrorMessage(error: unknown): string {
  if (!axios.isAxiosError<ApiErrorBody>(error)) {
    return 'Beklenmeyen bir hata oluştu.'
  }

  if (error.message === 'Network Error') {
    return 'Sunucuya ulaşılamadı. Backend uygulamasının çalıştığından emin olun.'
  }

  const body = error.response?.data
  const validationMessages = body?.errors
    ? Object.values(body.errors).flat()
    : []

  return (
    validationMessages[0] ??
    body?.detail ??
    body?.title ??
    error.message ??
    'İstek başarısız oldu.'
  )
}
