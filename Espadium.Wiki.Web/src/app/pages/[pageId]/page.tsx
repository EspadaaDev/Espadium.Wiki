"use client";
import { useParams } from "next/navigation";
import useSWR from "swr";
import { apiFetch } from "@/src/lib/api";

export default function PageView(){
  const { pageId } = useParams<{pageId:string}>();
  const { data } = useSWR(pageId? `/pages/${pageId}` : null, (url)=>apiFetch(url).then(r=>r.json()));
  if(!data) return <div className="p-4">Загрузка…</div>;
  return <div className="p-4 space-y-4">
    <h1 className="text-2xl font-semibold">{data.title}</h1>
    <pre className="text-sm p-3 border rounded bg-slate-50 overflow-auto">{data.snapshotJson || "(пусто)"}</pre>
    <a className="text-blue-600 hover:underline" href={`/pages/${pageId}/edit`}>Редактировать (простым JSON)</a>
  </div>;
}

