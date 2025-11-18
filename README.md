<div align="center">

# **Stater — Moteur d’états pour Unity**

[![Made with Unity](https://img.shields.io/badge/Made%20with-Unity%206000.0.58f-57b9d3.svg?style=for-the-badge\&logo=unity)](https://unity3d.com)
[![Release](https://img.shields.io/github/v/release/Gamagorat/unity-statemachine?style=for-the-badge\&logo=github)](https://github.com/Gamagorat/unity-statemachine/releases)
</div>

**Stater** est un utilitaire léger permettant de définir des machines d’états basées sur des énumérations (**value-types**).
Il expose une API fluide pour déclarer des états et leurs callbacks associés : **ENTER**, **STEP**, **AT_STEP**, **ON_TRIGGER**, **EXIT**.

Le code source se trouve ici : `Assets/Scripts/States`.

---

## 🚀 Principes clés

* **Générique :**
  `Stater<TStateIdentifier>` où `TStateIdentifier` doit être un `struct` (souvent une `enum`).

* **Timings disponibles :**

  * **ENTER** — exécuté à l’entrée d’un état
  * **STEP** — exécuté à chaque `Step(dt)`
  * **AT_STEP(time)** — exécuté une seule fois après `time` secondes dans l’état
  * **ON_TRIGGER(id)** — exécuté lors d’un appel à `Trigger(id)`
  * **EXIT** — exécuté quand on quitte l’état

* **Durée d’état automatique :**
  `SetDuration(duration, nextState)` → transition automatique après `duration`.

* **Avancement temporel :**
  Appeler chaque frame :

  ```csharp
  stater.Step(Time.deltaTime);
  ```

---

## 📁 Fichiers importants

* **`Stater.cs`** — gestion des états, `Go`, `Step`, `Trigger`
* **`StaterState.cs`** — définition d’un état : actions, durée, transition
* **`StaterAction.cs`** — helpers pour ENTER / STEP / AT_STEP / ON_TRIGGER / EXIT

---

## 🧩 API essentielle

### **Constructeurs**

* `new Stater<T>("DebugName")`
* `new Stater<T>("DebugName", enableDebugLog)`

### **Méthodes**

* `AddState(T id)` → retourne `StaterState<T>`
* `GetState(T id)`
* `Go(T id)` — transition immédiate
* `Trigger(string id)` — exécute les ON_TRIGGER de l’état courant
* `Step(float dt)` — avance l’état

### **Propriétés**

* `CurState` — état courant
* `StateTime` — temps passé dans l’état

---

## 📘 Exemples

### 1) **Exemple minimal**

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

---

### 2) **Trigger et AT_STEP**

```csharp
stater.AddState(MyState.Idle)
    .AddAction(StaterAction.AT_STEP(0.5f, () => Debug.Log("0.5s passed")))
    .AddAction(StaterAction.ON_TRIGGER("DoIt", () => Debug.Log("Triggered")));

stater.Trigger("DoIt");
```

---

### 3) **Exemple réel : `NpcLife.cs` (schéma)**

* Deux machines :
  **locationStater** (LOCATION) et **actionStater** (ACTION)
* Entrée dans `LOCATION.BED` → déclenche `ACTION.WALK` + définition de destination du `NavMeshAgent`
* À l’arrivée : `actionStater` → `ACTION.SLEEP`
* Après une durée : retour à `ACTION.WALK`, puis changement de lieu via `locationStater`
* Important : appeler `locationStater.Step(Time.deltaTime)` (+ l’autre si nécessaire)

---

## ⚠️ Conseils & pièges

* Toujours appeler `Step(dt)` si état utilise STEP/AT_STEP/durée
* `AT_STEP(0f)` s’exécute au **premier Step après Enter**
* `SetDuration(0f)` → transition immédiate → attention aux boucles infinies
* `Trigger` sans handler → warning
* `enableDebugLog` est **static par type générique** → peut affecter d’autres instances
* Non thread-safe (usage Unity main thread)

---

## 🔧 Dépannage

* **Erreur “CurrentState not found”**
  → vérifier que l’état a été ajouté via `AddState`

* **Besoin de logs ?**
  → utiliser `new Stater<T>("name", true)` pour activer toutes les transitions et triggers
