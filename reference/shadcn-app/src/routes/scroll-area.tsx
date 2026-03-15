import { createFileRoute } from "@tanstack/react-router"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Separator } from "@/components/ui/separator"

const tags = Array.from({ length: 50 }, (_, i) => `Tag ${i + 1}`)

export const Route = createFileRoute("/scroll-area")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Scroll Area</h1>
      <div>
        <ScrollArea className="h-72 w-48 rounded-md border">
          <div className="p-4">
            <h4 className="mb-4 text-sm font-medium leading-none">Tags</h4>
            {tags.map((tag) => (
              <div key={tag}>
                <div className="text-sm">{tag}</div>
                <Separator className="my-2" />
              </div>
            ))}
          </div>
        </ScrollArea>
      </div>
    </div>
  ),
})
