import { createRootRoute, Link, Outlet, useRouterState } from "@tanstack/react-router"
import { TooltipProvider } from "@/components/ui/tooltip"
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarSeparator,
  SidebarTrigger,
} from "@/components/ui/sidebar"

const simpleComponents = [
  { label: "Alert", to: "/alert" },
  { label: "AspectRatio", to: "/aspect-ratio" },
  { label: "Avatar", to: "/avatar" },
  { label: "Badge", to: "/badge" },
  { label: "Breadcrumb", to: "/breadcrumb" },
  { label: "Button", to: "/button" },
  { label: "ButtonGroup", to: "/button-group" },
  { label: "Card", to: "/card" },
  { label: "Checkbox", to: "/checkbox" },
  { label: "Empty", to: "/empty" },
  { label: "Input", to: "/input" },
  { label: "InputGroup", to: "/input-group" },
  { label: "Item", to: "/item" },
  { label: "Kbd", to: "/kbd" },
  { label: "Pagination", to: "/pagination" },
  { label: "Progress", to: "/progress" },
  { label: "Radio", to: "/radio" },
  { label: "Separator", to: "/separator" },
  { label: "Skeleton", to: "/skeleton" },
  { label: "Slider", to: "/slider" },
  { label: "Spinner", to: "/spinner" },
  { label: "Switch", to: "/switch" },
  { label: "Table", to: "/table" },
  { label: "Toggle", to: "/toggle" },
  { label: "ToggleGroup", to: "/toggle-group" },
  { label: "Typography", to: "/typography" },
] as const

const overlayComponents = [
  { label: "AlertDialog", to: "/alert-dialog" },
  { label: "Combobox", to: "/combobox" },
  { label: "ContextMenu", to: "/context-menu" },
  { label: "Dialog", to: "/dialog" },
  { label: "Drawer", to: "/drawer" },
  { label: "HoverCard", to: "/preview-card" },
  { label: "Menu", to: "/menu" },
  { label: "Popover", to: "/popover" },
  { label: "Select", to: "/select" },
  { label: "Sheet", to: "/sheet" },
  { label: "Toast", to: "/toast" },
  { label: "Tooltip", to: "/tooltip" },
] as const

const layoutComponents = [
  { label: "Accordion", to: "/accordion" },
  { label: "Collapsible", to: "/collapsible" },
  { label: "Menubar", to: "/menubar" },
  { label: "NavigationMenu", to: "/navigation-menu" },
  { label: "ScrollArea", to: "/scroll-area" },
  { label: "Sidebar", to: "/sidebar" },
  { label: "Tabs", to: "/tabs" },
] as const

const formComponents = [
  { label: "Field", to: "/field" },
  { label: "InputOTP", to: "/input-otp" },
  { label: "Label", to: "/label" },
  { label: "NativeSelect", to: "/native-select" },
  { label: "Textarea", to: "/textarea" },
] as const

const blockComponents = [
  { label: "Login-02", to: "/blocks/login-02" },
  { label: "Sidebar-01", to: "/blocks/sidebar-01" },
  { label: "Sidebar-07", to: "/blocks/sidebar-07" },
] as const

function NavGroup({
  label,
  items,
}: {
  label: string
  items: ReadonlyArray<{ label: string; to: string }>
}) {
  const pathname = useRouterState({ select: (s) => s.location.pathname })

  return (
    <SidebarGroup>
      <SidebarGroupLabel>{label}</SidebarGroupLabel>
      <SidebarGroupContent>
        <SidebarMenu>
          {items.map((item) => (
            <SidebarMenuItem key={item.to}>
              <SidebarMenuButton
                isActive={pathname === item.to}
                render={<Link to={item.to} />}
              >
                <span>{item.label}</span>
              </SidebarMenuButton>
            </SidebarMenuItem>
          ))}
        </SidebarMenu>
      </SidebarGroupContent>
    </SidebarGroup>
  )
}

function RootComponent() {
  const pathname = useRouterState({ select: (s) => s.location.pathname })
  const isBlock = pathname.startsWith("/blocks/")

  // Block pages render standalone — they provide their own full-page layout.
  if (isBlock) {
    return (
      <TooltipProvider>
        <Outlet />
      </TooltipProvider>
    )
  }

  return (
    <TooltipProvider>
      <SidebarProvider>
        <Sidebar>
          <SidebarHeader>
            <div className="flex items-center gap-2 px-2 py-1">
              <span className="text-lg font-bold">BlazeUI</span>
            </div>
          </SidebarHeader>
          <SidebarContent>
            <NavGroup label="Simple" items={simpleComponents} />
            <SidebarSeparator />
            <NavGroup label="Overlay" items={overlayComponents} />
            <SidebarSeparator />
            <NavGroup label="Layout" items={layoutComponents} />
            <SidebarSeparator />
            <NavGroup label="Form" items={formComponents} />
            <SidebarSeparator />
            <NavGroup label="Blocks" items={blockComponents} />
          </SidebarContent>
          <SidebarFooter>
            <div className="px-2 text-xs text-sidebar-foreground/50">
              BlazeUI Showcase
            </div>
          </SidebarFooter>
        </Sidebar>
        <SidebarInset>
          <header className="flex h-12 items-center gap-2 border-b px-4">
            <SidebarTrigger />
            <div className="mx-2 h-4 w-px bg-border" />
            <span className="text-sm text-muted-foreground">
              Component Showcase
            </span>
          </header>
          <div className="flex-1 p-6">
            <Outlet />
          </div>
        </SidebarInset>
      </SidebarProvider>
    </TooltipProvider>
  )
}

export const Route = createRootRoute({
  component: RootComponent,
})
