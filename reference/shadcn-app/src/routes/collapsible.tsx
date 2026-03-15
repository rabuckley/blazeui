import * as React from "react"
import { createFileRoute } from "@tanstack/react-router"
import { Button } from "@/components/ui/button"
import {
  Collapsible,
  CollapsibleTrigger,
  CollapsibleContent,
} from "@/components/ui/collapsible"
import { ChevronsUpDownIcon } from "lucide-react"

function CollapsibleDemo() {
  const [isOpen, setIsOpen] = React.useState(false)

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Collapsible</h1>
      <div className="w-80 space-y-2">
        <Collapsible open={isOpen} onOpenChange={setIsOpen}>
          <div className="flex items-center justify-between space-x-4 rounded-md border px-4 py-2">
            <h4 className="text-sm font-semibold">
              @peduarte starred 3 repositories
            </h4>
            <CollapsibleTrigger
              render={
                <Button variant="ghost" size="icon" />
              }
            >
              <ChevronsUpDownIcon className="h-4 w-4" />
              <span className="sr-only">Toggle</span>
            </CollapsibleTrigger>
          </div>
          <div className="rounded-md border px-4 py-2 text-sm font-mono">
            @radix-ui/primitives
          </div>
          <CollapsibleContent className="space-y-2">
            <div className="rounded-md border px-4 py-2 text-sm font-mono">
              @radix-ui/colors
            </div>
            <div className="rounded-md border px-4 py-2 text-sm font-mono">
              @stitches/react
            </div>
          </CollapsibleContent>
        </Collapsible>
      </div>
    </div>
  )
}

export const Route = createFileRoute("/collapsible")({
  component: CollapsibleDemo,
})
