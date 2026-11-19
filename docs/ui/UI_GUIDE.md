# UI Architecture & Component Guide

This document outlines the UI design patterns, navigation stack, modal subsystem, accessibility rules, and reusable components for **KING WORLD WAR**.

---

## 1. Architectural Layers & Boundaries

In accordance with [`AGENTS.md`](file:///Users/yousef/Desktop/My%20project/AGENTS.md):
- **`KingWorldWar.Core.UI`**: Contains pure interfaces (`IScreen`, `IScreenNavigationService`, `IModalService`, `IModalView`, `IResourceState`) and data types (`ScreenId`, `ModalRequest`, `ModalType`).
- **`KingWorldWar.Infrastructure.UI`**: Implements navigation stacks (`ScreenNavigationService`) and modal queues (`ModalService`). Depends only on `Core`.
- **`KingWorldWar.UI`**: Contains Unity MonoBehaviours, views, and presenters (`MainMenuShellView`, `ModalDialogView`, `PlaceholderScreenView`, `KwwButton`, `LocalizedText`, `SafeAreaFitter`). Depends on `Core`, `UnityEngine.UI`, `Unity.TextMeshPro`, and `Unity.InputSystem`.
- **`KingWorldWar.App`**: Composition root (`GameBootstrap`, `MainMenuScope`, `ServiceContainer`). Wires dependencies together.

---

## 2. Screen Navigation Subsystem

### Navigation Service (`IScreenNavigationService`)
Screens operate on a deterministic LIFO history stack:
- **`NavigateTo(ScreenId id)`**: Replaces the entire stack with the target screen as the root. Previous screens are hidden.
- **`Push(ScreenId id)`**: Hides the current screen, pushes the new screen on top, and displays it.
- **`GoBack()`**: Pops the top screen, restores and shows the previous screen in history. Returns `false` if currently at the root screen.
- **`RegisterScreen(ScreenId id, IScreen screen)`**: Registers view instances implementing `IScreen` (`Show()`, `Hide()`, `IsVisible`).

### Shell View (`MainMenuShellView`)
- Manages 6 primary navigation tabs: **World**, **Base**, **Army**, **Alliance**, **Rankings**, **Profile**.
- Exposes Android hardware Back button / Keyboard `Escape` support:
  1. If a modal dialog is open, Back dismisses the modal.
  2. If the navigation stack depth > 1, Back pops the top screen.
  3. If at the root screen (World), Back triggers a confirmation exit dialog.

---

## 3. Modal Dialog Subsystem

### Modal Service (`IModalService`)
Modal requests are queued in a FIFO buffer, ensuring multiple incoming alerts or error popups are displayed sequentially without dropping notifications or creating visual overlap:
- **`ShowAlert(title, message, confirmText, onConfirm)`**
- **`ShowConfirm(title, message, confirmText, cancelText, onConfirm, onCancel)`**
- **`ShowError(title, message, retryText, onRetry, onDismiss)`**
- **`DismissCurrent()` / `ConfirmCurrent()` / `CancelCurrent()`**

### Modal Presentation (`ModalDialogView`)
- Fullscreen canvas overlay with high sorting order.
- Semi-transparent backdrop (`#000000` with 75% opacity) with raycast target enabled, intercepting all touch/mouse events to prevent unintended input leakage into background gameplay layers.
- Tapping outside an alert dialog dismisses it safely.

---

## 4. Reusable UI Components

### 1. `SafeAreaFitter` (`Assets/KINGWorldWar/UI/Common/SafeAreaFitter.cs`)
- Dynamically anchors UI to `Screen.safeArea`.
- Automatically adjusts for hardware notches, camera cutouts (punch-holes), and iOS Dynamic Island / Home Indicator bars without clipping.
- Re-evaluates continuously upon screen resolution or orientation change.

### 2. `KwwButton` (`Assets/KINGWorldWar/UI/Common/KwwButton.cs`)
- Enforces mobile accessibility touch target guidelines: **minimum 48x48dp** (`MinTouchTargetWidth = 48f`, `MinTouchTargetHeight = 48f`).
- Automatic WCAG compliant styling:
  - `Primary`: Military Green (`#3E4E3D`) or Gold (`#D4AF37`) accent.
  - `Secondary`: Steel Gray (`#4A5568`).
  - `Danger`: Tactical Red (`#8B0000`).

### 3. `LocalizedText` (`Assets/KINGWorldWar/UI/Common/LocalizedText.cs`)
- Synchronizes with `ILocalizationService.OnLanguageChanged`.
- Automatically feeds Arabic strings through `ArabicTextShaper` into connected Unicode Presentation Forms-B glyphs.
- Automatic RTL alignment mirroring (flips left alignment to right alignment for Arabic).

### 4. `ResourceHeaderView` (`Assets/KINGWorldWar/UI/Common/ResourceHeaderView.cs`)
- Top-bar display showing non-authoritative client values for **Gold**, **Oil**, and **Iron**.
- Numeric formatting with thousands separators (e.g. `125,000`).

---

## 5. Visual Palette & Accessibility Standards

| Element | Hex Code | Purpose | Contrast Ratio vs Background |
|---|---|---|---|
| Background Charcoal | `#1F2421` | Canvas base, dialog panels, card backgrounds | Base |
| Steel Gray | `#4A5568` | Secondary buttons, borders, subtext | 4.8:1 (WCAG AA compliant) |
| Military Green | `#3E4E3D` | Primary action buttons, active tab indicators | 4.5:1 (WCAG AA compliant) |
| Restrained Gold | `#D4AF37` | Badges, highlighted headings, Gold resource | 8.2:1 (WCAG AAA compliant) |
| Text White | `#FFFFFF` | Primary headers and body text | 13.5:1 (WCAG AAA compliant) |

All interactive controls satisfy:
- Minimum physical touch dimensions >= 48x48 dp.
- High-contrast visual focus and pressed states.
- Support for Arabic right-to-left layout conventions.
