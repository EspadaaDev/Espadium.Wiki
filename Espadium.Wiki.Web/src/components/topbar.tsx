"use client";
import { Search, User } from "lucide-react";
import Link from "next/link";

export function Topbar({ onLogout }: { onLogout?: () => void }) {
  return (
    <div className="h-12 border-b bg-white text-slate-800 flex items-center justify-between px-4">
      <Link href="/" className="font-semibold">Espadium.Wiki</Link>
      <div className="flex items-center gap-3">
        <Link href="/search" className="p-2 hover:bg-slate-100 rounded"><Search className="w-5 h-5" /></Link>
        <Link href="/account" className="p-2 hover:bg-slate-100 rounded"><User className="w-5 h-5" /></Link>
        <button onClick={onLogout} className="text-sm px-3 py-1 border rounded hover:bg-slate-100">Выйти</button>
      </div>
    </div>
  );
}

