# RR.Blazor Documentation Generation Requirements

## Core Philosophy: Source of Truth Approach

### Fundamental Principle
**SOURCE FILES ARE THE SINGLE SOURCE OF TRUTH**
- SCSS files in `RR.Blazor/Styles/` → utility classes and CSS variables
- R*.razor files in `RR.Blazor/Components/` → component documentation
- **NO MANUAL ADDITIONS** - everything extracted from actual codebase
- **NO HARDCODED PATTERNS** - all data comes from real files
- TOKEN EFFICIENT, SHORT DOCS - AI CAN EXTRAPOLATE FROM PATTERNS

### Token Efficiency Strategy
1. **Extract** real data from source files
2. **Pack** into short, token-efficient format for AI consumption
3. **Instruct** AI on first lines with directive language on navigation and usage

## Overview
Scripts to Generate 2 separate AI-optimized documentation files for RR.Blazor with bracket notation patterns and concise component format.

## File 1: Styles Documentation (`rr-ai-styles.json`)

### Source of Truth: SCSS Files
- **Extract from**: `RR.Blazor/Styles/**/*.scss`
- **What to extract**: All CSS classes (`.class-name`) and CSS variables (`--var-name`)
- **Processing**: Group by category, convert to bracket notation for token efficiency
- **NO MANUAL DATA** - only real selectors from actual files
Example: "justify-":["items-":["start", "end", "center", "stretch"], "self-":["auto", "start", "end", "center", "stretch"]]

### Content Requirements
- **All CSS classes and variables** parsed from actual SCSS files in `RR.Blazor/Styles/`
- **No manual additions** - only real selectors from the codebase
- **Bracket notation format** for AI extrapolation

### AI Instructions Format (First Lines of JSON)
**PURPOSE**: Instruct AI on first lines with directive language on navigation and usage

```json
{
  "_ai_instructions": {
    "CRITICAL": "You must use bracket notation for extrapolation. You are required to understand patterns.",
    "NAVIGATION": {
      "ai_instructions": "Lines 1-X: You must read these AI instructions first",
      "utility_patterns": "Lines Y-Z: You must use these utility classes in bracket notation",
      "css_variables": "Lines A-B: You must use these CSS variables"
    },
    "USAGE_DIRECTIVE": "You must extrapolate from these patterns. You are not allowed to invent classes that don't exist.",
    "BRACKET_NOTATION": {
      "FORMAT": "You must understand that prefix-[value1, value2, value3] means prefix-value1, prefix-value2, prefix-value3",
      "EXAMPLES": [
        "You must know that p-[0, 1, 2, 4, 8] means p-0, p-1, p-2, p-4, p-8",
        "You must know that justify-[start, center, end, between] means justify-start, justify-center, justify-end, justify-between"
      ]
    }
  }
}
```

### Bracket Notation Examples
Classes should be grouped with bracket notation for AI extrapolation:

```json
{
  "utility_patterns": {
    "spacing": [
      "p-[0, 1, 2, 3, 4, 6, 8, 12, 16, 24]",
      "m-[0, 1, 2, 3, 4, 6, 8, 12, 16, 24]",
      "gap-[0, 1, 2, 3, 4, 6, 8, 12, 16, 24]"
    ],
    "layout": [
      "justify-[start, center, end, between, around, evenly]",
      "justify-[items-[start, end, center, stretch], self-[auto, start, end, center, stretch]]",
      "align-[start, center, end, stretch, baseline]",
      "flex-[row, column, wrap, nowrap]",
      "d-[none, block, inline, flex, grid]"
    ],
    "sizing": [
      "w-[0, 1, 2, 4, 8, 12, 16, 24, 32, 48, 64, 96, auto, full, screen]",
      "h-[0, 1, 2, 4, 8, 12, 16, 24, 32, 48, 64, 96, auto, full, screen]",
      "min-w-[0, full, min, max, fit]",
      "max-w-[0, full, min, max, fit, prose, screen-sm, screen-md, screen-lg, screen-xl]"
    ]
  }
}
```

## File 2: Components Documentation (`rr-ai-components.json`)

### Source of Truth: R*.razor Files
- **Extract from**: `RR.Blazor/Components/**/*.razor`
- **What to extract**: Component purpose and essential parameters from actual R* files
- **Processing**: Extract into concise format for token efficiency
- **NO MANUAL DATA** - only real components from actual files

### Content Requirements
- **All R* components** parsed from actual `.razor` files in `RR.Blazor/Components/`
- **No manual additions** - only real components from the codebase
- **Concise format** for AI consumption

### AI Instructions Format (First Lines of JSON)
**PURPOSE**: Instruct AI on first lines with directive language on navigation and usage

```json
{
  "_ai_instructions": {
    "CRITICAL": "You must use this exact component format for UI generation",
    "NAVIGATION": {
      "ai_instructions": "Lines 1-Y: You must read these AI instructions first",
      "component-groupX": "Lines Y-B: Those are components for ...",
      "component-groupY": "Lines B-Z: Those are components for ...",
    },
    "USAGE_DIRECTIVE": "You must use <RComponentName Parameter1='value' Parameter2='value' /> in Blazor markup",
    "COMPONENT_FORMAT": "RComponentName:[Purpose: What this component does, Parameter1: Type - AI hint, Parameter2: Type - AI hint]",
    "ESSENTIAL_ONLY": "You are only shown essential parameters with AI hints. Standard Blazor parameters (Class, Style, etc.) are available but not documented."
  }
}
```

### Component Format Examples
```json
{
  "components": {
    "RButton": {
      "Purpose": "Professional button component with enterprise variants, AI-optimized for rapid development",
      "Parameters": {
        "Variant": "ButtonVariant[Primary, Secondary, Ghost, Danger, Info, Outline, Glass, Success, Warning] - Primary for main actions Secondary for supporting Danger for destructive",
        "Size": "ButtonSize[ExtraSmall, Small, Medium, Large, ExtraLarge] - Medium is default Large for prominent actions Small for compact",
        "Type": "ButtonType[Button, Submit, Reset] - Submit for forms, Button for regular actions",
        "Text": "string - Use action verbs like Save Delete Create Cancel",
        "Icon": "string - Common icons: save, delete, edit, add, search, settings",
        "IconPosition": "IconPosition[Start, End, Top, Bottom] - Start is most common, End for arrows",
        "Loading": "bool - Use during async operations to show processing state",
        "OnClick": "EventCallback<MouseEventArgs>"
      }
    },
    "RCard": {
      "Purpose": "Professional card component for content containers with business-grade variants",
      "Parameters": {
        "Variant": "CardVariant[Default, Outlined, Elevated, Glass, Flat] - Default for standard cards, Elevated for prominence, Glass for modern overlays",
        "Title": "string - Use descriptive titles like Employee Details, Payment Summary, Settings",
        "Subtitle": "string - Use for additional context like dates, categories, descriptions",
        "Content": "string - Use for simple text, or ChildContent for complex layouts",
        "Icon": "string - Common icons: dashboard, person, settings, analytics, payment, work",
        "ImageSrc": "string - Use for hero images, thumbnails, or visual content",
        "Clickable": "bool - Set to true for interactive cards that perform actions",
        "Loading": "bool - Use during async operations to show processing state",
        "Elevation": "int - Use 0 for flat, 2-4 for standard, 8+ for prominent. -1 uses variant default",
        "OnClick": "EventCallback<MouseEventArgs>",
        "ChildContent": "RenderFragment"
      }
    }
  }
}
```

## Technical Implementation Requirements

### SCSS Parsing
1. **Extract all CSS classes** from `RR.Blazor/Styles/*.scss` files using regex
2. **Extract all CSS variables** from `RR.Blazor/Styles/*.scss` files using regex
3. **Group classes by category** (spacing, layout, typography, appearance, etc.)
4. **Convert to bracket notation** for AI extrapolation

### Component Parsing
1. **Extract all R* components** from `RR.Blazor/Components/**/*.razor` files
2. **Parse component purpose** from `@**` blocks or XML comments
3. **Extract essential parameters** with [Parameter] attributes
4. **Include AI hints** from AIParameter attributes
5. **Filter to essential parameters only** (with AI hints, required, or common names)

### Parameter Extraction Rules
- **Include parameters with**: AIParameter hints, Required=true, or names in: Text, Icon, Variant, Size, OnClick, Disabled, Loading, Value, Label, Title, Content, Items, ChildContent
- **Exclude parameters**: Standard Blazor parameters (Class, Style, etc.) unless they have AI hints
- **Format**: `ParameterName: Type - AI hint [suggested values]`

## File Structure Requirements

### Both Files Must Have:
1. **AI instructions at the very top** of the JSON structure
2. **Directive language**: "You must...", "You are required to...", "You are not allowed to..."
3. **Clear section references** with exact line numbers or sections
4. **Extraction info** showing total counts and source directories

### File Naming
- **Styles**: `rr-ai-styles.json`
- **Components**: `rr-ai-components.json`

## Quality Requirements

### Reduce Pollution
- **Essential parameters only** for components
- **No manual additions** beyond AI instructions
- **Concise format** for fast AI processing
- **Real data only** from actual source files

### AI Optimization
- **Bracket notation** for pattern extrapolation
- **Clear instructions** with imperatives
- **Structured format** for easy parsing
- **Examples** showing exact usage patterns

## Implementation Notes

### PowerShell Script Requirements
1. **Extract real classes** from SCSS files using regex patterns
2. **Extract real components** from Razor files using regex patterns
3. **Generate bracket notation** by grouping similar prefixes
4. **Create concise component format** with essential parameters only
5. **Place AI instructions first** in ordered hashtables
6. **Use directive language** in all AI instructions