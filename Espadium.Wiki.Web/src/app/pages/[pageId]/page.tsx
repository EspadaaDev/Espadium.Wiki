"use client";
import { useParams } from "next/navigation";
import useSWR from "swr";
import { apiFetch } from "@/lib/api";
import Link from "next/link";

type Attachment = { id: string; filename: string; mime: string; size: number; previewKey?: string | null };

export default function PageView(){
  const { pageId } = useParams<{pageId:string}>();
  const { data } = useSWR(pageId? `/pages/${pageId}` : null, (url: string)=>apiFetch(url).then((r: Response)=>r.json()));
  const { data: atts } = useSWR<Attachment[]>(pageId? `/pages/${pageId}/attachments` : null, (url: string)=>apiFetch(url).then((r: Response)=>r.json()));
  if(!data) return <div className="p-4">Загрузка...</div>;
  return <div className="p-4 space-y-4">
    <h1 className="text-2xl font-semibold">{data.title}</h1>
    <pre className="text-sm p-3 border rounded bg-slate-50 overflow-auto">{data.snapshotJson || "(пусто)"}</pre>
    <div>
      <div className="font-medium mb-2">Вложения</div>
      <div className="space-y-1">
        {atts?.map(a => (
          <div key={a.id} className="text-sm flex items-center justify-between border rounded p-2">
            <div className="flex items-center gap-2">
              <span className="font-medium">{a.filename}</span>
              <span className="text-slate-500">{a.mime}</span>
              <span className="text-slate-500">{(a.size/1024).toFixed(1)} KB</span>
            </div>
          </div>
        ))}
        {!atts?.length && <div className="text-sm text-slate-500">Нет вложений</div>}
      </div>
    </div>
    <Link className="text-sky-700 hover:underline" href={`/pages/${pageId}/edit`}>Редактировать</Link>
  </div>;
}

