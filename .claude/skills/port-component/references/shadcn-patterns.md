# shadcn/ui v4 Common CSS Patterns

Quick reference for recurring Tailwind class patterns across shadcn/ui v4 components.

## Focus Ring

```
outline-none focus-visible:border-ring focus-visible:ring-[3px] focus-visible:ring-ring/50
```

## Disabled

```
disabled:pointer-events-none disabled:opacity-50
```

For data-attribute disabled (custom components):
```
data-[disabled]:pointer-events-none data-[disabled]:opacity-50
```

## Aria Invalid

```
aria-invalid:border-destructive aria-invalid:ring-destructive/20 dark:aria-invalid:ring-destructive/40
```

## SVG Reset

Applied to interactive containers (Button, Toggle, MenuItem, etc.):
```
[&_svg]:pointer-events-none [&_svg]:shrink-0 [&_svg:not([class*='size-'])]:size-4
```

## Overlay Backdrop

```
fixed inset-0 z-50 bg-black/50
data-[open]:animate-in data-[open]:fade-in-0
data-[closed]:animate-out data-[closed]:fade-out-0
data-[closed]:hidden
```

## Overlay Popup (Dialog/AlertDialog)

```
fixed left-1/2 top-1/2 z-50 grid w-full max-w-lg -translate-x-1/2 -translate-y-1/2
gap-4 border bg-background p-6 shadow-lg duration-200 sm:rounded-lg
data-[open]:animate-in data-[open]:fade-in-0 data-[open]:zoom-in-95
data-[closed]:animate-out data-[closed]:fade-out-0 data-[closed]:zoom-out-95
data-[closed]:hidden
```

## Dropdown/Popover Content

```
z-50 min-w-[8rem] rounded-md border bg-popover p-1 text-popover-foreground shadow-md
data-[open]:animate-in data-[open]:fade-in-0 data-[open]:zoom-in-95
data-[closed]:animate-out data-[closed]:fade-out-0 data-[closed]:zoom-out-95
```

## Menu Item

```
relative flex cursor-default items-center gap-2 rounded-sm px-2 py-1.5 text-sm
outline-hidden select-none
focus:bg-accent focus:text-accent-foreground
data-[disabled]:pointer-events-none data-[disabled]:opacity-50
```

## Menu Separator

```
-mx-1 my-1 h-px bg-border
```

## Tooltip Content

```
z-50 overflow-hidden rounded-md bg-foreground px-3 py-1.5 text-xs text-background
animate-in fade-in-0 zoom-in-95
```

## Close Button (inside dialogs)

```
absolute right-4 top-4 rounded-sm opacity-70 ring-offset-background
transition-opacity hover:opacity-100
focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2
```

## Button Variants

| Variant     | Classes |
|-------------|---------|
| default     | `bg-primary text-primary-foreground shadow-xs hover:bg-primary/90` |
| destructive | `bg-destructive text-white shadow-xs hover:bg-destructive/90 focus-visible:ring-destructive/20` |
| outline     | `border border-input bg-background shadow-xs hover:bg-accent hover:text-accent-foreground` |
| secondary   | `bg-secondary text-secondary-foreground shadow-xs hover:bg-secondary/80` |
| ghost       | `hover:bg-accent hover:text-accent-foreground` |
| link        | `text-primary underline-offset-4 hover:underline` |

## Button Sizes

| Size    | Classes |
|---------|---------|
| default | `h-9 px-4 py-2 has-[>svg]:px-3` |
| sm      | `h-8 gap-1.5 rounded-md px-3 has-[>svg]:px-2.5` |
| lg      | `h-10 rounded-md px-6 has-[>svg]:px-4` |
| icon    | `size-9` |

## Theme Color Tokens

These map to `--blazeui-*` CSS custom properties via `@theme` in input.css:

| Token | Usage |
|-------|-------|
| `background` / `foreground` | Page background, default text |
| `primary` / `primary-foreground` | Buttons, links, active indicators |
| `secondary` / `secondary-foreground` | Secondary buttons, tags |
| `muted` / `muted-foreground` | Subtle backgrounds, placeholder text |
| `accent` / `accent-foreground` | Hover states, toggle pressed state |
| `destructive` / `destructive-foreground` | Error states, delete buttons |
| `border` | Default borders |
| `input` | Form input borders |
| `ring` | Focus ring color |
| `popover` / `popover-foreground` | Dropdown/popup backgrounds |
| `card` / `card-foreground` | Card surfaces |
