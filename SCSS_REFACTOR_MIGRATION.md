# SCSS REFACTOR MIGRATION GUIDE

## MIGRATION PHILOSOPHY (@CLAUDE.md Compliance)
- **NO BACKWARDS COMPATIBILITY** - Complete migration, no fallbacks
- **NO OBSOLETE CODE** - Delete all duplicates and legacy patterns
- **NO PROTECTIVE CODING** - Remove all redundant safety checks
- **FAIL FAST** - Let errors surface immediately for debugging

## STEP 1: FILES TO DELETE COMPLETELY

### Delete These Files (No Migration Needed)
```
[ ] RR.Blazor/Styles/utilities/_modifiers.scss         # Duplicate of states
[ ] RR.Blazor/Styles/utilities/_universal.scss         # Failed attempt
[ ] RR.Blazor/Styles/utilities/_interaction-states.scss # Moved patterns
[ ] RR.Blazor/Styles/components/_states.scss           # Component duplicates
[ ] RR.Blazor/Styles/abstracts/mixins/_loading.scss    # 198 lines of duplication
[ ] RR.Blazor/Styles/abstracts/_layers.scss            # Invalid @layer syntax
```

## STEP 2: DUPLICATE PATTERNS TO REMOVE

### Loading States (36 duplicates → 1 pattern)
```
[ ] components/_buttons.scss - Lines 54-70, 132-148, 160-175 (button loading states)
[ ] components/_card.scss - Lines 128-145 (card loading shimmer)
[ ] components/_forms.scss - Lines 167-184, 220-237 (form loading states)
[ ] components/_tables.scss - Lines 89-106 (table loading)
[ ] utilities/_states.scss - Lines 12-29 (base loading state)
[ ] abstracts/_extends.scss - Lines 305-307 (icon loading)
```
**Migration**: Use single `.is-loading` class

### Skeleton/Shimmer (60 duplicates → 1 system)
```
[ ] utilities/_animations.scss - Lines 134-141 (shimmer animation)
[ ] utilities/_states.scss - Lines 82-102 (skeleton patterns)
[ ] components/_loading.scss - Lines 119-126 (loading skeleton)
[ ] abstracts/_extends.scss - Lines 400-409 (loading skeleton base)
```
**Migration**: Use single `.skeleton` system with modifiers

### Active States (286 duplicates → 1 pattern)
```
[ ] components/_buttons.scss - Lines 71-83 (button active states)
[ ] components/_card.scss - Lines 45-57 (card active)
[ ] components/_tables.scss - Lines 156-168 (table row active)
[ ] components/_forms.scss - Lines 195-207 (form field active)
[ ] utilities/_states.scss - Lines 30-42 (base active state)
[ ] abstracts/_extends.scss - Lines 1235-1248 (active state base)
```
**Migration**: Use single `.is-active` class

### Glass Effects (146+ duplicates → 4 variants)
```
[ ] utilities/_glass.scss - Lines 1-82 (glass utility classes)
[ ] abstracts/_extends.scss - Lines 309-341 (glass base extends)
[ ] abstracts/mixins/_semantic-variants.scss - Lines 62-129 (glass mixins)
[ ] components/_card.scss - Lines 38-43 (card glass variants)
[ ] components/_buttons.scss - Lines 103-140 (button glass variants)
[ ] components/_avatars.scss - Lines 138-157 (avatar glass variants)
```
**Migration**: Move to `morphisms/glass/` structure

### Gradient Patterns (111 occurrences → centralized)
```
[ ] themes/_default.scss - Lines 7, 21-25, 38, 47, 49, 52, 55
[ ] themes/_dark.scss - Lines 19-25, 36, 45, 47, 50, 53
[ ] utilities/_backgrounds.scss - Lines 20-74 (gradient backgrounds)
[ ] components/_buttons.scss - Lines 104, 116-121, 195-200, 222-254
[ ] components/_avatars.scss - Lines 7, 73, 84, 95, 106, 117, 128, 139-162
```
**Migration**: Use gradient variables and morphisms

## STEP 3: COMPONENT-SPECIFIC CLEANUP

### _buttons.scss
```scss
// DELETE Lines 54-70 (button-loading class)
// DELETE Lines 103-140 (button-gradient variants)
// DELETE Lines 160-175 (button shimmer effects)
// DELETE Lines 195-254 (button enterprise gradients)

// KEEP ONLY:
.button {
  // Base structure only
  display: inline-flex;
  align-items: center;
  gap: var(--space-2);
  padding: var(--space-3) var(--space-4);
  border-radius: var(--radius-md);
  // NO appearance, NO states, NO colors
}
```

### _card.scss
```scss
// DELETE Lines 38-43 (card-gradient)
// DELETE Lines 45-57 (card-active)
// DELETE Lines 128-145 (card-loading)

// KEEP ONLY:
.card {
  // Base structure only
  display: flex;
  flex-direction: column;
  // NO glass effects, NO gradients, NO states
}
```

### _forms.scss
```scss
// DELETE Lines 167-184 (form-loading)
// DELETE Lines 195-207 (form-field-active)
// DELETE Lines 220-237 (form shimmer)

// KEEP ONLY:
.form {
  // Structure only
}
.input {
  // Structure only
}
```

### _tables.scss
```scss
// DELETE Lines 89-106 (table-loading)
// DELETE Lines 156-168 (table-row-active)

// KEEP ONLY:
.table {
  // Structure only
}
```

## STEP 4: MIGRATE EXISTING EXTENDS

### From abstracts/_extends.scss
```
[ ] Lines 309-341: %glass-* → morphisms/glass/_extends.scss
[ ] Lines 43-82: %badge-base → semantic/_badges.scss
[ ] Lines 84-95: %status-indicator-base → semantic/_status.scss
[ ] Lines 98-127: %icon-* → semantic/_icons.scss
[ ] Lines 129-149: %card-* → Keep structure in components/_cards.scss
[ ] Lines 206-263: %button-base → Keep in extends/_base.scss
[ ] Lines 343-375: %form-* → Keep structure in components/_forms.scss
[ ] Lines 376-398: %table-* → Keep structure in components/_tables.scss
[ ] Lines 400-409: %loading-skeleton-base → semantic/_loading.scss
[ ] Lines 1071-1102: %nav-item → components/_navigation.scss
```

## STEP 5: UTILITY CLASS MIGRATIONS

### Class Name Changes for PayrollAI
```scss
// OLD → NEW
.button-loading → .button.is-loading
.button-gradient-primary → .button.bg-gradient-primary
.button-glass → .button.glass-medium
.card-loading → .card.is-loading
.card-glass-primary → .card.glass-primary
.card-gradient → .card.bg-gradient-primary
.form-loading → .form.is-loading
.table-active → .table.is-active

// Gradient migrations
.bg-success → .bg-gradient-success (keep gradient default)
.bg-success-solid → .bg-solid-success (opt-in solid)
.bg-success-light → .bg-subtle-success (subtle variant)

// Glass migrations  
.glass → .glass-medium (be specific)
.glass-card → .card.glass-medium
.glass-button → .button.glass-light

// State migrations
.loading → .is-loading
.active → .is-active
.disabled → .is-disabled
```

## STEP 6: PAYROLLAI COMPONENT UPDATES

### High Priority Files (Most Used Classes)
```
[ ] PayrollAI.Client/Pages/Dashboard.razor
[ ] PayrollAI.Client/Components/Payroll/EmployeeSelectionComponent.razor
[ ] PayrollAI.Client/Components/Company/CompanyPayslipManager.razor
[ ] PayrollAI.Client/Layout/MainLayout.razor
[ ] PayrollAI.Client/Components/Payroll/PayrollUploadComponent.razor
[ ] PayrollAI.Client/Components/Reports/PayrollReportViewer.razor
```

### Common Pattern Updates
```html
<!-- OLD -->
<RCard Variant="CardVariant.Glass" Class="glass-medium card-loading pa-4">

<!-- NEW -->
<RCard Class="card glass-medium is-loading pa-4">

<!-- OLD -->
<div class="bg-warning-light glass-card pa-3">

<!-- NEW -->  
<div class="bg-subtle-warning glass-light pa-3">

<!-- OLD -->
<RButton Variant="ButtonVariant.Primary" Class="button-gradient button-loading">

<!-- NEW -->
<RButton Class="button bg-gradient-primary is-loading">
```

## STEP 7: VALIDATION COMMANDS

### After Each Migration Step
```bash
# Compile SCSS
cd RR.Blazor && npx sass Styles:wwwroot/css --style=compressed

# Check for unused classes
pwsh ./RR.Blazor/Scripts/ValidateClassUsage.ps1

# Find remaining duplicates
pwsh ./RR.Core/Scripts/CheckForDuplicates.ps1

# Validate components
pwsh ./RR.Blazor/Scripts/ValidateComponentUsage.ps1
```

### Final Validation
```bash
# Tree-shake CSS
pwsh ./RR.Blazor/Scripts/TreeShakeOptimize.ps1

# Check CSS size (target: <350KB)
ls -lh RR.Blazor/wwwroot/css/main.css

# Run full test suite
dotnet test
```

## MIGRATION CHECKLIST

### Phase 1: Delete Files (30 min)
- [ ] Delete 6 obsolete files
- [ ] Commit: "refactor: Remove obsolete SCSS files"

### Phase 2: Create Morphism Structure (2 hours)
- [ ] Create morphisms/ folder structure
- [ ] Move glass extends from abstracts/_extends.scss
- [ ] Create _registry.scss for paradigm switching
- [ ] Create utility classes for each morphism
- [ ] Commit: "refactor: Create morphism architecture"

### Phase 3: Consolidate States (1 hour)
- [ ] Create single .is-loading pattern
- [ ] Create single .is-active pattern
- [ ] Create single .is-disabled pattern
- [ ] Create single .skeleton system
- [ ] Remove all duplicates from components
- [ ] Commit: "refactor: Consolidate state patterns"

### Phase 4: Clean Components (2 hours)
- [ ] Strip _buttons.scss to structure only
- [ ] Strip _card.scss to structure only
- [ ] Strip _forms.scss to structure only
- [ ] Strip _tables.scss to structure only
- [ ] Commit: "refactor: Clean component styles"

### Phase 5: Update PayrollAI (3 hours)
- [ ] Update Dashboard.razor
- [ ] Update EmployeeSelectionComponent.razor
- [ ] Update CompanyPayslipManager.razor
- [ ] Update MainLayout.razor
- [ ] Update all other components
- [ ] Commit: "refactor: Migrate PayrollAI to new CSS system"

### Phase 6: Validate & Optimize (1 hour)
- [ ] Run all validation commands
- [ ] Tree-shake unused CSS
- [ ] Verify CSS size < 350KB
- [ ] Run full test suite
- [ ] Commit: "refactor: Optimize and validate CSS"

## SUCCESS CRITERIA

- [ ] CSS size reduced from 785KB to <350KB
- [ ] Zero duplicate patterns
- [ ] All PayrollAI components working
- [ ] No console errors
- [ ] All tests passing
- [ ] No backwards compatibility code

## NO TOLERANCE FOR

- Keeping "just in case" code
- Adding fallback patterns
- Protective styling
- Gradual migration
- Compatibility shims
- Obsolete patterns
- Redundant checks

**This is a COMPLETE migration - no compromises.**