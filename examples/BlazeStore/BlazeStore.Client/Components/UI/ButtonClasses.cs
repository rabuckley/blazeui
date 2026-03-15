namespace BlazeStore.Client.Components.UI;

/// <summary>
/// Shared Button class strings for composing trigger elements that render as
/// buttons (mirroring shadcn's <c>render={&lt;Button /&gt;}</c> pattern).
/// </summary>
internal static class ButtonClasses
{
    internal const string Base =
        "group/button inline-flex shrink-0 items-center justify-center rounded-lg border border-transparent bg-clip-padding text-sm font-medium whitespace-nowrap transition-all outline-none select-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 active:translate-y-px disabled:pointer-events-none disabled:opacity-50 aria-invalid:border-destructive aria-invalid:ring-3 aria-invalid:ring-destructive/20 dark:aria-invalid:border-destructive/50 dark:aria-invalid:ring-destructive/40 [&_svg]:pointer-events-none [&_svg]:shrink-0 [&_svg:not([class*='size-'])]:size-4";

    internal static string GetVariant(Button.ButtonVariant variant) => variant switch
    {
        Button.ButtonVariant.Default => "bg-primary text-primary-foreground [a]:hover:bg-primary/80",
        Button.ButtonVariant.Destructive => "bg-destructive/10 text-destructive hover:bg-destructive/20 focus-visible:border-destructive/40 focus-visible:ring-destructive/20 dark:bg-destructive/20 dark:hover:bg-destructive/30 dark:focus-visible:ring-destructive/40",
        Button.ButtonVariant.Outline => "border-border bg-background hover:bg-muted hover:text-foreground aria-expanded:bg-muted aria-expanded:text-foreground dark:border-input dark:bg-input/30 dark:hover:bg-input/50",
        Button.ButtonVariant.Secondary => "bg-secondary text-secondary-foreground hover:bg-secondary/80 aria-expanded:bg-secondary aria-expanded:text-secondary-foreground",
        Button.ButtonVariant.Ghost => "hover:bg-muted hover:text-foreground aria-expanded:bg-muted aria-expanded:text-foreground dark:hover:bg-muted/50",
        Button.ButtonVariant.Link => "text-primary underline-offset-4 hover:underline",
        _ => ""
    };

    internal static string GetSize(Button.ButtonSize size) => size switch
    {
        Button.ButtonSize.Default => "h-8 gap-1.5 px-2.5 has-data-[icon=inline-end]:pr-2 has-data-[icon=inline-start]:pl-2",
        Button.ButtonSize.Sm => "h-7 gap-1 rounded-[min(var(--radius-md),12px)] px-2.5 text-[0.8rem] in-data-[slot=button-group]:rounded-lg has-data-[icon=inline-end]:pr-1.5 has-data-[icon=inline-start]:pl-1.5 [&_svg:not([class*='size-'])]:size-3.5",
        Button.ButtonSize.Lg => "h-9 gap-1.5 px-2.5 has-data-[icon=inline-end]:pr-3 has-data-[icon=inline-start]:pl-3",
        Button.ButtonSize.Icon => "size-8",
        _ => ""
    };
}
