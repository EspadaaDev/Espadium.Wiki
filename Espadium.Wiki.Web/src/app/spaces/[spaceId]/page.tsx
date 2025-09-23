"use client";
import { useParams } from "next/navigation";
import { Sidebar } from "@/components/sidebar";
import { PageTree } from "@/components/page-tree";
import React from "react";
import { apiFetch } from "@/lib/api";

export default function SpacePage(){
  const { spaceId } = useParams<{spaceId:string}>();
  return <div className="grid grid-cols-[16rem_1fr]">
    <Sidebar activeId={spaceId}/>
    <div className="p-4 space-y-3">
      <h1 className="text-xl font-semibold mb-2">Страницы</h1>
      <div className="flex gap-2">
        <CreatePageButton spaceId={spaceId} />
        <button onClick={()=>location.reload()} className="px-3 py-2 rounded border border-sky-200 text-sky-700 hover:bg-sky-50">Обновить дерево</button>
      </div>
      <PageTree spaceId={spaceId} />
      <div className="text-slate-500 mt-6">Выберите страницу слева.</div>
    </div>
  </div>;
}

function CreatePageButton({ spaceId }: { spaceId: string }){
  const [open, setOpen] = React.useState(false);
  const [title, setTitle] = React.useState("");
  async function submit(e: React.FormEvent){ e.preventDefault();
    const res = await apiFetch('/pages', { method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify({ spaceId, title, slug: title.toLowerCase().replace(/\s+/g,'-') }) });
    if(res.ok){ setOpen(false); setTitle(""); location.reload(); }
  }
  return open ? (
    <form onSubmit={submit} className="flex items-center gap-2">
      <input className="border rounded p-2" placeholder="Название страницы" value={title} onChange={e=>setTitle(e.target.value)} />
      <button className="px-3 py-2 rounded bg-sky-600 text-white hover:bg-sky-700">Создать страницу</button>
      <button type="button" onClick={()=>setOpen(false)} className="px-3 py-2 rounded border border-sky-200 text-sky-700 hover:bg-sky-50">Отмена</button>
    </form>
  ) : (
    <button onClick={()=>setOpen(true)} className="px-3 py-2 rounded bg-sky-600 text-white hover:bg-sky-700">Создать страницу</button>
  );
}

