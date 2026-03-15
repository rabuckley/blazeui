import { createFileRoute } from "@tanstack/react-router"
import { Button } from "@/components/ui/button"
import {
  Tooltip,
  TooltipTrigger,
  TooltipContent,
} from "@/components/ui/tooltip"

export const Route = createFileRoute("/tooltip")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Tooltip</h1>
      <div className="flex gap-6">
        <Tooltip>
          <TooltipTrigger render={<Button variant="outline" />}>
            Hover me
          </TooltipTrigger>
          <TooltipContent>
            This is a tooltip
          </TooltipContent>
        </Tooltip>

        <Tooltip>
          <TooltipTrigger render={<Button variant="outline" />}>
            Right
          </TooltipTrigger>
          <TooltipContent side="right">
            Right-side tooltip
          </TooltipContent>
        </Tooltip>

        <Tooltip>
          <TooltipTrigger render={<Button variant="outline" />}>
            Bottom
          </TooltipTrigger>
          <TooltipContent side="bottom">
            Bottom tooltip
          </TooltipContent>
        </Tooltip>
      </div>
    </div>
  ),
})
