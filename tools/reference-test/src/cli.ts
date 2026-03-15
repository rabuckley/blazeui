#!/usr/bin/env node

import { Command } from "commander";
import { extractCommand } from "./commands/extract.js";
import { diffCommand } from "./commands/diff.js";
import { batchCommand } from "./commands/batch.js";
import { animateCommand } from "./commands/animate.js";
import { inspectCommand } from "./commands/inspect.js";
import { verifyCommand } from "./commands/verify.js";
import { isolateCommand } from "./commands/isolate.js";

const program = new Command();

program
  .name("reftest")
  .description(
    "Compare BlazeUI styled components against shadcn/ui reference app.\n\n" +
      "Setup: cd tools/reference-test && pnpm install && pnpm exec playwright install chromium"
  )
  .version("1.0.0");

program
  .command("extract")
  .description("Extract computed styles from a page into structured JSON")
  .argument("<url>", "URL to extract styles from")
  .option("--blazor", "Wait for Blazor runtime before extracting")
  .option("--interact <component>", "Run interaction steps before extracting")
  .option("--scope <selector>", "CSS selector to scope extraction to")
  .option("--pseudo-states <states>", "Comma-separated pseudo-states to capture (e.g. hover,focus-visible)")
  .option("-o, --output <file>", "Write output to file instead of stdout")
  .action(async (url: string, options) => {
    try {
      await extractCommand(url, options);
    } catch (err) {
      console.error(`Error: ${err}`);
      process.exit(2);
    }
  });

program
  .command("diff")
  .description("Compare two style snapshots")
  .argument("<ref>", "Reference snapshot JSON file")
  .argument("<test>", "Test snapshot JSON file")
  .option(
    "--format <format>",
    "Output format: json, summary, or detail",
    "json"
  )
  .option("--resolved", "Skip CSS custom properties, compare only resolved values")
  .action(async (refPath: string, testPath: string, options) => {
    try {
      await diffCommand(refPath, testPath, options);
    } catch (err) {
      console.error(`Error: ${err}`);
      process.exit(2);
    }
  });

program
  .command("batch")
  .description("End-to-end batch comparison of components")
  .argument("[components...]", "Components to test (default: all)")
  .option(
    "--ref-base <url>",
    "Reference app base URL",
    "http://localhost:5200"
  )
  .option(
    "--test-base <url>",
    "Test app base URL",
    "http://127.0.0.1:5199"
  )
  .option(
    "--format <format>",
    "Output format: json, summary, or detail",
    "summary"
  )
  .option("--resolved", "Skip CSS custom properties, compare only resolved values")
  .option("--pseudo-states <states>", "Comma-separated pseudo-states to capture (e.g. hover,focus-visible)")
  .action(async (components: string[], options) => {
    try {
      await batchCommand(components, options);
    } catch (err) {
      console.error(`Error: ${err}`);
      process.exit(2);
    }
  });

program
  .command("animate")
  .description("Compare open/close animation trajectories between ref and test")
  .argument("<component>", "Component name (must have targetSelector in registry)")
  .option(
    "--ref-base <url>",
    "Reference app base URL",
    "http://localhost:5200"
  )
  .option(
    "--test-base <url>",
    "Test app base URL",
    "http://127.0.0.1:5199"
  )
  .option("--timeout <ms>", "Time to wait for animation to complete", "500")
  .option("--format <format>", "Output format: json or summary", "summary")
  .option(
    "--mode <mode>",
    "Animation mode: close (single open/close) or switch (item switching)",
    "close"
  )
  .action(async (component: string, options) => {
    try {
      await animateCommand(component, options);
    } catch (err) {
      console.error(`Error: ${err}`);
      process.exit(2);
    }
  });

program
  .command("inspect")
  .description("Dump DOM HTML of a specific element for quick inspection")
  .argument("<url>", "URL or component name (when --diff is used)")
  .argument("<selector>", "CSS selector for the target element")
  .option("--blazor", "Wait for Blazor runtime")
  .option("--depth <n>", "0 = element outerHTML, 1 = parent innerHTML", "0")
  .option("--interact <component>", "Run interaction steps before inspecting")
  .option("--diff", "Compare HTML from both ref and test apps (first arg is component name)")
  .option("--no-main", "Don't auto-scope selector to <main> element")
  .option("--ref-base <url>", "Reference app base URL (with --diff)", "http://localhost:5200")
  .option("--test-base <url>", "Test app base URL (with --diff)", "http://127.0.0.1:5199")
  .action(async (url: string, selector: string, options) => {
    try {
      await inspectCommand(url, selector, options);
    } catch (err) {
      console.error(`Error: ${err}`);
      process.exit(2);
    }
  });

program
  .command("verify")
  .description("Unified pass/fail check: CSS diff + animation + behavioral analysis")
  .argument("[components...]", "Components to verify (default: all)")
  .option(
    "--ref-base <url>",
    "Reference app base URL",
    "http://localhost:5200"
  )
  .option(
    "--test-base <url>",
    "Test app base URL",
    "http://127.0.0.1:5199"
  )
  .option(
    "--format <format>",
    "Output format: summary, json, or detail",
    "summary"
  )
  .option("--timeout <ms>", "Time to wait for animations", "500")
  .option("--resolved", "Skip CSS custom properties, compare only resolved values")
  .option("--pseudo-states <states>", "Comma-separated pseudo-states to capture (e.g. hover,focus-visible)")
  .action(async (components: string[], options) => {
    try {
      await verifyCommand(components, options);
    } catch (err) {
      console.error(`Error: ${err}`);
      process.exit(2);
    }
  });

program
  .command("isolate")
  .description("Test that multiple component instances don't share JS state")
  .argument("<component>", "Component name")
  .option("--test-base <url>", "Test app base URL", "http://127.0.0.1:5199")
  .option("--format <format>", "Output format: summary or json", "summary")
  .action(async (component: string, options) => {
    try {
      await isolateCommand(component, options);
    } catch (err) {
      console.error(`Error: ${err}`);
      process.exit(2);
    }
  });

program.parse();
