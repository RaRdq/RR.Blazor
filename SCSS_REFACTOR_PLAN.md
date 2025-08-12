# SCSS REFACTORING & CLEANUP PLAN

## CURRENT STATE

### Major Issues Identified
1. **36 loading state implementations** (should be 1)
2. **60 skeleton/shimmer patterns** (should be 1 system)
3. **286 active state variations** (should be 1)
4. **146+ glass effect duplicates** (should be 4 variants)
5. **Gradient pollution** in semantic classes
6. **Responsive utility bloat** (2,280+ unused variants)
7. **Progress bar** with 100 hardcoded rules

## ONION ARCHITECTURE FOR SCSS

### Layer 1: Core (Innermost - Zero Dependencies)
```
abstracts/
├── _variables.scss          # CSS custom properties
├── _functions.scss          # Pure SCSS functions
└── _constants.scss          # Static values
```

### Layer 2: Foundation (Depends on Core)
```
abstracts/
├── mixins/
│   ├── _core.scss          # Base mixins (responsive, etc)
│   ├── _φ-system.scss      # Golden ratio calculations
│   └── _typography.scss    # Type system mixins
└── extends/
    ├── _base.scss          # Foundation extends
    └── _layout.scss        # Layout patterns
```

### Layer 3: Design Paradigms (Morphisms)
```
morphisms/                   # Design systems layer
├── _registry.scss          # Paradigm switcher & variables
├── glass/
│   ├── _extends.scss       # %glass-base, %glass-light, etc
│   ├── _mixins.scss        # @mixin glass-effect()
│   └── _utilities.scss     # .glass-* classes
├── material/
│   ├── _extends.scss       # %material-raised, %material-fab
│   ├── _mixins.scss        # @mixin material-shadow()
│   └── _utilities.scss     # .material-* classes
├── neumorphism/
│   ├── _extends.scss       # %neumorph-base, %neumorph-inset
│   ├── _mixins.scss        # @mixin neumorph-shadow()
│   └── _utilities.scss     # .neumorph-* classes
├── claymorphism/
│   ├── _extends.scss       # %clay-base, %clay-heavy
│   ├── _mixins.scss        # @mixin clay-effect()
│   └── _utilities.scss     # .clay-* classes
└── flat/
    ├── _extends.scss       # %flat-base, %flat-contrast
    ├── _mixins.scss        # @mixin flat-border()
    └── _utilities.scss     # .flat-* classes
```

### Layer 4: Semantic Layer (Business Logic)
```
semantic/
├── _colors.scss            # Semantic color system
├── _states.scss            # Universal states (loading, active, disabled)
├── _variants.scss          # Semantic variants (success, warning, etc)
└── _interactions.scss      # Interaction patterns
```

### Layer 5: Components (Uses All Lower Layers)
```
components/
├── _buttons.scss          # Button structure only
├── _cards.scss            # Card structure only
├── _forms.scss            # Form structure only
└── _tables.scss           # Table structure only
```

### Layer 6: Utilities (Outermost - Helper Classes)
```
utilities/
├── _spacing.scss          # Margin, padding utilities
├── _typography.scss       # Text utilities
├── _layout.scss          # Flexbox, grid utilities
└── _helpers.scss         # Misc helper classes
```

## DESIGN PARADIGM SYSTEM

### How Morphisms Work
1. **Paradigm defines visual properties** via CSS variables
2. **Components inherit** paradigm properties by default
3. **Modifiers override** specific properties
4. **Utilities compose** for fine control

### Morphism Registry (`morphisms/_registry.scss`)
```scss
:root {
  // Active paradigm
  --design-paradigm: 'material-gradient';
  
  // Morphism properties (updated by paradigm)
  --morph-blur: 0;
  --morph-opacity: var(--opacity-100);
  --morph-shadow: var(--shadow-md);
  --morph-surface: var(--gradient-primary);
  --morph-border-radius: var(--radius-md);
  --morph-border: var(--border-1) solid transparent;
}
```

### Design Paradigms Supported
- **Material Gradient** (DEFAULT) - Modern enterprise with gradients
- **Glassmorphism** - Translucent with backdrop blur
- **Neumorphism** - Soft shadows and extrusions
- **Claymorphism** - 3D clay-like appearance
- **Flat/Minimal** - Clean, no effects
- **Custom** - User-defined paradigms

## LEGO-BLOCK MODIFIER SYSTEM

### The True Lego Constructor Philosophy
Each class is a **single-purpose building block** that can be combined freely:

```scss
// STRUCTURE (Component Layer)
.button { /* Base structure only */ }
.card { /* Base structure only */ }

// MORPHISM (Design Layer) 
@extend %glass-light;     // From existing _extends.scss
@extend %neumorph-raised; // Reuse existing extends
.clay-heavy { }           // Utility class

// SEMANTIC (Business Layer)
.bg-success { }           // Semantic background
.text-warning { }         // Semantic text
.border-error { }         // Semantic border

// STATE (Interaction Layer)
.is-loading { }           // Loading state
.is-active { }            // Active state
.is-disabled { }          // Disabled state

// MODIFIERS (Utility Layer)
.elevate-md { }           // Elevation
.rounded-lg { }           // Border radius
.p-4 { }                  // Padding
```

### Composition Examples
```html
<!-- Pure Lego Composition -->
<button class="button bg-primary elevate-sm rounded-lg p-4">
  Default Material Button
</button>

<!-- Glass Morphism Card -->
<div class="card glass-medium bg-gradient-primary elevate-lg">
  Premium Glass Card
</div>

<!-- Neumorphic Form -->
<form class="form neumorph-raised p-6 rounded-xl">
  <input class="input neumorph-inset" />
</form>

<!-- State Composition -->
<button class="button bg-success is-loading elevate-md">
  Loading...
</button>

<!-- Mixed Paradigms (Advanced) -->
<div class="card" data-morph="glass">
  <header class="card-header material-raised">
    <h2 class="text-primary">Mixed Design</h2>
  </header>
  <div class="card-body flat-bordered">
    Content with flat borders
  </div>
</div>
```

## INTEGRATION WITH EXISTING CODE

### Reuse Existing Glass Morphism (`abstracts/_extends.scss`)
```scss
// ALREADY EXISTS - Lines 309-341
%glass-base { /* Keep and move to morphisms/glass/_extends.scss */ }
%glass-light { /* Keep and enhance */ }
%glass-medium { /* Keep and enhance */ }
%glass-heavy { /* Keep and enhance */ }
```

### Reuse Existing Mixins (`abstracts/mixins/`)
```scss
// FROM _interactions.scss - Keep and enhance
@mixin interactive-base() { /* Universal interaction pattern */ }
@mixin interactive-hover() { /* Hover states */ }
@mixin interactive-active() { /* Active states */ }
@mixin interactive-disabled() { /* Disabled states */ }
@mixin interactive-loading() { /* Loading states */ }

// FROM _semantic-variants.scss - Refactor into morphisms
@mixin semantic-glass() { /* Move to morphisms/glass/_mixins.scss */ }
@mixin glass-gradient() { /* Move to morphisms/glass/_mixins.scss */ }
```

### Migrate Existing Extends to New Structure
```scss
// FROM _extends.scss → morphisms/[paradigm]/_extends.scss
%badge-base → semantic/_badges.scss
%button-base → Keep in extends/_base.scss (foundation)
%card-base-enhanced → components/_cards.scss (structure only)
%icon-base → semantic/_icons.scss
%nav-item → components/_navigation.scss
```

## PHASE 1: DELETE FILES & DUPLICATES

### Files to DELETE Completely
```
utilities/_modifiers.scss         # Duplicate of states
utilities/_universal.scss         # My failed attempt
utilities/_interaction-states.scss # Moved patterns
components/_states.scss           # Component duplicates
abstracts/mixins/_loading.scss    # 198 lines of duplication
abstracts/_layers.scss            # Invalid @layer syntax
```

### Code to UPDATE in Existing Files

#### 1. Enhance Semantic Backgrounds (`utilities/_backgrounds.scss`)
**Lines 20-48:** Keep gradients as DEFAULT, add solid variants
```scss
// KEEP AS DEFAULT:
&-success { 
  background: var(--gradient-success); // Gradient by default
  color: var(--color-text-on-success);
  
  &.bg-solid { // Opt-in solid variant
    background: var(--color-success);
  }
  
  &.bg-subtle { // Subtle background variant
    background: var(--color-success-bg);
    color: var(--color-success);
  }
}

&-primary {
  background: var(--gradient-primary);
  color: var(--color-text-inverse);
  
  &.bg-solid {
    background: var(--color-primary);
  }
  
  &.bg-subtle {
    background: var(--color-primary-light);
    color: var(--color-primary);
  }
}
```

#### 2. Remove Component Gradient Duplicates
- **`components/_choice.scss`** Lines 695-705: Delete gradient variants
- **`components/_card.scss`** Lines 38-43: Delete gradient variants  
- **`components/_buttons.scss`** Lines 103-140: Delete gradient variants
- **`components/_avatars.scss`** Lines 138-157: Delete gradient variants

#### 3. Remove Duplicate Border Patterns
- **`utilities/_borders.scss`** Lines 200-295: Evaluate if truly needed

## PHASE 2: CREATE MORPHISM SYSTEM

### 2.0 NEW Morphism Structure
**Create proper morphism architecture:**

```
morphisms/
├── _registry.scss              # Central paradigm switcher
├── _index.scss                 # Import orchestrator
├── glass/
│   ├── _extends.scss          # Move existing %glass-* from abstracts/_extends.scss
│   ├── _mixins.scss           # Move glass mixins from _semantic-variants.scss
│   └── _utilities.scss        # .glass-* utility classes
├── material/
│   ├── _extends.scss          # %material-raised, %material-fab
│   ├── _mixins.scss           # @mixin material-elevation()
│   └── _utilities.scss        # .material-* classes
├── neumorphism/
│   ├── _extends.scss          # %neumorph-base, %neumorph-inset
│   ├── _mixins.scss           # @mixin neumorph-shadow()
│   └── _utilities.scss        # .neumorph-* classes
├── claymorphism/
│   ├── _extends.scss          # %clay-base variations
│   ├── _mixins.scss           # @mixin clay-effect()
│   └── _utilities.scss        # .clay-* classes
└── flat/
    ├── _extends.scss          # %flat-base, %flat-contrast
    ├── _mixins.scss           # @mixin flat-style()
    └── _utilities.scss        # .flat-* classes
```

**Create `utilities/_claymorphism.scss`:**
```scss
@use '../abstracts' as *;

// Claymorphism effect utilities using theme variables
.claymorph {
  background: linear-gradient(135deg, 
    color-mix(in srgb, var(--color-surface-elevated) 30%, transparent), 
    color-mix(in srgb, var(--color-surface-elevated) 10%, transparent)
  );
  backdrop-filter: blur(var(--blur-sm));
  border-radius: var(--radius-2xl);
  
  &-light {
    background: color-mix(in srgb, var(--color-surface-elevated) 20%, transparent);
    box-shadow: var(--space-9) var(--space-9) calc(var(--space-17) * 4) 0px rgba(var(--color-primary-rgb), 0.3);
  }
  
  &-medium {
    background: color-mix(in srgb, var(--color-surface-elevated) 30%, transparent);
    box-shadow: var(--space-9) var(--space-9) calc(var(--space-17) * 4) 0px rgba(var(--color-primary-rgb), 0.5);
  }
  
  &-heavy {
    background: color-mix(in srgb, var(--color-surface-elevated) 40%, transparent);
    box-shadow: var(--space-9) var(--space-9) calc(var(--space-17) * 4) 0px rgba(var(--color-primary-rgb), 0.7);
  }
  
  &-primary { 
    background: linear-gradient(135deg,
      rgba(var(--color-primary-rgb), 0.3),
      rgba(var(--color-primary-rgb), 0.1)
    );
    border: var(--border-1) solid rgba(var(--color-primary-rgb), 0.2);
  }
  
  &-success { 
    background: linear-gradient(135deg,
      rgba(var(--color-success-rgb), 0.3),
      rgba(var(--color-success-rgb), 0.1)
    );
    border: var(--border-1) solid rgba(var(--color-success-rgb), 0.2);
  }
  
  &-warning {
    background: linear-gradient(135deg,
      rgba(var(--color-warning-rgb), 0.3),
      rgba(var(--color-warning-rgb), 0.1)
    );
    border: var(--border-1) solid rgba(var(--color-warning-rgb), 0.2);
  }
}
```

**Create `utilities/_neumorphism.scss`:**
```scss
@use '../abstracts' as *;

// Neumorphism effects using theme variables
.neumorph {
  background: var(--color-surface);
  border-radius: var(--radius-xl);
  
  &-raised {
    box-shadow: var(--space-5) var(--space-5) var(--space-15) var(--overlay-light),
                calc(var(--space-5) * -1) calc(var(--space-5) * -1) var(--space-15) var(--color-surface-elevated);
  }
  
  &-inset {
    box-shadow: inset var(--space-2) var(--space-2) var(--space-5) var(--overlay-light),
                inset calc(var(--space-2) * -1) calc(var(--space-2) * -1) var(--space-5) var(--color-surface-elevated);
  }
  
  &-flat {
    box-shadow: var(--space-3) var(--space-3) var(--space-6) var(--overlay-light),
                calc(var(--space-3) * -1) calc(var(--space-3) * -1) var(--space-6) var(--color-surface-elevated);
  }
  
  &-interactive {
    transition: all var(--duration-normal) var(--ease-out);
    
    &:hover {
      box-shadow: var(--space-6) var(--space-6) var(--space-20) var(--overlay-medium),
                  calc(var(--space-6) * -1) calc(var(--space-6) * -1) var(--space-20) var(--color-surface-elevated);
    }
    
    &:active {
      box-shadow: inset var(--space-1) var(--space-1) var(--space-3) var(--overlay-light),
                  inset calc(var(--space-1) * -1) calc(var(--space-1) * -1) var(--space-3) var(--color-surface-elevated);
    }
  }
}
```

**Create `utilities/_gradients.scss`:**
```scss
@use '../abstracts' as *;

// Gradient utilities - DEFAULT for modern design
.gradient {
  &-primary { 
    background: var(--gradient-primary);
    color: var(--color-text-inverse);
  }
  &-success { 
    background: var(--gradient-success);
    color: var(--color-text-on-success);
  }
  &-warning {
    background: var(--gradient-warning);
    color: var(--color-text-on-warning);
  }
  &-error {
    background: var(--gradient-error);
    color: var(--color-text-on-error);
  }
  &-info {
    background: var(--gradient-info);
    color: var(--color-text-on-info);
  }
  &-neutral {
    background: var(--gradient-neutral);
    color: var(--color-text);
  }
  &-subtle {
    background: var(--gradient-subtle);
    color: var(--color-text-muted);
  }
  &-surface {
    background: var(--gradient-surface);
    color: var(--color-text);
  }
  
  // Advanced gradients
  &-radial {
    &-primary { 
      background: radial-gradient(circle, var(--color-primary) 0%, var(--color-primary-hover) 100%);
    }
    &-success {
      background: radial-gradient(circle, var(--color-success) 0%, var(--color-success-dark) 100%);
    }
  }
  
  &-mesh {
    background: 
      radial-gradient(at 40% 20%, var(--color-primary) 0px, transparent 50%),
      radial-gradient(at 80% 0%, var(--color-success) 0px, transparent 50%),
      radial-gradient(at 0% 50%, var(--color-info) 0px, transparent 50%);
  }
  
  &-animated {
    background: linear-gradient(-45deg, 
      var(--color-primary), 
      var(--color-success), 
      var(--color-info), 
      var(--color-primary)
    );
    background-size: 400% 400%;
    animation: gradient-shift var(--duration-long) var(--ease-in-out) infinite;
  }
}

@keyframes gradient-shift {
  0% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
  100% { background-position: 0% 50%; }
}
```

**Create `morphisms/glass/_extends.scss` (Move from abstracts/_extends.scss):**
```scss
@use '../../abstracts' as *;

// MOVE existing glass extends (lines 309-341 from _extends.scss)
%glass-base {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px) saturate(110%);
  -webkit-backdrop-filter: blur(10px) saturate(110%);
  border: 1px solid rgba(255, 255, 255, 0.2);
  box-shadow: 0 8px 32px 0 rgba(31, 38, 135, 0.15);
  
  [data-theme="dark"] & {
    background: rgba(17, 25, 40, 0.75);
    border-color: rgba(255, 255, 255, 0.125);
  }
}

%glass-light {
  @extend %glass-base;
  background: var(--glass-bg-light);
  backdrop-filter: var(--glass-blur-sm) var(--glass-saturate);
  -webkit-backdrop-filter: var(--glass-blur-sm) var(--glass-saturate);
}

%glass-medium {
  @extend %glass-base;
  background: var(--glass-bg-medium);
  backdrop-filter: var(--glass-blur-md) var(--glass-saturate);
  -webkit-backdrop-filter: var(--glass-blur-md) var(--glass-saturate);
}

%glass-heavy {
  @extend %glass-base;
  background: var(--glass-bg-heavy);
  backdrop-filter: var(--glass-blur-lg) var(--glass-saturate-intense);
  -webkit-backdrop-filter: var(--glass-blur-lg) var(--glass-saturate-intense);
}
```

**Create `morphisms/glass/_utilities.scss`:**
```scss
@use '../../abstracts' as *;
@use 'extends' as *;

// Glass utility classes
.glass {
  &-light { @extend %glass-light; }
  &-medium { @extend %glass-medium; }
  &-heavy { @extend %glass-heavy; }
  &-frost { 
    @extend %glass-heavy;
    backdrop-filter: var(--glass-blur-xl) var(--glass-saturate-intense) var(--glass-brightness);
  }
  
  // Colored glass variants
  &-primary {
    background: rgba(var(--color-primary-rgb), var(--opacity-10));
    backdrop-filter: var(--glass-blur-md) var(--glass-saturate);
    border: var(--border-1) solid rgba(var(--color-primary-rgb), var(--opacity-20));
  }
  
  &-success {
    background: rgba(var(--color-success-rgb), var(--opacity-10));
    backdrop-filter: var(--glass-blur-md) var(--glass-saturate);
    border: var(--border-1) solid rgba(var(--color-success-rgb), var(--opacity-20));
  }
}

// Glass gradient combinations
.glass-gradient {
  @extend %glass-base;
  
  &-primary { 
    background: linear-gradient(135deg, 
      rgba(var(--color-primary-rgb), var(--opacity-20)) 0%, 
      rgba(var(--color-primary-rgb), var(--opacity-5)) 100%
    );
  }
}
```

## PHASE 3: CONSOLIDATE TO SINGLE SOURCE

### 2.1 Skeleton System (`components/_skeleton.scss`) - ✅ COMPLETED
**Unified skeleton loading with smart overlay:**
- Extracted from `_states.scss` into dedicated component file
- Implemented smart overlay system for proper content wrapping
- Container-agnostic design following CLAUDE.md principles
- Supports glass effect integration
- Full accessibility and reduced motion support

### 2.2 State System (`utilities/_states.scss`)
**Make this THE ONLY state file:**
```scss
// ONE loading pattern
.is-loading {
  position: relative;
  pointer-events: none;
  cursor: wait;
  
  &::after {
    content: '';
    position: absolute;
    top: 50%;
    left: 50%;
    width: calc(var(--base-unit) * 1.2);
    height: calc(var(--base-unit) * 1.2);
    margin: calc(var(--base-unit) * -0.6) 0 0 calc(var(--base-unit) * -0.6);
    border: var(--border-2) solid currentColor;
    border-color: currentColor transparent;
    border-radius: var(--radius-full);
    animation: spin var(--duration-slow) linear infinite;
  }
}

// ONE active pattern
.is-active {
  color: var(--color-primary);
  background: var(--color-primary-light);
  border-color: var(--color-primary);
  box-shadow: 0 0 0 var(--space-px) rgba(var(--color-primary-rgb), var(--opacity-25));
}

// ONE disabled pattern
.is-disabled {
  opacity: var(--opacity-disabled);
  cursor: not-allowed;
  pointer-events: none;
}

// ONE skeleton pattern
.skeleton {
  background: linear-gradient(90deg,
    var(--color-surface-elevated) 0%,
    var(--color-surface) 50%,
    var(--color-surface-elevated) 100%
  );
  background-size: 200% 100%;
  animation: shimmer var(--duration-very-slow) var(--ease-in-out) infinite;
  border-radius: var(--radius-md);
  
  &-text { 
    height: var(--base-unit);
    margin-bottom: var(--space-2);
  }
  &-title { 
    height: calc(var(--base-unit) * 1.5);
    margin-bottom: var(--space-3);
  }
  &-avatar { 
    width: calc(var(--base-unit) * 3);
    height: calc(var(--base-unit) * 3);
    border-radius: var(--radius-full);
  }
  &-card { 
    height: calc(var(--base-unit) * 10);
    padding: var(--space-4);
  }
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@keyframes shimmer {
  0% { background-position: -200% 0; }
  100% { background-position: 200% 0; }
}
```

**Create `utilities/_material.scss`:**
```scss
@use '../abstracts' as *;

// Material Design effects using theme variables
.material {
  // Standard elevation
  &-raised {
    box-shadow: var(--shadow-md);
    border-radius: var(--radius-md);
    transition: box-shadow var(--duration-fast) var(--ease-out),
                transform var(--duration-fast) var(--ease-out);
    
    &:hover {
      box-shadow: var(--shadow-lg);
      transform: translateY(calc(var(--space-px) * -2));
    }
  }
  
  // Material buttons with ripple
  &-button {
    position: relative;
    overflow: hidden;
    
    &::after {
      content: '';
      position: absolute;
      top: 50%;
      left: 50%;
      width: 0;
      height: 0;
      border-radius: var(--radius-full);
      background: color-mix(in srgb, var(--color-text-inverse) var(--opacity-50), transparent);
      transform: translate(-50%, -50%);
      transition: width var(--duration-normal) var(--ease-out), 
                  height var(--duration-normal) var(--ease-out);
    }
    
    &:active::after {
      width: calc(var(--space-50) * 4);
      height: calc(var(--space-50) * 4);
    }
  }
  
  // FAB (Floating Action Button)
  &-fab {
    border-radius: var(--radius-full);
    box-shadow: var(--shadow-lg);
    padding: var(--space-4);
    transition: box-shadow var(--duration-fast) var(--ease-out);
    
    &:hover {
      box-shadow: var(--shadow-xl);
    }
  }
}
```

**Create `utilities/_flat.scss`:**
```scss
@use '../abstracts' as *;

// Flat/Minimal design effects using theme variables
.flat {
  box-shadow: none !important;
  background: var(--color-surface-elevated);
  border-radius: var(--radius-sm);
  
  &-bordered {
    border: var(--border-1) solid var(--color-border);
    
    &-primary {
      border-color: var(--color-primary);
    }
    
    &-subtle {
      border-color: var(--color-border-light);
    }
  }
  
  &-borderless {
    border: none;
  }
  
  // Minimal interactions
  &-interactive {
    transition: background-color var(--duration-fast) var(--ease-out),
                color var(--duration-fast) var(--ease-out);
    
    &:hover {
      background: var(--state-hover-bg);
    }
    
    &:active {
      background: var(--state-active-bg);
    }
    
    &:focus-visible {
      outline: var(--border-2) solid var(--color-focus);
      outline-offset: calc(var(--space-px) * 2);
    }
  }
  
  // High contrast variant
  &-contrast {
    background: var(--color-text);
    color: var(--color-text-inverse);
    transition: all var(--duration-fast) var(--ease-out);
    
    &:hover {
      background: var(--color-text-muted);
    }
    
    &:active {
      background: var(--color-text-subtle);
    }
  }
}
```

**Create `utilities/_morphisms.scss` (Registry):**
```scss
@use '../abstracts' as *;

// Morphism registry and global mixins using theme variables
:root {
  // Design paradigm switching
  --design-paradigm: 'material-gradient'; // Default
  
  // Morphism properties (updated by paradigm)
  --morph-blur: 0;
  --morph-opacity: var(--opacity-100);
  --morph-shadow: var(--shadow-md);
  --morph-surface: var(--gradient-primary);
  --morph-border-radius: var(--radius-md);
  --morph-border: var(--border-1) solid transparent;
}

// Global paradigm classes
[data-design="material-gradient"] {
  --morph-blur: 0;
  --morph-shadow: var(--shadow-md);
  --morph-surface: var(--gradient-primary);
  --morph-border: none;
}

[data-design="glassmorphism"] {
  --morph-blur: var(--blur-lg);
  --morph-opacity: var(--opacity-20);
  --morph-shadow: 0 var(--space-2) var(--space-8) 0 rgba(var(--color-primary-rgb), 0.37);
  --morph-surface: color-mix(in srgb, var(--color-surface-elevated) var(--morph-opacity), transparent);
  --morph-border: var(--border-1) solid var(--glass-border-medium);
}

[data-design="neumorphism"] {
  --morph-blur: 0;
  --morph-shadow: var(--space-5) var(--space-5) var(--space-15) var(--overlay-light),
                  calc(var(--space-5) * -1) calc(var(--space-5) * -1) var(--space-15) var(--color-surface-elevated);
  --morph-surface: var(--color-surface);
  --morph-border-radius: var(--radius-xl);
}

[data-design="claymorphism"] {
  --morph-blur: var(--blur-sm);
  --morph-opacity: var(--opacity-60);
  --morph-shadow: var(--space-9) var(--space-9) calc(var(--space-17) * 4) 0px rgba(var(--color-primary-rgb), 0.5);
  --morph-surface: linear-gradient(135deg, 
    color-mix(in srgb, var(--color-surface-elevated) 30%, transparent),
    color-mix(in srgb, var(--color-surface-elevated) 10%, transparent)
  );
  --morph-border-radius: var(--radius-2xl);
}

[data-design="flat"] {
  --morph-blur: 0;
  --morph-shadow: none;
  --morph-surface: var(--color-surface-elevated);
  --morph-border-radius: var(--radius-sm);
  --morph-border: var(--border-1) solid var(--color-border);

// Component morphism overrides
[data-button-morph="glass"] .button {
  @extend .glass-medium;
}

[data-card-morph="neumorph"] .card {
  @extend .neumorph-raised;
}

[data-modal-morph="clay"] .modal {
  @extend .claymorph-medium;
}
```

### 2.3 Elevation System (`utilities/_elevation.scss`)
```scss
// Single elevation system
.elevate {
  &-none { box-shadow: none !important; }
  &-xs { box-shadow: var(--shadow-xs) !important; }
  &-sm { box-shadow: var(--shadow-sm) !important; }
  &-md { box-shadow: var(--shadow-md) !important; }
  &-lg { box-shadow: var(--shadow-lg) !important; }
  &-xl { box-shadow: var(--shadow-xl) !important; }
}
```

## PHASE 3: FIX COMPONENT-SPECIFIC BLOAT

### Update ALL Components to Use Universal Modifiers

#### _buttons.scss
```scss
// REMOVE:
.button-loading { /* delete */ }
.button-ghost-* { /* delete */ }
.button-outlined-* { /* delete */ }
.button-gradient { /* delete */ }

// Component should only define:
.button {
  // Structure and layout ONLY
  // NO state/effect styles
}
```

#### _card.scss
```scss
// REMOVE:
.card-loading { /* delete */ }
.card-ghost { /* delete */ }
.card-outlined { /* delete */ }
.card-gradient { /* delete */ }
.card-glass-* { /* delete */ }

// Keep only:
.card {
  // Structure ONLY
}
```

#### _tables.scss, _forms.scss, _modals.scss
Same pattern - remove ALL state/effect duplicates

## PHASE 4: OPTIMIZE CRITICAL ISSUES

### 4.1 Fix Progress Bar Component
**Current:** 100 individual CSS rules
```scss
// DELETE all [data-progress="X"] rules
// USE:
.upload-progress-bar-fill {
  width: calc(var(--progress) * 1%);
}
```

### 4.2 Reduce Responsive Utilities
```scss
// Only generate responsive for ACTUALLY USED utilities:
$responsive-utilities: (
  'display': (flex, none, block),
  'justify': (center, between),
  'align': (center),
  'padding': (0, 2, 4),
  'margin': (0, 2, 4)
);
// DELETE all others
```

### 4.3 Consolidate Flexbox
```scss
// Create standard utilities:
.flex-center {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-2);
}

.flex-between {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-2);
}

.flex-column {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
}

.flex-wrap {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-2);
}

// Use these instead of repeating patterns
```

## PHASE 5: COMPONENT MIGRATION

### For Each Component:
1. Identify current pattern
2. Map to universal modifier
3. Update Razor files
4. Remove old SCSS

### Example Migrations:
```html
<!-- OLD -->
<div class="card card-loading card-ghost card-gradient-warning">
<button class="button button-outlined-primary button-loading">

<!-- NEW -->
<div class="card is-loading bg-ghost-warning glass-gradient-warning">
<button class="button border-outline-primary is-loading">
```

## PHASE 6: VALIDATION & TESTING

### Build & Measure
```bash
# Compile SCSS
cd RR.Blazor && npx sass Styles:wwwroot/css --style=compressed

# Measure CSS size
ls -lh wwwroot/css/main.css

# Target: <250KB
```

### Visual Testing
1. Check all components render correctly
2. Test modifier combinations
3. Verify no visual regressions

## HOW TO USE THE NEW SYSTEM

### 1. Import Order (main.scss)
```scss
// Layer 1: Core
@use 'abstracts/variables';
@use 'abstracts/functions';

// Layer 2: Foundation
@use 'abstracts/mixins';
@use 'abstracts/extends';

// Layer 3: Morphisms
@use 'morphisms'; // Includes all design paradigms

// Layer 4: Semantic
@use 'semantic';

// Layer 5: Components
@use 'components';

// Layer 6: Utilities
@use 'utilities';
```

### 2. Component Development Pattern
```scss
// components/_cards.scss
.card {
  // STRUCTURE ONLY
  display: flex;
  flex-direction: column;
  padding: var(--space-4);
  
  // NO appearance styles
  // NO state styles
  // NO semantic colors
}

// Usage in HTML
<div class="card glass-medium bg-gradient-primary elevate-lg">
  <!-- Card inherits glass morphism, gradient bg, and elevation -->
</div>
```

### 3. Creating Custom Morphisms
```scss
// morphisms/custom/_extends.scss
%custom-effect {
  // Your custom morphism
  background: /* custom */;
  box-shadow: /* custom */;
}

// morphisms/custom/_utilities.scss
.custom-effect {
  @extend %custom-effect;
}
```

### 4. Switching Design Paradigms
```html
<!-- Global paradigm -->
<body data-design="glassmorphism">
  <!-- All components inherit glass morphism -->
</body>

<!-- Component-level override -->
<div class="card" data-morph="neumorphism">
  <!-- This card uses neumorphism -->
</div>
```

## MIGRATION STRATEGY

### Step 1: Audit Current Usage
```bash
# Find all glass effect usage
rg "glass-" --type scss

# Find all gradient usage  
rg "gradient" --type scss

# Find all state variations
rg "is-active|is-loading|is-disabled" --type scss
```

### Step 2: Create Migration Map
```scss
// OLD → NEW
.card-glass-primary → .card glass-primary
.button-gradient-success → .button bg-gradient-success
.form-loading → .form is-loading
.table-active → .table is-active
```

### Step 3: Update Components
```html
<!-- OLD -->
<div class="card-glass-primary card-elevated card-loading">

<!-- NEW -->
<div class="card glass-primary elevate-md is-loading">
```

### Step 4: Remove Old Code
- Delete component-specific morphism styles
- Delete duplicate state implementations
- Delete unused responsive utilities

## NO BACKWARDS COMPATIBILITY

- **DELETE** all legacy patterns
- **NO** deprecated classes
- **NO** compatibility shims
- **FORCE** migration to new system
- Clean, minimal, maintainable

## KEY BENEFITS OF ONION ARCHITECTURE

### 1. Clear Dependency Direction
```
Core → Foundation → Morphisms → Semantic → Components → Utilities
```
- Inner layers don't know about outer layers
- Changes propagate outward, not inward
- Easy to test and maintain

### 2. True Reusability
```scss
// Extends can be reused everywhere
%glass-base → Used by mixins, utilities, and components
%button-base → Foundation for all button variants
%interactive-base → Universal interaction pattern
```

### 3. Morphism Flexibility
```html
<!-- Mix and match paradigms -->
<div class="card glass-medium">
  <button class="button material-raised">
    <span class="badge neumorph-inset">3</span>
  </button>
</div>
```

### 4. Semantic Consistency
```scss
// Same semantic colors everywhere
.bg-success    // Background
.text-success  // Text
.border-success // Border
.glass-success // Glass morphism
```

### 5. Zero Conflicts
- Each class has ONE responsibility
- No overlapping concerns
- Predictable composition
- No specificity wars
