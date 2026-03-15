import { createFileRoute } from "@tanstack/react-router"
import {
  InputGroup,
  InputGroupAddon,
  InputGroupButton,
  InputGroupText,
  InputGroupInput,
} from "@/components/ui/input-group"
import { SearchIcon } from "lucide-react"

export const Route = createFileRoute("/input-group")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">InputGroup</h1>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">With icon addon</h2>
        <InputGroup className="max-w-sm">
          <InputGroupAddon>
            <SearchIcon />
          </InputGroupAddon>
          <InputGroupInput placeholder="Search..." />
        </InputGroup>
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">With text addon</h2>
        <InputGroup className="max-w-sm">
          <InputGroupAddon>
            <InputGroupText>https://</InputGroupText>
          </InputGroupAddon>
          <InputGroupInput placeholder="example.com" />
        </InputGroup>
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">With trailing addon</h2>
        <InputGroup className="max-w-sm">
          <InputGroupInput placeholder="Enter amount" />
          <InputGroupAddon align="inline-end">
            <InputGroupText>USD</InputGroupText>
          </InputGroupAddon>
        </InputGroup>
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">With button</h2>
        <InputGroup className="max-w-sm">
          <InputGroupInput placeholder="Search..." />
          <InputGroupAddon align="inline-end">
            <InputGroupButton>Go</InputGroupButton>
          </InputGroupAddon>
        </InputGroup>
      </div>
    </div>
  ),
})
