import Link from "next/link";
export default function Home(){
  return (<div className="p-6"><Link href="/spaces" className="text-blue-600 hover:underline">Перейти к пространствам</Link></div>);
}
