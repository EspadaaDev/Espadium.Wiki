"use client";
import { Search, User, LogIn, UserPlus } from "lucide-react";
import Link from "next/link";

export function Topbar({ onLogout, isAuthed }: { onLogout?: () => void; isAuthed?: boolean }) {
  return (
    <div className="h-12 border-b border-sky-100 bg-white text-slate-800 flex items-center justify-between px-4">
      <Link href="/" className="font-semibold text-sky-700 hover:text-sky-800">Espadium.Wiki</Link>
      <div className="flex items-center gap-3">
        <Link href="/search" className="p-2 hover:bg-sky-50 rounded"><Search className="w-5 h-5 text-sky-700" /></Link>
        <Link href="/account" className="p-2 hover:bg-sky-50 rounded"><User className="w-5 h-5 text-sky-700" /></Link>
        {isAuthed ? (
          <button onClick={onLogout} className="text-sm px-3 py-1 border border-sky-200 text-sky-700 rounded hover:bg-sky-50">Выйти</button>
        ) : (
          <div className="flex items-center gap-2">
            <Link href="/login" className="inline-flex items-center gap-1 text-sky-700 hover:underline"><LogIn className="w-4 h-4"/>Войти</Link>
            <Link href="/register" className="inline-flex items-center gap-1 text-sky-700 hover:underline"><UserPlus className="w-4 h-4"/>Регистрация</Link>
          </div>
        )}
      </div>
    </div>
  );
}

