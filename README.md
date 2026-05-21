# MIST

Implements change notification for properties (i.e. `INotifyPropertyChanged`) using IL weaving and a custom MSBuild task.

## Supported target frameworks

The MIST attribute library (`Mathtone.MIST`) is multi-targeted and supports:

* `netstandard2.0`
* `net472` (legacy .NET Framework)
* `net6.0`, `net8.0`, `net9.0`, `net10.0` (.NET / .NET Core)

The build task (`Mathtone.MIST.Builder`) ships flavors for both full-framework MSBuild (`net472`, used by Visual Studio / `msbuild.exe`) and the .NET SDK (`net8.0`, used by `dotnet build`), and the right one is picked automatically at build time.

## Installation

Add the NuGet packages to the project that contains the classes for which you want notifications to be implemented:

```
dotnet add package Mathtone.MIST
dotnet add package Mathtone.MIST.Builder
```

That's it. There is **no longer any `Install.ps1` / `Uninstall.ps1` step and no manual edit of the project file required.** The `Mathtone.MIST.Builder` package now uses the modern NuGet `build/` and `buildTransitive/` props/targets convention to automatically:

1. Register `Mathtone.MIST.NotificationWeaverBuildTask` via `<UsingTask>`.
2. Hook the task into the build (`AfterTargets="Build"`) so the produced assembly is IL-woven on every build.
3. Pick the correct task assembly for the MSBuild runtime in use (full framework vs. .NET SDK).

If you previously used the legacy NuGet 2.x install script, you can safely remove any `<UsingTask>` and `AfterBuild` entries that point at `packages\Mathtone.MIST...\tools\...` from your `.csproj`.

### Optional MSBuild properties

The integration exposes a couple of MSBuild properties you can override in your project:

| Property | Default | Description |
|---|---|---|
| `MathtoneMistEnabled` | `true` | Set to `false` to skip the IL-weaving step for a project. |
| `MathtoneMistDebugMode` | `$(DebugSymbols)` | Controls the `DebugMode` parameter of the build task. |

Example:

```xml
<PropertyGroup>
  <MathtoneMistEnabled>true</MathtoneMistEnabled>
  <MathtoneMistDebugMode>true</MathtoneMistDebugMode>
</PropertyGroup>
```

## Usage

Apply the `Notifier`, `Notify`, `NotifyTarget`, and `SuppressNotify` attributes:

```csharp
public class NotifierBase : INotifyPropertyChanged {

    [NotifyTarget]
    protected void RaisePropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler PropertyChanged;
}

[Notifier(NotificationMode.Implicit)]
public class ImplicitTestNotifier : NotifierBase {

    public string Property1 { get; set; }
    public string Property2 { get; set; }
    public string Property3 { get; set; }

    [SuppressNotify]
    public string Suppressed { get; set; }

    [Notify("Prop1And2", "Property1", "Property2")]
    public string Prop1And2 { get; set; }
}

[Notifier]
public class ExplicitTestNotifier : NotifierBase {

    [Notify] public string Property1 { get; set; }
    [Notify] public string Property2 { get; set; }
    [Notify] public string Property3 { get; set; }

    public string Suppressed { get; set; }

    [Notify("Prop1And2", "Property1", "Property2")]
    public string Prop1And2 { get; set; }
}
```

If everything is wired up correctly you should see a "Lightly Misting: &lt;your project output&gt;" message in the build output at build time.

## OnChange notification

MIST also supports raising notifications only when the value being set actually changes. This is controlled by `NotificationStyle.OnChange`, which can be applied to `NotifierAttribute` as a default or to `NotifyAttribute` as an override. It respects all standard equality conventions (overridden `Equals`, etc.):

```csharp
[Notifier(NotificationMode.Implicit, NotificationStyle.OnChange)]
public class OnChangeTest : TestNotifierBase
{
    public string StringValue { get; set; } = "";

    [Notify(NotificationStyle.OnSet)]
    public string ExplicitOnSetString { get; set; }
}
```

Happy coding!
