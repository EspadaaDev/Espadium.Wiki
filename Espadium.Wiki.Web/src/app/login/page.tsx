"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { useAuth } from "@/lib/auth-store";
import Link from "next/link";

export default function LoginPage() {
  const [email,setEmail]=useState(""); const [password,setPassword]=useState(""); const [err,setErr]=useState<string|null>(null);
  const { setAccessToken } = useAuth();
  const router = useRouter();
  async function submit(e: React.FormEvent){ e.preventDefault(); setErr(null);
    const res = await apiFetch("/auth/login",{ method:"POST", headers:{ "Content-Type":"application/json" }, body: JSON.stringify({ email, password }) });
    if(!res.ok){ setErr("Неверная почта или пароль"); return; }
    const { accessToken } = await res.json(); setAccessToken(accessToken); router.push("/spaces");
  }
  return <div className="max-w-sm mx-auto p-6 space-y-4 bg-surface rounded-md shadow-[0_1px_1px_rgba(0,0,0,.04),_0_4px_8px_rgba(9,74,154,.06)]">
    <h1 className="text-xl font-semibold">Вход</h1>
    <form onSubmit={submit} className="space-y-3">
      <input className="w-full border border-border rounded-md p-2 focus:ring-2 focus:ring-brand-500 focus:border-brand-400" value={email} onChange={e=>setEmail(e.target.value)} placeholder="Email" />
      <input className="w-full border border-border rounded-md p-2 focus:ring-2 focus:ring-brand-500 focus:border-brand-400" type="password" value={password} onChange={e=>setPassword(e.target.value)} placeholder="Пароль" />
      {err && <div className="text-red-600 text-sm">{err}</div>}
      <button className="px-3 py-2 rounded-md bg-brand-600 hover:bg-brand-700 text-white focus:ring-2 focus:ring-brand-500">Войти</button>
    </form>
    <Link className="text-brand-700 hover:underline" href="/register">Регистрация</Link>
  </div>;
}

