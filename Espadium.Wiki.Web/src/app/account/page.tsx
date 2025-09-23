"use client";
import Link from "next/link";
import { useAuth } from "@/lib/auth-store";

type Jwt = { sub?: string; email?: string };

function decodeJwt(token: string): Jwt | null {
  try {
    const parts = token.split(".");
    if (parts.length < 2) return null;
    const payload = parts[1].replace(/-/g, "+").replace(/_/g, "/");
    const json = typeof window === 'undefined' ? Buffer.from(payload, 'base64').toString('utf-8') : atob(payload);
    return JSON.parse(json);
  } catch { return null; }
}

export default function AccountPage() {
  const { state } = useAuth();
  const token = state.accessToken;
  let email: string | undefined;
  let sub: string | undefined;
  if (token) { const p = decodeJwt(token); email = p?.email; sub = p?.sub; }

  if (!token) {
    return (
      <div className="p-6 space-y-3">
        <h1 className="text-xl font-semibold text-slate-800">Аккаунт</h1>
        <div className="text-slate-600">Вы не вошли в систему.</div>
        <div className="flex gap-3">
          <Link href="/login" className="px-3 py-1.5 rounded bg-sky-600 text-white hover:bg-sky-700">Войти</Link>
          <Link href="/register" className="px-3 py-1.5 rounded border border-sky-200 text-sky-700 hover:bg-sky-50">Зарегистрироваться</Link>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-3">
      <h1 className="text-xl font-semibold text-slate-800">Профиль</h1>
      <div className="text-slate-700">Email: <span className="font-medium">{email || '(неизвестно)'}</span></div>
      <div className="text-slate-700">ID: <span className="font-mono">{sub || '(неизвестно)'}</span></div>
    </div>
  );
}


