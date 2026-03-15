import { createFileRoute } from "@tanstack/react-router"
import {
  NativeSelect,
  NativeSelectOption,
} from "@/components/ui/native-select"

export const Route = createFileRoute("/native-select")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Native Select</h1>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">
          Default
        </h2>
        <NativeSelect>
          <NativeSelectOption value="">Select a fruit</NativeSelectOption>
          <NativeSelectOption value="apple">Apple</NativeSelectOption>
          <NativeSelectOption value="banana">Banana</NativeSelectOption>
          <NativeSelectOption value="blueberry">Blueberry</NativeSelectOption>
          <NativeSelectOption value="pineapple">Pineapple</NativeSelectOption>
        </NativeSelect>
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">
          Disabled
        </h2>
        <NativeSelect disabled>
          <NativeSelectOption value="">Select a fruit</NativeSelectOption>
          <NativeSelectOption value="apple">Apple</NativeSelectOption>
        </NativeSelect>
      </div>
    </div>
  ),
})
