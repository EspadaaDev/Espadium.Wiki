"use client";
import { useState } from "react";
import { apiFetch } from "@/src/lib/api";

export default function RegisterPage() {
  const [email,setEmail]=useState(""); const [password,setPassword]=useState(""); const [ok,setOk]=useState(false);
  async function submit(e: React.FormEvent){ e.preventDefault();
    const res = await apiFetch("/auth/register",{ method:"POST", headers:{ "Content-Type":"application/json" }, body: JSON.stringify({ email, password }) });
    setOk(res.ok);
  }
  return <div className="max-w-sm mx-auto p-6 space-y-4">
    <h1 className="text-xl font-semibold">Регистрация</h1>
    {ok ? <div className="text-green-700">Проверьте почту для подтверждения.</div> : (
      <form onSubmit={submit} className="space-y-3">
        <input className="w-full border rounded p-2" value={email} onChange={e=>setEmail(e.target.value)} placeholder="Email" />
        <input className="w-full border rounded p-2" type="password" value={password} onChange={e=>setPassword(e.target.value)} placeholder="Пароль" />
        <button className="px-3 py-2 border rounded hover:bg-slate-100">Зарегистрироваться</button>
      </form>
    )}
  </div>;
}

