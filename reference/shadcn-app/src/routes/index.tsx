import { createFileRoute } from "@tanstack/react-router"

export const Route = createFileRoute("/")({
  component: () => (
    <div>
      <h1 className="text-2xl font-bold mb-4">shadcn/ui + BaseUI Reference App</h1>
      <p className="text-muted-foreground">
        Select a component from the sidebar to see the reference implementation.
        Compare these against the BlazeUI demo at{" "}
        <code className="text-xs bg-muted px-1 py-0.5 rounded">http://127.0.0.1:5199</code>.
      </p>
    </div>
  ),
})
