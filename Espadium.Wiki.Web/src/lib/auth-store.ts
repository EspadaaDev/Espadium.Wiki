"use client";
import React, { createContext, useContext, useReducer, useMemo } from 'react';

type State = { accessToken: string | null };
type Action = { type: 'set', token: string } | { type: 'clear' };

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case 'set': return { accessToken: action.token };
    case 'clear': return { accessToken: null };
    default: return state;
  }
}

const Ctx = createContext<{ state: State; setAccessToken: (t: string) => void; clear: () => void } | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, dispatch] = useReducer(reducer, { accessToken: null });
  const value = useMemo(() => ({
    state,
    setAccessToken: (t: string) => dispatch({ type: 'set', token: t }),
    clear: () => dispatch({ type: 'clear' })
  }), [state]);
  return <Ctx.Provider value={value}>{children}</Ctx.Provider>;
}

export function useAuth() {
  const ctx = useContext(Ctx);
  if (!ctx) throw new Error('AuthProvider missing');
  return ctx;
}

