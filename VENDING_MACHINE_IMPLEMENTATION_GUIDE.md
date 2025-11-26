# Vending Machine Implementation Guide - Prison Scene

This is a **detailed, step-by-step guide** with **exact values** to implement the vending machine in the Prison scene.

---

## STEP 1: Create MiniGameCanvas

1. **Open the Prison scene** in Unity
2. In Hierarchy, **right-click** → **UI** → **Canvas**
3. **Rename** it to: `MiniGameCanvas`
4. **Select MiniGameCanvas** and configure in Inspector:

### Canvas Component:
- **Render Mode**: `Screen Space - Overlay`
- **Pixel Perfect**: `☐` (unchecked)
- **Sort Order**: `100`
- **Target Display**: `Display 1`
- **Additional Shader Channels**: `TexCoord1, TexCoord2, Normal, Tangent`

### CanvasScaler Component (add if missing):
- **UI Scale Mode**: `Scale With Screen Size`
- **Reference Resolution**: 
  - X: `480`
  - Y: `270`
- **Screen Match Mode**: `Match Width Or Height`
- **Match**: `0.5`
- **Reference Pixels Per Unit**: `100`

### GraphicRaycaster Component (add if missing):
- **Ignore Reversed Graphics**: `☑` (checked)
- **Blocking Objects**: `None`
- **Blocking Mask**: `Everything` (all layers checked)

### ForceConstantPixelCanvas Component:
- Add the script component: `ForceConstantPixelCanvas`
- **Scale Factor**: `1`

### RectTransform:
- **Pos X**: `0`
- **Pos Y**: `0`
- **Pos Z**: `0`
- **Width**: `0`
- **Height**: `0`
- **Scale X**: `0`
- **Scale Y**: `0`
- **Scale Z**: `0`
- **Anchor Min**: `(0, 0)`
- **Anchor Max**: `(0, 0)`
- **Pivot**: `(0, 0)`

---

## STEP 2: Create VendingPopup (Child of MiniGameCanvas)

1. **Right-click MiniGameCanvas** in Hierarchy → **UI** → **Image**
2. **Rename** it to: `VendingPopup`
3. **Select VendingPopup** and configure:

### RectTransform:
- **Anchor Presets**: Hold `Alt+Shift` and click **stretch-stretch** (bottom-right)
- **Pos X**: `0`
- **Pos Y**: `0`
- **Pos Z**: `0`
- **Width**: (auto-set to screen width minus 200)
- **Height**: (auto-set to screen height minus 40)
- **Size Delta**: 
  - X: `-200`
  - Y: `-40`
- **Scale**: `(1, 1, 1)`
- **Pivot**: `(0.5, 0.5)`

### CanvasGroup Component:
- **Alpha**: `1`
- **Interactable**: `☑` (checked)
- **Blocks Raycasts**: `☑` (checked)
- **Ignore Parent Groups**: `☐` (unchecked)

### Image Component:
- **Source Image**: Drag `Assets/PixelArt/Vending/vendingMachine.png` here
- **Color**: `(255, 255, 255, 255)` - White, fully opaque
- **Material**: `None`
- **Raycast Target**: `☑` (checked)
- **Preserve Aspect**: `☑` (checked)
- **Type**: `Simple`
- **Pixels Per Unit Multiplier**: `1`

### Canvas Component (Child Canvas - World Space):
- **Render Mode**: `World Space`
- **Pixel Perfect**: `☐` (unchecked)
- **Sort Order**: `0`
- **Target Display**: `Display 1`
- **⚠️ IMPORTANT**: **Disable this Canvas** (uncheck the checkbox at top of component)

### CanvasScaler Component (for child canvas):
- **UI Scale Mode**: `Constant Pixel Size`
- **Scale Factor**: `1`
- **Reference Pixels Per Unit**: `100`

### GraphicRaycaster Component (for child canvas):
- **Ignore Reversed Graphics**: `☑` (checked)
- **Blocking Objects**: `None`
- **Blocking Mask**: `Everything`

### ⚠️ CRITICAL: Set VendingPopup Initially Inactive
- In Hierarchy, **uncheck the checkbox** next to `VendingPopup` name
- This makes it start hidden until player interacts

---

## STEP 3: Create SidePanelClickArea (Child of VendingPopup)

1. **Right-click VendingPopup** → **UI** → **Image**
2. **Rename** to: `SidePanelClickArea`
3. **Select SidePanelClickArea** and configure:

### RectTransform:
- **Anchor Presets**: Hold `Alt+Shift` and click **stretch-stretch**
- **Pos X**: `85.88919`
- **Pos Y**: `-60.31939`
- **Pos Z**: `0`
- **Size Delta**: 
  - X: `-290.0445`
  - Y: `-259.2672`
- **Scale**: `(1, 1, 1)`
- **Pivot**: `(0.5, 0.5)`

### Image Component:
- **Source Image**: `None` (leave empty)
- **Color**: `(255, 255, 255, 0)` - **Fully transparent** (alpha = 0)
- **Raycast Target**: `☑` (checked) - **MUST be checked for clicks to work**

### VendingSidePanelClick Component:
- Add script: `VendingSidePanelClick`
- **Parent**: Drag the `VendingMachine` GameObject here (we'll create this next)

### Button Component:
- Add `Button` component
- **Interactable**: `☑` (checked)
- **Transition**: `Color Tint`
- **Target Graphic**: Drag the `Image` component here
- **Normal Color**: `(255, 255, 255, 255)`
- **Highlighted Color**: `(245, 245, 245, 255)`
- **Pressed Color**: `(200, 200, 200, 255)`
- **Selected Color**: `(245, 245, 245, 255)`
- **Disabled Color**: `(200, 200, 200, 128)`
- **Color Multiplier**: `1`
- **Fade Duration**: `0.1`
- **OnClick()**: Leave empty (VendingSidePanelClick handles it)

---

## STEP 4: Create PinScreen (Keypad Click Area - Child of VendingPopup)

1. **Right-click VendingPopup** → **UI** → **Image**
2. **Rename** to: `PinScreen`
3. **Select PinScreen** and configure:

### RectTransform:
- **Anchor Presets**: Hold `Alt+Shift` and click **stretch-stretch**
- **Pos X**: `71.0804`
- **Pos Y**: `20.4325`
- **Pos Z**: `0`
- **Size Delta**: 
  - X: `-293.6367`
  - Y: `-256.5558`
- **Scale**: `(1, 1, 1)`
- **Pivot**: `(0.5, 0.5)`

### Image Component:
- **Source Image**: `None` (leave empty)
- **Color**: `(255, 255, 255, 108)` - **Semi-transparent** (alpha = 108/255 ≈ 0.42)
- **Raycast Target**: `☑` (checked)
- **Preserve Aspect**: `☑` (checked)

### VendingKeypadClick Component:
- Add script: `VendingKeypadClick`
- **Parent**: Drag the `VendingMachine` GameObject here

### Button Component:
- Add `Button` component
- **Interactable**: `☑` (checked)
- **Transition**: `Color Tint`
- **Target Graphic**: Drag the `Image` component here
- **Normal Color**: `(255, 255, 255, 255)`
- **Highlighted Color**: `(245, 245, 245, 255)`
- **Pressed Color**: `(200, 200, 200, 255)`
- **Selected Color**: `(245, 245, 245, 255)`
- **Disabled Color**: `(200, 200, 200, 128)`
- **Color Multiplier**: `1`
- **Fade Duration**: `0.1`
- **OnClick()**: Leave empty (VendingKeypadClick handles it)

---

## STEP 5: Create CloseButton (Child of VendingPopup)

1. **Right-click VendingPopup** → **UI** → **Button**
2. **Rename** to: `CloseButton`
3. **Select CloseButton** and configure:

### RectTransform:
- **Anchor Presets**: Click **center-center** (middle)
- **Pos X**: `78.171`
- **Pos Y**: `95.2`
- **Pos Z**: `0`
- **Width**: `24.812`
- **Height**: `24.274`
- **Scale**: `(1, 1, 1)`
- **Pivot**: `(0.5, 0.5)`

### Image Component (on CloseButton):
- **Source Image**: Drag `Assets/PixelArt/Vending/CLOSEBUTTON.png` here
- **Color**: `(255, 255, 255, 255)` - White
- **Raycast Target**: `☑` (checked)
- **Preserve Aspect**: `☐` (unchecked)
- **Type**: `Simple`

### Button Component:
- **Interactable**: `☑` (checked)
- **Transition**: `Color Tint`
- **Target Graphic**: Drag the `Image` component here
- **Normal Color**: `(255, 255, 255, 255)`
- **Highlighted Color**: `(245, 245, 245, 255)`
- **Pressed Color**: `(200, 200, 200, 255)`
- **Selected Color**: `(245, 245, 245, 255)`
- **Disabled Color**: `(200, 200, 200, 128)`
- **Color Multiplier**: `1`
- **Fade Duration**: `0.1`
- **OnClick()**: 
  - Click `+` to add event
  - Drag `VendingMachine` GameObject to object field
  - Select: `VendingPopupInteractable` → `CloseAll()`

---

## STEP 6: Create VendingMachine GameObject (In Scene)

1. In Hierarchy, **right-click** → **Create Empty**
2. **Rename** to: `VendingMachine`
3. **Select VendingMachine** and configure:

### Transform:
- **Position**: Set to where you want the vending machine in your scene (e.g., `(0, 0, 0)`)
- **Rotation**: `(0, 0, 0)`
- **Scale**: `(1, 1, 1)`

### SpriteRenderer Component:
- **Sprite**: Drag `Assets/PixelArt/Vending/vendingMachine.png` here
- **Color**: `(255, 255, 255, 255)` - White
- **Material**: `Default-Sprite` (or leave default)
- **Sorting Layer**: `Default`
- **Order in Layer**: `0`
- **Mask Interaction**: `None`

### BoxCollider2D Component:
- **Is Trigger**: `☑` (checked) - **CRITICAL: Must be checked**
- **Used By Effector**: `☐` (unchecked)
- **Offset**: `(0, 0)`
- **Size**: 
  - X: `1.9214792`
  - Y: `1.74986`
- **Edge Radius**: `0`

### VendingPopupInteractable Component:
- Add script: `VendingPopupInteractable`
- **Mini Game Canvas**: Drag `MiniGameCanvas` from Hierarchy here
- **Vending Popup**: Drag `VendingPopup` from Hierarchy here
- **Keypad Prefab**: Drag `Assets/Prefabs/KeypadUI.prefab` from Project window here
- **Screw Panel Prefab**: Drag `Assets/Prefabs/ScrewPanelClosed.prefab` from Project window here

---

## STEP 7: Link Click Area References

1. **Select SidePanelClickArea** in Hierarchy
2. In `VendingSidePanelClick` component:
   - **Parent**: Drag `VendingMachine` GameObject here

3. **Select PinScreen** in Hierarchy
4. In `VendingKeypadClick` component:
   - **Parent**: Drag `VendingMachine` GameObject here

---

## STEP 8: Verify Everything

### Checklist:
- [ ] MiniGameCanvas exists with all components
- [ ] VendingPopup is child of MiniGameCanvas
- [ ] VendingPopup is **inactive** (unchecked in Hierarchy)
- [ ] SidePanelClickArea is child of VendingPopup
- [ ] PinScreen is child of VendingPopup
- [ ] CloseButton is child of VendingPopup
- [ ] VendingMachine GameObject exists in scene
- [ ] VendingMachine has BoxCollider2D with **Is Trigger = true**
- [ ] VendingPopupInteractable has all 4 references filled:
  - [ ] Mini Game Canvas
  - [ ] Vending Popup
  - [ ] Keypad Prefab
  - [ ] Screw Panel Prefab
- [ ] SidePanelClickArea has Parent reference to VendingMachine
- [ ] PinScreen has Parent reference to VendingMachine
- [ ] CloseButton OnClick calls VendingMachine.CloseAll()

---

## STEP 9: Test

1. **Save the scene** (Ctrl+S)
2. **Press Play**
3. **Walk your character** to the VendingMachine GameObject
4. **Press Interact key** (usually E)
5. **VendingPopup should appear**
6. **Click on PinScreen area** → Keypad should open
7. **Click on SidePanelClickArea** → Screw panel should open
8. **Click CloseButton** → Everything should close

---

## Troubleshooting

### Vending popup doesn't appear when interacting:
- Check VendingMachine has BoxCollider2D with **Is Trigger = true**
- Check VendingPopupInteractable has MiniGameCanvas and VendingPopup assigned
- Check VendingPopup is initially inactive (unchecked)

### Click areas don't work:
- Check Image components have **Raycast Target = true**
- Check Parent references in VendingSidePanelClick and VendingKeypadClick
- Check Button components are Interactable

### Keypad/Screw panel don't open:
- Check prefab references in VendingPopupInteractable
- Check prefabs exist at: `Assets/Prefabs/KeypadUI.prefab` and `Assets/Prefabs/ScrewPanelClosed.prefab`

### UI looks wrong:
- Check Canvas Render Mode is `Screen Space - Overlay`
- Check Canvas Sort Order is `100`
- Check all RectTransform values match the guide exactly

---

## Exact File Paths Reference

- **Vending Machine Sprite**: `Assets/PixelArt/Vending/vendingMachine.png`
- **Close Button Sprite**: `Assets/PixelArt/Vending/CLOSEBUTTON.png`
- **Keypad Prefab**: `Assets/Prefabs/KeypadUI.prefab`
- **Screw Panel Prefab**: `Assets/Prefabs/ScrewPanelClosed.prefab`
- **Scripts Location**: `Assets/Scripts/Vending/`

---

## Summary of GameObject Hierarchy

```
Scene Root
├── MiniGameCanvas (Canvas, CanvasScaler, GraphicRaycaster, ForceConstantPixelCanvas)
│   └── VendingPopup (Image, CanvasGroup, Canvas, CanvasScaler, GraphicRaycaster) [INACTIVE]
│       ├── SidePanelClickArea (Image, VendingSidePanelClick, Button)
│       ├── PinScreen (Image, VendingKeypadClick, Button)
│       └── CloseButton (Image, Button)
└── VendingMachine (SpriteRenderer, BoxCollider2D, VendingPopupInteractable)
```

---

**That's it!** Follow each step exactly and your vending machine should work perfectly in the Prison scene.

