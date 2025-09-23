"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";
import { apiFetch } from "@/lib/api";
import { useAuth } from "@/lib/auth-store";

export default function LoginPage() {
  const [email,setEmail]=useState(""); const [password,setPassword]=useState(""); const [err,setErr]=useState<string|null>(null);
  const { setAccessToken } = useAuth();
  const router = useRouter();
  async function submit(e: React.FormEvent){ e.preventDefault(); setErr(null);
    const res = await apiFetch("/auth/login",{ method:"POST", headers:{ "Content-Type":"application/json" }, body: JSON.stringify({ email, password }) });
    if(!res.ok){ setErr("Неверная почта или пароль"); return; }
    const { accessToken } = await res.json(); setAccessToken(accessToken); router.push("/spaces");
  }
  return <div className="max-w-sm mx-auto p-6 space-y-4">
    <h1 className="text-xl font-semibold">Вход</h1>
    <form onSubmit={submit} className="space-y-3">
      <input className="w-full border rounded p-2" value={email} onChange={e=>setEmail(e.target.value)} placeholder="Email" />
      <input className="w-full border rounded p-2" type="password" value={password} onChange={e=>setPassword(e.target.value)} placeholder="Пароль" />
      {err && <div className="text-red-600 text-sm">{err}</div>}
      <button className="px-3 py-2 border rounded hover:bg-slate-100">Войти</button>
    </form>
    <a className="text-blue-600 hover:underline" href="/register">Регистрация</a>
  </div>;
}

