import { createContext, useEffect, useMemo, useState } from 'react'
import { authService } from '../services/authService'

const AUTH_STORAGE_KEY = 'smart-splitter-auth'

export const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [authState, setAuthState] = useState(() => {
    const saved = localStorage.getItem(AUTH_STORAGE_KEY)

    if (!saved) {
      return { user: null, token: null }
    }

    try {
      return JSON.parse(saved)
    } catch {
      localStorage.removeItem(AUTH_STORAGE_KEY)
      return { user: null, token: null }
    }
  })

  useEffect(() => {
    if (authState.token) {
      localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(authState))
    } else {
      localStorage.removeItem(AUTH_STORAGE_KEY)
    }
  }, [authState])

  useEffect(() => {
    const handleUnauthorized = () => {
      setAuthState({ user: null, token: null })
    }

    window.addEventListener('auth:unauthorized', handleUnauthorized)
    return () => window.removeEventListener('auth:unauthorized', handleUnauthorized)
  }, [])

  const login = async (payload) => {
    const response = await authService.login(payload)
    const nextState = {
      token: response.token,
      user: {
        userId: response.userId,
        name: response.name,
        email: response.email,
      },
    }

    setAuthState(nextState)
    return response
  }

  const register = async (payload) => {
    const response = await authService.register(payload)
    const nextState = {
      token: response.token,
      user: {
        userId: response.userId,
        name: response.name,
        email: response.email,
      },
    }

    setAuthState(nextState)
    return response
  }

  const logout = () => {
    setAuthState({ user: null, token: null })
  }

  const value = useMemo(() => ({
    user: authState.user,
    token: authState.token,
    isAuthenticated: Boolean(authState.token),
    login,
    register,
    logout,
  }), [authState.user, authState.token])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
