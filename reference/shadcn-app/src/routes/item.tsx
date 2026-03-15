import { createFileRoute } from "@tanstack/react-router"
import { Button } from "@/components/ui/button"
import {
  Item,
  ItemGroup,
  ItemSeparator,
  ItemMedia,
  ItemContent,
  ItemTitle,
  ItemDescription,
  ItemActions,
} from "@/components/ui/item"
import { FileTextIcon } from "lucide-react"

export const Route = createFileRoute("/item")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Item</h1>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Default</h2>
        <ItemGroup>
          <Item>
            <ItemMedia variant="icon">
              <FileTextIcon />
            </ItemMedia>
            <ItemContent>
              <ItemTitle>Document.pdf</ItemTitle>
              <ItemDescription>Updated 2 days ago</ItemDescription>
            </ItemContent>
            <ItemActions>
              <Button variant="ghost" size="sm">Open</Button>
            </ItemActions>
          </Item>
          <ItemSeparator />
          <Item>
            <ItemMedia variant="icon">
              <FileTextIcon />
            </ItemMedia>
            <ItemContent>
              <ItemTitle>Report.xlsx</ItemTitle>
              <ItemDescription>Updated 5 days ago</ItemDescription>
            </ItemContent>
            <ItemActions>
              <Button variant="ghost" size="sm">Open</Button>
            </ItemActions>
          </Item>
        </ItemGroup>
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Outline variant</h2>
        <Item variant="outline">
          <ItemContent>
            <ItemTitle>Outline item</ItemTitle>
            <ItemDescription>This item has a visible border.</ItemDescription>
          </ItemContent>
        </Item>
      </div>

      <div>
        <h2 className="text-sm font-medium text-muted-foreground mb-3">Muted variant</h2>
        <Item variant="muted">
          <ItemContent>
            <ItemTitle>Muted item</ItemTitle>
            <ItemDescription>This item has a muted background.</ItemDescription>
          </ItemContent>
        </Item>
      </div>
    </div>
  ),
})
