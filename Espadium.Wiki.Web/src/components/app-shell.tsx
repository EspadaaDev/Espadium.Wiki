"use client";
import { ReactNode, useEffect, useState } from "react";
import { useAuth } from "@/lib/auth-store";
import { wireAuthAccessors, apiFetch } from "@/lib/api";
import { Topbar } from "@/components/topbar";

export function AppShell({ children }: { children: ReactNode }) {
  const { state, clear, setAccessToken } = useAuth();
  const [initDone, setInitDone] = useState(false);

  useEffect(() => {
    wireAuthAccessors(() => state.accessToken, clear);
  }, [state.accessToken, clear]);

  // Lazy session refresh on first mount
  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const res = await apiFetch('/auth/refresh', { method: 'POST' }, false);
        if (!cancelled && res.ok) {
          const data = await res.json().catch(() => null);
          if (data?.accessToken) setAccessToken(data.accessToken);
        } else if (!cancelled && res.status === 401) {
          clear();
        }
      } catch {
        if (!cancelled) clear();
      } finally {
        if (!cancelled) setInitDone(true);
      }
    })();
    return () => { cancelled = true; };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function logout() {
    try { await apiFetch('/auth/logout', { method: 'POST' }); } catch {}
    clear();
    if (typeof window !== 'undefined') window.location.href = '/login';
  }

  return (
    <div className="min-h-screen flex flex-col bg-sky-50">
      <Topbar onLogout={logout} isAuthed={!!state.accessToken} />
      <div className="flex-1">
        {children}
      </div>
    </div>
  );
}
