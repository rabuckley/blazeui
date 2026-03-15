import { createFileRoute } from "@tanstack/react-router"
import { Textarea } from "@/components/ui/textarea"

export const Route = createFileRoute("/textarea")({
  component: () => (
    <div>
      <h1 className="text-2xl font-bold mb-6">Textarea</h1>
      <div className="max-w-sm space-y-4">
        <Textarea placeholder="Type your message here." />
        <Textarea placeholder="Disabled" disabled />
      </div>
    </div>
  ),
})
