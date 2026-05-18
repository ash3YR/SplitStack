import { createClient } from '@supabase/supabase-js'

const supabaseUrl = import.meta.env.VITE_SUPABASE_URL
const supabaseAnonKey = import.meta.env.VITE_SUPABASE_ANON_KEY
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || '/api'

// Initialize Supabase client
export const supabase = createClient(supabaseUrl, supabaseAnonKey)

export const authService = {
  async login(payload) {
    const { data, error } = await supabase.auth.signInWithPassword({
      email: payload.email,
      password: payload.password,
    })

    if (error) {
      throw new Error(error.message)
    }

    const { session, user } = data

    // Also sync login in case they confirmed email and this is their first active session
    await fetch(`${apiBaseUrl}/auth/sync`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${session.access_token}`,
      },
      body: JSON.stringify({ name: user.user_metadata?.name || 'User' }),
    }).catch(() => {}) // Ignore if it fails on login

    return {
      token: session.access_token,
      userId: user.id,
      name: user.user_metadata?.name || '',
      email: user.email,
    }
  },

  async register(payload) {
    const { data, error } = await supabase.auth.signUp({
      email: payload.email,
      password: payload.password,
      options: {
        data: {
          name: payload.name,
        },
      },
    })

    if (error) {
      throw new Error(error.message)
    }

    const { session, user } = data

    if (session?.access_token) {
      // Automatically sync the new user with the .NET backend API
      const syncResponse = await fetch(`${apiBaseUrl}/auth/sync`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${session.access_token}`,
        },
        body: JSON.stringify({ name: payload.name }),
      })

      if (!syncResponse.ok) {
        throw new Error('Failed to synchronize user with backend database.')
      }
    } else {
        throw new Error('Check your email to verify your account, or disable "Confirm Email" in Supabase settings.')
    }

    return {
      token: session?.access_token || null,
      userId: user?.id,
      name: payload.name,
      email: payload.email,
    }
  },
}
