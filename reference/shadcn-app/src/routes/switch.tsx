import { useState } from "react"
import { createFileRoute } from "@tanstack/react-router"
import { Switch } from "@/components/ui/switch"

function SwitchDemo() {
  const [airplane, setAirplane] = useState(true)
  const [darkMode, setDarkMode] = useState(false)

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Switch</h1>
      <div className="space-y-4">
        <div className="flex items-center gap-3">
          <Switch
            id="airplane"
            checked={airplane}
            onCheckedChange={setAirplane}
          />
          <span className="text-sm">Airplane mode</span>
        </div>
        <div className="flex items-center gap-3">
          <Switch
            id="dark-mode"
            checked={darkMode}
            onCheckedChange={setDarkMode}
          />
          <span className="text-sm">Dark mode</span>
        </div>
        <div className="flex items-center gap-3">
          <Switch id="disabled" disabled />
          <span className="text-sm text-muted-foreground">Disabled</span>
        </div>
      </div>
    </div>
  )
}

export const Route = createFileRoute("/switch")({
  component: SwitchDemo,
})
