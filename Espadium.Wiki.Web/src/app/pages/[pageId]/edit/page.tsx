"use client";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiFetch } from "@/lib/api";

export default function EditSimple(){
  const { pageId } = useParams<{pageId:string}>(); const router = useRouter();
  const [title,setTitle]=useState(""); const [snapshot,setSnapshot]=useState("{}"); const [loading,setLoading]=useState(true);

  useEffect(()=>{ (async()=>{
    const r = await apiFetch(`/pages/${pageId}`); if(!r.ok) return;
    const p = await r.json(); setTitle(p.title || ""); setSnapshot(p.snapshotJson || "{}"); setLoading(false);
  })(); },[pageId]);

  async function save(){
    const res = await apiFetch(`/pages/${pageId}`, { method:"PATCH", headers:{ "Content-Type":"application/json" }, body: JSON.stringify({ title, snapshotJson: snapshot }) });
    if(res.ok) router.push(`/pages/${pageId}`);
  }

  if(loading) return <div className="p-4">Загрузка...</div>;
  return <div className="p-4 space-y-3">
    <input className="w-full border rounded p-2" value={title} onChange={e=>setTitle(e.target.value)} />
    <textarea className="w-full border rounded p-2 h-80 font-mono" value={snapshot} onChange={e=>setSnapshot(e.target.value)} />
    <div className="flex gap-2">
      <button onClick={save} className="px-3 py-1 border rounded hover:bg-slate-100">Сохранить</button>
      <a href={`/pages/${pageId}`} className="px-3 py-1 border rounded hover:bg-slate-100">Отмена</a>
    </div>
  </div>;
}

