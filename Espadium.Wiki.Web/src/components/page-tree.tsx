"use client";
import useSWR from 'swr';
import Link from 'next/link';
import { ChevronDown, ChevronRight } from 'lucide-react';
import { apiFetch } from '@/lib/api';
import { useState } from 'react';

type Node = { id: string; title: string; children?: Node[] };

export function PageTree({ spaceId }: { spaceId: string }) {
  const { data } = useSWR<Node[]>(spaceId ? `/spaces/${spaceId}/pages/tree` : null, (url: string) => apiFetch(url).then((r: Response) => r.json()));
  if (!spaceId) return null;
  return (
    <div className="p-2 text-sm">
      {data?.map(n => <TreeNode key={n.id} node={n} />)}
    </div>
  );
}

function TreeNode({ node }: { node: Node }) {
  const [open, setOpen] = useState(false);
  const hasChildren = !!node.children?.length;
  return (
    <div className="pl-2">
      <div className="flex items-center gap-1">
        {hasChildren ? (
          <button onClick={() => setOpen(!open)} className="p-1 hover:bg-slate-100 rounded">
            {open ? <ChevronDown className="w-4 h-4"/> : <ChevronRight className="w-4 h-4"/>}
          </button>
        ) : <span className="w-6"/>}
        <Link href={`/pages/${node.id}`} className="px-1 py-0.5 hover:bg-slate-100 rounded">
          {node.title}
        </Link>
      </div>
      {open && hasChildren && (
        <div className="pl-4">
          {node.children!.map(c => <TreeNode key={c.id} node={c} />)}
        </div>
      )}
    </div>
  );
}

