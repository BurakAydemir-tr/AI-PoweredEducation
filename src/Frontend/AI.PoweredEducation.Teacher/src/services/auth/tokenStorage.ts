const accessTokenKey = 'teacher_access_token'
const refreshTokenKey = 'teacher_refresh_token'
const firstNameKey = 'teacher_first_name'
const lastNameKey = 'teacher_last_name'

export const tokenStorage = {
  getAccessToken(): string | null {
    return localStorage.getItem(accessTokenKey)
  },

  setAccessToken(token: string): void {
    localStorage.setItem(accessTokenKey, token)
  },

  getRefreshToken(): string | null {
    return localStorage.getItem(refreshTokenKey)
  },

  setRefreshToken(token: string): void {
    localStorage.setItem(refreshTokenKey, token)
  },

  getFirstName(): string | null {
    return localStorage.getItem(firstNameKey)
  },

  getLastName(): string | null {
    return localStorage.getItem(lastNameKey)
  },

  setTeacherName(firstName: string, lastName: string): void {
    localStorage.setItem(firstNameKey, firstName)
    localStorage.setItem(lastNameKey, lastName)
  },

  clear(): void {
    localStorage.removeItem(accessTokenKey)
    localStorage.removeItem(refreshTokenKey)
    localStorage.removeItem(firstNameKey)
    localStorage.removeItem(lastNameKey)
  },
}
