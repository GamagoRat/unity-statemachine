# Stater — petit moteur d'états pour Unity

<div align="center">
[![Made with Unity](https://img.shields.io/badge/Made%20with-Unity-Editor-57b9d3.svg?style=for-the-badge&logo=unity)]()
</div>

Stater est un utilitaire léger pour définir des machines d'états basées sur des énumérations (value-type). Il expose une API fluide pour déclarer des états et des callbacks liés aux timings suivants : ENTER / STEP / AT_STEP / ON_TRIGGER / EXIT. Le code source se trouve dans ce dossier : `Assets/Scripts/States`.

Principes clés
- Générique : `Stater<TStateIdentifier>` où `TStateIdentifier` doit être un `struct` (typiquement une enum).
- Timings disponibles :
  - ENTER : exécuté lors de l'entrée d'un état.
  - STEP : exécuté à chaque appel de `Step(dt)`.
  - AT_STEP(time) : exécuté une seule fois lorsque le temps passé dans l'état dépasse `time`.
  - ON_TRIGGER(id) : exécuté quand `Trigger(id)` est appelé sur la machine.
  - EXIT : exécuté lors de la sortie d'un état.
- Durée d'état automatique : `SetDuration(duration, nextState)` permet de basculer automatiquement vers `nextState` après `duration` secondes.
- Avancement temporel : appeler `stater.Step(Time.deltaTime)` depuis `Update()` pour que STEP / AT_STEP et les transitions temporelles fonctionnent.

Fichiers importants
- `Stater.cs`        — classe principale : gestion des états, Go, Step, Trigger.
- `StaterState.cs`   — définition d'un état : actions, durée, transition suivante.
- `StaterAction.cs`  — helpers statiques pour créer les actions (ENTER, STEP, AT_STEP, ON_TRIGGER, EXIT).

API essentielle (résumé)
- Constructeurs :
  - `new Stater<T>("DebugName")`
  - `new Stater<T>("DebugName", enableDebugLog)`
- Méthodes :
  - `AddState(T id)` → retourne `StaterState<T>`
  - `GetState(T id)`
  - `Go(T id)` — transition immédiate vers l'état.
  - `Trigger(string id)` — déclenche actions ON_TRIGGER dans l'état courant.
  - `Step(float dt)` — avance le temps de l'état courant.
- Propriétés :
  - `CurState` — état courant (T).
  - `StateTime` — temps passé dans l'état courant.

Exemples

1) Exemple minimal

```csharp
// Copier dans un MonoBehaviour pour tester
using States;
using UnityEngine;

public enum ExampleState { A, B }

public class Example : MonoBehaviour
{
    private Stater<ExampleState> stater;

    void Start()
    {
        stater = new Stater<ExampleState>("Example", true);

        stater.AddState(ExampleState.A)
            .AddAction(StaterAction.ENTER(() => Debug.Log("Enter A")))
            .AddAction(StaterAction.STEP(() => Debug.Log("Stepping A")))
            .SetDuration(2.0f, ExampleState.B);

        stater.AddState(ExampleState.B)
            .AddAction(StaterAction.ENTER(() => Debug.Log("Enter B")))
            .AddAction(StaterAction.AT_STEP(1.0f, () => Debug.Log("1s in B")))
            .SetDuration(3.0f, ExampleState.A);

        stater.Go(ExampleState.A);
    }

    void Update()
    {
        stater.Step(Time.deltaTime);
    }
}
```

2) Exemple d'usage de Trigger et AT_STEP

```csharp
stater.AddState(MyState.Idle)
    .AddAction(StaterAction.AT_STEP(0.5f, () => Debug.Log("0.5s passed")))
    .AddAction(StaterAction.ON_TRIGGER("DoIt", () => Debug.Log("Triggered")));

stater.Trigger("DoIt");
```

3) Exemple réel tiré de `Assets/example/Scripts/NpcLife.cs` (schéma)
- Deux machines : `locationStater` (LOCATION) et `actionStater` (ACTION).
- Lorsqu'on entre dans `LOCATION.BED` on déclenche `ACTION.WALK` puis on définit la destination du `NavMeshAgent`.
- Quand l'agent arrive, `actionStater` passe à `ACTION.SLEEP` ; après une durée configurée, l'action revient à `ACTION.WALK`, puis la `locationStater` bascule sur un autre lieu, etc.
- Important : appeler `locationStater.Step(Time.deltaTime)` (et `actionStater.Step` si l'un de ses états utilise STEP/AT_STEP/SetDuration).

Conseils et pièges
- Toujours appeler `Step(dt)` chaque frame si des STEP/AT_STEP ou durées sont utilisées.
- `AT_STEP(0.0f)` s'exécutera dès le premier appel à `Step` après `Enter`.
- `SetDuration(0.0f, next)` provoque une transition immédiate lors du prochain check de Step — attention aux boucles instantanées.
- `Trigger` sans handler dans l'état courant loggue un warning.
- Le flag `enableDebugLog` est un static générique dans l'implémentation actuelle — activer le debug sur une instance affectera possiblement d'autres instances du même type générique.
- Conçu pour le thread principal Unity — non thread-safe.

Migration / renommage (optionnel)
- Si tu préfères un nom plus courant : `Stater` → `StateMachine`, `StaterState` → `State`, `StaterAction` → `StateAction`.
- Stratégie sûre :
  1. Copier les fichiers et renommer les classes (nouveaux fichiers).
  2. Mettre à jour les usages progressivement (rechercher/remplacer).
  3. Garder les anciens fichiers pendant la transition, supprimer après validation.

Dépannage
- Erreur "CurrentState not found" lors de `Go(...)` : vérifier que l'état a bien été ajouté via `AddState`.
- Logs : activer le debug via `new Stater<T>("name", true)` pour voir les transitions et triggers dans la console.
