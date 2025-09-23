"use client";
import useSWR from "swr";
import Link from "next/link";
import { apiFetch } from "@/lib/api";
import React from "react";

type Space = { id: string; key: string; name: string };

export default function SpacesPage(){
  const { data, mutate } = useSWR<Space[]>("/spaces", ()=>apiFetch("/spaces").then((r: Response)=>r.json()));
  return <div className="p-4 space-y-2">
    <h1 className="text-xl font-semibold mb-2">Spaces</h1>
    <CreateSpace onCreated={()=>mutate()} />
    <div className="grid gap-2">
      {data?.map(s => <Link key={s.id} href={`/spaces/${s.id}`} className="border rounded p-3 hover:bg-sky-50">{s.name}</Link>)}
    </div>
  </div>;
}

function CreateSpace({ onCreated }: { onCreated: () => void }){
  const [open, setOpen] = React.useState(false);
  const [key, setKey] = React.useState("");
  const [name, setName] = React.useState("");
  const [error, setError] = React.useState<string|undefined>();
  async function submit(e: React.FormEvent){ e.preventDefault(); setError(undefined);
    const res = await apiFetch('/spaces', { method: 'POST', headers: { 'Content-Type':'application/json' }, body: JSON.stringify({ key, name }) });
    if(!res.ok){ setError('Ошибка создания пространства'); return; }
    setOpen(false); setKey(""); setName(""); onCreated();
  }
  return (
    <div className="mb-3">
      {open ? (
        <form onSubmit={submit} className="flex items-center gap-2">
          <input className="border rounded p-2 w-32" placeholder="KEY" value={key} onChange={e=>setKey(e.target.value)} />
          <input className="border rounded p-2 w-64" placeholder="Name" value={name} onChange={e=>setName(e.target.value)} />
          <button className="px-3 py-2 rounded bg-sky-600 text-white hover:bg-sky-700">Создать</button>
          <button type="button" onClick={()=>setOpen(false)} className="px-3 py-2 rounded border border-sky-200 text-sky-700 hover:bg-sky-50">Отмена</button>
          {error && <span className="text-red-600 text-sm">{error}</span>}
        </form>
      ) : (
        <button onClick={()=>setOpen(true)} className="px-3 py-2 rounded bg-sky-600 text-white hover:bg-sky-700">Создать пространство</button>
      )}
    </div>
  );
}

