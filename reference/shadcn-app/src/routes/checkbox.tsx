import { useState } from "react"
import { createFileRoute } from "@tanstack/react-router"
import { Checkbox } from "@/components/ui/checkbox"

function CheckboxDemo() {
  const [checked, setChecked] = useState(true)
  const [unchecked, setUnchecked] = useState(false)

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Checkbox</h1>
      <div className="space-y-4">
        <div className="flex items-center gap-2">
          <Checkbox
            id="terms"
            checked={checked}
            onCheckedChange={(val) => setChecked(val as boolean)}
          />
          <label htmlFor="terms" className="text-sm">Accept terms and conditions</label>
        </div>
        <div className="flex items-center gap-2">
          <Checkbox
            id="unchecked"
            checked={unchecked}
            onCheckedChange={(val) => setUnchecked(val as boolean)}
          />
          <label htmlFor="unchecked" className="text-sm">Unchecked by default</label>
        </div>
        <div className="flex items-center gap-2">
          <Checkbox id="disabled" disabled />
          <label htmlFor="disabled" className="text-sm text-muted-foreground">Disabled</label>
        </div>
      </div>
    </div>
  )
}

export const Route = createFileRoute("/checkbox")({
  component: CheckboxDemo,
})
