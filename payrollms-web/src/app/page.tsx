import { redirect } from "next/navigation"

export default function Home() {
  // Temporary redirect for development/testing
  redirect("/superadmin/dashboard")
}
