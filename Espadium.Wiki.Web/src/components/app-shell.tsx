"use client";
import { ReactNode, useEffect } from "react";
import { useAuth } from "@/lib/auth-store";
import { wireAuthAccessors } from "@/lib/api";
import { Topbar } from "@/components/topbar";

export function AppShell({ children }: { children: ReactNode }) {
  const { state, clear } = useAuth();
  useEffect(() => {
    wireAuthAccessors(() => state.accessToken, clear);
  }, [state.accessToken, clear]);
  return (
    <div className="min-h-screen flex flex-col">
      <Topbar onLogout={clear} />
      <div className="flex-1 grid grid-cols-[16rem_1fr]">
        {children}
      </div>
    </div>
  );
}
