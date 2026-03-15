import { createFileRoute } from "@tanstack/react-router"
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select"

export const Route = createFileRoute("/select")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Select</h1>
      <div className="max-w-xs">
        <Select>
          <SelectTrigger>
            <SelectValue placeholder="Choose a fruit..." />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="apple">Apple</SelectItem>
            <SelectItem value="banana">Banana</SelectItem>
            <SelectItem value="cherry">Cherry</SelectItem>
          </SelectContent>
        </Select>
      </div>
    </div>
  ),
})
