import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface AuthUser {
  email: string
  firstName: string
  lastName: string
}

interface AuthState {
  token: string | null
  refreshToken: string | null
  user: AuthUser | null
  _hasHydrated: boolean
  login: (token: string, refreshToken: string, user: AuthUser) => void
  setTokens: (token: string, refreshToken: string) => void
  logout: () => void
  setHasHydrated: (val: boolean) => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      refreshToken: null,
      user: null,
      _hasHydrated: false,
      login: (token, refreshToken, user) => set({ token, refreshToken, user }),
      setTokens: (token, refreshToken) => set({ token, refreshToken }),
      logout: () => set({ token: null, refreshToken: null, user: null }),
      setHasHydrated: (val) => set({ _hasHydrated: val }),
    }),
    {
       name: 'auth-storage',
      partialize: (state) => ({ token: state.token, refreshToken: state.refreshToken, user: state.user }),
      onRehydrateStorage: () => (state) => {
        state?.setHasHydrated(true)  // ← use the action instead
      },
    }
  )
)
