## What is Partlyx?
Partlyx allows you to create schemes (recipes) of varying complexity, consisting of components, which can be any previously created resources.

![Screenshot1Small.png](.assets/Screenshot_1_small.png)

-----

## What problem does Partlyx solve?

There are many systems in which resources/entities:
* Consist of other resources
* Form deep and branched dependency graphs
* Require visualization and counting of fundamental/basic elements
* Require finding and calculating resource conversion chains

For example: Production chains, crafts in games, economic models.

Performing such tasks using standard calculators and tables is often either impossible or inefficient, and also not very visual. For example, if we need to change the recipe of one of the frequently used elements, we may need to recalculate its components for all elements that use it.

## How does Partlyx solve this problem?

* It allows describing entities as a dependency graph
* Visualizes multi-level structure
* Calculates total cost/weight/price across the entire graph
* Works with abstract types of resources

The application is tailored not to a specific situation, but to a class of tasks

The application features flexibility and is focused on visual representation of complex structures

-----
## Resource Conversion Search

Partlyx can find conversion chains between two selected resources and calculate quantitative ratios for these chains.

Features:
* Search for all possible transformation paths from resource A to resource B and vice versa
* Calculation of conversion coefficients for each path
* Two calculation modes:
  * resulting amount from a given input amount
  * required input amount for a given output amount

If multiple paths exist, all of them are calculated independently, allowing comparison and selection of the most suitable option.
![Screenshot1Small.png](.assets/Screenshot_2_small.png)
## Technical features of the application:

The application is written in C# using Avalonia - a modern cross-platform framework

MVVM structure, divided into 6 layers: Core <- Infrastructure <- Services <- ViewModels <- UI (View), as well as Tests

Applied technologies:
* Dependency Injection for flexibility and modularity
* SQLite for databases
* ReactiveUI for reactivity in View and ViewModels
* MSAGL for graph layout

-----

The app is still being polished, and you can help us find bugs or suggest your ideas.
You can download the available versions of the app here:
https://github.com/Jiofef/Partlyx/releases