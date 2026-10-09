import type { Metadata } from "next";
import { PostEditor } from "@/components/admin/content/posts";

export const metadata: Metadata = { title: "Nova publicação" };

export default function Page() {
  return <PostEditor />;
}
