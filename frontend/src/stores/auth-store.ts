import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface AuthUser {
  email: string
  firstName: string
  lastName: string
}

interface AuthState {
  token: string | null
  user: AuthUser | null
  isInitialized: boolean
  login: (token: string, user: AuthUser) => void
  setToken: (token: string) => void
  logout: () => void
  setInitialized: (val: boolean) => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      isInitialized: false,
      login: (token, user) => set({ token, user }),
      setToken: (token) => set({ token }),
      logout: () => set({ token: null, user: null }),
      setInitialized: (val) => set({ isInitialized: val }),
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({ user: state.user }),
    }
  )
)
