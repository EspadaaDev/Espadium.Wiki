"use client";
import useSWR from 'swr';
import Link from 'next/link';
import { apiFetch } from '@/src/lib/api';

type Space = { id: string; key: string; name: string };

export function Sidebar({ activeId }: { activeId?: string }) {
  const { data } = useSWR<Space[]>("/spaces", (url) => apiFetch("/spaces").then(r => r.json()));
  return (
    <div className="w-64 border-r bg-slate-50 h-[calc(100vh-3rem)] overflow-y-auto p-2 text-sm">
      <div className="mb-2 px-2 text-slate-500 uppercase">Spaces</div>
      <div className="space-y-1">
        {data?.map(s => (
          <Link key={s.id} href={`/spaces/${s.id}`} className={`block px-3 py-2 rounded hover:bg-slate-100 ${activeId===s.id? 'bg-slate-200 font-medium':''}`}>{s.name}</Link>
        ))}
      </div>
    </div>
  );
}

