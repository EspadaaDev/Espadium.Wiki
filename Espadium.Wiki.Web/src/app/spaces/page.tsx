"use client";
import useSWR from "swr";
import Link from "next/link";
import { apiFetch } from "@/lib/api";

type Space = { id: string; key: string; name: string };

export default function SpacesPage(){
  const { data } = useSWR<Space[]>("/spaces", ()=>apiFetch("/spaces").then((r: Response)=>r.json()));
  return <div className="p-4 space-y-2">
    <h1 className="text-xl font-semibold mb-2">Spaces</h1>
    <div className="grid gap-2">
      {data?.map(s => <Link key={s.id} href={`/spaces/${s.id}`} className="border rounded p-3 hover:bg-slate-50">{s.name}</Link>)}
    </div>
  </div>;
}

