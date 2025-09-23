"use client";
import { useState } from "react";
import { apiFetch } from "@/lib/api";

export default function RegisterPage() {
  const [email,setEmail]=useState(""); const [password,setPassword]=useState(""); const [ok,setOk]=useState(false);
  async function submit(e: React.FormEvent){ e.preventDefault();
    const res = await apiFetch("/auth/register",{ method:"POST", headers:{ "Content-Type":"application/json" }, body: JSON.stringify({ email, password }) });
    setOk(res.ok);
  }
  return <div className="max-w-sm mx-auto p-6 space-y-4 bg-surface rounded-md shadow-[0_1px_1px_rgba(0,0,0,.04),_0_4px_8px_rgba(9,74,154,.06)]">
    <h1 className="text-xl font-semibold">Регистрация</h1>
    {ok ? <div className="text-green-700">Проверьте почту для подтверждения.</div> : (
      <form onSubmit={submit} className="space-y-3">
        <input className="w-full border border-border rounded-md p-2 focus:ring-2 focus:ring-brand-500 focus:border-brand-400" value={email} onChange={e=>setEmail(e.target.value)} placeholder="Email" />
        <input className="w-full border border-border rounded-md p-2 focus:ring-2 focus:ring-brand-500 focus:border-brand-400" type="password" value={password} onChange={e=>setPassword(e.target.value)} placeholder="Пароль" />
        <button className="px-3 py-2 rounded-md bg-brand-600 hover:bg-brand-700 text-white focus:ring-2 focus:ring-brand-500">Зарегистрироваться</button>
      </form>
    )}
  </div>;
}

