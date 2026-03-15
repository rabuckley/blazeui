import { createFileRoute } from "@tanstack/react-router"
import { Input } from "@/components/ui/input"

export const Route = createFileRoute("/input")({
  component: () => (
    <div>
      <h1 className="text-2xl font-bold mb-6">Input</h1>
      <div className="max-w-sm space-y-4">
        <Input placeholder="Default input" />
        <Input placeholder="Disabled" disabled />
        <Input type="email" placeholder="Email" />
        <Input type="password" placeholder="Password" />
      </div>
    </div>
  ),
})
