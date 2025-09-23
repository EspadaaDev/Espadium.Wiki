"use client";
import { useParams } from "next/navigation";
import { Sidebar } from "@/components/sidebar";
import { PageTree } from "@/components/page-tree";

export default function SpacePage(){
  const { spaceId } = useParams<{spaceId:string}>();
  return <div className="flex">
    <Sidebar activeId={spaceId}/>
    <div className="flex-1 p-4">
      <h1 className="text-xl font-semibold mb-2">Страницы</h1>
      <PageTree spaceId={spaceId} />
      <div className="text-slate-500 mt-6">Выберите страницу слева.</div>
    </div>
  </div>;
}

