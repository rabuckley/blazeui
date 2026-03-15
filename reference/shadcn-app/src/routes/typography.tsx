import { createFileRoute } from "@tanstack/react-router"

export const Route = createFileRoute("/typography")({
  component: () => (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Typography</h1>

      <div className="space-y-8 max-w-prose">
        <div>
          <h2 className="text-sm font-medium text-muted-foreground mb-3">h1</h2>
          <h1 className="scroll-m-20 text-4xl font-extrabold tracking-tight text-balance">
            Taxing Laughter: The Joke Tax Chronicles
          </h1>
        </div>

        <div>
          <h2 className="text-sm font-medium text-muted-foreground mb-3">h2</h2>
          <h2 className="scroll-m-20 border-b pb-2 text-3xl font-semibold tracking-tight first:mt-0">
            The People of the Kingdom
          </h2>
        </div>

        <div>
          <h2 className="text-sm font-medium text-muted-foreground mb-3">h3</h2>
          <h3 className="scroll-m-20 text-2xl font-semibold tracking-tight">
            The Joke Tax
          </h3>
        </div>

        <div>
          <h2 className="text-sm font-medium text-muted-foreground mb-3">h4</h2>
          <h4 className="scroll-m-20 text-xl font-semibold tracking-tight">
            People stopped telling jokes
          </h4>
        </div>

        <div>
          <h2 className="text-sm font-medium text-muted-foreground mb-3">p</h2>
          <p className="leading-7 [&:not(:first-child)]:mt-6">
            The king, seeing how much happier his subjects were, realized the
            error of his ways and repealed the joke tax.
          </p>
        </div>

        <div>
          <h2 className="text-sm font-medium text-muted-foreground mb-3">
            blockquote
          </h2>
          <blockquote className="mt-6 border-l-2 pl-6 italic">
            "After all," he said, "everyone enjoys a good joke, so it's only
            fair that they should pay for the privilege."
          </blockquote>
        </div>

        <div>
          <h2 className="text-sm font-medium text-muted-foreground mb-3">
            list
          </h2>
          <ul className="my-6 ml-6 list-disc [&>li]:mt-2">
            <li>1st level of puns: 5 gold coins</li>
            <li>2nd level of jokes: 10 gold coins</li>
            <li>3rd level of one-liners: 20 gold coins</li>
          </ul>
        </div>

        <div>
          <h2 className="text-sm font-medium text-muted-foreground mb-3">
            inline code
          </h2>
          <code className="relative rounded bg-muted px-[0.3rem] py-[0.2rem] font-mono text-sm font-semibold">
            @radix-ui/react-alert-dialog
          </code>
        </div>

        <div>
          <h2 className="text-sm font-medium text-muted-foreground mb-3">
            lead
          </h2>
          <p className="text-xl text-muted-foreground">
            A modal dialog that interrupts the user with important content and
            expects a response.
          </p>
        </div>

        <div>
          <h2 className="text-sm font-medium text-muted-foreground mb-3">
            large
          </h2>
          <div className="text-lg font-semibold">Are you absolutely sure?</div>
        </div>

        <div>
          <h2 className="text-sm font-medium text-muted-foreground mb-3">
            small
          </h2>
          <small className="text-sm leading-none font-medium">
            Email address
          </small>
        </div>

        <div>
          <h2 className="text-sm font-medium text-muted-foreground mb-3">
            muted
          </h2>
          <p className="text-sm text-muted-foreground">
            Enter your email address.
          </p>
        </div>
      </div>
    </div>
  ),
})
