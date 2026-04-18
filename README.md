# Finestrio

## Table of Contents
* [What is Finestrio](#what-is-finestrio)
* [Base Usage](#base-usage)
  * [Creating manager](#creating-manager)
  * [Creating own window (and putting it in resource)](#creating-own-window-and-putting-it-in-resource)
  * [Creating own window (and putting it in addressables)](#creating-own-window-and-putting-it-in-addressables)
  * [Adding new window to manager](#adding-new-window-to-manager)
  * [Changing current window](#changing-current-window)
  * [Moving back to previous window](#moving-back-to-previous-window)
  * [Simple synchronous setup](#simple-synchronous-setup)
* [Setup](#setup)
  * [Example of synchronous and asynchronous setup](#example-of-synchronous-and-asynchronous-setup)
* [Callbacks](#callbacks)
  * [Listening to window lifecycle events](#listening-to-window-lifecycle-events)
* [Error Handling](#error-handling)
  * [Handling transition errors](#handling-transition-errors)
* [Animations](#animations)
  * [What is ITransitionAnimation](#what-is-itransitionanimation)
  * [What is IWindowAnimation](#what-is-iwindowanimation)
  * [How to add default window animation to window](#how-to-add-default-window-animation-to-window)
  * [How to disable animation for transition](#how-to-disable-animation-for-transition)
  * [How to create transition animation based on two custom window animation for transition](#how-to-create-transition-animation-based-on-two-custom-window-animation-for-transition)
  * [How to create custom transition animation](#how-to-create-custom-transition-animation)
  * [How to implement custom IWindowAnimation](#how-to-implement-custom-iwindowanimation)
  * [WindowOrderInAnimation](#windoworderinanimation)
  * [Available animations: SequentioWindowAnimation](#available-animations-sequentiowindowanimation)
* [WindowSource](#windowsource)
  * [How to implement own window source](#how-to-implement-own-window-source)

---

## What is Finestrio
Finestrio is a modern, asynchronous-first Window and UI Management library for Unity. Built on top of `UniTask`, it offers a clean, robust, and highly flexible architecture for orchestrating UI screens (Windows), their lifecycles, and the transitions between them. It supports multiple loading strategies out of the box, including Unity Resources and Addressables, and provides a powerful animation framework for smooth UI transitions.

## Base Usage

### Creating manager
The `WindowManager` is the core of the library. It requires an `IWindowSource` to know how to instantiate and destroy your windows.

```csharp
using Damdor.Finestrio;
using UnityEngine;

public class GameUIInitializer : MonoBehaviour
{
    public WindowManager Manager { get; private set; }

    [SerializeField] private Transform canvasParent;

    private void Awake()
    {
        // Example using both addressable and resources
        var windowSource = new CombinedWindowSource(
            new AddressableWindowSource(canvasParent),
            new ResourcesWindowSource(canvasParent)
        );
        Manager = new WindowManager(windowSource);
    }
}
```

### Creating own window (and putting it in resource)
To create a window, simply inherit from `Window` and decorate it with `[WindowInResources]`.

```csharp
using Damdor.Finestrio;

// The path is relative to any "Resources" folder in your project
[WindowInResources("UI/Windows/MainMenuWindow")]
public class MainMenuWindow : Window
{
    // Your window logic here
}
```
*Note: Make sure your `WindowManager` is initialized with an `ResourcesWindowSource`.*

### Creating own window (and putting it in addressables)
To use Addressables, inherit from `Window` and decorate it with `[WindowInAddressables]`.

```csharp
using Damdor.Finestrio;

// The key must match the Addressable Name/Label of your prefab
[WindowInAddressables("MainMenuWindow_Prefab")]
public class MainMenuWindow : Window
{
}
```
*Note: Make sure your `WindowManager` is initialized with an `AddressableWindowSource`.*

### Adding new window to manager
Using `TransitionType.Add` will instantiate a new window and place it on top of the stack. The previous window is paused but remains alive.

```csharp
var request = TransitionRequest.Of<MainMenuWindow>(TransitionType.Add);
MainMenuWindow window = await manager.Transite(request);
```

### Changing current window
Using `TransitionType.Change` swaps the current top window with a new one. The old window is removed and destroyed.

```csharp
var request = TransitionRequest.Of<SettingsWindow>(TransitionType.Change);
MainMenuWindow window = await manager.Transite(request);
```

### Moving back to previous window
Using `TransitionType.Back` destroys the current top window and resumes the window beneath it on the stack.

```csharp
var request = TransitionRequest.Of<Window>(TransitionType.Back);
Window previousWindow = await manager.Transite(request);
```

If you are sure that window manager will go back to a specific window you can use it:
```csharp
var request = TransitionRequest.Of<SomePreviousWindow>(TransitionType.Back);
SomePreviousWindow previousWindow = await manager.Transite(request);
```

### Simple synchronous setup
You can easily pass models or data into your windows during the transition.

```csharp
var myModel = new PlayerProfileModel { Name = "Hero" };

var request = TransitionRequest.Of<ProfileWindow>(TransitionType.Add)
    .Setup(myModel, (window, model) => window.RefreshProfile(model));

await manager.Transite(request);
```

---

## Setup
Finestrio's setup builder allows you to inject data into your windows either synchronously or asynchronously. This is incredibly useful for fetching data from a server before the window finishes its transition.

### Example of synchronous and asynchronous setup

```csharp
// 1. Fully Synchronous
var request1 = TransitionRequest.Of<GameWindow>(TransitionType.Add)
    .Setup(syncModel, (window, model) => window.Init(model));

// 2. Asynchronous Model Retrieval, Synchronous Window Setup
var request2 = TransitionRequest.Of<GameWindow>(TransitionType.Add)
    .Setup(
        retrieve: async (token) => await FetchDataFromServer(token),
        setup: (window, model) => window.Init(model)
    );

// 3. Synchronous Model, Asynchronous Window Setup
var request3 = TransitionRequest.Of<GameWindow>(TransitionType.Add)
    .Setup(
        model: localData,
        setup: async (window, model, token) => await window.PlayIntroAnimationAndInit(model, token)
    );

// 4. Fully Asynchronous (Async Retrieval & Async Setup)
var request4 = TransitionRequest.Of<GameWindow>(TransitionType.Add)
    .Setup(
        retrieve: async (token) => await FetchDataFromServer(token),
        setup: async (window, model, token) => await window.InitAsync(model, token)
    );
```

---

## Callbacks

### Listening to window lifecycle events
`WindowManager` provides a `Callbacks` property that allows you to subscribe to key window lifecycle events. This is useful for analytics, state management, or other cross-cutting concerns.

```csharp
// Subscribe to an event
manager.Callbacks.TopWindowChanged += (newTopWindow) => 
{
    Debug.Log($"Top window is now: {newTopWindow?.GetType().Name ?? "None"}");
};

manager,Callbacks.WindowCreated += (createdWindow) => 
{
    Debug.Log($"Window created: {createdWindow.GetType().Name}");
};

manager.Callbacks.BeforeWindowDestroyed += (destroyedWindow) => 
{
    Debug.Log($"Window destroyed: {destroyedWindow.GetType().Name}");
};

manager.Callbacks.WindowPaused += (pausedWindow) => 
{
    Debug.Log($"Window paused: {pausedWindow.GetType().Name}");
};

manager.Callbacks.WindowResumed += (resumedWindow) => 
{
    Debug.Log($"Window resumed: {resumedWindow.GetType().Name}");
};
```

Available callbacks:
- `TopWindowChanged`: Invoked when the top-most window in the stack changes.
- `WindowCreated`: Invoked when a new window is created and added to the stack.
- `BeforeWindowDestroyed`: Invoked before a window is destroyed and removed from the stack.
- `WindowPaused`: Invoked when a window is paused (e.g., a new window is pushed on top of it).
- `WindowResumed`: Invoked when a window is resumed (e.g., the window on top of it is removed).

---

## Error Handling

### Handling transition errors
Sometimes window transitions can fail (e.g., failing to load a prefab or an exception thrown during window setup). `WindowManager` handles cleaning up internally, but you may want to know when a transition fails so you can alert the user or log the error.
You can use the `ErrorHandler` delegate for this purpose:

```csharp
// Setup an error handler for the WindowManager
manager.ErrorHandler = async (exception) =>
{
    Debug.LogError($"Window transition failed: {exception.Message}");
    
    // You can optionally show a popup window or perform other async tasks here
    var popupRequest = TransitionRequest.Of<ErrorPopup>(TransitionType.Add)
        .Setup(new ErrorModel { Message = exception.Message }, (w, m) => w.ShowError(m));
        
    await manager.Transite(popupRequest);
};
```
Clean-up will be performed after handler finishes
---

## Animations

### What is ITransitionAnimation
`ITransitionAnimation` represents the overall visual transition happening between the active window stack. It orchestrates the animation sequence, determining how the old window exits and the new window enters.

### What is IWindowAnimation
`IWindowAnimation` represents an animation bound to a *single* window (e.g., sliding in, fading out). Typically, a compound transition combines two `IWindowAnimation`s (one for the exiting window, one for the entering window).

### How to add default window animation to window
In your custom `Window` prefab, you will see a `Default Window Animation` section in the Inspector. You can assign scripts that implement `MonoWindowAnimation` (like `SequentioWindowAnimation`) to the Add, Remove, Pause, and Resume slots. Finestrio will automatically pick these up during transitions.

### How to disable animation for transition
You can override the default animation lookup and force an empty transition using the `Animation` method on your request.

```csharp
var request = TransitionRequest.Of<PopupWindow>(TransitionType.Add)
    .Animation((source, target, type) => null); // Returning null disables animations

await manager.Transite(request);
```

### How to create transition animation based on two custom window animation for transition
If you want to manually combine two specific window animations for a single transition:

```csharp

var request = TransitionRequest.Of<MyWindow>(TransitionType.Change)
    .Animation((source, target, type) => 
        CompoundTransitionAnimation.Combine(source.GetComponentInChildren<MyCustomAnimation>(), target.GetComponentInChildren<MyCustomAnimation>())
    );

await manager.Transite(request);
```
Mind that looking for animation inside window (`source`, `target`) is the most common option, but you could create the animation
on fly.

### How to create custom transition animation
Implement the `ITransitionAnimation` interface to create complex, highly customized screen wipes or crossfades.
This is the most generic version of animation where you have access to both windows at once.

```csharp
public class MyCustomAnimation : ITransitionAnimation
{
    public WindowOrderInAnimation Order => WindowOrderInAnimation.Default;

    private Window source;
    private Window target;
    
    public CrossfadeTransition(Window source, Window target)
    {
        this.source = source;
        this.target = target;
    }
    
    public UniTask Prepare()
    {
        // Setup initial canvas groups, blocks raycasts, etc.
        return UniTask.CompletedTask;
    }

    public async UniTask Play()
    {
        // Animate your crossfade visually here using DOTween, LeanTween, or native coroutines
        await UniTask.Delay(500); 
    }
}

// Usage:
var req = TransitionRequest.Of<MyWindow>(TransitionType.Add)
    .Animation((s, t, type) => new CrossfadeTransition());
```

### How to implement custom IWindowAnimation
Inherit from `IWindowAnimation` or `MonoWindowAnimation` (if you want it configurable in the Inspector).

```csharp
public class ScalePunchAnimation : MonoWindowAnimation
{
    public override WindowOrderInAnimation Order => WindowOrderInAnimation.NewOnTop;

    public override UniTask Prepare()
    {
        transform.localScale = Vector3.zero;
        return UniTask.CompletedTask;
    }

    public override async UniTask Play()
    {
        // Example: Scale to 1 over time
        float t = 0;
        while(t < 1f)
        {
            t += Time.deltaTime * 2f;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            await UniTask.Yield();
        }
        transform.localScale = Vector3.one;
    }
}
```

### WindowOrderInAnimation
This enum dictates how canvases are layered during a transition:
* `Default`: System calculates ordering automatically.
* `NewOnTop`: The incoming window is rendered above the outgoing window.
* `OldOnTop`: The outgoing window remains rendered above the incoming window (useful for "slide out" reveals).

*Note: Normally `Default` means the same as `NewOnTop`

*Note: When using `CompoundTransitionAnimation` calculates order base on order of internal window animations and `Default` means
"take value from the other animation"

### Available animations: SequentioWindowAnimation
If you have the Damdor Sequentio library installed, Finestrio provides `SequentioWindowAnimation` out of the box. Simply add this component to your Window prefab, define your sequence in the Inspector, and assign it to the Window's `Default Animations` slots.

---

## WindowSource

### How to implement own window source
If you load your UI prefabs from an asset bundle, a custom server, or a specialized dependency injection container (like Zenject), you can create a custom `IWindowSource`.

```csharp
using System;
using Cysharp.Threading.Tasks;
using Damdor.Finestrio;
using Object = UnityEngine.Object;

public class MyCustomWindowSource : IWindowSource
{
    public bool Support(Type type)
    {
        // Define your rules. For example, returning true for all Windows:
        return typeof(Window).IsAssignableFrom(type);
    }

    public async UniTask<TWindow> Create<TWindow>() where TWindow : Window
    {
        // 1. Load the prefab your own way
        GameObject prefab = await MyAssetLoader.LoadPrefabAsync(typeof(TWindow).Name);
        
        // 2. Instantiate it
        GameObject instance = Object.Instantiate(prefab);
        
        // 3. Return the window component
        return instance.GetComponent<TWindow>();
    }

    public void Destroy(Window window)
    {
        // Clean up resources if necessary
        MyAssetLoader.Release(window.gameObject);
        Object.Destroy(window.gameObject);
    }
}

// Usage:
var manager = new WindowManager(new MyCustomWindowSource());
```
